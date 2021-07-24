using System;
using System.Diagnostics.CodeAnalysis;
using andrefmello91.Extensions;
using MathNet.Numerics;
using UnitsNet;
using UnitsNet.Units;
#nullable enable

namespace andrefmello91.Material.Reinforcement
{
	/// <summary>
	///     Uniaxial reinforcement class.
	/// </summary>
	public class UniaxialReinforcement : IUniaxialMaterial, IUnitConvertible<LengthUnit>, IApproachable<UniaxialReinforcement, Length>, IEquatable<UniaxialReinforcement>, IComparable<UniaxialReinforcement>, ICloneable<UniaxialReinforcement>
	{

		#region Fields

		/// <summary>
		///     The tolerance to consider displacements equal.
		/// </summary>
		public static readonly Length Tolerance = Length.FromMillimeters(1E-3);

		#endregion

		#region Properties

		/// <summary>
		///     The reinforcement area.
		/// </summary>
		public Area Area { get; private set; }

		/// <summary>
		///     Get bar diameter.
		/// </summary>
		public Length BarDiameter { get; private set; }

		/// <summary>
		///     Get/set concrete area.
		/// </summary>
		public Area ConcreteArea { get; set; }

		/// <summary>
		///     The current force.
		/// </summary>
		public Force Force => Area * Steel.Stress;

		/// <inheritdoc />
		public double Strain => Steel.Strain;

		/// <inheritdoc />
		public Pressure Stress => Steel.Stress;

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

		/// <summary>
		///     Get the yield force.
		/// </summary>
		public Force YieldForce => Area * Steel.Parameters.YieldStress;

		#region Interface Implementations

		/// <inheritdoc />
		public LengthUnit Unit
		{
			get => BarDiameter.Unit;
			set => ChangeUnit(value);
		}

		#endregion

		#endregion

		#region Constructors

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

		#endregion

		#region Methods

		/// <inheritdoc cref="IUnitConvertible{TUnit}.Convert" />
		public UniaxialReinforcement Convert(LengthUnit unit) => new(NumberOfBars, BarDiameter.ToUnit(unit), Steel.Clone(), ConcreteArea.ToUnit(unit.GetAreaUnit()));

		/// <summary>
		///     Compare two reinforcement objects.
		///     <para>Returns true if <see cref="NumberOfBars" /> and <see cref="BarDiameter" /> are equal.</para>
		/// </summary>
		/// <param name="other">The other reinforcement object.</param>
		/// <param name="tolerance">The tolerance.</param>
		public bool EqualsNumberAndDiameter(UniaxialReinforcement? other, Length tolerance) => other is not null && NumberOfBars == other.NumberOfBars && BarDiameter.Approx(other.BarDiameter, tolerance);

		/// <summary>
		///     Calculate maximum value of tensile strength that can be transmitted across cracks.
		/// </summary>
		public Pressure MaximumPrincipalTensileStress() => Ratio * (Steel.Parameters.YieldStress - Steel.Stress);

		/// <summary>
		///     Set steel strain and stress.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void Calculate(double strain) => Steel.Calculate(strain);

		/// <summary>
		///     Calculated reinforcement area.
		/// </summary>
		private Area CalculateArea() => 0.25 * NumberOfBars * Constants.Pi * BarDiameter * BarDiameter;

		#region Interface Implementations

		/// <inheritdoc />
		public bool Approaches(UniaxialReinforcement? other, Length tolerance) => other is not null && EqualsNumberAndDiameter(other, tolerance);

		/// <inheritdoc />
		public void ChangeUnit(LengthUnit unit)
		{
			if (Unit == unit)
				return;

			BarDiameter = BarDiameter.ToUnit(unit);
			Area        = Area.ToUnit(unit.GetAreaUnit());
		}

		/// <inheritdoc />
		public UniaxialReinforcement Clone() => new(NumberOfBars, BarDiameter, Steel.Clone(), ConcreteArea);

		/// <inheritdoc />
		public int CompareTo(UniaxialReinforcement? other) =>
			other is null || Area > other.Area
				? 1
				: Area == other.Area
					? 0
					: -1;

		IUnitConvertible<LengthUnit> IUnitConvertible<LengthUnit>.Convert(LengthUnit unit) => Convert(unit);

		/// <summary>
		///     Compare two reinforcement objects.
		///     <para>Returns true if parameters are equal.</para>
		/// </summary>
		/// <param name="other">The other reinforcement object.</param>
		public virtual bool Equals(UniaxialReinforcement? other) => Approaches(other, Tolerance);

		#endregion

		#region Object override

		/// <inheritdoc />
		public override bool Equals(object? other) => other is UniaxialReinforcement reinforcement && Equals(reinforcement);

		/// <inheritdoc />
		public override int GetHashCode() => (int) BarDiameter.Millimeters.Pow(NumberOfBars);

		/// <inheritdoc />
		public override string ToString()
		{
			var phi = (char) Characters.Phi;

			return
				$"Reinforcement: {NumberOfBars} {phi} {BarDiameter} ({Area})\n\n"
				+ Steel;
		}

		#endregion

		#endregion

		#region Operators

		/// <summary>
		///     Returns true if steel parameters are equal.
		/// </summary>
		public static bool operator ==(UniaxialReinforcement? left, UniaxialReinforcement? right) => left.IsEqualTo(right);

		/// <summary>
		///     Returns true if steel parameters are different.
		/// </summary>
		public static bool operator !=(UniaxialReinforcement? left, UniaxialReinforcement? right) => left.IsNotEqualTo(right);

		#endregion

	}
}