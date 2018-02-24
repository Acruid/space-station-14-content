using Content.Server.GameObjects.EntitySystems;
using Content.Shared.GameObjects;
using SS14.Server.GameObjects;
using SS14.Shared.GameObjects;
using SS14.Shared.Interfaces.GameObjects;
using SS14.Shared.Maths;

namespace Content.Server.GameObjects
{
    public class ServerDoorComponent : SharedDoorComponent, IAttackHand
    {
        public bool Opened { get; private set; }

        private float OpenTimeCounter;

        private CollidableComponent collidableComponent;

        public override void Initialize()
        {
            base.Initialize();

            collidableComponent = Owner.GetComponent<CollidableComponent>();
        }

        public override void OnRemove()
        {
            collidableComponent = null;
        }

        public bool Attackhand(IEntity user)
        {
            if (Opened)
            {
                Close();
            }
            else
            {
                Open();
            }
            return true;
        }

        public override void HandleMessage(object owner, ComponentMessage message)
        {
            base.HandleMessage(owner, message);

            switch (message)
            {
                case BumpedEntMsg msg:
                    if (!Opened)
                        Open();
                    break;
            }
        }

        public void Open()
        {
            Opened = true;
            collidableComponent.IsHardCollidable = false;
        }

        public bool Close()
        {
            if (collidableComponent.TryCollision(Vector2.Zero))
            {
                // Do nothing, somebody's in the door.
                return false;
            }
            Opened = false;
            OpenTimeCounter = 0;
            collidableComponent.IsHardCollidable = true;
            return true;
        }

        public override ComponentState GetComponentState()
        {
            return new DoorComponentState(Opened);
        }

        private const float AUTO_CLOSE_DELAY = 5;

        public override void Update(float frameTime)
        {
            if (!Opened)
            {
                return;
            }

            OpenTimeCounter += frameTime;
            if (OpenTimeCounter > AUTO_CLOSE_DELAY)
            {
                if (!Close())
                {
                    // Try again in 2 seconds if it's jammed or something.
                    OpenTimeCounter -= 2;
                }
            }
        }
    }
}
