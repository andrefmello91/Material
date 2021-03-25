using andrefmello91.Material.Reinforcement;
using Extensions;
using UnitsNet;

#nullable enable

namespace andrefmello91.Material.Concrete
{
	public partial class UniaxialConcrete
	{
		/// <summary>
		///     Base class for concrete constitutive model.
		/// </summary>
		private abstract class Constitutive : IConstitutive
		{
			#region Fields

			/// <summary>
			///     Concrete <see cref="IParameters" />.
			/// </summary>
			protected readonly IParameters Parameters;

			#endregion

			#region Properties

			public bool ConsiderCrackSlip { get; protected set; }

			public bool Cracked { get; set; }

			public abstract ConstitutiveModel Model { get; }

			#endregion

			#region Constructors

			// Constructor
			/// <summary>
			///     Base class for concrete behavior
			/// </summary>
			/// <param name="parameters">Concrete parameters object.</param>
			protected Constitutive(IParameters parameters) => Parameters = parameters;

			#endregion

			#region Methods

			/// <summary>
			///     Get concrete <see cref="Constitutive" /> object based on the <see cref="ConstitutiveModel" />.
			/// </summary>
			/// <param name="constitutiveModel">The <see cref="ConstitutiveModel" /> for concrete.</param>
			/// <param name="parameters">Concrete <see cref="Parameters" />.</param>
			public static Constitutive Read(ConstitutiveModel constitutiveModel, IParameters parameters) =>
				constitutiveModel switch
				{
					ConstitutiveModel.DSFM => new DSFMConstitutive(parameters),
					_                      => new MCFTConstitutive(parameters),
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
			public Pressure CalculateStress(double strain, UniaxialReinforcement? reinforcement = null) =>
				strain.ApproxZero()
					? Pressure.Zero
					: strain > 0
						? TensileStress(strain, reinforcement)
						: CompressiveStress(strain);

			/// <summary>
			///     Calculate current secant module.
			/// </summary>
			/// <param name="stress">Current stress.</param>
			/// <param name="strain">Current strain.</param>
			public Pressure SecantModule(Pressure stress, double strain) =>
				stress.Abs() <= Material.Concrete.Parameters.Tolerance || strain.Abs() <= 1E-9 
					? Parameters.ElasticModule 
					: stress / strain;

			/// <summary>
			///     Calculate tensile stress for <see cref="UniaxialConcrete" /> case.
			/// </summary>
			/// <param name="strain">Tensile strain in concrete.</param>
			/// <param name="reinforcement">The <see cref="UniaxialReinforcement" /> (only for <see cref="DSFMConstitutive" />).</param>
			protected abstract Pressure TensileStress(double strain, UniaxialReinforcement? reinforcement = null);

			/// <summary>
			///     Calculate compressive stress for <see cref="UniaxialConcrete" /> case.
			/// </summary>
			/// <param name="strain">Compressive strain (negative) in concrete.</param>
			protected abstract Pressure CompressiveStress(double strain);

			/// <summary>
			///     Check if concrete is cracked for <see cref="UniaxialConcrete" /> case and set cracked property.
			/// </summary>
			/// <param name="strain">Current strain</param>
			protected void VerifyCrackedState(double strain)
			{
				if (!Cracked && strain >= Parameters.CrackingStrain)
					Cracked = true;
			}

			public bool Equals(IConstitutive? other) => Model == other?.Model;

			public override string ToString() => $"{Model}";

			#endregion
		}
	}
}