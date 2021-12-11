using System;
using andrefmello91.Extensions;
#nullable enable

namespace andrefmello91.Material.Concrete;

/// <summary>
///     Constitutive models for concrete.
/// </summary>
public enum ConstitutiveModel
{
	/// <summary>
	///     Softened Membrane Model constitutive model.
	/// </summary>
	SMM,

	/// <summary>
	///     Modified Compression Field constitutive model.
	/// </summary>
	MCFT,

	/// <summary>
	///     Disturbed Stress Field constitutive model.
	/// </summary>
	DSFM
}

/// <summary>
///     Directions for concrete.
/// </summary>
public enum Direction
{
	/// <summary>
	///     Uniaxial direction.
	/// </summary>
	Uniaxial,

	/// <summary>
	///     Biaxial direction.
	/// </summary>
	Biaxial
}

/// <summary>
///     Base class for concrete object.
/// </summary>
public abstract class Concrete : IEquatable<Concrete>, IComparable<Concrete>
{

	#region Properties

	/// <summary>
	///     Check if concrete is cracked.
	/// </summary>
	/// <returns>
	///     <b>True</b> if concrete is cracked.
	/// </returns>
	public abstract bool Cracked { get; }

	/// <summary>
	///     Check if concrete crushed.
	/// </summary>
	/// <returns>
	///     <b>True</b> if concrete strain is bigger than <see cref="IConcreteParameters.UltimateStrain" />.
	/// </returns>
	public abstract bool Crushed { get; }

	/// <summary>
	///     Get concrete <see cref="ConstitutiveModel" />.
	/// </summary>
	public ConstitutiveModel Model { get; }

	/// <summary>
	///     Get concrete <see cref="IConcreteParameters" />.
	/// </summary>
	public IConcreteParameters Parameters { get; }

	/// <summary>
	///     Check if concrete yielded.
	/// </summary>
	/// <returns>
	///     <b>True</b> if concrete strain is bigger than <see cref="IConcreteParameters.PlasticStrain" />.
	/// </returns>
	public abstract bool Yielded { get; }

	#endregion

	#region Constructors

	/// <summary>
	///     Base concrete object.
	/// </summary>
	/// <param name="parameters">Concrete <see cref="IConcreteParameters" /> object.</param>
	/// <param name="model">The <see cref="ConstitutiveModel" />.</param>
	protected Concrete(IConcreteParameters parameters, ConstitutiveModel model = ConstitutiveModel.SMM)
	{
		Parameters = parameters;
		Model      = model;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is Concrete concrete && Equals(concrete);

	/// <inheritdoc />
	public override int GetHashCode() => Parameters.GetHashCode();



	/// <inheritdoc />
	public override string ToString() => Parameters.ToString()!;

	/// <inheritdoc />
	public int CompareTo(Concrete? other) => Parameters.CompareTo(other?.Parameters);

	/// <inheritdoc />
	public virtual bool Equals(Concrete? other) => Model == other?.Model && Parameters == other?.Parameters;

	#endregion

	#region Operators

	/// <summary>
	///     Returns true if parameters and constitutive model are equal.
	/// </summary>
	public static bool operator ==(Concrete? left, Concrete? right) => left.IsEqualTo(right);

	/// <summary>
	///     Returns true if parameters and constitutive model are different.
	/// </summary>
	public static bool operator !=(Concrete? left, Concrete? right) => left.IsNotEqualTo(right);

	#endregion

}