using System;

namespace Material.Concrete
{
	/// <summary>
	/// Parameters calculated according to Modified Compression Field Theory.
	/// </summary>
	public class MCFTParameters : Parameters
	{
		/// <inheritdoc/>
		/// <summary>
		/// Parameters based on Classic MCFT formulation.
		/// </summary>
		public MCFTParameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite) : base(strength, aggregateDiameter, aggregateType)
		{
			UpdateParameters();
		}

		private double fcr() => 0.33 * Math.Sqrt(Strength);
		private double ec = -0.002;
		private double ecu = -0.0035;
		private double Ec() => -2 * Strength / ec;

		///<inheritdoc/>
		public override void UpdateParameters()
		{
			TensileStrength = fcr();
			PlasticStrain   = ec;
			InitialModule   = Ec();
			UltimateStrain  = ecu;
		}

		/// <inheritdoc/>
		public override bool Equals(Parameters other)
		{
			if (other != null && other is MCFTParameters)
				return base.Equals(other);

			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is MCFTParameters other)
				return base.Equals(other);

			return false;
		}

		public override int GetHashCode() => base.GetHashCode();
	}
}