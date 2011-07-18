using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave
{
    public class Animation
    {
        public Vector3 Position;
        public int Count;
        public Vector3 Texture = Vector3.Zero;

        protected int Current;
        private int step;

        public int Delay = 10;

        public void Update()
        {
            if (Wait())
                return;

            Texture.X = (Position.X + Current);
            Texture.Y = Position.Y;
            NextFrame();

            if (EndFrame())
                Reset();
        }

        protected virtual bool EndFrame()
        {
            return Current >= Count;
        }

        protected bool Wait()
        {
            step++;
            if (step < Delay)
                return true;
            step = 0;
            return false;
        }

        protected virtual void NextFrame()
        {
            Current++;
        }


        public virtual void Reset()
        {
            Current = 0;
        }
    }

    public class TransitionAnimation : Animation
    {
        public delegate void AnimationEvent();
        public AnimationEvent Event;
        public AnimationEvent End;
        public int EventFrame;

        protected override void NextFrame()
        {
            if (Current == EventFrame && Event != null)
                Event();
            base.NextFrame();
        }

        public override void Reset()
        {
            base.Reset();
            End();
        }

    }

    public class AnimationSet
    {
        public Dictionary<string, Animation> Frames = new Dictionary<string, Animation>();

        public Animation Active;

        public AnimationSet()
        {
            AnimationSystem.Instance.Add(this);
        }

        protected AnimationSet(bool none)
        { 
        
        }

        private string active;
        public string ActiveAnimation
        {
            get { return active; }
            set {  SetActive(value); }
        }

        public void SetActive(string name)
        {
            Active = Frames[name];
            active = name;
        }

        internal virtual void Update()
        {
            Active.Update();
        }
    }


    public class ActiveAnimationSet : AnimationSet
    {
        public ActiveAnimationSet()
        {

        }

        internal override void Update()
        {
            foreach (var frame in Frames.Values)
                frame.Update();
        }
    }

    public class PassiveAnimationSet : AnimationSet
    {
        public PassiveAnimationSet() : base (false)
        {

        }

        internal override void Update()
        {

        }
    }


    public class AnimationSystem : GameComponent
    {
        private List<AnimationSet> Animations = new List<AnimationSet>();
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

        public void Add(AnimationSet anim )
        {
            Animations.Add(anim);
        }
    }


}
