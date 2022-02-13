using System;
using UnitsNet;

namespace andrefmello91.Material.Concrete;

/// <summary>
///     Parameter calculator base class.
/// </summary>
internal abstract class ParameterCalculator : IEquatable<ParameterCalculator>
{

	private Pressure _strength;
	private AggregateType _type;

	/// <summary>
	///     Get initial elastic module.
	/// </summary>
	public Pressure ElasticModule { get; protected set; }

	/// <summary>
	///     Get fracture parameter.
	/// </summary>
	public virtual ForcePerLength FractureParameter => ForcePerLength.FromNewtonsPerMillimeter(0.075);

	/// <summary>
	///     Get the <see cref="ParameterModel" />.
	/// </summary>
	public abstract ParameterModel Model { get; }

	/// <summary>
	///     Get concrete plastic (peak) strain (negative value).
	/// </summary>
	public double PlasticStrain { get; protected set; }

	/// <summary>
	///     Get secant module.
	/// </summary>
	public virtual Pressure SecantModule => Strength / PlasticStrain;

	/// <summary>
	///     Concrete strength.
	/// </summary>
	public Pressure Strength
	{
		get => _strength;
		set
		{
			_strength = value;
			CalculateCustomParameters();
		}
	}

	/// <summary>
	///     Get tensile strength.
	/// </summary>
	public Pressure TensileStrength { get; protected set; }

	/// <summary>
	///     The type of concrete aggregate.
	/// </summary>
	public AggregateType Type
	{
		get => _type;
		set
		{
			_type = value;
			CalculateCustomParameters();
		}
	}

	/// <summary>
	///     Get concrete ultimate strain (negative value).
	/// </summary>
	public double UltimateStrain { get; protected set; }

	/// <summary>
	///     Base parameter calculator class.
	/// </summary>
	/// <param name="strength">Concrete compressive strength (positive value).</param>
	/// <param name="type">The <see cref="AggregateType" />.</param>
	protected ParameterCalculator(Pressure strength, AggregateType type)
	{
		_strength = strength;
		_type     = type;
		CalculateCustomParameters();
	}

	/// <summary>
	///     Get the <see cref="ParameterCalculator" /> based on <see cref="ParameterModel" />.
	/// </summary>
	/// <param name="model">The <see cref="ParameterModel" />.</param>
	/// <inheritdoc cref="ParameterCalculator(Pressure, AggregateType)" />
	public static ParameterCalculator GetCalculator(Pressure strength, ParameterModel model, AggregateType type) =>
		model switch
		{
			ParameterModel.MC2010  => new MC2010(strength, type),
			ParameterModel.NBR6118 => new NBR6118(strength, type),
			_                      => new Default(strength, type)
		};

	/// <summary>
	///     Calculate and update values for custom parameters.
	/// </summary>
	protected abstract void CalculateCustomParameters();

	public bool Equals(ParameterCalculator? other) => other is not null && Model == other.Model;
}