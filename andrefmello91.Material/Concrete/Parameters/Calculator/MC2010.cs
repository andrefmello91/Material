using System;
using System.Linq;
using andrefmello91.Extensions;
using MathNet.Numerics.Interpolation;
using UnitsNet;
using UnitsNet.Units;

namespace andrefmello91.Material.Concrete
{
	/// <summary>
	///     Parameters calculated according to FIB Model Code 2010.
	/// </summary>
	internal class MC2010 : ParameterCalculator
	{

		#region Fields

		/// <summary>
		///     Array of high strength concrete classes, C50 to C90 (MC2010).
		/// </summary>
		private static readonly double[] Classes =
		{
			50, 55, 60, 70, 80, 90
		};

		/// <summary>
		///     Array of ultimate strains for each concrete class, C50 to C90 (MC2010).
		/// </summary>
		private static readonly double[] ClassesUltStrains =
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
		internal MC2010(Pressure strength, AggregateType type = AggregateType.Quartzite) : base(strength, type)
		{
		}

		#endregion

		#region Methods

		protected override void CalculateCustomParameters()
		{
			TensileStrength = fctm(Strength);
			ElasticModule   = Eci(Strength, Type);
			PlasticStrain   = ec1(Strength);
			UltimateStrain  = ecu(Strength);
		}

		private static double AlphaE(AggregateType type) =>
			type switch
			{
				AggregateType.Basalt    => 1.2,
				AggregateType.Quartzite => 1,
				_                       => 0.9
			};

		private static double ec1(Pressure strength) => -1.6 / 1000 * (0.1 * strength.Megapascals).Pow(0.25);

		private static Pressure Ec1(Pressure strength) => strength / ec1(strength);

		private static Pressure Eci(Pressure strength, AggregateType type) => Pressure.From(21500, PressureUnit.Megapascal) * AlphaE(type) * (0.1 * strength.Megapascals).Pow(1.0 / 3);

		private static double ecu(Pressure strength)
		{
			switch (strength.Megapascals)
			{
				// Verify fcm
				case < 50:
					return
						-0.0035;

				case >= 90:
					return
						-0.003;
			}

			// Get classes and ultimate strains
			// Interpolate values
			if (!Classes.Contains(strength.Megapascals))
				return
					UltimateStrainSpline().Interpolate(strength.Megapascals);

			var i = Array.IndexOf(Classes, strength.Megapascals);

			return
				ClassesUltStrains[i];
		}

		private static Pressure fctm(Pressure strength) =>
			strength.Megapascals <= 50
				? Pressure.From(0.3, PressureUnit.Megapascal) * strength.Megapascals.Pow(2.0 / 3)
				: Pressure.From(2.12, PressureUnit.Megapascal) * Math.Log(1 + 0.1 * strength.Megapascals);

		/// <summary>
		///     Interpolation for ultimate strains.
		/// </summary>
		private static CubicSpline UltimateStrainSpline() =>
			CubicSpline.InterpolateAkimaSorted(Classes, ClassesUltStrains);

		#endregion

	}
}