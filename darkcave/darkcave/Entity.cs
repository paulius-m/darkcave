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

    public class Tool
    {
        public BoundingSphere CollisionSphere;
        public int Damage;
    }


    public class Entity : Instanced
    {
        public Vector3 Speed;
        public Vector3 Color = Vector3.One / 2;
        public Vector3 Postion;
        public Vector3 Scale = Vector3.One;
        public Vector3 Size = Vector3.One;
        public BoundingBox CollisionBox;
        public float MaxSpeedX = 0.1f;
        public float MaxSpeedY = 0.2f;
        public float Gravity = 0.01f;

        public bool InJump;

        public AnimationSet Frames;
        public int XDirection = 1;
        public float RotationX;

        protected bool InAction;

        public LocalEnvironment Environment;
        public World World;
        protected InstanceData Instance = new InstanceData();

        public interface IController
        {
            void Move(Entity ent);
        }

        public IController Control;
        public Tool Weapon;

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
            if (InAction)
                return;

            Control.Move(this);
            ApplyForces();
        }

        protected virtual void ApplyForces()
        {
            Speed.Y -= Gravity;
            Speed.X *= 0.5f;

            if (Math.Abs(Speed.X) < 0.001f)
                Speed.X = 0;
        }

        void Instanced.GetInstanceData(RenderGroup RenderGroup)
        {
            Instance.Color = Color;
            if (Environment.Node != null)
            {
                Instance.Light = Environment.Node.Incident + Environment.Node.Emmision;
            }

            Instance.Texture = Frames.Active.Texture;
            RenderGroup.AddInstance(Instance);
        }

        public void SetPosition(Vector3 pos)
        {
            Postion = pos;

            var minV = new Vector3(pos.X - Size.X / 2, pos.Y - Size.Y / 2, 0);
            var maxV = new Vector3(pos.X + Size.X / 2, pos.Y + Size.Y / 2, 0);

            CollisionBox = new BoundingBox(minV, maxV);
            Instance.World = Matrix.CreateRotationY(RotationX) * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(pos);
        }

        public void Update()
        {
            if (InAction)
                return;

            if (Speed.X != 0)
                Frames.SetActive("run");
            else
                Frames.SetActive("idle");

            SetPosition(Postion + Speed);
        }

        public void Attack()
        {
            Frames.SetActive("attack");
            InAction = true;

            Vector3 hitpoint = new Vector3(Postion.X + XDirection * Weapon.CollisionSphere.Center.X, Postion.Y, 0);

            if (World != null)
                World.Damage(this, new BoundingSphere(hitpoint, Weapon.CollisionSphere.Radius), Weapon.Damage);
        }

        public void EndAttack()
        {
            InAction = false;
        }

        public void Damage()
        {
            Color = new Vector3(1,0,0);
            Frames.SetActive("damage");
            InAction = true;
        }

        public void EndDamage()
        {
            Color = Vector3.One;
            InAction = false;
        }
    }

    public static class EntityFactory
    {
        public static Entity GetPlayer()
        {
            var e = new Entity { 
                Control = new KeyboardController(),
                Weapon = new Tool
                {
                    CollisionSphere = new BoundingSphere { Center = new Vector3(0.5f, 0, 0), Radius = .25f },
                    Damage = 10
                }
            };

            e.Frames = new AnimationSet
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
                            Event = e.Attack,
                            End = e.EndAttack,
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
                            End = e.EndDamage,
                            Count = 2, Delay = 20}
                        },
                    }
            };
            return e;
        }

        public static Entity GetWorm()
        {
            var e = new Entity
            {
                MaxSpeedX = 0.02f,
                Control = new AIController(),
                Weapon = new Tool {
                        CollisionSphere = new BoundingSphere { Center = Vector3.Zero, Radius = .5f },
                        Damage = 10
                    }
            };
            e.Frames = new AnimationSet
            {
                Frames = {
                    { "run", new Animation{ 
                        Texture = new Vector3(0, 4, 0),
                        Position = new Vector3(0, 4, 0),
                        Count = 4 , Delay = 5}
                    },
                    { "idle", new Animation{
                        Texture = new Vector3 (0, 4, 0),
                        Position = new Vector3(0, 4, 0),
                        Count = 1 }
                    },
                    {"damage", new TransitionAnimation{
                        Texture = new Vector3 (2, 4, 0),
                        Position = new Vector3(2, 4, 0),  
                        End = e.EndDamage,
                        Count = 1, Delay = 40}
                    },
                }
            };
            return e;
        }

        public class KeyboardController : Entity.IController
        {
            public void Move(Entity ent)
            {
                Keys[] keys = Keyboard.GetState().GetPressedKeys();

                for (int i = 0; i < keys.Length; i++)
                {
                    switch (keys[i])
                    {
                        case Keys.O:
                            ent.Attack();
                            break;
                        case Keys.S:
                            //Speed.Y -= MaxSpeed;
                            break;
                        case Keys.A:
                            ent.Speed.X -= ent.MaxSpeedX;
                            ent.RotationX = MathHelper.Pi;
                            ent.XDirection = -1;
                            break;
                        case Keys.D:
                            ent.Speed.X += ent.MaxSpeedX;
                            ent.RotationX = 0;
                            ent.XDirection = 1;
                            break;
                        case Keys.Space:
                            if (ent.Speed.Y == 0)
                                ent.Speed.Y = ent.MaxSpeedY;
                            break;
                    }
                }
            }
        }

        public class AIController : Entity.IController
        {
            Random r = new Random();
            int count = 0;
            bool goleft;

            public void Move(Entity ent)
            {
                //Frames.SetActive("attack");
                //InAction = true;
                return;
                count++;
                if (count >= 30)
                {
                    goleft = r.NextDouble() > 0.5f;
                    count = 0;
                }

                if (goleft)
                {
                    ent.Speed.X -= ent.MaxSpeedX;
                    ent.RotationX = MathHelper.Pi;
                    ent.XDirection = -1;
                }
                else
                {
                    ent.Speed.X += ent.MaxSpeedX;
                    ent.XDirection = 1;
                    ent.RotationX = 0;
                }
            }
        }

    }


}
