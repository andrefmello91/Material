using System;
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
	public class SMMConcrete : BiaxialConcrete
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
		internal SMMConcrete(IConcreteParameters parameters)
			: base(parameters, ConstitutiveModel.SMM)
		{
			Strains  = new StrainState(0, 0, 0, Constants.PiOver4);
			Stresses = new StressState(0, 0, 0, Constants.PiOver4);
		}

		/// <inheritdoc />
		public override void Calculate(StrainState strains, WebReinforcement? reinforcement, Length? referenceLength = null)
		{
			// Update strains
			// var theta          = strains.ToPrincipal().Theta1 + DeviationAngle;
			Strains            = StrainsAtPrincipal(strains, Stresses);
			PrincipalStrains   = Strains.ToPrincipal();
			
			// Calculate deviation angle
			DeviationAngle = CalculateDeviationAngle(Strains);

			var noPoisson = RemovePoissonEffect(Strains, reinforcement, Cracked);
			
			// Calculate stresses
			var calculatedStresses = ConstitutiveEquations.CalculateStresses(noPoisson, reinforcement, deviationAngle: DeviationAngle);
			Stresses               = StressesAtPrincipal(Strains, calculatedStresses);
			PrincipalStresses      = Stresses.ToPrincipal();
			
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
		private static double CalculateDeviationAngle(StrainState strains) => 0.5 * (strains.GammaXY / (strains.EpsilonX - strains.EpsilonY)).Atan().AsFinite();

		public void UpdateShearStress(StressState averageStresses, StressState reinforcementStresses)
		{
			Pressure
				sx  = averageStresses.SigmaX,
				sy  = averageStresses.SigmaY,
				s1  = Stresses.SigmaX,
				s2  = Stresses.SigmaY,
				fsx = reinforcementStresses.SigmaX,
				fsy = reinforcementStresses.SigmaY;

			var theta      = Stresses.ThetaX;
			var (cos, sin) = (2 * theta).DirectionCosines();

			var tau = sin.ApproxZero()
				? Pressure.Zero
				: 0.5 * (fsx - fsy - sx + sy + (s1 - s2) * cos) / sin;

			Stresses = new StressState(s1, s2, tau, theta);
		}
		
		private static StressState StressesAtPrincipal(StrainState strainsAtPrincipal, StressState calculatedStresses)
		{
			if (strainsAtPrincipal.EpsilonX < strainsAtPrincipal.EpsilonY)
				strainsAtPrincipal = strainsAtPrincipal.Transform(Constants.PiOver2);

			double
				e1  = strainsAtPrincipal.EpsilonX,
				e2  = strainsAtPrincipal.EpsilonY,
				y21 = strainsAtPrincipal.GammaXY;

			Pressure
				s1 = UnitMath.Max(calculatedStresses.SigmaX, calculatedStresses.SigmaY),
				s2 = UnitMath.Min(calculatedStresses.SigmaX, calculatedStresses.SigmaY);

			// Calculate shear stress
			var tau = e1.Approx(e2)
				? Pressure.Zero
				: 0.5 * y21 * (s1 - s2) / (e1 - e2);

			return
				new StressState(s1, s2, tau, strainsAtPrincipal.ThetaX);
		}
		
		private static StrainState StrainsAtPrincipal(StrainState appliedStrains)
		{
			var principal = appliedStrains.ToPrincipal();

			double
				ex  = appliedStrains.EpsilonX,
				ey  = appliedStrains.EpsilonY,
				e1  = principal.Epsilon1,
				e2  = principal.Epsilon2,
				sin = (2 * principal.Theta2).DirectionCosines().sin,
				tan = (2 * principal.Theta2).Tan();

			var y21 = (ey - ex) / sin + (e1 - e2) / tan;

			return
				new StrainState(e1, e2, y21, principal.Theta1);
		}

		private static StrainState StrainsAtPrincipal(StrainState appliedStrains, StressState stressesAtPrincipal)
		{
			var principal = appliedStrains.ToPrincipal();

			double
				e1 = principal.Epsilon1,
				e2 = principal.Epsilon2;

			if (stressesAtPrincipal.SigmaX <= stressesAtPrincipal.SigmaY)
				stressesAtPrincipal = stressesAtPrincipal.Transform(Constants.PiOver2);
			
			Pressure
				s1  = stressesAtPrincipal.SigmaX,
				s2  = stressesAtPrincipal.SigmaY,
				t21 = stressesAtPrincipal.TauXY;

			var y21 = 2 * t21 * (e1 - e2) / (s1 - s2);

			return
				new StrainState(e1, e2, y21, principal.Theta1);
		}

		/// <summary>
		///		Calculate the strain state affected by Poisson ratios.
		/// </summary>
		/// <param name="strainsAtAvgPrincipal">The strain state in concrete, at the average principal strain direction of the membrane element.</param>
		/// <param name="reinforcement">The reinforcement.</param>
		/// <param name="cracked">The cracked state of concrete. True if cracked.</param>
		/// <returns>
		///		The <see cref="andrefmello91.OnPlaneComponents.StrainState"/> without Poisson effect.
		/// </returns>
		public static StrainState RemovePoissonEffect(StrainState strainsAtAvgPrincipal, WebReinforcement? reinforcement, bool cracked)
		{
			// Get initial strains
			var e1i = strainsAtAvgPrincipal.EpsilonX;
			var e2i = strainsAtAvgPrincipal.EpsilonY;
			
			// Get coefficients
			var (v12, v21) = PoissonCoefficients(reinforcement, cracked);
			
			// Calculate strains
			var v1 = 1D / (1D - v12 * v21);
			var v2 = v21 * v1;

			var e1 = v1 * e1i + v2 * e2i;
			var e2 = v2 * e1i + v1 * e2i;

			return new StrainState(e1, e2, strainsAtAvgPrincipal.GammaXY, strainsAtAvgPrincipal.ThetaX);
		}

		/// <summary>
		///		Calculate the Poisson coefficients for SMM.
		/// </summary>
		/// <param name="reinforcement">The reinforcement.</param>
		/// <param name="cracked">The cracked state of concrete. True if cracked.</param>
		private static (double v12, double v21) PoissonCoefficients(WebReinforcement? reinforcement, bool cracked)
		{
			var v21 = cracked
				? 0
				: 0.2;

			if (reinforcement is null)
				return (0.2, v21);

			var strains = reinforcement.Strains;
				
			var esf = Math.Max(strains.EpsilonX, strains.EpsilonY);
				
			var ey = strains.EpsilonX >= strains.EpsilonY
				? reinforcement.DirectionX?.Steel.Parameters.YieldStrain
				: reinforcement.DirectionY?.Steel.Parameters.YieldStrain;

			var v12 = esf <= 0 || !ey.HasValue
				? 0.2
				: 0.2 + 850 * esf;

			return (v12, v21);
		}
	}
}