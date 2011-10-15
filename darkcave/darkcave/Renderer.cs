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
        void GetInstanceData(RenderGroup instancer);
    }

    public class RenderGroup
    {
        public string Name;
        public Texture2D Texture;
        public DynamicVertexBuffer instanceVertexBuffer;
        public Camera Camera;
        InstanceData[] instances;
        public int InstanceCount;

        public Instanced[] Instances;

        public RenderGroup(int bufferSize, string name, params Instanced[] group)
        {
            Name = name;
            this.instances = new InstanceData[bufferSize];
            Instances = group;
        }

        public void Load()
        {
            Texture = Game1.Instance.Content.Load<Texture2D>(Name);
        }

        public virtual void GetInstanceData()
        {
            Reset();
            foreach (var instance in Instances)
            {
                instance.GetInstanceData(this);
            }
        }

        public void AddInstance(Instanced instance)
        {
            instance.GetInstanceData(this);
        }

        public void AddInstance(InstanceData data)
        {
            instances[InstanceCount++] = data;
        }

        public void Reset()
        {
            InstanceCount = 0;
        }

        public void Update(Camera cam)
        {
            Camera = cam;
            GetInstanceData();
            if (InstanceCount == 0)
                return;

            if ((instanceVertexBuffer == null) || (InstanceCount > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(
                    Game1.Instance.GraphicsDevice,
                    typeof(InstanceData),
                    InstanceCount,
                    BufferUsage.WriteOnly);
            }

            instanceVertexBuffer.SetData(instances, 0, InstanceCount, SetDataOptions.Discard);
        }
    }

    public class Renderer
    {
        Model model;
        public List<RenderGroup> Groups = new List<RenderGroup>();
        protected GraphicsDevice Device;

        RenderTarget2D shadowmaptarget;
        RenderTarget2D zmap;
        RenderTarget2D polar;

        RenderTarget2D color;
        RenderTarget2D opacity;

        public DynamicVertexBuffer instanceVertexBuffer;
        public Renderer()
        {
            Device = Game1.Instance.GraphicsDevice;
            instanceVertexBuffer = new DynamicVertexBuffer(
                    Game1.Instance.GraphicsDevice,
                    typeof(InstanceData),
                    1,
                    BufferUsage.WriteOnly);

            instanceVertexBuffer.SetData( new []{new InstanceData { World = Matrix.Identity }}, 0, 1, SetDataOptions.Discard);
        }

        public void Load()
        {
            model = Game1.Instance.Content.Load<Model>("plane2");
            Effect ef = Game1.Instance.Content.Load<Effect>("InstancedModel");
            
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = ef.Clone();
                }
            }

            foreach (var group in Groups)
                group.Load();

            PresentationParameters pp = Device.PresentationParameters;
            color = new RenderTarget2D(Device, pp.BackBufferWidth, pp.BackBufferHeight, true, SurfaceFormat.Color, DepthFormat.None);
            opacity = new RenderTarget2D(Device, pp.BackBufferWidth, pp.BackBufferHeight, true, SurfaceFormat.Alpha8, DepthFormat.None);
            polar = new RenderTarget2D(Device, 1024, 1024, true, SurfaceFormat.Rg32, DepthFormat.None);
            zmap = new RenderTarget2D(Device, 1024, 1, true, SurfaceFormat.Rg32, DepthFormat.None);
        }

        public void Draw(Camera cam)
        {
            EffectPass pass;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {

                    // Set up the instance rendering effect.
                    Effect effect = meshPart.Effect;

                    effect.Parameters["World"].SetValue(Matrix.Identity);
                    effect.Parameters["View"].SetValue(cam.View);
                    effect.Parameters["Projection"].SetValue(cam.Projection);
                    effect.Parameters["Light"].SetValue(Game1.Instance.player.Postion);

                    Device.Indices = meshPart.IndexBuffer;
                    effect.CurrentTechnique = effect.Techniques["ShadowMapInstancing"];

                    Device.SetRenderTargets(color, opacity);
                    
                    foreach (var group in Groups)
                    {
                        group.Update(cam);
                        if (group.InstanceCount == 0)
                            continue;

                        effect.Parameters["Texture"].SetValue(group.Texture);

                        Device.SetVertexBuffers(
                            new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                            new VertexBufferBinding(group.instanceVertexBuffer, 0, 1)
                        );

                        pass = effect.CurrentTechnique.Passes[0];
                        pass.Apply();

                        Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, group.InstanceCount);
                    }

                    Device.SetRenderTarget(polar);
                    effect.CurrentTechnique = effect.Techniques["ZMap"];
                    effect.Parameters["Shadow"].SetValue(opacity);
                    effect.Parameters["Texture"].SetValue(color);

                    //shadowmaptarget.SaveAsPng(System.IO.File.OpenWrite("D:\\1.png"), 500, 500);

                    var position = new Vector3(0, 0, 10);
                    var target = new Vector3(0, 0, 0);
                    Matrix view = Matrix.CreateLookAt(position, target, Vector3.Up);
                    Matrix proj = Matrix.CreateOrthographic(1, 1, 1.0f, 1000.0f);

                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(proj);

                    Device.SetVertexBuffers(
                        new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                        new VertexBufferBinding(instanceVertexBuffer, 0, 1)
                    );
                    pass = effect.CurrentTechnique.Passes[0];
                    pass.Apply();
                    Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, 1);

                    //using( System.IO.Stream f = System.IO.File.Create("D:\\z.png"))
                    //    shadowmaptarget.SaveAsPng(f, shadowmaptarget.Width, shadowmaptarget.Height);
                    Device.SetRenderTarget(zmap);
                    effect.Parameters["Shadow"].SetValue(polar);
                    pass = effect.CurrentTechnique.Passes[1];
                    pass.Apply();
                    Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, 1);


                    Device.SetRenderTarget(null);

                    effect.Parameters["Shadow"].SetValue(zmap);
                    effect.CurrentTechnique = effect.Techniques["Final"];
                    pass = effect.CurrentTechnique.Passes[0];
                    pass.Apply();
                    Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, 1);
                }
            }
        }






    }
}
