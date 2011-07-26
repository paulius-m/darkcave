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

        public Node[][,] Data;

        public Node[,] ForeGround;

        public Map(Vector3 size )
        {
            Size = size;
            X = (int)Size.X;
            Y = (int)Size.Y;
        }

        List<IMapComponent> components = new List<IMapComponent> { new LightingComponent(), new WaterSimulator(), new TextureModifier() };

        public void Init()
        {
            Data = new Node[2][,];
            Data[0] = new Node[X, Y];
            Data[1] = new Node[X, Y];

            ForeGround = Data[0];

            int i3 = 50;
            for (int i1 = 0; i1 < X; i1++)
            {
                double x = i1 / Size.X;
                    
                double noise = Noise.NextOctave1D(1, -x, 0) / 2 + 0.5f;
                double noise2 = Math.Abs(Noise.NextOctave1D(4, x, 2.5f)) / 10;

                noise = MathHelper.Clamp( (float) (noise + noise2), 0, 1);
                i3 = (int)(noise * (Y -1));

                var node = new Node
                {
                    Value = noise,
                    
                };

                node.SetPosition(new Vector3(i1, i3, 0));
                node.SetType( NodeFactory.Get(NodeTypes.Soil));

                ForeGround[i1, i3] = node;

                for (int i2 = 0; i2< i3 - 3; i2++)
                {
                    double y = i2 * 1.0 / i3;
                    noise = 1 - Noise.NextOctave2D(4, x, y) / 4;
                    node = new Node
                    {
                        Value = noise,
                    };

                    node.SetPosition(new Vector3(i1, i2, 0));
                    node.SetType(NodeFactory.Get(noise < 0.6f ? NodeTypes.Earth : NodeTypes.Air, true));

                    ForeGround[i1, i2] = node;
                }

                for (int i2 = i3 - 3; i2 < i3; i2++)
                {
                    node = new Node
                    {
                        Value = noise,
                    };
                    node.SetPosition(new Vector3(i1, i2, 0));
                    node.SetType(NodeFactory.Get(NodeTypes.Earth));

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
                    node.SetPosition(new Vector3(i1, i2, 0));
                    node.SetType(NodeFactory.Get(NodeTypes.Air));

                    //node.Ambience = Sky;
                    ForeGround[i1, i2] = node;
                }
            }

            foreach (var component in components)
                component.Init(ForeGround, X, Y);
        }

        private void PreUpdate(BoundingBox area)
        {
            
            foreach (var component in components)
                component.PreUpdate(area);
        }

        public Node GetNode(int x, int y)
        {

            if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
            {
                return ForeGround[x, y];
            }

            return null;
        }

        public void Update(Camera cam)
        {
            int startX = (int)MathHelper.Clamp(cam.Position.X - cam.ViewSize.X / 2 - 15, 0, X);
            int endX = (int)MathHelper.Clamp(cam.Position.X + cam.ViewSize.X / 2 + 15, 0, X);
            int startY = (int)MathHelper.Clamp(cam.Position.Y - cam.ViewSize.Y / 2 - 15, 0, Y);
            int endY = (int)MathHelper.Clamp(cam.Position.Y + cam.ViewSize.Y / 2 + 15, 0, Y);
            var area = new BoundingBox(new Vector3(startX, startY, 0), new Vector3(endX, endY, 0));
            PreUpdate(area);

            for (int i1 = startX; i1 < endX; i1++)
            {
                for (int i2 = startY; i2 < endY; i2++)
                {
                    var node = ForeGround[i1, i2];
                    foreach (var component in components)
                        component.Update(node);
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
            int startx = (int)MathHelper.Clamp(cam.Position.X - cam.ViewSize.X / 2, 0, X);
            int endx = (int)MathHelper.Clamp(cam.Position.X + cam.ViewSize.X / 2 + 2, 0, X);
            int starty = (int)MathHelper.Clamp(cam.Position.Y - cam.ViewSize.Y / 2, 0, Y);
            int endy = (int)MathHelper.Clamp(cam.Position.Y + cam.ViewSize.Y / 2 + 2, 0, Y);

            for (int i1 = startx; i1 < endx; i1++)
                for (int i2 = starty; i2 < endy; i2++)
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

        public LocalEnvironment Describe (Vector3 position)
        {
            return new LocalEnvironment { Node = this.GetNode((int) Math.Round( position.X), (int) Math.Round(position.Y))};
        }

        public void SpawnLight(Vector3 position)
        {
            //lights.Add(new PointLight { Position = position });
        }

    }
}
