﻿using System;
using Extensions;
using Material.Reinforcement.Uniaxial;

namespace Material.Concrete.Uniaxial
{
	public partial class UniaxialConcrete
	{
		/// <summary>
		///     DSFM constitutive class.
		/// </summary>
		private class DSFMConstitutive : Constitutive
		{
			#region Fields

			private double? _refLength;

			#endregion

			#region Properties

			public override ConstitutiveModel Model { get; } = ConstitutiveModel.DSFM;

			#endregion

			#region Constructors

			/// <summary>
			///		DSFM constitutive object.
			/// </summary>
			/// <inheritdoc cref="Constitutive(IParameters)"/>
			/// <param name="considerCrackSlip">Consider crack slip? (default: true)</param>
			public DSFMConstitutive(IParameters parameters, bool considerCrackSlip = true) : base(parameters) => ConsiderCrackSlip = considerCrackSlip;

			#endregion

			#region

			/// <inheritdoc />
			protected override double TensileStress(double strain, UniaxialReinforcement reinforcement = null)
			{
				// Check if concrete is cracked
				if (strain <= Parameters.CrackingStrain) // Not cracked
					return
						Parameters.ElasticModule.Megapascals * strain;

				// Cracked
				// Calculate concrete post-cracking stress associated with tension softening
				var fc1a = TensionSoftening(strain, reinforcement);

				// Calculate concrete postcracking stress associated with tension stiffening
				var fc1b = TensionStiffening(strain, reinforcement);

				// Return maximum
				return
					Math.Max(fc1a, fc1b);
			}

			/// <inheritdoc />
			protected override double CompressiveStress(double strain)
			{
				// Calculate the principal compressive stress in concrete
				// Get strains
				var
					ec2 = strain;

				// Calculate fp and ep
				double
					fp = -Parameters.Strength.Megapascals,
					ep =  Parameters.PlasticStrain;

				// Calculate parameters of concrete
				double
					k = ep <= ec2 ? 1 : 0.67 - fp / 62,
					n = 0.8 - fp / 17,
					ec2_ep = ec2 / ep;

				// Calculate the principal compressive stress in concrete
				return
					fp * n * ec2_ep / (n - 1 + ec2_ep.Pow(n * k));
			}

			/// <summary>
			///     Calculate concrete post-cracking stress associated with tension stiffening (for <see cref="UniaxialConcrete" />).
			/// </summary>
			/// <param name="strain">The tensile strain to calculate stress.</param>
			/// <param name="reinforcement">The <see cref="UniaxialReinforcement" />.</param>
			private double TensionStiffening(double strain, UniaxialReinforcement reinforcement)
			{
				if (reinforcement is null)
					return 0;

				// Calculate coefficient for tension stiffening effect
				var m = reinforcement.TensionStiffeningCoefficient();

				// Calculate concrete postcracking stress associated with tension stiffening
				var fc1b = Parameters.TensileStrength.Megapascals / (1 + Math.Sqrt(2.2 * m * strain));

				// Check the maximum value of fc1 that can be transmitted across cracks
				var fc1s = reinforcement.MaximumPrincipalTensileStress();

				// Return minimum
				return
					Math.Min(fc1b, fc1s);
			}

			/// <summary>
			///     Calculate concrete post-cracking stress associated with tension softening.
			/// </summary>
			/// <param name="strain">The tensile strain to calculate stress.</param>
			/// <param name="reinforcement">The <see cref="UniaxialReinforcement" />.</param>
			private double TensionSoftening(double strain, UniaxialReinforcement reinforcement)
			{
				double
					Gf  = Parameters.FractureParameter.NewtonsPerMillimeter,
					ft  = Parameters.TensileStrength.Megapascals,
					ecr = Parameters.CrackingStrain,
					ets = 2.0 * Gf / (ft * ReferenceLength(reinforcement));

				return
					ft * (1.0 - (strain - ecr) / (ets - ecr));
			}

			/// <summary>
			///     Calculate reference length.
			/// </summary>
			/// <param name="reinforcement">The <see cref="UniaxialReinforcement" />.</param>
			private double ReferenceLength(UniaxialReinforcement reinforcement)
			{
				if (!_refLength.HasValue)
					_refLength = reinforcement is null ? 21 : 21 + 0.155 * reinforcement.BarDiameter / reinforcement.Ratio;

				return _refLength.Value;
			}

			public override string ToString() => "DSFM";

			/// <summary>
			///     Compare two constitutive objects.
			/// </summary>
			/// <param name="other">The other constitutive object.</param>
			public override bool Equals(IConstitutive other) => other is DSFMConstitutive;

			public override bool Equals(object other) => other is DSFMConstitutive;

			public override int GetHashCode() => base.GetHashCode();

			#endregion
		}
	}
}