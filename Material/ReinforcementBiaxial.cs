﻿using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Material
{
	public abstract partial class Reinforcement
	{
		public class Biaxial : Relations
		{
			// Properties
			public (double X, double Y) BarDiameter { get; }
			public (double X, double Y) BarSpacing  { get; }
			public (Steel X, Steel Y)   Steel       { get; }
			public (double X, double Y) Ratio       { get; }
			private double              PanelWidth  { get; }

			// Constructor
			public Biaxial((double X, double Y) barDiameter, (double X, double Y) barSpacing,
				(Steel X, Steel Y) steel, double panelWidth)
			{
				BarDiameter = barDiameter;
				BarSpacing  = barSpacing;
				Steel       = steel;
				PanelWidth  = panelWidth;
				Ratio       = CalculateRatio();
			}

			// Verify if reinforcement is set
			public bool xSet  => BarDiameter.X > 0 && BarSpacing.X > 0;
			public bool ySet  => BarDiameter.Y > 0 && BarSpacing.Y > 0;
			public bool IsSet => xSet || ySet;

			// Get stress vector
			public Vector<double> StressVector
			{
				get
				{
					var (psx, psy) = Ratio;
					var (fsx, fsy) = Stresses;

					return
						Vector<double>.Build.DenseOfArray(new []
						{
							psx * fsx, psy * fsy, 0
						});
				}
			}

			// Get reinforcement stresses
			public (double fsx, double fsy) Stresses => (Steel.X.Stress, Steel.Y.Stress);

			// Get reinforcement secant module
			public (double Esx, double Esy) SecantModule => (Steel.X.SecantModule, Steel.Y.SecantModule);

            // Calculate the panel reinforcement ratio
            public (double X, double Y) CalculateRatio()
			{
				// Initialize psx and psy
				double
					psx = 0,
					psy = 0;

				if (xSet)
					psx = 0.5 * Constants.Pi * BarDiameter.X * BarDiameter.X / (BarSpacing.X * PanelWidth);

				if (ySet)
					psy = 0.5 * Constants.Pi * BarDiameter.Y * BarDiameter.Y / (BarSpacing.Y * PanelWidth);

				return
					(psx, psy);
			}

			// Calculate angles related to crack
			public (double X, double Y) Angles(double theta1)
			{
				// Calculate angles
				double
					thetaNx = theta1,
					thetaNy = theta1 - Constants.PiOver2;

				return
					(thetaNx, thetaNy);
			}

			// Calculate tension stiffening coefficient
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

			// Calculate maximum value of fc1 that can be transmitted across cracks
			public double MaximumPrincipalTensileStress(double theta1)
			{
				// Get reinforcement angles and stresses
				var (thetaNx, thetaNy) = Angles(theta1);
				(double psx, double psy) = Ratio;
				(double fsx, double fsy) = Stresses;
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

			// Set steel strains
			public void SetStrains(Vector<double> strains)
			{
				Steel.X.SetStrain(strains[0]);
				Steel.Y.SetStrain(strains[1]);
			}

			// Set steel stresses
			public void SetStresses(Vector<double> strains)
			{
				Steel.X.SetStress(strains[0]);
				Steel.Y.SetStress(strains[1]);
			}

			// Set steel strain and stresses
			public void SetStrainsAndStresses(Vector<double> strains)
			{
				SetStrains(strains);
				SetStresses(strains);
			}

			public override string ToString()
			{
				// Approximate reinforcement ratio
				double
					psx = Math.Round(Ratio.X, 3),
					psy = Math.Round(Ratio.Y, 3);

				char rho = (char) Characters.Rho;
				char phi = (char) Characters.Phi;

				return
					"Reinforcement (x): " + phi + BarDiameter.X + " mm, s = " + BarSpacing.X +
					" mm (" + rho + "sx = " + psx + ")\n" + Steel.X +

					"\n\nReinforcement (y) = " + phi + BarDiameter.Y + " mm, s = " + BarSpacing.Y + " mm (" +
					rho + "sy = " + psy + ")\n" + Steel.Y;
			}
		}
	}
}
