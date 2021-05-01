using System;
using andrefmello91.Extensions;
using andrefmello91.Material.Reinforcement;
using MathNet.Numerics;
using UnitsNet;
using static UnitsNet.UnitMath;

#nullable enable

namespace andrefmello91.Material.Concrete
{
	public partial class BiaxialConcrete
	{
		/// <summary>
		///     DSFM constitutive class.
		/// </summary>
		private class DSFMConstitutive : Constitutive
		{

			#region Properties

			public override ConstitutiveModel Model { get; }

			#endregion

			#region Constructors

			//
			/// <summary>
			///     DSFM constitutive object.
			/// </summary>
			/// <param name="considerCrackSlip">Consider crack slip (default: true)</param>
			/// <inheritdoc cref="Constitutive(IParameters)" />
			public DSFMConstitutive(IParameters parameters, bool considerCrackSlip = true) : base(parameters) => ConsiderCrackSlip = considerCrackSlip;

			#endregion

			#region Methods

			/// <summary>
			///     Calculate tension stiffening coefficient (for DSFM).
			/// </summary>
			/// <inheritdoc cref="TensionStiffening" />
			private static double TensionStiffeningCoefficient(WebReinforcement? reinforcement, double theta1)
			{
				var x = reinforcement?.DirectionX;
				var y = reinforcement?.DirectionY;

				if (reinforcement is null || x is null && y is null)
					return 0;

				// Get reinforcement angles and stresses
				var (thetaNx, thetaNy) = reinforcement.Angles(theta1);

				double den = 0;

				if (x is not null)
				{
					double
						psx   = x.Ratio,
						phiX  = x.BarDiameter.Millimeters,
						cosNx = thetaNx.Cos(true);

					den += psx / phiX * cosNx;
				}

				if (y is not null)
				{
					double
						psy   = y.Ratio,
						phiY  = y.BarDiameter.Millimeters,
						cosNy = thetaNy.Cos(true);

					den += psy / phiY * cosNy;
				}

				// Return m
				return
					0.25 / den;
			}

			/// <inheritdoc />
			protected override Pressure CompressiveStress(double strain, double transverseStrain, double confinementFactor = 1)
			{
				if (!strain.IsFinite() || strain >= 0)
					return Pressure.Zero;

				var fc = Parameters.Strength;

				// Get strains
				double
					ec1 = transverseStrain,
					ec2 = strain,
					ec  = Parameters.PlasticStrain;

				// Calculate beta D
				var betaD = SofteningFactor(ec2, ec1);

				// Calculate fp and ep
				var fp = -betaD * fc * confinementFactor;
				var ep = betaD * ec * confinementFactor;

				// Calculate parameters of concrete
				double
					k = ep <= ec2
						? 1
						: 0.67 - fp.Megapascals / 62,
					n      = 0.8 - fp.Megapascals / 17,
					ec2_ep = ec2 / ep;

				// Calculate the principal compressive stress in concrete
				return
					fp * n * ec2_ep / (n - 1 + ec2_ep.Pow(n * k));
			}

			/// <inheritdoc />
			protected override Pressure TensileStress(double strain, double transverseStrain, double theta1 = Constants.PiOver4, Length? referenceLength = null, WebReinforcement? reinforcement = null)
			{
				if (!strain.IsFinite() || strain <= 0)
					return Pressure.Zero;

				// Get strains
				double
					ec1 = strain,
					ec2 = transverseStrain;

				// Calculate initial uncracked state
				var fc1 = UncrackedStress(ec1, ec2, theta1, reinforcement);

				// Not cracked
				if (!Cracked)
					return fc1;

				// Cracked
				// Calculate concrete post-cracking stress associated with tension softening
				var fc1a = TensionSoftening(ec1, referenceLength!.Value);

				// Calculate concrete post-cracking stress associated with tension stiffening.
				var fc1b = TensionStiffening(ec1, theta1, reinforcement);

				// Return maximum
				return
					Max(fc1a, fc1b);
			}

			/// <summary>
			///     Calculate compression softening factor (beta D).
			/// </summary>
			/// <param name="strain">The compressive strain (negative) to calculate stress.</param>
			/// <param name="transverseStrain">The strain at the transverse direction to <paramref name="strain" />.</param>
			private double SofteningFactor(double strain, double transverseStrain)
			{
				// Calculate strain ratio
				var r = Math.Min(-transverseStrain / strain, 400);

				if (r < 0.28) // Cd = 0
					return 1;

				// Calculate Cd and Cs
				var Cd = 0.35 * (r - 0.28).Pow(0.8);

				return
					Math.Min(1.0 / (1 + Cs * Cd), 1);
			}

			/// <summary>
			///     Calculate concrete post-cracking stress associated with tension softening.
			/// </summary>
			/// <param name="strain">The tensile strain to calculate stress.</param>
			/// <param name="referenceLength">The reference length.</param>
			private Pressure TensionSoftening(double strain, Length referenceLength)
			{
				var ft = Parameters.TensileStrength;

				double
					Gf  = Parameters.FractureParameter.NewtonsPerMillimeter,
					ecr = Parameters.CrackingStrain,
					ets = 2.0 * Gf / (ft.Megapascals * referenceLength.Millimeters);

				return
					ft * (1.0 - (strain - ecr) / (ets - ecr));
			}

			/// <summary>
			///     Calculate concrete post-cracking stress associated with tension stiffening (for
			///     <see cref="Material.Concrete.BiaxialConcrete" />).
			/// </summary>
			/// <param name="strain">The tensile strain to calculate stress.</param>
			/// <param name="theta1">The angle of maximum principal strain, in radians.</param>
			/// <param name="reinforcement">The <see cref="WebReinforcement" />.</param>
			private Pressure TensionStiffening(double strain, double theta1, WebReinforcement? reinforcement)
			{
				// Calculate coefficient for tension stiffening effect
				var m = TensionStiffeningCoefficient(reinforcement, theta1);

				// Calculate concrete postcracking stress associated with tension stiffening
				var fc1b = Parameters.TensileStrength / (1 + (2.2 * m * strain).Sqrt());

				// Check the maximum value of fc1 that can be transmitted across cracks
				var fc1s = reinforcement?.MaximumPrincipalTensileStress(theta1) ?? Pressure.Zero;

				// Return minimum
				return
					Min(fc1b, fc1s);
			}

			#endregion

		}
	}
}