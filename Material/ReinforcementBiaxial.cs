using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using UnitsNet;
using UnitsNet.Units;

namespace Material
{
	/// <summary>
    /// Reinforcement base class.
    /// </summary>
	public abstract partial class Reinforcement
	{
		/// <summary>
        /// Biaxial reinforcement class.
        /// </summary>
		public class Biaxial : Relations
		{
			// Properties
			public (double X, double Y) BarDiameter  { get; }
			public (double X, double Y) BarSpacing   { get; }
			public (Steel X, Steel Y)   Steel        { get; }
			public (double X, double Y) Ratio        { get; }
			public Matrix<double>       Stiffness    { get; set; }
			public Vector<double>       Strains      { get; set; }
			private double              SectionWidth { get; }

            /// <summary>
            /// Reinforcement for biaxial calculations, for horizontal (X) and vertical (Y) directions.
            /// </summary>
            /// <param name="barDiameter">The bar diameter (in mm) for directions X and Y.</param>
            /// <param name="barSpacing">The bar spacing (in mm) for directions X and Y.</param>
            /// <param name="steel">The steel objects for directions X and Y.</param>
            /// <param name="sectionWidth">The width (in mm) of cross-section.</param>
            public Biaxial((double X, double Y) barDiameter, (double X, double Y) barSpacing, (Steel X, Steel Y) steel, double sectionWidth)
			{
				BarDiameter  = barDiameter;
				BarSpacing   = barSpacing;
				Steel        = steel;
				SectionWidth = sectionWidth;
				Ratio        = CalculateRatio();
				Stiffness    = InitialStiffness();
			}

			// Verify if reinforcement is set
			public bool xSet  => BarDiameter.X > 0 && BarSpacing.X > 0;
			public bool ySet  => BarDiameter.Y > 0 && BarSpacing.Y > 0;
			public bool IsSet => xSet || ySet;

			// Get steel parameters
			public double fyx  => Steel.X.YieldStress;
			public double Esxi => Steel.X.ElasticModule;
			public double fyy  => Steel.Y.YieldStress;
			public double Esyi => Steel.Y.ElasticModule;

            /// <summary>
            /// Get reinforcement current stresses, in MPa.
            /// </summary>
            public (double fsx, double fsy) SteelStresses => (Steel.X.Stress, Steel.Y.Stress);

            /// <summary>
            /// Get reinforcement current secant module, in MPa.
            /// </summary>
			public (double Esx, double Esy) SecantModule => (Steel.X.SecantModule, Steel.Y.SecantModule);

            /// <summary>
            /// Get reinforcement current stress vector, in MPa.
            /// </summary>
			public Vector<double> Stresses
			{
				get
				{
					var (psx, psy) = Ratio;
					var (fsx, fsy) = SteelStresses;

					return
						Vector<double>.Build.DenseOfArray(new[]
						{
							psx * fsx, psy * fsy, 0
						});
				}
			}

            /// <summary>
            /// Calculate reinforcement ratios, in X and Y, in the cross-section.
            /// </summary>
            public (double X, double Y) CalculateRatio()
			{
				// Initialize psx and psy
				double
					psx = 0,
					psy = 0;

				if (xSet)
					psx = 0.5 * Constants.Pi * BarDiameter.X * BarDiameter.X / (BarSpacing.X * SectionWidth);

				if (ySet)
					psy = 0.5 * Constants.Pi * BarDiameter.Y * BarDiameter.Y / (BarSpacing.Y * SectionWidth);

				return
					(psx, psy);
			}

            /// <summary>
            /// Calculate angles (in radians) related to crack angle.
            /// </summary>
            /// <param name="theta1">Principal tensile strain angle, in radians.</param>
            public (double X, double Y) Angles(double theta1)
			{
				// Calculate angles
				double
					thetaNx = theta1,
					thetaNy = theta1 - Constants.PiOver2;

				return
					(thetaNx, thetaNy);
			}

            /// <summary>
            /// Calculate current stresses, in MPs.
            /// </summary>
            /// <param name="strains">Current strains.</param>
            public void CalculateStresses(Vector<double> strains)
			{
				Strains = strains;
				
				// Calculate stresses in steel
				SetStrainsAndStresses(Strains);
			}

            /// <summary>
            /// Calculate current reinforcement stiffness matrix.
            /// </summary>
            /// <param name="steelSecantModule">Current secant modules, in MPa (default: <see cref="SecantModule"/>).</param>
            public void CalculateStiffness((double Esx, double Esy)? steelSecantModule = null)
			{
				var (psx, psy) = Ratio;

				var (Esx, Esy) = steelSecantModule ?? SecantModule;

				// Steel matrix
				var Ds = Matrix<double>.Build.Dense(3, 3);
				Ds[0, 0] = psx * Esx;
				Ds[1, 1] = psy * Esy;

				Stiffness = Ds;
			}

            /// <summary>
            /// Calculate initial reinforcement stiffness matrix.
            /// </summary>
            public Matrix<double> InitialStiffness()
			{
				var (psx, psy) = Ratio;

                // Steel matrix
                var Ds = Matrix<double>.Build.Dense(3, 3);
				Ds[0, 0] = psx * Esxi;
				Ds[1, 1] = psy * Esyi;

				return Ds;
			}

            /// <summary>
            /// Calculate tension stiffening coefficient (for DSFM).
            /// </summary>
            /// <param name="theta1">Principal tensile strain angle, in radians.</param>
            /// <returns></returns>
            public double TensionStiffeningCoefficient(double theta1)
			{
				// Get reinforcement angles and stresses
				var (thetaNx, thetaNy)     = Angles(theta1);
				(double psx, double psy)   = Ratio;
				(double phiX, double phiY) = BarDiameter;

				double
					cosNx = Math.Abs(DirectionCosines(thetaNx).cos),
					cosNy = Math.Abs(DirectionCosines(thetaNy).cos);

				// Calculate coefficient for tension stiffening effect
				return
					0.25 / (psx / phiX * cosNx + psy / phiY * cosNy);
			}

            /// <summary>
            /// Calculate maximum value of principal tensile strength (fc1, in MPa) that can be transmitted across cracks.
            /// </summary>
            /// <param name="theta1">Principal tensile strain angle, in radians.</param>
            public double MaximumPrincipalTensileStress(double theta1)
			{
				// Get reinforcement angles and stresses
				var (thetaNx, thetaNy) = Angles(theta1);
				(double psx, double psy) = Ratio;
				(double fsx, double fsy) = SteelStresses;
				double fyx = Steel.X.YieldStress;
				double fyy = Steel.Y.YieldStress;

				double
					cosNx = Math.Abs(DirectionCosines(thetaNx).cos),
					cosNy = Math.Abs(DirectionCosines(thetaNy).cos);

				// Check the maximum value of fc1 that can be transmitted across cracks
				double
					cos2x = cosNx * cosNx,
					cos2y = cosNy * cosNy;

				return
					psx * (fyx - fsx) * cos2x + psy * (fyy - fsy) * cos2y;
			}

            /// <summary>
            /// Set steel strains.
            /// </summary>
            /// <param name="strains">Current strains.</param>
            public void SetStrains(Vector<double> strains)
			{
				Steel.X.SetStrain(strains[0]);
				Steel.Y.SetStrain(strains[1]);
			}

            /// <summary>
            /// Set steel stresses, given strains.
            /// </summary>
            /// <param name="strains">Current strains.</param>
			public void SetStresses(Vector<double> strains)
			{
				Steel.X.SetStress(strains[0]);
				Steel.Y.SetStress(strains[1]);
			}

            /// <summary>
            /// Set steel strains and calculate stresses, in MPa.
            /// </summary>
            /// <param name="strains">Current strains.</param>
			public void SetStrainsAndStresses(Vector<double> strains)
			{
				SetStrains(strains);
				SetStresses(strains);
			}

			/// <summary>
			/// Write string with default units (mm and MPa).
			/// </summary>
			public override string ToString() => ToString();

            /// <summary>
            /// Write string with custom units.
            /// </summary>
            /// <param name="diameterUnit">The unit of bar diameter (default: mm).</param>
            /// <param name="spacingUnit">The unit of bar spacing (default: mm).</param>
            /// <param name="strengthUnit">The unit of steel strength (default: MPa).</param>
            /// <returns></returns>
            public string ToString(LengthUnit diameterUnit = LengthUnit.Millimeter, LengthUnit spacingUnit = LengthUnit.Millimeter, PressureUnit strengthUnit = PressureUnit.Megapascal)
			{
				Length
					phiX = Length.FromMillimeters(BarDiameter.X).ToUnit(diameterUnit),
					phiY = Length.FromMillimeters(BarDiameter.Y).ToUnit(diameterUnit),
					sX   = Length.FromMillimeters(BarSpacing.X).ToUnit(spacingUnit),
					sY   = Length.FromMillimeters(BarSpacing.Y).ToUnit(spacingUnit);

				// Approximate reinforcement ratio
				double
					psx = Math.Round(Ratio.X, 3),
					psy = Math.Round(Ratio.Y, 3);

				char rho = (char) Characters.Rho;
				char phi = (char) Characters.Phi;

				return
					"Reinforcement (x): " + phi + phiX + ", s = " + sX +
					" (" + rho + "sx = " + psx + ")\n" + Steel.X.ToString(strengthUnit) + "\n\n" +

					"Reinforcement (y) = " + phi + phiY + ", s = " + sY + " (" +
					rho + "sy = " + psy + ")\n" + Steel.Y.ToString(strengthUnit);
			}
		}
	}
}
