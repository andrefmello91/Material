using System;
using andrefmello91.Extensions;
using andrefmello91.Material.Reinforcement;
using UnitsNet;
using UnitsNet.Units;
#nullable enable

namespace andrefmello91.Material.Concrete
{
	public partial class UniaxialConcrete
	{
		/// <summary>
		///     MCFT constitutive class.
		/// </summary>
		private class SMMConstitutive : Constitutive
		{
			private double _strengthFunction;
			
			#region Properties

			public override ConstitutiveModel Model => ConstitutiveModel.SMM;

			#endregion

			#region Constructors

			/// <summary>
			///     MCFT constitutive object.
			/// </summary>
			/// <inheritdoc cref="Concrete" />
			public SMMConstitutive(IParameters parameters) : base(parameters)
			{
				_strengthFunction = StrengthFunction(Parameters.Strength);
			}

			#endregion

			#region Methods

			/// <inheritdoc />
			protected override Pressure CompressiveStress(double strain)
			{
				// Calculate softening coefficient
				var soft = _strengthFunction;
				
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
						true => fp * (2 * e2_ep - e2_ep * e2_ep),
						
						// Post-peak
						_ => UnitMath.Min(fp * (1D - ((e2_ep - 1D) / (4D / soft - 1D)).Pow(2)), 0.5 * fp)
					};
			}

			/// <inheritdoc />
			protected override Pressure CrackedStress(double strain, UniaxialReinforcement? reinforcement = null) =>
				Parameters.TensileStrength * (Parameters.CrackingStrain / strain).Pow(0.4);

			/// <summary>
			///		Calculate the strength function for the softening parameter.
			/// </summary>
			/// <param name="strength">The compressive strength of concrete.</param>
			private static double StrengthFunction(Pressure strength) => Math.Min(5.8 / strength.Megapascals.Sqrt(), 0.9);

			#endregion

		}
	}
}