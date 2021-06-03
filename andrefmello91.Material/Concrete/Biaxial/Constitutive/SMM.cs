using System;
using andrefmello91.Extensions;
using andrefmello91.Material.Reinforcement;
using andrefmello91.OnPlaneComponents;
using MathNet.Numerics;
using UnitsNet;
using static UnitsNet.UnitMath;

#nullable enable

namespace andrefmello91.Material.Concrete
{
	public partial class BiaxialConcrete
	{
		/// <summary>
		///     SMM constitutive class.
		/// </summary>
		protected class SMMConstitutive : Constitutive
		{

			/// <summary>
			///	<inheritdoc cref="StrengthFunction"/>
			/// </summary>
			private double _strengthFunction;
			
			#region Properties

			public override ConstitutiveModel Model { get; } = ConstitutiveModel.SMM;

			#endregion

			#region Constructors

			/// <summary>
			///     MCFT constitutive object.
			/// </summary>
			/// <inheritdoc cref="Constitutive(IParameters)" />
			public SMMConstitutive(IParameters parameters) : base(parameters)
			{
				_strengthFunction = StrengthFunction(parameters.Strength);
			}

			#endregion

			#region Methods

			// /// <inheritdoc cref="Constitutive.CalculateStresses"/>
			// /// <inheritdoc cref="ConfinementStresses"/>
			// public StressState CalculateStresses(StrainState affectedStrains, WebReinforcement? reinforcement, double deviationAngle)
			// {
			// 	if (affectedStrains.IsZero)
			// 		return StressState.Zero;
			//
			// 	// Get strains
			// 	double
			// 		ec1 = affectedStrains.EpsilonX.AsFinite(),
			// 		ec2 = affectedStrains.EpsilonY.AsFinite(),
			// 		yxy = affectedStrains.GammaXY.AsFinite();
			//
			// 	// Get the case
			// 	var pCase = new PrincipalStrainState(ec1, ec2).Case;
			// 	
			// 	Pressure fc1, fc2;
			//
			// 	switch (pCase)
			// 	{
			// 		// Verify case
			// 		case PrincipalCase.UniaxialCompression:
			// 		case PrincipalCase.TensionCompression:
			// 			fc1 = TensileStress(ec1, ec2, affectedStrains.ThetaX, reinforcement: reinforcement);
			// 			fc2 = CompressiveStress(ec2, ec1, deviationAngle, 1);
			// 			break;
			//
			// 		case PrincipalCase.UniaxialTension:
			// 		case PrincipalCase.PureTension:
			// 			fc1 = TensileStress(ec1, ec2, affectedStrains.ThetaX, reinforcement: reinforcement);
			// 			fc2 = TensileStress(ec2, ec1, affectedStrains.ThetaX, reinforcement: reinforcement);
			// 			break;
			//
			// 		case PrincipalCase.PureCompression when !Parameters.ConsiderConfinement:
			// 			fc1 = CompressiveStress(ec1, ec2, deviationAngle, 1);
			// 			fc2 = CompressiveStress(ec2, ec1, deviationAngle, 1);
			// 			break;
			//
			// 		case PrincipalCase.PureCompression when Parameters.ConsiderConfinement:
			// 			var conf = ConfinementStresses(affectedStrains, deviationAngle);
			// 			fc1 = conf.SigmaX;
			// 			fc2 = conf.SigmaY;
			// 			break;
			//
			// 		default:
			// 			return StressState.Zero;
			// 	}
			//
			// 	// Calculate shear stress
			// 	var tau = 0.5 * yxy * (fc1 - fc2) / (ec1 - ec2);
			// 	
			// 	return
			// 		new StressState(fc1, fc2, tau, affectedStrains.ThetaX);
			// }
			//
			// /// <summary>
			// ///     Calculate confinement <see cref="PrincipalStressState" />.
			// /// </summary>
			// /// <param name="affectedStrains">The smeared strains in concrete, affected by Poisson effect, at the direction of average principal strains.</param>
			// /// <param name="deviationAngle">The deviation angle between applied principal stresses and concrete principal stresses.</param>
			// private StressState ConfinementStresses(StrainState affectedStrains, double deviationAngle)
			// {
			// 	// Get strains
			// 	double
			// 		ec1 = affectedStrains.EpsilonX,
			// 		ec2 = affectedStrains.EpsilonY;
			//
			// 	// Calculate initial stresses
			// 	Pressure
			// 		fc1 = CompressiveStress(ec1, ec2, deviationAngle, 1),
			// 		fc2 = CompressiveStress(ec2, ec1, deviationAngle, 1);
			//
			// 	var tol = Pressure.FromMegapascals(0.01);
			//
			// 	// Iterate to find stresses (maximum 20 iterations)
			// 	for (var it = 1; it <= 20; it++)
			// 	{
			// 		// Calculate confinement factors
			// 		double
			// 			betaL1 = ConfinementFactor(fc2, Parameters.Strength),
			// 			betaL2 = ConfinementFactor(fc1, Parameters.Strength);
			//
			// 		// Calculate iteration stresses
			// 		Pressure
			// 			fc1It = CompressiveStress(ec1, ec2, deviationAngle, betaL1),
			// 			fc2It = CompressiveStress(ec2, ec1, deviationAngle, betaL2);
			//
			// 		// Verify tolerances
			//
			// 		if ((fc1 - fc1It).Abs() <= tol && (fc2 - fc2It).Abs() <= tol)
			// 			break;
			//
			// 		// Update stresses
			// 		fc1 = fc1It;
			// 		fc2 = fc2It;
			// 	}
			//
			// 	return
			// 		new StressState(fc1, fc2, Pressure.Zero, affectedStrains.ThetaX);
			// }
			
			/// <inheritdoc />
			protected override Pressure CrackedStress(double strain, double theta1, WebReinforcement? reinforcement, Length? referenceLength = null)
			{
				var fc1 = Parameters.TensileStrength * (Parameters.CrackingStrain / strain).Pow(0.4);

				if (reinforcement is null)
					return fc1;
				
				// Check the maximum value of fc1 that can be transmitted across cracks
				var fc1s = reinforcement.MaximumPrincipalTensileStress(theta1);

				// Return minimum
				return
					Min(fc1, fc1s);
			}

			/// <summary>
			///		Calculate the Poisson coefficients for SMM.
			/// </summary>
			/// <param name="reinforcement">The reinforcement.</param>
			/// <param name="cracked">The cracked state of concrete. True if cracked.</param>
			public static (double v12, double v21) PoissonCoefficients(WebReinforcement? reinforcement, bool cracked)
			{
				var v21 = cracked
					? 0
					: 0.2;

				if (reinforcement is null)
					return (0.2, v21);

				var strains = reinforcement.Strains;
				
				var esf     = Math.Max(strains.EpsilonX, strains.EpsilonY);
				
				var ey = strains.EpsilonX >= strains.EpsilonY
					? reinforcement.DirectionX?.Steel.YieldStrain
					: reinforcement.DirectionY?.Steel.YieldStrain;

				var v12 = esf <= 0 || !ey.HasValue
					? 0.2
					: 0.2 + 850 * esf;

				return (v12, v21);
			}
			
			/// <summary>
			///		Calculate the tensile strain function for the softening parameter.
			/// </summary>
			/// <param name="epsilon1">The average principal tensile strain, not considering Poisson effect.</param>
			private static double TensileStrainFunction(double epsilon1) => 1D / (1D + 400 * epsilon1).Sqrt();

			/// <summary>
			///		Calculate the strength function for the softening parameter.
			/// </summary>
			/// <param name="strength">The compressive strength of concrete.</param>
			private static double StrengthFunction(Pressure strength) => Math.Min(5.8 / strength.Megapascals.Sqrt(), 0.9);
			
			/// <summary>
			///		Calculate the deviation angle function for the softening parameter.
			/// </summary>
			/// <param name="deviationAngle">The deviation angle between applied principal stresses and concrete principal stresses.</param>
			private static double DeviationFunction(double deviationAngle) => 1D - deviationAngle.Abs() / 0.418879;
			
			///  <summary>
			/// 		Calculate compressive stress.
			///  </summary>
			///  <inheritdoc cref="CompressiveStress(double,double,double)"/>
			protected override Pressure CompressiveStress(double strain, double transverseStrain, double deviationAngle = 0, double confinementFactor = 1)
			{
				// Calculate softening coefficient
				var soft = (TensileStrainFunction(transverseStrain) * _strengthFunction * DeviationFunction(deviationAngle)).AsFinite();
				
				// Calculate peak stress and strain
				var fp = -soft * Parameters.Strength;
				var ep =  soft * Parameters.PlasticStrain;

				// Calculate strain ratio:
				var e2_ep = (strain / ep).AsFinite();

				return 
					(e2_ep <= 1) switch
					{
						{ } when e2_ep < 0 => Pressure.Zero,

						// Pre-peak
						true => fp * (2 * e2_ep - e2_ep * e2_ep) * confinementFactor,
						
						// Post-peak
						_    => fp * (1D - ((e2_ep - 1D) / (4D / soft - 1D)).Pow(2)) * confinementFactor
					};
			}
			
			#endregion

		}
	}
}