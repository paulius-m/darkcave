﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace darkcave
{
    public class Camera
    {
        public float AspectRatio;
        public Matrix Projection;
        public Matrix View;
        public Vector2 ViewSize;
        private Viewport view;

        private Vector3 position;
        public Vector3 Position
        {
            get {return position; }
            set {position = value; updateView(); }
        }
        
        private Vector3 target;
        public Vector3 Target
        {
            get { return target; }
            set { target = value; updateView(); }
        }

        public BoundingFrustum Frustrum;

        private void updateView()
        {
            View = Matrix.CreateLookAt(position, target, Vector3.Up);
            Frustrum = new BoundingFrustum(View * Projection);
        }

        public Camera()
        {
            int zoom = 32;
            GraphicsDeviceManager graphics = Game1.Instance.graphics;
            view = Game1.Instance.GraphicsDevice.Viewport;
            AspectRatio = graphics.GraphicsDevice.Viewport.Width * 1.0f / graphics.GraphicsDevice.Viewport.Height;
            ViewSize = new Vector2(graphics.GraphicsDevice.Viewport.Width / zoom, graphics.GraphicsDevice.Viewport.Height / zoom);
            Projection = Matrix.CreateOrthographic(ViewSize.X, ViewSize.Y, 1.0f, 1000.0f);

            position = new Vector3(50, 50,40);
            target = new Vector3(30, 30, 0);
            updateView();
        }

        public Ray Unproject(int mouseX, int mouseY)
        {
            Vector3 near = view.Unproject(new Vector3(mouseX, mouseY, 0), Projection, View, Matrix.Identity);
            Vector3 far = view.Unproject(new Vector3(mouseX, mouseY, 1), Projection, View, Matrix.Identity);
            return new Ray(near,Vector3.Normalize(far - near));
        }

    }
}
