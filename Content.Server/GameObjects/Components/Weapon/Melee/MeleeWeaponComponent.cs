using SS14.Shared.GameObjects;
using Content.Server.GameObjects.EntitySystems;
using SS14.Shared.Interfaces.GameObjects;
using SS14.Shared.Map;
using SS14.Shared.IoC;
using SS14.Shared.Maths;
using SS14.Server.Interfaces.GameObjects;
using SS14.Shared.Serialization;
using SS14.Shared.Interfaces.GameObjects.Components;
using Content.Shared.GameObjects;
using SS14.Shared.Interfaces.Map;

namespace Content.Server.GameObjects.Components.Weapon.Melee
{
    public class MeleeWeaponComponent : Component, IAfterAttack
    {
        public override string Name => "MeleeWeapon";

        public int Damage = 1;
        public float Range = 1;
        public float ArcWidth = 90;

        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);

            serializer.DataField(ref Damage, "damage", 5);
            serializer.DataField(ref Range, "range", 1);
            serializer.DataField(ref ArcWidth, "arcwidth", 90);
        }

        void IAfterAttack.Afterattack(IEntity user, GridCoordinates clicklocation, IEntity attacked)
        {
            var mapManager = IoCManager.Resolve<IMapManager>();
            var gridPosition = user.GetComponent<ITransformComponent>().GridPosition;
            var angle = new Angle(clicklocation.ToWorld(mapManager).Position - gridPosition.ToWorld(mapManager).Position);
            var entities = IoCManager.Resolve<IServerEntityManager>().GetEntitiesInArc(gridPosition, Range, angle, ArcWidth);

            foreach (var entity in entities)
            {
                if (!entity.GetComponent<ITransformComponent>().IsMapTransform || entity == user)
                    continue;

                if (entity.TryGetComponent(out DamageableComponent damagecomponent))
                {
                    damagecomponent.TakeDamage(DamageType.Brute, Damage);
                }
            }
        }
    }
}
