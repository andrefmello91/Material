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

			public override Pressure SecantModule => AlphaI(Strength) * ElasticModule;

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
				TensileStrength = fctm(Strength);
				ElasticModule   = Eci(Strength, Type);
				PlasticStrain   = ec2(Strength);
				UltimateStrain  = ecu(Strength);
			}

			private static double AlphaE(AggregateType type) =>
				type switch
				{
					AggregateType.Basalt    => 1.2,
					AggregateType.Quartzite => 1,
					AggregateType.Limestone => 0.9,
					_                       => 0.7
				};

			private static double AlphaI(Pressure strength) => Math.Min(0.8 + 0.2 * strength.Megapascals / 80, 1);

			private static double ec2(Pressure strength) =>
				strength.Megapascals <= 50
					? -0.002
					: -0.002 - 0.000085 * (strength.Megapascals - 50).Pow(0.53);

			private static Pressure Eci(Pressure strength, AggregateType type) =>
				strength.Megapascals <= 50
					? Pressure.From(5600, PressureUnit.Megapascal) * AlphaE(type) * Math.Sqrt(strength.Megapascals)
					: Pressure.From(21500, PressureUnit.Megapascal) * AlphaE(type) * (0.1 * strength.Megapascals + 1.25).Pow(1.0 / 3);

			private static double ecu(Pressure strength) =>
				strength.Megapascals <= 50
					? -0.0035
					: -0.0026 - 0.035 * (0.01 * (90 - strength.Megapascals)).Pow(4);

			private Pressure fctm(Pressure strength) =>
				strength.Megapascals <= 50
					? Pressure.From(0.3, PressureUnit.Megapascal) * strength.Megapascals.Pow(2.0 / 3)
					: Pressure.From(2.12, PressureUnit.Megapascal) * Math.Log(1 + 0.11 * strength.Megapascals);

			#endregion

		}
	}
}