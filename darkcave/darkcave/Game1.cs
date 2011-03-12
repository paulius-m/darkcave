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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Map map;
        Instancer render;
        Camera cam;
        Entity player;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Instance = this;

        }

        public static Game1 Instance
        {
            get;
            private set;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            IsMouseVisible = true;
            render = new Instancer(200*100);
            map = new Map (new Vector3(200, 100, 0));
            cam = new Camera();
            player = new Entity();
            player.FuturePosition = new Vector3(70, 70, 0);
            map.Init();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            render.Load();
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

            Vector3 point = getMapPoint(cam.Unproject(mouse.X, mouse.Y));

            Node node = map.GetNode((int)point.X, (int) point.Y );

            cam.Position = new Vector3(player.Postion.X, player.Postion.Y, cam.Position.Z);
            map.sun = cam.Target = player.Postion;

            if (node != null)
            {

                if (node.Type == NodeType.Air)
                    node.Diffuse = new Vector3(1,0,0);

                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    node.SetType(NodeType.Air);
                    node.Ambience = Vector3.Zero;
                }

            }
            render.Reset();

            player.Update();
            map.Update();

            node = map.GetNode((int)player.FuturePosition.X, (int)player.FuturePosition.Y);

            //if (node != null && node.Type == NodeType.Earth)
            //    player.FuturePosition = player.Postion;

            base.Update(gameTime);
        }

        Vector3 getMapPoint(Vector3 mouseRay)
        {
            float t = -cam.Position.Z / mouseRay.Z;

            return new Vector3(cam.Position.X + t * mouseRay.X, cam.Position.Y + t * mouseRay.Y, 0);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            map.AddToDraw(cam, render);
            render.AddInstance(player);
            
            render.Draw(cam);
        }
    }
}
