using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave
{
    public enum NodeType
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

    public class Node : Instanced
    {
        public NodeType Type;
        public double Value;

        public Vector3 Postion;
        public Vector3 Color = Vector3.One;
        public Vector3 Diffuse;
        public Vector3 Ambience;
        public LightType LType;
        public Vector3 Texture;
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
            if (Type == NodeType.Air)
            {
                Color = Vector3.One;
                Texture = new Vector3(0,1,0);
            }
        }

        public void SetPosition(Vector3 pos)
        {
            Postion = pos;
            Instance.World = Matrix.CreateTranslation(pos);
            var minV = new Vector3(pos.X - Size.X/2, pos.Y - Size.Y/2, 0);
            var maxV = new Vector3(pos.X + Size.X / 2, pos.Y + Size.Y / 2, 0);

            CollisionBox = new BoundingBox(minV, maxV);
        }

        public InstanceData GetInstanceData()
        {
            Instance.Color = Color;
            Instance.Light = Diffuse + Ambience;
            Instance.Texture = Texture;
            return Instance;
        }
    }
}
