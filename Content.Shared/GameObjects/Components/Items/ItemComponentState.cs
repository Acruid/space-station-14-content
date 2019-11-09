using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using System;

namespace Content.Shared.GameObjects.Components.Items
{
    [Serializable, NetSerializable]
    public class ItemComponentState : ComponentState
    {
        public string EquippedPrefix { get; }
        public int StackCount { get; }
        public int StackMax { get; }

        public ItemComponentState(string equippedPrefix, int stackCount, int stackMax) : base(ContentNetIDs.ITEM)
        {
            EquippedPrefix = equippedPrefix;
            StackCount = stackCount;
            StackMax = stackMax;
        }

        protected ItemComponentState(string equippedPrefix, uint netId, int stackCount, int stackMax) : base(netId)
        {
            EquippedPrefix = equippedPrefix;
            StackCount = stackCount;
            StackMax = stackMax;
        }
    }
}
