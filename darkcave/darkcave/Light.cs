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
            for (int a = 0; a < 180; )
            {
                //TODO: move to table
                float c = (float)Math.Cos(MathHelper.ToRadians(a));
                float s = (float)Math.Sin(MathHelper.ToRadians(a));
                Vector2 ray = new Vector2(c, s);

                for (int r = 1; r < 50; r++)
                {
                    int x = (int)((r * ray.X) + Position.X);
                    int y = (int)((r * ray.Y) + Position.Y);

                    if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                    {

                        if (ForeGround[x, y].Type.Type != NodeTypes.Air)
                        {
                            ForeGround[x, y].Diffuse = new Vector3(1, 1, 1);
                            ForeGround[x, y].LType = LightType.Direct;
                            DirectlyLight.Add(ForeGround[x, y]);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    goto next;
                }
            next:
                a++;
            }
        }
    }
}
