namespace Material
{
	// Concrete
	public partial class Concrete
	{
		public class Uniaxial : Concrete
		{
            // Properties
            public double Strain  { get; set; }
            public double Stress  { get; set; }
            public double Area    { get; }

            ///<inheritdoc/>
            /// <summary>
            /// Concrete for uniaxial calculations.
            /// </summary>
            public Uniaxial(double strength, double aggregateDiameter, double concreteArea, ParameterModel parameterModel = ParameterModel.MCFT, BehaviorModel behavior = BehaviorModel.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0) : base(strength, aggregateDiameter, parameterModel, behavior, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain)
            {
	            Area = concreteArea;
            }

            ///<inheritdoc/>
            /// <summary>
            /// Concrete for uniaxial calculations.
            /// </summary>
            public Uniaxial(Parameters parameters, double concreteArea, BehaviorModel behavior = BehaviorModel.MCFT) : base(parameters, behavior)
            {
	            Area = concreteArea;
            }

            ///<inheritdoc/>
            /// <summary>
            /// Concrete for uniaxial calculations.
            /// </summary>
            public Uniaxial(Parameters parameters, double concreteArea, Behavior concreteBehavior) : base(parameters, concreteBehavior)
            {
	            Area = concreteArea;
            }

            // Calculate secant module of concrete
            public double SecantModule => ConcreteBehavior.SecantModule(Stress, Strain);

			// Calculate normal stiffness
			public double Stiffness => Ec * Area;

			// Calculate maximum force
			public double MaxForce => -fc * Area;

			// Calculate current force
			public double Force => Stress * Area;

			// Calculate force given strain
			public double CalculateForce(double strain, double referenceLength = 0, Reinforcement.Uniaxial reinforcement = null)
			{
				double stress = CalculateStress(strain, referenceLength, reinforcement);

				return
					stress * Area;
			}

			// Calculate stress given strain
			public double CalculateStress(double strain, double referenceLength = 0, Reinforcement.Uniaxial reinforcement = null)
			{
				if (strain == 0)
					return 0;

				if (strain > 0)
					return
						ConcreteBehavior.TensileStress(strain, referenceLength, reinforcement);

				return
					ConcreteBehavior.CompressiveStress(strain);
            }

            // Set concrete principal strains
            public void SetStrain(double strain)
            {
	            Strain = strain;
            }

            // Set concrete stresses given strains
            public void SetStress(double strain, double referenceLength = 0, Reinforcement.Uniaxial reinforcement = null)
            {
	            Stress = CalculateStress(strain, referenceLength, reinforcement);
            }

            // Set concrete strains and stresses
            public void SetStrainsAndStresses(double strain, double referenceLength = 0, Reinforcement.Uniaxial reinforcement = null)
            {
	            SetStrain(strain);
	            SetStress(strain, referenceLength, reinforcement);
            }
        }
    }
}