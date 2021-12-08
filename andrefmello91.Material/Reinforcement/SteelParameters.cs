using System;
using andrefmello91.Extensions;
using UnitsNet;
using UnitsNet.Units;

namespace andrefmello91.Material.Reinforcement;

/// <summary>
///     Steel parameters struct.
/// </summary>
public struct SteelParameters : IMaterialParameters, IApproachable<SteelParameters, Pressure>, IEquatable<SteelParameters>, IComparable<SteelParameters>, ICloneable<SteelParameters>
{

	#region Fields

	/// <summary>
	///     The default <see cref="Pressure" /> tolerance.
	/// </summary>
	public static readonly Pressure Tolerance = Pressure.FromPascals(1E-3);

	#endregion

	#region Properties

	/// <summary>
	///     Steel hardening consideration.
	/// </summary>
	public bool ConsiderHardening { get; }

	/// <summary>
	///     Get hardening module.
	/// </summary>
	public Pressure HardeningModule { get; private set; }

	/// <summary>
	///     Get hardening strain.
	/// </summary>
	public double HardeningStrain { get; }

	/// <summary>
	///     Get yield strain.
	/// </summary>
	public double YieldStrain => YieldStress / ElasticModule;

	/// <summary>
	///     Get yield stress.
	/// </summary>
	public Pressure YieldStress { get; private set; }

	/// <inheritdoc />
	public Pressure ElasticModule { get; private set; }

	/// <inheritdoc />
	public double UltimateStrain { get; }

	/// <inheritdoc />
	public PressureUnit Unit
	{
		get => YieldStress.Unit;
		set => ChangeUnit(value);
	}

	/// <inheritdoc />
	Pressure IMaterialParameters.CompressiveStrength => -YieldStress;

	/// <inheritdoc />
	double IMaterialParameters.PlasticStrain => YieldStrain;

	/// <inheritdoc />
	Pressure IMaterialParameters.TensileStrength => YieldStress;

	#endregion

	#region Constructors

	/// <summary>
	///     Create steel parameters without hardening consideration.
	/// </summary>
	/// <param name="yieldStress">Steel yield stress.</param>
	/// <param name="elasticModule">Steel elastic module.</param>
	/// <param name="ultimateStrain">Steel ultimate strain.</param>
	public SteelParameters(Pressure yieldStress, Pressure elasticModule, double ultimateStrain = 0.01)
		: this(yieldStress, elasticModule, ultimateStrain, false, Pressure.Zero, 0)
	{
		YieldStress    = yieldStress;
		ElasticModule  = elasticModule.ToUnit(yieldStress.Unit);
		UltimateStrain = ultimateStrain;
	}

	/// <inheritdoc cref="SteelParameters(Pressure, Pressure, double)" />
	/// <param name="unit">The unit of <paramref name="yieldStress" /> and <paramref name="elasticModule" />.</param>
	public SteelParameters(double yieldStress, double elasticModule = 210000, double ultimateStrain = 0.01, PressureUnit unit = PressureUnit.Megapascal)
		: this((Pressure) yieldStress.As(unit), (Pressure) elasticModule.As(unit), ultimateStrain)
	{
	}

	/// <summary>
	///     Create steel parameters with hardening consideration.
	/// </summary>
	/// <inheritdoc cref="SteelParameters(Pressure, Pressure, double)" />
	/// <param name="hardeningModule">Steel hardening module.</param>
	/// <param name="hardeningStrain">Steel strain at the beginning of hardening.</param>
	public SteelParameters(Pressure yieldStress, Pressure elasticModule, Pressure hardeningModule, double hardeningStrain, double ultimateStrain = 0.01)
		: this(yieldStress, elasticModule, ultimateStrain, true, hardeningModule, hardeningStrain)
	{
	}

	/// <inheritdoc cref="SteelParameters(Pressure, Pressure, Pressure, double, double)" />
	private SteelParameters(Pressure yieldStress, Pressure elasticModule, double ultimateStrain, bool considerHardening, Pressure hardeningModule, double hardeningStrain)
	{
		ConsiderHardening = considerHardening;
		YieldStress       = yieldStress;
		ElasticModule     = elasticModule;
		UltimateStrain    = ultimateStrain;
		HardeningModule   = hardeningModule;
		HardeningStrain   = hardeningStrain;
	}

	#endregion

	#region Methods

	/// <inheritdoc cref="IUnitConvertible{TUnit}.Convert" />
	public SteelParameters Convert(PressureUnit unit)
	{
		var param = Clone();

		if (Unit != unit)
			param.ChangeUnit(unit);

		return param;
	}

	/// <inheritdoc />
	public override bool Equals(object? other) => other is SteelParameters parameters && Equals(parameters);

	/// <inheritdoc />
	public override int GetHashCode() => (int) ElasticModule.Gigapascals * (int) YieldStress.Megapascals;

	/// <inheritdoc />
	public override string ToString()
	{
		var epsilon = (char) Characters.Epsilon;

		var msg =
			"Steel Parameters:\n" +
			$"fy = {YieldStress}\n" +
			$"Es = {ElasticModule}\n" +
			$"{epsilon}y = {YieldStrain:0.##E+00}";

		if (ConsiderHardening)
			msg += "\n\n" +
			       "Hardening parameters:\n" +
			       $"Es = {HardeningModule}\n" +
			       $"{epsilon}y = {HardeningStrain:0.##E+00}";

		return msg;
	}

	/// <inheritdoc />
	public bool Approaches(SteelParameters other, Pressure tolerance)
	{
		var basic = YieldStress.Approx(other.YieldStress, tolerance) && ElasticModule.Approx(other.ElasticModule, tolerance) && UltimateStrain.Approx(other.UltimateStrain);

		if (!other.ConsiderHardening)
			return basic;

		return basic && HardeningModule.Approx(other.HardeningModule, tolerance) && HardeningStrain.Approx(other.HardeningStrain);
	}


	/// <inheritdoc />
	public SteelParameters Clone() => new(YieldStress, ElasticModule, UltimateStrain, ConsiderHardening, HardeningModule, HardeningStrain);

	/// <inheritdoc />
	public int CompareTo(SteelParameters other) =>
		YieldStress > other.YieldStress || YieldStress.Approx(other.YieldStress, Tolerance) && ElasticModule > other.ElasticModule
			? 1
			: YieldStress.Approx(other.YieldStress, Tolerance) && ElasticModule.Approx(other.ElasticModule, Tolerance)
				? 0
				: -1;

	/// <inheritdoc />
	public bool Equals(SteelParameters other) => Approaches(other, Tolerance);

	/// <inheritdoc />
	public bool Approaches(IMaterialParameters other, Pressure tolerance) => other is SteelParameters parameters && Approaches(parameters, tolerance);

	/// <inheritdoc />
	public void ChangeUnit(PressureUnit unit)
	{
		if (Unit == unit)
			return;

		YieldStress     = YieldStress.ToUnit(unit);
		ElasticModule   = ElasticModule.ToUnit(unit);
		HardeningModule = HardeningModule.ToUnit(unit);
	}

	/// <inheritdoc />
	public int CompareTo(IMaterialParameters other) => other is SteelParameters parameters
		? CompareTo(parameters)
		: 0;

	/// <inheritdoc />
	public bool Equals(IMaterialParameters other) => other is SteelParameters parameters && Equals(parameters);

	IUnitConvertible<PressureUnit> IUnitConvertible<PressureUnit>.Convert(PressureUnit unit) => Convert(unit);

	#endregion

	#region Operators

	/// <summary>
	///     Returns true if steel parameters are equal.
	/// </summary>
	public static bool operator ==(SteelParameters left, SteelParameters right) => left.IsEqualTo(right);

	/// <summary>
	///     Create a steel from this parameters.
	/// </summary>
	public static implicit operator Steel(SteelParameters parameters) => new(parameters);

	/// <summary>
	///     Get this steel's parameters.
	/// </summary>
	public static implicit operator SteelParameters?(Steel? steel) => steel?.Parameters;

	/// <summary>
	///     Returns true if steel parameters are different.
	/// </summary>
	public static bool operator !=(SteelParameters left, SteelParameters right) => left.IsNotEqualTo(right);

	#endregion

}