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
		private double _devAngle;
		
		/// <summary>
		///     Get concrete <see cref="BiaxialConcrete.Constitutive" />.
		/// </summary>
		private new SMMConstitutive ConstitutiveEquations => (SMMConstitutive) base.ConstitutiveEquations;

		/// <summary>
		///		The strain state in the average principal strain direction, not affected by Poisson effect.
		/// </summary>
		private StrainState NotAffectedStrains { get; set; }
		
		/// <inheritdoc />
		internal SMMConcrete(IParameters parameters)
			: base(parameters, ConstitutiveModel.SMM)
		{
			Strains  = NotAffectedStrains = new StrainState(0, 0, 0, Constants.PiOver4);
			Stresses = new StressState(0, 0, 0, Constants.PiOver4);
		}

		/// <inheritdoc />
		public override void CalculatePrincipalStresses(StrainState strains, WebReinforcement? reinforcement, Length? referenceLength = null)
		{
			// Update strains
			var theta          = strains.ToPrincipal().Theta1 + _devAngle;
			Strains            = strains.Transform(theta);
			NotAffectedStrains = CalculatePoissonEffect(Strains, reinforcement, Cracked);
			PrincipalStrains   = Strains.ToPrincipal();
			
			// Calculate deviation angle
			_devAngle = DeviationAngle(Strains);
			
			// Calculate stresses
			Stresses          = ConstitutiveEquations.CalculateStresses(NotAffectedStrains, reinforcement, deviationAngle: _devAngle);
			PrincipalStresses = Stresses.ToPrincipal();
		}

		/// <summary>
		///		Calculate the strain state affected by Poisson ratios.
		/// </summary>
		/// <param name="smearedStrains">The smeared strain state.</param>
		/// <inheritdoc cref="BiaxialConcrete.SMMConstitutive.PoissonCoefficients"/>
		/// <returns>
		///		The <see cref="StrainState"/> without Poisson effect.
		/// </returns>
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

			return new StrainState(e1, e2, smearedStrains.GammaXY, smearedStrains.ThetaX);
		}
		
		/// <summary>
		///		Calculate the deviation angle for a strain state.
		/// </summary>
		/// <param name="strains">The strain state for the principal direction of concrete.</param>
		private static double DeviationAngle(StrainState strains) => 0.5 * (strains.GammaXY / (strains.EpsilonX - strains.EpsilonY)).Atan();

	}
}