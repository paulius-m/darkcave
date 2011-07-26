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

        public void Init(Node[,] foreGround, int x, int y)
        {
            ForeGround = foreGround;
            X = x;
            Y = y;

            Relief = new int[x];
            sun = new PointLight { Position = new Vector3(50, 90, 0) };
            sky = new SkyLight { Color = new Vector3(.4f, .5f, 1f) };

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

            sun.Update(ForeGround, Relief, X, Y, area);
            sky.Update(ForeGround, Relief, X, Y, area);

            foreach (var light in lights)
            {
                light.Update(ForeGround, Relief, X, Y, area);
            }
        }

        public void Update(Node node)
        {
            lightUp(node);
        }

        private void lightUp(Node node)
        {
            Vector3 sum = new Vector3();
            Vector3 sum2 = new Vector3();
            int count = 0, count2 = 0;

            for (int i2 = 0; i2 < Utils.Rays.Length; i2++)
            {
                int x = (int)(node.Postion.X + Utils.Rays[i2].X);
                int y = (int)(node.Postion.Y + Utils.Rays[i2].Y);

                if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                {
                    Node hit = ForeGround[x, y];
                    //if (hit.Type.Type != node.Type.Type)
                    {
                        sum += hit.GetLightColor();
                        sum2 += hit.GetAmbientColor();
                    }
                    count++;
                }
                else
                {
                    //if (node.Type == NodeType.Air)
                    //    sum += Sky;
                    //count++;
                }
            }
            if ((node.LType & LightType.Direct) != LightType.Direct)
                node.Diffuse = (sum / (float)(count == 0 ? 1 : count));
            if ((node.LType & LightType.Ambient) != LightType.Ambient)
                node.Ambience = (sum2 / (float)(count == 0 ? 1 : count));
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
        }
    }

    public class WaterSimulator : IMapComponent
    {
        protected Node[,] ForeGround;
        private int X;
        private int Y;

        Random rand = new Random();

        public void Init(Node[,] foreGround, int x, int y)
        {
            ForeGround = foreGround;
            X = x;
            Y = y;
        }

        public void PreUpdate(BoundingBox area)
        {
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
                if (downNode.Type.Type == NodeTypes.Air)
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
                    if (ForeGround[i1, i2].Type.Type == NodeTypes.Earth || ForeGround[i1, i2].Type.Type == NodeTypes.Water)
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

            if (node.Type.Type == NodeTypes.Earth || node.Type.Type == NodeTypes.Water)
                UpdateNodeTexure(node);

            for (int i = 0; i < Utils.Rays.Length; i += 2)
            {
                int x = (nodeX + (int)Utils.Rays[i].X);
                int y = (nodeY + (int)Utils.Rays[i].Y);

                if (MyMath.IsBetween(x, 0, X) && MyMath.IsBetween(y, 0, Y))
                {
                    Node neighbour = ForeGround[x, y];
                    if (neighbour.Type.Type == NodeTypes.Earth || neighbour.Type.Type == NodeTypes.Water)
                        UpdateNodeTexure(neighbour);
                }
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
        }
    }
}
