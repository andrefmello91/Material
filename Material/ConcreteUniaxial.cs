using MathNet.Numerics;

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

            public Uniaxial(double strength, double aggregateDiameter, ModelParameters modelParameters, ModelBehavior behavior = ModelBehavior.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0) : base(strength, aggregateDiameter, modelParameters, behavior, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain)
            {
            }

            public Uniaxial(Parameters parameters, ModelBehavior behavior) : base(parameters, behavior)
            {
            }

            // Calculate secant module of concrete
            public double SecantModule => ConcreteBehavior.SecantModule(Stress, Strain);

            // Set concrete principal strains
            public void SetStrain(double strain)
            {
	            Strain = strain;
            }

            // Set concrete stresses given strains
            public void SetStress(double strain, double referenceLength = 0, double theta1 = Constants.PiOver4, Reinforcement.Biaxial reinforcement = null)
            {
	            if (strain == 0)
		            Stress = 0;

				else if (strain > 0)
		            Stress = ConcreteBehavior.TensileStress(strain, referenceLength, theta1, reinforcement);

	            else
		            Stress = ConcreteBehavior.CompressiveStress(strain);
            }

            // Set concrete strains and stresses
            public void SetStrainsAndStresses(double strain, double referenceLength = 0, double theta1 = Constants.PiOver4, Reinforcement.Biaxial reinforcement = null)
            {
	            SetStrain(strain);
	            SetStress(strain, referenceLength, theta1, reinforcement);
            }
        }
    }
}