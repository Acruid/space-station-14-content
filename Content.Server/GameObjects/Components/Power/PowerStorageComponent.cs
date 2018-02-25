using System;
using SS14.Shared.GameObjects;
using SS14.Shared.GameObjects.Serialization;

namespace Content.Server.GameObjects.Components.Power
{
    /// <summary>
    /// Feeds energy from the powernet and may have the ability to supply back into it
    /// </summary>
    public class PowerStorageComponent : Component
    {
        private bool _chargePowernet;
        private float _capacity;
        private float _charge;
        private float _chargeRate;
        private float _distributionRate;

        /// <inheritdoc />
        public override string Name => "PowerStorage";

        /// <summary>
        /// Maximum amount of energy the internal battery can store
        /// </summary>
        public float Capacity
        {
            get => _capacity;
            private set => _capacity = value;
        }

        /// <summary>
        /// Energy the battery is currently storing
        /// </summary>
        public float Charge
        {
            get => _charge;
            private set => _charge = value;
        }

        /// <summary>
        /// Rate at which energy will be taken to charge internal battery
        /// </summary>
        public float ChargeRate
        {
            get => _chargeRate;
            private set => _chargeRate = value;
        }

        /// <summary>
        /// Rate at which energy will be distributed to the powernet if needed
        /// </summary>
        public float DistributionRate
        {
            get => _distributionRate;
            private set => _distributionRate = value;
        }

        /// <summary>
        /// Do we distribute power into the powernet from our stores if the powernet requires it?
        /// </summary>
        public bool ChargePowernet
        {
            get => _chargePowernet;
            set
            {
                _chargePowernet = value;
                if (Owner.TryGetComponent(out PowerNodeComponent node))
                {
                    node.Parent?.UpdateStorageType(this);
                }
            }
        }

        /// <inheritdoc />
        public override void ExposeData(EntitySerializer serializer)
        {
            base.ExposeData(serializer);

            serializer.DataField(ref _capacity, "Capacity", 10000);
            serializer.DataField(ref _charge, "Charge", 0);
            serializer.DataField(ref _chargeRate, "ChargeRate", 1000);
            serializer.DataField(ref _distributionRate, "DistributionRate", 1000);
            serializer.DataField(ref _chargePowernet, "ChargePowernet", false);
        }

        public override void OnAdd()
        {
            base.OnAdd();

            if (!Owner.TryGetComponent(out PowerNodeComponent node))
            {
                Owner.AddComponent<PowerNodeComponent>();
            }
            node.OnPowernetConnect += PowernetConnect;
            node.OnPowernetDisconnect += PowernetDisconnect;
            node.OnPowernetRegenerate += PowernetRegenerate;
        }

        public override void OnRemove()
        {
            if (Owner.TryGetComponent(out PowerNodeComponent node))
            {
                if (node.Parent != null)
                {
                    node.Parent.RemovePowerStorage(this);
                }

                node.OnPowernetConnect -= PowernetConnect;
                node.OnPowernetDisconnect -= PowernetDisconnect;
                node.OnPowernetRegenerate -= PowernetRegenerate;
            }

            base.OnRemove();
        }

        /// <summary>
        /// Checks if the storage can supply the amount of charge directly requested
        /// </summary>
        public bool CanDeductCharge(float todeduct)
        {
            if (Charge > todeduct)
                return true;
            return false;
        }

        /// <summary>
        /// Deducts the requested charge from the energy storage
        /// </summary>
        public void DeductCharge(float todeduct)
        {
            Charge = Math.Min(0, Charge - todeduct);
        }

        /// <summary>
        /// Returns all possible charge available from the energy storage
        /// </summary>
        public float RequestAllCharge()
        {
            return Math.Min(ChargeRate, Capacity - Charge);
        }

        /// <summary>
        /// Returns the charge available from the energy storage
        /// </summary>
        public float RequestCharge()
        {
            return Math.Min(ChargeRate, Capacity - Charge);
        }

        /// <summary>
        /// Returns the charge available from the energy storage
        /// </summary>
        public float AvailableCharge()
        {
            return Math.Min(DistributionRate, Charge);
        }

        /// <summary>
        /// Gives the storage one full tick of charging its energy storage
        /// </summary>
        public void ChargePowerTick()
        {
            Charge = Math.Max(Charge + ChargeRate, Capacity);
        }

        /// <summary>
        /// Takes from the storage one full tick of energy
        /// </summary>
        public void RetrievePassiveStorage()
        {
            Charge = Math.Min(Charge - DistributionRate, 0);
        }

        /// <summary>
        /// Node has become anchored to a powernet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventarg"></param>
        private void PowernetConnect(object sender, PowernetEventArgs eventarg)
        {
            eventarg.Powernet.AddPowerStorage(this);
        }

        /// <summary>
        /// Node has had its powernet regenerated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventarg"></param>
        private void PowernetRegenerate(object sender, PowernetEventArgs eventarg)
        {
            eventarg.Powernet.AddPowerStorage(this);
        }

        /// <summary>
        /// Node has become unanchored from a powernet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventarg"></param>
        private void PowernetDisconnect(object sender, PowernetEventArgs eventarg)
        {
            eventarg.Powernet.RemovePowerStorage(this);
        }
    }
}
