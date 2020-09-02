using System;
using Material.Reinforcement;
using MathNet.Numerics;

namespace Material.Concrete
{
	/// <summary>
	/// DSFM constitutive class.
	/// </summary>
	public class DSFMConstitutive : Constitutive
	{
		// Constructor
		/// <inheritdoc/>
		/// <param name="parameters">Concrete parameters object.</param>
		/// <param name="considerCrackSlip">Consider crack slip (default: true)</param>
		public DSFMConstitutive(Parameters parameters, bool considerCrackSlip = true) : base(parameters, considerCrackSlip)
		{
		}

		#region Uniaxial
		/// <inheritdoc/>
		protected override double TensileStress(double strain, double referenceLength = 0, UniaxialReinforcement reinforcement = null)
		{
			// Check if concrete is cracked
			if (strain <= ecr) // Not cracked
				return
					Ec * strain;

			// Cracked
			// Calculate concrete post-cracking stress associated with tension softening
			double fc1a = TensionSoftening(strain, referenceLength);

			// Calculate concrete postcracking stress associated with tension stiffening
			double fc1b = TensionStiffening(strain, reinforcement);

			// Return maximum
			return
				Math.Max(fc1a, fc1b);
		}

        /// <inheritdoc/>
        protected override double CompressiveStress(double strain)
		{
			// Calculate the principal compressive stress in concrete
			return
				CompressiveStress(strain, 0);
		}

        /// <summary>
        /// Calculate concrete post-cracking stress associated with tension stiffening (for <see cref="UniaxialConcrete"/>).
        /// </summary>
        /// <param name="strain">The tensile strain to calculate stress.</param>
        /// <param name="reinforcement">The <see cref="UniaxialReinforcement"/>.</param>
        private double TensionStiffening(double strain, UniaxialReinforcement reinforcement)
        {
	        // Calculate coefficient for tension stiffening effect
	        double m = reinforcement.TensionStiffeningCoefficient();

	        // Calculate concrete postcracking stress associated with tension stiffening
	        double fc1b = ft / (1 + Math.Sqrt(2.2 * m * strain));

	        // Check the maximum value of fc1 that can be transmitted across cracks
	        double fc1s = reinforcement.MaximumPrincipalTensileStress();

	        // Return minimum
	        return
		        Math.Min(fc1b, fc1s);
        }

        #endregion

        #region Biaxial
        /// <inheritdoc/>
        protected override double CompressiveStress(double strain, double transverseStrain, double confinementFactor = 1)
		{
			// Get strains
			double
				ec1 = transverseStrain,
				ec2 = strain;

			// Calculate beta D
			double betaD = SofteningFactor(ec2, ec1);

			// Calculate fp and ep
			double
				fp = -betaD * fc * confinementFactor,
				ep =  betaD * ec * confinementFactor;

			// Calculate parameters of concrete
			double 
				k = ep <= ec2 ? 1 : 0.67 - fp / 62,
				n = 0.8 - fp / 17,
				ec2_ep = ec2 / ep;

			// Calculate the principal compressive stress in concrete
			return
				fp * n * ec2_ep / (n - 1 + Math.Pow(ec2_ep, n * k));
		}

		/// <inheritdoc/>
        protected override double TensileStress(double strain, double transverseStrain, double theta1 = Constants.PiOver4, double referenceLength = 0, BiaxialReinforcement reinforcement = null)
		{
			// Get strains
			double
				ec1 = strain,
				ec2 = transverseStrain;

			// Calculate initial uncracked state
			double fc1 = UncrackedStress(ec1, ec2);

			// Not cracked
			if (!Cracked)
				return fc1;

            // Cracked
            // Calculate concrete post-cracking stress associated with tension softening
            double fc1a = TensionSoftening(ec1, referenceLength);

            // Calculate concrete post-cracking stress associated with tension stiffening.
            double fc1b = TensionStiffening(ec1, theta1, reinforcement);

            // Return maximum
            return
                Math.Max(fc1a, fc1b);
        }

		/// <summary>
		/// Calculate concrete post-cracking stress associated with tension stiffening (for <see cref="BiaxialConcrete"/>).
		/// </summary>
		/// <param name="strain">The tensile strain to calculate stress.</param>
		/// <param name="theta1">The angle of maximum principal strain, in radians.</param>
		/// <param name="reinforcement">The <see cref="BiaxialReinforcement"/>.</param>
		private double TensionStiffening(double strain, double theta1, BiaxialReinforcement reinforcement)
		{
			// Calculate coefficient for tension stiffening effect
			double m = reinforcement.TensionStiffeningCoefficient(theta1);

			// Calculate concrete postcracking stress associated with tension stiffening
			double fc1b = ft / (1 + Math.Sqrt(2.2 * m * strain));

			// Check the maximum value of fc1 that can be transmitted across cracks
			double fc1s = reinforcement.MaximumPrincipalTensileStress(theta1);

			// Return minimum
			return
				Math.Min(fc1b, fc1s);
		}

        #endregion

        /// <summary>
        /// Calculate compression softening factor (beta D).
        /// </summary>
        /// <param name="strain">The compressive strain (negative) to calculate stress.</param>
        /// <param name="transverseStrain">The strain at the transverse direction to <paramref name="strain"/>.</param>
		private double SofteningFactor(double strain, double transverseStrain)
		{
			// Calculate strain ratio
			double r  = Math.Min(-transverseStrain / strain, 400);

			if (r < 0.28) // Cd = 0
				return 1;

			// Calculate Cd
            double Cd = 0.35 * Math.Pow(r - 0.28, 0.8);

			return
				Math.Min(1 / (1 + Cs * Cd), 1);
		}

        /// <summary>
        /// Calculate concrete post-cracking stress associated with tension softening.
        /// </summary>
        /// <param name="strain">The tensile strain to calculate stress.</param>
        /// <param name="referenceLength">The reference length.</param>
        private double TensionSoftening(double strain, double referenceLength)
        {
	        double ets = 2 * Gf / (ft * referenceLength);

	        return
		        ft * (1 - (strain - ecr) / (ets - ecr));
        }

        public override string ToString() => "DSFM";

		/// <summary>
		/// Compare two constitutive objects.
		/// </summary>
		/// <param name="other">The other constitutive object.</param>
		public override bool Equals(Constitutive other)
		{
			if (other is DSFMConstitutive)
				return true;

			return false;
		}

		public override bool Equals(object other)
		{
			if (other is DSFMConstitutive)
				return true;

			return false;
		}

		public override int GetHashCode() => base.GetHashCode();

	}
}