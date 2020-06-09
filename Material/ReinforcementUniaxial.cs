using System;
using MathNet.Numerics;

namespace Material
{
	public abstract partial class Reinforcement
	{
		public class Uniaxial
		{
			// Properties
			public  int    NumberOfBars  { get; }
			public  double BarDiameter   { get; }
			public  double Area          { get; }
			public  Steel  Steel         { get; }
			private double ConcreteArea  { get; }

            // Constructor
            public Uniaxial(int numberOfBars, double barDiameter, double concreteArea = 0, Steel steel = null)
			{
				NumberOfBars = numberOfBars;
				BarDiameter  = barDiameter;
				ConcreteArea = concreteArea;
				Steel        = steel;
				Area         = ReinforcementArea();
			}

			// Verify if reinforcement is set
			public bool IsSet => NumberOfBars > 0 && BarDiameter > 0;

			// Get reinforcement ratio
			public double Ratio
			{
				get
				{
					if (Area != 0 && ConcreteArea != 0)
						return
							Area / ConcreteArea;

					return 0;
				}
			}

			// Get strain and stress
			public double Strain => Steel.Strain;
			public double Stress => Steel.Stress;

			// Calculate yield force
			public double YieldForce => Area * Steel.YieldStress;

			// Calculate current force
			public double Force => Area * Stress;

			// Calculated reinforcement area
			private double ReinforcementArea()
			{
				if (IsSet)
					return
						0.25 * NumberOfBars * Constants.Pi * BarDiameter * BarDiameter;

				return 0;
			}

			// Set steel strains
			public void SetStrain(double strain)
			{
				Steel.SetStrain(strain);
			}

			// Set steel stresses
			public void SetStress(double strain)
			{
				Steel.SetStress(strain);
			}

			// Set steel strain and stresses
			public void SetStrainsAndStresses(double strain)
			{
				SetStrain(strain);
				SetStress(strain);
			}

            public override string ToString()
			{
				// Approximate steel area
				double As = Math.Round(Area, 2);

				char phi = (char) Characters.Phi;

				return
					"Reinforcement: " + NumberOfBars + " " + phi + BarDiameter + " mm (" + As +
					" mm²)\n\n" + Steel;
			}
		}
	}
}
