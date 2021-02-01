using Extensions;
using UnitsNet;

namespace Material.Concrete
{
	public partial struct Parameters
	{
		/// <summary>
		///     Parameters calculated according to Modified Compression Field Theory.
		/// </summary>
		private class MCFT : ParameterCalculator
		{
			#region Fields

			// Strains
			private const double ec  = -0.002;
			private const double ecu = -0.0035;

			#endregion

			#region Properties

			public override ParameterModel Model => ParameterModel.MCFT;

			#endregion

			#region Constructors

			/// <summary>
			///     Parameter calculator based on Classic MCFT formulation.
			/// </summary>
			/// <inheritdoc />
			public MCFT(Pressure strength, AggregateType type = AggregateType.Quartzite) : base(strength,  type)
			{
				TensileStrength = Pressure.FromMegapascals(fcr());
				ElasticModule   = Ec();
				PlasticStrain   = ec;
				UltimateStrain  = ecu;
			}

			#endregion

			#region

			private double fcr() => 0.33 * Strength.Megapascals.Sqrt();

			private Pressure Ec()  => -2 * Strength / ec;

			#endregion
		}
	}
}