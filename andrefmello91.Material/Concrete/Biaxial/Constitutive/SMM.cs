using System;
using andrefmello91.Extensions;
using andrefmello91.Material.Reinforcement;
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

			#region Fields

			/// <summary>
			///     <inheritdoc cref="StrengthFunction" />
			/// </summary>
			private readonly double _strengthFunction;

			#endregion

			#region Properties

			public override ConstitutiveModel Model { get; } = ConstitutiveModel.SMM;

			#endregion

			#region Constructors

			/// <summary>
			///     MCFT constitutive object.
			/// </summary>
			/// <inheritdoc cref="Constitutive(IConcreteParameters)" />
			public SMMConstitutive(IConcreteParameters parameters) : base(parameters) => _strengthFunction = StrengthFunction(parameters.Strength);

			#endregion

			#region Methods

			/// <summary>
			///     Calculate the deviation angle function for the softening parameter.
			/// </summary>
			/// <param name="deviationAngle">The deviation angle between applied principal stresses and concrete principal stresses.</param>
			private static double DeviationFunction(double deviationAngle) => 1D - deviationAngle.Abs() / 0.418879;

			/// <summary>
			///     Calculate the strength function for the softening parameter.
			/// </summary>
			/// <param name="strength">The compressive strength of concrete.</param>
			private static double StrengthFunction(Pressure strength) => Math.Min(5.8 / strength.Megapascals.Sqrt(), 0.9);

			/// <summary>
			///     Calculate the tensile strain function for the softening parameter.
			/// </summary>
			/// <param name="epsilon1">The average principal tensile strain, not considering Poisson effect.</param>
			private static double TensileStrainFunction(double epsilon1) => 1D / (1D + 400 * epsilon1).Sqrt();

			/// <inheritdoc />
			protected override Pressure CompressiveStress(double strain, double transverseStrain, double deviationAngle = 0, double confinementFactor = 1)
			{
				// Calculate softening coefficient
				var soft = (TensileStrainFunction(transverseStrain) * _strengthFunction * DeviationFunction(deviationAngle)).AsFinite();

				// Calculate peak stress and strain
				var fp = -soft * Parameters.Strength;
				var ep = soft * Parameters.PlasticStrain;

				// Calculate strain ratio:
				var e2_ep = (strain / ep).AsFinite();

				return
					(e2_ep <= 1) switch
					{
						{ } when e2_ep < 0 => Pressure.Zero,

						// Pre-peak
						true => fp * (2 * e2_ep - e2_ep * e2_ep) * confinementFactor,

						// Post-peak
						_ => Max(fp * (1D - ((e2_ep - 1D) / (4D / soft - 1D)).Pow(2)), 0.5 * fp) * confinementFactor
					};
			}

			/// <inheritdoc />
			protected override Pressure CrackedStress(double strain, double theta1, WebReinforcement? reinforcement, Length? referenceLength = null) =>
				Parameters.TensileStrength * (Parameters.CrackingStrain / strain).Pow(0.4);

			#endregion

		}
	}
}