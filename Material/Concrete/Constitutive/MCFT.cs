using System;
using Material.Reinforcement;
using MathNet.Numerics;
using OnPlaneComponents;

namespace Material.Concrete
{
	/// <summary>
	/// MCFT constitutive class.
	/// </summary>
	public class MCFTConstitutive : Constitutive
	{
		// Constructor
		/// <inheritdoc/>
		public MCFTConstitutive(Parameters parameters, bool considerCrackSlip = false) : base(parameters, considerCrackSlip)
		{
		}

        #region Uniaxial
        /// <inheritdoc/>
        protected override double CompressiveStress(double strain)
		{
			double n = strain / ec;

			return
				-fc * (2 * n - n * n);
		}

        // Calculate tensile stress in concrete
        /// <inheritdoc/>
        protected override double TensileStress(double strain, double referenceLength = 0, UniaxialReinforcement reinforcement = null)
		{
			// Constitutive relation
			if (strain <= ecr) // Not cracked
				return
					strain * Ec;
					
			// Else, cracked
			// Constitutive relation
			return
				ft / (1 + Math.Sqrt(500 * strain));
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

            // Calculate the maximum concrete compressive stress
            double
                f2maxA = ec1 > 0 ? -fc / (0.8 - 0.34 * ec1 / ec) : -fc,
				f2max  = Math.Max(f2maxA, -fc) * confinementFactor;

			// Calculate the principal compressive stress in concrete
			double n = ec2 / ec;

			return
				f2max * (2 * n - n * n);
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

			// Else, cracked
			return
				ft / (1 + Math.Sqrt(500 * ec1));

		}
		#endregion

		public override string ToString() => "MCFT";

		/// <summary>
		/// Compare two constitutive objects.
		/// </summary>
		/// <param name="other">The other constitutive object.</param>
		public override bool Equals(Constitutive other)
		{
			if (other is MCFTConstitutive)
				return true;

			return false;
		}

		public override bool Equals(object other)
		{
			if (other is MCFTConstitutive)
				return true;

			return false;
		}

		public override int GetHashCode() => base.GetHashCode();
	}
}