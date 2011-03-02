using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave
{
    class Camera
    {
        public float AspectRatio;
        public Matrix Projection;
        public Matrix View;



        public Camera()
        {
            GraphicsDeviceManager graphics = Game1.Instance.graphics;
            AspectRatio = graphics.GraphicsDevice.Viewport.Width * 1.0f / graphics.GraphicsDevice.Viewport.Height;

            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), AspectRatio, 1.0f, 1000.0f);

            View = Matrix.CreateLookAt(new Vector3(15, 15, 60), new Vector3(15, 15, 0), Vector3.Up);
        }

        public void Update()
        {

        }


    }
}
