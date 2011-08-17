using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave
{
    [Flags]
    public enum LightType
    {
        None = 0,
        Ambient = 1,
        Direct = 2,
    }

    public interface ILight
    {
        void Update(BoundingBox area);
        void Clear();
        void Update(Node node);
    }
    
    
    public class PointLight : ILight
    {
        private Vector3 position;
        public Vector3 Position
        {
            get { return position; }
            set { 
                if (value != position)

                position = value;}
        }

        public Node[,] ForeGround;
        public int X;
        public int Y;

        private List<Node> DirectlyLight = new List<Node>();
        private int[] distances = new int[360];
        private float[] Cos;
        private float[] Sin;

        public PointLight()
        {
            Cos = new float[360];
            Sin = new float[360];

            for (int a = 0; a < 360; a++)
            {
                Cos[a] = (float)Math.Cos(MathHelper.ToRadians(a));
                Sin[a] = (float)Math.Sin(MathHelper.ToRadians(a));
                distances[a] = int.MaxValue;
            }
        }


        public void Update(BoundingBox area)
        {

            for (int a = 0; a < 360; a++)
            {
                //TODO: move to table
                float c = Cos[a];
                float s = Sin[a];
                float intensity = 1.0f;
                Vector3 ray = new Vector3(c, s, 0);

                int r = distances[a] == int.MaxValue ? 1 : distances[a];

                for (; r < 100 && intensity > 0; r++)
                {
                    int x = (int)((r * ray.X) + Position.X);
                    int y = (int)((r * ray.Y) + Position.Y);

                    Node oldNode;

                    if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                    {
                        var node = ForeGround[x, y];


                        if (node.Type.Opacity != 0)
                        {
                            if (r < distances[a])
                                distances[a] = r;

                            node.Diffuse = new Vector3(intensity);
                            //node.LightDirection += ray;
                            node.LType |= LightType.Direct;

                            /*
                            if (node.Type.ReflectionAngle != 0)
                            {
                                ray = Rotate(ray, node.Type.ReflectionAngle);
                            }*/
                            DirectlyLight.Add(node);

                            intensity -= node.Type.Opacity;
                        }
                        
                    }
                }
            }
        }

        private Vector2 Rotate(Vector2 ray, float a)
        {
            float c2 = (float)Math.Cos(a);
            float s2 = (float)Math.Sin(a);
            return new Vector2(ray.X * c2 - ray.Y * s2, ray.X * s2 + ray.Y * c2);
        }


        public void Clear()
        {
            for (int i = 0; i < DirectlyLight.Count; i++)
                DirectlyLight[i].LType = LightType.None;

            DirectlyLight.Clear();
        }

        private void ClearDistances()
        {
            for (int a = 0; a < 360; a++)
            {
                distances[a] = int.MaxValue;
            }
        }


        public void Update(Node node)
        {
            if (node.Type.Opacity == 0)
            {
                return;
            }
            var direction = node.Postion - Position;
            int distance = (int)direction.Length();


            for (int a = 0; a < 360; a++)
            {
                if (distance < distances[a])
                    distances[a] = distance;
            }
        }
    }

    public class SkyLight : ILight
    {
        public Vector3 Color;
        public Node[,] ForeGround;
        public int X;
        public int Y;
        public int[] Relief;




        public List<Node> DirectlyLight = new List<Node>();

        public void Update(BoundingBox area)
        {
            for (int i1 = (int)area.Min.X; i1 < area.Max.X; i1++)
            {
                float ambIntencity = 1;

                for (int i2 = Relief[i1]; i2 >= area.Min.Y; i2--)
                {
                    var node = ForeGround[i1, i2];
                    if (node.Type.Opacity != 0 && ambIntencity > 0)
                    {
                        node.Ambience = Color * ambIntencity;
                        ambIntencity -= node.Type.Opacity;
                        DirectlyLight.Add(node);
                        node.LType |= LightType.Ambient;
                    }
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < DirectlyLight.Count; i++)
                 DirectlyLight[i].LType = LightType.None;

            DirectlyLight.Clear();
        }


        public void Update(Node node)
        {
        }
    }
}
