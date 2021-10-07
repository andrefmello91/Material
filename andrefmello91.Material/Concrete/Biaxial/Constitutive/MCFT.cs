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
		///     MCFT constitutive class.
		/// </summary>
		protected class MCFTConstitutive : Constitutive
		{

			#region Properties

			public override ConstitutiveModel Model { get; } = ConstitutiveModel.MCFT;

			#endregion

			#region Constructors

			/// <summary>
			///     MCFT constitutive object.
			/// </summary>
			/// <inheritdoc cref="Constitutive(IConcreteParameters)" />
			public MCFTConstitutive(IConcreteParameters parameters) : base(parameters)
			{
			}

			#endregion

			#region Methods

			/// <inheritdoc />
			protected override Pressure CompressiveStress(double strain, double transverseStrain, double deviationAngle = 0, double confinementFactor = 1)
			{
				if (!strain.IsFinite() || !transverseStrain.IsFinite() || strain.ApproxZero())
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
			protected override Pressure CrackedStress(double strain, double theta1, WebReinforcement? reinforcement, Length? referenceLength = null) =>
				Parameters.TensileStrength / (1 + Math.Sqrt(500 * strain));

			#endregion

		}
	}
}