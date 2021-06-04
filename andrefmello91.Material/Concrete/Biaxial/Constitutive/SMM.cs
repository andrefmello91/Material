﻿using System;
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
		protected class SMMConstitutive : Constitutive
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
			protected override Pressure CrackedStress(double strain, double theta1, WebReinforcement? reinforcement, Length? referenceLength = null) => 
				Parameters.TensileStrength * (Parameters.CrackingStrain / strain).Pow(0.4);

			/// <summary>
			///		Calculate the Poisson coefficients for SMM.
			/// </summary>
			/// <param name="reinforcement">The reinforcement.</param>
			/// <param name="cracked">The cracked state of concrete. True if cracked.</param>
			public static (double v12, double v21) PoissonCoefficients(WebReinforcement? reinforcement, bool cracked)
			{
				var v21 = cracked
					? 0
					: 0.2;

				if (reinforcement is null)
					return (0.2, v21);

				var strains = reinforcement.Strains;
				
				var esf     = Math.Max(strains.EpsilonX, strains.EpsilonY);
				
				var ey = strains.EpsilonX >= strains.EpsilonY
					? reinforcement.DirectionX?.Steel.YieldStrain
					: reinforcement.DirectionY?.Steel.YieldStrain;

				var v12 = esf <= 0 || !ey.HasValue
					? 0.2
					: 0.2 + 850 * esf;

				return (v12, v21);
			}
			
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
			
			///  <summary>
			/// 		Calculate compressive stress.
			///  </summary>
			///  <inheritdoc cref="CompressiveStress(double,double,double)"/>
			protected override Pressure CompressiveStress(double strain, double transverseStrain, double deviationAngle = 0, double confinementFactor = 1)
			{
				// Calculate softening coefficient
				var soft = (TensileStrainFunction(transverseStrain) * _strengthFunction * DeviationFunction(deviationAngle)).AsFinite();
				
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
						true => fp * (2 * e2_ep - e2_ep * e2_ep) * confinementFactor,
						
						// Post-peak
						_    => fp * (1D - ((e2_ep - 1D) / (4D / soft - 1D)).Pow(2)) * confinementFactor
					};
			}
			
			#endregion

		}
	}
}