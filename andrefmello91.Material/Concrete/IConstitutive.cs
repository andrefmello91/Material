using System;

namespace andrefmello91.Material.Concrete
{
	/// <summary>
	///     Constitutive models for concrete.
	/// </summary>
	public enum ConstitutiveModel
	{
		/// <summary>
		///     Modified Compression Field constitutive model.
		/// </summary>
		MCFT,

		/// <summary>
		///     Disturbed Stress Field constitutive model.
		/// </summary>
		DSFM,
		
		/// <summary>
		///     Softened Membrane Model constitutive model.
		/// </summary>
		SMM
	}

	/// <summary>
	///     Base class for concrete constitutive model.
	/// </summary>
	public interface IConstitutive : IEquatable<IConstitutive>
	{

		#region Properties

		/// <summary>
		///     Get/set crack slip consideration.
		/// </summary>
		bool ConsiderCrackSlip { get; }

		/// <summary>
		///     Get/set concrete cracked state.
		/// </summary>
		bool Cracked { get; set; }

		/// <summary>
		///     Get the <see cref="ConstitutiveModel" />.
		/// </summary>
		ConstitutiveModel Model { get; }

		#endregion

	}
}