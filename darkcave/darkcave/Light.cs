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

        public List<Node> DirectlyLight = new List<Node>();
        private float[] distances = new float[722];
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
            /*for (int i1 = (int)area.Min.X; i1 < area.Max.X; i1++)
            {
                for (int i2 = (int)area.Min.Y; i2 < area.Max.Y; i2++)
                {
                    var node = ForeGround[i1, i2];

                    if (node.Type.Opacity == 1)
                    {
                        var radial = ToRadial(node.Postion);
                        if (radial.Item2 <= distances[radial.Item1])
                        {
                            distances[radial.Item1] = radial.Item2;
                        }
                    }
                }
            }

            for (int i1 = (int)area.Min.X; i1 < area.Max.X; i1++)
            {
                for (int i2 = (int)area.Min.Y; i2 < area.Max.Y; i2++)
                {
                    var node = ForeGround[i1, i2];

                    var radial = ToRadial(node.Postion);
                    if (radial.Item2 <= distances[radial.Item1])
                    {
                        node.Emmision += new Vector3(.5f);
                    }
                }
            }
            return;*/
            
            Vector3 ray = Vector3.Zero;
            for (int a = 0; a < 360; a++)
            {
                ray.X = Cos[a];
                ray.Y = Sin[a];
                float intensity = 1f;

                float r = distances[a] == int.MaxValue ? 1 : distances[a];

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
                            node.LType |= LightType.Direct; 
                            node.LightDirection = Vector3.Normalize(node.Postion - position);
                            if (node.Emmision.X < 2 && node.Emmision.Y < 2 && node.Emmision.Z < 2)
                                node.Emmision += new Vector3(intensity);
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

        private Tuple<int, float> ToRadial(Vector3 position)
        {
            Vector3 diff = position - Position;
            float dist = diff.Length();
            float a = MathHelper.ToDegrees((float)(Math.Atan2(diff.X, diff.Y) + Math.PI));
            //System.IO.File.AppendAllText("D:\\1.txt", ((int)a).ToString() + " " + ((int)(a*2)).ToString() + "\n");
            int angle = (int)(a);
            return Tuple.Create(angle, dist);
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
            {
                DirectlyLight[i].LType = LightType.None;
                DirectlyLight[i].Emmision = Vector3.Zero;
            }

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

            var radial = ToRadial(node.Postion);
            if (distances[radial.Item1] < radial.Item2)
                distances[radial.Item1] = radial.Item2;
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
                        DirectlyLight.Add(node);
                        node.LType |= LightType.Direct;
                        node.LightDirection = new Vector3 (0, -1, 0);
                        if (node.Emmision.X < 2 && node.Emmision.Y < 2 && node.Emmision.Z < 2)
                            node.Emmision += Color * (ambIntencity);
                        ambIntencity -= node.Type.Opacity;
                    }
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < DirectlyLight.Count; i++)
            {
                DirectlyLight[i].LType = LightType.None;
                DirectlyLight[i].Emmision = Vector3.Zero;
            }
            DirectlyLight.Clear();
        }


        public void Update(Node node)
        {
        }
    }
}
