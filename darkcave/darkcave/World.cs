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
        public List<Entity> Items = new List<Entity>();
        public Map Map;
        private Interaction[] actions = new Interaction[10];
        private int count;

        Vector4[] sky1 = new Vector4[] { new Vector4(0.4f, 0.6f, 0.9f, 1), new Vector4(0.4f, 0.6f, 0.9f, 1), new Vector4(0.1f, 0.2f, 0.5f, 1), new Vector4(0.1f, 0.1f, 0.1f, 1), new Vector4(0, 0.0f, 0.1f, 1), new Vector4(0.0f, 0.2f, 0.5f, 1) };
        Vector4[] sky2 = new Vector4[] { new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1), new Vector4(2, 0.5f, 0.5f, 1), new Vector4(0, 0.0f, 0.1f, 1), new Vector4(0, 0.0f, 0.1f, 1), new Vector4(1, 0.4f, 0.6f, 1), };

        public Vector4 SkyColor;
        public Vector4 SunColor;

        int pos1 = 0;
        int pos2 = 0;


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
                Console.Write(count);
            }
            count = 0;

            for (int i = Entities.Count - 1; i >= 0; i--)
                if (Entities[i].Alive == false)
                    RemoveEntity(Entities[i]);

            for (int i = 0; i < Entities.Count; i++)
                Entities[i].Environment = Map.Describe(Entities[i].Postion);
            for (int i = 0; i < Entities.Count; i++)
                Entities[i].Move();
            for (int i = 0; i < Entities.Count; i++)
                Map.ResolveCollisions(Entities[i]);
            for (int i = 0; i < Entities.Count; i++)
                Entities[i].Update();

            pos2++;
            const float maxpos2 = 1000;
            if (pos2 > maxpos2)
            {
                pos1 = (pos1 + 1) % sky1.Length;
                pos2 = 0;
            }

            SkyColor = sky1[pos1] * (1f - pos2 / maxpos2) + sky1[(pos1 + 1) % sky1.Length] * (pos2 / maxpos2);
            SunColor = sky2[pos1] * (1f - pos2 / maxpos2) + sky2[(pos1 + 1) % sky2.Length] * (pos2 / maxpos2);
        }

        public void Damage(Entity sender, BoundingSphere area, int amount)
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

        private void ApplyDamage(Entity receiver, int amount, Entity sender)
        {
            //receiver.Speed += Vector3.Normalize(receiver.Postion - sender.Postion);
            receiver.Damage(amount);
        }

        private void RemoveEntity(Entity ent)
        {
            Entities.Remove(ent);
            ((Game1)Game).RemoveEntity(ent);
        }

        public Entity CheckEntity(Vector3 start, Vector3 direction, Entity sender)
        {
            Ray ray = new Ray(start, direction);

            foreach (var ent in Entities)
            {
                if (ent == sender)
                    continue;

                if (ray.Intersects(ent.CollisionBox) != null)
                    return ent;
            }
            return null;
        }

        public Node CheckNode(Vector3 start, Vector3 direction)
        {
            return Map.Describe(start + direction).Node;
        }

    }
}
