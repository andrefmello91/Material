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
	public class Concrete : Relations, IEquatable<Concrete>
	{
		// Properties
		public Parameters   Parameters      { get; }
		public Constitutive Constitutive    { get; }

		public bool			 Cracked           => Constitutive.Cracked;
        public AggregateType Type              => Parameters.Type;
		public double        AggregateDiameter => Parameters.AggregateDiameter;

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
        public Concrete(Parameters parameters, ConstitutiveModel constitutiveModel = ConstitutiveModel.MCFT)
		{
			// Initiate parameters
			Parameters   = parameters;
			Constitutive = Constitutive.ReadConstitutive(constitutiveModel, Parameters);
		}

        /// <summary>
        /// Base concrete object.
        /// </summary>
        /// <param name="parameters">Concrete parameters object (<see cref="Material.Concrete.Parameters"/>).</param>
        /// <param name="constitutive">Concrete constitutive object (<see cref="Material.Concrete.Constitutive"/>).</param>
        public Concrete(Parameters parameters, Constitutive constitutive)
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

        /// <summary>
        /// Write string with default units (MPa and mm).
        /// </summary>
        public override string ToString() => ToString();

		/// <summary>
		/// Write string with custom units.
		/// </summary>
		/// <param name="strengthUnit">The stress unit for strength (default: MPa)</param>
		/// <param name="aggregateUnit">The aggregate dimension unit (default: mm)</param>
		public string ToString(PressureUnit strengthUnit = PressureUnit.Megapascal, LengthUnit aggregateUnit = LengthUnit.Millimeter) => Parameters.ToString(strengthUnit, aggregateUnit);

		/// <summary>
		/// Compare two concrete objects.
		/// <para>Returns true if parameters and constitutive model are equal.</para>
		/// </summary>
		/// <param name="other">The other concrete object.</param>
		public virtual bool Equals(Concrete other)
		{
			if (other != null)
				return Parameters == other.Parameters && Constitutive == other.Constitutive;

			return false;
		}

		public override bool Equals(object other)
		{
			if (other != null && other is Concrete concrete)
				return Equals(concrete);

			return false;
		}

		public override int GetHashCode() => Parameters.GetHashCode();

        /// <summary>
        /// Returns true if parameters and constitutive model are equal.
        /// </summary>
        public static bool operator == (Concrete left, Concrete right) => left != null && left.Equals(right);

        /// <summary>
        /// Returns true if parameters and constitutive model are different.
        /// </summary>
        public static bool operator != (Concrete left, Concrete right) => left != null && !left.Equals(right);
	}
}