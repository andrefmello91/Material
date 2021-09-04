using System;
using andrefmello91.Extensions;
using andrefmello91.Material.Reinforcement;
using UnitsNet;
#nullable enable

namespace andrefmello91.Material.Concrete
{
	public partial class UniaxialConcrete
	{
		/// <summary>
		///     Base class for concrete constitutive model.
		/// </summary>
		private abstract class Constitutive : IEquatable<Constitutive>
		{

			#region Fields

			/// <summary>
			///     Concrete <see cref="IConcreteParameters" />.
			/// </summary>
			protected readonly IConcreteParameters Parameters;

			#endregion

			#region Properties

			/// <summary>
			///     Check if concrete is cracked.
			/// </summary>
			/// <returns>
			///     <b>True</b> if concrete is cracked.
			/// </returns>
			public bool Cracked { get; private set; }

			/// <summary>
			///     The constitutive model of concrete.
			/// </summary>
			public abstract ConstitutiveModel Model { get; }

			#endregion

			#region Constructors

			// Constructor
			/// <summary>
			///     Base class for concrete behavior
			/// </summary>
			/// <param name="parameters">Concrete parameters object.</param>
			protected Constitutive(IConcreteParameters parameters) => Parameters = parameters;

			#endregion

			#region Methods

			/// <summary>
			///     Get concrete <see cref="Constitutive" /> object based on the <see cref="ConstitutiveModel" />.
			/// </summary>
			/// <param name="constitutiveModel">The <see cref="ConstitutiveModel" /> for concrete.</param>
			/// <param name="parameters">Concrete <see cref="Parameters" />.</param>
			public static Constitutive From(ConstitutiveModel constitutiveModel, IConcreteParameters parameters) =>
				constitutiveModel switch
				{
					ConstitutiveModel.DSFM => new DSFMConstitutive(parameters),
					ConstitutiveModel.MCFT => new MCFTConstitutive(parameters),
					_                      => new SMMConstitutive(parameters)
				};

			/// <summary>
			///     Calculate stress (in MPa) given <paramref name="strain" />.
			///     <para>For <seealso cref="UniaxialConcrete" />.</para>
			/// </summary>
			/// <param name="strain">Current strain.</param>
			/// <param name="reinforcement">
			///     The <see cref="UniaxialReinforcement" /> reinforcement (only for
			///     <see cref="DSFMConstitutive" />).
			/// </param>
			public Pressure CalculateStress(double strain, UniaxialReinforcement? reinforcement = null)
			{
				// Correct value
				strain = strain.AsFinite();

				return strain.ApproxZero()
					? Pressure.Zero
					: strain > 0
						? TensileStress(strain, reinforcement)
						: CompressiveStress(strain);
			}

			/// <summary>
			///     Calculate current secant module.
			/// </summary>
			/// <param name="stress">Current stress.</param>
			/// <param name="strain">Current strain.</param>
			public Pressure SecantModule(Pressure stress, double strain) =>
				stress.Abs() <= Material.Concrete.Parameters.Tolerance || strain.Abs() <= 1E-9
					? Parameters.ElasticModule
					: stress / strain;

			public override string ToString() => $"{Model}";

			/// <summary>
			///     Calculate compressive stress for <see cref="UniaxialConcrete" /> case.
			/// </summary>
			/// <param name="strain">Compressive strain (negative) in concrete.</param>
			protected abstract Pressure CompressiveStress(double strain);

			/// <summary>
			///     Calculate the tensile strain for cracked state.
			/// </summary>
			/// <inheritdoc cref="TensileStress" />
			protected abstract Pressure CrackedStress(double strain, UniaxialReinforcement? reinforcement = null);

			/// <summary>
			///     Calculate tensile stress for <see cref="UniaxialConcrete" /> case.
			/// </summary>
			/// <param name="strain">Tensile strain in concrete.</param>
			/// <param name="reinforcement">The <see cref="UniaxialReinforcement" /> (only for <see cref="DSFMConstitutive" />).</param>
			protected Pressure TensileStress(double strain, UniaxialReinforcement? reinforcement = null)
			{
				if (strain.ApproxZero())
					return Pressure.Zero;

				CheckCrackedState(strain);

				return !Cracked
					? UncrackedStress(strain)
					: CrackedStress(strain, reinforcement);
			}

			/// <summary>
			///     Calculate the tensile strain for uncracked state.
			/// </summary>
			/// <param name="strain">Tensile strain in concrete.</param>
			protected Pressure UncrackedStress(double strain) => Parameters.ElasticModule * strain;

			/// <summary>
			///     Check if concrete is cracked based on <paramref name="strain" />.
			/// </summary>
			/// <inheritdoc cref="UncrackedStress" />
			private void CheckCrackedState(double strain)
			{
				Cracked = strain >= Parameters.CrackingStrain;
			}

			public bool Equals(Constitutive? other) => Model == other?.Model;

			#endregion

		}
	}
}