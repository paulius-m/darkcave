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

        public static NodeType Get(NodeTypes type)
        {
            var o = new NodeType();
            o.Type = type;

            switch (type)
            { 
                case NodeTypes.Earth:
                    o.Color = new Vector3(.4f, .2f, 0.1f);
                    o.Texture = new Vector3(1, 0, 0);
                    break;
                case NodeTypes.Soil:
                    o.Color = new Vector3(.5f, 1.0f, 0.0f);
                    o.Texture = new Vector3(2, 0, 0);
                    break;
                case NodeTypes.Air:
                    o.Color = Vector3.One;
                    o.Texture = new Vector3(0,1,0);
                    break;
            }
            return o;
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
            if (Type == newType)
                return;
            Type = newType;
        }

        public void SetPosition(Vector3 pos)
        {
            Postion = pos;
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
    }
}
