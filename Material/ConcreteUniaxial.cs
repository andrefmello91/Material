namespace Material
{
	// Concrete
	public partial class Concrete
	{
		/// <summary>
        /// Concrete uniaxial class.
        /// </summary>
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
            /// <param name="concreteArea">The concrete area, in mm2.</param>
            public Uniaxial(double strength, double aggregateDiameter, double concreteArea, ParameterModel parameterModel = ParameterModel.MCFT, BehaviorModel behavior = BehaviorModel.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0) : base(strength, aggregateDiameter, parameterModel, behavior, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain)
            {
	            Area = concreteArea;
            }

            ///<inheritdoc/>
            /// <summary>
            /// Concrete for uniaxial calculations.
            /// </summary>
            ///<param name="concreteArea">The concrete area, in mm2.</param>
            public Uniaxial(Parameters parameters, double concreteArea, BehaviorModel behavior = BehaviorModel.MCFT) : base(parameters, behavior)
            {
	            Area = concreteArea;
            }

            ///<inheritdoc/>
            /// <summary>
            /// Concrete for uniaxial calculations.
            /// </summary>
            ///<param name="concreteArea">The concrete area, in mm2.</param>
            public Uniaxial(Parameters parameters, double concreteArea, Behavior concreteBehavior) : base(parameters, concreteBehavior)
            {
	            Area = concreteArea;
            }

            /// <summary>
            /// Calculate current secant module of concrete, in MPa.
            /// </summary>
            public double SecantModule => ConcreteBehavior.SecantModule(Stress, Strain);

            /// <summary>
            /// Calculate normal stiffness
            /// </summary>
            public double Stiffness => Ec * Area;

            /// <summary>
            /// Calculate maximum force resisted by concrete, in N.
            /// </summary>
            public double MaxForce => -fc * Area;

            /// <summary>
            /// Calculate current concrete force, in N.
            /// </summary>
			public double Force => Stress * Area;

            /// <summary>
            /// Calculate force (in N) given strain.
            /// </summary>
            /// <param name="strain">Current strain.</param>
            /// <param name="referenceLength">The reference length (only for DSFM).</param>
            /// <param name="reinforcement">The uniaxial reinforcement (only for DSFM).</param>
            public double CalculateForce(double strain, double referenceLength = 0, Reinforcement.Uniaxial reinforcement = null)
			{
				double stress = CalculateStress(strain, referenceLength, reinforcement);

				return
					stress * Area;
			}

            /// <summary>
            /// Calculate stress (in MPa) given strain.
            /// </summary>
            /// <param name="strain">Current strain.</param>
            /// <param name="referenceLength">The reference length (only for DSFM).</param>
            /// <param name="reinforcement">The uniaxial reinforcement (only for DSFM).</param>
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

            /// <summary>
            /// Set concrete strain.
            /// </summary>
            /// <param name="strain">Current strain.</param>
            public void SetStrain(double strain)
            {
	            Strain = strain;
            }

            /// <summary>
            /// Set concrete stress (in MPa) given strain.
            /// </summary>
            /// <param name="strain">Current strain.</param>
            /// <param name="referenceLength">The reference length (only for DSFM).</param>
            /// <param name="reinforcement">The uniaxial reinforcement (only for DSFM).</param>
            public void SetStress(double strain, double referenceLength = 0, Reinforcement.Uniaxial reinforcement = null)
            {
	            Stress = CalculateStress(strain, referenceLength, reinforcement);
            }

            /// <summary>
            /// Set concrete strain and calculate stress, in MPa.
            /// </summary>
            /// <param name="strain">Current strain.</param>
            /// <param name="referenceLength">The reference length (only for DSFM).</param>
            /// <param name="reinforcement">The uniaxial reinforcement (only for DSFM).</param>
            public void SetStrainsAndStresses(double strain, double referenceLength = 0, Reinforcement.Uniaxial reinforcement = null)
            {
	            SetStrain(strain);
	            SetStress(strain, referenceLength, reinforcement);
            }
        }
    }
}