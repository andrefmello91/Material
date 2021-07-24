using andrefmello91.OnPlaneComponents;
using UnitsNet;

namespace andrefmello91.Material
{
	/// <summary>
	///		Interface for uniaxial material.
	/// </summary>
	public interface IUniaxialMaterial
	{
		/// <summary>
		///     The cross section area.
		/// </summary>
		Area Area { get; }

		/// <summary>
		///     The force at the cross-section.
		/// </summary>
		Force Force { get; }

		/// <summary>
		///     The strain at the cross-section.
		/// </summary>
		double Strain { get; }

		/// <summary>
		///     The stress at the cross-section.
		/// </summary>
		Pressure Stress { get; }

		/// <summary>
		///     Update strain and calculate stress in this material.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		void Calculate(double strain);
	}

	/// <summary>
	///		Interface for biaxial material.
	/// </summary>
	public interface IBiaxialMaterial
	{
		/// <summary>
		///     The principal strains at this material.
		/// </summary>
		PrincipalStrainState PrincipalStrains { get; }

		/// <summary>
		///     The principal stresses at this material.
		/// </summary>
		PrincipalStressState PrincipalStresses { get; }

		/// <summary>
		///     The strains at this material.
		/// </summary>
		StrainState Strains { get; }

		/// <summary>
		///     The stresses at this material.
		/// </summary>
		StressState Stresses { get; }

		/// <summary>
		///		The stiffness of this material.
		/// </summary>
		MaterialMatrix Stiffness { get; }

		/// <summary>
		///		Update strains and calculate stresses.
		/// </summary>
		/// <param name="strainState">The current strain state.</param>
		void Calculate(StrainState strainState);
	}
}