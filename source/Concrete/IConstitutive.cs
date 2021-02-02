using System;
using Extensions;
using Material.Concrete.Biaxial;

namespace Material.Concrete
{
	/// <summary>
	///     Constitutive models for concrete.
	/// </summary>
	public enum ConstitutiveModel
	{
		Linear,
		MCFT,
		DSFM
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
		///		Get the <see cref="ConstitutiveModel"/>.
		/// </summary>
		ConstitutiveModel Model { get; }

		#endregion
	}
}