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
		private class SMMConstitutive : Constitutive
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

			/// <inheritdoc />
			protected override Pressure CompressiveStress(double strain, double transverseStrain, double confinementFactor = 1)
			{
				if (!strain.IsFinite() || !transverseStrain.IsFinite() || strain >= 0)
					return Pressure.Zero;

				// Get strains
				double
					ec1 = transverseStrain,
					ec2 = strain,
					ec  = Parameters.PlasticStrain;

				var fc = Parameters.Strength;

				// Calculate the maximum concrete compressive stress
				Pressure
					f2maxA = ec1 > 0
						? -fc / (0.8 - 0.34 * ec1 / ec)
						: -fc,
					f2max = f2maxA.Value < 0 && f2maxA.Value.IsFinite()
						? Max(f2maxA, -fc) * confinementFactor
						: -fc * confinementFactor;

				// Calculate the principal compressive stress in concrete
				var n = ec2 / ec;

				return
					f2max * (2 * n - n * n).AsFinite();
			}

			/// <inheritdoc />
			protected override Pressure TensileStress(double strain, double transverseStrain, double theta1 = Constants.PiOver4, Length? referenceLength = null, WebReinforcement? reinforcement = null)
			{
				if (!strain.IsFinite() || strain <= 0)
					return Pressure.Zero;

				// Calculate initial uncracked state
				var fc1 = UncrackedStress(strain, transverseStrain, theta1, reinforcement);

				// Not cracked
				return
					!Cracked
						? fc1
						: CrackedStress(strain);
			}

			/// <summary>
			///     Calculate tensile stress for cracked concrete.
			/// </summary>
			/// <param name="strain">Current tensile strain.</param>
			private Pressure CrackedStress(double strain) => Parameters.TensileStrength / (1 + Math.Sqrt(500 * strain));

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

			/// <summary>
			///		Calculate the deviation angle for a strain state.
			/// </summary>
			/// <param name="strains">The strain state for the principal direction of concrete.</param>
			private static double DeviationAngle(StrainState strains) => 0.5 * (strains.GammaXY / (strains.EpsilonX - strains.EpsilonY)).Atan();

			private Pressure CompressiveStress(StrainState strainState)
			{
				var ec1 = Math.Max(strainState.EpsilonX, strainState.EpsilonY);
				var ec2 = Math.Min(strainState.EpsilonX, strainState.EpsilonY);
				
				// Calculate beta
				var beta = DeviationAngle(strainState);
				
				// Calculate softening coefficient
				var soft = TensileStrainFunction(ec1) * _strengthFunction * DeviationFunction(beta);
				
				// Calculate peak stress and strain
				var fp = -soft * Parameters.Strength;
				var ep =  soft * Parameters.PlasticStrain;

				// Calculate strain ratio:
				var e2_ep = ec2 / ep;

				return 
					(e2_ep <= 1) switch
					{
						{ } when e2_ep < 0 => Pressure.Zero,

						// Pre-peak
						true => fp * (2 * e2_ep - e2_ep * e2_ep),
						
						// Post-peak
						_    => fp * (1D - ((e2_ep - 1D) / (4D / soft - 1D)).Pow(2))
					};
			}
			
			#endregion

		}
	}
}