using andrefmello91.Extensions;
using UnitsNet;
using UnitsNet.Units;

namespace andrefmello91.Material.Concrete
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
			private const double ec = -0.002;
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
			public MCFT(Pressure strength, AggregateType type = AggregateType.Quartzite) : base(strength, type)
			{
			}

			#endregion

			#region

			protected override void CalculateCustomParameters()
			{
				TensileStrength = (Pressure) fcr().As(PressureUnit.Megapascal);
				ElasticModule   = Ec();
				PlasticStrain   = ec;
				UltimateStrain  = ecu;
			}

			private double fcr() => 0.33 * Strength.Megapascals.Sqrt();

			private Pressure Ec() => -2 * Strength / ec;

			#endregion

		}
	}
}