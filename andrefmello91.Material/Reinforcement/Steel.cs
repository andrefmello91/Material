using System;
using andrefmello91.Extensions;
using UnitsNet;
using UnitsNet.Units;

namespace andrefmello91.Material.Reinforcement
{
	/// <summary>
	///     Steel class.
	/// </summary>
	public class Steel : IUnitConvertible<PressureUnit>, IApproachable<Steel, Pressure>, IEquatable<Steel>, IComparable<Steel>, ICloneable<Steel>
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
		///     The steel parameters.
		/// </summary>
		public SteelParameters Parameters { get; }

		/// <summary>
		///     Get current steel secant module.
		/// </summary>
		public Pressure SecantModule => Strain.ApproxZero()
			? Parameters.ElasticModule
			: Stress / Strain;

		/// <summary>
		///     Get current strain.
		/// </summary>
		public double Strain { get; private set; }

		/// <summary>
		///     Get current stress.
		/// </summary>
		public Pressure Stress { get; private set; }

		/// <summary>
		///     Check if steel yielded.
		/// </summary>
		/// <returns>
		///     True if steel strain is bigger than <see cref="SteelParameters.YieldStrain" />.
		/// </returns>
		public bool Yielded => Strain.Abs() >= Parameters.YieldStrain;

		/// <summary>
		///     Get the <see cref="PressureUnit" /> that this was constructed with.
		/// </summary>
		public PressureUnit Unit
		{
			get => Parameters.Unit;
			set => ChangeUnit(value);
		}

		#endregion

		#region Constructors

		/// <summary>
		///     Create a steel object from steel parameters.
		/// </summary>
		/// <param name="parameters">Steel parameters.</param>
		public Steel(SteelParameters parameters) => Parameters = parameters;

		/// <inheritdoc cref="Steel(Pressure, Pressure, double)" />
		/// <param name="unit">
		///     The <see cref="PressureUnit" /> of <paramref name="yieldStress" /> and
		///     <paramref name="elasticModule" />.
		/// </param>
		public Steel(double yieldStress, double elasticModule = 210000, double ultimateStrain = 0.01, PressureUnit unit = PressureUnit.Megapascal)
			: this(new SteelParameters(yieldStress, elasticModule, ultimateStrain, unit))
		{
		}

		/// <summary>
		///     Steel object with no tensile hardening.
		/// </summary>
		/// <param name="yieldStress">Steel yield stress.</param>
		/// <param name="elasticModule">Steel elastic module.</param>
		/// <param name="ultimateStrain">Steel ultimate strain.</param>
		public Steel(Pressure yieldStress, Pressure elasticModule, double ultimateStrain = 0.01)
			: this(new SteelParameters(yieldStress, elasticModule, ultimateStrain))
		{
		}

		/// <inheritdoc cref="Steel(Pressure, Pressure, Pressure, double, double)" />
		/// <inheritdoc cref="Steel(double, double, double, PressureUnit)" />
		public Steel(double yieldStress, double elasticModule, double hardeningModule, double hardeningStrain, double ultimateStrain = 0.01, PressureUnit unit = PressureUnit.Megapascal)
			: this((Pressure) yieldStress.As(unit), (Pressure) elasticModule.As(unit), (Pressure) hardeningModule.As(unit), hardeningStrain, ultimateStrain)
		{
		}

		/// <summary>
		///     Steel object with tensile hardening.
		/// </summary>
		/// <param name="hardeningModule">Steel hardening module.</param>
		/// <param name="hardeningStrain">Steel strain at the beginning of hardening.</param>
		/// <inheritdoc cref="Steel(Pressure, Pressure, double)" />
		public Steel(Pressure yieldStress, Pressure elasticModule, Pressure hardeningModule, double hardeningStrain, double ultimateStrain = 0.01)
			: this(new SteelParameters(yieldStress, elasticModule, hardeningModule, hardeningStrain, ultimateStrain))
		{
		}

		#endregion

		#region Methods

		/// <summary>
		///     Calculate stress, given strain.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		private static Pressure CalculateStress(SteelParameters parameters, double strain)
		{
			// Correct value
			strain = strain.AsFinite();

			return parameters.ConsiderHardening switch
			{
				// Failure
				{ } when strain.Abs() >= parameters.UltimateStrain => Pressure.Zero,

				// Elastic
				{ } when strain.IsBetween(-parameters.YieldStrain, parameters.YieldStrain) => parameters.ElasticModule * strain,

				// Compression yielding
				{ } when strain.IsBetween(-parameters.UltimateStrain, -parameters.YieldStrain) => -parameters.YieldStress,

				// Tension yielding with no hardening
				false when strain.IsBetween(parameters.YieldStrain, parameters.UltimateStrain) => parameters.YieldStress,

				// Tension yielding with hardening
				true when strain.IsBetween(parameters.YieldStrain, parameters.HardeningStrain) => parameters.YieldStress,

				// Tension hardening (if considered)
				true when strain.IsBetween(parameters.HardeningStrain, parameters.UltimateStrain) => parameters.YieldStress + parameters.HardeningModule * (strain - parameters.HardeningStrain),

				// Default
				_ => Pressure.Zero
			};

			// Failure
		}

		/// <summary>
		///     Set steel strain and calculate stress.
		/// </summary>
		/// <param name="strain">Current strain.</param>
		public void Calculate(double strain)
		{
			Strain = strain.AsFinite();
			Stress = CalculateStress(Parameters, strain);
		}

		/// <inheritdoc cref="IUnitConvertible{TUnit}.Convert" />
		public Steel Convert(PressureUnit unit) => new(Parameters.Convert(unit));


		/// <inheritdoc />
		public override bool Equals(object? other) => other is Steel steel && Equals(steel);

		/// <inheritdoc />
		public override int GetHashCode() => Parameters.GetHashCode();

		/// <inheritdoc />
		public override string ToString() => Parameters.ToString();

		/// <inheritdoc />
		public bool Approaches(Steel? other, Pressure tolerance) => other is not null && Parameters.Approaches(other.Parameters, tolerance);

		/// <inheritdoc />
		public Steel Clone() => new(Parameters.Clone());


		/// <inheritdoc />
		public int CompareTo(Steel? other) =>
			other is not null
				? Parameters.CompareTo(other.Parameters)
				: 1;

		/// <inheritdoc />
		public virtual bool Equals(Steel? other) => Approaches(other, Tolerance);

		/// <inheritdoc />
		public void ChangeUnit(PressureUnit unit)
		{
			if (Unit == unit)
				return;

			Parameters.ChangeUnit(unit);
			Stress = Stress.ToUnit(unit);
		}

		IUnitConvertible<PressureUnit> IUnitConvertible<PressureUnit>.Convert(PressureUnit unit) => Convert(unit);

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