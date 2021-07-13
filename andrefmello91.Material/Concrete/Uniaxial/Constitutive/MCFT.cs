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
		private class MCFTConstitutive : Constitutive
		{

			#region Properties

			public override ConstitutiveModel Model { get; } = ConstitutiveModel.MCFT;

			#endregion

			#region Constructors

			/// <summary>
			///     MCFT constitutive object.
			/// </summary>
			/// <inheritdoc cref="Concrete" />
			public MCFTConstitutive(IParameters parameters) : base(parameters)
			{
			}

			#endregion

			#region Methods

			/// <inheritdoc />
			protected override Pressure CompressiveStress(double strain)
			{
				double
					ec = Parameters.PlasticStrain,
					fc = Parameters.Strength.Megapascals,
					n  = strain / ec,
					f  = -fc * (2 * n - n * n);

				return
					(Pressure) f.As(PressureUnit.Megapascal);
			}

			/// <inheritdoc />
			protected override Pressure CrackedStress(double strain, UniaxialReinforcement? reinforcement = null) =>
				Parameters.TensileStrength / (1 + Math.Sqrt(500 * strain));

			#endregion

		}
	}
}