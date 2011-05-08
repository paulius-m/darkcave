using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave
{
    public class AnimationFrame
    {
        public Vector3 Position;
        public int Count;
        public Vector3 Texture = Vector3.Zero;

        private int current;
        private int step;

        public void Update()
        {
            step++;
            if (step < 10)
                return;
            step = 0;
            
            Texture.X = (Position.X + current);
            Texture.Y = Position.Y;
            current++;
            if (current >= Count)
                current = 0;
        }

        public void Reset()
        {
            current = 0;
        }
    }

    public class Animation
    {
        public Dictionary<string, AnimationFrame> Frames = new Dictionary<string,AnimationFrame>();

        public AnimationFrame Active;

        public void SetActive(string name)
        {
            Active = Frames[name];
        }
    }
}
