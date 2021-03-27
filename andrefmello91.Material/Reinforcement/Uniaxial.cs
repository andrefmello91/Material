using System;
using andrefmello91.Extensions;
using andrefmello91.OnPlaneComponents;
using MathNet.Numerics;
using UnitsNet;
using UnitsNet.Units;
#nullable enable

namespace andrefmello91.Material.Reinforcement
{
	/// <summary>
	///     Uniaxial reinforcement class.
	/// </summary>
	public class UniaxialReinforcement : IUnitConvertible<UniaxialReinforcement, LengthUnit>, IApproachable<UniaxialReinforcement, Length>, IEquatable<UniaxialReinforcement>, IComparable<UniaxialReinforcement>, ICloneable<UniaxialReinforcement>
	{

		#region Fields

		/// <summary>
		///     The tolerance to consider displacements equal.
		/// </summary>
		public static readonly Length Tolerance = Length.FromMillimeters(1E-3);
		
		private Lazy<Length> _refLength;

		#endregion

		#region Properties

		/// <summary>
		///     Get reinforcement area.
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
		///     Get current force.
		/// </summary>
		public Force Force => Area * Steel.Stress;

		/// <summary>
		///     Get number of reinforcing bars.
		/// </summary>
		public int NumberOfBars { get; }

		/// <summary>
		///     Get reinforcement ratio in the cross-section.
		/// </summary>
		public double Ratio => ConcreteArea == Area.Zero ? 0 : Area / ConcreteArea;

		/// <summary>
		///     Get <see cref="Reinforcement.Steel" /> of this.
		/// </summary>
		public Steel Steel { get; }

		/// <summary>
		///     Get normal stiffness.
		/// </summary>
		public Force Stiffness => Steel.ElasticModule * Area;

		/// <summary>
		///     Get the yield force.
		/// </summary>
		public Force YieldForce => Area * Steel.YieldStress;

		/// <inheritdoc />
		public LengthUnit Unit
		{
			get => BarDiameter.Unit;
			set => ChangeUnit(value);
		}

		#endregion

		#region Constructors

		/// <param name="concreteArea">The concrete area, in <see cref="AreaUnit" /> compatible to <paramref name="unit" />.</param>
		/// <param name="unit">The <see cref="LengthUnit" /> of <paramref name="barDiameter" />.</param>
		/// <inheritdoc cref="UniaxialReinforcement" />
		public UniaxialReinforcement(int numberOfBars, double barDiameter, Steel steel, double concreteArea = 0, LengthUnit unit = LengthUnit.Millimeter)
			: this(numberOfBars, Length.From(barDiameter, unit), steel, Area.From(concreteArea, unit.GetAreaUnit()))
		{
		}

		/// <summary>
		///     Reinforcement for uniaxial calculations
		/// </summary>
		/// <param name="numberOfBars">The number of bars of reinforcement.</param>
		/// <param name="barDiameter">The bar diameter.</param>
		/// <param name="steel">The steel object.</param>
		/// <param name="concreteArea">The concrete area.</param>
		public UniaxialReinforcement(int numberOfBars, Length barDiameter, Steel steel, Area concreteArea)
		{
			NumberOfBars = numberOfBars;
			BarDiameter  = barDiameter;
			Area         = CalculateArea().ToUnit(barDiameter.Unit.GetAreaUnit());
			ConcreteArea = concreteArea;
			Steel        = steel;
		}

		#endregion

		#region Methods

		/// <summary>
		///     Calculate current force.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public Force CalculateForce(double strain) => Area * Steel.CalculateStress(strain);

		/// <inheritdoc />
		public override bool Equals(object? other) => other is UniaxialReinforcement reinforcement && Equals(reinforcement);

		/// <summary>
		///     Compare two reinforcement objects.
		///     <para>Returns true if <see cref="NumberOfBars" /> and <see cref="BarDiameter" /> are equal.</para>
		/// </summary>
		/// <param name="other">The other reinforcement object.</param>
		/// <param name="tolerance">The tolerance.</param>
		public bool EqualsNumberAndDiameter(UniaxialReinforcement? other, Length tolerance) => !(other is null) && NumberOfBars == other.NumberOfBars && BarDiameter.Approx(other.BarDiameter, tolerance);

		/// <summary>
		///     Calculate maximum value of tensile strength that can be transmitted across cracks.
		/// </summary>
		public Pressure MaximumPrincipalTensileStress() => Ratio * (Steel.YieldStress - Steel.Stress);

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
		public bool Approaches(UniaxialReinforcement? other, Length tolerance) => !(other is null) && EqualsNumberAndDiameter(other, tolerance);

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

		/// <inheritdoc />
		public void ChangeUnit(LengthUnit unit)
		{
			if (Unit == unit)
				return;

			BarDiameter = BarDiameter.ToUnit(unit);
			Area        = Area.ToUnit(unit.GetAreaUnit());
		}

		/// <inheritdoc />
		public UniaxialReinforcement Convert(LengthUnit unit) => new(NumberOfBars, BarDiameter.ToUnit(unit), Steel.Clone(), ConcreteArea.ToUnit(unit.GetAreaUnit()));

		/// <summary>
		///     Calculated reinforcement area.
		/// </summary>
		private Area CalculateArea() => 0.25 * NumberOfBars * Constants.Pi * BarDiameter * BarDiameter;

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