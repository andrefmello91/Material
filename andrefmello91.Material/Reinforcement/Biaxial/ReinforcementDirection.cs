using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using andrefmello91.Extensions;
using MathNet.Numerics;
using UnitsNet;
using UnitsNet.Units;
using static andrefmello91.Material.Reinforcement.UniaxialReinforcement;

#nullable enable

namespace andrefmello91.Material.Reinforcement
{
	/// <summary>
	///     Reinforcement direction class for web reinforcement.
	/// </summary>
	public class WebReinforcementDirection : IUnitConvertible<WebReinforcementDirection, LengthUnit>, IApproachable<WebReinforcementDirection, Length>, IEquatable<WebReinforcementDirection>, IComparable<WebReinforcementDirection>, ICloneable<WebReinforcementDirection>
	{

		#region Fields

		private Length _width;

		#endregion

		#region Properties

		/// <summary>
		///     Get the angle (in radians) of this <see cref="WebReinforcementDirection" />, related to horizontal axis.
		/// </summary>
		/// <remarks>The angle is positive if counterclockwise.</remarks>
		public double Angle { get; }

		/// <summary>
		///     Get the bar diameter.
		/// </summary>
		public Length BarDiameter { get; private set; }

		/// <summary>
		///     Get the bar spacing.
		/// </summary>
		public Length BarSpacing { get; private set; }

		/// <summary>
		///     Get reinforcement capacity reserve for tension.
		///     <para>(<see cref="YieldStress" /> - <see cref="Stress" />).</para>
		/// </summary>
		public Pressure CapacityReserve => YieldStress - Stress.Abs();

		/// <summary>
		///     Get reinforcement initial stiffness (ratio multiplied by steel elastic module).
		/// </summary>
		public Pressure InitialStiffness => Ratio * Steel.ElasticModule;

		/// <summary>
		///     Returns true if <see cref="Angle" /> is approximately zero.
		/// </summary>
		public bool IsHorizontal => Angle.ApproxZero(1E-3);

		/// <summary>
		///     Returns true if <see cref="Angle" /> is approximately 90 degrees.
		/// </summary>
		public bool IsVertical => Angle.Approx(Constants.PiOver2, 1E-3);

		/// <summary>
		///     Get reinforcement ratio.
		/// </summary>
		public double Ratio => CalculateRatio(this);

		/// <summary>
		///     Get the steel object.
		/// </summary>
		public Steel Steel { get; }

		/// <summary>
		///     Get reinforcement stiffness (ratio multiplied by steel secant module).
		/// </summary>
		public Pressure Stiffness => Ratio * Steel.SecantModule;

		/// <summary>
		///     Get reinforcement stress (ratio multiplied by steel stress).
		/// </summary>
		public Pressure Stress => Ratio * Steel.Stress;

		/// <summary>
		///     Get/set the cross-section width.
		/// </summary>
		public Length Width
		{
			get => _width;
			set => _width = value.ToUnit(Unit);
		}

		/// <summary>
		///     Get reinforcement yield stress (ratio multiplied by steel yield stress).
		/// </summary>
		public Pressure YieldStress => Ratio * Steel.YieldStress;

		/// <summary>
		///     Get/set the <see cref="LengthUnit" /> of <see cref="BarDiameter" />, <see cref="BarSpacing" /> and
		///     <see cref="Width" />.
		/// </summary>
		public LengthUnit Unit
		{
			get => BarDiameter.Unit;
			set => ChangeUnit(value);
		}

		#endregion

		#region Constructors

		/// <inheritdoc cref="WebReinforcementDirection" />
		/// <param name="unit">
		///     The <see cref="LengthUnit" /> of <paramref name="barDiameter" />, <paramref name="barSpacing" /> and
		///     <paramref name="width" />.
		/// </param>
		public WebReinforcementDirection(double barDiameter, double barSpacing, [NotNull] Steel steel, double width, double angle, LengthUnit unit = LengthUnit.Millimeter)
			: this((Length) barDiameter.As(unit), (Length) barSpacing.As(unit), steel, (Length) width.As(unit), angle)
		{
		}

		/// <summary>
		///     Reinforcement direction object for web reinforcement.
		/// </summary>
		/// <param name="barDiameter">The bar diameter.</param>
		/// <param name="barSpacing">The bar spacing.</param>
		/// <param name="steel">The steel object (not null).</param>
		/// <param name="width">The width of cross-section.</param>
		/// <param name="angle">
		///     The angle (in radians) of this <see cref="WebReinforcementDirection" />, related to horizontal axis.
		///     <para><paramref name="angle" /> is positive if counterclockwise.</para>
		/// </param>
		public WebReinforcementDirection(Length barDiameter, Length barSpacing, [NotNull] Steel steel, Length width, double angle)
		{
			BarDiameter = barDiameter;
			BarSpacing  = barSpacing.ToUnit(barDiameter.Unit);
			Steel       = steel;
			_width      = width.ToUnit(barDiameter.Unit);
			Angle       = angle;
		}

		#endregion

		#region Methods

		/// <summary>
		///     Calculate reinforcement ratio for distributed reinforcement.
		/// </summary>
		public static double CalculateRatio(WebReinforcementDirection? direction) =>
			direction is null || direction.BarDiameter.ApproxZero(Tolerance) || direction.BarSpacing.ApproxZero(Tolerance) || direction.Width.ApproxZero(Tolerance)
				? 0
				: 0.5 * Constants.Pi * direction.BarDiameter * direction.BarDiameter / (direction.BarSpacing * direction.Width);

		/// <summary>
		///     Calculate the crack spacing at <paramref name="direction" />, according to Kaklauskas (2019) expression.
		/// </summary>
		/// <inheritdoc cref="CrackSpacing()" />
		/// <param name="direction">The <see cref="WebReinforcementDirection" />.</param>
		public static Length CrackSpacing(WebReinforcementDirection? direction) =>
			direction is null || direction.BarDiameter.ApproxZero(Tolerance) || direction.Ratio.ApproxZero()
				? Length.FromMillimeters(21)
				: Length.FromMillimeters(21) + 0.155 * direction.BarDiameter / direction.Ratio;

		/// <inheritdoc cref="GetDirection(Length, Length, Reinforcement.Steel?, Length, double)" />
		/// <inheritdoc cref="WebReinforcementDirection(double, double, Reinforcement.Steel, double, double, LengthUnit)" select="params" />
		public static WebReinforcementDirection? GetDirection(double barDiameter, double barSpacing, Steel? steel, double width, double angle, LengthUnit unit = LengthUnit.Millimeter) =>
			GetDirection((Length) barDiameter.As(unit), (Length) barSpacing.As(unit), steel, (Length) width.As(unit), angle);

		/// <summary>
		///     Get a <see cref="WebReinforcementDirection" />.
		/// </summary>
		/// <returns>
		///     Null if <paramref name="barDiameter" /> or <paramref name="barSpacing" /> are zero, or if
		///     <paramref name="steel" /> is null.
		/// </returns>
		/// <inheritdoc cref="WebReinforcementDirection(Length, Length, Reinforcement.Steel, Length, double)" select="params"/>
		public static WebReinforcementDirection? GetDirection(Length barDiameter, Length barSpacing, Steel? steel, Length width, double angle) =>
			steel is null || barDiameter.ApproxZero(Tolerance) || barSpacing.ApproxZero(Tolerance)
				? null
				: new WebReinforcementDirection(barDiameter, barSpacing, steel, width, angle);

		/// <summary>
		///     Return the reinforcement stress, given <paramref name="strain" />.
		/// </summary>
		/// <param name="strain">The strain for calculating stress.</param>
		public Pressure CalculateStress(double strain) => Ratio * Steel.CalculateStress(strain);

		/// <summary>
		///     Calculate the crack spacing at this direction.
		/// </summary>
		/// <remarks>
		///		According to Kaklauskas (2019) expression:
		///		<code>
		///			sm = 21 mm + 0.155 BarDiameter / Ratio
		///		</code>
		/// </remarks>
		public Length CrackSpacing() => CrackSpacing(this);
		
		/// <inheritdoc />
		public override bool Equals(object? other) => other is WebReinforcementDirection reinforcement && Equals(reinforcement);

		/// <summary>
		///     Compare two reinforcement objects.
		/// </summary>
		/// <remarks>
		///     Returns true if <see cref="BarDiameter" /> and <see cref="BarSpacing" /> are nearly equal.
		/// </remarks>
		/// <param name="other">The other reinforcement object.</param>
		/// <param name="tolerance">The tolerance to consider values being equal.</param>
		public virtual bool EqualsDiameterAndSpacing(WebReinforcementDirection? other, Length? tolerance = null) => !(other is null) && BarDiameter.Approx(other.BarDiameter, tolerance ?? Tolerance) && BarSpacing.Approx(other.BarSpacing, tolerance ?? Tolerance);

		/// <summary>
		///     Set steel strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStrain(double strain) => Steel.SetStrain(strain);

		/// <summary>
		///     Set steel strain and stress.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStrainAndStress(double strain) => Steel.SetStrainAndStress(strain);

		/// <summary>
		///     Set steel stress, given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStress(double strain) => Steel.SetStress(strain);

		/// <inheritdoc />
		public bool Approaches(WebReinforcementDirection? other, Length tolerance) => !(other is null) && EqualsDiameterAndSpacing(other, tolerance);

		/// <inheritdoc />
		public WebReinforcementDirection Clone() => new(BarDiameter, BarSpacing, Steel.Clone(), Width, Angle);

		/// <inheritdoc />
		public int CompareTo(WebReinforcementDirection? other) =>
			other is null || BarDiameter > other.BarDiameter || BarDiameter.Approx(other.BarDiameter, Tolerance) && BarSpacing > other.BarSpacing
				? 1
				: BarDiameter.Approx(other.BarDiameter, Tolerance) && BarSpacing.Approx(other.BarSpacing, Tolerance)
					? 0
					: -1;

		/// <summary>
		///     Compare two reinforcement objects.
		///     <para>Returns true if parameters are equal.</para>
		/// </summary>
		/// <param name="other">The other reinforcement object.</param>
		public virtual bool Equals(WebReinforcementDirection? other) => !Approaches(other, Tolerance);

		/// <inheritdoc />
		public void ChangeUnit(LengthUnit unit)
		{
			if (Unit == unit)
				return;

			BarDiameter = BarDiameter.ToUnit(unit);
			BarSpacing  = BarSpacing.ToUnit(unit);
			_width      = _width.ToUnit(unit);
		}

		/// <inheritdoc />
		public WebReinforcementDirection Convert(LengthUnit unit) => new(BarDiameter.ToUnit(unit), BarSpacing.ToUnit(unit), Steel.Clone(), Width.ToUnit(unit), Angle);

		/// <inheritdoc />
		public override int GetHashCode() => (int) BarDiameter.Millimeters.Pow(BarSpacing.Millimeters);

		/// <inheritdoc />
		public override string ToString()
		{
			var rho = (char) Characters.Rho;
			var phi = (char) Characters.Phi;

			return
				$"{phi} = {BarDiameter}\n" +
				$"s = {BarSpacing}\n" +
				$"{rho}s = {Ratio:P}\n" +
				$"Angle = {Angle.ToDegree():0.00} deg\n" +
				Steel;
		}

		#endregion

		#region Operators

		/// <summary>
		///     Returns true if steel parameters are equal.
		/// </summary>
		public static bool operator ==(WebReinforcementDirection? left, WebReinforcementDirection? right) => left.IsEqualTo(right);

		/// <summary>
		///     Returns true if steel parameters are different.
		/// </summary>
		public static bool operator !=(WebReinforcementDirection? left, WebReinforcementDirection? right) => left.IsNotEqualTo(right);

		#endregion

	}
}