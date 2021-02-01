using System;
using Extensions;
using UnitsNet;

namespace Material.Concrete
{
	/// <summary>
	/// Parameters calculated according to Disturbed Stress Field Model.
	/// </summary>
	public class DSFMParameters : Parameters
	{
		// Strains
		private const double ec  = -0.002;
		private const double ecu = -0.0035;

        /// <summary>
        /// Parameters based on DSFM formulation.
        /// </summary>
        /// <inheritdoc/>
        public DSFMParameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite)
	        : this(Pressure.FromMegapascals(strength), Length.FromMillimeters(aggregateDiameter), aggregateType)
        {
        }

        /// <summary>
        /// Parameters based on DSFM formulation.
        /// </summary>
        /// <inheritdoc/>
        public DSFMParameters(Pressure strength, Length aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite) : base(strength, aggregateDiameter, aggregateType)
        {
        }

        private double fcr() => 0.33 * Strength.Sqrt();

        //private double fcr() => 0.65 * Math.Pow(Strength, 0.33);

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
		public override bool Equals(Parameters other) => other is DSFMParameters && base.Equals(other);

		public override bool Equals(object obj) => obj is DSFMParameters other && base.Equals(other);

		public override int GetHashCode() => base.GetHashCode();
	}
}