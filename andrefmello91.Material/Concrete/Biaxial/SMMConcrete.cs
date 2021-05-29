using andrefmello91.Material.Reinforcement;
using andrefmello91.OnPlaneComponents;
using UnitsNet;

namespace andrefmello91.Material.Concrete
{
	/// <summary>
	///		SMM concrete auxiliary class.
	/// </summary>
	internal class SMMConcrete : BiaxialConcrete
	{
		/// <summary>
		///		The smeared strain state in the average principal strain direction.
		/// </summary>
		/// <remarks>
		///		Not affected by Poisson effect.
		/// </remarks>
		private StrainState SmearedStrains { get; set; }
		
		/// <summary>
		///		The smeared strain state in the average principal strain direction.
		/// </summary>
		/// <remarks>
		///		Affected by Poisson effect.
		/// </remarks>
		private StrainState AffectedSmearedStrains { get; set; }
		
		/// <summary>
		///		The smeared stress state in the average principal strain direction.
		/// </summary>
		private StressState SmearedStresses{ get; set; }
		
		/// <inheritdoc />
		internal SMMConcrete(IParameters parameters)
			: base(parameters, ConstitutiveModel.SMM)
		{
		}

		/// <inheritdoc />
		public override void CalculatePrincipalStresses(StrainState strains, WebReinforcement? reinforcement, Length? referenceLength = null)
		{
			
		}

		/// <summary>
		///		Calculate the strain state affected by Poisson ratios.
		/// </summary>
		/// <param name="smearedStrains">The smeared strain state.</param>
		/// <inheritdoc cref="BiaxialConcrete.SMMConstitutive.PoissonCoefficients"/>
		private static StrainState CalculatePoissonEffect(StrainState smearedStrains, WebReinforcement? reinforcement, bool cracked)
		{
			// Get initial strains
			var e1i = smearedStrains.EpsilonX;
			var e2i = smearedStrains.EpsilonY;
			
			// Get coefficients
			var (v12, v21) = SMMConstitutive.PoissonCoefficients(reinforcement, cracked);
			
			// Calculate strains
			var v1 = 1D / (1D - v12 * v21);
			var v2 = v21 * v1;

			var e1 = v1 * e1i + v2 * e2i;
			var e2 = v2 * e1i + v1 * e2i;

			return new StrainState(e1, e2, smearedStrains.GammaXY);
		}

		/// <summary>
		///		Calculate the smeared shear stress in concrete.
		/// </summary>
		/// <param name="smearedStrains">The smeared strain state in the average principal strain direction. Not affected by Poisson effect.</param>
		/// <param name="smearedStresses">The smeared stress state in the average principal strain direction, calculated from constitutive model.</param>
		/// <returns></returns>
		private static Pressure SmearedShearStress(StrainState smearedStrains, StressState smearedStresses)
		{
			var s1  = smearedStresses.SigmaX;
			var s2  = smearedStresses.SigmaY;
			var e1  = smearedStrains.EpsilonX;
			var e2  = smearedStrains.EpsilonY;
			var yxy = smearedStrains.GammaXY;

			return
				yxy * (s1 - s2) / (2 * (e1 - e2));
		}
	}
}