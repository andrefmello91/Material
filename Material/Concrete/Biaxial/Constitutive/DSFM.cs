﻿using System;
using Extensions.Number;
using Material.Reinforcement.Biaxial;
using MathNet.Numerics;

namespace Material.Concrete.Biaxial
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
				fp * n * ec2_ep / (n - 1 + ec2_ep.Pow(n * k));
		}

		/// <inheritdoc/>
        protected override double TensileStress(double strain, double transverseStrain, double theta1 = Constants.PiOver4, double referenceLength = 0, WebReinforcement reinforcement = null)
		{
			// Get strains
			double
				ec1 = strain,
				ec2 = transverseStrain;

			// Calculate initial uncracked state
			var fc1 = UncrackedStress(ec1, ec2, theta1, reinforcement);

			// Not cracked
			if (!Cracked)
				return fc1;

            // Cracked
            // Calculate concrete post-cracking stress associated with tension softening
            var fc1a = TensionSoftening(ec1, referenceLength);

            // Calculate concrete post-cracking stress associated with tension stiffening.
            var fc1b = TensionStiffening(ec1, theta1, reinforcement);

            // Return maximum
            return
                Math.Max(fc1a, fc1b);
        }

		/// <summary>
		/// Calculate concrete post-cracking stress associated with tension stiffening (for <see cref="BiaxialConcrete"/>).
		/// </summary>
		/// <param name="strain">The tensile strain to calculate stress.</param>
		/// <param name="theta1">The angle of maximum principal strain, in radians.</param>
		/// <param name="reinforcement">The <see cref="WebReinforcement"/>.</param>
		private double TensionStiffening(double strain, double theta1, WebReinforcement reinforcement)
		{
			// Calculate coefficient for tension stiffening effect
			var m = reinforcement.TensionStiffeningCoefficient(theta1);

			// Calculate concrete postcracking stress associated with tension stiffening
			double fc1b = ft / (1 + (2.2 * m * strain).Sqrt());

			// Check the maximum value of fc1 that can be transmitted across cracks
			double fc1s = reinforcement.MaximumPrincipalTensileStress(theta1);

			// Return minimum
			return
				Math.Min(fc1b, fc1s);
		}

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
            double Cd = 0.35 * (r - 0.28).Pow(0.8);

			return
				Math.Min(1.0 / (1 + Cs * Cd), 1);
		}

        /// <summary>
        /// Calculate concrete post-cracking stress associated with tension softening.
        /// </summary>
        /// <param name="strain">The tensile strain to calculate stress.</param>
        /// <param name="referenceLength">The reference length.</param>
        private double TensionSoftening(double strain, double referenceLength)
        {
	        double ets = 2.0 * Gf / (ft * referenceLength);

	        return
		        ft * (1.0 - (strain - ecr) / (ets - ecr));
        }

        public override string ToString() => "DSFM";

		/// <summary>
		/// Compare two constitutive objects.
		/// </summary>
		/// <param name="other">The other constitutive object.</param>
		public override bool Equals(Material.Concrete.Constitutive other) => other is DSFMConstitutive;

		public override bool Equals(object other) => other is DSFMConstitutive;

		public override int GetHashCode() => base.GetHashCode();
	}
}