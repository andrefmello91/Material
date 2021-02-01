using Extensions;
using OnPlaneComponents;
using UnitsNet;
using UnitsNet.Units;

namespace Material.Concrete
{
	/// <summary>
	///     Concrete parameters struct.
	/// </summary>
	public partial struct Parameters : IParameter, ICloneable<Parameters>
	{
		#region Fields

		/// <summary>
		///     The default <see cref="Pressure" /> tolerance.
		/// </summary>
		public static readonly Pressure Tolerance = Pressure.FromPascals(1E-3);

		/// <summary>
		///     Calculator for concrete parameters.
		/// </summary>
		private readonly ParameterCalculator Calculator;

		#endregion

		#region Properties

		PressureUnit IUnitConvertible<IParameter, PressureUnit>.Unit
		{
			get => StressUnit;
			set => StressUnit = value;
		}

		LengthUnit IUnitConvertible<IParameter, LengthUnit>.Unit
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

		public ParameterModel Model => Calculator.Model;

		public Length AggregateDiameter { get; private set; }

		public Pressure TensileStrength => Calculator.TensileStrength.ToUnit(StressUnit);

		public Pressure ElasticModule => Calculator.ElasticModule.ToUnit(StressUnit);

		public Pressure SecantModule => Calculator.SecantModule.ToUnit(StressUnit);

		public double PlasticStrain => Calculator.PlasticStrain;

		public double UltimateStrain => Calculator.UltimateStrain;

		public double CrackingStrain => TensileStrength / ElasticModule;

		public Pressure TransverseModule => (SecantModule / 2.4).ToUnit(StressUnit);

		public ForcePerLength FractureParameter => Calculator.FractureParameter;

		public AggregateType Type { get; }

		#endregion

		#region Constructors

		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType)" />
		/// <param name="strengthUnit">The <see cref="PressureUnit" /> of <paramref name="strength" />.</param>
		/// <param name="diameterUnit">The <see cref="LengthUnit" /> of <paramref name="aggregateDiameter" />.</param>
		public Parameters(double strength, double aggregateDiameter, ParameterModel model = ParameterModel.MC2010, AggregateType type = AggregateType.Quartzite, PressureUnit strengthUnit = PressureUnit.Megapascal, LengthUnit diameterUnit = LengthUnit.Millimeter)
			: this(Pressure.From(strength, strengthUnit), Length.From(aggregateDiameter, diameterUnit), model, type)
		{
		}

		/// <summary>
		///     Parameters constructor.
		/// </summary>
		/// <param name="type">The <see cref="AggregateType" />.</param>
		/// <param name="strength">Concrete compressive strength (positive value).</param>
		/// <param name="aggregateDiameter">The maximum diameter of concrete aggregate.</param>
		/// <param name="model">The <see cref="ParameterModel" />.</param>
		public Parameters(Pressure strength, Length aggregateDiameter, ParameterModel model = ParameterModel.MC2010, AggregateType type = AggregateType.Quartzite)
		{
			Strength = strength;
			AggregateDiameter = aggregateDiameter;
			Type = type;
			Calculator = ParameterCalculator.GetCalculator(strength, model, type);
		}

		#endregion

		#region

		/// <summary>
		///     Get concrete class C20 (fc = 20 MPa).
		/// </summary>
		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType)" />
		public static Parameters C20(Length aggregateDiameter, ParameterModel model = ParameterModel.MC2010, AggregateType type = AggregateType.Quartzite) => new Parameters(Pressure.FromMegapascals(20), aggregateDiameter, model, type);

		/// <summary>
		///     Get concrete class C30 (fc = 30 MPa).
		/// </summary>
		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType)" />
		public static Parameters C30(Length aggregateDiameter, ParameterModel model = ParameterModel.MC2010, AggregateType type = AggregateType.Quartzite) => new Parameters(Pressure.FromMegapascals(30), aggregateDiameter, model, type);

		/// <summary>
		///     Get concrete class C40 (fc = 40 MPa).
		/// </summary>
		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType)" />
		public static Parameters C40(Length aggregateDiameter, ParameterModel model = ParameterModel.MC2010, AggregateType type = AggregateType.Quartzite) => new Parameters(Pressure.FromMegapascals(40), aggregateDiameter, model, type);

		/// <summary>
		///     Get concrete class C50 (fc = 40 MPa).
		/// </summary>
		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType)" />
		public static Parameters C50(Length aggregateDiameter, ParameterModel model = ParameterModel.MC2010, AggregateType type = AggregateType.Quartzite) => new Parameters(Pressure.FromMegapascals(50), aggregateDiameter, model, type);

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

		public IParameter Convert(LengthUnit unit) => new Parameters(Strength, AggregateDiameter.ToUnit(unit), Model, Type);

		/// <summary>
		///     Change <see cref="Strength" /> unit.
		/// </summary>
		/// <param name="unit">The desired <see cref="LengthUnit" />.</param>
		public void ChangeUnit(PressureUnit unit)
		{
			if (unit == StressUnit)
				return;

			Strength = Strength.ToUnit(unit);
		}

		public IParameter Convert(PressureUnit unit) => new Parameters(Strength.ToUnit(unit), AggregateDiameter, Model, Type);

		/// <summary>
		///     Create a clone of this object with converted units.
		/// </summary>
		/// <param name="stressUnit">The desired <see cref="PressureUnit" />.</param>
		/// <param name="lengthUnit">The desired <see cref="LengthUnit" />.</param>
		public Parameters Convert(PressureUnit stressUnit, LengthUnit lengthUnit) => new Parameters(Strength.ToUnit(stressUnit), AggregateDiameter.ToUnit(lengthUnit), Model, Type);

		public bool Approaches(IParameter other, Pressure tolerance) => Model == other.Model && Strength.Approx(other.Strength, tolerance);

		public Parameters Clone() => new Parameters(Strength, AggregateDiameter, Model, Type);

		/// <remarks>
		///     <see cref="Strength" /> is compared.
		/// </remarks>
		/// <inheritdoc />
		public int CompareTo(IParameter other) =>
			Strength == other.Strength
				? 0
				: Strength > other.Strength
					? 1
					: -1;

		public bool Equals(IParameter other) => Approaches(other, Tolerance);

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


		public override bool Equals(object obj) => obj is Parameters other && Equals(other);

		public override int GetHashCode() => (int) Strength.Megapascals * (int) AggregateDiameter.Millimeters;

		#endregion
	}
}