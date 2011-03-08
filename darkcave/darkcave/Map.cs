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

        public Node[,] Data;

        public Map(Vector3 size )
        {
            Size = size;
            X = (int)Size.X;
            Y = (int)Size.Y;
        }

        public void Init()
        {
            Data = new Node[X, Y];
            sun = new Vector3(50, 70, 0);

            for (int i1 = 0; i1 < X; i1++)
            for (int i2= 0; i2< Y; i2++)
            {
                double x = i1 / Size.X;
                double y = i2 / Size.Y;
                double noise = Noise.NextOctave2D(1, x, y) + y;

                var node = new Node {
                    Value = noise,
                };

                node.SetType((noise < .5) ? NodeType.Earth : NodeType.Air);
                node.SetPosition(new Vector3(i1, i2, 0));
                if (node.Type == NodeType.Earth)
                    node.Color = new Vector3(0, .5f, 0.0f);

                Data[i1, i2] = node;
            }

            ProprocesDraw();
        }

        readonly Vector2[] Rays = new Vector2[] { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0) };
        private Vector3 lightUp (Node node)
        {

            Vector3 sum = new Vector3();
            int count = 0;

            if (node.LType != LightType.Ambient && node.Type == NodeType.Earth)
                return node.Light;

            for (int i2 = 0; i2 < Rays.Length; i2++)
            {
                int x = (int)(node.Postion.X + Rays[i2].X);
                int y = (int)(node.Postion.Y + Rays[i2].Y);

                if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                {
                    Node hit = Data[x, y];
                    switch(Data[x, y].Type)
                    {
                        case NodeType.Earth:
                        {
                            if (node.Type == NodeType.Air)
                            {
                                sum += hit.Light * hit.Color;
                                count++;
                            }
                            break;
                        }
                        case NodeType.Air:
                        {
                            sum += hit.Light;
                            count++;
                            break;
                        }

                    }
                }
                else
                {
                    if (node.Type == NodeType.Air)
                        sum += new Vector3(.4f, .6f, 1);
                    count++;
                }
            }
            
            if (count == 0)
                return sum;

            return (sum / (float)count);
        }

        List<Node> DirectlyLight = new List<Node>();
        private void directLights()
        {
            for (int i = 0; i < DirectlyLight.Count; i++)
                DirectlyLight[i].LType = LightType.Ambient;

            DirectlyLight.Clear();
            for (int a = 0; a < 360; a++)
            {
                float c = (float)Math.Cos(a / 180.0f * Math.PI);
                float s = (float)Math.Sin(a / 180.0f * Math.PI);
                Vector2 ray = new Vector2(c, s);

                for (int r = 1; r < 100; r++)
                {
                    int x = (int)((r * ray.X) + sun.X);
                    int y = (int)((r * ray.Y) + sun.Y);

                    if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                    {

                        if (Data[x, y].Type == NodeType.Earth)
                        {
                            Data[x, y].Light = new Vector3(1, 1, 1);
                            Data[x, y].LType = LightType.Direct;
                            DirectlyLight.Add(Data[x, y]);
                        }
                        else
                            continue;
                    }
                    goto next;
                }
            next:
                continue;
            }
        }

        public Node GetNode(int x, int y)
        {

            if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
            {
                return Data[x, y];

            }

            return null;
        }

        private void ProprocesDraw()
        {
            for (int i1 = 0; i1 < X; i1++)
            {
                for (int i2 = Y - 1; i2 >= 0; i2--)
                {
                    var node = Data[i1, i2];
                    node.Light = new Vector3(.4f, .6f, 1);

                    if (node.Type != NodeType.Air)
                        goto next;
                }
            next:
                continue;
            }
        }

        public void Update( Camera cam, Instancer instancer)
        {
            directLights();
            for (int i1 = 0; i1 < X; i1++)
            for (int i2 = 0; i2 < Y; i2++)
            {
                var node = Data[i1, i2];

                node.Light = lightUp(node);

                if (cam.Frustrum.Contains(node.Postion) == ContainmentType.Contains)
                    instancer.AddInstance(node.GetInstanceData());
            }
        }

        /*
        public void AddToDraw()
        { 
            for (int i1 = 0; i1 < Size.X; i1++)
            for (int i2 = 0; i2 < Size.Y; i2++)
            {
                var node = Data[i1, i2];
                
            }
        }*/

    }
}
