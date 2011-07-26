using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave
{
    public enum NodeTypes
    {
        Custom = 0,
        Air,
        Soil,
        Earth,
        Water,
        Fire
    }

    public class NodeType
    {
        public NodeTypes Type;
        public Vector3 Color = Vector3.One;
        public virtual Vector3 Texture
        {
            get;
            set;
        }

        public NodeType OldNodeType;

        public bool CanCollide = true;
        public bool CanRender = true;

        public float Opacity;
        public float ReflectionAngle;

        public List<IDecal> Decals = new List<IDecal>();

        public delegate void Collision(Node node, Entity player, Vector3 speed);
        public delegate Vector3 LightColor(Node node);
        public Collision ResolveCollision;
        public LightColor GetDiffuseColor;
        public LightColor GetAmbientColor;

        public NodeType AddDecals(params IDecal[] decals)
        {
            Decals.AddRange(decals);
            return this;
        }

        public void Init(Node node)
        {
            foreach (var dec in Decals)
                dec.Init(node);
        }

        public Dictionary<string, Vector3> Textures = new Dictionary<string,Vector3>();
        public virtual void SetTexture(string name)
        {
            Texture = Textures[name];
        }
    }

    public class AnimatedNode : NodeType
    {
        public override Vector3 Texture
        {
            get
            {
                //ReflectionAngle = Animation.Active.Texture.X / 2;
                return Animation.Active.Texture;
            }
            set
            {
            }
        }
        
        public AnimationSet Animation;

        public override void SetTexture(string name)
        {
            if (Animation.Frames.ContainsKey(name)) Animation.SetActive(name);
            else if (name[3] == '1')
                Animation.SetActive("1");
            else if (name[1] == '1')
                Animation.SetActive("0");
            else Animation.SetActive("0000");
        }
    }

    public static class NodeFactory
    {
        private static AnimationSet waterAnims = new ActiveAnimationSet
                        {
                            Frames = { 
                                    { "1", new Animation { Position = new Vector3(1, 1, 0), Texture = new Vector3(1, 1, 0), Count = 8, Delay = 10} },
                                    { "0", new Animation { Position = new Vector3(1, 3, 0), Texture = new Vector3(1, 3, 0), Count = 8} },
                                    { "0000", new Animation { Position = new Vector3(1, 2, 0), Texture = new Vector3(1, 2, 0), Count = 1} },
                                    { "1111", new Animation {Position = new Vector3 (1, 4, 0), Texture = new Vector3(1, 4, 0), Count = 1 } }
                                },
                        };
        
        public static NodeType Get(NodeTypes type)
        {
            NodeType o = null;

            switch (type)
            {
                case NodeTypes.Earth:
                    o = new NodeType
                    {
                        Color = new Vector3(.6f, .4f, 0.3f),
                        Texture = new Vector3(0, 0, 0),
                        ResolveCollision = HardCollision,
                        Opacity = 1.0f,
                        GetDiffuseColor = DiffuseAmbientLight,
                        GetAmbientColor = (Node node) => { return node.Ambience * node.Type.Color / 2; },
                        Textures = { 
                            {"0000", new Vector3(0, 0, 0)},
                            {"1000", new Vector3(0, 1, 0)},
                            {"0100", new Vector3(0, 2, 0)},
                            {"1100", new Vector3(0, 3, 0)},
                            {"0010", new Vector3(0, 4, 0)},
                            {"1010", new Vector3(0, 5, 0)},
                            {"0110", new Vector3(0, 6, 0)},
                            {"1110", new Vector3(0, 7, 0)},
                            {"0001", new Vector3(0, 8, 0)},
                            {"1001", new Vector3(0, 9, 0)},
                            {"0101", new Vector3(0, 10, 0)},
                            {"1101", new Vector3(0, 11, 0)},
                            {"0011", new Vector3(0, 12, 0)},
                            {"1011", new Vector3(0, 13, 0)},
                            {"0111", new Vector3(0, 14, 0)},
                            {"1111", new Vector3(0, 15, 0)},
                        }
                    };
                    break;
                case NodeTypes.Soil:
                    o = new NodeType
                    {
                        Color = new Vector3(.6f, .4f, 0.3f),
                        Texture = new Vector3(0, 0, 0),
                        ResolveCollision = HardCollision,
                        Opacity = 1.0f,
                        GetDiffuseColor = DiffuseAmbientLight,
                        GetAmbientColor = (Node node) => { return node.Ambience * node.Type.Color / 2; },
                    }.AddDecals(DecalFactory.Get(DecalType.Grass));
                    break;
                case NodeTypes.Air:
                    o = new NodeType
                    {
                        Color = Vector3.One,
                        Texture = new Vector3(14, 0, 0),
                        CanCollide = false,
                        CanRender = false,
                        GetDiffuseColor = DiffuseLight,
                        GetAmbientColor = (Node node) => { return node.Ambience; },
                    };
                    break;
                case NodeTypes.Water:
                    o = new AnimatedNode
                    {
                        Animation = new PassiveAnimationSet
                        {
                            Frames = waterAnims.Frames,
                            ActiveAnimation = "0"
                        },
                        CanCollide = false,
                        CanRender = true,
                        //ReflectionAngle = 0.1f,
                        Color = new Vector3(0.5f, 0.5f, 1),
                        Opacity = 0.1f,
                        ResolveCollision = Slowdown,
                        GetDiffuseColor = DiffuseLight,
                        GetAmbientColor = (Node node) => { return node.Ambience * node.Type.Color; },
                    };
                    break;
                case NodeTypes.Fire:
                    o = new NodeType
                    {
                        Color = new Vector3(1.0f, .2f, 0),
                        Texture = new Vector3(15, 0, 0),
                        ResolveCollision = HardCollision,
                        Opacity = 1.0f,
                        GetDiffuseColor = (Node node) => { return node.Type.Color; },
                        GetAmbientColor = (Node node) => { return node.Type.Color; }
                    };
                    break;
                default:
                    o = new NodeType();
                    break;
            }
            o.Type = type;
            return o;
        }

        public static NodeType Get(NodeTypes type, bool renderable)
        {
            var o = NodeFactory.Get(type);
            o.CanRender = renderable;
            return o;
        }
        public static NodeType Get(NodeTypes type, Vector3 texture)
        {
            var o = NodeFactory.Get(type);
            o.CanRender = true;
            o.Texture = texture;
            return o;
        }

        public static void HardCollision(Node node, Entity player, Vector3 speed)
        {
            var delta = (player.Postion + speed - node.Postion);
            delta = new Vector3(Math.Abs(delta.X) > Math.Abs(delta.Y) ? Math.Sign(delta.X) : 0, Math.Abs(delta.X) > Math.Abs(delta.Y) ? 0 : Math.Sign(delta.Y), 0);
            var nV = Vector3.Dot(delta, speed);

            player.Speed = speed - MathHelper.Min(nV, 0) * delta;
        }

        public static void Slowdown(Node node, Entity player, Vector3 speed)
        {
            player.Speed = speed / 2;
        }


        public static Vector3 DiffuseAmbientLight (Node node)
        {
            return (node.Diffuse * node.Type.Color + node.Ambience);
        }

        public static Vector3 DiffuseLight (Node node)
        {
            return node.Diffuse;
        }
    }

    public class Node : Instanced
    {
        public NodeType Type;
        public double Value;

        public Vector3 Postion;
        public int Updated;

        public Vector3 Diffuse;
        public Vector3 Ambience;
        public LightType LType;

        protected InstanceData Instance;
        public Vector3 Size = Vector3.One;
        public BoundingBox CollisionBox;

        public delegate void NodeEventHandler(Node node);

        public event NodeEventHandler TypeChanged;

        public Node()
        {
            Instance = new InstanceData();
        }

        public void SetType(NodeType newType)
        {
            if (Type!= null && Type.Type == newType.Type)
                return;

            newType.OldNodeType = Type;
            Type = newType;

            Type.Init(this);
            if (TypeChanged != null)
                TypeChanged(this);
        }

        public void SetPosition(Vector3 pos)
        {
            Postion = pos;
            Instance.World = Matrix.CreateTranslation(pos);
            var minV = new Vector3(pos.X - Size.X / 2, pos.Y - Size.Y / 2, 0);
            var maxV = new Vector3(pos.X + Size.X / 2, pos.Y + Size.Y / 2, 0);

            CollisionBox = new BoundingBox(minV, maxV);
        }

        public void GetInstanceData(Instancer instancer)
        {
            Instance.Color = Type.Color;
            Instance.Light = Diffuse + Ambience;
            Instance.Texture = Type.Texture;
            instancer.AddInstance(Instance);

            foreach (IDecal decal in Type.Decals)
            {
                decal.GetInstanceData(instancer);
            }
        }

        public void ResolveCollision(Entity player, Vector3 speed)
        {
            Type.ResolveCollision(this, player, speed);
        }

        public Vector3 GetLightColor()
        {
            return Type.GetDiffuseColor(this);
        }

        public Vector3 GetAmbientColor()
        {
            return Type.GetAmbientColor(this);
        }
    }
}
