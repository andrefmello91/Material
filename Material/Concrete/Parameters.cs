using System;
using System.CodeDom;
using System.Linq;
using MathNet.Numerics.Interpolation;
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

    /// <summary>
    /// Parameters calculated according to NBR6118:2014.
    /// </summary>
    public class NBR6118Parameters : Parameters
    {
        /// <inheritdoc/>
        /// <summary>
        /// Parameters based on NBR 6118:2014.
        /// </summary>
        public NBR6118Parameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite) : base(strength, aggregateDiameter, aggregateType)
        {
            UpdateParameters();
        }

        private double AlphaE()
        {
            switch (Type)
            {
                case AggregateType.Basalt:
                    return 1.2;

                case AggregateType.Quartzite:
                    return 1;

                case AggregateType.Limestone:
                    return 0.9;
            }

            // Sandstone
            return 0.7;
        }

        private double AlphaI() => Math.Min(0.8 + 0.2 * Strength / 80, 1);

        private double fctm()
        {
            if (Strength <= 50)
                return
                    0.3 * Math.Pow(Strength, 0.66666667);
            //else
            return
                2.12 * Math.Log(1 + 0.11 * Strength);
        }

        private double Eci()
        {
            if (Strength <= 50)
                return
                    AlphaE() * 5600 * Math.Sqrt(Strength);

            return
                21500 * AlphaE() * Math.Pow((0.1 * Strength + 1.25), 0.333333);
        }

        private double Ecs() => AlphaI() * InitialModule;

        private double ec2()
        {
            if (Strength <= 50)
                return
                    -0.002;

            return
                -0.002 - 0.000085 * Math.Pow(Strength - 50, 0.53);
        }

        private double ecu()
        {
            if (Strength <= 50)
                return
                    -0.0035;

            return
                -0.0026 - 0.035 * Math.Pow(0.01 * (90 - Strength), 4);
        }

        ///<inheritdoc/>
        public override void UpdateParameters()
        {
            TensileStrength = fctm();
            PlasticStrain = ec2();
            InitialModule = Eci();
            SecantModule = Ecs();
            UltimateStrain = ecu();
        }

		/// <inheritdoc/>
        public override bool Equals(Parameters other)
        {
	        if (other != null && other is NBR6118Parameters)
		        return base.Equals(other);

	        return false;
        }

        public override bool Equals(object obj)
        {
	        if (obj != null && obj is NBR6118Parameters other)
		        return base.Equals(other);

	        return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// Parameters calculated according to FIB Model Code 2010.
    /// </summary>
    public class MC2010Parameters : Parameters
    {
        ///<inheritdoc/>
        /// <summary>
        /// Parameters based on fib Model Code 2010.
        /// </summary>
        public MC2010Parameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite) : base(strength, aggregateDiameter, aggregateType)
        {
            UpdateParameters();
        }

        // Parameter calculation using MC2010 nomenclature
        private double AlphaE()
        {
            switch (Type)
            {
                case AggregateType.Basalt:
                    return 1.2;

                case AggregateType.Quartzite:
                    return 1;
            }

            // Limestone or sandstone
            return 0.9;
        }

        private double fctm()
        {
            if (Strength <= 50)
                return
                    0.3 * Math.Pow(Strength, 0.66666667);
            //else
            return
                2.12 * Math.Log(1 + 0.1 * Strength);
        }

        private double Eci() => 21500 * AlphaE() * Math.Pow(Strength / 10, 0.33333333);
        private double ec1() => -1.6 / 1000 * Math.Pow(Strength / 10, 0.25);
        private double Ec1() => Strength / ec1();
        private double k() => Eci() / Ec1();

        private double ecu()
        {
            // Verify fcm
            if (Strength < 50)
                return
                    -0.0035;

            if (Strength >= 90)
                return
                    -0.003;

            // Get classes and ultimate strains
            if (classes.Contains(Strength))
            {
                int i = Array.IndexOf(classes, Strength);

                return
                    ultimateStrain[i];
            }

            // Interpolate values
            return
                UltimateStrainSpline().Interpolate(Strength);
        }

        public override double FractureParameter => 0.073 * Math.Pow(Strength, 0.18);

        /// <summary>
        /// Array of high strength concrete classes, C50 to C90 (MC2010).
        /// </summary>
        private readonly double[] classes =
        {
            50, 55, 60, 70, 80, 90
        };

        /// <summary>
        /// Array of ultimate strains for each concrete class, C50 to C90 (MC2010).
        /// </summary>
        private readonly double[] ultimateStrain =
        {
            -0.0034, -0.0034, -0.0033, -0.0032, -0.0031, -0.003
        };

        /// <summary>
        /// Interpolation for ultimate strains.
        /// </summary>
        private CubicSpline UltimateStrainSpline() => CubicSpline.InterpolateAkimaSorted(classes, ultimateStrain);

        ///<inheritdoc/>
        public override void UpdateParameters()
        {
            TensileStrength = fctm();
            PlasticStrain = ec1();
            InitialModule = Eci();
            SecantModule = Ec1();
            UltimateStrain = ecu();
        }

        /// <inheritdoc/>
        public override bool Equals(Parameters other)
        {
	        if (other != null && other is MC2010Parameters)
		        return base.Equals(other);

	        return false;
        }

        public override bool Equals(object obj)
        {
	        if (obj != null && obj is MC2010Parameters other)
		        return base.Equals(other);

	        return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// Parameters calculated according to Modified Compression Field Theory.
    /// </summary>
    public class MCFTParameters : Parameters
	{
		/// <inheritdoc/>
		/// <summary>
		/// Parameters based on Classic MCFT formulation.
		/// </summary>
		public MCFTParameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite) : base(strength, aggregateDiameter, aggregateType)
		{
			UpdateParameters();
		}

		private double fcr() => 0.33 * Math.Sqrt(Strength);
		private double ec = -0.002;
		private double ecu = -0.0035;
		private double Ec() => -2 * Strength / ec;

		///<inheritdoc/>
		public override void UpdateParameters()
		{
			TensileStrength = fcr();
			PlasticStrain   = ec;
			InitialModule   = Ec();
			UltimateStrain  = ecu;
		}

		/// <inheritdoc/>
		public override bool Equals(Parameters other)
		{
			if (other != null && other is MCFTParameters)
				return base.Equals(other);

			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is MCFTParameters other)
				return base.Equals(other);

			return false;
		}

		public override int GetHashCode() => base.GetHashCode();
	}

    /// <summary>
    /// Parameters calculated according to Disturbed Stress Field Model.
    /// </summary>
    public class DSFMParameters : Parameters
	{
		/// <inheritdoc/>
		/// <summary>
		/// Parameters based on DSFM formulation.
		/// </summary>
		public DSFMParameters(double strength, double aggregateDiameter, AggregateType aggregateType = AggregateType.Quartzite) : base(strength, aggregateDiameter, aggregateType)
		{
			UpdateParameters();
		}

		private double fcr() => 0.33 * Math.Sqrt(Strength);
		//private double fcr() => 0.65 * Math.Pow(Strength, 0.33);
		private double ec    = -0.002;
		private double ecu   = -0.0035;
		private double Ec()  => -2 * Strength / ec;

		///<inheritdoc/>
		public override void UpdateParameters()
		{
			TensileStrength = fcr();
			PlasticStrain   = ec;
			InitialModule   = Ec();
			UltimateStrain  = ecu;
		}

		/// <inheritdoc/>
		public override bool Equals(Parameters other)
		{
			if (other != null && other is DSFMParameters)
				return base.Equals(other);

			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is DSFMParameters other)
				return base.Equals(other);

			return false;
		}

		public override int GetHashCode() => base.GetHashCode();
	}

    /// <summary>
    /// Custom concrete parameters.
    /// </summary>
    public class CustomParameters : Parameters
    {
	    /// <inheritdoc/>
	    /// <summary>
	    /// Create custom concrete parameters
	    /// </summary>
	    /// <param name="strength">Concrete compressive strength in MPa.</param>
	    /// <param name="aggregateDiameter">Maximum aggregate diameter in mm.</param>
	    /// <param name="tensileStrength">Concrete tensile strength in MPa.</param>
	    /// <param name="elasticModule">Concrete initial elastic module in MPa.</param>
	    /// <param name="plasticStrain">Concrete peak strain (negative value).</param>
	    /// <param name="ultimateStrain">Concrete ultimate strain (negative value).</param>
	    public CustomParameters(double strength, double aggregateDiameter, double tensileStrength, double elasticModule, double plasticStrain, double ultimateStrain) : base(strength, aggregateDiameter)
	    {
		    TensileStrength = tensileStrength;
		    InitialModule   = elasticModule;
		    PlasticStrain   = plasticStrain;
		    UltimateStrain  = ultimateStrain;
	    }

	    ///<inheritdoc/>
	    public override void UpdateParameters()
	    {
	    }

	    /// <inheritdoc/>
	    public override bool Equals(Parameters other)
	    {
		    if (other != null && other is CustomParameters)
			    return 
				    base.Equals(other) && TensileStrength == other.TensileStrength && InitialModule == other.InitialModule && 
				    PlasticStrain == other.PlasticStrain && UltimateStrain == other.UltimateStrain;

		    return false;
	    }

	    public override bool Equals(object obj)
	    {
		    if (obj != null && obj is CustomParameters other)
			    return
				    base.Equals(other) && TensileStrength == other.TensileStrength && InitialModule == other.InitialModule &&
				    PlasticStrain == other.PlasticStrain && UltimateStrain == other.UltimateStrain;

		    return false;
	    }

	    public override int GetHashCode() => base.GetHashCode();
    }

}