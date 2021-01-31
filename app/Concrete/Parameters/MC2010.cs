using System;
using System.Linq;
using Extensions;
using MathNet.Numerics.Interpolation;
using UnitsNet;

namespace Material.Concrete
{
	/// <summary>
	/// Parameters calculated according to FIB Model Code 2010.
	/// </summary>
	public class MC2010Parameters : Parameters
	{
		/// <summary>
		/// Array of high strength concrete classes, C50 to C90 (MC2010).
		/// </summary>
		private readonly double[] _classes =
		{
			50, 55, 60, 70, 80, 90
		};

		/// <summary>
		/// Array of ultimate strains for each concrete class, C50 to C90 (MC2010).
		/// </summary>
		private readonly double[] _ultimateStrain =
		{
			-0.0034, -0.0034, -0.0033, -0.0032, -0.0031, -0.003
		};

        ///<inheritdoc/>
        public override double FractureParameter => 0.073 * Strength.Pow(0.18);

        /// <summary>
        /// Parameters based on fib Model Code 2010.
        /// </summary>
        ///<inheritdoc/>
        public MC2010Parameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite)
	        : this(Pressure.FromMegapascals(strength), Length.FromMillimeters(aggregateDiameter), aggregateType)
        {
        }

        /// <summary>
        /// Parameters based on fib Model Code 2010.
        /// </summary>
        /// <inheritdoc/>
        public MC2010Parameters(Pressure strength, Length aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite) : base(strength, aggregateDiameter, aggregateType)
        {
        }

        ///<inheritdoc/>
        public sealed override void UpdateParameters()
        {
	        TensileStrength = fctm();
	        PlasticStrain   = ec1();
	        InitialModule   = Eci();
	        SecantModule    = Ec1();
	        UltimateStrain  = ecu();
        }

        #region ModelCode2010 Parameters
        private double AlphaE()
		{
			switch (Type)
			{
				case AggregateType.Basalt:
					return 1.2;

				case AggregateType.Quartzite:
					return 1;

				// Limestone or sandstone
				default:
					return 0.9;
			}
		}

		private double fctm() => Strength <= 50 ? 0.3 * Strength.Pow(2.0 / 3) : 2.12 * Math.Log(1 + 0.1 * Strength);

		private double Eci() => 21500 * AlphaE() * (0.1 * Strength).Pow(1.0 / 3);

		private double ec1() => -1.6 / 1000 *(0.1 * Strength).Pow(0.25);

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
			if (_classes.Contains(Strength))
			{
				int i = Array.IndexOf(_classes, Strength);

				return
					_ultimateStrain[i];
			}

			// Interpolate values
			return
				UltimateStrainSpline().Interpolate(Strength);
		}

		/// <summary>
		/// Interpolation for ultimate strains.
		/// </summary>
		private CubicSpline UltimateStrainSpline() => CubicSpline.InterpolateAkimaSorted(_classes, _ultimateStrain);
		#endregion

		/// <inheritdoc/>
		public override bool Equals(Parameters other) => other is MC2010Parameters && base.Equals(other);

		public override bool Equals(object obj) => obj is MC2010Parameters other && base.Equals(other);

		public override int GetHashCode() => base.GetHashCode();
	}
}