using System;
using Extensions;
using OnPlaneComponents;
using UnitsNet;
using UnitsNet.Units;

namespace Material.Concrete
{
	/// <summary>
	///     Model for calculating concrete parameters.
	/// </summary>
	public enum ParameterModel
	{
		NBR6118,
		MC2010,
		MCFT,
		DSFM,
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
	public interface IParameters : IUnitConvertible<IParameters, PressureUnit>, IUnitConvertible<IParameters, LengthUnit>, IApproachable<IParameters, Pressure>, IEquatable<IParameters>, IComparable<IParameters>
	{
		#region Properties

		/// <summary>
		///		Get the aggregate type.
		/// </summary>
		public AggregateType Type { get; set; }

		/// <summary>
		///     Get/set maximum diameter of concrete aggregate.
		/// </summary>
		Length AggregateDiameter { get; set; }

		/// <summary>
		///     Get concrete cracking strain.
		/// </summary>
		double CrackingStrain { get; }

		/// <summary>
		///     Get/set the unit of concrete aggregate diameter.
		/// </summary>
		LengthUnit DiameterUnit { get; set; }

		/// <summary>
		///     Get initial elastic module.
		/// </summary>
		Pressure ElasticModule { get; }

		/// <summary>
		///     Get fracture parameter.
		/// </summary>
		ForcePerLength FractureParameter { get; }

		/// <summary>
		///     Get the <see cref="ParameterModel" />.
		/// </summary>
		ParameterModel Model { get; }

		/// <summary>
		///     Get concrete plastic (peak) strain (negative value).
		/// </summary>
		double PlasticStrain { get; }

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
		///     Get tensile strength.
		/// </summary>
		Pressure TensileStrength { get; }

		/// <summary>
		///     Get transverse (shear) module.
		/// </summary>
		Pressure TransverseModule { get; }

		/// <summary>
		///     Get concrete ultimate strain (negative value).
		/// </summary>
		double UltimateStrain { get; }

		#endregion
	}
}