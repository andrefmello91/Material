﻿using System;
using MathNet.Numerics;
using UnitsNet;
using UnitsNet.Units;

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

			// Calculate normal stiffness
			public double Stiffness => Steel.ElasticModule * Area;

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

			// Calculate current force
			public double CalculateForce(double strain)
			{
				return
					Area * Steel.CalculateStress(strain);
			}

			// Calculate tension stiffening coefficient
			public double TensionStiffeningCoefficient()
			{
				// Calculate coefficient for tension stiffening effect
				return
					0.25 * BarDiameter / Ratio;
			}

			// Calculate maximum value of fc1 that can be transmitted across cracks
			public double MaximumPrincipalTensileStress()
			{
				// Get reinforcement stress
				double
					fs = Stress,
					fy = Steel.YieldStress;

				// Check the maximum value of fc1 that can be transmitted across cracks
				return
					Ratio * (fy - fs);
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

            public string ToString(LengthUnit diameterUnit, PressureUnit strengthUnit)
            {
	            var areaUnit = AreaUnit.SquareMeter;

	            if (diameterUnit == LengthUnit.Millimeter)
		            areaUnit = AreaUnit.SquareMillimeter;

				else if (diameterUnit == LengthUnit.Centimeter)
		            areaUnit = AreaUnit.SquareCentimeter;

	            var d  = Length.FromMillimeters(BarDiameter).ToUnit(diameterUnit);
	            var As = UnitsNet.Area.FromSquareMillimeters(Area).ToUnit(areaUnit);

				char phi = (char) Characters.Phi;

				return
					"Reinforcement: " + NumberOfBars + " " + phi + d + " (" + As +
					")\n\n" + Steel.ToString(strengthUnit);
			}
		}
	}
}
