using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace darkcave
{
    public class LocalEnvironment
    {
        public Node Node;
    }

    public class Entity : Node, Instanced
    {

        public Vector3 Speed;

        public float MaxSpeedX = 0.05f;
        public float MaxSpeedY = 0.2f;
        public float Gravity = 0.01f;

        public bool InJump;

        public AnimationSet Frames;
        public float RotationX;

        private bool inAction;

        public LocalEnvironment Environment;

        public Entity()
        {
            Diffuse = Vector3.One;
            Size.X *= 0.9f;
            Frames = new AnimationSet
            {
                Frames = {
                    { "run", new Animation{ 
                        Texture = new Vector3(0, 1, 0),
                        Position = new Vector3(0, 1, 0),
                        Count = 8 , Delay =5}
                    },
                    { "attack", new TransitionAnimation{ 
                        Texture = new Vector3(0, 0, 0),
                        Position = new Vector3(0, 0, 0),
                        EventFrame = 3,
                        Event = Attack,
                        End = EndAttack,
                        Count = 6 , Delay = 6}
                    },
                    { "idle", new Animation{
                        Texture = new Vector3 (0, 0, 0),
                        Position = new Vector3(0, 0, 0),
                        Count = 1 }
                    }
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
                    case Keys.O:
                        Frames.SetActive("attack");
                        inAction = true;
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
            if (inAction)
                goto end;

            if (Speed.X != 0)
                Frames.SetActive("run");
            else
                Frames.SetActive("idle");
            end:
            Speed.Y -= Gravity;
        }

        void Attack()
        { 
        
        }

        void EndAttack()
        {
            inAction = false;
        }


        void Instanced.GetInstanceData( Instancer instancer)
        {
            Instance.Color = Vector3.One;
            if (Environment.Node != null)
            {
                Instance.Light = Environment.Node.Diffuse + Environment.Node.Ambience;
                Environment.Node.Ambience = Vector3.One;
            }
            else
                Instance.Light = Diffuse + Ambience;

            Instance.Texture = Frames.Active.Texture;
            instancer.AddInstance(Instance);
        }


        public new void SetPosition(Vector3 pos)
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
