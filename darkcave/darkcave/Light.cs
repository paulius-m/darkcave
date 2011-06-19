using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave
{
    class Light
    {
        public Vector3 Position;
        private List<Node> DirectlyLight = new List<Node>();

        public void Update(Node[,] ForeGround, int X, int Y)
        {
            for (int i = 0; i < DirectlyLight.Count; i++)
                DirectlyLight[i].LType = LightType.Ambient;

            DirectlyLight.Clear();
            for (int a = 0; a < 360; a++)
            {
                //TODO: move to table
                float c = (float)Math.Cos(MathHelper.ToRadians(a));
                float s = (float)Math.Sin(MathHelper.ToRadians(a));
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
                            node.LType = LightType.Direct;
                            DirectlyLight.Add(node);

                            intensity -= node.Type.Opacity;
                        }
                    }
                }
            }
        }
    }
}
