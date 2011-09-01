using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave
{
    public interface IDecal : Instanced
    {
        void Init(Node node);
    }

    public class Decal : IDecal
    {
        public Vector3 RelativePosition;
        public Vector3 Color;

        protected InstanceData Instance;
        protected Node Node;

        public virtual Vector3 Texture
        {
            get;
            set;
        }
        public Decal()
        {
            Instance = new InstanceData();
        }

        public void Init(Node node)
        {
            Node = node;
            SetPosition(node.Postion + RelativePosition);
        }

        public void SetPosition(Vector3 pos)
        {
            Instance.World = Matrix.CreateTranslation(pos);
        }

        public void GetInstanceData(Instancer instancer)
        {
            Instance.Color = Color;
            Instance.Light = Node.Incident + Node.Emmision;
            Instance.Texture = Texture;
            instancer.AddInstance(Instance);
        }
    }

    public class AnimatedDecal : Decal
    {
        public override Vector3 Texture
        {
            get
            {
                return Animation.Active.Texture;
            }
            set
            {
            }
        }

        public AnimationSet Animation;
    }


    public enum DecalType
    { 
        Grass,
        Water
    }


    public static class DecalFactory
    {
        private static AnimationSet GrassAnimation = new AnimationSet
        {
            Active = new Animation() { Position = new Vector3(9, 0, 0), Texture = new Vector3(9, 0, 0), Count = 4 }
        };
        public static IDecal Get(DecalType type)
        {
            IDecal dec = null;
            switch (type)
            { 
                case DecalType.Grass:
                    dec = new AnimatedDecal
                        {
                            Animation = GrassAnimation,
                            Color = new Vector3(.2f, 0.5f, 0.0f),
                            RelativePosition = new Vector3(0, .5f, 0),
                        };
                break;
            
            }
            return dec;
        }
    }


}
