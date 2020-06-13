using System;

namespace Material
{
	// Steel
	public class Steel
	{
		// Steel properties
		public double YieldStress    { get; }
		public double ElasticModule  { get; }
		public double UltimateStrain { get; }
		public double Strain         { get; set; }
		public double Stress         { get; set; }
		public double YieldStrain    => YieldStress / ElasticModule;

		// Hardening properties
		private bool   ConsiderStrainHardening { get; }
		private double HardeningModule         { get; }
		private double HardeningStrain         { get; }

        // Read the steel parameters
        public Steel(double yieldStress, double elasticModule = 210000, double ultimateStrain = 0.01, bool considerStrainHardening = false, double hardeningModule = 0, double hardeningStrain = 0)
		{
			YieldStress             = yieldStress;
			ElasticModule           = elasticModule;
			UltimateStrain          = ultimateStrain;
			ConsiderStrainHardening = considerStrainHardening;
			HardeningModule         = hardeningModule;
			HardeningStrain         = hardeningStrain;
		}

		// Set steel strain
		public void SetStrain(double strain)
		{
			Strain = strain;
		}

		// Calculate stress in reinforcement given strain
		public void SetStress(double strain)
		{
			Stress = CalculateStress(strain);
		}

		// Set Strain and calculate stress
		public void SetStrainAndStress(double strain)
		{
			SetStrain(strain);
			SetStress(strain);
		}

		// Calculate stress
		public double CalculateStress(double strain)
		{
			// Compression yielding
			if (strain <= -YieldStrain)
				return -YieldStress;

			// Elastic
			if (strain < YieldStrain)
				return ElasticModule * strain;

            // Tension yielding
            if (!ConsiderStrainHardening && strain < UltimateStrain)
	            return YieldStress;

            // Tension yielding
            if (ConsiderStrainHardening && strain < HardeningStrain)
	            return YieldStress;

            // Tension hardening (if considered)
            if (ConsiderStrainHardening && strain < UltimateStrain)
	            return YieldStress + HardeningModule * (strain - HardeningStrain);

            // Failure
            return 0;
		}

        // Calculate secant module of steel
        public double SecantModule
		{
			get
			{
				// Verify the strain
				if (Strain == 0)
					return ElasticModule;

				return
					Math.Min(Stress / Strain, ElasticModule);
			}
		}

		public override string ToString()
		{
			char epsilon = (char) Characters.Epsilon;

			double ey = Math.Round(1000 * YieldStrain, 2);

			string msg =
				"Steel Parameters:\n" +
				"fy = "          + YieldStress   + " MPa\n" +
				"Es = "          + ElasticModule + " MPa\n" +
				epsilon + "y = " + ey            + " E-03";

			if (ConsiderStrainHardening)
			{
				double esh = Math.Round(1000 * HardeningStrain, 2);

				msg += "\n\n" +
				       "Hardening parameters:\n" +
				       "Esh = "          + HardeningModule + " MPa\n" +
				       epsilon + "sh = " + esh             + " E-03";
			}

			return msg;
		}
	}
}
