using UnitsNet.Units;

namespace Material
{
	// Concrete
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

        // Read the concrete parameters
        public Concrete(double strength, double aggregateDiameter, ParameterModel parameterModel = ParameterModel.MCFT, BehaviorModel behavior = BehaviorModel.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0)
		{
			// Initiate parameters
			ConcreteParameterModel  = parameterModel;
			ConcreteParameters      = Concrete_Parameters(strength, aggregateDiameter, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain);
			ConcreteBehaviorModel   = behavior;
			ConcreteBehavior        = Concrete_Behavior();
		}

		// Alternates
		public Concrete(Parameters parameters, BehaviorModel behavior = BehaviorModel.MCFT)
		{
			// Initiate parameters
			ConcreteParameters    = parameters;
			ConcreteBehaviorModel = behavior;
			ConcreteBehavior      = Concrete_Behavior();
		}

		public Concrete(Parameters parameters, Behavior concreteBehavior)
		{           
			// Initiate parameters
			ConcreteParameters = parameters;
			ConcreteBehavior   = concreteBehavior;
		}

        // Get parameters
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

		// Get parameters
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

		public override string ToString() => ConcreteParameters.ToString();

		// T string with custom units
		public string ToString(PressureUnit stressUnit, LengthUnit lengthUnit) =>
			ConcreteParameters.ToString(stressUnit, lengthUnit);
	}
}