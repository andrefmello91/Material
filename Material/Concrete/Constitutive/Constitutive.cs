using System;
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
	    // Properties
	    public Parameters Parameters        { get; }
	    public bool       ConsiderCrackSlip { get; set; }
	    public bool       Cracked           { get; set; }

	    // Constructor
	    /// <summary>
	    /// Base class for concrete behavior
	    /// </summary>
	    /// <param name="parameters">Concrete parameters object.</param>
	    /// <param name="considerCrackSlip">Consider crack slip (only for DSFM) (default: false)</param>
	    public Constitutive(Parameters parameters, bool considerCrackSlip = false)
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
	    protected double Cs
	    {
		    get
		    {
			    if (ConsiderCrackSlip)
				    return 0.55;

			    return 1;
		    }
	    }

	    /// <summary>
	    /// Get concrete behavior based on the enum type (<see cref="ConstitutiveModel"/>).
	    /// </summary>
	    /// <param name="constitutiveModel">The constitutive model for concrete.</param>
	    /// <param name="parameters">Concrete parameters.</param>
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

	    public static ConstitutiveModel ReadConstitutiveModel(Constitutive constitutive)
	    {
		    if (constitutive is MCFTConstitutive)
			    return ConstitutiveModel.MCFT;

		    if (constitutive is DSFMConstitutive)
			    return ConstitutiveModel.DSFM;

		    return ConstitutiveModel.Linear;
	    }

        /// <summary>
        /// Calculate tensile stress for biaxial case.
        /// </summary>
        /// <param name="principalStrains">Principal strains in concrete.</param>
        /// <param name="referenceLength">The reference length (only for DSFM).</param>
        /// <param name="reinforcement">The biaxial reinforcement (only for DSFM).</param>
        /// <returns>Tensile stress in MPa</returns>
        public abstract double TensileStress(PrincipalStrainState principalStrains, double referenceLength = 0, BiaxialReinforcement reinforcement = null);

        /// <summary>
        /// Calculate tensile stress for uniaxial case.
        /// </summary>
        /// <param name="strain">Tensile strain in concrete.</param>
        /// <param name="referenceLength">The reference length (only for DSFM).</param>
        /// <param name="reinforcement">The uniaxial reinforcement (only for DSFM).</param>
        /// <returns>Tensile stress in MPa</returns>
        public abstract double TensileStress(double strain, double referenceLength = 0, UniaxialReinforcement reinforcement = null);

        /// <summary>
        /// Calculate compressive stress for biaxial case.
        /// </summary>
        /// <param name="principalStrains">Principal strains in concrete.</param>
        /// <returns>Compressive stress in MPa</returns>
        public abstract double CompressiveStress(PrincipalStrainState principalStrains);

        /// <summary>
        /// Calculate compressive stress for uniaxial case.
        /// </summary>
        /// <param name="strain">Compressive strain in concrete.</param>
        /// <returns>Compressive stress in MPa</returns>
        public abstract double CompressiveStress(double strain);

	    /// <summary>
	    /// Calculate current secant module.
	    /// </summary>
	    /// <param name="stress">Current stress in MPa.</param>
	    /// <param name="strain">Current strain.</param>
	    /// <returns>Secant module in MPa</returns>
	    public double SecantModule(double stress, double strain)
	    {
		    if (stress == 0 || strain == 0)
			    return Ec;
		    return
			    stress / strain;
	    }

	    /// <summary>
	    /// Check if concrete is cracked for uniaxial case and set cracked property.
	    /// </summary>
	    /// <param name="strain">Current strain</param>
	    public void VerifyCrackedState(double strain)
	    {
		    if (!Cracked && strain >= ecr)
			    Cracked = true;
	    }

	    /// <summary>
	    /// Check if concrete is cracked for biaxial case and set cracked property, from Gupta (1998) formulation.
	    /// </summary>
	    /// <param name="fc1">Principal tensile strain in MPa.</param>
	    /// <param name="ec2">Principal compressive strain.</param>
	    public void VerifyCrackedState(double fc1, double ec2)
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
        /// Compare two constitutive objects.
        /// </summary>
        /// <param name="other">The other constitutive object.</param>
        public abstract bool Equals(Constitutive other);

	    public override int GetHashCode() => Parameters.GetHashCode();

	    /// <summary>
	    /// Returns true if parameters are equal.
	    /// </summary>
	    public static bool operator == (Constitutive left, Constitutive right) => left.Equals(right);

	    /// <summary>
	    /// Returns true if parameters are different.
	    /// </summary>
	    public static bool operator != (Constitutive left, Constitutive right) => !left.Equals(right);
    }
}