using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace darkcave
{
    class Entity : Node, Instanced
    {

        public Vector3 Speed;

        public float MaxSpeed = 0.1f;
        public float Gravity = 0.05f;

        public bool InJump;


        public Entity()
        {
            Diffuse = Vector3.One;
            Size.X *= 0.9f;
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
                        Speed.Y += MaxSpeed;
                        break;
                    case Keys.S:
                        Speed.Y -= MaxSpeed;
                        break;
                    case Keys.A:
                        Speed.X -= MaxSpeed;
                        break;
                    case Keys.D:
                        Speed.X += MaxSpeed;
                        break;
                    case Keys.Space:
                        Speed.Y += MaxSpeed * 2;
                        break;
                }
            }
            Speed.Y -= Gravity;
        }

        InstanceData Instanced.GetInstanceData()
        {

            return base.GetInstanceData();
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
