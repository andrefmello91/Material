using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions.Number;
using MathNet.Numerics;
using UnitsNet;
using UnitsNet.Units;

namespace Material.Reinforcement
{
    /// <summary>
    /// Reinforcement direction class for web reinforcement.
    /// </summary>
    public class WebReinforcementDirection : IEquatable<WebReinforcementDirection>
    {
		// Auxiliary fields
		private Length _phi, _s, _w;
		private double? _ps;

		/// <summary>
        /// Get the <see cref="LengthUnit"/> that this was constructed with.
        /// </summary>
		public LengthUnit Unit => _phi.Unit;

		/// <summary>
		/// Get/set the bar diameter, in mm.
		/// </summary>
		public double BarDiameter
		{
			get => _phi.Millimeters; 
			set => _phi = Length.FromMillimeters(value).ToUnit(Unit);
		}

		/// <summary>
		/// Get/set the bar spacing, in mm.
		/// </summary>
		public double BarSpacing
		{
			get => _s.Millimeters; 
			set => _s = Length.FromMillimeters(value).ToUnit(Unit);
		}

		/// <summary>
		/// Get/set the cross-section width, in mm.
		/// </summary>
		public double Width
		{
			get => _w.Millimeters;
			set => _w = Length.FromMillimeters(value).ToUnit(Unit);
		}

        /// <summary>
        /// Get the steel object.
        /// </summary>
        public Steel Steel { get; }

        /// <summary>
        /// Get the angle (in radians) of this <see cref="WebReinforcementDirection"/>, related to horizontal axis.
        /// <para><see cref="Angle"/> is positive if counterclockwise.</para>
        /// </summary>
        public double Angle { get; }

		/// <summary>
        /// Returns true if <see cref="Angle"/> is approximately zero.
        /// </summary>
        public bool IsHorizontal => Angle.ApproxZero();

		/// <summary>
        /// Returns true if <see cref="Angle"/> is approximately 90 deg.
        /// </summary>
        public bool IsVertical => Angle.Approx(Constants.PiOver2);

        /// <summary>
        /// Reinforcement direction object for web reinforcement.
        /// </summary>
        /// <param name="barDiameter">The bar diameter (in mm).</param>
        /// <param name="barSpacing">The bar spacing (in mm).</param>
        /// <param name="steel">The steel object.</param>
        /// <param name="width">The width of cross-section (in mm).</param>
        /// <param name="angle">The angle (in radians) of this <see cref="WebReinforcementDirection"/>, related to horizontal axis.
        /// <para><paramref name="angle"/> is positive if counterclockwise.</para></param>
        public WebReinforcementDirection(double barDiameter, double barSpacing, Steel steel, double width, double angle)
			: this (Length.FromMillimeters(barDiameter), Length.FromMillimeters(barSpacing), steel, Length.FromMillimeters(width), angle)
        {
        }

        /// <summary>
        /// Reinforcement direction object for web reinforcement.
        /// </summary>
        /// <param name="barDiameter">The bar diameter.</param>
        /// <param name="barSpacing">The bar spacing.</param>
        /// <param name="steel">The steel object.</param>
        /// <param name="width">The width of cross-section.</param>
        /// <param name="angle">The angle (in radians) of this <see cref="WebReinforcementDirection"/>, related to horizontal axis.
        /// <para><paramref name="angle"/> is positive if counterclockwise.</para></param>
        public WebReinforcementDirection(Length barDiameter, Length barSpacing, Steel steel, Length width, double angle)
        {
	        _phi  = barDiameter;
	        _s    = barSpacing;
	        Steel = steel;
	        _w    = width;
	        Angle = angle;
        }

		/// <summary>
        /// Get reinforcement ratio.
        /// </summary>
        public double Ratio => _ps ?? CalculateRatio();

		/// <summary>
        /// Get reinforcement stress (ratio multiplied by steel stress).
        /// </summary>
		public double Stress => Ratio * Steel.Stress;

		/// <summary>
        /// Get reinforcement yield stress (ratio multiplied by steel yield stress).
        /// </summary>
		public double YieldStress => Ratio * Steel.YieldStress;

        /// <summary>
        /// Get reinforcement capacity reserve for tension.
        /// <para>(<see cref="YieldStress"/> - <see cref="Stress"/>).</para>
        /// </summary>
        public double CapacityReserve => Stress > 0 ? YieldStress - Stress : YieldStress;

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
		/// Return a copy of this <see cref="WebReinforcementDirection"/>.
		/// </summary>
		public WebReinforcementDirection Copy() => new WebReinforcementDirection(BarDiameter, BarSpacing, Steel.Copy(), Width, Angle);

        /// <summary>
        /// Read the <see cref="WebReinforcementDirection"/>.
        /// <para>Returns null if <paramref name="barDiameter"/> or <paramref name="barSpacing"/> are zero, or if <paramref name="steel"/> is null.</para>
        /// </summary>
        /// <param name="barDiameter">The bar diameter (in mm)</param>
        /// <param name="barSpacing">The bar spacing (in mm).</param>
        /// <param name="steel">The steel object.</param>
        /// <param name="width">The width (in mm) of cross-section.</param>
        /// <param name="angle">The angle (in radians) of this <see cref="WebReinforcementDirection"/>, related to horizontal axis.
        /// <para><paramref name="angle"/> is positive if counterclockwise.</para></param>
        public static WebReinforcementDirection Read(double barDiameter, double barSpacing, Steel steel, double width, double angle)
        {
            if (steel is null || barDiameter.ApproxZero() || barSpacing.ApproxZero())
                return null;

            return
                new WebReinforcementDirection(barDiameter, barSpacing, steel, width, angle);
        }

        /// <summary>
        /// Read the <see cref="WebReinforcementDirection"/>.
        /// <para>Returns null if <paramref name="barDiameter"/> or <paramref name="barSpacing"/> are zero, or if <paramref name="steel"/> is null.</para>
        /// </summary>
        /// <param name="barDiameter">The bar diameter.</param>
        /// <param name="barSpacing">The bar spacing .</param>
        /// <param name="steel">The steel object.</param>
        /// <param name="width">The width of cross-section.</param>
        /// <param name="angle">The angle (in radians) of this <see cref="WebReinforcementDirection"/>, related to horizontal axis.
        /// <para><paramref name="angle"/> is positive if counterclockwise.</para></param>
        public static WebReinforcementDirection Read(Length barDiameter, Length barSpacing, Steel steel, Length width, double angle) => Read(barDiameter.Millimeters, barSpacing.Millimeters, steel, width.Millimeters, angle);

        /// <summary>
        /// Calculate reinforcement ratio for distributed reinforcement.
        /// </summary>
        private double CalculateRatio()
        {
	        if (BarDiameter.ApproxZero() || BarSpacing.ApproxZero() || Width.ApproxZero())
		        _ps = 0;
			else
		       _ps = 0.5 * Constants.Pi * BarDiameter * BarDiameter / (BarSpacing * Width);

	        return _ps.Value;
        }

        /// <summary>
        /// Write string with default units (mm and MPa).
        /// </summary>
        public override string ToString() 
        {
            char rho = (char)Characters.Rho;
            char phi = (char)Characters.Phi;

            return
                $"{phi} = {_phi}\n" + 
	            $"s = {_s}\n" +
                $"{rho}s = {Ratio:P}\n" +
				$"Angle = {Angle.ToDegree():0.00} deg" +
                Steel;
        }

        /// <summary>
        /// Compare two reinforcement objects.
        /// <para>Returns true if parameters are equal.</para>
        /// </summary>
        /// <param name="other">The other reinforcement object.</param>
        public virtual bool Equals(WebReinforcementDirection other) => !(other is null) && (BarDiameter == other.BarDiameter && BarSpacing == other.BarSpacing && Steel == other.Steel);

        public override bool Equals(object other) => other is WebReinforcementDirection reinforcement && Equals(reinforcement);

        public override int GetHashCode() => (int)BarDiameter.Pow(BarSpacing);

        /// <summary>
        /// Returns true if steel parameters are equal.
        /// </summary>
        public static bool operator == (WebReinforcementDirection left, WebReinforcementDirection right) => !(left is null) && left.Equals(right);

        /// <summary>
        /// Returns true if steel parameters are different.
        /// </summary>
        public static bool operator != (WebReinforcementDirection left, WebReinforcementDirection right) => !(left is null) && !left.Equals(right);
    }
}
