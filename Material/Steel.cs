﻿using System;
using UnitsNet;
using UnitsNet.Units;

namespace Material
{
	/// <summary>
    /// Steel class.
    /// </summary>
	public class Steel
	{
		// Steel properties
		public double YieldStress    { get; }
		public double ElasticModule  { get; }
		public double UltimateStrain { get; }
		public double YieldStrain    => YieldStress / ElasticModule;

		// Hardening properties
		private bool   ConsiderTensileHardening { get; }
		private double HardeningModule          { get; }
		private double HardeningStrain          { get; }

		/// <summary>
		/// Current strain.
		/// </summary>
		public double Strain { get; set; }

		/// <summary>
		/// Current stress.
		/// </summary>
		public double Stress { get; set; }

        /// <summary>
        /// Steel object with no tensile hardening.
        /// </summary>
        /// <param name="yieldStress">Steel yield stress in MPa</param>
        /// <param name="elasticModule">Steel elastic module in MPa (default: 210000 MPa)</param>
        /// <param name="ultimateStrain">Steel ultimate strain in MPa (default: 0.01)</param>
        public Steel(double yieldStress, double elasticModule = 210000, double ultimateStrain = 0.01)
		{
			YieldStress              = yieldStress;
			ElasticModule            = elasticModule;
			UltimateStrain           = ultimateStrain;
			ConsiderTensileHardening = false;
		}

        /// <summary>
        /// Steel object with tensile hardening.
        /// </summary>
        /// <param name="yieldStress">Steel yield stress in MPa</param>
        /// <param name="elasticModule">Steel elastic module in MPa (default: 210000 MPa)</param>
        /// <param name="ultimateStrain">Steel ultimate strain in MPa (default: 0.01)</param>
        /// <param name="hardeningModule">Steel hardening module in MPa</param>
        /// <param name="hardeningStrain">Steel strain at the beginning of hardening</param>
        public Steel(double yieldStress, double hardeningModule, double hardeningStrain, double elasticModule = 210000, double ultimateStrain = 0.01)
		{
			YieldStress              = yieldStress;
			ElasticModule            = elasticModule;
			UltimateStrain           = ultimateStrain;
			ConsiderTensileHardening = true;
			HardeningModule          = hardeningModule;
			HardeningStrain          = hardeningStrain;
		}

        /// <summary>
        /// Set steel strain.
        /// </summary>
        /// <param name="strain">Current strain.</param>
		public void SetStrain(double strain)
		{
			Strain = strain;
		}

        /// <summary>
        /// Set steel stress, given strain.
        /// </summary>
        /// <param name="strain">Current strain.</param>
		public void SetStress(double strain)
		{
			Stress = CalculateStress(strain);
		}

        /// <summary>
        /// Set steel strain and stress.
        /// </summary>
        /// <param name="strain">Current strain.</param>
		public void SetStrainAndStress(double strain)
		{
			SetStrain(strain);
			SetStress(strain);
		}

        /// <summary>
        /// Calculate stress (in MPa), given strain.
        /// </summary>
        /// <param name="strain">Current strain.</param>
		public double CalculateStress(double strain)
		{
			// Compression yielding
			if (strain <= -YieldStrain)
				return -YieldStress;

			// Elastic
			if (strain < YieldStrain)
				return ElasticModule * strain;

            // Tension yielding
            if (!ConsiderTensileHardening && strain < UltimateStrain)
	            return YieldStress;

            // Tension yielding
            if (ConsiderTensileHardening && strain < HardeningStrain)
	            return YieldStress;

            // Tension hardening (if considered)
            if (ConsiderTensileHardening && strain < UltimateStrain)
	            return YieldStress + HardeningModule * (strain - HardeningStrain);

            // Failure
            return 0;
		}

        /// <summary>
        /// Get current steel secant module, in MPa.
        /// </summary>
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


        /// <summary>
        /// Write string with default unit (MPa)
        /// </summary>
        public override string ToString() => ToString();

		/// <summary>
        /// Write string with custom unit (default: MPa)
        /// </summary>
        /// <param name="unit">The stress unit.</param>
        /// <returns></returns>
		public string ToString(PressureUnit unit = PressureUnit.Megapascal)
		{
			char epsilon = (char) Characters.Epsilon;

			double ey = Math.Round(1000 * YieldStrain, 2);

			string msg =
				"Steel Parameters:\n" +
				"fy = " + Pressure.FromMegapascals(YieldStress).ToUnit(unit)   + "\n" +
				"Es = " + Pressure.FromMegapascals(ElasticModule).ToUnit(unit) + "\n" +
				epsilon + "y = " + ey + " E-03";

			if (ConsiderTensileHardening)
			{
				double esh = Math.Round(1000 * HardeningStrain, 2);

				msg += "\n\n" +
				       "Hardening parameters:\n" +
				       "Esh = "          + Pressure.FromMegapascals(HardeningModule).ToUnit(unit) + "\n" + 
				       epsilon + "sh = " + esh + " E-03";
			}

			return msg;
		}
	}
}
