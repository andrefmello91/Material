using System;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra;
using Relations;

namespace Material
{
	// Concrete
	public partial class Concrete
	{
		public class Biaxial : Concrete
		{
            // Properties
			public Vector<double>                 Strains           { get; set; }
			public (double theta1, double theta2) PrincipalAngles   { get; set; }
			public (double ec1, double ec2)       PrincipalStrains  { get; set; }
            public (double fc1, double fc2)       PrincipalStresses { get; set; }
			public Matrix<double>                 Stiffness         { get; set; }

            public Biaxial(double strength, double aggregateDiameter, ModelParameters modelParameters = ModelParameters.MCFT, ModelBehavior behavior = ModelBehavior.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0) : base(strength, aggregateDiameter, modelParameters, behavior, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain)
            {
	            Stiffness = InitialStiffness();
            }

            // Alternate
            public Biaxial(Parameters parameters, ModelBehavior behavior = ModelBehavior.MCFT) : base(parameters, behavior)
            {
            }

            // Calculate secant module of concrete
            public (double Ec1, double Ec2) SecantModule
            {
	            get
	            {
		            // Verify strains
		            // Get values
		            var (ec1, ec2) = PrincipalStrains;
		            var (fc1, fc2) = PrincipalStresses;

					// Calculate modules
					double
						Ec1 = ConcreteBehavior.SecantModule(fc1, ec1),
						Ec2 = ConcreteBehavior.SecantModule(fc2, ec2);

		            return
			            (Ec1, Ec2);
	            }
            }

			// Get stresses
			public Vector<double> Stresses => Stiffness * Strains;

            // Set concrete stresses given strains
            public void CalculatePrincipalStresses(Vector<double> strains, double referenceLength = 0, Reinforcement.Biaxial reinforcement = null)
            {
				// Get strains and principals
	            Strains          = strains;
	            PrincipalStrains = Principal_Strains();
	            PrincipalAngles  = StrainAngles();

                // Calculate stresses
                double
	                fc1 = ConcreteBehavior.TensileStress(PrincipalStrains, referenceLength, PrincipalAngles.theta1, reinforcement),
	                fc2 = ConcreteBehavior.CompressiveStress(PrincipalStrains);

                PrincipalStresses = (fc1, fc2);
            }

            // Calculate concrete stiffness matrix
            public void CalculateStiffness(double? thetaC1 = null, (double Ec1, double Ec2)? concreteSecantModule = null)
            {
	            double Ec1, Ec2;

	            if (concreteSecantModule.HasValue)
		            (Ec1, Ec2) = concreteSecantModule.Value;
	            else
		            (Ec1, Ec2) = SecantModule;

	            double Gc = Ec1 * Ec2 / (Ec1 + Ec2);

	            // Concrete matrix
	            var Dc1 = Matrix<double>.Build.Dense(3, 3);
	            Dc1[0, 0] = Ec1;
	            Dc1[1, 1] = Ec2;
	            Dc1[2, 2] = Gc;

	            // Get transformation matrix
	            var T = TransformationMatrix(thetaC1);

	            // Calculate Dc
	            Stiffness = T.Transpose() * Dc1 * T;
            }

            // Set tensile stress limited by crack check
            public void SetTensileStress(double fc1)
            {
	            // Get compressive stress
	            double fc2 = PrincipalStresses.fc2;

	            // Set
	            PrincipalStresses = (fc1, fc2);
            }

            // Calculate tensile strain angle
            public (double theta1, double theta2) StrainAngles(Vector<double> strains = null, (double ec1, double ec2)? principalStrains = null)
            {
	            if (strains == null)
		            strains = Strains;

	            if (!principalStrains.HasValue)
		            principalStrains = PrincipalStrains;

	            return
		            Strain.PrincipalAngles(strains, principalStrains.Value);
            }

            // Calculate principal strains
            public (double ec1, double ec2) Principal_Strains(Vector<double> strains = null)
            {
	            if (strains == null)
		            strains = Strains;

                return
	                Strain.PrincipalStrains(strains);
            }

            // Calculate initial stiffness
            public Matrix<double> InitialStiffness()
            {
	            // Concrete matrix
	            var Dc1 = Matrix<double>.Build.Dense(3, 3);
	            Dc1[0, 0] = Ec;
	            Dc1[1, 1] = Ec;
	            Dc1[2, 2] = 0.5 * Ec;

	            // Get transformation matrix
	            var T = TransformationMatrix(Constants.PiOver4);

	            // Calculate Dc
	            return
		            T.Transpose() * Dc1 * T;
            }

            // Calculate stresses/strains transformation matrix
            // This matrix transforms from x-y to 1-2 coordinates
            public Matrix<double> TransformationMatrix(double? theta1 = null)
            {
	            if (!theta1.HasValue)
		            theta1 = PrincipalAngles.theta1;

	            return
		            Strain.TransformationMatrix(theta1.Value);
            }
        }
    }
}