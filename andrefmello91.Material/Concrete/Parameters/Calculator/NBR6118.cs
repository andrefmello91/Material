using System;
using andrefmello91.Extensions;
using UnitsNet;
using UnitsNet.Units;

namespace andrefmello91.Material.Concrete
{
	public partial struct Parameters
	{
		/// <summary>
		///     Parameters calculated according to NBR6118:2014.
		/// </summary>
		private class NBR6118 : ParameterCalculator
		{

			#region Properties

			public override ParameterModel Model => ParameterModel.NBR6118;

			public override Pressure SecantModule => AlphaI() * ElasticModule;

			#endregion

			#region Constructors

			/// <summary>
			///     Parameters calculator based on NBR 6118:2014.
			/// </summary>
			/// <inheritdoc />
			public NBR6118(Pressure strength, AggregateType type = AggregateType.Quartzite)
				: base(strength, type)
			{
			}

			#endregion

			#region Methods

			protected override void CalculateCustomParameters()
			{
				TensileStrength = (Pressure) fctm().As(PressureUnit.Megapascal);
				ElasticModule   = (Pressure) Eci().As(PressureUnit.Megapascal);
				PlasticStrain   = ec2();
				UltimateStrain  = ecu();
			}

			private double AlphaE() =>
				Type switch
				{
					AggregateType.Basalt    => 1.2,
					AggregateType.Quartzite => 1,
					AggregateType.Limestone => 0.9,
					_                       => 0.7
				};

			private double AlphaI() => Math.Min(0.8 + 0.2 * Strength.Megapascals / 80, 1);

			private double ec2() => Strength.Megapascals <= 50 ? -0.002 : -0.002 - 0.000085 * (Strength.Megapascals - 50).Pow(0.53);

			private double Eci() =>
				Strength.Megapascals <= 50
					? AlphaE() * 5600 * Math.Sqrt(Strength.Megapascals)
					: 21500 * AlphaE() * (0.1 * Strength.Megapascals + 1.25).Pow(1.0 / 3);

			private double ecu() => Strength.Megapascals <= 50 ? -0.0035 : -0.0026 - 0.035 * (0.01 * (90 - Strength.Megapascals)).Pow(4);

			private double fctm() => Strength.Megapascals <= 50 ? 0.3 * Strength.Megapascals.Pow(2.0 / 3) : 2.12 * Math.Log(1 + 0.11 * Strength.Megapascals);

			#endregion

		}
	}
}