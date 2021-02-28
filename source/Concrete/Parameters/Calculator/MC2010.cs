using System;
using System.Linq;
using Extensions;
using MathNet.Numerics.Interpolation;
using UnitsNet;

namespace Material.Concrete
{
	public partial struct Parameters
	{
		/// <summary>
		///     Parameters calculated according to FIB Model Code 2010.
		/// </summary>
		private class MC2010 : ParameterCalculator
		{
			#region Fields

			/// <summary>
			///     Array of high strength concrete classes, C50 to C90 (MC2010).
			/// </summary>
			private readonly double[] _classes =
			{
				50, 55, 60, 70, 80, 90
			};

			/// <summary>
			///     Array of ultimate strains for each concrete class, C50 to C90 (MC2010).
			/// </summary>
			private readonly double[] _ultimateStrain =
			{
				-0.0034, -0.0034, -0.0033, -0.0032, -0.0031, -0.003
			};

			#endregion

			#region Properties

			public override ParameterModel Model => ParameterModel.MC2010;

			#endregion

			#region Constructors

			/// <summary>
			///     Parameter calculator based on fib Model Code 2010.
			/// </summary>
			/// <inheritdoc />
			public MC2010(Pressure strength, AggregateType type = AggregateType.Quartzite) : base(strength, type)
			{
			}

			#endregion

			#region

			protected override void CalculateCustomParameters()
			{
				TensileStrength = Pressure.FromMegapascals(fctm());
				ElasticModule   = Pressure.FromMegapascals(Eci());
				PlasticStrain   = ec1();
				UltimateStrain  = ecu();
			}

			private double AlphaE() =>
				Type switch
				{
					AggregateType.Basalt    => 1.2,
					AggregateType.Quartzite => 1,
					_                       => 0.9
				};

			private double fctm() => Strength.Megapascals <= 50 ? 0.3 * Strength.Megapascals.Pow(2.0 / 3) : 2.12 * Math.Log(1 + 0.1 * Strength.Megapascals);

			private double Eci() => 21500 * AlphaE() * (0.1 * Strength.Megapascals).Pow(1.0 / 3);

			private double ec1() => -1.6 / 1000 * (0.1 * Strength.Megapascals).Pow(0.25);

			private Pressure Ec1() => Strength / ec1();

			private double ecu()
			{
				// Verify fcm
				if (Strength.Megapascals < 50)
					return
						-0.0035;

				if (Strength.Megapascals >= 90)
					return
						-0.003;

				// Get classes and ultimate strains
				// Interpolate values
				if (!_classes.Contains(Strength.Megapascals))
					return
						UltimateStrainSpline().Interpolate(Strength.Megapascals);

				var i = Array.IndexOf(_classes, Strength.Megapascals);

				return
					_ultimateStrain[i];
			}

			/// <summary>
			///     Interpolation for ultimate strains.
			/// </summary>
			private CubicSpline UltimateStrainSpline() => CubicSpline.InterpolateAkimaSorted(_classes, _ultimateStrain);

			#endregion
		}
	}
}