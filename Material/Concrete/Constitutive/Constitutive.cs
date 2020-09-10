using System;
using Extensions.Number;
using MathNet.Numerics;
using Material.Reinforcement;
using OnPlaneComponents;

namespace Material.Concrete
{
	/// <summary>
    /// Constitutive models for concrete.
    /// </summary>
	public enum ConstitutiveModel
	{
		Linear,
		MCFT,
		DSFM
	}

    /// <summary>
    /// Base class for concrete constitutive model.
    /// </summary>
    public abstract class Constitutive : IEquatable<Constitutive>
    {
		/// <summary>
        /// Get concrete <see cref="Concrete.Parameters"/>.
        /// </summary>
	    public Parameters Parameters { get; }

		/// <summary>
        /// Get/set crack slip consideration.
        /// </summary>
	    public bool ConsiderCrackSlip { get; set; }

		/// <summary>
        /// Get/set concrete cracked state.
        /// </summary>
	    public bool Cracked { get; set; }

	    // Constructor
	    /// <summary>
	    /// Base class for concrete behavior
	    /// </summary>
	    /// <param name="parameters">Concrete parameters object.</param>
	    /// <param name="considerCrackSlip">Consider crack slip (only for DSFM) (default: false)</param>
	    protected Constitutive(Parameters parameters, bool considerCrackSlip = false)
	    {
		    Parameters        = parameters;
		    ConsiderCrackSlip = considerCrackSlip;
	    }

	    // Get concrete parameters
	    protected double fc  => Parameters.Strength;
	    protected double ft  => Parameters.TensileStrength;
	    protected double Ec  => Parameters.InitialModule;
	    protected double ec  => Parameters.PlasticStrain;
	    protected double ecu => Parameters.UltimateStrain;
	    protected double Ecs => Parameters.SecantModule;
	    protected double ecr => Parameters.CrackStrain;
	    protected double nu  => Parameters.Poisson;
	    protected double Gf  => Parameters.FractureParameter;
	    protected double Cs  => ConsiderCrackSlip ? 0.55 : 1;

	    /// <summary>
        /// Get concrete <see cref="Constitutive"/> object based on the <see cref="ConstitutiveModel"/>.
        /// </summary>
        /// <param name="constitutiveModel">The <see cref="ConstitutiveModel"/> for concrete.</param>
        /// <param name="parameters">Concrete <see cref="Material.Concrete.Parameters"/>.</param>
        public static Constitutive ReadConstitutive(ConstitutiveModel constitutiveModel, Parameters parameters)
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
        /// Get the <see cref="ConstitutiveModel"/> based on <see cref="Constitutive"/> object.
        /// </summary>
        /// <param name="constitutive">The <see cref="Constitutive"/> object.</param>
        /// <returns></returns>
        public static ConstitutiveModel ReadConstitutiveModel(Constitutive constitutive)
	    {
		    if (constitutive is MCFTConstitutive)
			    return ConstitutiveModel.MCFT;

		    if (constitutive is DSFMConstitutive)
			    return ConstitutiveModel.DSFM;

		    return ConstitutiveModel.Linear;
	    }

        /// <summary>
        /// Calculate concrete <see cref="PrincipalStressState"/> related to <see cref="PrincipalStrainState"/>.
        /// <para>For <seealso cref="BiaxialConcrete"/>.</para>
        /// </summary>
        /// <param name="principalStrains">The <see cref="PrincipalStrainState"/> in concrete.</param>
        /// <param name="referenceLength">The reference length (only for <see cref="DSFMConstitutive"/>).</param>
        /// <param name="reinforcement">The <see cref="WebReinforcement"/> (only for <see cref="DSFMConstitutive"/>).</param>
        public PrincipalStressState CalculateStresses(PrincipalStrainState principalStrains, double referenceLength = 0, WebReinforcement reinforcement = null)
        {
			if (principalStrains.IsZero)
				return PrincipalStressState.Zero;

			// Get strains
			double
				ec1 = principalStrains.Epsilon1,
				ec2 = principalStrains.Epsilon2;

	        double fc1, fc2;

			// Verify case
			if (principalStrains.Case is PrincipalCase.TensionCompression)
			{
				fc1 = TensileStress(ec1, ec2, principalStrains.Theta1, referenceLength, reinforcement);
				fc2 = CompressiveStress(ec2, ec1);
			}
			else if (principalStrains.Case is PrincipalCase.PureTension)
			{
				fc1 = TensileStress(ec1, ec2, principalStrains.Theta1, referenceLength, reinforcement);
				fc2 = TensileStress(ec2, ec1, principalStrains.Theta1, referenceLength, reinforcement);
			}
			else
				return ConfinementStresses(principalStrains);

			return
				new PrincipalStressState(fc1, fc2, principalStrains.Theta1);
        }

        /// <summary>
        /// Calculate stress (in MPa) given <paramref name="strain"/>.
        /// <para>For <seealso cref="UniaxialConcrete"/>.</para>
        /// </summary>
        /// <param name="strain">Current strain.</param>
        /// <param name="referenceLength">The reference length (only for <see cref="DSFMConstitutive"/>).</param>
        /// <param name="reinforcement">The <see cref="UniaxialReinforcement"/> reinforcement (only for <see cref="DSFMConstitutive"/>).</param>
        public double CalculateStress(double strain, double referenceLength = 0, UniaxialReinforcement reinforcement = null)
		{
			if (strain == 0)
				return 0;

			if (strain > 0)
				return
					TensileStress(strain, referenceLength, reinforcement);

			return
				CompressiveStress(strain);
		}

        /// <summary>
        /// Calculate tensile stress for <see cref="BiaxialConcrete"/> case.
        /// </summary>
        /// <param name="strain">The tensile strain to calculate stress.</param>
        /// <param name="transverseStrain">The strain at the transverse direction to <paramref name="strain"/>.</param>
        /// <param name="theta1">The angle of <paramref name="strain"/> related to horizontal axis, in radians.</param>
        /// <param name="referenceLength">The reference length (only for <see cref="DSFMConstitutive"/>).</param>
        /// <param name="reinforcement">The <see cref="WebReinforcement"/> (only for <see cref="DSFMConstitutive"/>).</param>
        protected abstract double TensileStress(double strain, double transverseStrain, double theta1 = Constants.PiOver4, double referenceLength = 0, WebReinforcement reinforcement = null);

        /// <summary>
        /// Calculate tensile stress for <see cref="UniaxialConcrete"/> case.
        /// </summary>
        /// <param name="strain">Tensile strain in concrete.</param>
        /// <param name="referenceLength">The reference length (only for <see cref="DSFMConstitutive"/>).</param>
        /// <param name="reinforcement">The <see cref="UniaxialReinforcement"/> (only for <see cref="DSFMConstitutive"/>).</param>
        protected abstract double TensileStress(double strain, double referenceLength = 0, UniaxialReinforcement reinforcement = null);

        /// <summary>
        /// Calculate compressive stress for <see cref="BiaxialConcrete"/> case.
        /// </summary>
        /// <param name="strain">The compressive strain (negative) to calculate stress.</param>
        /// <param name="transverseStrain">The strain at the transverse direction to <paramref name="strain"/>.</param>
        /// <param name="confinementFactor">The confinement factor for pure compression case.<para>See: <seealso cref="ConfinementFactor"/></para></param>
        /// <returns>Compressive stress in MPa</returns>
        protected abstract double CompressiveStress(double strain, double transverseStrain, double confinementFactor = 1);

        /// <summary>
        /// Calculate compressive stress for <see cref="UniaxialConcrete"/> case.
        /// </summary>
        /// <param name="strain">Compressive strain (negative) in concrete.</param>
        protected abstract double CompressiveStress(double strain);

	    /// <summary>
	    /// Calculate current secant module.
	    /// </summary>
	    /// <param name="stress">Current stress in MPa.</param>
	    /// <param name="strain">Current strain.</param>
	    public double SecantModule(double stress, double strain) => stress.Abs() <= 1E-6 || strain.Abs() <= 1E-9 ? Ec : stress / strain;

	    /// <summary>
        /// Calculate <see cref="BiaxialConcrete"/> tensile stress for uncracked state.
        /// </summary>
        /// <param name="strain">The compressive strain (negative) to calculate stress.</param>
        /// <param name="transverseStrain">The strain at the transverse direction to <paramref name="strain"/>.</param>
		/// <param name="theta1">The angle of <paramref name="strain"/> related to horizontal axis, in radians.</param>
        /// <param name="reinforcement">The <see cref="WebReinforcement"/> object.</param>
        protected double UncrackedStress(double strain, double transverseStrain, double theta1 = Constants.PiOver4, WebReinforcement reinforcement = null)
	    {
		    if (Cracked)
			    return 0;

		    // Get strains
		    double
			    ec1 = strain,
			    ec2 = transverseStrain;

		    // Calculate initial uncracked state
		    double fc1 = ec1 * Ec;

		    // Verify if fc1 cracks concrete
		    VerifyCrackedState(fc1, ec2);

		    if (reinforcement is null)
			    return fc1;

			// Check maximum stress that can be transmitted by reinforcement
			var fc1s = reinforcement.MaximumPrincipalTensileStress(theta1);

		    return
			    Math.Min(fc1, fc1s);
	    }

        /// <summary>
        /// Check if concrete is cracked for uniaxial case and set cracked property.
        /// </summary>
        /// <param name="strain">Current strain</param>
        protected void VerifyCrackedState(double strain)
	    {
		    if (!Cracked && strain >= ecr)
			    Cracked = true;
	    }

	    /// <summary>
	    /// Check if concrete is cracked for biaxial case and set cracked property, from Gupta (1998) formulation.
	    /// </summary>
	    /// <param name="fc1">Principal tensile strain in MPa.</param>
	    /// <param name="ec2">Principal compressive strain.</param>
	    protected void VerifyCrackedState(double fc1, double ec2)
	    {
		    if (!Cracked)
		    {
			    // Calculate current cracking stress
			    double fcr1 = ft * (1 - ec2 / ec);

			    // Verify limits
			    double fcr = Math.Max(fcr1, 0.25 * ft);
			    fcr = Math.Min(fcr, ft);

			    // Verify is concrete is cracked
			    if (fc1 >= fcr)
				    // Set cracked state
				    Cracked = true;
		    }
	    }

        /// <summary>
        /// Calculate confinement <see cref="PrincipalStressState"/>.
        /// </summary>
        /// <param name="principalStrains">The <see cref="PrincipalStrainState"/>, in pure compression.
        /// <para>See: <see cref="PrincipalStrainState.PureCompression"/>.</para></param>
        /// <returns></returns>
        private PrincipalStressState ConfinementStresses(PrincipalStrainState principalStrains)
	    {
			// Get strains
			double
				ec1 = principalStrains.Epsilon1,
				ec2 = principalStrains.Epsilon2;

		    // Calculate initial stresses
			double
				fc1 = CompressiveStress(ec1, ec2),
				fc2 = CompressiveStress(ec2, ec1);

		    // Iterate to find stresses (maximum 20 iterations)
		    for (int it = 1; it <= 20; it++)
		    {
			    // Calculate confinement factors
			    double
				    betaL1 = ConfinementFactor(fc2),
				    betaL2 = ConfinementFactor(fc1);

			    // Calculate iteration stresses
			    double
				    fc1It = CompressiveStress(ec1, ec2, betaL1),
				    fc2It = CompressiveStress(ec2, ec1, betaL2);

			    // Verify tolerances
			    if ((fc1 - fc1It).Abs() <= 0.01 && (fc2 - fc2It).Abs() <= 0.01)
				    break;

			    // Update stresses
			    fc1 = fc1It;
			    fc2 = fc2It;
		    }

			return
				new PrincipalStressState(fc1, fc2, principalStrains.Theta1);
        }

        /// <summary>
        /// Calculate confinement strength factor according to Kupfer et. al. (1969).
        /// </summary>
        /// <param name="transverseStress">The stress acting on the transverse direction of the analyzed direction.</param>
        private double ConfinementFactor(double transverseStress)
	    {
			// Get absolute value
		    double fcn_fc = (transverseStress / fc).Abs();

		    return
			    1 + 0.92 * fcn_fc - 0.76 * fcn_fc * fcn_fc;
	    }

        /// <summary>
        /// Compare two constitutive objects.
        /// </summary>
        /// <param name="other">The other constitutive object.</param>
        public abstract bool Equals(Constitutive other);

	    public override int GetHashCode() => Parameters.GetHashCode();

	    /// <summary>
	    /// Returns true if parameters are equal.
	    /// </summary>
	    public static bool operator == (Constitutive left, Constitutive right) => !(left is null) && left.Equals(right);

	    /// <summary>
	    /// Returns true if parameters are different.
	    /// </summary>
	    public static bool operator != (Constitutive left, Constitutive right) => !(left is null) && !left.Equals(right);
    }
}