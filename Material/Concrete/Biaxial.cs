using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Relations;
using Material.Reinforcement;

namespace Material.Concrete
{
	public class BiaxialConcrete : Concrete
	{
		// Properties
		public Vector<double>                 Strains           { get; set; }
		public (double theta1, double theta2) PrincipalAngles   { get; set; }
		public (double ec1, double ec2)       PrincipalStrains  { get; set; }
		public (double fc1, double fc2)       PrincipalStresses { get; set; }
		public Matrix<double>                 Stiffness         { get; set; }

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
				var (ec1, ec2) = PrincipalStrains;
				var (fc1, fc2) = PrincipalStresses;

				// Calculate modules
				double
					Ec1 = Constitutive.SecantModule(fc1, ec1),
					Ec2 = Constitutive.SecantModule(fc2, ec2);

				return
					(Ec1, Ec2);
			}
		}

		/// <summary>
		/// Get current stresses
		/// </summary>
		public Vector<double> Stresses => Stiffness * Strains;

		/// <summary>
		/// Set concrete stresses given strains
		/// </summary>
		/// <param name="strains">Current strains.</param>
		/// <param name="referenceLength">The reference length (only for DSFM).</param>
		/// <param name="reinforcement">The biaxial reinforcement (only for DSFM)</param>
		public void CalculatePrincipalStresses(Vector<double> strains, double referenceLength = 0, BiaxialReinforcement reinforcement = null)
		{
			// Get strains and principals
			Strains          = strains;
			PrincipalStrains = Principal_Strains();
			PrincipalAngles  = StrainAngles();

			// Calculate stresses
			double
				fc1 = Constitutive.TensileStress(PrincipalStrains, referenceLength, PrincipalAngles.theta1, reinforcement),
				fc2 = Constitutive.CompressiveStress(PrincipalStrains);

			PrincipalStresses = (fc1, fc2);
		}

		/// <summary>
		/// Calculate concrete stiffness matrix
		/// </summary>
		/// <param name="thetaC1">Principal tensile strain angle (radians).</param>
		/// <param name="concreteSecantModule">Current secant module, in MPa.</param>
		public void CalculateStiffness(double? thetaC1 = null, (double Ec1, double Ec2)? concreteSecantModule = null)
		{
			var (Ec1, Ec2) = concreteSecantModule ?? SecantModule;

			double Gc = Ec1 * Ec2 / (Ec1 + Ec2);

			// Concrete matrix
			var Dc1 = Matrix<double>.Build.Dense(3, 3);
			Dc1[0, 0] = Ec1;
			Dc1[1, 1] = Ec2;
			Dc1[2, 2] = Gc;

			// Get transformation matrix
			var T = TransformationMatrix(thetaC1);

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
			double fc2 = PrincipalStresses.fc2;

			// Set
			PrincipalStresses = (fc1, fc2);
		}

		/// <summary>
		/// Calculate principal strain angles, in radians.
		/// </summary>
		/// <param name="strains">Current strains.</param>
		/// <param name="principalStrains">Current principal strains.</param>
		public (double theta1, double theta2) StrainAngles(Vector<double> strains = null, (double ec1, double ec2)? principalStrains = null)
		{
			var e  = strains          ?? Strains;
			var e1 = principalStrains ?? PrincipalStrains;

			return
				Strain.PrincipalAngles(e, e1);
		}

		/// <summary>
		/// Calculate principal strains.
		/// </summary>
		/// <param name="strains">Current strains.</param>
		/// <returns></returns>
		public (double ec1, double ec2) Principal_Strains(Vector<double> strains = null)
		{
			var e = strains ?? Strains;

			return
				Strain.PrincipalStrains(e);
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
			var T = TransformationMatrix(Constants.PiOver4);

			// Calculate Dc
			return
				T.Transpose() * Dc1 * T;
		}

		/// <summary>
		/// Calculate stresses/strains transformation matrix.
		/// <para>This matrix transforms from x-y to 1-2 coordinates.</para>
		/// </summary>
		/// <param name="theta1">Principal tensile strain angle, in radians.</param>
		/// <returns></returns>
		public Matrix<double> TransformationMatrix(double? theta1 = null)
		{
			double theta = theta1 ?? PrincipalAngles.theta1;

			return
				Strain.TransformationMatrix(theta);
		}
	}
}