using System;
using Content.Server.GameObjects.Components.Stack;
using Content.Server.GameObjects.EntitySystems;
using Content.Server.Interfaces.GameObjects;
using Content.Shared.GameObjects;
using Content.Shared.GameObjects.Components.Items;
using Content.Shared.GameObjects.Components.Materials;
using Robust.Server.GameObjects;
using Robust.Server.Interfaces.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Interfaces.Random;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Maths;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Content.Server.GameObjects
{
    [RegisterComponent]
    [ComponentReference(typeof(StoreableComponent))]
    public class ItemComponent : StoreableComponent, IAttackHand, IAttackBy, IExamine
    {
        public override string Name => "Item";
        public override uint? NetID => ContentNetIDs.ITEM;
        public override Type StateType => typeof(ItemComponentState);

        #pragma warning disable 649
        [Dependency] private readonly IRobustRandom _robustRandom;
#pragma warning restore 649

        private int _stackCount = 1;
        private int _stackMax = 1;

        private string _equippedPrefix;

        [ViewVariables(VVAccess.ReadWrite)]
        public string EquippedPrefix
        {
            get
            {
                return _equippedPrefix;
            }
            set
            {
                Dirty();
                _equippedPrefix = value;
            }
        }

        [ViewVariables(VVAccess.ReadWrite)]
        public int StackCount
        {
            get => _stackCount;
            set
            {
                if(_stackCount == value)
                    return;

                Dirty();

                _stackCount = value;

                if(value < 1)
                    Owner.Delete();
            }
        }

        [ViewVariables(VVAccess.ReadWrite)]
        public int StackMax
        {
            get => _stackMax;
            set => _stackMax = value;
        }

        public int StackAvailableSpace => _stackMax - _stackCount;

        protected override void Startup()
        {
            base.Startup();

            //TODO: Compatibility - Copy values from obsolete StackComponent
            if (Owner.TryGetComponent<StackComponent>(out var stackComp))
            {
                _stackCount = stackComp.Count;
                _stackMax = stackComp.MaxCount;
            }

        }

        public void Add(int amount)
        {
            StackCount += amount;
        }

        /// <summary>
        ///     Try to use an amount of items on this stack. If the stack is reduced to 0, then the item will be deleted.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>True if there were enough items to remove, false if not in which case nothing was changed.</returns>
        public bool Use(int amount)
        {
            if (StackCount >= amount)
            {
                StackCount -= amount;
                return true;
            }
            return false;
        }

        public void RemovedFromSlot()
        {
            foreach (var component in Owner.GetAllComponents<ISpriteRenderableComponent>())
            {
                component.Visible = true;
            }
        }

        public void EquippedToSlot()
        {
            foreach (var component in Owner.GetAllComponents<ISpriteRenderableComponent>())
            {
                component.Visible = false;
            }
        }

        public bool AttackBy(AttackByEventArgs eventArgs)
        {
            if (eventArgs.AttackWith.TryGetComponent<ItemComponent>(out var itemComp)
            && eventArgs.AttackWith.TryGetComponent<MaterialComponent>(out var theirMaterialComp)
            && Owner.TryGetComponent<MaterialComponent>(out var myMaterialComp))
            {
                if (myMaterialComp.MaterialTypes[MaterialKeys.Stack].Name != theirMaterialComp.MaterialTypes[MaterialKeys.Stack].Name)
                    return false;

                var toTransfer = Math.Min(StackCount, itemComp.StackAvailableSpace);
                StackCount -= toTransfer;
                itemComp.Add(toTransfer);
            }

            return false;
        }

        void IExamine.Examine(FormattedMessage message)
        {
            if(StackMax <= 1)
                return;

            var loc = IoCManager.Resolve<ILocalizationManager>();
            message.AddMarkup(loc.GetPluralString(
                "There is [color=lightgray]1[/color] thing in the stack",
                "There are [color=lightgray]{0}[/color] things in the stack.", _stackCount, _stackCount));
        }

        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);

            serializer.DataField(ref _equippedPrefix, "HeldPrefix", null);
            serializer.DataField(ref _stackMax, "stkCount", 1);
            serializer.DataField(ref _stackCount, "stkMax", 1);
        }

        public bool AttackHand(AttackHandEventArgs eventArgs)
        {
            var hands = eventArgs.User.GetComponent<IHandsComponent>();
            hands.PutInHand(this, hands.ActiveIndex, fallback: false);
            return true;
        }

        [Verb]
        public sealed class PickUpVerb : Verb<ItemComponent>
        {
            protected override string GetText(IEntity user, ItemComponent component)
            {
                if (user.TryGetComponent(out HandsComponent hands) && hands.IsHolding(component.Owner))
                {
                    return "Pick Up (Already Holding)";
                }
                return "Pick Up";
            }

            protected override VerbVisibility GetVisibility(IEntity user, ItemComponent component)
            {
                if (user.TryGetComponent(out HandsComponent hands) && hands.IsHolding(component.Owner))
                {
                    return VerbVisibility.Invisible;
                }

                return VerbVisibility.Visible;
            }

            protected override void Activate(IEntity user, ItemComponent component)
            {
                if (user.TryGetComponent(out HandsComponent hands) && !hands.IsHolding(component.Owner))
                {
                    hands.PutInHand(component);
                }
            }
        }

        public override ComponentState GetComponentState()
        {
            return new ItemComponentState(_equippedPrefix, _stackCount, _stackMax);
        }

        public void Fumble()
        {
            if (Owner.TryGetComponent<PhysicsComponent>(out var physicsComponent))
            {
                physicsComponent.LinearVelocity += RandomOffset();
            }
        }

        private Vector2 RandomOffset()
        {
            return new Vector2(RandomOffset(), RandomOffset());
            float RandomOffset()
            {
                var size = 15.0F;
                return (_robustRandom.NextFloat() * size) - size / 2;
            }
        }
    }
}
