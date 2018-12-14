using System.Collections.Generic;
using SS14.Client.Interfaces.GameObjects.Components;
using SS14.Shared.GameObjects;
using SS14.Shared.GameObjects.Systems;
using SS14.Shared.Interfaces.GameObjects;
using SS14.Shared.Log;
using SS14.Shared.Maths;

namespace Content.Client.GameObjects
{
    internal class SpriteConnectSystem : EntitySystem
    {
        private readonly Queue<IEntity> _dirtyEntities = new Queue<IEntity>();

        /// <inheritdoc />
        public override void SubscribeEvents()
        {
            base.SubscribeEvents();

            SubscribeEvent<SpriteConnectDirtyEvent>(HandleSpriteDirty);
        }

        /// <inheritdoc />
        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var entity in _dirtyEntities)
            {
                CalculateNewSprite(entity);
            }
        }

        private void HandleSpriteDirty(object sender, EntitySystemMessage ev)
        {
            if (!(sender is IEntity senderEnt) || !senderEnt.IsValid())
                return;

            if (!senderEnt.HasComponent<SpriteConnectCardinalComponent>())
                return;

            if (!senderEnt.HasComponent<ISpriteComponent>())
                return;

            _dirtyEntities.Enqueue(senderEnt);

            var position = senderEnt.Transform.LocalPosition;
            if (!position.IsValidLocation())
            {
                Logger.WarningS(nameof(SpriteConnectSystem), $"Entity position not valid! ent={senderEnt}");
                return;
            }

            var worldPos = position.ToWorld().Position;
            var snapSize = position.Grid.SnapSize;

            AddValidEntities(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitX * snapSize));
            AddValidEntities(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitX * -snapSize));
            AddValidEntities(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitY * snapSize));
            AddValidEntities(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitY * -snapSize));
            
        }

        private void AddValidEntities(IEnumerable<IEntity> ents)
        {
            foreach (var entity in ents)
            {
                if (entity.HasComponent<SpriteConnectCardinalComponent>() && entity.HasComponent<ISpriteComponent>())
                {
                    _dirtyEntities.Enqueue(entity);
                }
            }
        }

        private void CalculateNewSprite(IEntity entity)
        {
            //TODO: Implement me!
        }
    }

    /// <summary>
    ///     Event raised by a <see cref="SpriteConnectCardinalComponent"/> when it needs to be recalculated.
    /// </summary>
    public class SpriteConnectDirtyEvent : EntitySystemMessage { }
}
