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

        public Light sun;

        private Vector3 Sky = new Vector3(.6f, .6f, 1);

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
            sun = new Light { Position = new Vector3(50, 50, 0) };
            int i3 = 50;
            for (int i1 = 0; i1 < X; i1++)
            {
                double x = i1 / Size.X;
                    
                double noise = Noise.NextOctave1D(1, -x, 0) / 2 + 0.5f;
                double noise2 = Math.Abs(Noise.NextOctave1D(2, x, 5)) / 10;

                noise = MathHelper.Clamp( (float) (noise + noise2), 0, 1);
                i3 = (int)(noise * (Y -1));

                var node = new Node
                {
                    Value = noise,
                };

                node.SetType(NodeFactory.Get(NodeTypes.Soil));
                node.SetPosition(new Vector3(i1, i3, 0));

                ForeGround[i1, i3] = node;

                for (int i2 = 0; i2< i3 - 3; i2++)
                {
                    double y = i2 * 1.0 / i3;
                    noise = 1 - Noise.NextOctave2D(4, x, y) / 4;
                    node = new Node
                    {
                        Value = noise,
                    };

                    node.SetType(NodeFactory.Get(noise < 0.6f ? NodeTypes.Earth : NodeTypes.Air, true));

                    node.SetPosition(new Vector3(i1, i2, 0));

                    ForeGround[i1, i2] = node;
                }

                for (int i2 = i3 - 3; i2 < i3; i2++)
                {
                    node = new Node
                    {
                        Value = noise,
                    };

                    node.SetType(NodeFactory.Get(NodeTypes.Earth));

                    node.SetPosition(new Vector3(i1, i2, 0));
                    ForeGround[i1, i2] = node;
                }


                for (int i2 = i3 + 1; i2 < Y; i2++)
                {
                    double y = i2 / Size.Y;
                    noise = Noise.NextOctave2D(2, -x, -y);
                    node = new Node
                    {
                        Value = noise,
                    };

                    node.SetType(NodeFactory.Get(NodeTypes.Air, false));

                    node.SetPosition(new Vector3(i1, i2, 0));
                    node.Ambience = Sky;
                    ForeGround[i1, i2] = node;
                }
            }
        }

        readonly Vector2[] Rays = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 1) };
        private Vector3 lightUp (Node node)
        {
            Vector3 sum = new Vector3();
            Vector3 sum2 = new Vector3();
            int count = 0, count2 = 0;

            for (int i2 = 0; i2 < Rays.Length; i2++)
            {
                int x = (int)(node.Postion.X + Rays[i2].X);
                int y = (int)(node.Postion.Y + Rays[i2].Y);

                if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                {
                    Node hit = ForeGround[x, y];
                    switch(ForeGround[x, y].Type.Type)
                    {
                        case NodeTypes.Soil:
                        case NodeTypes.Earth:
                        {
                            sum2 += (hit.Diffuse * hit.Type.Color + hit.Ambience * hit.Type.Color*2);
                            count2++;
                            break;
                        }
                        default:
                        {
                            sum += hit.Diffuse;
                            count++;
                            break;
                        }
                    }
                }
                else
                {
                    //if (node.Type == NodeType.Air)
                    //    sum += Sky;
                    //count++;
                }
            }

                count += count2;
                sum += sum2;


            if (count == 0)
                return sum;

            return (sum / (float)count);
        }

        List<Node> DirectlyLight = new List<Node>();

        private void directLights()
        {
            sun.Update(ForeGround, X, Y);
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

                    if (node.Type.Type != NodeTypes.Air)
                    {
                        node.Ambience = Sky;
                        goto next;
                    }
                    node.Ambience = Sky;
                }
            next:
                i1++;
            }
        }

        public void Update(Camera cam)
        {
            directLights();

            int startX = (int)MathHelper.Clamp(cam.Position.X - 30, 0, X);
            int endX = (int)MathHelper.Clamp(cam.Position.X + 30, 0, X);
            int endY = (int)MathHelper.Clamp(cam.Position.Y - 20, 0, Y);

            for (int i1 = startX; i1 < endX; i1++)
            {
                bool amb = true;
                float  ambIntencity = 1;
                for (int i2 = Y - 1; i2 >= endY; i2--)
                {
                    var node = ForeGround[i1, i2];

                    if (amb)
                    {
                        if (node.Type.Opacity != 0)
                        {
                            node.Ambience = Sky * ambIntencity;
                            ambIntencity -= node.Type.Opacity;
                            if (ambIntencity <= 0)
                                amb = false;
                        }
                    }
                    if (node.LType!= LightType.Direct)
                        node.Diffuse = lightUp(node);
                }
            }
        }

        public void ResolveCollisions(Entity ent)
        {
            var dir = Vector3.Normalize(ent.Speed);
            var len = ent.Speed.Length();
            for (float r = 1; r < len + 1; r+=1f)
            {
                var dist = (r>len?len:r) * dir;
                var box = new BoundingBox(ent.CollisionBox.Min + dist, ent.CollisionBox.Max + dist);

                int minX = (int)box.Min.X - 1;
                int maxX = (int)box.Max.X + 2;
                int minY = (int)box.Min.Y - 1;
                int maxY = (int)box.Max.Y + 2;

                if (minX < 0) minX = 0;
                if (minY < 0) minY = 0;
                if (maxX >X ) maxX = X;
                if (maxY >= Y) maxY = Y;

                
                for (int i2 = minY; i2 < maxY; i2++)
                    for (int i1 = minX; i1 < maxX; i1++)
                {
                    var node = ForeGround[i1, i2];

                    if (! node.Type.CanCollide)
                        continue;

                    if (node.CollisionBox.Intersects(box))
                    {
                        node.ResolveCollision(ent, dist);
                        dir = Vector3.Normalize(ent.Speed);
                        len = ent.Speed.Length();
                        dist = (r > len ? len : r) * dir;
                        if (len == 0)
                            goto end;
                    }
                }
        }
        end:
            return;
        }

        public void AddToDraw(Camera cam, Instancer instancer)
        {
            for (int i1 = 0; i1 < X; i1++)
                for (int i2 = 0; i2 < Y; i2++)
                {
                    var node = ForeGround[i1, i2];

                    if (node.Type.CanRender)
                    {
                        var testres = cam.Frustrum.Contains(node.CollisionBox);
                        if (testres == ContainmentType.Intersects || testres == ContainmentType.Contains)
                            instancer.AddInstance(node);
                    }
                }
        }
    }
}
