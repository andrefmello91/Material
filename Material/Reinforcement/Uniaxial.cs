using System;
using MathNet.Numerics;
using UnitsNet;
using UnitsNet.Units;

namespace Material.Reinforcement
{
	/// <summary>
	/// Uniaxial reinforcement class.
	/// </summary>
	public class UniaxialReinforcement
	{
		// Properties
		public  int    NumberOfBars  { get; }
		public  double BarDiameter   { get; }
		public  double Area          { get; }
		public  Steel  Steel         { get; }
		private double ConcreteArea  { get; }

		/// <summary>
		/// Reinforcement for uniaxial calculations
		/// </summary>
		/// <param name="numberOfBars">The number of bars of reinforcement.</param>
		/// <param name="barDiameter">The bar diameter (in mm).</param>
		/// <param name="concreteArea">The concrete area (in mm2).</param>
		/// <param name="steel">The steel object.</param>
		public UniaxialReinforcement(int numberOfBars, double barDiameter, double concreteArea = 0, Steel steel = null)
		{
			NumberOfBars = numberOfBars;
			BarDiameter  = barDiameter;
			ConcreteArea = concreteArea;
			Steel        = steel;
			Area         = ReinforcementArea();
		}

		// Verify if reinforcement is set
		public bool IsSet => NumberOfBars > 0 && BarDiameter > 0;

		/// <summary>
		/// Get reinforcement ratio in the cross-section.
		/// </summary>
		public double Ratio
		{
			get
			{
				if (Area != 0 && ConcreteArea != 0)
					return
						Area / ConcreteArea;

				return 0;
			}
		}

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
		/// <returns></returns>
		private double ReinforcementArea()
		{
			if (IsSet)
				return
					0.25 * NumberOfBars * Constants.Pi * BarDiameter * BarDiameter;

			return 0;
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

		/// <summary>
		/// Write string with default units (mm and MPa).
		/// </summary>
		public override string ToString() => ToString();

		/// <summary>
		/// Write string with custom units.
		/// </summary>
		/// <param name="diameterUnit">The bar diameter unit (default: mm).</param>
		/// <param name="strengthUnit">The steel strength unit (default: MPa).</param>
		/// <returns></returns>
		public string ToString(LengthUnit diameterUnit = LengthUnit.Millimeter, PressureUnit strengthUnit = PressureUnit.Megapascal)
		{
			var areaUnit = AreaUnit.SquareMeter;

			if (diameterUnit == LengthUnit.Millimeter)
				areaUnit = AreaUnit.SquareMillimeter;

			else if (diameterUnit == LengthUnit.Centimeter)
				areaUnit = AreaUnit.SquareCentimeter;

			var d  = Length.FromMillimeters(BarDiameter).ToUnit(diameterUnit);
			var As = UnitsNet.Area.FromSquareMillimeters(Area).ToUnit(areaUnit);

			char phi = (char) Characters.Phi;

			return
				"Reinforcement: " + NumberOfBars + " " + phi + d + " (" + As +
				")\n\n" + Steel.ToString(strengthUnit);
		}

		/// <summary>
		/// Compare two reinforcement objects.
		/// <para>Returns true if parameters are equal.</para>
		/// </summary>
		/// <param name="other">The other reinforcement object.</param>
		public virtual bool Equals(UniaxialReinforcement other)
		{
			if (other != null)
				return Area == other.Area && Steel == other.Steel;

			return false;
		}

		public override bool Equals(object other)
		{
			if (other is UniaxialReinforcement reinforcement)
				return Equals(reinforcement);

			return false;
		}

		public override int GetHashCode() => (int)Math.Pow(BarDiameter, NumberOfBars);

		/// <summary>
		/// Returns true if steel parameters are equal.
		/// </summary>
		public static bool operator == (UniaxialReinforcement left, UniaxialReinforcement right) => left != null && left.Equals(right);

		/// <summary>
		/// Returns true if steel parameters are different.
		/// </summary>
		public static bool operator != (UniaxialReinforcement left, UniaxialReinforcement right) => left != null && !left.Equals(right);
	}
}
