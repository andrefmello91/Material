using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Material.Reinforcement;
using OnPlaneComponents;

namespace Material.Concrete
{
	public class BiaxialConcrete : Concrete
	{
		/// <summary>
        /// Get/set concrete strains.
        /// </summary>
		public Strain Strains { get; set; }

		/// <summary>
        /// Get/set concrete stresses.
        /// </summary>
		public Stress Stresses { get; set; }

		/// <summary>
        /// Get/set concrete stiffness
        /// </summary>
		public Matrix<double> Stiffness { get; set; }

		///<inheritdoc/>
		/// <summary>
		/// Concrete for membrane calculations.
		/// </summary>
		public BiaxialConcrete(double strength, double aggregateDiameter, ParameterModel parameterModel = ParameterModel.MCFT, ConstitutiveModel constitutiveModel = ConstitutiveModel.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0) : base(strength, aggregateDiameter, parameterModel, constitutiveModel, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain)
		{
			Stiffness = InitialStiffness();
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
		/// Calculate current secant module of concrete, in MPa.
		/// </summary>
		public (double Ec1, double Ec2) SecantModule
		{
			get
			{
				// Verify strains
				// Get values
				var (ec1, ec2) = Strains.PrincipalStrains;
				var (fc1, fc2) = Stresses.PrincipalStresses;

				// Calculate modules
				double
					Ec1 = Constitutive.SecantModule(fc1, ec1),
					Ec2 = Constitutive.SecantModule(fc2, ec2);

				return
					(Ec1, Ec2);
			}
		}

		/// <summary>
		/// Set concrete stresses given strains
		/// </summary>
		/// <param name="strains">Current strains in concrete.</param>
		/// <param name="referenceLength">The reference length (only for DSFM).</param>
		/// <param name="reinforcement">The biaxial reinforcement (only for DSFM)</param>
		public void CalculatePrincipalStresses(Strain strains, double referenceLength = 0, BiaxialReinforcement reinforcement = null)
		{
			// Get strains and principals
			Strains = strains;

			var principalStrains = Strains.PrincipalStrains;
			var principalAngles  = Strains.PrincipalAngles;

			// Calculate stresses
			double
				fc1 = Constitutive.TensileStress(principalStrains, referenceLength, principalAngles.theta1, reinforcement),
				fc2 = Constitutive.CompressiveStress(principalStrains);

			Stresses = Stress.FromPrincipal(fc1, fc2, principalAngles.theta1);
		}

		/// <summary>
		/// Calculate concrete stiffness matrix
		/// </summary>
		public void CalculateStiffness()
		{
			var (Ec1, Ec2) = SecantModule;

			double Gc = Ec1 * Ec2 / (Ec1 + Ec2);

			// Concrete matrix
			var Dc1 = Matrix<double>.Build.Dense(3, 3);
			Dc1[0, 0] = Ec1;
			Dc1[1, 1] = Ec2;
			Dc1[2, 2] = Gc;

			// Get transformation matrix
			var T = Strains.TransformationMatrix;

			// Calculate Dc
			Stiffness = T.Transpose() * Dc1 * T;
		}

		/// <summary>
		/// Set tensile stress.
		/// </summary>
		/// <param name="fc1">Concrete tensile stress, in MPa.</param>
		public void SetTensileStress(double fc1)
		{
			// Get compressive stress
			double fc2 = Stresses.PrincipalStresses.sigma2;

			// Set
			Stresses = Stress.FromPrincipal(fc1, fc2, Strains.PrincipalAngles.theta1);
		}

		/// <summary>
		/// Calculate concrete initial stiffness.
		/// </summary>
		/// <returns>Initial stiffness matrix.</returns>
		public Matrix<double> InitialStiffness()
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

        /// <summary>
        /// Return a copy of a <see cref="BiaxialConcrete"/> object.
        /// </summary>
        /// <param name="concreteToCopy">The <see cref="BiaxialConcrete"/> object to copy.</param>
        /// <returns></returns>
        public static BiaxialConcrete Copy(BiaxialConcrete concreteToCopy) => new BiaxialConcrete(concreteToCopy.Parameters, concreteToCopy.Constitutive);

        /// <inheritdoc/>
        public override bool Equals(Concrete other)
		{
			if (other != null && other is BiaxialConcrete)
				return Parameters == other.Parameters && Constitutive == other.Constitutive;

			return false;
		}

		public override bool Equals(object other)
		{
			if (other != null && other is BiaxialConcrete concrete)
				return Equals(concrete);

			return false;
		}

		public override int GetHashCode() => Parameters.GetHashCode();
	}
}