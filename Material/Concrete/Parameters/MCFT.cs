﻿using System;
using Extensions.Number;
using UnitsNet;

namespace Material.Concrete
{
	/// <summary>
	/// Parameters calculated according to Modified Compression Field Theory.
	/// </summary>
	public class MCFTParameters : Parameters
	{
		// Strains
		private const double ec  = -0.002;
		private const double ecu = -0.0035;

        /// <summary>
        /// Parameters based on Classic MCFT formulation.
        /// </summary>
        /// <inheritdoc/>
        public MCFTParameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite)
	        : this(Pressure.FromMegapascals(strength), Length.FromMillimeters(aggregateDiameter), aggregateType)
        {
        }

        /// <summary>
        /// Parameters based on Classic MCFT formulation.
        /// </summary>
        /// <inheritdoc/>
        public MCFTParameters(Pressure strength, Length aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite) : base(strength, aggregateDiameter, aggregateType)
        {
        }

        private double fcr() => 0.33 * Strength.Sqrt();

        private double Ec()  => -2 * Strength / ec;

		///<inheritdoc/>
		public sealed override void UpdateParameters()
		{
			TensileStrength = fcr();
			PlasticStrain   = ec;
			InitialModule   = Ec();
			UltimateStrain  = ecu;
		}

		/// <inheritdoc/>
		public override bool Equals(Parameters other) => other is MCFTParameters && base.Equals(other);

		public override bool Equals(object obj) => obj is MCFTParameters other && base.Equals(other);

		public override int GetHashCode() => base.GetHashCode();
	}
}