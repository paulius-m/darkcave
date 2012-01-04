using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace darkcave
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager graphics;

        public Map map;
        Renderer render;
        RenderGroup itemgroup;
        RenderGroup entitygroup;
        Camera cam;
        public Entity player;
        public World gameWorld;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Instance = this;
            graphics.ToggleFullScreen();
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 1280;
        }

        public static Game1 Instance
        {
            get;
            private set;
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;

            map = new Map (new Vector3(400, 100, 0));
            cam = new Camera();
            player = EntityFactory.GetPlayer();
            player.Postion = new Vector3(70, 100, 0);
            
            gameWorld = new World();
            gameWorld.AddEntity(player);
            gameWorld.Map = map;

            render = new Renderer();
            render.Groups.Add(
                new RenderGroup(40000, "atlas", map)
                );
            render.Groups.Add(
                entitygroup = new RenderGroup(200, "char1", player )
                );

            render.Groups.Add(
                itemgroup = new RenderGroup(2000, "char1")
                );

            itemgroup.TileCount = 32;
            //enemy.SetType(new NodeType { Texture = new Vector3(0, 2, 0) });

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            map.Init();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            render.Load();
        }

        protected override void UnloadContent()
        {
        }

        NodeTypes newNodeType = NodeTypes.Fire;
        MouseState oldstate;
        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

            Vector3 point = getMapPoint(cam.Unproject(mouse.X, mouse.Y));
            cam.Position = new Vector3(player.Postion.X, player.Postion.Y, cam.Position.Z);
            Node node = map.GetNode((int)point.X, (int) point.Y );
            cam.Target = player.Postion;

            if (node != null)
            {
                if (mouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == oldstate.LeftButton)
                {
                    var type = NodeFactory.Get(newNodeType);
                    //type.ResolveCollision = NodeFactory.Slope;
                    //type.Texture = new Vector3(1f, 5f, 0);
                    node.SetType(type);

                }
                else if (mouse.RightButton == ButtonState.Pressed && mouse.RightButton == mouse.RightButton)
                {
                    newNodeType = node.Type.Type;

                }

                if (mouse.MiddleButton == ButtonState.Pressed && mouse.MiddleButton != oldstate.MiddleButton)
                {
                    map.SpawnLight(point);
                    render.AddLight(point);
                    var torch = EntityFactory.GetTorch();
                    torch.SetPosition(point);
                    gameWorld.AddEntity(torch);
                    itemgroup.Instances.Add(torch);
                }

                if (Keyboard.GetState().IsKeyDown(Keys.L))
                {
                    var ent = EntityFactory.GetWorm();

                    ent.Postion = point;

                    gameWorld.AddEntity(ent);
                    entitygroup.Instances.Add(ent);
                }


            }

            oldstate = mouse;
            map.Update(cam);
            base.Update(gameTime);
        }

        Vector3 getMapPoint(Ray mouseRay)
        {
            float t = -cam.Position.Z / mouseRay.Position.Z;

            return new Vector3(mouseRay.Position.X + t * mouseRay.Direction.X, mouseRay.Position.Y + t * mouseRay.Direction.Y, 0);
        }


        protected override void Draw(GameTime gameTime)
        {
            render.Draw(cam);
        }

        public void RemoveEntity(Entity entity)
        {
            foreach (var group in render.Groups)
            {
                group.Instances.Remove(entity);
            }
        }

    }
}
