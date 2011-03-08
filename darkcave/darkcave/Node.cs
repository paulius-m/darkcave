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
    
    public class Node
    {
        public NodeType Type;
        public double Value;

        public Vector3 Postion;
        public Vector3 Color = Vector3.One;
        public Vector3 Light;
        public LightType LType;

        private InstanceData instance;

        public Node()
        {
            instance = new InstanceData();
        }

        public void SetType(NodeType newType)
        {
            Type = newType;
        }

        public void SetPosition(Vector3 pos)
        {
            Postion = pos;
            instance.World = Matrix.CreateTranslation(pos);
        }

        public InstanceData GetInstanceData()
        {
            instance.Color = Color;
            instance.Light = Light;
            
            return instance;
        }
    }
}
