﻿using System;
using Extensions;
using Extensions.Number;
using UnitsNet;
using UnitsNet.Units;

namespace Material.Reinforcement
{
	/// <summary>
    /// Steel class.
    /// </summary>
	public class Steel
	{
		// Auxiliary fields
		private Pressure _fy, _Es, _Esh;

		/// <summary>
        /// Get the <see cref="PressureUnit"/> that this was constructed with.
        /// </summary>
		public PressureUnit Unit => _fy.Unit;

		/// <summary>
		/// Get yield stress, in MPa.
		/// </summary>
		public double YieldStress => _fy.Megapascals;

		/// <summary>
		/// Get elastic module, in MPa.
		/// </summary>
		public double ElasticModule => _Es.Megapascals;

		/// <summary>
        /// Get ultimate strain.
        /// </summary>
		public double UltimateStrain { get; }

		/// <summary>
        /// Get yield strain.
        /// </summary>
		public double YieldStrain => _fy / _Es;

		/// <summary>
        /// Get tensile hardening consideration.
        /// </summary>
		private bool ConsiderTensileHardening { get; }

		/// <summary>
		/// Get hardening module, in MPa.
		/// </summary>
		public double HardeningModule => _Esh.Megapascals;

		/// <summary>
        /// Get hardening strain.
        /// </summary>
		public double HardeningStrain { get; }

		/// <summary>
		/// Get current strain.
		/// </summary>
		public double Strain { get; private set; }

        /// <summary>
        /// Get current stress, in MPa.
        /// </summary>
        public double Stress { get; private set; }

        /// <summary>
        /// Steel object with no tensile hardening, with units in MPa.
        /// </summary>
        /// <param name="yieldStress">Steel yield stress, in MPa.</param>
        /// <param name="elasticModule">Steel elastic module, in MPa <para>Default: 210000 MPa.</para>.</param>
        /// <param name="ultimateStrain">Steel ultimate strain <para>Default: 0.01.</para></param>
        public Steel(double yieldStress, double elasticModule = 210000, double ultimateStrain = 0.01)
		: this (Pressure.FromMegapascals(yieldStress), Pressure.FromMegapascals(elasticModule), ultimateStrain)
		{
		}

        /// <summary>
        /// Steel object with no tensile hardening, with custom <paramref name="unit"/>.
        /// </summary>
        /// <param name="yieldStress">Steel yield stress.</param>
        /// <param name="elasticModule">Steel elastic module.</param>
        /// <param name="ultimateStrain">Steel ultimate strain <para>Default: 0.01.</para></param>
        public Steel(Pressure yieldStress, Pressure elasticModule, double ultimateStrain = 0.01)
		{
			_fy = yieldStress;
			_Es = elasticModule;

			UltimateStrain = ultimateStrain;

			ConsiderTensileHardening = false;
		}

        /// <summary>
        /// Steel object with tensile hardening, with units in MPa.
        /// </summary>
        /// <param name="yieldStress">Steel yield stress, in MPa.</param>
        /// <param name="elasticModule">Steel elastic module, in MPa <para>Default: 210000 MPa.</para>.</param>
        /// <param name="ultimateStrain">Steel ultimate strain <para>Default: 0.01.</para></param>
        /// <param name="hardeningModule">Steel hardening module in MPa.</param>
        /// <param name="hardeningStrain">Steel strain at the beginning of hardening.</param>
        public Steel(double yieldStress, double hardeningModule, double hardeningStrain, double elasticModule = 210000, double ultimateStrain = 0.01)
		: this (Pressure.FromMegapascals(yieldStress), Pressure.FromMegapascals(hardeningModule), hardeningStrain, Pressure.FromMegapascals(elasticModule), ultimateStrain )
		{
		}

        /// <summary>
        /// Steel object with tensile hardening, with custom <paramref name="unit"/>.
        /// </summary>
        /// <param name="yieldStress">Steel yield stress.</param>
        /// <param name="elasticModule">Steel elastic module.</param>
        /// <param name="ultimateStrain">Steel ultimate strain <para>Default: 0.01.</para></param>
        /// <param name="hardeningModule">Steel hardening module.</param>
        /// <param name="hardeningStrain">Steel strain at the beginning of hardening.</param>
        public Steel(Pressure yieldStress, Pressure hardeningModule, double hardeningStrain, Pressure elasticModule, double ultimateStrain = 0.01)
		{
			_fy  = yieldStress;
			_Es  = elasticModule;
			_Esh = hardeningModule;

			UltimateStrain = ultimateStrain;

			ConsiderTensileHardening = true;

			HardeningStrain = hardeningStrain;
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
				if (Strain.Abs() <= 1E-6)
					return ElasticModule;

				return
					Math.Min(Stress / Strain, ElasticModule);
			}
		}

		/// <summary>
        /// Return a new steel object with the same parameters.
        /// </summary>
        /// <param name="steelToCopy">The steel object to copy.</param>
        /// <returns></returns>
        public static Steel Copy(Steel steelToCopy)
		{
			if (steelToCopy is null)
				return null;

	        if (!steelToCopy.ConsiderTensileHardening)
		        return
			        new Steel(steelToCopy.YieldStress, steelToCopy.ElasticModule, steelToCopy.UltimateStrain);

			return
				new Steel(steelToCopy.YieldStress, steelToCopy.HardeningModule, steelToCopy.HardeningStrain, steelToCopy.ElasticModule, steelToCopy.UltimateStrain);
        }

        public override string ToString() 
        {
			char epsilon = (char) Characters.Epsilon;

			string msg =
				"Steel Parameters:\n" +
				$"fy = {_fy}\n" +
				$"Es = {_Es}\n" +
				$"{epsilon}y = " + $"{YieldStrain:0.##E+00}";

			if (ConsiderTensileHardening)
			{
				msg += "\n\n" +
				       "Hardening parameters:\n" +
				       $"Es = {_Esh}\n" +
				       $"{epsilon}y = " + $"{HardeningStrain:0.##E+00}";
			}

            return msg;
		}

		/// <summary>
		/// Compare two steel objects.
		/// <para>Returns true if parameters are equal.</para>
		/// </summary>
		/// <param name="other">The other steel object.</param>
		public virtual bool Equals(Steel other)
		{
			if (other is null)
				return false;

			bool basic = YieldStress == other.YieldStress && ElasticModule == other.ElasticModule && UltimateStrain == other.UltimateStrain;

            if (!other.ConsiderTensileHardening)
				return basic;

            return basic && HardeningModule == other.HardeningModule && HardeningStrain == other.HardeningStrain;
		}

		public override bool Equals(object other)
		{
			if (other is Steel steel)
				return Equals(steel);

			return false;
		}

		public override int GetHashCode() => (int) Math.Pow(ElasticModule, YieldStress);

		/// <summary>
		/// Returns true if steel parameters are equal.
		/// </summary>
		public static bool operator == (Steel left, Steel right) => left != null && left.Equals(right);

        /// <summary>
        /// Returns true if steel parameters are different.
		/// </summary>
        public static bool operator != (Steel left, Steel right) => left != null && !left.Equals(right);
	}
}
