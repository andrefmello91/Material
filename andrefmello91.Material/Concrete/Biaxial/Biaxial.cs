using andrefmello91.Material.Reinforcement;
using andrefmello91.OnPlaneComponents.Strain;
using andrefmello91.OnPlaneComponents.Stress;
using Extensions;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnitsNet;
using UnitsNet.Units;
#nullable enable

namespace andrefmello91.Material.Concrete
{
	/// <summary>
	///		Biaxial concrete for membrane calculations.
	/// </summary>
	public partial class BiaxialConcrete : Concrete, ICloneable<BiaxialConcrete>
	{
		#region Fields

		/// <summary>
		///     Get concrete <see cref="BiaxialConcrete.Constitutive" />.
		/// </summary>
		private Constitutive _constitutive;

		#endregion

		#region Properties

		/// <summary>
		///     Returns true if concrete is cracked.
		/// </summary>
		public bool Cracked => _constitutive.Cracked;

		/// <summary>
		///     Get/set crack slip consideration.
		/// </summary>
		public bool ConsiderCrackSlip
		{
			get => _constitutive.ConsiderCrackSlip;
			set => _constitutive.ConsiderCrackSlip = value;
		}


		/// <summary>
		///     Get concrete initial stiffness <see cref="Matrix" />.
		/// </summary>
		/// <remarks>
		///		Elements are expressed in <see cref="PressureUnit.Megapascal"/>.
		/// </remarks>
		public Matrix<double> InitialStiffness
		{
			get
			{
				var Ec = Parameters.ElasticModule.Megapascals;

				// Concrete matrix
				var Dc1 = Matrix<double>.Build.Dense(3, 3);
				Dc1[0, 0] = Ec;
				Dc1[1, 1] = Ec;
				Dc1[2, 2] = 0.5 * Ec;

				// Get transformation matrix
				var T = StrainRelations.TransformationMatrix(Constants.PiOver4);

				// Calculate Dc
				return
					T.Transpose() * Dc1 * T;
			}
		}

		/// <summary>
		///     Get/set concrete <see cref="PrincipalStrainState" />.
		/// </summary>
		public PrincipalStrainState PrincipalStrains { get; private set; }

		/// <summary>
		///     Get/set concrete <see cref="PrincipalStressState" />.
		/// </summary>
		public PrincipalStressState PrincipalStresses { get; private set; }

		/// <summary>
		///     Calculate current secant module of concrete.
		/// </summary>
		private (Pressure Ec1, Pressure Ec2) SecantModule
		{
			get
			{
				// Get values
				double
					ec1 = PrincipalStrains.Epsilon1,
					ec2 = PrincipalStrains.Epsilon2;

				Pressure
					fc1 = PrincipalStresses.Sigma1,
					fc2 = PrincipalStresses.Sigma2;

				// Calculate modules
				Pressure
					Ec1 = _constitutive.SecantModule(fc1, ec1),
					Ec2 = _constitutive.SecantModule(fc2, ec2);

				return
					(Ec1, Ec2);
			}
		}

		/// <summary>
		///     Get concrete stiffness <see cref="Matrix" />, with elements in <see cref="PressureUnit.Megapascal"/>.
		/// </summary>
		public Matrix<double> Stiffness
		{
			get
			{
				var Ecs = SecantModule;
				var (Ec1, Ec2) = (Ecs.Ec1.Megapascals, Ecs.Ec2.Megapascals);

				var Gc = Ec1 * Ec2 / (Ec1 + Ec2);

				// Concrete matrix
				var Dc1 = Matrix<double>.Build.Dense(3, 3);
				Dc1[0, 0] = Ec1;
				Dc1[1, 1] = Ec2;
				Dc1[2, 2] = Gc;

				// Get transformation matrix
				var T = PrincipalStrains.TransformationMatrix;

				// Calculate Dc
				return
					T.Transpose() * Dc1 * T;
			}
		}

		/// <summary>
		///     Get/set concrete <see cref="StrainState" />.
		/// </summary>
		public StrainState Strains { get; private set; }

		/// <summary>
		///     Get/set concrete <see cref="StressState" />.
		/// </summary>
		public StressState Stresses { get; private  set; }

		#endregion

		#region Constructors

		/// <summary>
		///     Concrete for membrane calculations.
		/// </summary>
		/// <inheritdoc />
		public BiaxialConcrete(IParameters parameters, ConstitutiveModel model = ConstitutiveModel.MCFT)
			: base(parameters, model)
		{
			_constitutive = Material.Concrete.BiaxialConcrete.Constitutive.Read(model, parameters);
		}

		#endregion

		#region

		/// <summary>
		///     Set concrete <see cref="StressState" /> given <see cref="StrainState" />
		/// </summary>
		/// <param name="strains">Current <see cref="StrainState" /> in concrete.</param>
		/// <param name="reinforcement">The <see cref="WebReinforcement" />.</param>
		/// <param name="referenceLength">The reference length (only for <see cref="DSFMConstitutive" />).</param>
		public void CalculatePrincipalStresses(StrainState strains, WebReinforcement? reinforcement, Length? referenceLength = null)
		{
			// Get strains
			Strains = strains.Clone();

			// Calculate principal strains
			PrincipalStrains = PrincipalStrainState.FromStrain(Strains);

			// Get stresses from constitutive model
			PrincipalStresses = _constitutive.CalculateStresses(PrincipalStrains, reinforcement, referenceLength);
			Stresses          = StressState.FromPrincipal(PrincipalStresses);
		}

		/// <summary>
		///     Set tensile stress.
		/// </summary>
		/// <param name="fc1">Concrete tensile stress.</param>
		public void SetTensileStress(Pressure fc1)
		{
			// Get compressive stress and theta1
			var fc2    = PrincipalStresses.Sigma2;
			var theta1 = PrincipalStresses.Theta1;

			// Set new state
			PrincipalStresses = new PrincipalStressState(fc1, fc2, theta1);
			Stresses          = StressState.FromPrincipal(PrincipalStresses);
		}

		public BiaxialConcrete Clone() => new BiaxialConcrete(Parameters, Model);

		/// <inheritdoc />
		public override bool Equals(IConcrete? other) => other is BiaxialConcrete && base.Equals(other);

		public override bool Equals(object? obj) => obj is BiaxialConcrete concrete && Equals(concrete);

		public override int GetHashCode() => Parameters.GetHashCode();

		#endregion
	}
}