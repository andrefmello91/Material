using System;
using Material.Reinforcement.Biaxial;
using MathNet.Numerics;

namespace Material.Concrete.Biaxial
{
	public partial class BiaxialConcrete
	{
		/// <summary>
		///     MCFT constitutive class.
		/// </summary>
		private class MCFTConstitutive : Constitutive
		{
			#region Properties

			public override ConstitutiveModel Model { get; } = ConstitutiveModel.MCFT;

			#endregion

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
			protected override double CompressiveStress(double strain, double transverseStrain, double confinementFactor = 1)
			{
				// Get strains
				double
					ec1 = transverseStrain,
					ec2 = strain,
					fc  = Parameters.Strength.Megapascals,
					ec  = Parameters.PlasticStrain;


				// Calculate the maximum concrete compressive stress
				double
					f2maxA = ec1 > 0 ? -fc / (0.8 - 0.34 * ec1 / ec) : -fc,
					f2max  = Math.Max(f2maxA, -fc) * confinementFactor;

				// Calculate the principal compressive stress in concrete
				var n = ec2 / ec;

				return
					f2max * (2 * n - n * n);
			}

			/// <inheritdoc />
			protected override double TensileStress(double strain, double transverseStrain, double theta1 = Constants.PiOver4, double referenceLength = 0, WebReinforcement reinforcement = null)
			{
				// Get strains
				double
					ec1 = strain,
					ec2 = transverseStrain;

				// Calculate initial uncracked state
				var fc1 = UncrackedStress(ec1, ec2, theta1, reinforcement);

				// Not cracked
				if (!Cracked)
					return fc1;

				// Else, cracked
				return
					CrackedStress(ec1);
			}

			/// <summary>
			///     Calculate tensile stress for cracked concrete.
			/// </summary>
			/// <param name="strain">Current tensile strain.</param>
			private double CrackedStress(double strain) => Parameters.TensileStrength.Megapascals / (1 + Math.Sqrt(500 * strain));

			#endregion
		}
	}
}