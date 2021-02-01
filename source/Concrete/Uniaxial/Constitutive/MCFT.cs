using System;
using Material.Reinforcement.Uniaxial;

namespace Material.Concrete.Uniaxial
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
        protected override double CompressiveStress(double strain)
		{
			var n = strain / ec;

			return
				-fc * (2 * n - n * n);
		}

        /// <inheritdoc/>
        protected override double TensileStress(double strain, UniaxialReinforcement reinforcement = null) => strain <= ecr ? strain * Ec : CrackedStress(strain);

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