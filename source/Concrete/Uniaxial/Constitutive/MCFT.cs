﻿using System;
using Material.Reinforcement.Uniaxial;
using UnitsNet;

namespace Material.Concrete.Uniaxial
{
	public partial class UniaxialConcrete
	{
		/// <summary>
		///     MCFT constitutive class.
		/// </summary>
		private class MCFTConstitutive : Constitutive
		{
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
			protected override Pressure CompressiveStress(double strain)
			{
				double
					ec = Parameters.PlasticStrain,
					fc = Parameters.Strength.Megapascals,
					n  = strain / ec,
					f  = -fc * (2 * n - n * n);

				return
					Pressure.FromMegapascals(f);
			}

			public override ConstitutiveModel Model { get; } = ConstitutiveModel.MCFT;

			/// <inheritdoc />
			protected override Pressure TensileStress(double strain, UniaxialReinforcement reinforcement = null) =>
				strain <= Parameters.CrackingStrain
					? strain * Parameters.ElasticModule
					: CrackedStress(strain);

			/// <summary>
			///     Calculate tensile stress, for cracked concrete.
			/// </summary>
			/// <param name="strain">Current tensile strain.</param>
			private Pressure CrackedStress(double strain) => Parameters.TensileStrength / (1 + Math.Sqrt(500 * strain));


			#endregion
		}
	}
}