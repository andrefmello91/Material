using System;
using andrefmello91.Extensions;
using UnitsNet;
using UnitsNet.Units;

namespace andrefmello91.Material.Reinforcement
{
	/// <summary>
	///     Steel class.
	/// </summary>
	public class Steel : IUnitConvertible<Steel, PressureUnit>, IApproachable<Steel, Pressure>, IEquatable<Steel>, IComparable<Steel>, ICloneable<Steel>
	{

		#region Fields

		/// <summary>
		///     The default <see cref="Pressure" /> tolerance.
		/// </summary>
		public static readonly Pressure Tolerance = Pressure.FromPascals(1E-3);

		/// <summary>
		///     Get tensile hardening consideration.
		/// </summary>
		private readonly bool _considerHardening;

		#endregion

		#region Properties

		/// <summary>
		///     Get elastic module.
		/// </summary>
		public Pressure ElasticModule { get; private set; }

		/// <summary>
		///     Get hardening module.
		/// </summary>
		public Pressure HardeningModule { get; private set; }

		/// <summary>
		///     Get hardening strain.
		/// </summary>
		public double HardeningStrain { get; }

		/// <summary>
		///     Get current steel secant module.
		/// </summary>
		public Pressure SecantModule => Strain.ApproxZero() ? ElasticModule : Stress / Strain;

		/// <summary>
		///     Get current strain.
		/// </summary>
		public double Strain { get; private set; }

		/// <summary>
		///     Get current stress.
		/// </summary>
		public Pressure Stress { get; private set; }

		/// <summary>
		///     Get ultimate strain.
		/// </summary>
		public double UltimateStrain { get; }

		/// <summary>
		///     Get yield strain.
		/// </summary>
		public double YieldStrain => YieldStress / ElasticModule;

		/// <summary>
		///     Get yield stress.
		/// </summary>
		public Pressure YieldStress { get; private set; }

		/// <summary>
		///     Get the <see cref="PressureUnit" /> that this was constructed with.
		/// </summary>
		public PressureUnit Unit
		{
			get => YieldStress.Unit;
			set => ChangeUnit(value);
		}

		#endregion

		#region Constructors

		/// <inheritdoc cref="Steel(Pressure, Pressure, double)" />
		/// <param name="unit">
		///     The <see cref="PressureUnit" /> of <paramref name="yieldStress" /> and
		///     <paramref name="elasticModule" />.
		/// </param>
		public Steel(double yieldStress, double elasticModule = 210000, double ultimateStrain = 0.01, PressureUnit unit = PressureUnit.Megapascal)
			: this(Pressure.From(yieldStress, unit), Pressure.From(elasticModule, unit), ultimateStrain)
		{
		}

		/// <summary>
		///     Steel object with no tensile hardening.
		/// </summary>
		/// <param name="yieldStress">Steel yield stress.</param>
		/// <param name="elasticModule">Steel elastic module.</param>
		/// <param name="ultimateStrain">Steel ultimate strain.</param>
		public Steel(Pressure yieldStress, Pressure elasticModule, double ultimateStrain = 0.01)
		{
			YieldStress    = yieldStress;
			ElasticModule  = elasticModule.ToUnit(Unit);
			UltimateStrain = ultimateStrain;
		}

		/// <inheritdoc cref="Steel(Pressure, Pressure, Pressure, double, double)" />
		/// <inheritdoc cref="Steel(double, double, double, PressureUnit)" />
		public Steel(double yieldStress, double elasticModule, double hardeningModule, double hardeningStrain, double ultimateStrain = 0.01, PressureUnit unit = PressureUnit.Megapascal)
			: this(Pressure.From(yieldStress, unit), Pressure.From(elasticModule, unit), Pressure.From(hardeningModule, unit), hardeningStrain, ultimateStrain)
		{
		}

		/// <summary>
		///     Steel object with tensile hardening.
		/// </summary>
		/// <param name="hardeningModule">Steel hardening module.</param>
		/// <param name="hardeningStrain">Steel strain at the beginning of hardening.</param>
		/// <inheritdoc cref="Steel(Pressure, Pressure, double)" />
		public Steel(Pressure yieldStress, Pressure elasticModule, Pressure hardeningModule, double hardeningStrain, double ultimateStrain = 0.01)
			: this(yieldStress, elasticModule, ultimateStrain)
		{
			_considerHardening = true;
			HardeningModule    = hardeningModule.ToUnit(Unit);
			HardeningStrain    = hardeningStrain;
		}

		#endregion

		#region Methods

		/// <summary>
		///     Calculate stress, given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public Pressure CalculateStress(double strain)
		{
			// Compression yielding
			if (strain <= -YieldStrain)
				return -YieldStress;

			// Elastic
			if (strain < YieldStrain)
				return ElasticModule * strain;

			// Tension yielding
			if (!_considerHardening && strain < UltimateStrain)
				return YieldStress;

			// Tension yielding
			if (_considerHardening && strain < HardeningStrain)
				return YieldStress;

			// Tension hardening (if considered)
			if (_considerHardening && strain < UltimateStrain)
				return YieldStress + HardeningModule * (strain - HardeningStrain);

			// Failure
			return Pressure.Zero;
		}

		/// <inheritdoc />
		public override bool Equals(object? other) => other is Steel steel && Equals(steel);

		/// <summary>
		///     Set steel strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStrain(double strain) => Strain = strain;

		/// <summary>
		///     Set steel strain and stress.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStrainAndStress(double strain)
		{
			SetStrain(strain);
			SetStress(strain);
		}

		/// <summary>
		///     Set steel stress, given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void SetStress(double strain) => Stress = CalculateStress(strain);

		/// <inheritdoc />
		public bool Approaches(Steel? other, Pressure tolerance)
		{
			if (other is null)
				return false;

			var basic = YieldStress.Approx(other.YieldStress, tolerance) && ElasticModule.Approx(other.ElasticModule, tolerance) && UltimateStrain.Approx(other.UltimateStrain);

			if (!other._considerHardening)
				return basic;

			return basic && HardeningModule.Approx(other.HardeningModule, tolerance) && HardeningStrain.Approx(other.HardeningStrain);
		}

		/// <inheritdoc />
		public Steel Clone() => !_considerHardening
			? new Steel(YieldStress, ElasticModule, UltimateStrain)
			: new Steel(YieldStress, ElasticModule, HardeningModule, HardeningStrain, UltimateStrain);


		/// <inheritdoc />
		public int CompareTo(Steel? other) =>
			other is null || YieldStress > other.YieldStress || YieldStress.Approx(other.YieldStress, Tolerance) && ElasticModule > other.ElasticModule
				? 1
				: YieldStress.Approx(other.YieldStress, Tolerance) && ElasticModule.Approx(other.ElasticModule, Tolerance)
					? 0
					: -1;

		/// <inheritdoc />
		public virtual bool Equals(Steel? other) => Approaches(other, Tolerance);

		/// <inheritdoc />
		public void ChangeUnit(PressureUnit unit)
		{
			if (Unit == unit)
				return;

			YieldStress   = YieldStress.ToUnit(unit);
			ElasticModule = ElasticModule.ToUnit(unit);
			Stress        = Stress.ToUnit(unit);

			if (!_considerHardening)
				return;

			HardeningModule = HardeningModule.ToUnit(unit);
		}

		/// <inheritdoc />
		public Steel Convert(PressureUnit unit) =>
			!_considerHardening
				? new Steel(YieldStress.ToUnit(unit), ElasticModule.ToUnit(unit), UltimateStrain)
				: new Steel(YieldStress.ToUnit(unit), ElasticModule.ToUnit(unit), HardeningModule.ToUnit(unit), HardeningStrain, UltimateStrain);

		/// <inheritdoc />
		public override int GetHashCode() => (int) ElasticModule.Gigapascals * (int) YieldStress.Megapascals;

		/// <inheritdoc />
		public override string ToString()
		{
			var epsilon = (char) Characters.Epsilon;

			var msg =
				"Steel Parameters:\n" +
				$"fy = {YieldStress}\n" +
				$"Es = {ElasticModule}\n" +
				$"{epsilon}y = {YieldStrain:0.##E+00}";

			if (_considerHardening)
				msg += "\n\n" +
				       "Hardening parameters:\n" +
				       $"Es = {HardeningModule}\n" +
				       $"{epsilon}y = {HardeningStrain:0.##E+00}";

			return msg;
		}

		#endregion

		#region Operators

		/// <summary>
		///     Returns true if steel parameters are equal.
		/// </summary>
		public static bool operator ==(Steel? left, Steel? right) => left.IsEqualTo(right);


		/// <summary>
		///     Returns true if steel parameters are different.
		/// </summary>
		public static bool operator !=(Steel? left, Steel? right) => left.IsNotEqualTo(right);

		#endregion

	}
}