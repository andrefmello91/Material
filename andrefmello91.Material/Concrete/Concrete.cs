using System;
using andrefmello91.Extensions;
#nullable enable

namespace andrefmello91.Material.Concrete
{
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
		///     Get concrete <see cref="ConstitutiveModel" />.
		/// </summary>
		public ConstitutiveModel Model { get; }

		/// <summary>
		///     Get concrete <see cref="Material.Concrete.IParameters" />.
		/// </summary>
		public IParameters Parameters { get; }

		#endregion

		#region Constructors

		/// <summary>
		///     Base concrete object.
		/// </summary>
		/// <param name="parameters">Concrete <see cref="IParameters" /> object.</param>
		/// <param name="model">The <see cref="ConstitutiveModel" />.</param>
		protected Concrete(IParameters parameters, ConstitutiveModel model = ConstitutiveModel.MCFT)
		{
			Parameters = parameters;
			Model      = model;
		}

		#endregion

		#region Methods

		#region Interface Implementations

		/// <inheritdoc />
		public int CompareTo(Concrete? other) => Parameters.CompareTo(other?.Parameters);

		/// <inheritdoc />
		public virtual bool Equals(Concrete? other) => Model == other?.Model && Parameters == other?.Parameters;

		#endregion

		#region Object override

		/// <inheritdoc />
		public override bool Equals(object? obj) => obj is Concrete concrete && Equals(concrete);

		/// <inheritdoc />
		public override int GetHashCode() => Parameters.GetHashCode();



		/// <inheritdoc />
		public override string ToString() => Parameters.ToString()!;

		#endregion

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
}