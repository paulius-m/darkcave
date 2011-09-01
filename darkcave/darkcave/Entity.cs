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

    public class Entity : Instanced
    {

        public Vector3 Speed;
        public Vector3 Color = Vector3.One;
        public Vector3 Postion;
        public Vector3 Size = Vector3.One;
        public BoundingBox CollisionBox;
        public float MaxSpeedX = 0.05f;
        public float MaxSpeedY = 0.2f;
        public float Gravity = 0.01f;

        public bool InJump;

        protected AnimationSet Frames;
        public float RotationX;

        protected bool InAction;

        public LocalEnvironment Environment;
        public World World;
        protected InstanceData Instance = new InstanceData();

        public Entity()
        {
            Size.X *= 0.9f;
            Frames = SetTextures();
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

        protected virtual AnimationSet SetTextures()
        {
            return new AnimationSet
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
                    },
                    {"damage", new TransitionAnimation{
                        Texture = new Vector3 (1, 2, 0),
                        Position = new Vector3(1, 2, 0),  
                        End = EndDamage,
                        Count = 2, Delay = 20}
                    },
                }
            };
        }

        public void Move()
        {
            if (!InAction)
                ApplyControl();
            ApplyForces();
        }

        protected virtual void ApplyControl()
        {
            Keys[] keys = Keyboard.GetState().GetPressedKeys();

            for (int i = 0; i < keys.Length; i++)
            {
                switch (keys[i])
                {
                    case Keys.O:
                        Frames.SetActive("attack");
                        InAction = true;
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
                        //if (Speed.Y == 0)
                        Speed.Y = MaxSpeedY;
                        break;
                }
            }
        }

        protected virtual void ApplyForces()
        {
            Speed.Y -= Gravity;
        }

        void Instanced.GetInstanceData( Instancer instancer)
        {
            Instance.Color = Color;
            if (Environment.Node != null)
            {
                Instance.Light = Environment.Node.Incident + Environment.Node.Emmision;
                //Environment.Node.Ambience = Vector3.One;
            }

            Instance.Texture = Frames.Active.Texture;
            instancer.AddInstance(Instance);
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
            if (InAction == false)
            {
                if (Speed.X != 0)
                    Frames.SetActive("run");
                else
                    Frames.SetActive("idle");
            }
            SetPosition(Postion + Speed);
            Speed.X *= 0.5f;
            if (Math.Abs(Speed.X) < 0.001f)
                Speed.X = 0;
        }

        void Attack()
        {
            if (World != null)
                World.Damage(this, this.CollisionBox, 10);
        }

        void EndAttack()
        {
            InAction = false;
        }

        public void Damage()
        {
            Color = new Vector3(1,0,0);
            Frames.SetActive("damage");
            InAction = true;
        }

        private void EndDamage()
        {
            Color = Vector3.One;
            InAction = false;
        }
    }

    public class Enemy : Entity
    {

        
        Random r = new Random();
        int count = 0;
        bool goleft;
        protected override void  ApplyControl()
        {
            Frames.SetActive("attack");
            InAction = true;
            return;
            count++;
            if (count >= 30)
            {
                goleft = r.NextDouble() > 0.5f;
                count = 0;
            }

            if (goleft)
            {
                Speed.X -= MaxSpeedX;
                RotationX = MathHelper.Pi;
            }
            else
            {
                Speed.X += MaxSpeedX;
                RotationX = 0;
            }
        }

        protected override AnimationSet SetTextures()
        {
            return base.SetTextures();
        }


    }
}
