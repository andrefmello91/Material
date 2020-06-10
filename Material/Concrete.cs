using System;

namespace Material
{
	// Concrete
	public partial class Concrete : Relations
	{
		// Properties
		public ModelParameters          ConcreteModelParameters { get; }
		public Parameters               ConcreteParameters      { get; }
		public ModelBehavior            ConcreteModelBehavior   { get; }
		public Behavior                 ConcreteBehavior        { get; }

		public bool			 Cracked           => ConcreteBehavior.Cracked;
        public AggregateType Type              => ConcreteParameters.Type;
		public double        AggregateDiameter => ConcreteParameters.AggregateDiameter;

        // Read the concrete parameters
        public Concrete(double strength, double aggregateDiameter, ModelParameters modelParameters = ModelParameters.MCFT, ModelBehavior behavior = ModelBehavior.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0)
		{
			// Initiate parameters
			ConcreteModelParameters = modelParameters;
			ConcreteParameters      = Concrete_Parameters(strength, aggregateDiameter, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain);
			ConcreteModelBehavior   = behavior;
			ConcreteBehavior        = Concrete_Behavior();
		}

		// Alternate
		public Concrete(Parameters parameters, ModelBehavior behavior)
		{
			// Initiate parameters
			ConcreteParameters    = parameters;
			ConcreteModelBehavior = behavior;
			ConcreteBehavior      = Concrete_Behavior();
		}

        // Get parameters
        private Parameters Concrete_Parameters(double strength, double aggregateDiameter, AggregateType aggregateType, double tensileStrength, double elasticModule, double plasticStrain, double ultimateStrain)
		{
			switch (ConcreteModelParameters)
			{
                case ModelParameters.MC2010:
					return new Parameters.MC2010(strength, aggregateDiameter, aggregateType);

                case ModelParameters.NBR6118:
					return new Parameters.NBR6118(strength, aggregateDiameter, aggregateType);

                case ModelParameters.MCFT:
					return new Parameters.MCFT(strength, aggregateDiameter, aggregateType);

                case ModelParameters.DSFM:
					return new Parameters.DSFM(strength, aggregateDiameter, aggregateType);
			}

            // Custom parameters
            return new Parameters.Custom(strength, aggregateDiameter, tensileStrength, elasticModule, plasticStrain, ultimateStrain);
		}

		// Get parameters
		private Behavior Concrete_Behavior()
		{
			if (ConcreteModelBehavior == ModelBehavior.MCFT)
				return
					new Behavior.MCFT(ConcreteParameters);

			return
				new Behavior.DSFM(ConcreteParameters);
		}


        // Verify if concrete was set
        public bool IsSet => fc > 0;

        // Get parameters
        public double fc  => ConcreteParameters.Strength;
        public double fcr => ConcreteParameters.TensileStrength;
        public double Ec  => ConcreteParameters.InitialModule;
		public double ec  => ConcreteParameters.PlasticStrain;
		public double ecu => ConcreteParameters.UltimateStrain;
		public double Ecs => ConcreteParameters.SecantModule;
		public double ecr => ConcreteParameters.CrackStrain;
		public double nu  => ConcreteParameters.Poisson;

		public override string ToString()
		{
			char
				phi = (char) Characters.Phi,
				eps = (char) Characters.Epsilon;

			return
				"Concrete Parameters:\n"                          +
				"\nfc = "             + fc                        + " MPa" +
				"\nfcr = "            + Math.Round(fcr, 2)        + " MPa"  +
				"\nEc = "             + Math.Round(Ec, 2)         + " MPa"  +
				"\n" + eps + "c = "   + Math.Round(1000 * ec, 2)  + " E-03" +
                "\n" + eps + "cu = "  + Math.Round(1000 * ecu, 2) + " E-03" +
				"\n" + phi + ",ag = " + AggregateDiameter         + " mm";
        }
	}
}