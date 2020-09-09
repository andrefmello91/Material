using UnitsNet;

namespace Material.Concrete
{
	/// <summary>
	/// Custom concrete parameters.
	/// </summary>
	public class CustomParameters : Parameters
	{
		/// <summary>
		/// Create custom concrete parameters.
		/// </summary>
		/// <param name="strength">Concrete compressive strength, in MPa.</param>
		/// <param name="aggregateDiameter">Maximum aggregate diameter, in mm.</param>
		/// <param name="tensileStrength">Concrete tensile strength, in MPa.</param>
		/// <param name="elasticModule">Concrete initial elastic module, in MPa.</param>
		/// <param name="plasticStrain">Concrete peak strain (negative value).</param>
		/// <param name="ultimateStrain">Concrete ultimate strain (negative value).</param>
		public CustomParameters(double strength, double aggregateDiameter, double tensileStrength, double elasticModule, double plasticStrain, double ultimateStrain)
			: this (Pressure.FromMegapascals(strength), Length.FromMillimeters(aggregateDiameter), Pressure.FromMegapascals(tensileStrength), Pressure.FromMegapascals(elasticModule), plasticStrain, ultimateStrain)
		{
		}

        /// <summary>
        /// Create custom concrete parameters.
		/// </summary>
        /// <param name="strength">Concrete compressive strength.</param>
        /// <param name="aggregateDiameter">Maximum aggregate diameter.</param>
        /// <param name="tensileStrength">Concrete tensile strength.</param>
        /// <param name="elasticModule">Concrete initial elastic module.</param>
        /// <param name="plasticStrain">Concrete peak strain (negative value).</param>
        /// <param name="ultimateStrain">Concrete ultimate strain (negative value).</param>
        public CustomParameters(Pressure strength, Length aggregateDiameter, Pressure tensileStrength, Pressure elasticModule, double plasticStrain, double ultimateStrain) : base(strength, aggregateDiameter)
		{
			_ft  = tensileStrength;
			_Eci = elasticModule;

			PlasticStrain  = plasticStrain;
			UltimateStrain = ultimateStrain;
		}

        ///<inheritdoc/>
        public override void UpdateParameters()
		{
		}

		/// <inheritdoc/>
		public override bool Equals(Parameters other) =>
			 other is CustomParameters && base.Equals(other) && TensileStrength == other.TensileStrength && InitialModule == other.InitialModule && PlasticStrain == other.PlasticStrain && UltimateStrain == other.UltimateStrain;

		public override bool Equals(object obj) => obj is CustomParameters other && Equals(other);

		public override int GetHashCode() => base.GetHashCode();
	}
}