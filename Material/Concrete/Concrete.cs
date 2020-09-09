using System;
using System.CodeDom;
using UnitsNet.Units;

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
    /// Base class for concrete object.
    /// </summary>
	public class Concrete : IEquatable<Concrete>
	{
		/// <summary>
        /// Get concrete <see cref="Material.Concrete.Parameters"/>.
        /// </summary>
		public Parameters Parameters { get; }

		/// <summary>
		/// Get concrete <see cref="Material.Concrete.Constitutive"/>.
		/// </summary>
		public Constitutive Constitutive { get; }

		/// <summary>
        /// Returns true if concrete is cracked.
        /// </summary>
		public bool Cracked => Constitutive.Cracked;

        /// <summary>
        /// Get <see cref="AggregateType"/>.
        /// </summary>
        public AggregateType Type => Parameters.Type;

		/// <summary>
        /// Get aggregate diameter, in mm.
        /// </summary>
		public double AggregateDiameter => Parameters.AggregateDiameter;

        /// <summary>
        /// Base concrete object.
        /// </summary>
        /// <param name="strength">Concrete compressive strength, in MPa.</param>
        /// <param name="aggregateDiameter">Maximum aggregate diameter, in mm.</param>
        /// <param name="parameterModel">The model for calculating concrete parameters.</param>
        /// <param name="constitutiveModel">The concrete constitutive model.</param>
        /// <param name="aggregateType">The type of aggregate.</param>
        /// <param name="tensileStrength">Concrete tensile strength, in MPa.</param>
        /// <param name="elasticModule">Concrete initial elastic module, in MPa.</param>
        /// <param name="plasticStrain">Concrete peak strain (negative value).</param>
        /// <param name="ultimateStrain">Concrete ultimate strain (negative value).</param>
        public Concrete(double strength, double aggregateDiameter, ParameterModel parameterModel = ParameterModel.MCFT, ConstitutiveModel constitutiveModel = ConstitutiveModel.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0)
        {
			// Initiate parameters
			Parameters   = Parameters.ReadParameters(parameterModel, strength, aggregateDiameter, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain);
			Constitutive = Constitutive.ReadConstitutive(constitutiveModel, Parameters);
		}

        /// <summary>
        /// Base concrete object.
        /// </summary>
        /// <param name="parameters">Concrete parameters object.</param>
        /// <param name="constitutiveModel">The base model of concrete behavior.</param>
        public Concrete(in Parameters parameters, ConstitutiveModel constitutiveModel = ConstitutiveModel.MCFT)
			: this(parameters, Constitutive.ReadConstitutive(constitutiveModel, parameters))
		{
		}

        /// <summary>
        /// Base concrete object.
        /// </summary>
        /// <param name="parameters">Concrete parameters object (<see cref="Material.Concrete.Parameters"/>).</param>
        /// <param name="constitutive">Concrete constitutive object (<see cref="Material.Concrete.Constitutive"/>).</param>
        public Concrete(in Parameters parameters, in Constitutive constitutive)
		{           
			// Initiate parameters
			Parameters   = parameters;
			Constitutive = constitutive;
		}

        // Get parameters
        public double fc  => Parameters.Strength;
        public double ft  => Parameters.TensileStrength;
        public double Ec  => Parameters.InitialModule;
		public double ec  => Parameters.PlasticStrain;
		public double ecu => Parameters.UltimateStrain;
		public double Ecs => Parameters.SecantModule;
		public double ecr => Parameters.CrackStrain;
		public double nu  => Parameters.Poisson;

		/// <summary>
		/// Return a copy of this <see cref="Concrete"/> object.
		/// </summary>
		public Concrete Copy() => new Concrete(Parameters, Constitutive);

        /// <summary>
        /// Read concrete.
        /// </summary>
        /// <param name="direction">Uniaxial or biaxial?</param>
        /// <param name="parameters">Concrete parameters object (<see cref="Material.Concrete.Parameters"/>).</param>
        /// <param name="constitutive">Concrete constitutive object (<see cref="Material.Concrete.Constitutive"/>).</param>
        ///<param name="concreteArea">The concrete area, in mm2 (only for uniaxial case).</param>
        /// <returns></returns>
        public static Concrete ReadConcrete(Direction direction, Parameters parameters, Constitutive constitutive, double concreteArea = 0)
		{
			if (direction == Direction.Uniaxial)
				return new UniaxialConcrete(parameters, concreteArea, constitutive);

			return new BiaxialConcrete(parameters, constitutive);
		}

        public override string ToString() => Parameters.ToString();

		/// <summary>
		/// Compare two concrete objects.
		/// <para>Returns true if parameters and constitutive model are equal.</para>
		/// </summary>
		/// <param name="other">The other concrete object.</param>
		public virtual bool Equals(Concrete other) => !(other is null) && (Parameters == other.Parameters && Constitutive == other.Constitutive);

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