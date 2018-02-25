using System;
using Content.Server.Interfaces.GameObjects;
using Content.Shared.GameObjects;
using Content.Shared.Maths;
using SS14.Shared.GameObjects;
using SS14.Shared.GameObjects.Serialization;

namespace Content.Server.GameObjects
{
    /// <summary>
    /// Handles changing temperature,
    /// informing others of the current temperature,
    /// and taking fire damage from high temperature.
    /// </summary>
    public class TemperatureComponent : Component, ITemperatureComponent
    {
        /// <inheritdoc />
        public override string Name => "Temperature";

        /// <inheritdoc />
        public override uint? NetID => ContentNetIDs.TEMPERATURE;

        //TODO: should be programmatic instead of how it currently is
        public float CurrentTemperature { get; private set; } = PhysicalConstants.ZERO_CELCIUS;

        float _fireDamageThreshold;
        float _fireDamageCoefficient = 1;

        float _secondsSinceLastDamageUpdate;

        public override void ExposeData(EntitySerializer serializer)
        {
            base.ExposeData(serializer);

            serializer.DataField(ref _fireDamageThreshold, "firedamagethreshold", 0f);
            serializer.DataField(ref _fireDamageCoefficient, "firedamagecoefficient", 1f);
        }

        /// <inheritdoc />
        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var fireDamage = (int) Math.Floor(Math.Max(0, CurrentTemperature - _fireDamageThreshold) / _fireDamageCoefficient);

            _secondsSinceLastDamageUpdate += frameTime;

            Owner.TryGetComponent(out DamageableComponent component);

            while (_secondsSinceLastDamageUpdate >= 1)
            {
                component?.TakeDamage(DamageType.Heat, fireDamage);
                _secondsSinceLastDamageUpdate -= 1;
            }
        }
    }
}
