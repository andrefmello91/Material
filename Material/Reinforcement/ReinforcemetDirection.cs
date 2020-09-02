using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using UnitsNet;
using UnitsNet.Units;

namespace Material.Reinforcement
{
    /// <summary>
    /// Reinforcement direction class for web reinforcement.
    /// </summary>
    public class WebReinforcementDirection
    {
	    /// <summary>
        /// Get/set the bar diameter, in mm.
        /// </summary>
	    public double BarDiameter { get; set; }

	    /// <summary>
	    /// Get/set the bar spacing, in mm.
	    /// </summary>
	    public double BarSpacing  { get; set; }

	    /// <summary>
	    /// Get/set the cross-section width, in mm.
	    /// </summary>
	    public double Width       { get; set; }

	    /// <summary>
	    /// Get/set the steel object.
	    /// </summary>
	    public Steel  Steel       { get; set; }

        /// <summary>
        /// Reinforcement direction object for web reinforcement.
        /// </summary>
        /// <param name="barDiameter">The bar diameter (in mm).</param>
        /// <param name="barSpacing">The bar spacing (in mm).</param>
        /// <param name="steel">The steel object.</param>
        /// <param name="width">The width of cross-section (in mm).</param>
        public WebReinforcementDirection(double barDiameter, double barSpacing, Steel steel, double width)
        {
	        BarDiameter = barDiameter;
	        BarSpacing  = barSpacing;
	        Steel       = steel;
	        Width       = width;
        }

		/// <summary>
        /// Get reinforcement ratio.
        /// </summary>
        public double Ratio => CalculateRatio(BarDiameter, BarSpacing, Width);

		/// <summary>
        /// Get reinforcement stress (ratio multiplied by steel stress).
        /// </summary>
		public double Stress => Ratio * Steel.Stress;

		/// <summary>
        /// Get reinforcement yield stress (ratio multiplied by steel yield stress).
        /// </summary>
		public double YieldStress => Ratio * Steel.YieldStress;

        /// <summary>
        /// Get reinforcement capacity reserve
        /// <para>(<see cref="YieldStress"/> - <see cref="Stress"/>).</para>
        /// </summary>
        public double CapacityReserve => YieldStress - Stress;

        /// <summary>
        /// Get reinforcement stiffness (ratio multiplied by steel secant module).
        /// </summary>
        public double Stiffness => Ratio * Steel.SecantModule;

        /// <summary>
        /// Get reinforcement initial stiffness (ratio multiplied by steel elastic module).
        /// </summary>
        public double InitialStiffness => Ratio * Steel.ElasticModule;

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
		/// Return the reinforcement stress, given <paramref name="strain"/>.
		/// </summary>
		/// <param name="strain">The strain for calculating stress.</param>
		public double CalculateStress(double strain) => Ratio * Steel.CalculateStress(strain);

        /// <summary>
        /// Calculate reinforcement ratio for distributed reinforcement.
        /// </summary>
        /// <param name="barDiameter">The bar diameter.</param>
        /// <param name="barSpacing">The bar spacing.</param>
        /// <param name="width">The width of cross-section.</param>
        public static double CalculateRatio(double barDiameter, double barSpacing, double width)
        {
	        if (barDiameter == 0 || barSpacing == 0 || width == 0)
		        return 0;

	        return
		        0.5 * Constants.Pi * barDiameter * barDiameter / (barSpacing * width);
        }

        /// <summary>
        /// Return a copy of a <see cref="WebReinforcementDirection"/>.
        /// </summary>
        /// <param name="reinforcementToCopy">The <see cref="WebReinforcementDirection"/> to copy.</param>
        public static WebReinforcementDirection Copy(WebReinforcementDirection reinforcementToCopy)
        {
	        if (reinforcementToCopy is null)
		        return null;

			return
				new WebReinforcementDirection(reinforcementToCopy.BarDiameter, reinforcementToCopy.BarSpacing, reinforcementToCopy.Steel, reinforcementToCopy.Width);
        }

        /// <summary>
        /// Write string with default units (mm and MPa).
        /// </summary>
        public override string ToString() => ToString();

        /// <summary>
        /// Write string with custom units.
        /// </summary>
        /// <param name="diameterUnit">The unit of bar diameter (default: mm).</param>
        /// <param name="spacingUnit">The unit of bar spacing (default: mm).</param>
        /// <param name="strengthUnit">The unit of steel strength (default: MPa).</param>
        /// <returns></returns>
        public string ToString(LengthUnit diameterUnit = LengthUnit.Millimeter, LengthUnit spacingUnit = LengthUnit.Millimeter, PressureUnit strengthUnit = PressureUnit.Megapascal)
        {
            Length
                d = Length.FromMillimeters(BarDiameter).ToUnit(diameterUnit),
                s = Length.FromMillimeters(BarSpacing).ToUnit(spacingUnit);

            char rho = (char)Characters.Rho;
            char phi = (char)Characters.Phi;

            return
                phi + " = " + d + "\n" + 
	            "s = " + s + "\n" +
                rho + "s = " + $"{Ratio:P}" + "\n" +
                Steel.ToString(strengthUnit);
        }

        /// <summary>
        /// Compare two reinforcement objects.
        /// <para>Returns true if parameters are equal.</para>
        /// </summary>
        /// <param name="other">The other reinforcement object.</param>
        public virtual bool Equals(WebReinforcementDirection other)
        {
	        if (other != null)
		        return BarDiameter == other.BarDiameter && other.BarSpacing == BarSpacing && other.Steel == Steel;

            return false;
        }

        public override bool Equals(object other)
        {
            if (other is WebReinforcementDirection reinforcement)
                return Equals(reinforcement);

            return false;
        }

        public override int GetHashCode() => (int)Math.Pow(BarDiameter, BarSpacing);

        /// <summary>
        /// Returns true if steel parameters are equal.
        /// </summary>
        public static bool operator == (WebReinforcementDirection left, WebReinforcementDirection right) => left != null && left.Equals(right);

        /// <summary>
        /// Returns true if steel parameters are different.
        /// </summary>
        public static bool operator != (WebReinforcementDirection left, WebReinforcementDirection right) => left != null && !left.Equals(right);
    }
}
