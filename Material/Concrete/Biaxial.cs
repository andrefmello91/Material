using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Material.Reinforcement;
using OnPlaneComponents;

namespace Material.Concrete
{
	public class BiaxialConcrete : Concrete
	{
		/// <summary>
        /// Get/set concrete <see cref="StrainState"/>.
        /// </summary>
		public StrainState Strains { get; private set; }

		/// <summary>
        /// Get/set concrete <see cref="PrincipalStrainState"/>.
        /// </summary>
		public PrincipalStrainState PrincipalStrains { get; private  set; }

		/// <summary>
        /// Get/set concrete <see cref="StressState"/>.
        /// </summary>
		public StressState Stresses { get; private  set; }

		/// <summary>
        /// Get/set concrete <see cref="PrincipalStressState"/>.
        /// </summary>
		public PrincipalStressState PrincipalStresses { get; private set; }

		///<inheritdoc/>
		/// <summary>
		/// Concrete for membrane calculations.
		/// </summary>
		public BiaxialConcrete(double strength, double aggregateDiameter, ParameterModel parameterModel = ParameterModel.MCFT, ConstitutiveModel constitutiveModel = ConstitutiveModel.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0) : base(strength, aggregateDiameter, parameterModel, constitutiveModel, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain)
		{
		}

		///<inheritdoc/>
		/// <summary>
		/// Concrete for membrane calculations.
		/// </summary>
		public BiaxialConcrete(Parameters parameters, ConstitutiveModel constitutiveModel = ConstitutiveModel.MCFT) : base(parameters, constitutiveModel)
		{
		}

		///<inheritdoc/>
		/// <summary>
		/// Concrete for membrane calculations.
		/// </summary>
		public BiaxialConcrete(Parameters parameters, Constitutive constitutive) : base(parameters, constitutive)
		{
		}

        /// <summary>
        /// Get concrete stiffness <see cref="Matrix"/>.
        /// </summary>
        public Matrix<double> Stiffness
		{
			get
			{
				var (Ec1, Ec2) = SecantModule;

				double Gc = Ec1 * Ec2 / (Ec1 + Ec2);

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
        /// Get concrete initial stiffness <see cref="Matrix"/>.
        /// </summary>
        public Matrix<double> InitialStiffness
        {
	        get
	        {
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
        /// Calculate current secant module of concrete, in MPa.
        /// </summary>
        private (double Ec1, double Ec2) SecantModule
		{
			get
			{
				// Get values
				double
					ec1 = PrincipalStrains.Epsilon1,
					ec2 = PrincipalStrains.Epsilon2,
					fc1 = PrincipalStresses.Sigma1,
					fc2 = PrincipalStresses.Sigma2;

				// Calculate modules
				double
					Ec1 = Constitutive.SecantModule(fc1, ec1),
					Ec2 = Constitutive.SecantModule(fc2, ec2);

				return
					(Ec1, Ec2);
			}
		}

        /// <summary>
        /// Set concrete <see cref="StressState"/> given <see cref="StrainState"/>
        /// </summary>
        /// <param name="strains">Current <see cref="StrainState"/> in concrete.</param>
        /// <param name="referenceLength">The reference length (only for <see cref="DSFMConstitutive"/>).</param>
        /// <param name="reinforcement">The <see cref="WebReinforcement"/> (only for <see cref="DSFMConstitutive"/>)</param>
        public void CalculatePrincipalStresses(StrainState strains, WebReinforcement reinforcement = null, double referenceLength = 0)
		{
			// Get strains
			Strains = strains.Copy();

			// Calculate principal strains
			PrincipalStrains = PrincipalStrainState.FromStrain(Strains);

			// Get stresses from constitutive model
			PrincipalStresses = Constitutive.CalculateStresses(PrincipalStrains, referenceLength, reinforcement);
			Stresses          = StressState.FromPrincipal(PrincipalStresses);
		}

		/// <summary>
		/// Set tensile stress.
		/// </summary>
		/// <param name="fc1">Concrete tensile stress, in MPa.</param>
		public void SetTensileStress(double fc1)
		{
			// Get compressive stress and theta1
			double
				fc2    = PrincipalStresses.Sigma2,
				theta1 = PrincipalStresses.Theta1;

			// Set new state
			PrincipalStresses = new PrincipalStressState(fc1, fc2, theta1);
			Stresses          = StressState.FromPrincipal(PrincipalStresses);
		}

        /// <summary>
        /// Return a copy of this <see cref="BiaxialConcrete"/> object.
        /// </summary>
        public BiaxialConcrete Copy() => new BiaxialConcrete(Parameters, Constitutive);

        /// <inheritdoc/>
        public override bool Equals(Concrete other) => other is BiaxialConcrete && base.Equals(other);

        public override bool Equals(object obj) => obj is BiaxialConcrete concrete && base.Equals(concrete);

        public override int GetHashCode() => Parameters.GetHashCode();
	}
}