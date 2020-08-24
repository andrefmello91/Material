using System;
using Material.Reinforcement;
using MathNet.Numerics;
using OnPlaneComponents;

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
			double ets = 2 * Gf / (ft * referenceLength);
			double fc1a = ft * (1 - (strain - ecr) / (ets - ecr));

			// Calculate coefficient for tension stiffening effect
			double m = reinforcement.TensionStiffeningCoefficient();

			// Calculate concrete postcracking stress associated with tension stiffening
			double fc1b = ft / (1 + Math.Sqrt(2.2 * m * strain));

			// Calculate maximum tensile stress
			double fc1c = Math.Max(fc1a, fc1b);

			// Check the maximum value of fc1 that can be transmitted across cracks
			double fc1s = reinforcement.MaximumPrincipalTensileStress();

			// Calculate concrete tensile stress
			return
				Math.Min(fc1c, fc1s);
		}

        /// <inheritdoc/>
        protected override double CompressiveStress(double strain)
		{
			// Calculate the principal compressive stress in concrete
			return
				CompressiveStress(strain, 0);
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

			// Calculate the coefficients
			double Cd = 0.35 * Math.Pow(-ec1 / ec2 - 0.28, 0.8);
			if (double.IsNaN(Cd))
				Cd = 1;

			double betaD = Math.Min(1 / (1 + Cs * Cd), 1);

			// Calculate fp and ep
			double
				fp = -betaD * fc * confinementFactor,
				ep =  betaD * ec * confinementFactor;

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

        /// <inheritdoc/>
        protected override double TensileStress(double strain, double transverseStrain, double theta1 = Constants.PiOver4, double referenceLength = 0, BiaxialReinforcement reinforcement = null)
		{
			// Get strains
			double
				ec1 = strain,
				ec2 = transverseStrain;

			// Calculate initial uncracked state
			double fc1 = ec1 * Ec;

			// Verify if is cracked
			VerifyCrackedState(fc1, ec2);

			// Not cracked
			if (!Cracked)
				return fc1;

			// Cracked
			// Calculate concrete post-cracking stress associated with tension softening
			double ets = 2 * Gf / (ft * referenceLength);
			double fc1a = ft * (1 - (ec1 - ecr) / (ets - ecr));

			// Calculate coefficient for tension stiffening effect
			double m = reinforcement.TensionStiffeningCoefficient(theta1);

			// Calculate concrete postcracking stress associated with tension stiffening
			double fc1b = ft / (1 + Math.Sqrt(2.2 * m * ec1));

			// Calculate maximum tensile stress
			double fc1c = Math.Max(fc1a, fc1b);

			// Check the maximum value of fc1 that can be transmitted across cracks
			double fc1s = reinforcement.MaximumPrincipalTensileStress(theta1);

			// Calculate concrete tensile stress
			return
				Math.Min(fc1c, fc1s);
		}
		#endregion

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