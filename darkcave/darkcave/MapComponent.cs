using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave
{
    public interface IMapComponent
    {
        void Init(Node[,] ForeGround, int X, int Y);
        void PreUpdate(BoundingBox area);
        void Update(Node node);
    }

    public class LightingComponent : IMapComponent
    {
        public PointLight sun;
        public SkyLight sky;

        public List<ILight> lights = new List<ILight>();
        public int[] Relief;

        protected Node[,] ForeGround;
        private int X;
        private int Y;

        protected Vector3[][,] LightField;
        protected Vector3[,] LightDiffusion;

        public void Init(Node[,] foreGround, int x, int y)
        {
            ForeGround = foreGround;
            X = x;
            Y = y;
            LightField = new Vector3[2][,];
            LightField[0] = new Vector3[X, Y];
            LightField[1] = new Vector3[X, Y];
            LightDiffusion = new Vector3[X, Y];
            Relief = new int[x];

            sun = new PointLight { Position = new Vector3(50, 90, 0), ForeGround = foreGround, X = x, Y = y};
            sky = new SkyLight { Color = new Vector3(.4f, .5f, 1f), ForeGround = foreGround, X = x, Y = y, Relief = Relief};

            for (int i1 = 0; i1 < X; i1++)
            {
                bool set = false;
                for (int i2 = Y - 1; i2 >= 0; i2--)
                {
                    var node = ForeGround[i1, i2];
                    node.TypeChanged += new Node.NodeEventHandler(UpdateRelief);
                    if (!set && node.Type.Opacity > 0)
                    {
                        Relief[i1] = i2;
                        set = true;
                    }
                }
            }
        }

        public void PreUpdate(BoundingBox area)
        {
            sun.Clear();
            sky.Clear();
            foreach (var light in lights)
            {
                light.Clear();
            }

            sun.Update(area);
            sky.Update(area);

            foreach (var light in lights)
            {
                light.Update(area);
            }

            for (int i1 = (int)area.Min.X; i1 < area.Max.X; i1++)
            {
                for (int i2 = (int) area.Min.Y; i2 < area.Max.Y; i2++)
                {
                    var node = ForeGround[i1, i2];
                    LightField[0][i1, i2] = (node.Incident + node.Emmision ) * node.Type.Color;
                    LightField[1][i1, i2] = (node.Emmision + node.Type.Emission);
                    //node.LightDirection.Normalize();
                }
            }
        }

        public void Update(Node node)
        {
            lightUp(node);
        }

        private void lightUp(Node node)
        {
            Vector3 sum = new Vector3();
            Vector3 eSum = new Vector3();
            float count = 1;
            float ecount = 1;
            sum += LightField[0][(int)(node.Postion.X), (int)(node.Postion.Y)];
            eSum += LightField[1][(int)(node.Postion.X), (int)(node.Postion.Y)];
            for (int i2 = 0; i2 < Utils.Rays.Length; i2++)
            {
                int x = (int)(node.Postion.X + Utils.Rays[i2].X);
                int y = (int)(node.Postion.Y + Utils.Rays[i2].Y);

                if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                {
                    Node hit = ForeGround[x, y];

                    if (hit.Type.Emission != Vector3.Zero  )
                    {
                        sum += LightField[0][x, y];

                    } else if ((hit.LType & LightType.Direct) == LightType.Direct)
                    {
                        sum += LightField[0][x, y] * MathHelper.Clamp(Vector3.Dot(hit.LightDirection, Utils.Rays[i2]), 0, 1);
                    }
                    else if (hit.Type.Opacity != 1 || node.Type.Opacity != 1)
                    {
                        sum += LightField[0][x, y];
                        eSum += LightField[1][x, y];
                    }

                    count += 1;
                }
                else
                {
                    //if (node.Type == NodeType.Air)
                    //    sum += Sky;
                    //count++;
                }
            }
            node.Incident = (sum / count);
            node.Emmision = eSum / count;
        }

        public void UpdateRelief(Node node)
        {
            int nodeX = (int)node.Postion.X;
            int nodeY = (int)node.Postion.Y;

            if (node.Type.Opacity > 0)
            {
                if (Relief[nodeX] < nodeY)
                    Relief[nodeX] = nodeY;
            }
            else
            {
                if (Relief[nodeX] == nodeY)
                {
                }
            }
            sun.Update(node);
            foreach (var light in lights)
            {
                light.Update(node);
            }
        }
    }

    public class WaterSimulator : IMapComponent
    {
        protected Node[,] ForeGround;
        private int X;
        private int Y;

        public Vector3 waterSource;

        Random rand = new Random();

        public void Init(Node[,] foreGround, int x, int y)
        {
            ForeGround = foreGround;
            X = x;
            Y = y;
            waterSource = new Vector3(50, 50, 0);
        }

        public void PreUpdate(BoundingBox area)
        {
            area.Contains(waterSource);

            ForeGround[(int)waterSource.X, (int)waterSource.Y].SetType(NodeFactory.Get(NodeTypes.Water));

        }

        public void Update(Node node)
        {
            if (node.Type.Type == NodeTypes.Water)
            {
                if (node.Updated > 0)
                    node.Updated--;
                else
                    UpdateWater(node);
            }
        }


        int[,] waterX = new[,] { { 0, -1, 1, -1, 1 }, { 0, 1, -1, 1, -1 } };
        int[] waterY = new[] { -1, -1, -1, 0, 0 };

        private void UpdateWater(Node node)
        {
            int r = rand.Next(3) % 2;
            for (int i = 0; i < 5; i++)
            {
                var downNode = ForeGround[(int)MathHelper.Clamp(node.Postion.X + waterX[r, i], 0, X - 1), (int)MathHelper.Clamp(node.Postion.Y + waterY[i], 0, Y - 1)];
                if (downNode.Type.Type != NodeTypes.Water && !downNode.Type.CanCollide)
                {
                    var temp = node.Type;

                    node.SetType(temp.OldNodeType);
                    downNode.SetType(temp);

                    downNode.Updated = 10;
                    return;
                }
            }
        }

    }

    public class TextureModifier : IMapComponent
    {
        protected Node[,] ForeGround;
        private int X;
        private int Y;
        public void Init(Node[,] foreGround, int x, int y)
        {
            ForeGround = foreGround;
            X = x;
            Y = y;

            for (int i1 = 0; i1 < X; i1++)
            {
                for (int i2 = Y - 1; i2 >= 0; i2--)
                {
                    var node = ForeGround[i1, i2];
                    node.TypeChanged += new Node.NodeEventHandler(UpdateNodeNeighboursTexture);
                    if (ForeGround[i1, i2].Type.Type == NodeTypes.Earth || ForeGround[i1, i2].Type.Type == NodeTypes.Water || ForeGround[i1, i2].Type.Type == NodeTypes.Soil)
                        UpdateNodeTexure(ForeGround[i1, i2]);
                }
            }
        }

        public void PreUpdate(BoundingBox area)
        {
        }

        public void Update(Node node)
        {
        }
        public void UpdateNodeNeighboursTexture(Node node)
        {
            int nodeX = (int)node.Postion.X;
            int nodeY = (int)node.Postion.Y;

            if (node.Type.Type == NodeTypes.Earth || node.Type.Type == NodeTypes.Water || node.Type.Type == NodeTypes.Soil)
                UpdateNodeTexure(node);

            for (int i = 0; i < Utils.Rays.Length; i += 2)
            {
                int x = (nodeX + (int)Utils.Rays[i].X);
                int y = (nodeY + (int)Utils.Rays[i].Y);

                if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                {
                    Node neighbour = ForeGround[x, y];
                    if (neighbour.Type.Type == NodeTypes.Earth || neighbour.Type.Type == NodeTypes.Water || neighbour.Type.Type == NodeTypes.Soil)
                        UpdateNodeTexure(neighbour);
                }
            }

            if (node.Type.Type == NodeTypes.Earth)
            {
                int i = 1; bool cont = true;
                do
                {
                    if (MyMath.IsBetween(nodeX, 0, X) && MyMath.IsBetween(nodeY - i, 0, Y))
                    {
                        var n2 = ForeGround[nodeX, nodeY - i];
                        if (n2.Type.CanRender == false )
                        {
                            n2.Type.CanRender = true;
                            i++;
                        }
                        else if (n2.Type.OldNodeType != null && n2.Type.OldNodeType.CanRender == false)
                        {
                            n2.Type.OldNodeType.CanRender = true;
                            i++;
                        }
                        else
                            cont = false;
                    }
                    else cont = false;
                } while (cont);
            }


        }

        readonly Vector2[] Rays2 = new Vector2[] { new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1) };
        public void UpdateNodeTexure(Node node)
        {
            int nodeX = (int)node.Postion.X;
            int nodeY = (int)node.Postion.Y;
            string texture = "";
            for (int i = 0; i < Rays2.Length; i++)
            {
                int x = (nodeX + (int)Rays2[i].X);
                int y = (nodeY + (int)Rays2[i].Y);
                if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                {
                    Node neighbour = ForeGround[x, y];
                    if (neighbour.Type.Type == node.Type.Type)
                        texture += "0";
                    else
                        texture += "1";
                }
                else
                    texture += "1";
            }

            node.Type.SetTexture(texture);
            if (node.Type.Type == NodeTypes.Soil)
            {
                if (texture == "0011")
                {
                    node.Type.ResolveCollision = NodeFactory.LeftSlope;
                    node.Type.Decals.Clear();
                }

                else if (texture == "1001")
                {
                    node.Type.ResolveCollision = NodeFactory.RightSlope;
                    node.Type.Decals.Clear();
                }
                else
                {
                    node.Type.ResolveCollision = NodeFactory.HardCollision;
                }




            }

        }
    }

    public class Clouds : IMapComponent
    {
        protected Node[,] ForeGround;
        private int X;
        private int Y;
        private Random rand;
        public void Init(Node[,] foreGround, int X, int Y)
        {
            ForeGround = foreGround;
            this.X = X;
            this.Y = Y;
            rand = new Random();
            for (int i1 = 0; i1 < X; i1++)
            for (int i2 = Y / 2; i2 < Y; i2++)
            {
                double x = (double)i1 / X;
                double y = (double)i2 / Y;
                int oct = 5;
                double noise = Math.Sin(Noise.NextOctave2D(oct, x, y) / oct - 0.2)*2;
                if (noise > 0.0f)
                {
                    NodeType cloud = NodeFactory.Get(NodeTypes.Cloud);
                    cloud.Opacity = (float)noise / 5;
                    //cloud.Opacity 
                    ForeGround[i1, i2].SetType(cloud);
                }
            }

        }

        public void PreUpdate(BoundingBox area)
        {

        }

        public void Update(Node node)
        {
            if (node.Type.Type == NodeTypes.Cloud)
            {
                if (node.Updated > 0)
                    node.Updated--;
                else
                    UpdateCloud(node);
            }
        }

        private void UpdateCloud(Node node)
        {

            var nextnode = ForeGround[(int)MathHelper.Clamp(node.Postion.X - 1, 0, X - 1), (int)node.Postion.Y];
            if (nextnode.Type.Type != NodeTypes.Cloud)
            {
                var temp = node.Type;

                node.SetType(temp.OldNodeType);
                nextnode.SetType(temp);

                nextnode.Updated = 20;
                return;
            }

        }
    }


}
