using System;
using Extensions;
using MathNet.Numerics;
using UnitsNet;
using UnitsNet.Units;

#nullable enable

namespace Material.Reinforcement.Uniaxial
{
	/// <summary>
	///     Uniaxial reinforcement class.
	/// </summary>
	public class UniaxialReinforcement : IUnitConvertible<UniaxialReinforcement, LengthUnit>, IApproachable<UniaxialReinforcement, Length>, IEquatable<UniaxialReinforcement>, IComparable<UniaxialReinforcement>, ICloneable<UniaxialReinforcement>
	{
		#region Fields

		/// <summary>
		///     Concrete area.
		/// </summary>
		private readonly Area _concreteArea;

		/// <summary>
		///     The tolerance to consider displacements equal.
		/// </summary>
		public static readonly Length Tolerance = Length.FromMillimeters(1E-3);

		#endregion

		#region Properties

		public LengthUnit Unit
		{
			get => BarDiameter.Unit;
			set => ChangeUnit(value);
		}

		/// <summary>
		///     Get reinforcement area.
		/// </summary>
		public Area Area { get; private set; }

		/// <summary>
		///     Get bar diameter.
		/// </summary>
		public Length BarDiameter { get; private set; }

		/// <summary>
		///     Get current force.
		/// </summary>
		public Force Force => Area * Steel.Stress;

		/// <summary>
		///     Get number of reinforcing bars.
		/// </summary>
		public int NumberOfBars  { get; }

		/// <summary>
		///     Get reinforcement ratio in the cross-section.
		/// </summary>
		public double Ratio => _concreteArea == Area.Zero ? 0 : Area / _concreteArea;

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

		#endregion

		#region Constructors

		/// <param name="concreteArea">The concrete area, in <see cref="AreaUnit" /> compatible to <paramref name="unit" />.</param>
		/// <param name="unit">The <see cref="LengthUnit" /> of <paramref name="barDiameter" />.</param>
		/// <inheritdoc cref="UniaxialReinforcement(int, Length, Steel, Area)" />
		public UniaxialReinforcement(int numberOfBars, double barDiameter, Steel steel, double concreteArea = 0, LengthUnit unit = LengthUnit.Millimeter)
			: this (numberOfBars, Length.From(barDiameter, unit), steel, Area.From(concreteArea, unit.GetAreaUnit()))
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
			NumberOfBars  = numberOfBars;
			BarDiameter   = barDiameter;
			Area          = CalculateArea().ToUnit(barDiameter.Unit.GetAreaUnit());
			_concreteArea = concreteArea;
			Steel         = steel;
		}

		#endregion

		#region

		/// <summary>
		///     Calculate current force.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public Force CalculateForce(double strain) => Area * Steel.CalculateStress(strain);

		/// <summary>
		///     Calculate tension stiffening coefficient (for DSFM).
		/// </summary>
		public double TensionStiffeningCoefficient() => 0.25 * BarDiameter.Millimeters / Ratio;

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
		///     Set steel stress, given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStress(double strain) => Steel.SetStress(strain);

		/// <summary>
		///     Set steel strain and stress.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStrainAndStress(double strain) => Steel.SetStrainAndStress(strain);

		public UniaxialReinforcement Clone() => new UniaxialReinforcement(NumberOfBars, BarDiameter, Steel.Clone(), _concreteArea);

		public bool Approaches(UniaxialReinforcement? other, Length tolerance) => !(other is null) && EqualsNumberAndDiameter(other, tolerance) && Steel == other.Steel;


		public void ChangeUnit(LengthUnit unit)
		{
			if (Unit == unit)
				return;

			BarDiameter = BarDiameter.ToUnit(unit);
			Area        = Area.ToUnit(unit.GetAreaUnit());
		}

		public UniaxialReinforcement Convert(LengthUnit unit) => new UniaxialReinforcement(NumberOfBars, BarDiameter.ToUnit(unit), Steel.Clone(), _concreteArea.ToUnit(unit.GetAreaUnit()));

		/// <summary>
		///     Calculated reinforcement area.
		/// </summary>
		private Area CalculateArea() => 0.25 * NumberOfBars * Constants.Pi * BarDiameter * BarDiameter;

		public int CompareTo(UniaxialReinforcement? other) =>
			other is null || Area > other.Area
				? 1
				: Area == other.Area
					?  0
					: -1;

		/// <summary>
		///     Compare two reinforcement objects.
		///     <para>Returns true if parameters are equal.</para>
		/// </summary>
		/// <param name="other">The other reinforcement object.</param>
		public virtual bool Equals(UniaxialReinforcement? other) => Approaches(other, Tolerance);

		public override string ToString()
		{
			var phi = (char) Characters.Phi;

			return
				$"Reinforcement: {NumberOfBars} {phi} {BarDiameter} ({Area})\n\n"
				+ Steel;
		}

		/// <summary>
		///     Compare two reinforcement objects.
		///     <para>Returns true if <see cref="NumberOfBars" /> and <see cref="BarDiameter" /> are equal.</para>
		/// </summary>
		/// <param name="other">The other reinforcement object.</param>
		public virtual bool EqualsNumberAndDiameter(UniaxialReinforcement? other, Length tolerance) => !(other is null) && NumberOfBars == other.NumberOfBars && BarDiameter.Approx(other.BarDiameter, tolerance);

		public override bool Equals(object? other) => other is UniaxialReinforcement reinforcement && Equals(reinforcement);

		public override int GetHashCode() => (int) BarDiameter.Millimeters.Pow(NumberOfBars);

		#endregion

		#region Operators

		/// <summary>
		///     Returns true if steel parameters are equal.
		/// </summary>
		public static bool operator == (UniaxialReinforcement left, UniaxialReinforcement right) => !(left is null) && left.Equals(right);

		/// <summary>
		///     Returns true if steel parameters are different.
		/// </summary>
		public static bool operator != (UniaxialReinforcement left, UniaxialReinforcement right) => !(left is null) && !left.Equals(right);

		#endregion
	}
}