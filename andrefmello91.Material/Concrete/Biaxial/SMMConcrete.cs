using andrefmello91.Extensions;
using andrefmello91.Material.Reinforcement;
using andrefmello91.OnPlaneComponents;
using MathNet.Numerics;
using UnitsNet;

namespace andrefmello91.Material.Concrete
{
	/// <summary>
	///		SMM concrete auxiliary class.
	/// </summary>
	internal class SMMConcrete : BiaxialConcrete
	{
		/// <summary>
		///     Get concrete <see cref="BiaxialConcrete.Constitutive" />.
		/// </summary>
		private new SMMConstitutive ConstitutiveEquations => (SMMConstitutive) base.ConstitutiveEquations;

		// /// <summary>
		// ///		The strain state in the average principal strain direction, not affected by Poisson effect.
		// /// </summary>
		// private StrainState NotAffectedStrains { get; set; }
		
		/// <inheritdoc />
		internal SMMConcrete(IParameters parameters)
			: base(parameters, ConstitutiveModel.SMM)
		{
			Strains  = new StrainState(0, 0, 0, Constants.PiOver4);
			Stresses = new StressState(0, 0, 0, Constants.PiOver4);
		}

		/// <inheritdoc />
		public override void CalculatePrincipalStresses(StrainState strains, WebReinforcement? reinforcement, Length? referenceLength = null)
		{
			// Update strains
			// var theta          = strains.ToPrincipal().Theta1 + DeviationAngle;
			Strains            = strains;
			PrincipalStrains   = Strains.ToPrincipal();
			
			// Calculate deviation angle
			DeviationAngle = CalculateDeviationAngle(Strains);
			
			// Calculate stresses
			Stresses          = ConstitutiveEquations.CalculateStresses(Strains, reinforcement, deviationAngle: DeviationAngle);
			PrincipalStresses = Stresses.ToPrincipal();
			
			// Update stresses
			UpdateStresses(reinforcement);
		}

		/// <summary>
		///		Update the stress state based in equilibrium on crack.
		/// </summary>
		private void UpdateStresses(WebReinforcement? reinforcement)
		{
			if (reinforcement is null)
				return;
			
			// Check the maximum value of fc1 that can be transmitted across cracks
			var fc1s = reinforcement.MaximumPrincipalTensileStress(PrincipalStresses.Theta1);

			if (fc1s >= PrincipalStresses.Sigma1)
				return;

			// Recalculate stresses
			var pStresses     = PrincipalStresses.Clone();
			PrincipalStresses = new PrincipalStressState(fc1s, pStresses.Sigma2, pStresses.Theta1);
			Stresses          = PrincipalStresses.Transform(DeviationAngle);
		}
		
		/// <summary>
		///		Calculate the deviation angle for a strain state.
		/// </summary>
		/// <param name="strains">The strain state for the principal direction of concrete.</param>
		private static double CalculateDeviationAngle(StrainState strains) => 0.5 * (strains.GammaXY / (strains.EpsilonX - strains.EpsilonY)).Atan();

	}
}