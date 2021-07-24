using andrefmello91.Extensions;
using andrefmello91.Material.Reinforcement;
using UnitsNet;
#nullable enable

namespace andrefmello91.Material.Concrete
{
	/// <summary>
	///     Concrete uniaxial class.
	/// </summary>
	public partial class UniaxialConcrete : Concrete, IUniaxialMaterial, ICloneable<UniaxialConcrete>
	{

		#region Fields

		/// <summary>
		///     Get concrete <see cref="UniaxialConcrete.Constitutive" />.
		/// </summary>
		private readonly Constitutive _constitutive;

		#endregion

		#region Properties

		/// <summary>
		///     The concrete area.
		/// </summary>
		public Area Area { get; }

		/// <summary>
		///     The current concrete force.
		/// </summary>
		public Force Force => Stress * Area;

		/// <summary>
		///     Calculate maximum force resisted by concrete (negative value).
		/// </summary>
		public Force MaxForce => -Parameters.Strength * Area;

		/// <summary>
		///     Calculate current secant module of concrete.
		/// </summary>
		public Pressure SecantModule => _constitutive.SecantModule(Stress, Strain);

		/// <summary>
		///     Calculate normal stiffness.
		/// </summary>
		public Force Stiffness => Parameters.ElasticModule * Area;

		/// <summary>
		///     The concrete strain.
		/// </summary>
		public double Strain { get; private set; }

		/// <summary>
		///     The concrete stress.
		/// </summary>
		public Pressure Stress { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		///     Concrete for uniaxial calculations.
		/// </summary>
		/// <param name="concreteArea">The concrete cross-section area.</param>
		/// <inheritdoc cref="Concrete(IParameters, ConstitutiveModel)" />
		public UniaxialConcrete(IParameters parameters, Area concreteArea, ConstitutiveModel model = ConstitutiveModel.MCFT)
			: base(parameters, model)
		{
			Area          = concreteArea;
			_constitutive = Constitutive.From(model, parameters);
		}

		#endregion

		#region Methods

		/// <summary>
		///     Calculate stress given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		/// <param name="reinforcement">
		///     The <see cref="UniaxialReinforcement" /> (only for
		///     <see cref="UniaxialConcrete.DSFMConstitutive" />).
		/// </param>
		private Pressure CalculateStress(double strain, UniaxialReinforcement? reinforcement = null) => _constitutive.CalculateStress(strain, reinforcement);

		/// <summary>
		///     Set concrete strain and calculate stress, in MPa.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		/// <param name="reinforcement">
		///     The <see cref="UniaxialReinforcement" /> (only for
		///     <see cref="UniaxialConcrete.DSFMConstitutive" />).
		/// </param>
		public void Calculate(double strain, UniaxialReinforcement? reinforcement = null)
		{
			Strain = strain;
			Stress = CalculateStress(strain, reinforcement);
		}

		/// <inheritdoc />
		void IUniaxialMaterial.Calculate(double strain) => Calculate(strain);

		#region Interface Implementations

		/// <inheritdoc />
		public UniaxialConcrete Clone() => new(Parameters, Area, Model);

		#endregion

		#region Object override

		/// <inheritdoc />
		public override bool Equals(Concrete? other) => other is UniaxialConcrete && base.Equals(other);

		/// <inheritdoc />
		public override bool Equals(object? obj) => obj is UniaxialConcrete concrete && base.Equals(concrete);

		/// <inheritdoc />
		public override int GetHashCode() => Parameters.GetHashCode();

		#endregion

		#endregion

	}
}