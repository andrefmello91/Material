using System;
using andrefmello91.Extensions;
using andrefmello91.Material.Reinforcement;
using andrefmello91.OnPlaneComponents;
using MathNet.Numerics;
using UnitsNet;
using static UnitsNet.UnitMath;
using Pressure = UnitsNet.Pressure;

#nullable enable

namespace andrefmello91.Material.Concrete
{
	public partial class BiaxialConcrete
	{
		/// <summary>
		///     Base class for concrete constitutive model.
		/// </summary>
		protected abstract class Constitutive : IEquatable<Constitutive>
		{

			#region Fields

			/// <summary>
			///     Concrete <see cref="IConcreteParameters" />.
			/// </summary>
			protected readonly IConcreteParameters Parameters;

			#endregion

			#region Properties

			/// <summary>
			///     The crack slip consideration.
			/// </summary>
			/// <remarks>
			///     Only for <see cref="ConstitutiveModel.DSFM" />.
			/// </remarks>
			public bool ConsiderCrackSlip { get; set; }

			/// <inheritdoc cref="Concrete.Cracked" />
			public bool Cracked { get; private set; }

			/// <inheritdoc cref="BiaxialConcrete.Cs" />
			public double Cs { get; set; } = 0.55;

			/// <summary>
			///     The constitutive model of concrete.
			/// </summary>
			public abstract ConstitutiveModel Model { get; }

			#endregion

			#region Constructors

			// Constructor
			/// <summary>
			///     Base class for concrete behavior
			/// </summary>
			/// <inheritdoc cref="From" />
			protected Constitutive(IConcreteParameters parameters) => Parameters = parameters;

			#endregion

			#region Methods

			/// <summary>
			///     Get concrete <see cref="Constitutive" /> object based on the <see cref="ConstitutiveModel" />.
			/// </summary>
			/// <param name="constitutiveModel">The <see cref="ConstitutiveModel" /> for concrete.</param>
			/// <param name="parameters">Concrete <see cref="IConcreteParameters" />.</param>
			public static Constitutive From(ConstitutiveModel constitutiveModel, IConcreteParameters parameters) =>
				constitutiveModel switch
				{
					ConstitutiveModel.DSFM => new DSFMConstitutive(parameters),
					ConstitutiveModel.MCFT => new MCFTConstitutive(parameters),
					_                      => new SMMConstitutive(parameters)
				};

			/// <summary>
			///     Calculate confinement strength factor according to Kupfer et. al. (1969).
			/// </summary>
			/// <param name="transverseStress">The stress acting on the transverse direction of the analyzed direction.</param>
			/// <param name="concreteStrength">Concrete compressive strenght.</param>
			internal static double ConfinementFactor(Pressure transverseStress, Pressure concreteStrength)
			{
				// Get absolute value
				var fcn_fc = (transverseStress / concreteStrength).Abs();

				var c = 1 + 0.92 * fcn_fc - 0.76 * fcn_fc * fcn_fc;

				return
					c.IsFinite() && c > 1 && c < 2
						? c
						: 1;
			}

			/// <summary>
			///     Calculate concrete <see cref="PrincipalStressState" /> related to <see cref="PrincipalStrainState" />.
			///     <para>For <seealso cref="BiaxialConcrete" />.</para>
			/// </summary>
			/// <param name="strainsAtAvgPrincipal">
			///     The strain state in concrete, at the average principal strain direction of the membrane element.
			///     <remarks>
			///         The direction X must be the tensile (or largest) strain and Y the compressive (or smaller) one.
			///     </remarks>
			/// </param>
			/// <param name="reinforcement">The <see cref="WebReinforcement" />.</param>
			/// <param name="referenceLength">The reference length (only for <see cref="ConstitutiveModel.DSFM" />).</param>
			/// <param name="deviationAngle">
			///     The deviation angle between applied principal stresses and concrete principal stresses
			///     (only for <see cref="ConstitutiveModel.SMM" />).
			/// </param>
			/// <returns>
			///     The <see cref="StressState" /> at the direction of <paramref name="strainsAtAvgPrincipal" />.
			/// </returns>
			public StressState CalculateStresses(IState<double> strainsAtAvgPrincipal, WebReinforcement? reinforcement, Length? referenceLength = null, double deviationAngle = 0)
			{
				if (strainsAtAvgPrincipal.IsZero)
					return new StressState(0, 0, 0, strainsAtAvgPrincipal.ThetaX);

				// Rotate is necessary
				if (strainsAtAvgPrincipal.X < strainsAtAvgPrincipal.Y)
					strainsAtAvgPrincipal = strainsAtAvgPrincipal.Transform(Constants.PiOver2);

				// Get strains
				double
					ec1 = strainsAtAvgPrincipal.X.AsFinite(),
					ec2 = strainsAtAvgPrincipal.Y.AsFinite(),
					yxy = strainsAtAvgPrincipal.XY.AsFinite();

				// Get the case
				var pCase = strainsAtAvgPrincipal is IPrincipalState<double> pState
					? pState.Case
					: new PrincipalStrainState(ec1, ec2).Case;

				Pressure fc1, fc2;

				switch (pCase)
				{
					// Verify case
					case PrincipalCase.UniaxialCompression:
					case PrincipalCase.TensionCompression:
						fc1 = TensileStress(ec1, ec2, strainsAtAvgPrincipal.ThetaX, reinforcement, referenceLength);
						fc2 = CompressiveStress(ec2, ec1, deviationAngle);
						break;

					case PrincipalCase.UniaxialTension:
					case PrincipalCase.PureTension:
						fc1 = TensileStress(ec1, ec2, strainsAtAvgPrincipal.ThetaX, reinforcement, referenceLength);
						fc2 = TensileStress(ec2, ec1, strainsAtAvgPrincipal.ThetaX, reinforcement, referenceLength);
						break;

					case PrincipalCase.PureCompression when !Parameters.ConsiderConfinement:
						fc1 = CompressiveStress(ec1, ec2, deviationAngle);
						fc2 = CompressiveStress(ec2, ec1, deviationAngle);
						break;

					case PrincipalCase.PureCompression when Parameters.ConsiderConfinement:
						var conf = ConfinementStresses(strainsAtAvgPrincipal, deviationAngle);
						fc1 = conf.SigmaX;
						fc2 = conf.SigmaY;
						break;

					default:
						return new StressState(0, 0, 0, strainsAtAvgPrincipal.ThetaX);
				}

				// Calculate shear stress (for SMM)
				var tau = Model is ConstitutiveModel.SMM
					? 0.5 * yxy * (fc1 - fc2) / (ec1 - ec2)
					: Pressure.Zero;

				return
					new StressState(fc1, fc2, tau, strainsAtAvgPrincipal.ThetaX);
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

			/// <inheritdoc />
			public override string ToString() => $"{Model}";

			/// <summary>
			///     Calculate compressive stress for <see cref="Material.Concrete.BiaxialConcrete" /> case.
			/// </summary>
			/// <param name="strain">The compressive strain (negative) to calculate stress.</param>
			/// <param name="transverseStrain">The strain at the transverse direction to <paramref name="strain" />.</param>
			/// <param name="deviationAngle">
			///     The deviation angle between applied principal stresses and concrete principal stresses
			///     (only for <see cref="ConstitutiveModel.SMM" />).
			/// </param>
			/// <param name="confinementFactor">
			///     The confinement factor for pure compression case.
			///     <para>See: <seealso cref="ConfinementFactor" /></para>
			/// </param>
			/// <returns>Compressive stress in MPa</returns>
			protected abstract Pressure CompressiveStress(double strain, double transverseStrain, double deviationAngle = 0, double confinementFactor = 1);

			/// <summary>
			///     Calculate tensile stress for cracked concrete.
			/// </summary>
			/// <param name="strain">Current tensile strain.</param>
			/// <param name="theta1">The angle of <paramref name="strain" /> related to horizontal axis, in radians.</param>
			/// <param name="referenceLength">The reference length (only for <see cref="DSFMConstitutive" />).</param>
			/// <param name="reinforcement">The <see cref="WebReinforcement" /> (only for <see cref="DSFMConstitutive" />).</param>
			protected abstract Pressure CrackedStress(double strain, double theta1, WebReinforcement? reinforcement, Length? referenceLength = null);

			/// <summary>
			///     Check if concrete is cracked for <see cref="Material.Concrete.BiaxialConcrete" /> case and set cracked property,
			///     from Gupta (1998)
			///     formulation.
			/// </summary>
			/// <param name="fc1">Principal tensile stress.</param>
			/// <param name="ec2">Principal compressive strain.</param>
			private void CheckCrackedState(Pressure fc1, double ec2)
			{
				var ft = Parameters.TensileStrength;
				var ec = Parameters.PlasticStrain;

				// Calculate current cracking stress
				var fcr1 = ft * (1 - ec2 / ec);

				// Verify limits
				var fcr = Max(fcr1, 0.25 * ft);
				fcr = Min(fcr, ft);

				// Verify is concrete is cracked
				Cracked = fc1 >= fcr;
			}

			/// <summary>
			///     Calculate confinement <see cref="PrincipalStressState" />.
			/// </summary>
			/// <param name="strainsAtAvgPrincipal">
			///     The smeared strains in concrete, affected by Poisson effect, at the direction of
			///     average principal strains.
			/// </param>
			/// <param name="deviationAngle">The deviation angle between applied principal stresses and concrete principal stresses.</param>
			private StressState ConfinementStresses(IState<double> strainsAtAvgPrincipal, double deviationAngle)
			{
				// Get strains
				double
					ec1 = strainsAtAvgPrincipal.X,
					ec2 = strainsAtAvgPrincipal.Y;

				// Calculate initial stresses
				Pressure
					fc1 = CompressiveStress(ec1, ec2, deviationAngle),
					fc2 = CompressiveStress(ec2, ec1, deviationAngle);

				var tol = Pressure.FromMegapascals(0.01);

				// Iterate to find stresses (maximum 20 iterations)
				for (var it = 1; it <= 20; it++)
				{
					// Calculate confinement factors
					double
						betaL1 = ConfinementFactor(fc2, Parameters.Strength),
						betaL2 = ConfinementFactor(fc1, Parameters.Strength);

					// Calculate iteration stresses
					Pressure
						fc1It = CompressiveStress(ec1, ec2, deviationAngle, betaL1),
						fc2It = CompressiveStress(ec2, ec1, deviationAngle, betaL2);

					// Verify tolerances

					if ((fc1 - fc1It).Abs() <= tol && (fc2 - fc2It).Abs() <= tol)
						break;

					// Update stresses
					fc1 = fc1It;
					fc2 = fc2It;
				}

				return
					new StressState(fc1, fc2, Pressure.Zero, strainsAtAvgPrincipal.ThetaX);
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
						betaL1 = ConfinementFactor(fc2, Parameters.Strength),
						betaL2 = ConfinementFactor(fc1, Parameters.Strength);

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
			///     Calculate tensile stress for <see cref="Material.Concrete.BiaxialConcrete" /> case.
			/// </summary>
			/// <param name="strain">The tensile strain to calculate stress.</param>
			/// <param name="transverseStrain">The strain at the transverse direction to <paramref name="strain" />.</param>
			/// <param name="theta1">The angle of <paramref name="strain" /> related to horizontal axis, in radians.</param>
			/// <param name="reinforcement">The <see cref="WebReinforcement" />.</param>
			/// <param name="referenceLength">The reference length (only for <see cref="DSFMConstitutive" />).</param>
			private Pressure TensileStress(double strain, double transverseStrain, double theta1, WebReinforcement? reinforcement, Length? referenceLength = null)
			{
				if (!strain.IsFinite() || strain <= 0)
					return Pressure.Zero;

				// Calculate initial uncracked state
				var fc1 = UncrackedStress(strain, transverseStrain);

				return !Cracked
					? fc1
					: CrackedStress(strain, theta1, reinforcement, referenceLength);
			}

			/// <summary>
			///     Calculate <see cref="Material.Concrete.BiaxialConcrete" /> tensile stress for uncracked state.
			/// </summary>
			/// <param name="strain">The compressive strain (negative) to calculate stress.</param>
			/// <param name="transverseStrain">The strain at the transverse direction to <paramref name="strain" />.</param>
			private Pressure UncrackedStress(double strain, double transverseStrain)
			{
				// Get strains
				double
					ec1 = strain,
					ec2 = transverseStrain;

				// Calculate initial uncracked state
				var fc1 = ec1 * Parameters.ElasticModule;

				// Verify if fc1 cracks concrete
				CheckCrackedState(fc1, ec2);

				return fc1;
			}

			/// <inheritdoc />
			public bool Equals(Constitutive? other) => other is not null && Model == other.Model;

			#endregion

		}
	}
}