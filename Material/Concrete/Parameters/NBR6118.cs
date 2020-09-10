using System;
using Extensions.Number;
using UnitsNet;

namespace Material.Concrete
{
	/// <summary>
	/// Parameters calculated according to NBR6118:2014.
	/// </summary>
	public class NBR6118Parameters : Parameters
	{
        /// <summary>
        /// Parameters based on NBR 6118:2014.
        /// </summary>
		/// <inheritdoc/>
        public NBR6118Parameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite) 
	        : this(Pressure.FromMegapascals(strength), Length.FromMillimeters(aggregateDiameter), aggregateType)
		{
		}

        /// <summary>
        /// Parameters based on NBR 6118:2014.
        /// </summary>
        /// <inheritdoc/>
        public NBR6118Parameters(Pressure strength, Length aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite) : base(strength, aggregateDiameter, aggregateType)
        {
        }

        ///<inheritdoc/>
        public sealed override void UpdateParameters()
        {
	        TensileStrength = fctm();
	        PlasticStrain   = ec2();
	        InitialModule   = Eci();
	        SecantModule    = Ecs();
	        UltimateStrain  = ecu();
        }

        #region NBR6118 Parameters
        private double AlphaE()
		{
			switch (Type)
			{
				case AggregateType.Basalt:
					return 1.2;

				case AggregateType.Quartzite:
					return 1;

				case AggregateType.Limestone:
					return 0.9;

				// Sandstone
				default:
					return 0.7;
			}
		}

		private double AlphaI() => Math.Min(0.8 + 0.2 * Strength / 80, 1);

		private double fctm() => Strength <= 50 ? 0.3 * Strength.Pow(2 / 3) : 2.12 * Math.Log(1 + 0.11 * Strength);

		private double Eci() =>
			Strength <= 50
				? AlphaE() * 5600 * Math.Sqrt(Strength)
				: 21500 * AlphaE() *(0.1 * Strength + 1.25).Pow(1 / 3);

		private double Ecs() => AlphaI() * InitialModule;

		private double ec2() => Strength <= 50 ? -0.002 : -0.002 - 0.000085 * (Strength - 50).Pow(0.53);

		private double ecu() => Strength <= 50 ? -0.0035 : -0.0026 - 0.035 * (0.01 * (90 - Strength)).Pow(4);

		#endregion

		/// <inheritdoc/>
        public override bool Equals(Parameters other) => other is NBR6118Parameters && base.Equals(other);

		public override bool Equals(object obj) => obj is NBR6118Parameters other && base.Equals(other);

		public override int GetHashCode() => base.GetHashCode();
	}
}