using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave
{
    public enum NodeTypes
    {
        Air = 0,
        Soil,
        Earth,
        Water,
        Fire
    }

    public enum LightType
    {
        None = 0,
        Ambient,
        Direct
    }


    public class NodeType
    {
        public NodeTypes Type;
        public Vector3 Color = Vector3.One;
        public Vector3 Texture;
        public bool CanCollide = true;
        public bool CanRender = true;
        public delegate void Collision(Node node, Entity player, Vector3 speed);
        public Collision ResolveCollision;


        public static NodeType Get(NodeTypes type)
        {
            var o = new NodeType();
            o.Type = type;

            switch (type)
            { 
                case NodeTypes.Earth:
                    o.Color = new Vector3(.6f, .4f, 0.3f);
                    o.Texture = new Vector3(1, 0, 0);
                    o.ResolveCollision = HardCollision;
                    break;
                case NodeTypes.Soil:
                    o.Color = new Vector3(.2f, 0.5f, 0.0f);
                    o.Texture = new Vector3(2, 0, 0);
                    o.ResolveCollision = HardCollision;
                    o.CanCollide = false;
                    break;
                case NodeTypes.Air:
                    o.Color = Vector3.One;
                    o.Texture = new Vector3(0,0,0);
                    o.CanCollide = false;
                    o.CanRender = false;
                    break;
            }
            return o;
        }

        public static NodeType Get(NodeTypes type, bool renderable)
        {
            var o = NodeType.Get(type);
            o.CanRender = renderable;
            return o;
        }
        public static NodeType Get(NodeTypes type, Vector3 texture)
        {
            var o = NodeType.Get(type);
            o.CanRender = true;
            o.Texture = texture;
            return o;
        }

        protected static void HardCollision(Node node, Entity player, Vector3 speed)
        {
            node.Diffuse = new Vector3(1, 0, 0);
            var delta = (player.Postion + speed - node.Postion);
            delta = new Vector3(Math.Abs(delta.X) > Math.Abs(delta.Y) ? Math.Sign(delta.X) : 0, Math.Abs(delta.X) > Math.Abs(delta.Y) ? 0 : Math.Sign(delta.Y), 0);
            var nV = Vector3.Dot(delta, speed);

            player.Speed = speed - MathHelper.Min(nV, 0) * delta;
        }

        protected static void Slowdown(Node node, Entity player, Vector3 speed)
        {
            player.Speed = speed / 2;
        }


    }

    public class Node : Instanced
    {
        public NodeType Type;
        public double Value;

        public Vector3 Postion;

        public Vector3 Diffuse;
        public Vector3 Ambience;
        public LightType LType;

        protected InstanceData Instance;
        public Vector3 Size = Vector3.One;
        public BoundingBox CollisionBox;

        public Node()
        {
            Instance = new InstanceData();
        }

        public void SetType(NodeType newType)
        {
            if (Type!= null && Type.Type == newType.Type)
                return;
            Type = newType;
        }

        public void SetPosition(Vector3 pos)
        {
            Postion = pos;
            if (Type.Type == NodeTypes.Soil)

                Instance.World = Matrix.CreateTranslation(pos + new Vector3(0,-0.5f, 0));
            else
                Instance.World = Matrix.CreateTranslation(pos);
            var minV = new Vector3(pos.X - Size.X / 2, pos.Y - Size.Y / 2, 0);
            var maxV = new Vector3(pos.X + Size.X / 2, pos.Y + Size.Y / 2, 0);

            CollisionBox = new BoundingBox(minV, maxV);
        }

        public InstanceData GetInstanceData()
        {
            Instance.Color = Type.Color;
            Instance.Light = Diffuse + Ambience;
            Instance.Texture = Type.Texture;
            return Instance;
        }

        public void ResolveCollision(Entity player, Vector3 speed)
        {
            Type.ResolveCollision(this, player, speed);
        }
    }
}
