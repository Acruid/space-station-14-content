using System;
using Content.Shared.GameObjects;
using Content.Shared.GameObjects.Components.Items;
using Robust.Client.Graphics;
using Robust.Client.Interfaces.ResourceManagement;
using Robust.Client.ResourceManagement;
using Robust.Shared.GameObjects;
using Robust.Shared.GameObjects.Components.Renderable;
using Robust.Shared.IoC;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Content.Client.GameObjects
{
    [RegisterComponent]
    public class ItemComponent : Component
    {
        public override string Name => "Item";
        public override uint? NetID => ContentNetIDs.ITEM;
        public override Type StateType => typeof(ItemComponentState);

        [ViewVariables] protected ResourcePath RsiPath;
        private string _equippedPrefix;

        private int _stackCount = 1;
        private int _stackMax = 1;

        [ViewVariables(VVAccess.ReadWrite)]
        public string EquippedPrefix
        {
            get => _equippedPrefix;
            set => _equippedPrefix = value;
        }

        [ViewVariables]
        public int StackCount => _stackCount;

        [ViewVariables]
        public int StackMax => _stackMax;

        [ViewVariables]
        public int StackAvailableSpace => _stackMax - _stackCount;

        public (RSI rsi, RSI.StateId stateId)? GetInHandStateInfo(string hand)
        {
            if (RsiPath == null)
            {
                return null;
            }

            var rsi = GetRSI();
            var stateId = EquippedPrefix != null ? $"{EquippedPrefix}-inhand-{hand}" : $"inhand-{hand}";
            if (rsi.TryGetState(stateId, out _))
            {
                return (rsi, stateId);
            }

            return null;
        }

        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);

            serializer.DataFieldCached(ref RsiPath, "sprite", null);
            serializer.DataFieldCached(ref _equippedPrefix, "prefix", null);

            serializer.DataField(ref _stackMax, "stkCount", 1);
            serializer.DataField(ref _stackCount, "stkMax", 1);
        }

        protected RSI GetRSI()
        {
            var resourceCache = IoCManager.Resolve<IResourceCache>();
            return resourceCache.GetResource<RSIResource>(SharedSpriteComponent.TextureRoot / RsiPath).RSI;
        }

        public override void HandleComponentState(ComponentState curState, ComponentState nextState)
        {
            if(curState == null)
                return;

            var itemComponentState = (ItemComponentState)curState;
            EquippedPrefix = itemComponentState.EquippedPrefix;
            _stackCount = itemComponentState.StackCount;
            _stackMax = itemComponentState.StackMax;
        }
    }
}
