using System;
using andrefmello91.Extensions;
using UnitsNet;
using UnitsNet.Units;
#nullable enable

namespace andrefmello91.Material.Concrete
{
	/// <summary>
	///     Concrete parameters struct.
	/// </summary>
	public struct Parameters : IConcreteParameters, ICloneable<Parameters>
	{

		#region Fields

		/// <summary>
		///     The default <see cref="Pressure" /> tolerance.
		/// </summary>
		public static readonly Pressure Tolerance = Pressure.FromPascals(1E-3);

		private Length _aggDiameter;

		/// <summary>
		///     Calculator for concrete parameters.
		/// </summary>
		private ParameterCalculator _calculator;

		private Pressure _strength;

		#endregion

		#region Properties

		/// <inheritdoc />
		public Length AggregateDiameter
		{
			get => _aggDiameter;
			set => _aggDiameter = value.ToUnit(DiameterUnit);
		}

		/// <inheritdoc />
		public Pressure CompressiveStrength => Strength;

		/// <inheritdoc />
		public bool ConsiderConfinement { get; set; }

		/// <inheritdoc />
		public double CrackingStrain => TensileStrength / ElasticModule;

		/// <inheritdoc />
		public LengthUnit DiameterUnit
		{
			get => AggregateDiameter.Unit;
			set => ChangeUnit(value);
		}

		/// <inheritdoc />
		public Pressure ElasticModule => _calculator.ElasticModule.ToUnit(StressUnit);

		/// <inheritdoc />
		public ForcePerLength FractureParameter => _calculator.FractureParameter;

		/// <inheritdoc />
		public ParameterModel Model
		{
			get => _calculator.Model;
			set
			{
				if (value == _calculator.Model)
					return;

				// Change model
				_calculator = ParameterCalculator.GetCalculator(Strength, value, Type);
			}
		}

		/// <inheritdoc />
		public double PlasticStrain => _calculator.PlasticStrain;

		/// <inheritdoc />
		public Pressure SecantModule => _calculator.SecantModule.ToUnit(StressUnit);

		/// <inheritdoc />
		public Pressure Strength
		{
			get => _strength;
			set
			{
				_strength            = value.ToUnit(StressUnit);
				_calculator.Strength = _strength;
			}
		}

		/// <inheritdoc />
		public PressureUnit StressUnit
		{
			get => Strength.Unit;
			set => ChangeUnit(value);
		}

		/// <inheritdoc />
		public Pressure TensileStrength => _calculator.TensileStrength.ToUnit(StressUnit);

		/// <inheritdoc />
		public Pressure TransverseModule => (ElasticModule / 2.4).ToUnit(StressUnit);

		/// <inheritdoc />
		public AggregateType Type
		{
			get => _calculator.Type;
			set => _calculator.Type = value;
		}

		/// <inheritdoc />
		public double UltimateStrain => _calculator.UltimateStrain;

		PressureUnit IUnitConvertible<PressureUnit>.Unit
		{
			get => StressUnit;
			set => StressUnit = value;
		}

		LengthUnit IUnitConvertible<LengthUnit>.Unit
		{
			get => DiameterUnit;
			set => DiameterUnit = value;
		}

		#endregion

		#region Constructors

		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType, bool)" />
		/// <param name="strengthUnit">The <see cref="PressureUnit" /> of <paramref name="strength" />.</param>
		/// <param name="diameterUnit">The <see cref="LengthUnit" /> of <paramref name="aggregateDiameter" />.</param>
		public Parameters(double strength, double aggregateDiameter, ParameterModel model = ParameterModel.Default, AggregateType type = AggregateType.Quartzite, bool considerConfinement = false, PressureUnit strengthUnit = PressureUnit.Megapascal, LengthUnit diameterUnit = LengthUnit.Millimeter)
			: this((Pressure) strength.As(strengthUnit), (Length) aggregateDiameter.As(diameterUnit), model, type, considerConfinement)
		{
		}

		/// <summary>
		///     Parameters constructor.
		/// </summary>
		/// <param name="type">The <see cref="AggregateType" />.</param>
		/// <param name="strength">Concrete compressive strength (positive value).</param>
		/// <param name="aggregateDiameter">The maximum diameter of concrete aggregate.</param>
		/// <param name="model">The <see cref="ParameterModel" />.</param>
		/// <param name="considerConfinement">
		///     Consider confinement strength of concrete? If set to true, concrete strength is
		///     increase in case of biaxial compression.
		/// </param>
		public Parameters(Pressure strength, Length aggregateDiameter, ParameterModel model = ParameterModel.Default, AggregateType type = AggregateType.Quartzite, bool considerConfinement = false)
		{
			_strength           = strength;
			_aggDiameter        = aggregateDiameter;
			_calculator         = ParameterCalculator.GetCalculator(strength, model, type);
			ConsiderConfinement = considerConfinement;
		}

		#endregion

		#region Methods

		/// <summary>
		///     Get concrete class C20 (fc = 20 MPa).
		/// </summary>
		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType, bool)" />
		public static Parameters C20(Length aggregateDiameter, ParameterModel model = ParameterModel.MC2010, AggregateType type = AggregateType.Quartzite, bool considerConfinement = false) =>
			new(Pressure.FromMegapascals(20), aggregateDiameter, model, type, considerConfinement);

		/// <summary>
		///     Get concrete class C30 (fc = 30 MPa).
		/// </summary>
		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType, bool)" />
		public static Parameters C30(Length aggregateDiameter, ParameterModel model = ParameterModel.MC2010, AggregateType type = AggregateType.Quartzite, bool considerConfinement = false) =>
			new(Pressure.FromMegapascals(30), aggregateDiameter, model, type, considerConfinement);

		/// <summary>
		///     Get concrete class C40 (fc = 40 MPa).
		/// </summary>
		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType, bool)" />
		public static Parameters C40(Length aggregateDiameter, ParameterModel model = ParameterModel.MC2010, AggregateType type = AggregateType.Quartzite, bool considerConfinement = false) =>
			new(Pressure.FromMegapascals(40), aggregateDiameter, model, type, considerConfinement);

		/// <summary>
		///     Get concrete class C50 (fc = 40 MPa).
		/// </summary>
		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType, bool)" />
		public static Parameters C50(Length aggregateDiameter, ParameterModel model = ParameterModel.MC2010, AggregateType type = AggregateType.Quartzite, bool considerConfinement = false) =>
			new(Pressure.FromMegapascals(50), aggregateDiameter, model, type, considerConfinement);

		/// <summary>
		///     Create a clone of this object with converted units.
		/// </summary>
		/// <param name="stressUnit">The desired <see cref="PressureUnit" />.</param>
		/// <param name="lengthUnit">The desired <see cref="LengthUnit" />.</param>
		public Parameters Convert(PressureUnit? stressUnit = null, LengthUnit? lengthUnit = null) =>
			new(stressUnit.HasValue ? Strength.ToUnit(stressUnit.Value) : Strength, lengthUnit.HasValue ? AggregateDiameter.ToUnit(lengthUnit.Value) : AggregateDiameter, Model, Type);


		/// <inheritdoc />
		public override bool Equals(object? obj) => obj is Parameters other && Equals(other);

		/// <inheritdoc />
		public override int GetHashCode() => (int) Strength.Megapascals * (int) AggregateDiameter.Millimeters;

		/// <summary>
		///     Get a <see cref="CustomParameters" /> from this object.
		/// </summary>
		public CustomParameters ToCustomParameters() => new(Strength, TensileStrength, ElasticModule, AggregateDiameter, PlasticStrain, UltimateStrain, ConsiderConfinement);

		/// <inheritdoc />
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

		/// <inheritdoc />
		public Parameters Clone() => new(Strength, AggregateDiameter, Model, Type);

		/// <inheritdoc />
		public bool Approaches(IConcreteParameters? other, Pressure tolerance) => Model == other?.Model && Strength.Approx(other.Strength, tolerance);

		/// <summary>
		///     Change <see cref="AggregateDiameter" /> unit.
		/// </summary>
		/// <param name="unit">The desired <see cref="LengthUnit" />.</param>
		public void ChangeUnit(LengthUnit unit)
		{
			if (unit == DiameterUnit)
				return;

			_aggDiameter = _aggDiameter.ToUnit(unit);
		}

		/// <summary>
		///     Change <see cref="Strength" /> unit.
		/// </summary>
		/// <param name="unit">The desired <see cref="LengthUnit" />.</param>
		public void ChangeUnit(PressureUnit unit)
		{
			if (unit == StressUnit)
				return;

			_strength = _strength.ToUnit(unit);
		}

		/// <remarks>
		///     <see cref="Strength" /> is compared.
		/// </remarks>
		/// <inheritdoc />
		public int CompareTo(IConcreteParameters? other) =>
			Strength == other?.Strength
				? 0
				: Strength > other?.Strength
					? 1
					: -1;

		/// <inheritdoc />
		public bool Equals(IConcreteParameters? other) => Approaches(other, Tolerance);

		/// <inheritdoc />
		bool IApproachable<IMaterialParameters, Pressure>.Approaches(IMaterialParameters other, Pressure tolerance) => other is IConcreteParameters parameters && Approaches(parameters, tolerance);

		/// <inheritdoc />
		int IComparable<IMaterialParameters>.CompareTo(IMaterialParameters other) => other is IConcreteParameters parameters
			? CompareTo(parameters)
			: 0;

		IUnitConvertible<LengthUnit> IUnitConvertible<LengthUnit>.Convert(LengthUnit unit) => Convert(lengthUnit: unit);

		IUnitConvertible<PressureUnit> IUnitConvertible<PressureUnit>.Convert(PressureUnit unit) => Convert(unit);

		/// <inheritdoc />
		bool IEquatable<IMaterialParameters>.Equals(IMaterialParameters other) => other is IConcreteParameters parameters && Equals(parameters);

		#endregion

		#region Operators

		/// <summary>
		///     Returns true if objects are equal.
		/// </summary>
		public static bool operator ==(Parameters left, IConcreteParameters right) => left.Equals(right);

		/// <summary>
		///     Returns true if objects are not equal.
		/// </summary>
		public static bool operator !=(Parameters left, IConcreteParameters right) => !left.Equals(right);

		#endregion

	}
}