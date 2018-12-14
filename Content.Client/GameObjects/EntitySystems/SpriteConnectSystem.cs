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

            //Performance: This could be spread over multiple updates, or parallelized.
            while (_dirtyEntities.Count > 0)
            {
                CalculateNewSprite(_dirtyEntities.Dequeue());
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

            // queue up the 4 ents adjacent to the entity that requested an update.
            AddValidEntities(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitX * snapSize));
            AddValidEntities(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitX * -snapSize));
            AddValidEntities(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitY * snapSize));
            AddValidEntities(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitY * -snapSize));
        }

        private void AddValidEntities(IEnumerable<IEntity> ents)
        {
            foreach (var entity in ents)
            {
                if(!entity.IsValid())
                    continue;

                if (entity.HasComponent<SpriteConnectCardinalComponent>() && entity.HasComponent<ISpriteComponent>())
                {
                    _dirtyEntities.Enqueue(entity);
                }
            }
        }

        private void CalculateNewSprite(IEntity entity)
        {
            if(!entity.IsValid() ||
               !entity.HasComponent<SpriteConnectCardinalComponent>() ||
               !entity.TryGetComponent(out ISpriteComponent spriteComp))
                return;

            var position = entity.Transform.LocalPosition;
            if (!position.IsValidLocation())
            {
                Logger.WarningS(nameof(SpriteConnectSystem), $"Entity position not valid! ent={entity}");
                return;
            }

            var worldPos = position.ToWorld().Position;
            var snapSize = position.Grid.SnapSize;

            var result = MatchingEntity(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitX * snapSize), spriteComp.BaseRSIPath);
            spriteComp.LayerSetVisible(1, result);
            result = MatchingEntity(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitX * -snapSize), spriteComp.BaseRSIPath);
            spriteComp.LayerSetVisible(3, result);
            result = MatchingEntity(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitY * snapSize), spriteComp.BaseRSIPath);
            spriteComp.LayerSetVisible(2, result);
            result = MatchingEntity(EntityManager.GetEntitiesAt(worldPos + Vector2.UnitY * -snapSize), spriteComp.BaseRSIPath);
            spriteComp.LayerSetVisible(4, result);
        }

        private static bool MatchingEntity(IEnumerable<IEntity> entities, string rsiPath)
        {
            foreach (var entity in entities)
            {
                if (entity.IsValid() && entity.HasComponent<SpriteConnectCardinalComponent>() && entity.TryGetComponent(out ISpriteComponent spriteComp))
                {
                    var otherGfxPath = spriteComp.BaseRSIPath;

                    if (otherGfxPath == rsiPath)
                        return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    ///     Event raised by a <see cref="SpriteConnectCardinalComponent"/> when it needs to be recalculated.
    /// </summary>
    public class SpriteConnectDirtyEvent : EntitySystemMessage { }
}
