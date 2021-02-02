using Material.Reinforcement.Uniaxial;
using OnPlaneComponents;

namespace Material.Concrete.Uniaxial
{
	/// <summary>
	///     Concrete uniaxial class.
	/// </summary>
	public partial class UniaxialConcrete : Concrete, ICloneable<UniaxialConcrete>
	{
		#region Fields

		/// <summary>
		///     Get concrete <see cref="Uniaxial.Constitutive" />.
		/// </summary>
		private readonly Constitutive _constitutive;

		#endregion

		#region Properties

		/// <summary>
		///     Get concrete area, in mm2.
		/// </summary>
		public double Area { get; }

		/// <summary>
		///     Returns true if concrete is cracked.
		/// </summary>
		public bool Cracked => _constitutive.Cracked;

		/// <summary>
		///     Calculate current concrete force, in N.
		/// </summary>
		public double Force => Stress * Area;

		/// <summary>
		///     Calculate maximum force resisted by concrete, in N (negative value).
		/// </summary>
		public double MaxForce => -Parameters.Strength.Megapascals * Area;

		/// <summary>
		///     Calculate current secant module of concrete, in MPa.
		/// </summary>
		public double SecantModule => _constitutive.SecantModule(Stress, Strain);

		/// <summary>
		///     Calculate normal stiffness, in N.
		/// </summary>
		public double Stiffness => Parameters.ElasticModule.Megapascals * Area;

		/// <summary>
		///     Get/set concrete strain.
		/// </summary>
		public double Strain { get; private set; }

		/// <summary>
		///     Get/set concrete stress.
		/// </summary>
		public double Stress { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		///     Concrete for uniaxial calculations.
		/// </summary>
		/// <param name="concreteArea">The concrete area, in mm2.</param>
		/// <inheritdoc cref="Concrete(IParameters, ConstitutiveModel)" />
		public UniaxialConcrete(IParameters parameters, double concreteArea, ConstitutiveModel model = ConstitutiveModel.MCFT)
			: base(parameters, model)
		{
			Area          = concreteArea;
			_constitutive = Constitutive.Read(model, parameters);
		}

		#endregion

		#region

		/// <summary>
		///     Calculate force (in N) given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		/// <param name="reinforcement">The uniaxial reinforcement (only for DSFM).</param>
		public double CalculateForce(double strain, UniaxialReinforcement reinforcement = null) => Area * CalculateStress(strain, reinforcement);

		/// <summary>
		///     Calculate stress (in MPa) given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		/// <param name="reinforcement">The <see cref="UniaxialReinforcement" /> (only for <see cref="DSFMConstitutive" />).</param>
		public double CalculateStress(double strain, UniaxialReinforcement reinforcement = null) => _constitutive.CalculateStress(strain, reinforcement);

		/// <summary>
		///     Set concrete strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStrain(double strain) => Strain = strain;

		/// <summary>
		///     Set concrete stress (in MPa) given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		/// <param name="reinforcement">The <see cref="UniaxialReinforcement" /> (only for <see cref="DSFMConstitutive" />).</param>
		public void SetStress(double strain, UniaxialReinforcement reinforcement = null) => Stress = CalculateStress(strain, reinforcement);

		/// <summary>
		///     Set concrete strain and calculate stress, in MPa.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		/// <param name="reinforcement">The <see cref="UniaxialReinforcement" /> (only for <see cref="DSFMConstitutive" />).</param>
		public void SetStrainsAndStresses(double strain, UniaxialReinforcement reinforcement = null)
		{
			SetStrain(strain);
			SetStress(strain, reinforcement);
		}

		public UniaxialConcrete Clone() => new UniaxialConcrete(Parameters, Area, Model);

		/// <inheritdoc />
		public override bool Equals(IConcrete other) => other is UniaxialConcrete && base.Equals(other);

		public override bool Equals(object obj) => obj is UniaxialConcrete concrete && Equals(concrete);

		public override int GetHashCode() => Parameters.GetHashCode();

		#endregion
	}
}