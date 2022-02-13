using System;
using System.Diagnostics.CodeAnalysis;
using andrefmello91.Extensions;
using MathNet.Numerics;
using UnitsNet;
using UnitsNet.Units;
#nullable enable

namespace andrefmello91.Material.Reinforcement;

/// <summary>
///     Uniaxial reinforcement class.
/// </summary>
public class UniaxialReinforcement : IUniaxialMaterial, IUnitConvertible<LengthUnit>, IApproachable<UniaxialReinforcement, Length>, IEquatable<UniaxialReinforcement>, IComparable<UniaxialReinforcement>, ICloneable<UniaxialReinforcement>
{

	/// <summary>
	///     The tolerance to consider displacements equal.
	/// </summary>
	public static readonly Length Tolerance = Length.FromMillimeters(1E-3);

	/// <summary>
	///     Get bar diameter.
	/// </summary>
	public Length BarDiameter { get; private set; }

	/// <summary>
	///     Get/set concrete area.
	/// </summary>
	public Area ConcreteArea { get; set; }

	/// <summary>
	///     Get number of reinforcing bars.
	/// </summary>
	public int NumberOfBars { get; }

	/// <summary>
	///     Get reinforcement ratio in the cross-section.
	/// </summary>
	public double Ratio => ConcreteArea == Area.Zero
		? 0
		: Area / ConcreteArea;

	/// <summary>
	///     Get <see cref="Reinforcement.Steel" /> of this.
	/// </summary>
	public Steel Steel { get; }

	/// <summary>
	///     Get normal stiffness.
	/// </summary>
	public Force Stiffness => Steel.Parameters.ElasticModule * Area;

	/// <inheritdoc cref="Reinforcement.Steel.Yielded" />
	public bool Yielded => Steel.Yielded;

	/// <summary>
	///     Get the yield force.
	/// </summary>
	public Force YieldForce => Area * Steel.Parameters.YieldStress;

	/// <summary>
	///     The reinforcement area.
	/// </summary>
	public Area Area { get; private set; }

	/// <summary>
	///     The current force.
	/// </summary>
	public Force Force => Area * Steel.Stress;

	/// <inheritdoc />
	public double Strain => Steel.Strain;

	/// <inheritdoc />
	public Pressure Stress => Steel.Stress;

	/// <inheritdoc />
	public LengthUnit Unit
	{
		get => BarDiameter.Unit;
		set => ChangeUnit(value);
	}

	/// <param name="concreteArea">The concrete area, in <see cref="AreaUnit" /> compatible to <paramref name="unit" />.</param>
	/// <param name="unit">The <see cref="LengthUnit" /> of <paramref name="barDiameter" />.</param>
	/// <inheritdoc cref="UniaxialReinforcement" />
	public UniaxialReinforcement(int numberOfBars, double barDiameter, [NotNull] Steel steel, double concreteArea = 0, LengthUnit unit = LengthUnit.Millimeter)
		: this(numberOfBars, (Length) barDiameter.As(unit), steel, (Area) concreteArea.As(unit.GetAreaUnit()))
	{
	}

	/// <summary>
	///     Reinforcement for uniaxial calculations
	/// </summary>
	/// <param name="numberOfBars">The number of bars of reinforcement.</param>
	/// <param name="barDiameter">The bar diameter.</param>
	/// <param name="steel">The steel object.</param>
	/// <param name="concreteArea">The concrete area.</param>
	public UniaxialReinforcement(int numberOfBars, Length barDiameter, [NotNull] Steel steel, Area concreteArea)
	{
		NumberOfBars = numberOfBars;
		BarDiameter  = barDiameter;
		Area         = CalculateArea().ToUnit(barDiameter.Unit.GetAreaUnit());
		ConcreteArea = concreteArea;
		Steel        = steel;
	}

	/// <inheritdoc cref="IUnitConvertible{TUnit}.Convert" />
	public UniaxialReinforcement Convert(LengthUnit unit) => new(NumberOfBars, BarDiameter.ToUnit(unit), Steel.Clone(), ConcreteArea.ToUnit(unit.GetAreaUnit()));

	/// <inheritdoc />
	public override bool Equals(object? other) => other is UniaxialReinforcement reinforcement && Equals(reinforcement);

	/// <summary>
	///     Compare two reinforcement objects.
	///     <para>Returns true if <see cref="NumberOfBars" /> and <see cref="BarDiameter" /> are equal.</para>
	/// </summary>
	/// <param name="other">The other reinforcement object.</param>
	/// <param name="tolerance">The tolerance.</param>
	public bool EqualsNumberAndDiameter(UniaxialReinforcement? other, Length tolerance) => other is not null && NumberOfBars == other.NumberOfBars && BarDiameter.Approx(other.BarDiameter, tolerance);

	/// <inheritdoc />
	public override int GetHashCode() => (int) BarDiameter.Millimeters.Pow(NumberOfBars);

	/// <summary>
	///     Calculate maximum value of tensile strength that can be transmitted across cracks.
	/// </summary>
	public Pressure MaximumPrincipalTensileStress() => Ratio * (Steel.Parameters.YieldStress - Steel.Stress);

	/// <inheritdoc />
	public override string ToString()
	{
		var phi = (char) Characters.Phi;

		return
			$"Reinforcement: {NumberOfBars} {phi} {BarDiameter} ({Area})\n\n"
			+ Steel;
	}

	/// <summary>
	///     Calculated reinforcement area.
	/// </summary>
	private Area CalculateArea() => 0.25 * NumberOfBars * Constants.Pi * BarDiameter * BarDiameter;

	/// <inheritdoc />
	public bool Approaches(UniaxialReinforcement? other, Length tolerance) => other is not null && EqualsNumberAndDiameter(other, tolerance);

	/// <inheritdoc />
	public UniaxialReinforcement Clone() => new(NumberOfBars, BarDiameter, Steel.Clone(), ConcreteArea);

	/// <inheritdoc />
	public int CompareTo(UniaxialReinforcement? other) =>
		other is null || Area > other.Area
			? 1
			: Area == other.Area
				? 0
				: -1;

	/// <summary>
	///     Compare two reinforcement objects.
	///     <para>Returns true if parameters are equal.</para>
	/// </summary>
	/// <param name="other">The other reinforcement object.</param>
	public virtual bool Equals(UniaxialReinforcement? other) => Approaches(other, Tolerance);

	/// <summary>
	///     Set steel strain and stress.
	/// </summary>
	/// <param name="strain">Current strain.</param>
	public void Calculate(double strain) => Steel.Calculate(strain);

	/// <inheritdoc />
	public void ChangeUnit(LengthUnit unit)
	{
		if (Unit == unit)
			return;

		BarDiameter = BarDiameter.ToUnit(unit);
		Area        = Area.ToUnit(unit.GetAreaUnit());
	}

	IUnitConvertible<LengthUnit> IUnitConvertible<LengthUnit>.Convert(LengthUnit unit) => Convert(unit);

	/// <summary>
	///     Returns true if steel parameters are equal.
	/// </summary>
	public static bool operator ==(UniaxialReinforcement? left, UniaxialReinforcement? right) => left.IsEqualTo(right);

	/// <summary>
	///     Returns true if steel parameters are different.
	/// </summary>
	public static bool operator !=(UniaxialReinforcement? left, UniaxialReinforcement? right) => left.IsNotEqualTo(right);
}