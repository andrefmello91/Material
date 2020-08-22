using System;
using System.CodeDom;
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
		public AggregateType Type              { get; set; }
		public double        AggregateDiameter { get; set; }
		public double        Strength          { get; set; }
		public double        Poisson           { get; }
		public double        TensileStrength   { get; set; }
		public double        InitialModule     { get; set; }
		public double        SecantModule      { get; set; }
		public double        PlasticStrain     { get; set; }
		public double        UltimateStrain    { get; set; }

		// Automatic calculated properties
		public double         CrackStrain       => TensileStrength / InitialModule;
		public double         TransversalModule => SecantModule / 2.4;
		public virtual double FractureParameter => 0.075;

		// Verify if concrete was set
		public bool IsSet => Strength > 0;

		/// <summary>
		/// Base object of concrete parameters.
		/// </summary>
		/// <param name="strength">Concrete compressive strength in MPa.</param>
		/// <param name="aggregateDiameter">Maximum aggregate diameter in mm.</param>
		/// <param name="aggregateType">The type of aggregate.</param>
		public Parameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite)
		{
			Strength          = strength;
			AggregateDiameter = aggregateDiameter;
			Type              = aggregateType;
			Poisson           = 0.2;
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
			}

			// Custom parameters
			return new CustomParameters(strength, aggregateDiameter, tensileStrength, elasticModule, plasticStrain, ultimateStrain);
		}

		/// <summary>
        /// Get the enumeration based on parameter object.
        /// </summary>
        /// <param name="parameters">Parameters object.</param>
        /// <returns></returns>
        public static ParameterModel ReadParameterModel(Parameters parameters)
        {
	        if (parameters is NBR6118Parameters)
		        return ParameterModel.NBR6118;

	        if (parameters is MC2010Parameters)
		        return ParameterModel.MC2010;

	        if (parameters is MCFTParameters)
		        return ParameterModel.MCFT;

	        if (parameters is DSFMParameters)
		        return ParameterModel.DSFM;

	        return ParameterModel.Custom;
        }

        /// <summary>
        /// Recalculate parameters based on compressive strength.
        /// </summary>
        public abstract void UpdateParameters();

		/// <summary>
		/// Write string with default units (MPa and mm).
		/// </summary>
		public override string ToString() => ToString();
	        
		/// <summary>
		/// Write string with custom units.
		/// </summary>
		/// <param name="strengthUnit">The stress unit for strength (default: MPa)</param>
		/// <param name="aggregateUnit">The aggregate dimension unit (default: mm)</param>
		/// <returns>String with custom units</returns>
		public string ToString(PressureUnit strengthUnit = PressureUnit.Megapascal, LengthUnit aggregateUnit = LengthUnit.Millimeter)
		{
			IQuantity
				fc    = Pressure.FromMegapascals(Strength).ToUnit(strengthUnit),
				ft    = Pressure.FromMegapascals(TensileStrength).ToUnit(strengthUnit),
				Ec    = Pressure.FromMegapascals(InitialModule).ToUnit(strengthUnit),
				phiAg = Length.FromMillimeters(AggregateDiameter).ToUnit(aggregateUnit);

			char
				phi = (char)Characters.Phi,
				eps = (char)Characters.Epsilon;

			return
				"Concrete Parameters:\n" +
				"\nfc = " + fc +
				"\nft = " + ft +
				"\nEc = " + Ec +
				"\n" + eps + "c = "   + Math.Round(1000 * PlasticStrain, 2)  + " E-03" +
				"\n" + eps + "cu = "  + Math.Round(1000 * UltimateStrain, 2) + " E-03" +
				"\n" + phi + ",ag = " + phiAg;
		}

		/// <summary>
		/// Compare two parameter objects.
		/// </summary>
		/// <param name="other">The other parameter object.</param>
		public virtual bool Equals(Parameters other) => other != null && Strength == other.Strength && AggregateDiameter == other.AggregateDiameter && Type == other.Type;

		public override int GetHashCode() => (int) Math.Pow(Strength, AggregateDiameter);

		/// <summary>
		/// Returns true if parameters are equal.
		/// </summary>
		public static bool operator == (Parameters left, Parameters right) => left != null && left.Equals(right);

		/// <summary>
		/// Returns true if parameters are different.
		/// </summary>
		public static bool operator != (Parameters left, Parameters right) => left != null && !left.Equals(right);
	}
}