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
        void Update(Node[,] ForeGround, int[] Relief, int X, int Y, BoundingBox area);
        void Clear();
    }
    
    
    public class PointLight : ILight
    {
        public Vector3 Position;
        private List<Node> DirectlyLight = new List<Node>();
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
            }
        }


        public void Update(Node[,] ForeGround, int[] Relief, int X, int Y, BoundingBox area)
        {

            for (int a = 0; a < 360; a++)
            {
                //TODO: move to table
                float c = Cos[a];
                float s = Sin[a];
                float intensity = 1.0f;
                Vector2 ray = new Vector2(c, s);

                for (int r = 1; r < 100 && intensity > 0; r++)
                {
                    int x = (int)((r * ray.X) + Position.X);
                    int y = (int)((r * ray.Y) + Position.Y);

                    if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                    {
                        var node = ForeGround[x, y];
                        if (node.Type.Opacity != 0)
                        {
                            node.Diffuse = new Vector3(intensity);
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
    }

    public class SkyLight : ILight
    {
        public Vector3 Color;
        
        public List<Node> DirectlyLight = new List<Node>();

        public void Update(Node[,] ForeGround, int[] Relief, int X, int Y, BoundingBox area)
        {



            for (int i1 = (int)area.Min.X; i1 < area.Max.X; i1++)
            {
                float ambIntencity = 1;
                if (area.Max.Y - 1 < Relief[i1])
                    continue;
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
    }
}
