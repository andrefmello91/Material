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
	    public override double CompressiveStress(double strain)
	    {
		    double n = strain / ec;

		    return
			    -fc * (2 * n - n * n);
	    }

	    // Calculate tensile stress in concrete
	    /// <inheritdoc/>
	    public override double TensileStress(double strain, double referenceLength = 0, UniaxialReinforcement reinforcement = null)
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
	    // Principal stresses by classic formulation
	    /// <inheritdoc/>
	    public override double CompressiveStress(PrincipalStrainState principalStrains)
	    {
		    // Get strains
		    double
			    ec1 = principalStrains.Epsilon1,
			    ec2 = principalStrains.Epsilon2;

            // Calculate the maximum concrete compressive stress
            double
                f2maxA = -fc / (0.8 - 0.34 * ec1 / ec),
			    f2max = Math.Max(f2maxA, -fc);

		    // Calculate the principal compressive stress in concrete
		    double n = ec2 / ec;

		    return
			    f2max * (2 * n - n * n);
	    }

	    /// <inheritdoc/>
	    public override double TensileStress(PrincipalStrainState principalStrains, double referenceLength = 0, BiaxialReinforcement reinforcement = null)
	    {
		    // Get strains
		    double
			    ec1 = principalStrains.Epsilon1,
			    ec2 = principalStrains.Epsilon2;

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
        public override double TensileStress(double strain, double referenceLength = 0, UniaxialReinforcement reinforcement = null)
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
        public override double CompressiveStress(double strain)
        {
            // Calculate the principal compressive stress in concrete
            return
                CompressiveStress(new PrincipalStrainState(0, strain));
        }
        #endregion

        #region Biaxial
        /// <inheritdoc/>
        public override double CompressiveStress(PrincipalStrainState principalStrains)
        {
            // Get strains
            double
	            ec1 = principalStrains.Epsilon1,
	            ec2 = principalStrains.Epsilon2;

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
                ep = betaD * ec;

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
        public override double TensileStress(PrincipalStrainState principalStrains, double referenceLength = 0, BiaxialReinforcement reinforcement = null)
        {
	        // Get strains
	        double
		        ec1 = principalStrains.Epsilon1,
		        ec2 = principalStrains.Epsilon2;

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
            double m = reinforcement.TensionStiffeningCoefficient(principalStrains.Theta1);

            // Calculate concrete postcracking stress associated with tension stiffening
            double fc1b = ft / (1 + Math.Sqrt(2.2 * m * ec1));

            // Calculate maximum tensile stress
            double fc1c = Math.Max(fc1a, fc1b);

            // Check the maximum value of fc1 that can be transmitted across cracks
            double fc1s = reinforcement.MaximumPrincipalTensileStress(principalStrains.Theta1);

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