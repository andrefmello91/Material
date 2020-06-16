using System;
using MathNet.Numerics;

namespace Material
{
	// Concrete
	public partial class Concrete
	{
		public enum ModelBehavior
		{
			Linear,
			MCFT,
			DSFM
		}

        // Implementation of concrete parameters
        public abstract class Behavior
        {
			// Properties
			public Parameters Parameters        { get; }
			public bool       ConsiderCrackSlip { get; set; }
			public bool       Cracked           { get; set; }

			// Constructor
			public Behavior(Parameters parameters, bool considerCrackSlip = false)
			{
				Parameters        = parameters;
				ConsiderCrackSlip = considerCrackSlip;
			}

            // Get concrete parameters
            private double fc  => Parameters.Strength;
            private double fcr => Parameters.TensileStrength;
            private double Ec  => Parameters.InitialModule;
            private double ec  => Parameters.PlasticStrain;
            private double ecu => Parameters.UltimateStrain;
            private double Ecs => Parameters.SecantModule;
            private double ecr => Parameters.CrackStrain;
            private double nu  => Parameters.Poisson;
            private double Gf  => Parameters.FractureParameter;
            private double Cs
            {
	            get
	            {
		            if (ConsiderCrackSlip)
			            return 0.55;

		            return 1;
	            }
            }

            // Calculate concrete stresses
            public abstract double TensileStress((double ec1, double ec2) principalStrains, double referenceLength = 0, double theta1 = Constants.PiOver4, Reinforcement.Biaxial reinforcement = null);
            public abstract double TensileStress(double strain, double referenceLength = 0, Reinforcement.Uniaxial reinforcement = null);
	        public abstract double CompressiveStress((double ec1, double ec2) principalStrains);
	        public abstract double CompressiveStress(double strain);

			// Calculate secant module
			public double SecantModule(double stress, double strain)
			{
				if (stress == 0 || strain == 0)
					return Ec;

				return
					stress / strain;
			}

			// Verify if concrete is cracked (uniaxial)
			public void VerifyCracked(double strain)
			{
				if (!Cracked && strain >= ecr)
					Cracked = true;
			}

			// Verify if concrete is cracked (biaxial)
			public void VerifyCracked(double fc1, double ec2)
			{
				if (!Cracked)
				{
					// Calculate current cracking stress
					double fcr1 = this.fcr * (1 - ec2 / ec);

					// Verify limits
					double fcr = Math.Max(fcr1, 0.25 * this.fcr);
					fcr = Math.Min(fcr, this.fcr);

					// Verify is concrete is cracked
					if (fc1 >= fcr)
						// Set cracked state
						Cracked = true;
				}
			}


            public class MCFT : Behavior
	        {
		        // Constructor
		        public MCFT(Parameters parameters, bool considerCrackSlip = false) : base(parameters, considerCrackSlip)
		        {
		        }

		        #region Uniaxial
                public override double CompressiveStress(double strain)
                {
	                double n = strain / ec;

	                return
		                -fc * (2 * n - n * n);
                }

                // Calculate tensile stress in concrete
                public override double TensileStress(double strain, double referenceLength = 0, Reinforcement.Uniaxial reinforcement = null)
		        {
                    // Constitutive relation
                    if (strain <= ecr) // Not cracked
				        return
					        strain * Ec;

			        // Else, cracked
			        // Constitutive relation
			        return
				        fcr / (1 + Math.Sqrt(500 * strain));
		        }
                #endregion

                #region Biaxial
                // Principal stresses by classic formulation
                public override double CompressiveStress((double ec1, double ec2) principalStrains)
                {
	                // Get the strains
	                var (ec1, ec2) = principalStrains;

	                // Calculate the maximum concrete compressive stress
	                double
		                f2maxA = -fc / (0.8 - 0.34 * ec1 / ec),
		                f2max = Math.Max(f2maxA, -fc);

	                // Calculate the principal compressive stress in concrete
	                double n = ec2 / ec;

	                return
		                f2max * (2 * n - n * n);
                }

                public override double TensileStress((double ec1, double ec2) principalStrains, double referenceLength = 0, double theta1 = Constants.PiOver4, Reinforcement.Biaxial reinforcement = null)
                {
	                var (ec1, ec2) = principalStrains;

					// Calculate initial uncracked state
					double fc1 = ec1 * Ec;

					// Verify if is cracked
					VerifyCracked(fc1, ec2);

					// Not cracked
                    if (!Cracked)
						return fc1;

					// Else, cracked
					return
						fcr / (1 + Math.Sqrt(500 * ec1));

                }



                #endregion
            }

            public class DSFM : Behavior
	        {
                // Constructor
                public DSFM(Parameters parameters, bool considerCrackSlip = true) : base(parameters, considerCrackSlip)
                {
                }

                #region Uniaxial
                public override double TensileStress(double strain, double Lr, Reinforcement.Uniaxial reinforcement)
                {
                    // Check if concrete is cracked
                    if (strain <= ecr) // Not cracked
                        return
                            Ec * strain;

                    // Cracked
                    // Calculate concrete post-cracking stress associated with tension softening
                    double ets = 2 * Gf / (fcr * Lr);
                    double fc1a = fcr * (1 - (strain - ecr) / (ets - ecr));

                    // Calculate coefficient for tension stiffening effect
                    double m = reinforcement.TensionStiffeningCoefficient();

                    // Calculate concrete postcracking stress associated with tension stiffening
                    double fc1b = fcr / (1 + Math.Sqrt(2.2 * m * strain));

                    // Calculate maximum tensile stress
                    double fc1c = Math.Max(fc1a, fc1b);

                    // Check the maximum value of fc1 that can be transmitted across cracks
                    double fc1s = reinforcement.MaximumPrincipalTensileStress();

                    // Calculate concrete tensile stress
                    return
                        Math.Min(fc1c, fc1s);
                }

                public override double CompressiveStress(double strain)
                {
                    // Calculate the principal compressive stress in concrete
                    return
	                    CompressiveStress((0, strain));
                }
                #endregion

                #region Biaxial
                public override double CompressiveStress((double ec1, double ec2) principalStrains)
                {
	                // Get strains
	                var (ec1, ec2) = principalStrains;

	                //if (ec2 >= 0)
	                //    return 0;

	                // Calculate the coefficients
	                //double Cd = 0.27 * (ec1 / ec - 0.37);
	                double Cd = 0.35 * Math.Pow(-ec1 / ec2 - 0.28, 0.8);
	                if (double.IsNaN(Cd))
		                Cd = 1;

	                double betaD = Math.Min(1 / (1 + Cs * Cd), 1);

	                // Calculate fp and ep
	                double
		                fp = -betaD * fc,
		                ep =  betaD * ec;

	                // Calculate parameters of concrete
	                double k;
	                if (ep <= ec2)
		                k = 1;
	                else
		                k = 0.67 - fp / 62;

	                double
		                n = 0.8 - fp / 17,
		                ec2_ep = ec2 / ep;

	                // Calculate the principal compressive stress in concrete
	                return
		                fp * n * ec2_ep / (n - 1 + Math.Pow(ec2_ep, n * k));
                }

                public override double TensileStress((double ec1, double ec2) principalStrains, double referenceLength = 0, double theta1 = Constants.PiOver4, Reinforcement.Biaxial reinforcement = null)
                {
	                var (ec1, ec2) = principalStrains;

	                // Calculate initial uncracked state
	                double fc1 = ec1 * Ec;

	                // Verify if is cracked
	                VerifyCracked(fc1, ec2);

	                // Not cracked
	                if (!Cracked)
		                return fc1;

	                // Cracked
	                // Calculate concrete post-cracking stress associated with tension softening
	                double ets = 2 * Gf / (fcr * referenceLength);
	                double fc1a = fcr * (1 - (ec1 - ecr) / (ets - ecr));

	                // Calculate coefficient for tension stiffening effect
	                double m = reinforcement.TensionStiffeningCoefficient(theta1);

	                // Calculate concrete postcracking stress associated with tension stiffening
	                double fc1b = fcr / (1 + Math.Sqrt(2.2 * m * ec1));

	                // Calculate maximum tensile stress
	                double fc1c = Math.Max(fc1a, fc1b);

	                // Check the maximum value of fc1 that can be transmitted across cracks
	                double fc1s = reinforcement.MaximumPrincipalTensileStress(theta1);

	                // Calculate concrete tensile stress
	                return
		                Math.Min(fc1c, fc1s);
                }
                #endregion
            }
        }
	}
}