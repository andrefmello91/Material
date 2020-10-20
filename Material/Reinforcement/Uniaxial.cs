using System;
using Extensions.Number;
using MathNet.Numerics;
using UnitsNet;
using UnitsNet.Units;

namespace Material.Reinforcement
{
	/// <summary>
	/// Uniaxial reinforcement class.
	/// </summary>
	public class UniaxialReinforcement : IEquatable<UniaxialReinforcement>
	{
		// Auxiliary fields
		private Length _phi;
		private Area _As;
		private readonly Area _Ac;

		/// <summary>
        /// Get number of reinforcing bars.
        /// </summary>
		public int NumberOfBars  { get; }

		/// <summary>
		/// Get bar diameter, in mm.
		/// </summary>
		public double BarDiameter => _phi.Millimeters;

		/// <summary>
		/// Get reinforcement area, in mm2.
		/// </summary>
		public double Area => _As.SquareMillimeters;

		/// <summary>
        /// Get <see cref="Reinforcement.Steel"/> of this.
        /// </summary>
		public Steel Steel { get; }

        /// <summary>
        /// Reinforcement for uniaxial calculations
        /// </summary>
        /// <param name="numberOfBars">The number of bars of reinforcement.</param>
        /// <param name="barDiameter">The bar diameter (in mm).</param>
        /// <param name="steel">The steel object.</param>
        /// <param name="concreteArea">The concrete area (in mm2).</param>
        public UniaxialReinforcement(int numberOfBars, double barDiameter, Steel steel, double concreteArea = 0) 
			:this (numberOfBars, Length.FromMillimeters(barDiameter), steel, UnitsNet.Area.FromSquareMillimeters(concreteArea))
		{
		}

        /// <summary>
        /// Reinforcement for uniaxial calculations
        /// </summary>
        /// <param name="numberOfBars">The number of bars of reinforcement.</param>
        /// <param name="barDiameter">The bar diameter.</param>
        /// <param name="steel">The steel object.</param>
        /// <param name="concreteArea">The concrete area.</param>
        public UniaxialReinforcement(int numberOfBars, Length barDiameter, Steel steel, Area concreteArea)
		{
			NumberOfBars = numberOfBars;

			_phi  = barDiameter;
			_As   = CalculateArea();
			_Ac   = concreteArea;
			Steel = steel;
		}

		// Verify if reinforcement is set
		public bool IsSet => NumberOfBars > 0 && BarDiameter > 0;

		/// <summary>
		/// Get reinforcement ratio in the cross-section.
		/// </summary>
		public double Ratio => _Ac != UnitsNet.Area.Zero ? _As / _Ac : 0;

		/// <summary>
		/// Get normal stiffness, in N.
		/// </summary>
		public double Stiffness => Steel.ElasticModule * Area;

		/// <summary>
		/// Get the yield force, in N.
		/// </summary>
		public double YieldForce => Area * Steel.YieldStress;

		/// <summary>
		/// Get current force, in N.
		/// </summary>
		public double Force => Area * Steel.Stress;

		/// <summary>
		/// Calculated reinforcement area, in mm2.
		/// </summary>
		private double ReinforcementArea()
		{
			if (IsSet)
				return
					0.25 * NumberOfBars * Constants.Pi * BarDiameter * BarDiameter;

			return 0;
		}

		/// <summary>
		/// Calculated reinforcement area, in mm2.
		/// </summary>
		private Area CalculateArea()
		{
			if (IsSet)
				return
					0.25 * NumberOfBars * Constants.Pi * _phi * _phi;

			return UnitsNet.Area.Zero;
		}

		/// <summary>
		/// Calculate current force, in N.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public double CalculateForce(double strain)
		{
			return
				Area * Steel.CalculateStress(strain);
		}

		/// <summary>
		/// Calculate tension stiffening coefficient (for DSFM).
		/// </summary>
		public double TensionStiffeningCoefficient()
		{
			// Calculate coefficient for tension stiffening effect
			return
				0.25 * BarDiameter / Ratio;
		}

		/// <summary>
		/// Calculate maximum value of tensile strength that can be transmitted across cracks.
		/// </summary>
		public double MaximumPrincipalTensileStress()
		{
			// Get reinforcement stress
			double
				fs = Steel.Stress,
				fy = Steel.YieldStress;

			// Check the maximum value of fc1 that can be transmitted across cracks
			return
				Ratio * (fy - fs);
		}

		/// <summary>
		/// Set steel strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStrain(double strain)
		{
			Steel.SetStrain(strain);
		}

		/// <summary>
		/// Set steel stress, given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStress(double strain)
		{
			Steel.SetStress(strain);
		}

		/// <summary>
		/// Set steel strain and stress.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStrainAndStress(double strain)
		{
			Steel.SetStrainAndStress(strain);
		}

		public override string ToString()
		{
			char phi = (char) Characters.Phi;

			return
				$"Reinforcement: {NumberOfBars} {phi} {_phi} ({_As})\n\n"
				+ Steel;
		}

		/// <summary>
		/// Compare two reinforcement objects.
		/// <para>Returns true if <see cref="NumberOfBars"/> and <see cref="BarDiameter"/> are equal.</para>
		/// </summary>
		/// <param name="other">The other reinforcement object.</param>
		public virtual bool EqualsNumberAndDiameter(UniaxialReinforcement other) => !(other is null) && NumberOfBars == other.NumberOfBars && BarDiameter.Approx(other.BarDiameter);

		/// <summary>
		/// Compare two reinforcement objects.
		/// <para>Returns true if parameters are equal.</para>
		/// </summary>
		/// <param name="other">The other reinforcement object.</param>
		public virtual bool Equals(UniaxialReinforcement other) => !(other is null) && EqualsNumberAndDiameter(other) && Steel == other.Steel;

		public override bool Equals(object other) => other is UniaxialReinforcement reinforcement && Equals(reinforcement);

		public override int GetHashCode() => (int)BarDiameter.Pow(NumberOfBars);

		/// <summary>
		/// Returns true if steel parameters are equal.
		/// </summary>
		public static bool operator == (UniaxialReinforcement left, UniaxialReinforcement right) => !(left is null) && left.Equals(right);

		/// <summary>
		/// Returns true if steel parameters are different.
		/// </summary>
		public static bool operator != (UniaxialReinforcement left, UniaxialReinforcement right) => !(left is null) && !left.Equals(right);
	}
}
