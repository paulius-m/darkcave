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
        EarthBack,
        Water,
        Lava,
        Cloud,
        Brick,
        BrickBack
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
        public Vector3 Emission;

        public List<IDecal> Decals = new List<IDecal>();

        public delegate void Collision(Node node, Entity player, Vector3 speed);
        public delegate Vector3 LightColor(Node node);
        public Collision ResolveCollision;

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

        public void GetInstanceData(InstanceData data, RenderGroup RenderGroup)
        {
            if (this.Opacity != 0 && OldNodeType != null && OldNodeType.CanRender)
            {
                data.Texture = OldNodeType.Texture;
                data.Color = new Color(OldNodeType.Color);
                data.Light.W = OldNodeType.Opacity;
                RenderGroup.AddInstance(data);
            }
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

        private static AnimationSet lavaAnims = new ActiveAnimationSet
        {
            Frames = { 
                        { "0", new Animation { Position = new Vector3(1, 9, 0), Texture = new Vector3(1, 9, 0), Count = 8, Delay = 10} },
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
                        Color = new Vector3(.6f, .5f, 0.4f),
                        Texture = new Vector3(0, 0, 0),
                        ResolveCollision = HardCollision,
                        Opacity = 1.0f,
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
                        Color = new Vector3(.6f, .55f, 0.55f),
                        Texture = new Vector3(13, 0, 0),
                        ResolveCollision = HardCollision,
                        Opacity = 1.0f,
                        Textures = { 
                            {"0000", new Vector3(2, 5, 0)},
                            {"1000", new Vector3(2, 5, 0)},
                            {"0100", new Vector3(2, 5, 0)},
                            {"1100", new Vector3(1, 6, 0)},
                            {"0010", new Vector3(2, 5, 0)},
                            {"1010", new Vector3(2, 5, 0)},
                            {"0110", new Vector3(2, 5, 0)},
                            {"1110", new Vector3(2, 5, 0)},
                            {"0001", new Vector3(2, 5, 0)},
                            {"1001", new Vector3(1, 5, 0)},
                            {"0101", new Vector3(2, 5, 0)},
                            {"1101", new Vector3(2, 5, 0)},
                            {"0011", new Vector3(3, 5, 0)},
                            {"1011", new Vector3(2, 5, 0)},
                            {"0111", new Vector3(2, 5, 0)},
                            {"1111", new Vector3(2, 5, 0)},
                        }

                    }.AddDecals(DecalFactory.Get(DecalType.Grass));
                    break;
                case NodeTypes.Air:
                    o = new NodeType
                    {
                        Color = Vector3.One,
                        Texture = new Vector3(14, 0, 0),
                        CanCollide = false,
                        CanRender = false,
                        Emission = new Vector3(.1f, .125f, 0.25f),
                    };
                    break;
                case NodeTypes.EarthBack:
                    o = new NodeType
                    {
                        Color = Vector3.One,
                        Texture = new Vector3(14, 0, 0),
                        CanCollide = false,
                        CanRender = true,
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
                        CanCollide = true,
                        CanRender = true,
                        //ReflectionAngle = 0.1f,
                        Color = new Vector3(0.5f, 0.5f, 1),
                        Opacity = 0.1f,
                        ResolveCollision = Slowdown,
                    };
                    break;
                case NodeTypes.Lava:
                    o = new AnimatedNode
                    {
                        Animation = new PassiveAnimationSet
                        {
                            Frames = lavaAnims.Frames,
                            ActiveAnimation = "0"
                        },
                        
                        Color = new Vector3(1.0f, 0.3f, 0.0f),
                        ResolveCollision = HardCollision,
                        Opacity = 0.9f,
                        Emission = new Vector3(1f, 0.3f, 0.0f),
                    };
                    break;

                case NodeTypes.Cloud:
                    o = new NodeType
                    {
                        Color = Vector3.One,
                        Texture = new Vector3(15, 15, 0),
                        Opacity = 0.2f,
                        CanCollide = false,
                    };
                    break;
                case NodeTypes.Brick:
                    o = new NodeType
                    {
                        Color = new Vector3(0.9f, 0.9f, 0.9f),
                        Texture = new Vector3(1f, 7f, 0),
                        ResolveCollision = HardCollision,
                        Opacity = 1.0f,
                    };
                    break;
                case NodeTypes.BrickBack:
                    o = new NodeType
                    {
                        CanCollide = false,
                        Texture = new Vector3(2f, 7f, 0),
                        Color = new Vector3(0.6f, 0.6f, 0.5f),
                    };
                    break;
                default:
                    o = new NodeType();
                                        
                    o.Color = Vector3.One;
                    o.CanCollide = true;
                    o.ResolveCollision = NodeFactory.HardCollision;
                    o.Opacity = 1;
                    o.Texture = new Vector3(15, 15, 0);
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
            
            /*
            var delta = (player.Postion + speed - node.Postion);
            var dir = new Vector3(Math.Abs(delta.X)> Math.Abs(delta.Y) * player.Size.Y ? Math.Sign(delta.X) : 0,
                                  Math.Abs(delta.X)> Math.Abs(delta.Y) * player.Size.Y ? 0 : Math.Sign(delta.Y),
                                  0);

            var dist = new Vector3(delta.X - dir.X * (node.Size.X + player.Size.X) / 2, delta.Y - dir.Y * (node.Size.Y + player.Size.Y) / 2, 0);

            var nDir = Vector3.Dot(dir, dist);

            var finalSpeed = speed - nDir * dir;

            player.Speed = finalSpeed;


            if (dir.Y == 1)
                player.Force.Y = player.Gravity;*/

            var delta = (player.Postion + speed - node.Postion);
            delta = new Vector3(Math.Abs(delta.X) > Math.Abs(delta.Y) ? Math.Sign(delta.X) : 0, Math.Abs(delta.X) > Math.Abs(delta.Y) ? 0 : Math.Sign(delta.Y), 0);
            var nV = Vector3.Dot(delta, speed);

            player.Speed = speed - MathHelper.Min(nV, 0) * delta;
        }


        public static void Water(Node node, Entity player, Vector3 speed)
        {

        }

        public static void RightSlope(Node node, Entity player, Vector3 speed)
        {

            BoundingBox b = new BoundingBox(player.CollisionBox.Min + speed, player.CollisionBox.Max + speed);
            Ray r = new Ray(node.Postion - new Vector3(0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0));
            float? dist = r.Intersects(b);

            if (dist == null)
                return;

            if (b.Max.X > node.CollisionBox.Max.X || b.Max.Y <= node.CollisionBox.Min.Y + 0.1f)
            {
                HardCollision(node, player, speed);
                return;
            }

            Ray r2 = new Ray(new Vector3(b.Max.X, b.Min.Y, 0), new Vector3(-0.5f, 0.5f, 0));

            var c1 = r.Position.Y - r.Position.X;
            var c2 = r2.Position.X;

            var dir = new Vector3(r2.Position.X, (c1 + c2), 0) - r2.Position; // (c1 + c2) / 2 - c1,

            var finalSpeed = speed + dir;
            player.Speed = finalSpeed;
            player.Force.Y = -finalSpeed.Y + player.Gravity;
        }


        public static void LeftSlope(Node node, Entity player, Vector3 speed)
        {
            BoundingBox b = new BoundingBox(player.CollisionBox.Min + speed, player.CollisionBox.Max + speed);
            Ray r = new Ray(node.Postion + new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0));
            float? dist = r.Intersects(b);

            if (dist == null)
                return;

            if (b.Min.X < node.CollisionBox.Min.X || b.Max.Y < node.CollisionBox.Min.Y + 0.1f)
            {
                HardCollision(node, player, speed);
                return;
            }

            Ray r2 = new Ray(new Vector3(b.Min.X, b.Min.Y, 0), new Vector3(0.5f, 0.5f, 0));

            var c1 = r.Position.Y + r.Position.X;
            var c2 = r2.Position.X;

            var dir = new Vector3(r2.Position.X, (c1 - c2), 0) - r2.Position; // (c1 + c2) / 2 - c1,

            var finalSpeed = speed + dir;
            player.Speed = finalSpeed;
            player.Force.Y = -finalSpeed.Y + player.Gravity;
        }


        public static void Slowdown(Node node, Entity player, Vector3 speed)
        {
            player.Speed = speed / 2;
        }

        public static void FreeMotion(Node node, Entity player, Vector3 speed)
        {
            player.Force.Y = player.Gravity;
        }


        public static Vector3 DiffuseAmbientLight (Node node)
        {
            return (node.Incident);
        }

        public static Vector3 DiffuseLight (Node node)
        {
            return node.Incident;
        }
    }

    public class Node : Instanced
    {
        public NodeType Type;
        public double Value;

        public Vector3 Postion;
        public int Updated;


        public Vector3 Light;
        public Vector3 Incident;
        public Vector3 Exitent;
        public Vector3 Emmision;

        public float[] LightDirection = new float[8];
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
            if (Type!= null && Type.Type == newType.Type&& newType.Type != NodeTypes.Custom)
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

        public void GetInstanceData(RenderGroup RenderGroup)
        {
            Instance.Color = new Color(Type.Color);
            /*if (Emmision.X > 1 && Emmision.Y > 1 && Emmision.Z > 1)
                Instance.Light = Vector3.One; //Vector3.One * Type.Opacity;//
            else*/
            Instance.Light = new Vector4(Incident + Emmision, Type.Opacity);
            Instance.Texture = Type.Texture;
            Type.GetInstanceData(Instance, RenderGroup);
            RenderGroup.AddInstance(Instance);

            foreach (IDecal decal in Type.Decals)
            {
                decal.GetInstanceData(RenderGroup);
            }

        }

        public void ResolveCollision(Entity player, Vector3 speed)
        {
            Type.ResolveCollision(this, player, speed);
        }
    }
}
