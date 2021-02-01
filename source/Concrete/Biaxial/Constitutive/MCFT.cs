using System;
using Material.Reinforcement.Biaxial;
using MathNet.Numerics;

namespace Material.Concrete.Biaxial
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
			var n = ec2 / ec;

			return
				f2max * (2 * n - n * n);
		}

        /// <inheritdoc/>
        protected override double TensileStress(double strain, double transverseStrain, double theta1 = Constants.PiOver4, double referenceLength = 0, WebReinforcement reinforcement = null)
		{
			// Get strains
			double
				ec1 = strain,
				ec2 = transverseStrain;

			// Calculate initial uncracked state
			double fc1 = UncrackedStress(ec1, ec2, theta1, reinforcement);

			// Not cracked
			if (!Cracked)
				return fc1;

			// Else, cracked
			return
				CrackedStress(ec1);
		}

		/// <summary>
        /// Calculate tensile stress for cracked concrete.
        /// </summary>
        /// <param name="strain">Current tensile strain.</param>
		private double CrackedStress(double strain) => ft / (1 + Math.Sqrt(500 * strain));

        public override string ToString() => "MCFT";

		/// <summary>
		/// Compare two constitutive objects.
		/// </summary>
		/// <param name="other">The other constitutive object.</param>
		public override bool Equals(Material.Concrete.Constitutive other) => other is MCFTConstitutive;

		public override bool Equals(object other) => other is MCFTConstitutive;

		public override int GetHashCode() => base.GetHashCode();
	}
}