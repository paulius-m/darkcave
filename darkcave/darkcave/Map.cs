using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace darkcave
{
    class Map
    {
        public Vector3 Size;
        private readonly int X;
        private readonly int Y;

        public Vector3 sun;

        private Vector3 Sky = new Vector3(.4f, .6f, 1);

        public Node[][,] Data;

        public Node[,] ForeGround;

        public Map(Vector3 size )
        {
            Size = size;
            X = (int)Size.X;
            Y = (int)Size.Y;
        }

        public void Init()
        {
            Data = new Node[2][,];
            Data[0] = new Node[X, Y];
            Data[1] = new Node[X, Y];

            ForeGround = Data[0];
            sun = new Vector3(0, 99, 0);
            int i3 = 50;

            for (int i1 = 0; i1 < X; i1++)
            {
                double x = i1 / Size.X;
                    
                double noise = Noise.NextOctave2D(10, x, 1);

                i3 += (int)(noise);

                var node = new Node
                {
                    Value = noise,
                };

                node.SetType(NodeType.Earth);
                node.SetPosition(new Vector3(i1, i3, 0));
                node.Color = new Vector3(0, .1f, 0.0f);

                ForeGround[i1, i3] = node;
            }

            
            for (int i1 = 0; i1 < X; i1++)
            {
                int middle = -1;
                for (int i2 = Y-1; i2 >=0 ; i2--)
                {
                    double x = i1 / Size.X;
                    double y = i2 / Size.Y;
                    double noise = Noise.NextOctave2D(4, x, y);

                    //if (ForeGround[i1, i2] != null)
                    //{
                    //    middle = i2;
                    //    continue;
                    //}

                    var node = new Node
                    {
                        Value = noise,
                    };

                    node.SetType((noise < 0.5f) ? NodeType.Earth : NodeType.Air);

                    node.SetPosition(new Vector3(i1, i2, 0));

                    if (node.Type == NodeType.Earth)
                    {
                        node.Color = new Vector3(.4f, .2f, 0.1f);
                        node.Texture = new Vector3(1, 0, 0);
                    }

                    if (node.Type == NodeType.Air)
                    {
                        node.Color = new Vector3((float)noise * 2);
                        node.Texture = new Vector3(0,0,0);
                    }

                    ForeGround[i1, i2] = node;
                }
            }

            ProprocesDraw();
        }

        readonly Vector2[] Rays = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 1) };
        private Vector3 lightUp (Node node)
        {
            Vector3 sum = new Vector3();
            int count = 0;

            for (int i2 = 0; i2 < Rays.Length; i2++)
            {
                int x = (int)(node.Postion.X + Rays[i2].X);
                int y = (int)(node.Postion.Y + Rays[i2].Y);

                if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                {
                    Node hit = ForeGround[x, y];
                    switch(ForeGround[x, y].Type)
                    {
                        case NodeType.Earth:
                        {
                            if (node.Type == NodeType.Air)
                            {
                                sum += (hit.Diffuse + hit.Ambience) * hit.Color;
                                count++;
                            }
                            break;
                        }
                        case NodeType.Air:
                        {
                            sum += hit.Diffuse;
                            count++;
                            break;
                        }
                    }
                }
                else
                {
                    if (node.Type == NodeType.Air)
                        sum += Sky;
                    count++;
                }
            }

            if (count == 0)
                return sum;

            return (sum / (float)count);
        }

        List<Node> DirectlyLight = new List<Node>();

        const double DegToRad =  0.01745327;
        private void directLights()
        {
            for (int i = 0; i < DirectlyLight.Count; i++)
                DirectlyLight[i].LType = LightType.Ambient;

            DirectlyLight.Clear();
            for (int a = 0; a < 360; )
            {
                float c = (float)Math.Cos(MathHelper.ToRadians(a));
                float s = (float)Math.Sin(MathHelper.ToRadians(a));
                Vector2 ray = new Vector2(c, s);

                for (int r = 1; r < 100; r++)
                {
                    int x = (int)((r * ray.X) + sun.X);
                    int y = (int)((r * ray.Y) + sun.Y);

                    if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                    {

                        if (ForeGround[x, y].Type == NodeType.Earth)
                        {
                            ForeGround[x, y].Diffuse = new Vector3(1, 1, 1);
                            ForeGround[x, y].LType = LightType.Direct;
                            DirectlyLight.Add(ForeGround[x, y]);
                        }
                        else
                        {
                            //Data[x, y].Diffuse = new Vector3(0.1f, 0.1f, 0.1f);
                            //Data[x, y].LType = LightType.Direct;
                            //DirectlyLight.Add(Data[x, y]);
                            continue;
                        }
                    }
                    goto next;
                }
            next:
                a++;
            }
        }

        public Node GetNode(int x, int y)
        {

            if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
            {
                return ForeGround[x, y];

            }

            return null;
        }

        private void ProprocesDraw()
        {
            for (int i1 = 0; i1 < X; )
            {
                for (int i2 = Y - 1; i2 >= 0; i2--)
                {
                    var node = ForeGround[i1, i2];


                    if (node.Type != NodeType.Air)
                    {
                        node.Ambience = Sky;
                        goto next;
                    }
                }
            next:
                i1++;
            }
        }

        int start = 0;

        public void Update()
        {
            directLights();

            for (int i1 = start; i1 < X; i1 += 2)
            {
                bool amb = true;
                for (int i2 = Y - 1; i2 >= 0; i2--)
                {
                    var node = ForeGround[i1, i2];
                    if (amb)
                    {
                        
                        amb = node.Type == NodeType.Air;
                        if (!amb)
                            node.Ambience = Sky;
                    }   


                    if (node.LType == LightType.Ambient || node.Type != NodeType.Earth)
                    {
                        node.Diffuse = lightUp(node);
                    }
                }
            }
            start ^= 1;
        }

        public void AddToDraw(Camera cam, Instancer instancer)
        {
            for (int i1 = 0; i1 < X; i1++)
                for (int i2 = 0; i2 < Y; i2++)
                {
                    var node = ForeGround[i1, i2];

                    if (cam.Frustrum.Contains(node.Postion) == ContainmentType.Contains)
                        instancer.AddInstance(node);
                }
        }
    }
}
