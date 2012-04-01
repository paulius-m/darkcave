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
        public Color Color;
        public Vector4 Light;
        public Vector3 Texture;

        private readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration
            (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3),
            new VertexElement(64, VertexElementFormat.Color, VertexElementUsage.Color, 1),
            new VertexElement(68, VertexElementFormat.Vector4, VertexElementUsage.Color, 2),
            new VertexElement(84, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1)
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
        public int TileCount = 16;
        public DynamicVertexBuffer instanceVertexBuffer;
        public Camera Camera;
        InstanceData[] instances;
        public int InstanceCount;

        public List<Instanced> Instances = new List<Instanced>();

        public RenderGroup(int bufferSize, string name, params Instanced[] group)
        {
            Name = name;
            this.instances = new InstanceData[bufferSize];
            Instances.AddRange(group);
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

        List<RenderTarget2D> zmaps;
        RenderTarget2D polar;

        RenderTarget2D color;
        RenderTarget2D opacity;
        RenderTarget2D ambience;

        RenderTarget2D opacityScaled;
        RenderTarget2D ambienceScaled;

        RenderTarget2D shadow;

        Camera planeCam;

        List<Entity> lights = new List<Entity>();

        BasicEffect be;


        public DynamicVertexBuffer instanceVertexBuffer;
        public Renderer()
        {
            planeCam = new Camera { View = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), Vector3.Up), Projection = Matrix.CreateOrthographic(1f, 1f, 1f, 100.0f)};

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
            opacity = new RenderTarget2D(Device, pp.BackBufferWidth, pp.BackBufferHeight, true, SurfaceFormat.Color, DepthFormat.None);
            ambience = new RenderTarget2D(Device, pp.BackBufferWidth, pp.BackBufferHeight, true, SurfaceFormat.Color, DepthFormat.None);

            opacityScaled = new RenderTarget2D(Device, pp.BackBufferWidth / 2, pp.BackBufferHeight / 2, true, SurfaceFormat.Color, DepthFormat.None);
            ambienceScaled = new RenderTarget2D(Device, pp.BackBufferWidth / 2, pp.BackBufferHeight / 2, true, SurfaceFormat.Color, DepthFormat.None);

            polar = new RenderTarget2D(Device, 1024, 512, true, SurfaceFormat.Rg32, DepthFormat.None);
            zmaps = new List<RenderTarget2D>();
            shadow = new RenderTarget2D(Device, pp.BackBufferWidth/2, pp.BackBufferHeight/2, true, SurfaceFormat.Color, DepthFormat.None); 
            //new RenderTarget2D(Device, pp.BackBufferWidth, pp.BackBufferHeight, true, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            be = new BasicEffect(Device);
        }

        public void AddLight(Entity source)
        {
            lights.Add(source);
            zmaps.Add(new RenderTarget2D(Device, 1024, 1, true, SurfaceFormat.Rg32, DepthFormat.None));
        }

        public void Draw(Camera cam)
        {
            render(model.Meshes[0].MeshParts[0], cam);
        }

        private void render(ModelMeshPart meshPart, Camera cam)
        {
            // Set up the instance rendering effect.
            Effect effect = meshPart.Effect;
            RenderInstances(meshPart, effect, cam);

            Device.SetVertexBuffers(
                new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                new VertexBufferBinding(instanceVertexBuffer, 0, 1)
            );
            
            
            
            effect.Parameters["View"].SetValue(planeCam.View);
            effect.Parameters["Projection"].SetValue(planeCam.Projection);
            effect.Parameters["Offset"].SetValue(planeCam.Offset);


            RenderShadows(meshPart, effect, cam);

            Device.SetRenderTarget(null);
            effect.Parameters["Texture"].SetValue(color);
            effect.Parameters["Ambient"].SetValue(ambience);
            effect.Parameters["Ambient2"].SetValue(opacity);
            effect.Parameters["Shadow"].SetValue(shadow);
            effect.CurrentTechnique = effect.Techniques["Final"];
            effect.CurrentTechnique.Passes[0].Apply();
            Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, 1);

            //using (System.IO.Stream s = System.IO.File.OpenWrite("D:\\sh.png"))
            //    shadow.SaveAsPng(s, shadow.Width, shadow.Height);

        }

        private Vector2 toScreenSpace(Vector3 r, Camera cam )
        {
            r = Vector3.Transform(r, cam.View);
            r = Vector3.Transform(r, cam.Projection);

            return new Vector2(r.X / 2 + .5f, -r.Y / 2 + 0.5f);
        }

        private void RenderInstances(ModelMeshPart meshPart, Effect effect, Camera cam)
        {
            effect.Parameters["View"].SetValue(planeCam.View);
            effect.Parameters["Projection"].SetValue(planeCam.Projection);
            effect.Parameters["Offset"].SetValue(planeCam.Offset);

            effect.Parameters["skycolor"].SetValue(Game1.Instance.gameWorld.SkyColor);
            effect.Parameters["downcolor"].SetValue(Game1.Instance.gameWorld.SunColor);
            Device.Indices = meshPart.IndexBuffer;

            Device.SetRenderTargets(color, opacity, ambience);
            Device.Clear(new Color(1f, 1f, 1f, 0f));

            effect.CurrentTechnique = effect.Techniques["Sky"];

            Device.SetVertexBuffers(
                new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                new VertexBufferBinding(instanceVertexBuffer, 0, 1)
            );

            effect.CurrentTechnique.Passes[0].Apply();

            Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, 1);

            effect.Parameters["View"].SetValue(cam.View);
            effect.Parameters["Projection"].SetValue(cam.Projection);
            effect.Parameters["Offset"].SetValue(cam.Offset);


            effect.CurrentTechnique = effect.Techniques["Color"];



            foreach (var group in Groups)
            {
                group.Update(cam);
                if (group.InstanceCount == 0)
                    continue;

                effect.Parameters["Texture"].SetValue(group.Texture);
                effect.Parameters["TileCount"].SetValue(group.TileCount);
                Device.SetVertexBuffers(
                    new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                    new VertexBufferBinding(group.instanceVertexBuffer, 0, 1)
                );

                effect.CurrentTechnique.Passes[0].Apply();

                Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, group.InstanceCount);
            }
        }


        private void RenderInstances2(ModelMeshPart meshPart, Effect effect, Camera cam)
        {
            effect.Parameters["View"].SetValue(cam.View);
            effect.Parameters["Projection"].SetValue(cam.Projection);

            Device.Indices = meshPart.IndexBuffer;
            effect.CurrentTechnique = effect.Techniques["Wire"];

            Device.Clear(Color.Black);

            foreach (var group in Groups)
            {
                group.Update(cam);
                if (group.InstanceCount == 0)
                    continue;

                effect.Parameters["Texture"].SetValue(group.Texture);
                effect.Parameters["TileCount"].SetValue(group.TileCount);
                Device.SetVertexBuffers(
                    new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                    new VertexBufferBinding(group.instanceVertexBuffer, 0, 1)
                );

                effect.CurrentTechnique.Passes[0].Apply();
                Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, group.InstanceCount);
            }
        }

        private void RenderShadows(ModelMeshPart meshPart, Effect effect, Camera cam)
        {
            for (int i = 0; i < lights.Count; i++)
            {
                Vector3 r = lights[i].Postion;

                if (Vector3.Distance(r, cam.Target) > cam.ViewSize.Length())
                    continue;

                Device.SetRenderTarget(zmaps[i]);
                effect.Parameters["Light"].SetValue(toScreenSpace(r, cam));
                effect.Parameters["Distance"].SetValue(0.5f);

                effect.Parameters["Shadow"].SetValue(opacity);

                effect.CurrentTechnique = effect.Techniques["ZMap"];
                effect.CurrentTechnique.Passes[1].Apply();
                Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, 1);
            }

            Device.SetRenderTarget(shadow);
            Device.Clear(Color.Black);
            
            for (int i = 0; i < lights.Count; i++)
            {
                Vector3 r = lights[i].Postion;
                if (Vector3.Distance(r, cam.Target) > cam.ViewSize.Length())
                    continue;

                effect.Parameters["Light"].SetValue(toScreenSpace(r, cam));
                effect.Parameters["Distance"].SetValue(0.5f);
                effect.Parameters["Shadow"].SetValue(zmaps[i]);
                effect.CurrentTechnique = effect.Techniques["ShadowAccum"];
                effect.CurrentTechnique.Passes[0].Apply();
                Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, 1);
            }
        }
    }
}
