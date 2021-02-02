using System;
using UnitsNet;

namespace Material.Concrete
{
	public partial struct Parameters
	{
		/// <summary>
		///		Parameter calculator base class.
		/// </summary>
		private abstract class ParameterCalculator : IEquatable<ParameterCalculator>
		{
			/// <summary>
			///		Get the <see cref="ParameterModel"/>.
			/// </summary>
			public abstract ParameterModel Model { get; }

			/// <summary>
			///     Get tensile strength.
			/// </summary>
			public Pressure TensileStrength { get; protected set; }

			/// <summary>
			///     The type of concrete aggregate.
			/// </summary>
			protected readonly AggregateType Type;

			/// <summary>
			///     Concrete strength.
			/// </summary>
			protected Pressure Strength;

			/// <summary>
			///     Get initial elastic module.
			/// </summary>
			public Pressure ElasticModule { get; protected set; }

			/// <summary>
			///     Get secant module.
			/// </summary>
			public virtual Pressure SecantModule => Strength / PlasticStrain;

			/// <summary>
			///     Get concrete plastic (peak) strain (negative value).
			/// </summary>
			public double PlasticStrain { get; protected set; }

			/// <summary>
			///     Get concrete ultimate strain (negative value).
			/// </summary>
			public double UltimateStrain { get; protected set; }

			/// <summary>
			///     Get fracture parameter.
			/// </summary>
			public virtual ForcePerLength FractureParameter => ForcePerLength.FromNewtonsPerMillimeter(0.075);

			/// <summary>
			/// Base parameter calculator class.
			/// </summary>
			/// <param name="strength">Concrete compressive strength (positive value).</param>
			/// <param name="type">The <see cref="AggregateType"/>.</param>
			protected ParameterCalculator(Pressure strength, AggregateType type)
			{
				Strength = strength;
				Type     = type;
			}

			/// <summary>
			///		Get the <see cref="ParameterCalculator"/> based on <see cref="ParameterModel"/>.
			/// </summary>
			/// <param name="model">The <see cref="ParameterModel"/>.</param>
			/// <inheritdoc cref="ParameterCalculator(Pressure, AggregateType)"/>
			public static ParameterCalculator GetCalculator(Pressure strength, ParameterModel model, AggregateType type) =>
				model switch
				{
					ParameterModel.MC2010  => new MC2010(strength, type),
					ParameterModel.NBR6118 => new NBR6118(strength, type),
					ParameterModel.MCFT    => new MCFT(strength, type),
					_                      => new DSFM(strength, type)
				};

			public bool Equals(ParameterCalculator? other) => !(other is null) && Model == other.Model;
		}
	}
}
