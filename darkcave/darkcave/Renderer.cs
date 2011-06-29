using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace darkcave
{
    public struct InstanceData : IVertexType
    {
        public Matrix World;
        public Vector3 Color;
        public Vector3 Light;
        public Vector3 Texture;

        private readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration
            (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3),
            new VertexElement(64, VertexElementFormat.Vector3, VertexElementUsage.Color, 1),
            new VertexElement(76, VertexElementFormat.Vector3, VertexElementUsage.Color, 2),
            new VertexElement(88, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1)
            );

        public VertexDeclaration VertexDeclaration
        {
            get
            {
                return vertexDeclaration;
            }
        }
    }

    public interface Instanced
    {
        void GetInstanceData(Instancer instancer);
    }

    public class Instancer
    {
        DynamicVertexBuffer instanceVertexBuffer;
        
        InstanceData[] instances;
        int instanceCount;

        Model model;
        Texture2D atlas;
        public Instancer(int bufferSize)
        {
            instances = new InstanceData[bufferSize];
        }

        public void Load()
        {
            model = Game1.Instance.Content.Load<Model>("plane2");
            Effect ef = Game1.Instance.Content.Load<Effect>("InstancedModel");
            atlas = Game1.Instance.Content.Load<Texture2D>("atlas");
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = ef.Clone();
                }
            }
        }

        public void AddInstance(Instanced instance)
        {
            instance.GetInstanceData(this);
        }

        public void AddInstance(InstanceData data)
        {
            instances[instanceCount++] = data;
        }

        public void Reset()
        {
            instanceCount = 0;
        }

        public void Draw(Camera cam)
        {
            if (instanceCount == 0)
                return;

            if ((instanceVertexBuffer == null) || (instanceCount > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(Game1.Instance.GraphicsDevice, typeof(InstanceData),
                                                               instanceCount, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            instanceVertexBuffer.SetData(instances, 0, instanceCount, SetDataOptions.Discard);

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

                    effect.Parameters["World"].SetValue(Matrix.Identity);
                    effect.Parameters["View"].SetValue(cam.View);
                    effect.Parameters["Projection"].SetValue(cam.Projection);
                    effect.Parameters["Texture"].SetValue(atlas);
                    
                    // Draw all the instance copies in a single call.
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        Game1.Instance.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, instanceCount);
                    }
                }
            }
        }

    }
}
