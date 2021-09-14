using andrefmello91.Extensions;
using andrefmello91.Material.Reinforcement;
using andrefmello91.OnPlaneComponents;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using UnitsNet;
using UnitsNet.Units;
#nullable enable

namespace andrefmello91.Material.Concrete
{
	/// <summary>
	///     Biaxial concrete for membrane calculations.
	/// </summary>
	public partial class BiaxialConcrete : Concrete, IBiaxialMaterial, ICloneable<BiaxialConcrete>
	{

		#region Properties

		/// <summary>
		///     Get/set crack slip consideration.
		/// </summary>
		public bool ConsiderCrackSlip
		{
			get => ConstitutiveEquations.ConsiderCrackSlip;
			set => ConstitutiveEquations.ConsiderCrackSlip = value;
		}

		/// <summary>
		///     Returns true if concrete is cracked.
		/// </summary>
		public override bool Cracked => ConstitutiveEquations.Cracked;

		/// <inheritdoc />
		public override bool Crushed => PrincipalStrains.Epsilon2.Abs() >= Parameters.UltimateStrain.Abs();

		/// <summary>
		///     Get/set Cs coefficient for concrete softening.
		/// </summary>
		public double Cs
		{
			get => ConstitutiveEquations.Cs;
			set => ConstitutiveEquations.Cs = value;
		}

		/// <summary>
		///     Get the deviation angle between <see cref="Strains" /> and <see cref="PrincipalStrains" />.
		/// </summary>
		public double DeviationAngle { get; protected set; }

		/// <summary>
		///     Get concrete initial stiffness <see cref="Matrix" />.
		/// </summary>
		/// <remarks>
		///     Elements are expressed in <see cref="PressureUnit.Megapascal" />.
		/// </remarks>
		public MaterialMatrix InitialStiffness
		{
			get
			{
				var Ec = Parameters.ElasticModule;

				// Concrete matrix
				var Dc1 = MaterialMatrix.Zero(Constants.PiOver4, Ec.Unit);
				Dc1[0, 0] = Ec;
				Dc1[1, 1] = Ec;
				Dc1[2, 2] = 0.5 * Ec;

				// Get transformation matrix
				// var t = StrainRelations.TransformationMatrix(Constants.PiOver4);

				// Calculate Dc
				return
					Dc1.ToHorizontal();
			}
		}

		/// <inheritdoc />
		public override bool Yielded => PrincipalStrains.Epsilon2.Abs() >= Parameters.PlasticStrain.Abs();

		/// <summary>
		///     Get concrete <see cref="BiaxialConcrete.Constitutive" />.
		/// </summary>
		protected Constitutive ConstitutiveEquations { get; }

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
					Ec1 = ConstitutiveEquations.SecantModule(fc1, ec1),
					Ec2 = ConstitutiveEquations.SecantModule(fc2, ec2);

				return
					(Ec1, Ec2);
			}
		}

		/// <summary>
		///     Get/set concrete <see cref="PrincipalStrainState" />.
		/// </summary>
		public PrincipalStrainState PrincipalStrains { get; protected set; }

		/// <summary>
		///     Get/set concrete <see cref="PrincipalStressState" />.
		/// </summary>
		public PrincipalStressState PrincipalStresses { get; protected set; }

		/// <summary>
		///     Get concrete stiffness <see cref="Matrix" />, with elements in <see cref="PressureUnit.Megapascal" />.
		/// </summary>
		public MaterialMatrix Stiffness
		{
			get
			{
				var (Ec1, Ec2) = (SecantModule.Ec1.Megapascals, SecantModule.Ec2.Megapascals);

				var Gc = Ec1 * Ec2 / (Ec1 + Ec2);

				// Concrete matrix
				var array = new double[3, 3];
				array[0, 0] = Ec1;
				array[1, 1] = Ec2;
				array[2, 2] = Gc;

				var Dc1 = new MaterialMatrix(array, PrincipalStrains.Theta1)
				{
					Unit = Parameters.StressUnit
				};

				// Get transformation matrix
				// var t = PrincipalStrains.TransformationMatrix;

				// Calculate Dc
				return
					Dc1.ToHorizontal();
			}
		}

		/// <summary>
		///     Get/set concrete <see cref="StrainState" />.
		/// </summary>
		public StrainState Strains { get; protected set; }

		/// <summary>
		///     Get/set concrete <see cref="StressState" />.
		/// </summary>
		public StressState Stresses { get; protected set; }

		#endregion

		#region Constructors

		/// <summary>
		///     Create a concrete object for membrane calculations.
		/// </summary>
		/// <inheritdoc />
		protected BiaxialConcrete(IConcreteParameters parameters, ConstitutiveModel model = ConstitutiveModel.MCFT)
			: base(parameters, model) =>
			ConstitutiveEquations = Constitutive.From(model, parameters);

		#endregion

		#region Methods

		/// <inheritdoc cref="BiaxialConcrete(IConcreteParameters, ConstitutiveModel)" />
		public static BiaxialConcrete From(IConcreteParameters parameters, ConstitutiveModel model = ConstitutiveModel.MCFT) =>
			model switch
			{
				ConstitutiveModel.SMM => new SMMConcrete(parameters),
				_                     => new BiaxialConcrete(parameters, model)
			};



		/// <summary>
		///     Set concrete <see cref="StressState" /> given <see cref="StrainState" />
		/// </summary>
		/// <param name="strains">Current <see cref="StrainState" /> in concrete.</param>
		/// <param name="reinforcement">The <see cref="WebReinforcement" />.</param>
		/// <param name="referenceLength">The reference length (only for <see cref="DSFMConstitutive" />).</param>
		public virtual void Calculate(StrainState strains, WebReinforcement? reinforcement, Length? referenceLength = null)
		{
			// Get strains
			Strains = strains.Clone();

			// Calculate principal strains
			PrincipalStrains = Strains.ToPrincipal();

			// Get stresses from constitutive model
			PrincipalStresses = ConstitutiveEquations.CalculateStresses(PrincipalStrains, reinforcement, referenceLength).ToPrincipal();
			Stresses          = PrincipalStresses.ToHorizontal();
		}

		/// <inheritdoc />
		public override bool Equals(Concrete? other) => other is BiaxialConcrete && base.Equals(other);

		/// <inheritdoc />
		public override bool Equals(object? obj) => obj is BiaxialConcrete concrete && Equals(concrete);

		/// <inheritdoc />
		public override int GetHashCode() => Parameters.GetHashCode();

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

		/// <inheritdoc />
		void IBiaxialMaterial.Calculate(StrainState strainState) => Calculate(strainState, null);

		/// <inheritdoc />
		public BiaxialConcrete Clone() => new(Parameters, Model);

		#endregion

	}
}