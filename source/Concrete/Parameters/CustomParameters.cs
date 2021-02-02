using Extensions;
using OnPlaneComponents;
using UnitsNet;
using UnitsNet.Units;

namespace Material.Concrete
{
	/// <summary>
	///     Concrete parameters struct.
	/// </summary>
	public struct CustomParameters : IParameters, ICloneable<CustomParameters>
	{
		#region Properties

		PressureUnit IUnitConvertible<IParameters, PressureUnit>.Unit
		{
			get => StressUnit;
			set => StressUnit = value;
		}

		LengthUnit IUnitConvertible<IParameters, LengthUnit>.Unit
		{
			get => DiameterUnit;
			set => DiameterUnit = value;
		}

		public PressureUnit StressUnit
		{
			get => Strength.Unit;
			set => ChangeUnit(value);
		}

		public LengthUnit DiameterUnit
		{
			get => AggregateDiameter.Unit;
			set => ChangeUnit(value);
		}

		public Pressure Strength { get; private set; }

		public ParameterModel Model => ParameterModel.Custom;

		public Length AggregateDiameter { get; private set; }

		public Pressure TensileStrength { get; private set; }

		public Pressure ElasticModule { get; private set; }

		public Pressure SecantModule => Strength / PlasticStrain;

		public double PlasticStrain { get;  }

		public double UltimateStrain { get;  }

		public double CrackingStrain => TensileStrength / ElasticModule;

		public Pressure TransverseModule => (SecantModule / 2.4).ToUnit(StressUnit);

		public ForcePerLength FractureParameter => ForcePerLength.FromNewtonsPerMillimeter(0.075);

		#endregion

		#region Constructors

		/// <inheritdoc cref="CustomParameters(Pressure, Pressure, Pressure, Length, double, double)" />
		public CustomParameters(double strength, double tensileStrength, double elasticModule, double aggregateDiameter, double plasticStrain = 0.002, double ultimateStrain = 0.0035, PressureUnit strengthUnit = PressureUnit.Megapascal, LengthUnit diameterUnit = LengthUnit.Millimeter)
			: this(Pressure.From(strength, strengthUnit), Pressure.From(tensileStrength, strengthUnit), Pressure.From(elasticModule, strengthUnit), Length.From(aggregateDiameter, diameterUnit), plasticStrain, ultimateStrain)
		{
		}

		/// <summary>
		///     Custom parameters constructor.
		/// </summary>
		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType)" />
		/// <param name="tensileStrength">Concrete tensile strength.</param>
		/// <param name="elasticModule">Concrete initial elastic module.</param>
		/// <param name="plasticStrain">Concrete plastic strain (positive or negative value).</param>
		/// <param name="ultimateStrain">Concrete ultimate strain (positive or negative value).</param>
		public CustomParameters(Pressure strength, Pressure tensileStrength, Pressure elasticModule, Length aggregateDiameter, double plasticStrain = 0.002, double ultimateStrain = 0.0035)
		{
			Strength          = strength.Abs();
			TensileStrength   = tensileStrength;
			ElasticModule     = elasticModule;
			PlasticStrain     = - plasticStrain.Abs();
			UltimateStrain    = - ultimateStrain.Abs();
			AggregateDiameter = aggregateDiameter;
		}

		#endregion

		#region

		/// <summary>
		///     Change <see cref="AggregateDiameter" /> unit.
		/// </summary>
		/// <param name="unit">The desired <see cref="LengthUnit" />.</param>
		public void ChangeUnit(LengthUnit unit)
		{
			if (unit == DiameterUnit)
				return;

			AggregateDiameter = AggregateDiameter.ToUnit(unit);
		}

		public IParameters Convert(LengthUnit unit) => new CustomParameters(Strength, TensileStrength, ElasticModule, AggregateDiameter.ToUnit(unit), PlasticStrain, UltimateStrain);

		/// <summary>
		///     Change <see cref="Strength" /> unit.
		/// </summary>
		/// <param name="unit">The desired <see cref="LengthUnit" />.</param>
		public void ChangeUnit(PressureUnit unit)
		{
			if (unit == StressUnit)
				return;

			Strength        = Strength.ToUnit(unit);
			TensileStrength = TensileStrength.ToUnit(unit);
			ElasticModule   = ElasticModule.ToUnit(unit);
		}

		public IParameters Convert(PressureUnit unit) => new CustomParameters(Strength.ToUnit(unit), TensileStrength.ToUnit(unit), ElasticModule.ToUnit(unit), AggregateDiameter, PlasticStrain, UltimateStrain);

		/// <summary>
		///     Create a clone of this object with converted units.
		/// </summary>
		/// <param name="stressUnit">The desired <see cref="PressureUnit" />.</param>
		/// <param name="lengthUnit">The desired <see cref="LengthUnit" />.</param>
		public CustomParameters Convert(PressureUnit stressUnit, LengthUnit lengthUnit) => new CustomParameters(Strength.ToUnit(stressUnit), TensileStrength.ToUnit(stressUnit), ElasticModule.ToUnit(stressUnit), AggregateDiameter.ToUnit(lengthUnit), PlasticStrain, UltimateStrain);

		public bool Approaches(IParameters? other, Pressure tolerance) => Model == other?.Model && Strength.Approx(other.Strength, tolerance);

		public CustomParameters Clone() => new CustomParameters(Strength, TensileStrength, ElasticModule, AggregateDiameter, PlasticStrain, UltimateStrain);

		/// <remarks>
		///     <see cref="Strength" /> is compared.
		/// </remarks>
		/// <inheritdoc />
		public int CompareTo(IParameters? other) =>
			Strength == other?.Strength
				? 0
				: Strength > other?.Strength
					?  1
					: -1;

		public bool Equals(IParameters? other) => Approaches(other, Parameters.Tolerance);

		public override string ToString()
		{
			char
				phi = (char) Characters.Phi,
				eps = (char) Characters.Epsilon;

			return
				"Concrete Parameters:\n\n" +
				$"fc = {Strength}\n" +
				$"ft = {TensileStrength}\n" +
				$"Ec = {ElasticModule}\n" +
				$"{eps}c = {PlasticStrain:0.##E+00}\n" +
				$"{eps}cu = {UltimateStrain:0.##E+00}\n" +
				$"{phi},ag = {AggregateDiameter}";
		}


		public override bool Equals(object? obj) => obj is CustomParameters other && Equals(other);

		public override int GetHashCode() => (int) Strength.Megapascals * (int) AggregateDiameter.Millimeters;

		#endregion

		#region Operators

		public static bool operator ==(CustomParameters left, IParameters right) => !(right is null) && left.Equals(right);

		public static bool operator !=(CustomParameters left, IParameters right) => !(right is null) && !left.Equals(right);

		#endregion
	}
}