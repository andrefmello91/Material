using Extensions.Number;
using Material.Reinforcement.Uniaxial;

namespace Material.Concrete.Uniaxial
{
    /// <summary>
    /// Base class for concrete constitutive model.
    /// </summary>
    public abstract class Constitutive : Material.Concrete.Constitutive
    {
	    // Constructor
	    /// <summary>
	    /// Base class for concrete behavior
	    /// </summary>
	    /// <param name="parameters">Concrete parameters object.</param>
	    /// <param name="considerCrackSlip">Consider crack slip (only for DSFM) (default: false)</param>
	    protected Constitutive(Parameters parameters, bool considerCrackSlip = false)
			: base(parameters, considerCrackSlip)
	    {
	    }

	    /// <summary>
        /// Get concrete <see cref="Constitutive"/> object based on the <see cref="ConstitutiveModel"/>.
        /// </summary>
        /// <param name="constitutiveModel">The <see cref="ConstitutiveModel"/> for concrete.</param>
        /// <param name="parameters">Concrete <see cref="Parameters"/>.</param>
        public static Constitutive Read(ConstitutiveModel constitutiveModel, Parameters parameters)
	    {
		    switch (constitutiveModel)
		    {
			    case ConstitutiveModel.MCFT:
				    return
					    new MCFTConstitutive(parameters);

			    case ConstitutiveModel.DSFM:
				    return
					    new DSFMConstitutive(parameters);
		    }

		    // Linear:
		    return null;
	    }

        /// <summary>
        /// Calculate stress (in MPa) given <paramref name="strain"/>.
        /// <para>For <seealso cref="UniaxialConcrete"/>.</para>
        /// </summary>
        /// <param name="strain">Current strain.</param>
        /// <param name="reinforcement">The <see cref="UniaxialReinforcement"/> reinforcement (only for <see cref="DSFMConstitutive"/>).</param>
        public double CalculateStress(double strain, UniaxialReinforcement reinforcement = null)
		{
			if (strain.ApproxZero())
				return 0;

			return strain > 0 ? TensileStress(strain, reinforcement) : CompressiveStress(strain);
		}

        /// <summary>
        /// Calculate tensile stress for <see cref="UniaxialConcrete"/> case.
        /// </summary>
        /// <param name="strain">Tensile strain in concrete.</param>
        /// <param name="reinforcement">The <see cref="UniaxialReinforcement"/> (only for <see cref="DSFMConstitutive"/>).</param>
        protected abstract double TensileStress(double strain, UniaxialReinforcement reinforcement = null);

        /// <summary>
        /// Calculate compressive stress for <see cref="UniaxialConcrete"/> case.
        /// </summary>
        /// <param name="strain">Compressive strain (negative) in concrete.</param>
        protected abstract double CompressiveStress(double strain);

		/// <summary>
		/// Check if concrete is cracked for <see cref="UniaxialConcrete"/> case and set cracked property.
		/// </summary>
		/// <param name="strain">Current strain</param>
		protected void VerifyCrackedState(double strain)
	    {
		    if (!Cracked && strain >= ecr)
			    Cracked = true;
	    }
    }
}