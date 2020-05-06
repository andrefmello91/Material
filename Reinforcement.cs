﻿using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Material
{
    public class Reinforcement
    {
        public class Panel : Reinforcement
        {
            // Properties
            public (double X, double Y) BarDiameter { get; }
            public (double X, double Y) BarSpacing { get; }
            public (double esx, double esy) Strains    { get; set; }
            public (Steel X, Steel Y) Steel { get; }
            private double PanelWidth { get; }

            // Constructor
            public Panel((double X, double Y) barDiameter, (double X, double Y) barSpacing, (Steel X, Steel Y) steel, double panelWidth)
            {
                BarDiameter = barDiameter;
                BarSpacing = barSpacing;
                Steel = steel;
                PanelWidth = panelWidth;
            }

            // Calculate the panel reinforcement ratio
            public (double X, double Y) Ratio
            {
                get
                {
                    // Initialize psx and psy
                    double
                        psx = 0,
                        psy = 0;

                    if (BarDiameter.X > 0 && BarSpacing.X > 0)
                        psx = 0.5 * Constants.Pi * BarDiameter.X * BarDiameter.X / (BarSpacing.X * PanelWidth);

                    if (BarDiameter.Y > 0 && BarSpacing.Y > 0)
                        psy = 0.5 * Constants.Pi * BarDiameter.Y * BarDiameter.Y / (BarSpacing.Y * PanelWidth);

                    return
                        (psx, psy);
                }
            }

			// Get reinforcement stresses
			public (double fsx, double fsy) Stresses => (Steel.X.Stress, Steel.Y.Stress);

			// Get reinforcement secant module
			public (double Esx, double Esy) SecantModule => (Steel.X.SecantModule, Steel.Y.SecantModule);

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
        }
    }
}
