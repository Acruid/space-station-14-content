﻿using Robust.Client.Interfaces.GameObjects.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.GameObjects.Components.Transform;

namespace Content.Client.GameObjects.Components
{
    /// <summary>
    /// Simple component that automatically hides the sibling
    /// <see cref="ISpriteComponent" /> when the tile it's on is not a sub floor
    /// (plating).
    /// </summary>
    /// <seealso cref="P:Content.Shared.Maps.ContentTileDefinition.IsSubFloor" />
    [RegisterComponent]
    public sealed class SubFloorHideComponent : Component
    {
        private SnapGridComponent _snapGridComponent;

        /// <inheritdoc />
        public override string Name => "SubFloorHide";

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            _snapGridComponent = Owner.GetComponent<SnapGridComponent>();
        }

        /// <inheritdoc />
        protected override void Startup()
        {
            base.Startup();

            _snapGridComponent.OnPositionChanged += SnapGridOnPositionChanged;
            EntityEventArgs toRaise = new SubFloorHideDirtyEvent();
            Owner.EntityManager.EventBus.RaiseEvent((object) Owner, toRaise);
        }

        /// <inheritdoc />
        protected override void Shutdown()
        {
            base.Shutdown();

            _snapGridComponent.OnPositionChanged -= SnapGridOnPositionChanged;
            EntityEventArgs toRaise = new SubFloorHideDirtyEvent();
            Owner.EntityManager.EventBus.RaiseEvent((object) Owner, toRaise);
        }

        private void SnapGridOnPositionChanged()
        {
            EntityEventArgs toRaise = new SubFloorHideDirtyEvent();
            Owner.EntityManager.EventBus.RaiseEvent((object) Owner, toRaise);
        }
    }

    internal sealed class SubFloorHideDirtyEvent : EntitySystemMessage { }
}
