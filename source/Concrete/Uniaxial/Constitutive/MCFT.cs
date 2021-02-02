using System;
using Material.Reinforcement.Uniaxial;

namespace Material.Concrete.Uniaxial
{
	public partial class UniaxialConcrete
	{
		/// <summary>
		///     MCFT constitutive class.
		/// </summary>
		private class MCFTConstitutive : Constitutive
		{
			#region Constructors

			/// <summary>
			///		MCFT constitutive object.
			/// </summary>
			/// <inheritdoc cref="Constitutive(IParameters)"/>
			public MCFTConstitutive(IParameters parameters) : base(parameters)
			{
			}

			#endregion

			#region

			/// <inheritdoc />
			protected override double CompressiveStress(double strain)
			{
				double
					ec = Parameters.PlasticStrain,
					fc = Parameters.Strength.Megapascals,
					n  = strain / ec;

				return
					-fc * (2 * n - n * n);
			}

			public override ConstitutiveModel Model { get; } = ConstitutiveModel.MCFT;

			/// <inheritdoc />
			protected override double TensileStress(double strain, UniaxialReinforcement reinforcement = null) =>
				strain <= Parameters.CrackingStrain
					? strain * Parameters.ElasticModule.Megapascals
					: CrackedStress(strain);

			/// <summary>
			///     Calculate tensile stress, in MPa, for cracked concrete.
			/// </summary>
			/// <param name="strain">Current tensile strain.</param>
			private double CrackedStress(double strain) => Parameters.TensileStrength.Megapascals / (1 + Math.Sqrt(500 * strain));


			#endregion
		}
	}
}