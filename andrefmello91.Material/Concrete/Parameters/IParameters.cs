﻿using System;
using andrefmello91.Extensions;
using UnitsNet;
using UnitsNet.Units;

namespace andrefmello91.Material.Concrete;

/// <summary>
///     Model for calculating concrete parameters.
/// </summary>
public enum ParameterModel
{
	/// <summary>
	///     Default parameter model.
	/// </summary>
	Default,

	/// <summary>
	///     Model based in brazilian standard (NBR6118:2014) formulation.
	/// </summary>
	NBR6118,

	/// <summary>
	///     Model based in FIB Model Code 2010 formulation.
	/// </summary>
	MC2010,

	/// <summary>
	///     Custom parameter model.
	/// </summary>
	/// <seealso cref="CustomParameters" />
	Custom
}

/// <summary>
///     Types of concrete aggregate.
/// </summary>
public enum AggregateType
{
	Basalt,
	Quartzite,
	Limestone,
	Sandstone
}

/// <summary>
///     Interface for concrete parameters.
/// </summary>
public interface IConcreteParameters : IMaterialParameters, IUnitConvertible<LengthUnit>, IApproachable<IConcreteParameters, Pressure>, IEquatable<IConcreteParameters>, IComparable<IConcreteParameters>
{

	#region Properties

	/// <summary>
	///     Get/set maximum diameter of concrete aggregate.
	/// </summary>
	Length AggregateDiameter { get; set; }

	/// <summary>
	///     Get/set confinement compressive strength consideration.
	/// </summary>
	/// <remarks>
	///     If set to true, concrete strength is increase in case of biaxial compression.
	/// </remarks>
	bool ConsiderConfinement { get; set; }

	/// <summary>
	///     Get concrete cracking strain.
	/// </summary>
	double CrackingStrain { get; }

	/// <summary>
	///     Get/set the unit of concrete aggregate diameter.
	/// </summary>
	LengthUnit DiameterUnit { get; set; }

	/// <summary>
	///     Get fracture parameter.
	/// </summary>
	ForcePerLength FractureParameter { get; }

	/// <summary>
	///     Get the <see cref="ParameterModel" />.
	/// </summary>
	ParameterModel Model { get; set; }

	/// <summary>
	///     Get secant module.
	/// </summary>
	Pressure SecantModule { get; }

	/// <summary>
	///     Get the compressive strength of concrete (positive value).
	/// </summary>
	Pressure Strength { get; set; }

	/// <summary>
	///     Get/set the unit of concrete strength parameters.
	/// </summary>
	PressureUnit StressUnit { get; set; }

	/// <summary>
	///     Get transverse (shear) module.
	/// </summary>
	Pressure TransverseModule { get; }

	/// <summary>
	///     Get the aggregate type.
	/// </summary>
	public AggregateType Type { get; set; }

	#endregion

}