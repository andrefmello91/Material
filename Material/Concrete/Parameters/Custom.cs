namespace Material.Concrete
{
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