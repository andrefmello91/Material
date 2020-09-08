using System;
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
	        UpdateParameters();
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
			}

			// Sandstone
			return 0.7;
		}

		private double AlphaI() => Math.Min(0.8 + 0.2 * Strength / 80, 1);

		private double fctm()
		{
			if (Strength <= 50)
				return
					0.3 * Math.Pow(Strength, 0.66666667);
			//else
			return
				2.12 * Math.Log(1 + 0.11 * Strength);
		}

		private double Eci()
		{
			if (Strength <= 50)
				return
					AlphaE() * 5600 * Math.Sqrt(Strength);

			return
				21500 * AlphaE() * Math.Pow((0.1 * Strength + 1.25), 0.333333);
		}

		private double Ecs() => AlphaI() * InitialModule;

		private double ec2()
		{
			if (Strength <= 50)
				return
					-0.002;

			return
				-0.002 - 0.000085 * Math.Pow(Strength - 50, 0.53);
		}

		private double ecu()
		{
			if (Strength <= 50)
				return
					-0.0035;

			return
				-0.0026 - 0.035 * Math.Pow(0.01 * (90 - Strength), 4);
		}
		#endregion

		/// <inheritdoc/>
        public override bool Equals(Parameters other)
		{
			if (other != null && other is NBR6118Parameters)
				return base.Equals(other);

			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is NBR6118Parameters other)
				return base.Equals(other);

			return false;
		}

		public override int GetHashCode() => base.GetHashCode();
	}
}