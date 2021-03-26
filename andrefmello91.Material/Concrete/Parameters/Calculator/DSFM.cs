using System;
using UnitsNet;

namespace andrefmello91.Material.Concrete
{
	public partial struct Parameters
	{
		/// <summary>
		///     Parameters calculated according to Modified Compression Field Theory.
		/// </summary>
		private class DSFM : ParameterCalculator
		{

			#region Fields

			// Strains
			private const double ec = -0.002;
			private const double ecu = -0.0035;

			#endregion

			#region Properties

			public override ParameterModel Model => ParameterModel.MCFT;

			#endregion

			#region Constructors

			/// <summary>
			///     Parameter calculator based on Classic DSFM formulation.
			/// </summary>
			/// <inheritdoc />
			public DSFM(Pressure strength, AggregateType type = AggregateType.Quartzite) : base(strength, type)
			{
				TensileStrength = Pressure.FromMegapascals(fcr());
				ElasticModule   = Ec();
				PlasticStrain   = ec;
				UltimateStrain  = ecu;
			}

			#endregion

			#region Methods

			protected override void CalculateCustomParameters()
			{
				TensileStrength = Pressure.FromMegapascals(fcr());
				ElasticModule   = Ec();
				PlasticStrain   = ec;
				UltimateStrain  = ecu;
			}

			private Pressure Ec() => -2 * Strength / ec;

			private double fcr() => 0.65 * Math.Pow(Strength.Megapascals, 0.33);

			#endregion

		}
	}
}