using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace darkcave
{
    public class World : GameComponent
    {
        public delegate void Interaction();

        public List<Entity> Entities = new List<Entity>();
        public Map Map;
        private Interaction[] actions = new Interaction[10];
        private int count;


        public World()
            : base(Game1.Instance)
        {
            Game1.Instance.Components.Add(this);
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < count; i++)
            {
                actions[i]();
                actions[i] = null;
            }
            count = 0;

            for (int i = 0; i < Entities.Count; i++)
                Entities[i].Environment = Map.Describe(Entities[i].Postion);
            for (int i = 0; i < Entities.Count; i++)
                Entities[i].Move();
            for (int i = 0; i < Entities.Count; i++)
                Map.ResolveCollisions(Entities[i]);
            for (int i = 0; i < Entities.Count; i++)
                Entities[i].Update();

        }

        public void Damage(Entity sender, BoundingSphere area, float amount)
        {
            var receiver = getCollided(area, sender);
            if (receiver == null)
                return;
            actions[count++] = () => {ApplyDamage(receiver,amount, sender);} ;
        }

        private Entity getCollided(BoundingSphere area, Entity exept)
        {
            foreach (var entity in Entities)
            {
                if (entity == exept)
                    continue;

                if (entity.CollisionBox.Intersects(area))
                    return entity;
            }
            return null;
        }

        public void AddEntity(Entity ent)
        {
            Entities.Add(ent);
            ent.World = this;
        }

        private void ApplyDamage(Entity receiver, float amount, Entity sender)
        {
            receiver.Speed += Vector3.Normalize(receiver.Postion - sender.Postion);
            receiver.Damage();
        }
    }
}
