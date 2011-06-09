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
        public int Delay = 10;
        public void Update()
        {
            step++;
            if (step < Delay)
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

        public Animation()
        {
            AnimationSystem.Instance.Add(this);
        }

        public void SetActive(string name)
        {
            Active = Frames[name];
        }

        internal void Update()
        {
            Active.Update();
        }
    }

    public class AnimationSystem : GameComponent
    {
        private List<Animation> Animations = new List<Animation>();
        public AnimationSystem()
            : base(Game1.Instance)
        {

        }

        private static AnimationSystem instance;
        public static AnimationSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AnimationSystem();
                    Game1.Instance.Components.Add(instance);
                }
                return instance;
            }
        }
        
        
        public override void Update(GameTime gameTime)
        {
            foreach (var anim in Animations)
                anim.Update();
        }

        public void Add(Animation anim )
        {
            Animations.Add(anim);
        }
    }


}
