using UnitsNet.Units;

namespace Material
{
	/// <summary>
    /// Base class for concrete object.
    /// </summary>
	public partial class Concrete : Relations
	{
		// Properties
		public ParameterModel ConcreteParameterModel { get; }
		public Parameters     ConcreteParameters      { get; }
		public BehaviorModel  ConcreteBehaviorModel   { get; }
		public Behavior       ConcreteBehavior        { get; }

		public bool			 Cracked           => ConcreteBehavior.Cracked;
        public AggregateType Type              => ConcreteParameters.Type;
		public double        AggregateDiameter => ConcreteParameters.AggregateDiameter;

        /// <summary>
        /// Base concrete object.
        /// </summary>
        /// <param name="strength">Concrete compressive strength, in MPa.</param>
        /// <param name="aggregateDiameter">Maximum aggregate diameter, in mm.</param>
        /// <param name="parameterModel">The model for calculating concrete parameters.</param>
        /// <param name="behavior">The base model of concrete behavior.</param>
        /// <param name="aggregateType">The type of aggregate.</param>
        /// <param name="tensileStrength">Concrete tensile strength, in MPa.</param>
        /// <param name="elasticModule">Concrete initial elastic module, in MPa.</param>
        /// <param name="plasticStrain">Concrete peak strain (negative value).</param>
        /// <param name="ultimateStrain">Concrete ultimate strain (negative value).</param>
        public Concrete(double strength, double aggregateDiameter, ParameterModel parameterModel = ParameterModel.MCFT, BehaviorModel behavior = BehaviorModel.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0)
		{
			// Initiate parameters
			ConcreteParameterModel  = parameterModel;
			ConcreteParameters      = Concrete_Parameters(strength, aggregateDiameter, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain);
			ConcreteBehaviorModel   = behavior;
			ConcreteBehavior        = Concrete_Behavior();
		}

        /// <summary>
        /// Base concrete object.
        /// </summary>
        /// <param name="parameters">Concrete parameters object.</param>
        /// <param name="behavior">The base model of concrete behavior.</param>
        public Concrete(Parameters parameters, BehaviorModel behavior = BehaviorModel.MCFT)
		{
			// Initiate parameters
			ConcreteParameters    = parameters;
			ConcreteBehaviorModel = behavior;
			ConcreteBehavior      = Concrete_Behavior();
		}

        /// <summary>
        /// Base concrete object.
        /// </summary>
        /// <param name="parameters">Concrete parameters object.</param>
        /// <param name="concreteBehavior">Concrete behavior object.</param>
        public Concrete(Parameters parameters, Behavior concreteBehavior)
		{           
			// Initiate parameters
			ConcreteParameters = parameters;
			ConcreteBehavior   = concreteBehavior;
		}

        /// <summary>
        /// Get concrete parameters based on the enum type (<see cref="ConcreteParameterModel"/>).
        /// </summary>
        /// <param name="strength">Concrete compressive strength, in MPa.</param>
        /// <param name="aggregateDiameter">Maximum aggregate diameter, in mm.</param>
        /// <param name="aggregateType">The type of aggregate.</param>
        /// <param name="tensileStrength">Concrete tensile strength, in MPa.</param>
        /// <param name="elasticModule">Concrete initial elastic module, in MPa.</param>
        /// <param name="plasticStrain">Concrete peak strain (negative value).</param>
        /// <param name="ultimateStrain">Concrete ultimate strain (negative value).</param>
        private Parameters Concrete_Parameters(double strength, double aggregateDiameter, AggregateType aggregateType, double tensileStrength, double elasticModule, double plasticStrain, double ultimateStrain)
		{
			switch (ConcreteParameterModel)
			{
                case ParameterModel.MC2010:
					return new Parameters.MC2010(strength, aggregateDiameter, aggregateType);

                case ParameterModel.NBR6118:
					return new Parameters.NBR6118(strength, aggregateDiameter, aggregateType);

                case ParameterModel.MCFT:
					return new Parameters.MCFT(strength, aggregateDiameter, aggregateType);

                case ParameterModel.DSFM:
					return new Parameters.DSFM(strength, aggregateDiameter, aggregateType);
			}

            // Custom parameters
            return new Parameters.Custom(strength, aggregateDiameter, tensileStrength, elasticModule, plasticStrain, ultimateStrain);
		}

		/// <summary>
        /// Get concrete behavior based on the enum type (<see cref="ConcreteBehaviorModel"/>).
        /// </summary>
		private Behavior Concrete_Behavior()
		{
			switch (ConcreteBehaviorModel)
			{
                case BehaviorModel.MCFT:
	                return
		                new Behavior.MCFT(ConcreteParameters);

                case BehaviorModel.DSFM:
	                return
		                new Behavior.DSFM(ConcreteParameters);
            }

			// Linear:
			return null;
        }

        // Get parameters
        public double fc  => ConcreteParameters.Strength;
        public double fcr => ConcreteParameters.TensileStrength;
        public double Ec  => ConcreteParameters.InitialModule;
		public double ec  => ConcreteParameters.PlasticStrain;
		public double ecu => ConcreteParameters.UltimateStrain;
		public double Ecs => ConcreteParameters.SecantModule;
		public double ecr => ConcreteParameters.CrackStrain;
		public double nu  => ConcreteParameters.Poisson;

        /// <summary>
        /// Write string with default units (MPa and mm).
        /// </summary>
        public override string ToString() => ToString();

		/// <summary>
		/// Write string with custom units.
		/// </summary>
		/// <param name="strengthUnit">The stress unit for strength (default: MPa)</param>
		/// <param name="aggregateUnit">The aggregate dimension unit (default: mm)</param>
		public string ToString(PressureUnit strengthUnit = PressureUnit.Megapascal, LengthUnit aggregateUnit = LengthUnit.Millimeter) => ConcreteParameters.ToString(strengthUnit, aggregateUnit);
	}
}