using System;
using Material.Concrete.Biaxial;
using Material.Concrete.Uniaxial;

namespace Material.Concrete
{
	/// <summary>
    /// Directions for concrete.
    /// </summary>
	public enum Direction
	{
		Uniaxial,
		Biaxial
	}

	/// <summary>
	///		Concrete interface.
	/// </summary>
	public interface IConcrete : IEquatable<IConcrete>, IComparable<IConcrete>
	{
		/// <summary>
		///     Get concrete <see cref="Material.Concrete.Parameters"/>.
		/// </summary>
		IParameters Parameters { get; }

		/// <summary>
		///     Get concrete <see cref="ConstitutiveModel"/>.
		/// </summary>
		public ConstitutiveModel Model { get; }
	}

    /// <summary>
    ///		Base class for concrete object.
    /// </summary>
    public abstract class Concrete : IConcrete
	{
		public IParameters Parameters { get; }

		public ConstitutiveModel Model { get; }

		/// <summary>
		/// Base concrete object.
		/// </summary>
		/// <param name="parameters">Concrete <see cref="IParameters"/> object.</param>
		/// <param name="model">The <see cref="ConstitutiveModel"/>.</param>
		protected Concrete(IParameters parameters, ConstitutiveModel model = ConstitutiveModel.MCFT)
        {
	        Parameters = parameters;
	        Model      = model;
        }

        /// <summary>
        /// Read concrete.
        /// </summary>
        /// <param name="direction">Uniaxial or biaxial?</param>
        /// <param name="parameters">Concrete parameters object (<see cref="Material.Concrete.Parameters"/>).</param>
        /// <param name="model">Concrete constitutive object (<see cref="ConstitutiveModel"/>).</param>
        ///<param name="concreteArea">The concrete area, in mm2 (only for uniaxial case).</param>
        /// <returns></returns>
        public static Concrete ReadConcrete(Direction direction, Parameters parameters, ConstitutiveModel model = ConstitutiveModel.MCFT, double concreteArea = 0)
        {
	        switch (direction)
	        {
		        case Direction.Uniaxial:
			        return new UniaxialConcrete(parameters, concreteArea, model);

		        default:
			        return new BiaxialConcrete(parameters, model);
	        }
        }

        public override string ToString() => Parameters.ToString();

        public virtual bool Equals(IConcrete other) => !(other is null) && (Model == other.Model && Parameters == other.Parameters);

		public int CompareTo(IConcrete other) => Parameters.CompareTo(other.Parameters);

		public override bool Equals(object obj) => obj is Concrete concrete && Equals(concrete);

		public override int GetHashCode() => Parameters.GetHashCode();

        /// <summary>
        /// Returns true if parameters and constitutive model are equal.
        /// </summary>
        public static bool operator == (Concrete left, Concrete right) => !(left is null) && left.Equals(right);

        /// <summary>
        /// Returns true if parameters and constitutive model are different.
        /// </summary>
        public static bool operator != (Concrete left, Concrete right) => !(left is null) && !left.Equals(right);
	}
}