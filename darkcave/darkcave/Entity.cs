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

        public Vector3 FuturePosition;
        public float Speed = 0.2f;
        public Entity()
        {
            Diffuse = Vector3.One;
            Size = Size * 0.9f;
        }

        public BoundingBox FutureCollisionBox
        {
            get
            {
                var minV = new Vector3(FuturePosition.X - Size.X / 2, FuturePosition.Y - Size.Y / 2, 0);
                var maxV = new Vector3(FuturePosition.X + Size.X / 2, FuturePosition.Y + Size.Y / 2, 0);

                return new BoundingBox(minV, maxV);
            }
        
        }


        public void Update()
        {
            Keys[] keys = Keyboard.GetState().GetPressedKeys();

            for (int i = 0; i < keys.Length; i++)
            {
                switch (keys[i])
                {
                    case Keys.W:
                        FuturePosition.Y += Speed;
                        break;
                    case Keys.S:
                        FuturePosition.Y -= Speed;
                        break;
                    case Keys.A:
                        FuturePosition.X -= Speed;
                        break;
                    case Keys.D:
                        FuturePosition.X += Speed;
                        break;
                }
            }

            FuturePosition.Y -= 0.2f;
        }
        
        InstanceData Instanced.GetInstanceData()
        {
            SetPosition(FuturePosition);
            return base.GetInstanceData();
        }
    }
}
