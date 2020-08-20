using Material.Reinforcement;

namespace Material.Concrete
{
	/// <summary>
	/// Concrete uniaxial class.
	/// </summary>
	public class UniaxialConcrete : Concrete
	{
		// Properties
		public double Strain  { get; set; }
		public double Stress  { get; set; }
		public double Area    { get; }

		///<inheritdoc/>
		/// <summary>
		/// Concrete for uniaxial calculations.
		/// </summary>
		/// <param name="concreteArea">The concrete area, in mm2.</param>
		public UniaxialConcrete(double strength, double aggregateDiameter, double concreteArea, ParameterModel parameterModel = ParameterModel.MCFT, ConstitutiveModel constitutiveModel = ConstitutiveModel.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0) : base(strength, aggregateDiameter, parameterModel, constitutiveModel, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain)
		{
			Area = concreteArea;
		}

		///<inheritdoc/>
		/// <summary>
		/// Concrete for uniaxial calculations.
		/// </summary>
		///<param name="concreteArea">The concrete area, in mm2.</param>
		public UniaxialConcrete(Parameters parameters, double concreteArea, ConstitutiveModel constitutiveModel = ConstitutiveModel.MCFT) : base(parameters, constitutiveModel)
		{
			Area = concreteArea;
		}

		///<inheritdoc/>
		/// <summary>
		/// Concrete for uniaxial calculations.
		/// </summary>
		///<param name="concreteArea">The concrete area, in mm2.</param>
		public UniaxialConcrete(Parameters parameters, double concreteArea, Constitutive constitutive) : base(parameters, constitutive)
		{
			Area = concreteArea;
		}

		/// <summary>
		/// Calculate current secant module of concrete, in MPa.
		/// </summary>
		public double SecantModule => Constitutive.SecantModule(Stress, Strain);

		/// <summary>
		/// Calculate normal stiffness
		/// </summary>
		public double Stiffness => Ec * Area;

		/// <summary>
		/// Calculate maximum force resisted by concrete, in N.
		/// </summary>
		public double MaxForce => -fc * Area;

		/// <summary>
		/// Calculate current concrete force, in N.
		/// </summary>
		public double Force => Stress * Area;

		/// <summary>
		/// Calculate force (in N) given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		/// <param name="referenceLength">The reference length (only for DSFM).</param>
		/// <param name="reinforcement">The uniaxial reinforcement (only for DSFM).</param>
		public double CalculateForce(double strain, double referenceLength = 0, UniaxialReinforcement reinforcement = null)
		{
			double stress = CalculateStress(strain, referenceLength, reinforcement);

			return
				stress * Area;
		}

		/// <summary>
		/// Calculate stress (in MPa) given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		/// <param name="referenceLength">The reference length (only for DSFM).</param>
		/// <param name="reinforcement">The uniaxial reinforcement (only for DSFM).</param>
		public double CalculateStress(double strain, double referenceLength = 0, UniaxialReinforcement reinforcement = null)
		{
			if (strain == 0)
				return 0;

			if (strain > 0)
				return
					Constitutive.TensileStress(strain, referenceLength, reinforcement);

			return
				Constitutive.CompressiveStress(strain);
		}

		/// <summary>
		/// Set concrete strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStrain(double strain)
		{
			Strain = strain;
		}

		/// <summary>
		/// Set concrete stress (in MPa) given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		/// <param name="referenceLength">The reference length (only for DSFM).</param>
		/// <param name="reinforcement">The uniaxial reinforcement (only for DSFM).</param>
		public void SetStress(double strain, double referenceLength = 0, UniaxialReinforcement reinforcement = null)
		{
			Stress = CalculateStress(strain, referenceLength, reinforcement);
		}

		/// <summary>
		/// Set concrete strain and calculate stress, in MPa.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		/// <param name="referenceLength">The reference length (only for DSFM).</param>
		/// <param name="reinforcement">The uniaxial reinforcement (only for DSFM).</param>
		public void SetStrainsAndStresses(double strain, double referenceLength = 0, UniaxialReinforcement reinforcement = null)
		{
			SetStrain(strain);
			SetStress(strain, referenceLength, reinforcement);
		}

		/// <summary>
		/// Return a copy of a <see cref="UniaxialConcrete"/> object.
		/// </summary>
		/// <param name="concreteToCopy">The <see cref="UniaxialConcrete"/> object to copy.</param>
		/// <returns></returns>
		public static UniaxialConcrete Copy(UniaxialConcrete concreteToCopy) => new UniaxialConcrete(concreteToCopy.Parameters, concreteToCopy.Area, concreteToCopy.Constitutive);

        /// <inheritdoc/>
        public override bool Equals(Concrete other)
		{
			if (other != null && other is UniaxialConcrete)
				return Parameters == other.Parameters && Constitutive == other.Constitutive;

			return false;
		}

		public override bool Equals(object other)
		{
			if (other != null && other is UniaxialConcrete concrete)
				return Equals(concrete);

			return false;
		}

		public override int GetHashCode() => Parameters.GetHashCode();
	}
}