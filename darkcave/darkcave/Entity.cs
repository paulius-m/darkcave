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

        public Entity()
        {
            Diffuse = Vector3.One;
        }
        
        
        public void Update()
        {
            Keys[] keys = Keyboard.GetState().GetPressedKeys();

            for (int i = 0; i < keys.Length; i++)
            {
                switch (keys[i])
                {
                    case Keys.W:
                        FuturePosition.Y++;
                        break;
                    case Keys.S:
                        FuturePosition.Y--;
                        break;
                    case Keys.A:
                        FuturePosition.X--;
                        break;
                    case Keys.D:
                        FuturePosition.X++;
                        break;
                }
            }
        }
        
        InstanceData Instanced.GetInstanceData()
        {
            SetPosition(FuturePosition);
            return base.GetInstanceData();
        }
    }
}
