﻿using andrefmello91.Extensions;
using UnitsNet;
using UnitsNet.Units;

namespace andrefmello91.Material.Concrete
{
	/// <summary>
	///     Concrete parameters struct.
	/// </summary>
	public struct CustomParameters : IParameters, ICloneable<CustomParameters>
	{
		private Length _aggDiameter;
		private Pressure _strength;
		private Pressure _tensileStrength;
		private Pressure _elasticModule;
		private double _plasticStrain;
		private double _ultimateStrain;

		#region Properties

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

		/// <inheritdoc />
		public PressureUnit StressUnit
		{
			get => Strength.Unit;
			set => ChangeUnit(value);
		}

		/// <inheritdoc />
		public LengthUnit DiameterUnit
		{
			get => AggregateDiameter.Unit;
			set => ChangeUnit(value);
		}

		/// <inheritdoc />
		public Pressure Strength
		{
			get => _strength;
			set => _strength = value.ToUnit(StressUnit);
		}

		/// <inheritdoc />
		public ParameterModel Model
		{
			get => ParameterModel.Custom;
			set
			{
			}
		}

		/// <inheritdoc />
		public AggregateType Type { get; set; }

		/// <inheritdoc />
		public Length AggregateDiameter
		{
			get => _aggDiameter;
			set => _aggDiameter = value.ToUnit(DiameterUnit);
		}

		/// <inheritdoc />
		public bool ConsiderConfinement { get; set; }

		/// <inheritdoc />
		public Pressure TensileStrength
		{
			get => _tensileStrength;
			set => _tensileStrength = value.ToUnit(StressUnit);
		}

		/// <inheritdoc />
		public Pressure ElasticModule
		{
			get => _elasticModule;
			set => _elasticModule = value.ToUnit(StressUnit);
		}

		/// <inheritdoc />
		public Pressure SecantModule => Strength / PlasticStrain;

		/// <inheritdoc />
		public double PlasticStrain
		{
			get => _plasticStrain;
			set => _plasticStrain = -value.Abs();
		}

		/// <inheritdoc />
		public double UltimateStrain
		{
			get => _ultimateStrain;
			set => _ultimateStrain = -value.Abs();
		}

		/// <inheritdoc />
		public double CrackingStrain => TensileStrength / ElasticModule;

		/// <inheritdoc />
		public Pressure TransverseModule => (ElasticModule / 2.4).ToUnit(StressUnit);

		/// <inheritdoc />
		public ForcePerLength FractureParameter => ForcePerLength.FromNewtonsPerMillimeter(0.075);

		#endregion

		#region Constructors

		/// <inheritdoc cref="CustomParameters(Pressure, Pressure, Pressure, Length, double, double, bool)" />
		public CustomParameters(double strength, double tensileStrength, double elasticModule, double aggregateDiameter, double plasticStrain = 0.002, double ultimateStrain = 0.0035, bool considerConfinement = false, PressureUnit strengthUnit = PressureUnit.Megapascal, LengthUnit diameterUnit = LengthUnit.Millimeter)
			: this((Pressure) strength.As(strengthUnit), (Pressure) tensileStrength.As(strengthUnit), (Pressure) elasticModule.As(strengthUnit), (Length) aggregateDiameter.As(diameterUnit), plasticStrain, ultimateStrain, considerConfinement)
		{
		}

		/// <summary>
		///     Custom parameters constructor.
		/// </summary>
		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType, bool)" />
		/// <param name="tensileStrength">Concrete tensile strength.</param>
		/// <param name="elasticModule">Concrete initial elastic module.</param>
		/// <param name="plasticStrain">Concrete plastic strain (positive or negative value).</param>
		/// <param name="ultimateStrain">Concrete ultimate strain (positive or negative value).</param>
		public CustomParameters(Pressure strength, Pressure tensileStrength, Pressure elasticModule, Length aggregateDiameter, double plasticStrain = 0.002, double ultimateStrain = 0.0035, bool considerConfinement = false)
		{
			_strength           = strength.Abs();
			_tensileStrength    = tensileStrength;
			_elasticModule      = elasticModule;
			_plasticStrain      = -plasticStrain.Abs();
			_ultimateStrain     = -ultimateStrain.Abs();
			_aggDiameter        = aggregateDiameter;
			Type                = AggregateType.Quartzite;
			ConsiderConfinement = considerConfinement;
		}

		#endregion

		#region Methods

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

			_strength        = _strength.ToUnit(unit);
			_tensileStrength = _tensileStrength.ToUnit(unit);
			_elasticModule   = _elasticModule.ToUnit(unit);
		}

		/// <summary>
		///     Create a clone of this object with converted units.
		/// </summary>
		/// <param name="stressUnit">The desired <see cref="PressureUnit" />.</param>
		/// <param name="lengthUnit">The desired <see cref="LengthUnit" />.</param>
		public CustomParameters Convert(PressureUnit? stressUnit = null, LengthUnit? lengthUnit = null) =>
			new(Strength.ToUnit(stressUnit ?? StressUnit), TensileStrength.ToUnit(stressUnit ?? StressUnit), ElasticModule.ToUnit(stressUnit ?? StressUnit), AggregateDiameter.ToUnit(lengthUnit ?? DiameterUnit), PlasticStrain, UltimateStrain);

		IUnitConvertible<LengthUnit> IUnitConvertible<LengthUnit>.Convert(LengthUnit unit) => Convert(lengthUnit: unit);

		IUnitConvertible<PressureUnit> IUnitConvertible<PressureUnit>.Convert(PressureUnit unit) => Convert(unit);

		/// <inheritdoc />
		public bool Approaches(IParameters? other, Pressure tolerance) => Model == other?.Model && Strength.Approx(other.Strength, tolerance);

		/// <inheritdoc />
		public CustomParameters Clone() => new(Strength, TensileStrength, ElasticModule, AggregateDiameter, PlasticStrain, UltimateStrain);

		/// <summary>
		///     Get a <see cref="Parameters" /> from this object.
		/// </summary>
		/// <param name="model">The required <see cref="ParameterModel" />. Not <see cref="ParameterModel.Custom" />.</param>
		/// <inheritdoc cref="Parameters(Pressure, Length, ParameterModel, AggregateType, bool)" />
		public Parameters ToParameters(ParameterModel model, AggregateType type = AggregateType.Quartzite) =>
			new(Strength, AggregateDiameter, model, type, ConsiderConfinement);

		/// <remarks>
		///     <see cref="Strength" /> is compared.
		/// </remarks>
		/// <inheritdoc />
		public int CompareTo(IParameters? other) =>
			Strength == other?.Strength
				? 0
				: Strength > other?.Strength
					? 1
					: -1;

		/// <inheritdoc />
		public bool Equals(IParameters? other) => Approaches(other, Parameters.Tolerance);

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
		public override bool Equals(object? obj) => obj is CustomParameters other && Equals(other);

		/// <inheritdoc />
		public override int GetHashCode() => (int) Strength.Megapascals * (int) AggregateDiameter.Millimeters;

		#endregion

		#region Operators

		public static bool operator ==(CustomParameters left, IParameters right) => left.Equals(right);

		public static bool operator !=(CustomParameters left, IParameters right) => !left.Equals(right);

		#endregion

	}
}