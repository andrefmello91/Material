using System;
using andrefmello91.Extensions;
using UnitsNet;
using UnitsNet.Units;

namespace andrefmello91.Material.Concrete;

/// <summary>
///     Parameters calculated according to Modified Compression Field Theory.
/// </summary>
internal class Default : ParameterCalculator
{

	// Strains
	private const double ec = -0.002;
	private const double ecu = -0.0035;

	public override ParameterModel Model => ParameterModel.Default;

	/// <summary>
	///     Parameter calculator based on Classic DSFM formulation.
	/// </summary>
	/// <inheritdoc />
	internal Default(Pressure strength, AggregateType type = AggregateType.Quartzite) : base(strength, type)
	{
	}

	private static Pressure Ec(Pressure strength) => (Pressure) (4732.98 * strength.Megapascals.Sqrt()).As(PressureUnit.Megapascal);

	private static Pressure fcr(Pressure strength) => (Pressure) (0.65 * Math.Pow(strength.Megapascals, 1D / 3)).As(PressureUnit.Megapascal);

	protected override void CalculateCustomParameters()
	{
		TensileStrength = fcr(Strength);
		ElasticModule   = Ec(Strength);
		PlasticStrain   = ec;
		UltimateStrain  = ecu;
	}
}