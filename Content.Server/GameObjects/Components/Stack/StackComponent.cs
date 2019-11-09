using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using Robust.Shared.ViewVariables;

namespace Content.Server.GameObjects.Components.Stack
{
    [RegisterComponent, Obsolete("Use the ItemComponent")]
    public class StackComponent : Component
    {
        private int _count = 50;
        private int _maxCount = 50;

        public override string Name => "Stack";

        [ViewVariables]
        public int Count => _count;

        [ViewVariables]
        public int MaxCount => _maxCount;

        public override void ExposeData(ObjectSerializer serializer)
        {
            serializer.DataFieldCached(ref _maxCount, "max", 50);
            serializer.DataFieldCached(ref _count, "count", 50);
        }
    }
}
