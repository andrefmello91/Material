using System;
using andrefmello91.Extensions;
using andrefmello91.Material.Reinforcement;
using UnitsNet;
using static UnitsNet.UnitMath;

#nullable enable

namespace andrefmello91.Material.Concrete
{
	public partial class UniaxialConcrete
	{
		/// <summary>
		///     DSFM constitutive class.
		/// </summary>
		private class DSFMConstitutive : Constitutive
		{

			#region Properties

			public override ConstitutiveModel Model { get; } = ConstitutiveModel.DSFM;

			#endregion

			#region Constructors

			/// <summary>
			///     DSFM constitutive object.
			/// </summary>
			/// <inheritdoc cref="Concrete" />
			/// <param name="considerCrackSlip">Consider crack slip? (default: true)</param>
			public DSFMConstitutive(IParameters parameters, bool considerCrackSlip = true) : base(parameters) => ConsiderCrackSlip = considerCrackSlip;

			#endregion

			#region Methods

			/// <summary>
			///     Calculate reference length.
			/// </summary>
			/// <inheritdoc cref="TensionStiffening" />
			private static Length ReferenceLength(UniaxialReinforcement? reinforcement) =>
				0.5 * (reinforcement is null
					? Length.FromMillimeters(21)
					: Length.FromMillimeters(21) + 0.155 * reinforcement.BarDiameter / reinforcement.Ratio);

			/// <summary>
			///     Calculate tension stiffening coefficient (for DSFM).
			/// </summary>
			/// <inheritdoc cref="TensionStiffening" />
			private static double TensionStiffeningCoefficient(UniaxialReinforcement? reinforcement) => reinforcement is null
				? 0
				: 0.25 * reinforcement.BarDiameter.Millimeters / reinforcement.Ratio;

			/// <inheritdoc />
			protected override Pressure CompressiveStress(double strain)
			{
				// Calculate the principal compressive stress in concrete
				// Get strains
				var ec2 = strain;

				// Calculate fp and ep
				var fp = -Parameters.Strength;
				var ep = Parameters.PlasticStrain;

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
			protected override Pressure TensileStress(double strain, UniaxialReinforcement? reinforcement = null)
			{
				// Check if concrete is cracked
				if (strain <= Parameters.CrackingStrain) // Not cracked
					return
						Parameters.ElasticModule * strain;

				// Cracked
				// Calculate concrete post-cracking stress associated with tension softening
				var fc1a = TensionSoftening(strain, reinforcement);

				// Calculate concrete postcracking stress associated with tension stiffening
				var fc1b = TensionStiffening(strain, reinforcement);

				// Return maximum
				return
					fc1a >= fc1b
						? fc1a
						: fc1b;
			}

			/// <summary>
			///     Calculate concrete post-cracking stress associated with tension softening.
			/// </summary>
			/// <inheritdoc cref="TensionStiffening" />
			private Pressure TensionSoftening(double strain, UniaxialReinforcement? reinforcement)
			{
				double
					Gf  = Parameters.FractureParameter.NewtonsPerMillimeter,
					ft  = Parameters.TensileStrength.Megapascals,
					ecr = Parameters.CrackingStrain,
					ets = 2.0 * Gf / (ft * ReferenceLength(reinforcement).Millimeters);

				return
					Parameters.TensileStrength * (1.0 - (strain - ecr) / (ets - ecr));
			}

			/// <summary>
			///     Calculate concrete post-cracking stress associated with tension stiffening (for
			///     <see cref="Material.Concrete.UniaxialConcrete" />).
			/// </summary>
			/// <param name="strain">The tensile strain to calculate stress.</param>
			/// <param name="reinforcement">The <see cref="UniaxialReinforcement" />.</param>
			private Pressure TensionStiffening(double strain, UniaxialReinforcement? reinforcement)
			{
				if (reinforcement is null)
					return Pressure.Zero;

				// Calculate coefficient for tension stiffening effect
				var m = TensionStiffeningCoefficient(reinforcement);

				// Calculate concrete postcracking stress associated with tension stiffening
				var fc1b = Parameters.TensileStrength / (1 + Math.Sqrt(2.2 * m * strain));

				// Check the maximum value of fc1 that can be transmitted across cracks
				var fc1s = reinforcement?.MaximumPrincipalTensileStress() ?? Pressure.Zero;

				// Return minimum
				return
					Min(fc1s, fc1b);
			}

			#endregion

		}
	}
}