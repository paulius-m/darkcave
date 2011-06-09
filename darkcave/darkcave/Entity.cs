using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace darkcave
{
    public class Entity : Node, Instanced
    {

        public Vector3 Speed;

        public float MaxSpeedX = 0.05f;
        public float MaxSpeedY = 0.2f;
        public float Gravity = 0.01f;

        public bool InJump;

        public Animation Frames;
        public float RotationX;


        public Entity()
        {
            Diffuse = Vector3.One;
            Size.X *= 0.9f;
            Frames = new Animation
            {
                Frames ={
                    { "run", new AnimationFrame{ Texture = new Vector3(0, 3, 0), Position = new Vector3(0, 3, 0), Count = 8 , Delay = 5} },
                    { "idle", new AnimationFrame{ Texture = new Vector3 (0, 2, 0), Position = new Vector3(0, 2, 0), Count = 1 } }
                }
            };
        }

        public BoundingBox FutureCollisionBox
        {
            get
            {
                var minV = new Vector3(Postion.X - Size.X, Postion.Y - Size.Y, 0);
                var maxV = new Vector3(Postion.X + Size.X, Postion.Y + Size.Y, 0);

                return new BoundingBox(minV, maxV);
            }
        
        }


        public void Move()
        {
            Keys[] keys = Keyboard.GetState().GetPressedKeys();

            for (int i = 0; i < keys.Length; i++)
            {
                switch (keys[i])
                {
                    case Keys.W:
                        //Speed.Y += MaxSpeed;
                        break;
                    case Keys.S:
                        //Speed.Y -= MaxSpeed;
                        break;
                    case Keys.A:
                        Speed.X -= MaxSpeedX;
                        RotationX = MathHelper.Pi;
                        break;
                    case Keys.D:
                        Speed.X += MaxSpeedX;
                        RotationX = 0;
                        break;
                    case Keys.Space:
                        if (Speed.Y == 0)
                            Speed.Y += MaxSpeedY;
                        break;
                }
            }
            if (Speed.X != 0)
                Frames.SetActive("run");
            else
                Frames.SetActive("idle");

            //Frames.Active.Update();

            Speed.Y -= Gravity;
        }

        InstanceData Instanced.GetInstanceData()
        {
            Instance.Color = Type.Color;
            Instance.Light = Diffuse + Ambience;
            Instance.Texture = Frames.Active.Texture;
            return Instance;
        }


        public void SetPosition(Vector3 pos)
        {
            Postion = pos;

            var minV = new Vector3(pos.X - Size.X / 2, pos.Y - Size.Y / 2, 0);
            var maxV = new Vector3(pos.X + Size.X / 2, pos.Y + Size.Y / 2, 0);

            CollisionBox = new BoundingBox(minV, maxV);
            Instance.World = Matrix.CreateRotationY(RotationX) * Matrix.CreateTranslation(pos);

        }

        public void Update()
        {
           SetPosition(Postion + Speed);

           Speed.X *= 0.5f;
           if (Math.Abs(Speed.X) < 0.001f)
               Speed.X = 0;
        }
    }
}
