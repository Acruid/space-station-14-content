using SS14.Shared.GameObjects;

namespace Content.Client.GameObjects
{
    public class SpriteConnectCardinalComponent : Component
    {
        public override string Name => "SpriteConnectedCardinal";

        /// <inheritdoc />
        public override void Startup()
        {
            base.Startup();

            Owner.EntityManager.RaiseEvent(Owner, new SpriteConnectDirtyEvent());
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            Owner.EntityManager.RaiseEvent(Owner, new SpriteConnectDirtyEvent());

            base.Shutdown();
        }
    }
}
