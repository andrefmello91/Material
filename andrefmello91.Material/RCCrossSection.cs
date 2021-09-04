using System;
using andrefmello91.Extensions;
using andrefmello91.Material.Concrete;
using andrefmello91.Material.Reinforcement;
using UnitsNet;

namespace andrefmello91.Material
{
	/// <summary>
	///     Reinforced concrete cross section class for axial calculation.
	/// </summary>
	public class RCCrossSection : IUniaxialMaterial, IEquatable<RCCrossSection>, ICloneable<RCCrossSection>
	{

		#region Properties

		/// <summary>
		///     The concrete at this cross section.
		/// </summary>
		public UniaxialConcrete Concrete { get; }

		/// <summary>
		///     The reinforcement at this cross section.
		/// </summary>
		public UniaxialReinforcement? Reinforcement { get; }

		/// <inheritdoc />
		public Area Area => Concrete.Area + (Reinforcement?.Area ?? Area.Zero);

		/// <inheritdoc />
		public Force Force => Concrete.Force + (Reinforcement?.Force ?? Force.Zero);

		/// <inheritdoc />
		public double Strain => Concrete.Strain;

		/// <inheritdoc />
		public Pressure Stress => Concrete.Stress + (Reinforcement?.Ratio * Reinforcement?.Stress ?? Pressure.Zero);

		#endregion

		#region Constructors

		/// <summary>
		///     Create a reinforced concrete cross section.
		/// </summary>
		/// <param name="concrete">The concrete at this cross section.</param>
		/// <param name="reinforcement">The reinforcement at this cross section.</param>
		public RCCrossSection(UniaxialConcrete concrete, UniaxialReinforcement? reinforcement)
		{
			Concrete      = concrete;
			Reinforcement = reinforcement;

			if (Reinforcement is not null)
				Reinforcement.ConcreteArea = Concrete.Area;
		}

		/// <inheritdoc cref="RCCrossSection(UniaxialConcrete, UniaxialReinforcement)" />
		/// <param name="concreteParameters">The parameters of concrete.</param>
		/// <param name="concreteArea">The area of concrete cross section.</param>
		/// <param name="concreteModel">The constitutive model of concrete.</param>
		public RCCrossSection(IConcreteParameters concreteParameters, Area concreteArea, ConstitutiveModel concreteModel, UniaxialReinforcement? reinforcement)
			: this(new UniaxialConcrete(concreteParameters, concreteArea, concreteModel), reinforcement)
		{
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override bool Equals(object obj) => obj is RCCrossSection rc && Equals(rc);

		/// <inheritdoc />
		public override int GetHashCode() => Area.GetHashCode() + Concrete.GetHashCode() + (Reinforcement?.GetHashCode() ?? 0);

		/// <inheritdoc />
		public override string ToString() =>
			$"Cross section area: {Area}\n" +
			$"{Concrete}\n" +
			$"{Reinforcement}";

		/// <inheritdoc />
		public RCCrossSection Clone() => new(Concrete.Clone(), Reinforcement?.Clone());

		/// <inheritdoc />
		public bool Equals(RCCrossSection other) => Area == other.Area && Concrete == other.Concrete && Reinforcement == other.Reinforcement;

		/// <inheritdoc />
		public void Calculate(double strain)
		{
			Reinforcement?.Calculate(strain);
			Concrete.Calculate(strain, Reinforcement);
		}

		#endregion

		#region Operators

		/// <inheritdoc cref="ComparisonExtensions.IsEqualTo{T}" />
		public static bool operator ==(RCCrossSection? left, RCCrossSection? right) => left.IsEqualTo(right);

		/// <inheritdoc cref="ComparisonExtensions.IsNotEqualTo{T}" />
		public static bool operator !=(RCCrossSection? left, RCCrossSection? right) => left.IsNotEqualTo(right);

		#endregion

	}
}