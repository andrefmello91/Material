using System;
using andrefmello91.Extensions;
using UnitsNet;
using UnitsNet.Units;

namespace andrefmello91.Material.Concrete
{
	/// <summary>
	///     Model for calculating concrete parameters.
	/// </summary>
	public enum ParameterModel
	{
		Default,
		NBR6118,
		MC2010,
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
	public interface IParameters : IUnitConvertible<PressureUnit>, IUnitConvertible<LengthUnit>, IApproachable<IParameters, Pressure>, IEquatable<IParameters>, IComparable<IParameters>
	{

		#region Properties

		/// <summary>
		///     Get/set maximum diameter of concrete aggregate.
		/// </summary>
		Length AggregateDiameter { get; set; }

		/// <summary>
		///		Get/set confinement compressive strength consideration.
		/// </summary>
		/// <remarks>
		///		If set to true, concrete strength is increase in case of biaxial compression.
		/// </remarks>
		public bool ConsiderConfinement { get; set; }
		
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
		ParameterModel Model { get; set; }

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
		///     Get the aggregate type.
		/// </summary>
		public AggregateType Type { get; set; }

		/// <summary>
		///     Get concrete ultimate strain (negative value).
		/// </summary>
		double UltimateStrain { get; }

		#endregion

	}
}