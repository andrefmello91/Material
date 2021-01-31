﻿using Material.Reinforcement.Uniaxial;
using static Material.Concrete.Constitutive;
using static Material.Concrete.Uniaxial.Constitutive;

namespace Material.Concrete.Uniaxial
{
	/// <summary>
	/// Concrete uniaxial class.
	/// </summary>
	public class UniaxialConcrete : Concrete
	{
		// Auxiliary fields
		private Parameters _parameters;

		/// <summary>
		/// Get concrete <see cref="Material.Concrete.Parameters"/>.
		/// </summary>
		public override Parameters Parameters
		{
			get => _parameters;
			set
			{
				_parameters = value;
				Constitutive.Parameters = value;
			}
		}

		/// <inheritdoc/>
		public sealed override ConstitutiveModel Model
		{
			get => ReadModel(Constitutive);
			set => Constitutive = Read(value, Parameters);
		}

		/// <summary>
		/// Get concrete <see cref="Uniaxial.Constitutive"/>.
		/// </summary>
		public Constitutive Constitutive { get; private set; }

		/// <summary>
		/// Returns true if concrete is cracked.
		/// </summary>
		public bool Cracked => Constitutive.Cracked;

		/// <summary>
		/// Get/set concrete strain.
		/// </summary>
		public double Strain { get; private set; }

		/// <summary>
        /// Get/set concrete stress.
        /// </summary>
		public double Stress { get; private set; }

		/// <summary>
        /// Get concrete area, in mm2.
        /// </summary>
		public double Area { get; }

		/// <summary>
		/// Calculate current secant module of concrete, in MPa.
		/// </summary>
		public double SecantModule => Constitutive.SecantModule(Stress, Strain);

		/// <summary>
		/// Calculate normal stiffness
		/// </summary>
		public double Stiffness => Ec * Area;

		/// <summary>
		/// Calculate maximum force resisted by concrete, in N (negative value).
		/// </summary>
		public double MaxForce => -fc * Area;

		/// <summary>
		/// Calculate current concrete force, in N.
		/// </summary>
		public double Force => Stress * Area;

		///<inheritdoc/>
		/// <summary>
		/// Concrete for uniaxial calculations.
		/// </summary>
		/// <param name="concreteArea">The concrete area, in mm2.</param>
		public UniaxialConcrete(double strength, double aggregateDiameter, double concreteArea, ParameterModel parameterModel = ParameterModel.MCFT, ConstitutiveModel model = ConstitutiveModel.MCFT, AggregateType aggregateType = AggregateType.Quartzite, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0)
			: this(Parameters.ReadParameters(parameterModel, strength, aggregateDiameter, aggregateType, tensileStrength, elasticModule, plasticStrain, ultimateStrain), concreteArea, model)
		{
		}

		///<inheritdoc/>
		/// <summary>
		/// Concrete for uniaxial calculations.
		/// </summary>
		///<param name="concreteArea">The concrete area, in mm2.</param>
		public UniaxialConcrete(Parameters parameters, double concreteArea, ConstitutiveModel model = ConstitutiveModel.MCFT) 
			: base(parameters)
		{
			Area        = concreteArea;
			_parameters = parameters;
			Model       = model;
		}

		/// <summary>
		/// Calculate force (in N) given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		/// <param name="reinforcement">The uniaxial reinforcement (only for DSFM).</param>
		public double CalculateForce(double strain, UniaxialReinforcement reinforcement = null) => Area * CalculateStress(strain, reinforcement);

		/// <summary>
        /// Calculate stress (in MPa) given strain.
        /// </summary>
        /// <param name="strain">Current strain.</param>
        /// <param name="reinforcement">The <see cref="UniaxialReinforcement"/> (only for <see cref="DSFMConstitutive"/>).</param>
        public double CalculateStress(double strain, UniaxialReinforcement reinforcement = null) => Constitutive.CalculateStress(strain, reinforcement);

		/// <summary>
		/// Set concrete strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStrain(double strain) => Strain = strain;

		/// <summary>
        /// Set concrete stress (in MPa) given strain.
        /// </summary>
        /// <param name="strain">Current strain.</param>
        /// <param name="reinforcement">The <see cref="UniaxialReinforcement"/> (only for <see cref="DSFMConstitutive"/>).</param>
		public void SetStress(double strain, UniaxialReinforcement reinforcement = null) => Stress = CalculateStress(strain, reinforcement);

		/// <summary>
        /// Set concrete strain and calculate stress, in MPa.
        /// </summary>
        /// <param name="strain">Current strain.</param>
        /// <param name="reinforcement">The <see cref="UniaxialReinforcement"/> (only for <see cref="DSFMConstitutive"/>).</param>
		public void SetStrainsAndStresses(double strain, UniaxialReinforcement reinforcement = null)
		{
			SetStrain(strain);
			SetStress(strain, reinforcement);
		}
		
        /// <summary>
        /// Return a copy of this <see cref="UniaxialConcrete"/> object.
        /// </summary>
        public UniaxialConcrete Copy() => new UniaxialConcrete(Parameters, Area, Model);


        /// <inheritdoc/>
        public override bool Equals(Concrete other) => other is UniaxialConcrete && (Parameters == other.Parameters && Model == other.Model);

        public override bool Equals(object obj) => obj is UniaxialConcrete concrete && Equals(concrete);

        public override int GetHashCode() => Parameters.GetHashCode();
	}
}