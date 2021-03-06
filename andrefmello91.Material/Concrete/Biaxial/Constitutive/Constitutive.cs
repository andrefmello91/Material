﻿using andrefmello91.Extensions;
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
		///     Base class for concrete constitutive model.
		/// </summary>
		private abstract class Constitutive : IConstitutive
		{

			#region Fields

			/// <summary>
			///     Concrete <see cref="IParameters" />.
			/// </summary>
			protected readonly IParameters Parameters;

			#endregion

			#region Properties

			/// <inheritdoc cref="BiaxialConcrete.Cs" />
			public double Cs { get; set; } = 0.55;

			#region Interface Implementations

			public bool ConsiderCrackSlip { get; set; }

			public bool Cracked { get; set; }

			public abstract ConstitutiveModel Model { get; }

			#endregion

			#endregion

			#region Constructors

			// Constructor
			/// <summary>
			///     Base class for concrete behavior
			/// </summary>
			/// <inheritdoc cref="From" />
			protected Constitutive(IParameters parameters) => Parameters = parameters;

			#endregion

			#region Methods

			/// <summary>
			///     Get concrete <see cref="Constitutive" /> object based on the <see cref="ConstitutiveModel" />.
			/// </summary>
			/// <param name="constitutiveModel">The <see cref="ConstitutiveModel" /> for concrete.</param>
			/// <param name="parameters">Concrete <see cref="IParameters" />.</param>
			public static Constitutive From(ConstitutiveModel constitutiveModel, IParameters parameters) =>
				constitutiveModel switch
				{
					ConstitutiveModel.DSFM => new DSFMConstitutive(parameters),
					_                      => new MCFTConstitutive(parameters)
				};

			/// <summary>
			///     Calculate concrete <see cref="PrincipalStressState" /> related to <see cref="PrincipalStrainState" />.
			///     <para>For <seealso cref="BiaxialConcrete" />.</para>
			/// </summary>
			/// <param name="principalStrains">The <see cref="PrincipalStrainState" /> in concrete.</param>
			/// <param name="reinforcement">The <see cref="WebReinforcement" />.</param>
			/// <param name="referenceLength">The reference length (only for <see cref="DSFMConstitutive" />).</param>
			public PrincipalStressState CalculateStresses(PrincipalStrainState principalStrains, WebReinforcement? reinforcement, Length? referenceLength = null)
			{
				if (principalStrains.IsZero)
					return PrincipalStressState.Zero;

				// Get strains
				double
					ec1 = principalStrains.Epsilon1.AsFinite(),
					ec2 = principalStrains.Epsilon2.AsFinite();

				Pressure fc1, fc2;

				switch (principalStrains.Case)
				{
					// Verify case
					case PrincipalCase.UniaxialCompression:
					case PrincipalCase.TensionCompression:
						fc1 = TensileStress(ec1, ec2, principalStrains.Theta1, referenceLength, reinforcement);
						fc2 = CompressiveStress(ec2, ec1);
						break;

					case PrincipalCase.UniaxialTension:
					case PrincipalCase.PureTension:
						fc1 = TensileStress(ec1, ec2, principalStrains.Theta1, referenceLength, reinforcement);
						fc2 = TensileStress(ec2, ec1, principalStrains.Theta1, referenceLength, reinforcement);
						break;

					case PrincipalCase.PureCompression when !Parameters.ConsiderConfinement:
						fc1 = CompressiveStress(ec1, ec2);
						fc2 = CompressiveStress(ec2, ec1);
						break;

					case PrincipalCase.PureCompression when Parameters.ConsiderConfinement:
						return ConfinementStresses(principalStrains);

					default:
						return PrincipalStressState.Zero;
				}

				return
					new PrincipalStressState(fc1, fc2, principalStrains.Theta1);
			}

			/// <summary>
			///     Calculate current secant module.
			/// </summary>
			/// <param name="stress">Current stress.</param>
			/// <param name="strain">Current strain.</param>
			public Pressure SecantModule(Pressure stress, double strain) =>
				stress.Abs() <= Material.Concrete.Parameters.Tolerance || strain.Abs() <= 1E-9
					? Parameters.ElasticModule
					: stress / strain;

			/// <summary>
			///     Calculate compressive stress for <see cref="Material.Concrete.BiaxialConcrete" /> case.
			/// </summary>
			/// <param name="strain">The compressive strain (negative) to calculate stress.</param>
			/// <param name="transverseStrain">The strain at the transverse direction to <paramref name="strain" />.</param>
			/// <param name="confinementFactor">
			///     The confinement factor for pure compression case.
			///     <para>See: <seealso cref="ConfinementFactor" /></para>
			/// </param>
			/// <returns>Compressive stress in MPa</returns>
			protected abstract Pressure CompressiveStress(double strain, double transverseStrain, double confinementFactor = 1);

			/// <summary>
			///     Calculate tensile stress for <see cref="Material.Concrete.BiaxialConcrete" /> case.
			/// </summary>
			/// <param name="strain">The tensile strain to calculate stress.</param>
			/// <param name="transverseStrain">The strain at the transverse direction to <paramref name="strain" />.</param>
			/// <param name="theta1">The angle of <paramref name="strain" /> related to horizontal axis, in radians.</param>
			/// <param name="referenceLength">The reference length (only for <see cref="DSFMConstitutive" />).</param>
			/// <param name="reinforcement">The <see cref="WebReinforcement" /> (only for <see cref="DSFMConstitutive" />).</param>
			protected abstract Pressure TensileStress(double strain, double transverseStrain, double theta1 = Constants.PiOver4, Length? referenceLength = null, WebReinforcement? reinforcement = null);

			/// <summary>
			///     Calculate <see cref="Material.Concrete.BiaxialConcrete" /> tensile stress for uncracked state.
			/// </summary>
			/// <param name="strain">The compressive strain (negative) to calculate stress.</param>
			/// <param name="transverseStrain">The strain at the transverse direction to <paramref name="strain" />.</param>
			/// <param name="theta1">The angle of <paramref name="strain" /> related to horizontal axis, in radians.</param>
			/// <param name="reinforcement">The <see cref="WebReinforcement" /> object.</param>
			protected Pressure UncrackedStress(double strain, double transverseStrain, double theta1 = Constants.PiOver4, WebReinforcement? reinforcement = null)
			{
				if (Cracked)
					return Pressure.Zero;

				// Get strains
				double
					ec1 = strain,
					ec2 = transverseStrain;

				// Calculate initial uncracked state
				var fc1 = ec1 * Parameters.ElasticModule;

				// Verify if fc1 cracks concrete
				VerifyCrackedState(fc1, ec2);

				if (reinforcement is null)
					return fc1;

				// Check maximum stress that can be transmitted by reinforcement
				var fc1s = reinforcement.MaximumPrincipalTensileStress(theta1);

				return
					Min(fc1, fc1s);
			}

			/// <summary>
			///     Calculate confinement strength factor according to Kupfer et. al. (1969).
			/// </summary>
			/// <param name="transverseStress">The stress acting on the transverse direction of the analyzed direction.</param>
			private double ConfinementFactor(Pressure transverseStress)
			{
				// Get absolute value
				var fcn_fc = (transverseStress / Parameters.Strength).Abs();

				var c = 1 + 0.92 * fcn_fc - 0.76 * fcn_fc * fcn_fc;

				return
					c.IsFinite() && c > 1 && c < 2
						? c
						: 1;
			}

			/// <summary>
			///     Calculate confinement <see cref="PrincipalStressState" />.
			/// </summary>
			/// <param name="principalStrains">
			///     The <see cref="PrincipalStrainState" />, in pure compression.
			/// </param>
			private PrincipalStressState ConfinementStresses(PrincipalStrainState principalStrains)
			{
				// Get strains
				double
					ec1 = principalStrains.Epsilon1,
					ec2 = principalStrains.Epsilon2;

				// Calculate initial stresses
				Pressure
					fc1 = CompressiveStress(ec1, ec2),
					fc2 = CompressiveStress(ec2, ec1);

				var tol = Pressure.FromMegapascals(0.01);

				// Iterate to find stresses (maximum 20 iterations)
				for (var it = 1; it <= 20; it++)
				{
					// Calculate confinement factors
					double
						betaL1 = ConfinementFactor(fc2),
						betaL2 = ConfinementFactor(fc1);

					// Calculate iteration stresses
					Pressure
						fc1It = CompressiveStress(ec1, ec2, betaL1),
						fc2It = CompressiveStress(ec2, ec1, betaL2);

					// Verify tolerances

					if ((fc1 - fc1It).Abs() <= tol && (fc2 - fc2It).Abs() <= tol)
						break;

					// Update stresses
					fc1 = fc1It;
					fc2 = fc2It;
				}

				return
					new PrincipalStressState(fc1, fc2, principalStrains.Theta1);
			}

			/// <summary>
			///     Check if concrete is cracked for <see cref="Material.Concrete.BiaxialConcrete" /> case and set cracked property,
			///     from Gupta (1998)
			///     formulation.
			/// </summary>
			/// <param name="fc1">Principal tensile stress.</param>
			/// <param name="ec2">Principal compressive strain.</param>
			private void VerifyCrackedState(Pressure fc1, double ec2)
			{
				if (Cracked)
					return;

				var ft = Parameters.TensileStrength;
				var ec = Parameters.PlasticStrain;

				// Calculate current cracking stress
				var fcr1 = ft * (1 - ec2 / ec);

				// Verify limits
				var fcr = Max(fcr1, 0.25 * ft);
				fcr = Min(fcr, ft);

				// Verify is concrete is cracked
				if (fc1 >= fcr)

					// Set cracked state
					Cracked = true;
			}

			#region Interface Implementations

			public bool Equals(IConstitutive? other) => other is not null && Model == other.Model;

			#endregion

			#region Object override

			public override string ToString() => $"{Model}";

			#endregion

			#endregion

		}
	}
}