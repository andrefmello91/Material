using System;
using System.CodeDom;
using System.Runtime.Remoting;
using Extensions.Number;
using UnitsNet;
using UnitsNet.Units;

namespace Material.Concrete
{
	/// <summary>
	/// Model for calculating concrete parameters.
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
	/// Types of concrete aggregate.
	/// </summary>
	public enum AggregateType
	{
		Basalt,
		Quartzite,
		Limestone,
		Sandstone
	}

    /// <summary>
    ///Base class for implementation of concrete parameters.
    /// </summary>
    public abstract class Parameters : IEquatable<Parameters>
	{
		// Auxiliary fields
		protected Length _phiAg;
		protected Pressure _fc, _ft, _Eci, _Ecs;
		private AggregateType _type;

		/// <summary>
        /// Get the <see cref="PressureUnit"/> that this was constructed with.
        /// </summary>
		public PressureUnit Unit => _fc.Unit;

        /// <summary>
        /// Get the <see cref="LengthUnit"/> that this was constructed with.
        /// </summary>
        public LengthUnit AggUnit => _phiAg.Unit;

        /// <summary>
        /// Get <see cref="AggregateType"/>.
        /// </summary>
        public AggregateType Type
        {
	        get => _type;
	        protected set
	        {
		        _type = value;
		        UpdateParameters();
	        }
        }

		/// <summary>
		/// Get/set maximum diameter of aggregate, in mm.
		/// </summary>
		public double AggregateDiameter
		{
			get => _phiAg.Millimeters;
			set
			{
				_phiAg = Length.FromMillimeters(value).ToUnit(AggUnit);
				UpdateParameters();
			}
		}

		/// <summary>
		/// Get/set concrete compressive strength, in MPa (positive value).
		/// </summary>
		public double Strength
		{
			get => _fc.Megapascals;
			set
			{
				_fc = Pressure.FromMegapascals(value).ToUnit(Unit);
				UpdateParameters();
			}
        }

		/// <summary>
		/// Get/set concrete tensile strength, in MPa.
		/// </summary>
		public double TensileStrength
		{
			get => _ft.Megapascals;
			protected set => _ft = Pressure.FromMegapascals(value).ToUnit(Unit);
		}

		/// <summary>
		/// Get/set concrete initial elastic module, in MPa.
		/// </summary>
		public double InitialModule
		{
			get => _Eci.Megapascals;
			protected set => _Eci = Pressure.FromMegapascals(value).ToUnit(Unit);
		}

        /// <summary>
        /// Get/set concrete secant elastic module, at peak stress, in MPa.
        /// </summary>
        public double SecantModule
        {
	        get => _Ecs.Megapascals;
	        protected set => _Ecs = Pressure.FromMegapascals(value).ToUnit(Unit);
        }
		
        /// <summary>
        /// Get concrete plastic (peak) strain (negative value).
        /// </summary>
        public double PlasticStrain { get; protected set; }

        /// <summary>
        /// Get concrete ultimate strain (negative value).
        /// </summary>
        public double UltimateStrain { get; protected set; }

		/// <summary>
        /// Get concrete cracking strain.
        /// </summary>
		public double CrackStrain => _ft / _Eci;

		/// <summary>
        /// Get transverse (shear) module, in MPa.
        /// </summary>
		public double TransversalModule => SecantModule / 2.4;

		/// <summary>
        /// Get fracture parameter.
        /// </summary>
		public virtual double FractureParameter => 0.075;

		/// <summary>
        /// Returns true if strength is not zero.
        /// </summary>
		public bool IsSet => Strength > 0;

		/// <summary>
		/// Get Poisson coefficient.
		/// </summary>
		public const double Poisson = 0.2;

        /// <summary>
        /// Base object of concrete parameters.
        /// </summary>
        /// <param name="strength">Concrete compressive strength, in MPa.</param>
        /// <param name="aggregateDiameter">Maximum aggregate diameter, in mm.</param>
        /// <param name="aggregateType">The type of aggregate.</param>
        protected Parameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite)
			: this (Pressure.FromMegapascals(strength), Length.FromMillimeters(aggregateDiameter), aggregateType)
		{
		}

		/// <summary>
		/// Base object of concrete parameters.
		/// </summary>
		/// <param name="strength">Concrete compressive strength..</param>
		/// <param name="aggregateDiameter">Maximum aggregate diameter.</param>
		/// <param name="aggregateType">The type of aggregate.</param>
		protected Parameters(Pressure strength, Length aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite)
		{
			_fc     = strength;
			_phiAg  = aggregateDiameter;
			Type    = aggregateType;
			UpdateParameters();
		}

        /// <summary>
        /// Get concrete parameters based on the enum type (<see cref="ParameterModel"/>).
        /// </summary>
        /// <param name="parameterModel">Model of concrete parameters.</param>
        /// <param name="strength">Concrete compressive strength, in MPa.</param>
        /// <param name="aggregateDiameter">Maximum aggregate diameter, in mm.</param>
        /// <param name="aggregateType">The type of aggregate.</param>
        /// <param name="tensileStrength">Concrete tensile strength, in MPa (only for custom parameters).</param>
        /// <param name="elasticModule">Concrete initial elastic module, in MPa (only for custom parameters).</param>
        /// <param name="plasticStrain">Concrete peak strain (negative value) (only for custom parameters).</param>
        /// <param name="ultimateStrain">Concrete ultimate strain (negative value) (only for custom parameters).</param>
        public static Parameters ReadParameters(ParameterModel parameterModel, double strength, double aggregateDiameter, AggregateType aggregateType, double tensileStrength = 0, double elasticModule = 0, double plasticStrain = 0, double ultimateStrain = 0)
        {
            switch (parameterModel)
			{
				case ParameterModel.MC2010:
					return new MC2010Parameters(strength, aggregateDiameter, aggregateType);

				case ParameterModel.NBR6118:
					return new NBR6118Parameters(strength, aggregateDiameter, aggregateType);

				case ParameterModel.MCFT:
					return new MCFTParameters(strength, aggregateDiameter, aggregateType);

				case ParameterModel.DSFM:
					return new DSFMParameters(strength, aggregateDiameter, aggregateType);

				default:
					return new CustomParameters(strength, aggregateDiameter, tensileStrength, elasticModule, plasticStrain, ultimateStrain);
            }
		}

		/// <summary>
        /// Get the enumeration based on parameter object.
        /// </summary>
        /// <param name="parameters">Parameters object.</param>
        /// <returns></returns>
        public static ParameterModel ReadParameterModel(Parameters parameters)
        {
	        switch (parameters)
	        {
		        case NBR6118Parameters _ :
			        return ParameterModel.NBR6118;

		        case MC2010Parameters _ :
			        return ParameterModel.MC2010;

		        case MCFTParameters _ :
			        return ParameterModel.MCFT;

		        case DSFMParameters _ :
			        return ParameterModel.DSFM;

		        default:
			        return ParameterModel.Custom;
	        }
        }

        /// <summary>
        /// Recalculate parameters based on compressive strength.
        /// </summary>
        public abstract void UpdateParameters();

		public override string ToString()
		{
			char
				phi = (char)Characters.Phi,
				eps = (char)Characters.Epsilon;

			return
				"Concrete Parameters:\n\n" +
				$"fc = {_fc}\n"  +
				$"ft = {_ft}\n"  +
				$"Ec = {_Eci}\n" + 
				$"{eps}c = {PlasticStrain:0.##E+00}\n"   +
				$"{eps}cu = {UltimateStrain:0.##E+00}\n" +
                $"{phi},ag = {_phiAg}";
		}

		/// <summary>
		/// Compare two parameter objects.
		/// </summary>
		/// <param name="other">The other parameter object.</param>
		public virtual bool Equals(Parameters other) => !(other is null) && Strength == other.Strength && AggregateDiameter == other.AggregateDiameter && Type == other.Type;

		public override bool Equals(object obj) => obj is Parameters other && Equals(other);

		public override int GetHashCode() => (int) Strength.Pow(AggregateDiameter);

		/// <summary>
		/// Returns true if parameters are equal.
		/// </summary>
		public static bool operator == (Parameters left, Parameters right) => !(left is null) && left.Equals(right);

		/// <summary>
		/// Returns true if parameters are different.
		/// </summary>
		public static bool operator != (Parameters left, Parameters right) => !(left is null) && !left.Equals(right);
	}
}