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
        public enum NodeType
        { 
            Air = 0,
            Earth,
            Water,
            Fire
        }

        public enum LightType
        { 
            None = 0,
            Ambient,
            Direct
        }

        public class Node
        {
            public NodeType Type;
            public double Value;
            public Vector3 Postion;
            public Vector3 Color = Vector3.One;
            public Vector3 Light;
            public LightType LType;
        }

        public Model model;
        public Vector3 Size;
        public Vector3 sun;


        public Node[,] Data;
        protected int curLight;

        protected VertexData[] ToDrawArray;

        Matrix[] instancedModelBones;

        public Map()
        {

        }

        public void Init()
        {
            Data = new Node[(int)Size.X, (int)Size.Y];
            Random r = new Random(0);
            ToDrawArray = new VertexData[Data.Length];
            curLight = 0;

            sun = new Vector3(50, 70, 0);

            for (int i1 = 0; i1 < Size.X; i1++)
            for (int i2= 0; i2< Size.Y; i2++)
            {
                double x = i1 / Size.X;
                double y = i2 / Size.Y;
                double noise = Noise.NextOctave2D(10, x, y) + y;

                var node = new Node {
                    Postion = new Vector3(i1, i2, 0),
                    Value = noise,
                    
                    Type = (noise < .5)?NodeType.Earth: NodeType.Air
                };

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

                if (MyMath.IsBetween(x, 0, Size.X) && MyMath.IsBetween(y, 0, Size.Y))
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
                    //count++;
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

                    if (MyMath.IsBetween(x, 0, Size.X) && MyMath.IsBetween(y, 0, Size.Y))
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

            if (MyMath.IsBetween(x, 0, Size.X) && MyMath.IsBetween(y, 0, Size.Y))
            {
                return Data[x, y];

            }

            return null;
        }

        private void ProprocesDraw()
        {
            for (int i1 = 0; i1 < Size.X; i1++)
            {
                for (int i2 = (int)Size.Y - 1; i2 >= 0; i2--)
                {
                    var node = Data[i1, i2];
                    node.Light = new Vector3(.4f, .6f, 1);

                    if (node.Type != NodeType.Air)
                        goto next;
                }
            next:
                continue;
            }
            
            for (int i1 = 0; i1 < Size.X; i1++)
                for (int i2 = 0; i2 < Size.Y; i2++)
                {
                    var node = Data[i1, i2];
                    //if (node.Type == NodeType.Empty)
                    //    continue;

                    var world = Matrix.CreateTranslation(node.Type == NodeType.Air ? new Vector3(node.Postion.X, node.Postion.Y, -1) : node.Postion);

                    ToDrawArray[i1 * (int)Size.X + i2] = new VertexData { World = world, Color = node.Color };
                }
        }

        public void Load()
        {
            model = Game1.Instance.Content.Load<Model>("box");
            Effect ef = Game1.Instance.Content.Load<Effect>("InstancedModel");

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = ef.Clone();
                }
            }
            instancedModelBones = new Matrix[model.Bones.Count];
            instancedModelBones[0] = Matrix.Identity;
        }

        public void Update()
        {
            directLights();
            for (int i1 = 0; i1 < Size.X; i1++)
            for (int i2 = 0; i2 < Size.Y; i2++)
            {
                var node = Data[i1, i2];

                node.Light = lightUp(node);
                var drawData = ToDrawArray[i1 * (int)Size.X + i2];

                drawData.Light = node.Light;
                drawData.Color = node.Color;
                ToDrawArray[i1 * (int)Size.X + i2] = drawData;
            }
        }

        public void Draw(Camera cam)
        {
            DrawModelHardwareInstancing(model, instancedModelBones, ToDrawArray, cam.View, cam.Projection);
        }

        public struct VertexData : IVertexType
        {
            public Matrix World;
            public Vector3 Color;
            public Vector3 Light;

            public readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration
                (
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
                new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
                new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
                new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3),
                new VertexElement(64, VertexElementFormat.Vector3, VertexElementUsage.Color, 0),
                new VertexElement(76, VertexElementFormat.Vector3, VertexElementUsage.Color, 1));

            public VertexDeclaration VertexDeclaration
            {
                get
                {
                    return vertexDeclaration;

                }
            }
        }

        DynamicVertexBuffer instanceVertexBuffer;

        void DrawModelHardwareInstancing(Model model, Matrix[] modelBones, VertexData[] instances, Matrix view, Matrix projection)
        {
            if (instances.Length == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((instanceVertexBuffer == null) || (instances.Length > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(Game1.Instance.GraphicsDevice, typeof(VertexData),
                                                               instances.Length, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            instanceVertexBuffer.SetData(instances, 0, instances.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                    Game1.Instance.GraphicsDevice.SetVertexBuffers(
                        new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                        new VertexBufferBinding(instanceVertexBuffer, 0, 1)
                    );

                    Game1.Instance.GraphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the instance rendering effect.
                    Effect effect = meshPart.Effect;

                    effect.CurrentTechnique = effect.Techniques["HardwareInstancing"];

                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index]);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);

                    // Draw all the instance copies in a single call.
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        Game1.Instance.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, instances.Length);
                    }
                }
            }
        }
    }
}
