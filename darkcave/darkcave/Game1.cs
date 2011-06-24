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
            IsMouseVisible = true;
            render = new Instancer(200010);
            map = new Map (new Vector3(400, 100, 0));
            cam = new Camera();
            player = new Entity();
            player.Postion = new Vector3(70, 100, 0);
            player.SetType(new  NodeType {Texture = new Vector3(0, 2, 0)});

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

        NodeTypes newNodeType = NodeTypes.Water;

        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

            Vector3 point = getMapPoint(cam.Unproject(mouse.X, mouse.Y));
            map.sun.Position = cam.Position = new Vector3(player.Postion.X, player.Postion.Y, cam.Position.Z);
            Node node = map.GetNode((int)point.X, (int) point.Y );

            cam.Target = player.Postion;

            if (node != null)
            {
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    node.SetType(NodeFactory.Get(newNodeType));
                    node.Ambience = Vector3.Zero;
                }
                else if (mouse.RightButton == ButtonState.Pressed)
                {
                    newNodeType = node.Type.Type;
                }
            }
            render.Reset();

            map.Update(cam);
            player.Move();
            map.ResolveCollisions(player);

            player.Update();

            base.Update(gameTime);
        }



        Vector3 getMapPoint(Ray mouseRay)
        {
            float t = -cam.Position.Z / mouseRay.Position.Z;

            return new Vector3(mouseRay.Position.X + t * mouseRay.Direction.X, mouseRay.Position.Y + t * mouseRay.Direction.Y, 0);
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
