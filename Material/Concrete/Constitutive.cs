using System;
using Extensions.Number;

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
	    /// <param name="direction">The <see cref="Direction"/>.</param>
	    public static Constitutive Read(ConstitutiveModel constitutiveModel, Parameters parameters, Direction direction)
	    {
		    switch (direction)
		    {
				case Direction.Uniaxial:
					return Uniaxial.Constitutive.Read(constitutiveModel, parameters);

				default:
					return Biaxial.Constitutive.Read(constitutiveModel, parameters);
		    }
	    }

        /// <summary>
        /// Get the <see cref="ConstitutiveModel"/> based on <see cref="Constitutive"/> object.
        /// </summary>
        /// <param name="constitutive">The <see cref="Constitutive"/> object.</param>
        /// <returns></returns>
        public static ConstitutiveModel ReadModel(Constitutive constitutive)
	    {
		    if (constitutive is Biaxial.MCFTConstitutive || constitutive is Uniaxial.MCFTConstitutive)
			    return ConstitutiveModel.MCFT;

		    if (constitutive is Biaxial.DSFMConstitutive || constitutive is Uniaxial.DSFMConstitutive)
			    return ConstitutiveModel.DSFM;

		    return ConstitutiveModel.Linear;
	    }

	    /// <summary>
	    /// Calculate current secant module.
	    /// </summary>
	    /// <param name="stress">Current stress in MPa.</param>
	    /// <param name="strain">Current strain.</param>
	    public double SecantModule(double stress, double strain) => stress.Abs() <= 1E-6 || strain.Abs() <= 1E-9 ? Ec : stress / strain;

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