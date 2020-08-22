using System;
using System.Linq;
using MathNet.Numerics.Interpolation;

namespace Material.Concrete
{
	/// <summary>
	/// Parameters calculated according to FIB Model Code 2010.
	/// </summary>
	public class MC2010Parameters : Parameters
	{
		///<inheritdoc/>
		/// <summary>
		/// Parameters based on fib Model Code 2010.
		/// </summary>
		public MC2010Parameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite) : base(strength, aggregateDiameter, aggregateType)
		{
			UpdateParameters();
		}

		// Parameter calculation using MC2010 nomenclature
		private double AlphaE()
		{
			switch (Type)
			{
				case AggregateType.Basalt:
					return 1.2;

				case AggregateType.Quartzite:
					return 1;
			}

			// Limestone or sandstone
			return 0.9;
		}

		private double fctm()
		{
			if (Strength <= 50)
				return
					0.3 * Math.Pow(Strength, 0.66666667);
			//else
			return
				2.12 * Math.Log(1 + 0.1 * Strength);
		}

		private double Eci() => 21500 * AlphaE() * Math.Pow(Strength / 10, 0.33333333);
		private double ec1() => -1.6 / 1000 * Math.Pow(Strength / 10, 0.25);
		private double Ec1() => Strength / ec1();
		private double k() => Eci() / Ec1();

		private double ecu()
		{
			// Verify fcm
			if (Strength < 50)
				return
					-0.0035;

			if (Strength >= 90)
				return
					-0.003;

			// Get classes and ultimate strains
			if (classes.Contains(Strength))
			{
				int i = Array.IndexOf(classes, Strength);

				return
					ultimateStrain[i];
			}

			// Interpolate values
			return
				UltimateStrainSpline().Interpolate(Strength);
		}

		public override double FractureParameter => 0.073 * Math.Pow(Strength, 0.18);

		/// <summary>
		/// Array of high strength concrete classes, C50 to C90 (MC2010).
		/// </summary>
		private readonly double[] classes =
		{
			50, 55, 60, 70, 80, 90
		};

		/// <summary>
		/// Array of ultimate strains for each concrete class, C50 to C90 (MC2010).
		/// </summary>
		private readonly double[] ultimateStrain =
		{
			-0.0034, -0.0034, -0.0033, -0.0032, -0.0031, -0.003
		};

		/// <summary>
		/// Interpolation for ultimate strains.
		/// </summary>
		private CubicSpline UltimateStrainSpline() => CubicSpline.InterpolateAkimaSorted(classes, ultimateStrain);

		///<inheritdoc/>
		public override void UpdateParameters()
		{
			TensileStrength = fctm();
			PlasticStrain = ec1();
			InitialModule = Eci();
			SecantModule = Ec1();
			UltimateStrain = ecu();
		}

		/// <inheritdoc/>
		public override bool Equals(Parameters other)
		{
			if (other != null && other is MC2010Parameters)
				return base.Equals(other);

			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is MC2010Parameters other)
				return base.Equals(other);

			return false;
		}

		public override int GetHashCode() => base.GetHashCode();
	}
}