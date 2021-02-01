using System;
using Extensions;
using UnitsNet;

namespace Material.Concrete
{
	public partial struct Parameters
	{
		/// <summary>
		/// Parameters calculated according to Modified Compression Field Theory.
		/// </summary>
		private class DSFM : ParameterCalculator
		{
			// Strains
			private const double ec  = -0.002;
			private const double ecu = -0.0035;

			public override ParameterModel Model => ParameterModel.MCFT;

			/// <summary>
			///		Parameter calculator based on Classic MCFT formulation.
			/// </summary>
			/// <inheritdoc/>
			public DSFM(Pressure strength, AggregateType type = AggregateType.Quartzite) : base(strength,  type)
			{
				TensileStrength = Pressure.FromMegapascals(fcr());
				ElasticModule   = Ec();
				PlasticStrain   = ec;
				UltimateStrain  = ecu;
			}

			private double fcr() => 0.65 * Math.Pow(Strength.Megapascals, 0.33);

			private Pressure Ec()  => -2 * Strength / ec;
		}
	}
}