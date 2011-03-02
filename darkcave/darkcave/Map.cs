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
        protected class Node
        {
            public bool Filled;
        }

        public Model model;
        public Vector3 Size;

        protected Node[,] Data;

        public Map()
        { 
        
        }

        public void Init()
        {
            Data = new Node[(int)Size.X, (int)Size.Y];
            Random r = new Random(0);

            for (int i1 = 0; i1 < Size.X; i1++)
            for (int i2= 0; i2< Size.Y; i2++)
            {
                Data[i1, i2] = new Node { Filled = (r.Next(i2/2) == 0) };
            }
        }

        public void Draw(Camera cam)
        {
            
            for (int m = 0; m < model.Meshes.Count; m++)
            {
                ModelMesh mesh = model.Meshes[m];

                foreach (BasicEffect e in mesh.Effects)
                {
                    e.View = cam.View;
                    e.Projection = cam.Projection;
                    e.DiffuseColor = new Vector3();
                    for (int i1 = 0; i1 < Size.X; i1++)
                    for (int i2 = 0; i2 < Size.Y; i2++)
                    {

                        if (Data[i1, i2].Filled == false)
                            continue;

                        Matrix world = Matrix.CreateTranslation(new Vector3(i1, i2, 0));

                        e.World = world;
                        mesh.Draw();
                    }
                }
            }
        }

    }
}
