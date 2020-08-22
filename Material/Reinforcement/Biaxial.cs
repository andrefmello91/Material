using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnitsNet;
using UnitsNet.Units;
using OnPlaneComponents;

namespace Material.Reinforcement
{
	/// <summary>
	/// Biaxial reinforcement class.
	/// </summary>
	public class BiaxialReinforcement : Relations
	{
		// Properties
		/// <summary>
        /// Get/set the web reinforcement of X direction.
        /// </summary>
		public WebReinforcementDirection DirectionX { get; set; }

		/// <summary>
        /// Get/set the web reinforcement of Y direction.
        /// </summary>
		public WebReinforcementDirection DirectionY { get; set; }

		/// <summary>
        /// Get/set the stiffness matrix.
        /// </summary>
		public Matrix<double> Stiffness { get; set; }

		/// <summary>
        /// Get/set reinforcement strains.
        /// </summary>
		public StrainState Strains { get; set; }

		/// <summary>
        /// Get cross-section width.
        /// </summary>
		private double Width { get; }

		/// <summary>
		/// Web reinforcement for biaxial calculations, for equal horizontal (X) and vertical (Y) directions.
		/// </summary>
		/// <param name="barDiameter">The bar diameter (in mm) for directions X and Y.</param>
		/// <param name="barSpacing">The bar spacing (in mm) for directions X and Y.</param>
		/// <param name="steel">The steel objects for directions X and Y.</param>
		/// <param name="width">The width (in mm) of cross-section.</param>
		public BiaxialReinforcement(double barDiameter, double barSpacing, Steel steel, double width)
		{
			// Get new steel for calculations
			Steel
				x = Steel.Copy(steel),
				y = Steel.Copy(steel);

			DirectionX = new WebReinforcementDirection(barDiameter, barSpacing, x, width);
			DirectionY = new WebReinforcementDirection(barDiameter, barSpacing, y, width);
			Width      = width;
		}

        /// <summary>
        /// Web reinforcement for biaxial calculations, for different horizontal (X) and vertical (Y) directions.
        /// </summary>
        /// <param name="barDiameterX">The bar diameter (in mm) for horizontal (X) direction.</param>
        /// <param name="barSpacingX">The bar spacing (in mm) for horizontal (X) direction.</param>
        /// <param name="steelX">The steel objects for horizontal (X) direction.</param>
        /// <param name="barDiameterY">The bar diameter (in mm) for vertical (Y) direction.</param>
        /// <param name="barSpacingY">The bar spacing (in mm) for vertical (Y) direction.</param>
        /// <param name="steelY">The steel objects for vertical (Y) direction.</param>
        /// <param name="width">The width (in mm) of cross-section.</param>
        public BiaxialReinforcement(double barDiameterX, double barSpacingX, Steel steelX, double barDiameterY, double barSpacingY, Steel steelY, double width)
		{
			DirectionX = new WebReinforcementDirection(barDiameterX, barSpacingX, steelX, width);
			DirectionY = new WebReinforcementDirection(barDiameterY, barSpacingY, steelY, width);
			Width      = width;
		}

        /// <summary>
        /// Reinforcement for biaxial calculations, for horizontal (X) and vertical (Y) directions.
        /// </summary>
        /// <param name="barDiameter">The bar diameter (in mm) for directions X and Y.</param>
        /// <param name="barSpacing">The bar spacing (in mm) for directions X and Y.</param>
        /// <param name="steel">The steel objects for directions X and Y.</param>
        /// <param name="width">The width (in mm) of cross-section.</param>
        public BiaxialReinforcement((double X, double Y) barDiameter, (double X, double Y) barSpacing, (Steel X, Steel Y) steel, double width)
		{
			DirectionX = new WebReinforcementDirection(barDiameter.X, barSpacing.X, steel.X, width);
			DirectionY = new WebReinforcementDirection(barDiameter.Y, barSpacing.Y, steel.Y, width);
			Width      = width;
		}

        // Verify if reinforcement is set
        public bool xSet  => DirectionX != null && DirectionX.BarDiameter > 0 && DirectionX.BarSpacing > 0;
		public bool ySet  => DirectionY != null && DirectionY.BarDiameter > 0 && DirectionY.BarSpacing > 0;
        public bool IsSet => xSet || ySet;

		/// <summary>
		/// Get reinforcement stresses, in MPa.
		/// </summary>
		public StressState Stresses
		{
			get
			{
				double
					fsx = DirectionX?.Stress ?? 0,
					fsy = DirectionY?.Stress ?? 0;

				return
					new StressState(fsx, fsy, 0);
			}
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
		/// <param name="strainsState">Current strains.</param>
		public void CalculateStresses(StrainState strainsState)
		{
			Strains = strainsState;
				
			// Calculate stresses in steel
			SetStrainsAndStresses(Strains);
		}

		/// <summary>
		/// Calculate current reinforcement stiffness matrix.
		/// </summary>
		public void CalculateStiffness()
		{
			// Steel matrix
			var Ds = Matrix<double>.Build.Dense(3, 3);

			Ds[0, 0] = DirectionX?.Stiffness ?? 0;
			Ds[1, 1] = DirectionY?.Stiffness ?? 0;
			
            Stiffness = Ds;
		}

		/// <summary>
		/// Calculate initial reinforcement stiffness matrix.
		/// </summary>
		public Matrix<double> InitialStiffness()
		{
			// Steel matrix
			var Ds = Matrix<double>.Build.Dense(3, 3);

			Ds[0, 0] = DirectionX?.InitialStiffness ?? 0;
			Ds[1, 1] = DirectionY?.InitialStiffness ?? 0;

            return Ds;
		}

		/// <summary>
		/// Calculate tension stiffening coefficient (for DSFM).
		/// </summary>
		/// <param name="theta1">Principal tensile strain angle, in radians.</param>
		/// <returns></returns>
		public double TensionStiffeningCoefficient(double theta1)
		{
			if (DirectionX is null && DirectionY is null)
				return 0;

			// Get reinforcement angles and stresses
			var (thetaNx, thetaNy)     = Angles(theta1);

			double
				psx  = DirectionX?.Ratio       ?? 0,
				phiX = DirectionX?.BarDiameter ?? 0,
				psy  = DirectionY?.Ratio       ?? 0,
				phiY = DirectionY?.BarDiameter ?? 0;

			double
				cosNx = Math.Abs(DirectionCosines(thetaNx).cos),
				cosNy = Math.Abs(DirectionCosines(thetaNy).cos);

			// Calculate coefficient for tension stiffening effect
			double m = 0.25 / (psx / phiX * cosNx + psy / phiY * cosNy);

			if (double.IsNaN(m))
				m = 0;

            return m;
		}

		/// <summary>
		/// Calculate maximum value of principal tensile strength (fc1, in MPa) that can be transmitted across cracks.
		/// </summary>
		/// <param name="theta1">Principal tensile strain angle, in radians.</param>
		public double MaximumPrincipalTensileStress(double theta1)
		{
			if (DirectionX is null && DirectionY is null)
				return 0;

            // Get reinforcement angles and stresses
            var (thetaNx, thetaNy) = Angles(theta1);

            double
	            fcx = DirectionX?.CapacityReserve ?? 0,
	            fcy = DirectionY?.CapacityReserve ?? 0;

			double
				cosNx = Math.Abs(DirectionCosines(thetaNx).cos),
				cosNy = Math.Abs(DirectionCosines(thetaNy).cos);

			// Check the maximum value of fc1 that can be transmitted across cracks
			double
				cos2x = cosNx * cosNx,
				cos2y = cosNy * cosNy;

			return
				fcx * cos2x + fcy * cos2y;
		}

		/// <summary>
		/// Set steel strains.
		/// </summary>
		/// <param name="strainsState">Current strains.</param>
		public void SetStrains(StrainState strainsState)
		{
			DirectionX.Steel.SetStrain(strainsState.EpsilonX);
			DirectionY.Steel.SetStrain(strainsState.EpsilonY);
		}

		/// <summary>
		/// Set steel stresses, given strains.
		/// </summary>
		/// <param name="strainsState">Current strains.</param>
		public void SetStresses(StrainState strainsState)
		{
			DirectionX.Steel.SetStress(strainsState.EpsilonX);
			DirectionY.Steel.SetStress(strainsState.EpsilonY);
		}

		/// <summary>
		/// Set steel strains and calculate stresses, in MPa.
		/// </summary>
		/// <param name="strainsState">Current strains.</param>
		public void SetStrainsAndStresses(StrainState strainsState)
		{
			SetStrains(strainsState);
			SetStresses(strainsState);
		}

        /// <summary>
        /// Return a copy of a <see cref="BiaxialReinforcement"/>.
        /// </summary>
        /// <param name="reinforcementToCopy">The <see cref="BiaxialReinforcement"/> to copy.</param>
        /// <returns></returns>
        public static BiaxialReinforcement Copy(BiaxialReinforcement reinforcementToCopy)
		{
			var x = reinforcementToCopy.DirectionX;
			var y = reinforcementToCopy.DirectionY;

			return
				new BiaxialReinforcement(x.BarDiameter, x.BarSpacing, Steel.Copy(x.Steel), y.BarDiameter, y.BarSpacing, Steel.Copy(y.Steel), reinforcementToCopy.Width);
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
			return
				"Reinforcement (x): " + "\n" +
				DirectionX.ToString(diameterUnit, spacingUnit, strengthUnit) + "\n\n" +

				"Reinforcement (y): " + "\n" +
				DirectionY.ToString(diameterUnit, spacingUnit, strengthUnit);
		}

        /// <summary>
        /// Compare two reinforcement objects.
        /// <para>Returns true if parameters are equal.</para>
        /// </summary>
        /// <param name="other">The other reinforcement object.</param>
        public virtual bool Equals(BiaxialReinforcement other)
		{
			if (other != null)
				return DirectionX == other.DirectionX && DirectionY == other.DirectionY;

			return false;
		}

		public override bool Equals(object other)
		{
			if (other is BiaxialReinforcement reinforcement)
				return Equals(reinforcement);

			return false;
		}

		public override int GetHashCode() => DirectionX?.GetHashCode()?? 1 * DirectionY?.GetHashCode() ?? 1 * (int)Width;

		/// <summary>
		/// Returns true if steel parameters are equal.
		/// </summary>
		public static bool operator == (BiaxialReinforcement left, BiaxialReinforcement right) => left != null && left.Equals(right);

		/// <summary>
		/// Returns true if steel parameters are different.
		/// </summary>
		public static bool operator != (BiaxialReinforcement left, BiaxialReinforcement right) => left != null && !left.Equals(right);

    }
}
