﻿using System;
using andrefmello91.Extensions;
using MathNet.Numerics;
using UnitsNet;
using UnitsNet.Units;
using static andrefmello91.Material.Reinforcement.UniaxialReinforcement;

#nullable enable

namespace andrefmello91.Material.Reinforcement;

/// <summary>
///     Reinforcement direction class for web reinforcement.
/// </summary>
public class WebReinforcementDirection : IUnitConvertible<LengthUnit>, IApproachable<WebReinforcementDirection, Length>, IEquatable<WebReinforcementDirection>, IComparable<WebReinforcementDirection>, ICloneable<WebReinforcementDirection>
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
	///     The cross-section area of this reinforcement direction, per stirrup.
	/// </summary>
	public Area Area
	{
		get
		{
			var unit = Unit.GetAreaUnit();

			if (unit is AreaUnit.Undefined)
				unit = AreaUnit.SquareMillimeter;

			return
				(0.25 * NumberOfLegs * Constants.Pi * BarDiameter * BarDiameter)
				.ToUnit(unit);
		}
	}

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
	public Pressure InitialStiffness => Ratio * Steel.Parameters.ElasticModule;

	/// <summary>
	///     Returns true if <see cref="Angle" /> is approximately zero.
	/// </summary>
	public bool IsHorizontal => Angle.ApproxZero(1E-3);

	/// <summary>
	///     Returns true if <see cref="Angle" /> is approximately 90 degrees.
	/// </summary>
	public bool IsVertical => Angle.Approx(Constants.PiOver2, 1E-3);

	/// <summary>
	///     The number of stirrup legs/ branches.
	/// </summary>
	public int NumberOfLegs { get; }

	/// <summary>
	///     Get reinforcement ratio.
	/// </summary>
	public double Ratio => BarSpacing.ApproxZero(Tolerance) || Width.ApproxZero(Tolerance)
		? 0
		: Area / (BarSpacing * Width);

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

	/// <inheritdoc cref="Reinforcement.Steel.Yielded" />
	public bool Yielded => Steel.Yielded;

	/// <summary>
	///     Get reinforcement yield stress (ratio multiplied by steel yield stress).
	/// </summary>
	public Pressure YieldStress => Ratio * Steel.Parameters.YieldStress;

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

	/// <summary>
	///     Reinforcement direction object for web reinforcement.
	/// </summary>
	/// <param name="barDiameter">The bar diameter.</param>
	/// <param name="barSpacing">The bar spacing.</param>
	/// <param name="steelParameters">The steel object (not null).</param>
	/// <param name="width">The width of cross-section.</param>
	/// <param name="angle">
	///     The angle (in radians) of this <see cref="WebReinforcementDirection" />, related to horizontal axis.
	///     <para><paramref name="angle" /> is positive if counterclockwise.</para>
	/// </param>
	/// <param name="numberOfLegs">The number of stirrup legs/ branches. Default: 2.</param>
	private WebReinforcementDirection(Length barDiameter, Length barSpacing, SteelParameters steelParameters, Length width, double angle, int numberOfLegs = 2)
	{
		BarDiameter  = barDiameter;
		BarSpacing   = barSpacing.ToUnit(barDiameter.Unit);
		Steel        = steelParameters;
		_width       = width.ToUnit(barDiameter.Unit);
		Angle        = angle;
		NumberOfLegs = numberOfLegs;
	}

	#endregion

	#region Methods

	/// <inheritdoc cref="From(Length, Length, SteelParameters, Length, double, int)" />
	/// <param name="unit">
	///     The unit of <paramref name="barDiameter" />, <paramref name="barSpacing" /> and
	///     <paramref name="width" />.
	/// </param>
	public static WebReinforcementDirection? From(double barDiameter, double barSpacing, SteelParameters steelParameters, double width, double angle, int numberOfLegs = 2, LengthUnit unit = LengthUnit.Millimeter) =>
		From((Length) barDiameter.As(unit), (Length) barSpacing.As(unit), steelParameters, (Length) width.As(unit), angle, numberOfLegs);

	/// <summary>
	///     Get a <see cref="WebReinforcementDirection" />.
	/// </summary>
	/// <returns>
	///     Null any of <paramref name="barDiameter" /> or <paramref name="barSpacing" /> is zero.
	/// </returns>
	/// <inheritdoc cref="WebReinforcementDirection(Length, Length, SteelParameters, Length, double, int)" select="params" />
	public static WebReinforcementDirection? From(Length barDiameter, Length barSpacing, SteelParameters steelParameters, Length width, double angle, int numberOfLegs = 2) =>
		barDiameter.ApproxZero(Tolerance) || barSpacing.ApproxZero(Tolerance)
			? null
			: new WebReinforcementDirection(barDiameter, barSpacing, steelParameters, width, angle, numberOfLegs);

	/// <summary>
	///     Set steel strain and stress.
	/// </summary>
	/// <param name="strain">Current strain.</param>
	public void Calculate(double strain) => Steel.Calculate(strain);

	/// <inheritdoc cref="IUnitConvertible{TUnit}.Convert" />
	public WebReinforcementDirection Convert(LengthUnit unit) => new(BarDiameter.ToUnit(unit), BarSpacing.ToUnit(unit), Steel.Parameters.Clone(), Width.ToUnit(unit), Angle);

	/// <summary>
	///     Calculate the crack spacing at this direction, according to Kaklauskas (2019) expression.
	/// </summary>
	public Length CrackSpacing() =>
		BarDiameter.ApproxZero(Tolerance) || Ratio.ApproxZero()
			? Length.FromMillimeters(21)
			: Length.FromMillimeters(21) + 0.155 * BarDiameter / Ratio;

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
	public virtual bool EqualsDiameterAndSpacing(WebReinforcementDirection? other, Length? tolerance = null) => other is not null && BarDiameter.Approx(other.BarDiameter, tolerance ?? Tolerance) && BarSpacing.Approx(other.BarSpacing, tolerance ?? Tolerance);

	/// <inheritdoc />
	public override int GetHashCode() => NumberOfLegs * (int) BarDiameter.Millimeters.Pow(BarSpacing.Millimeters);

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

	/// <inheritdoc />
	public bool Approaches(WebReinforcementDirection? other, Length tolerance) => other is not null && NumberOfLegs == other.NumberOfLegs && EqualsDiameterAndSpacing(other, tolerance);

	/// <inheritdoc />
	public WebReinforcementDirection Clone() => new(BarDiameter, BarSpacing, Steel.Parameters.Clone(), Width, Angle);

	/// <inheritdoc />
	public int CompareTo(WebReinforcementDirection? other) =>
		other is null || Area > other.Area || BarDiameter > other.BarDiameter || BarDiameter.Approx(other.BarDiameter, Tolerance) && BarSpacing > other.BarSpacing
			? 1
			: Equals(other)
				? 0
				: -1;

	/// <summary>
	///     Compare two reinforcement objects.
	///     <para>Returns true if parameters are equal.</para>
	/// </summary>
	/// <param name="other">The other reinforcement object.</param>
	public virtual bool Equals(WebReinforcementDirection? other) => Approaches(other, Tolerance);

	/// <inheritdoc />
	public void ChangeUnit(LengthUnit unit)
	{
		if (Unit == unit)
			return;

		BarDiameter = BarDiameter.ToUnit(unit);
		BarSpacing  = BarSpacing.ToUnit(unit);
		_width      = _width.ToUnit(unit);
	}

	IUnitConvertible<LengthUnit> IUnitConvertible<LengthUnit>.Convert(LengthUnit unit) => Convert(unit);

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