using System;
using System.Runtime.CompilerServices;
using Extensions.Number;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnitsNet;
using UnitsNet.Units;
using OnPlaneComponents;

namespace Material.Reinforcement
{
	/// <summary>
	/// Web reinforcement class.
	/// </summary>
	public class WebReinforcement : IEquatable<WebReinforcement>
	{
		// Auxiliary fields
		private Length _w;

		// Properties
		/// <summary>
        /// Get the <see cref="WebReinforcementDirection"/> on X direction.
        /// </summary>
		public WebReinforcementDirection DirectionX { get; }

        /// <summary>
        /// Get the <see cref="WebReinforcementDirection"/> on Y direction.
        /// </summary>
        public WebReinforcementDirection DirectionY { get; }

		/// <summary>
        /// Get/set reinforcement <see cref="StrainState"/>.
        /// </summary>
		public StrainState Strains { get; private set; }

		/// <summary>
		/// Get cross-section width, in mm.
		/// </summary>
		private double Width => _w.Millimeters;

		/// <summary>
        /// Returns true if reinforcement <see cref="DirectionX"/> exists.
        /// </summary>
        public bool XReinforced  => DirectionX != null && DirectionX.BarDiameter > 0 && DirectionX.BarSpacing > 0;

		/// <summary>
		/// Returns true if reinforcement <see cref="DirectionY"/> exists.
		/// </summary>
		public bool YReinforced  => DirectionY != null && DirectionY.BarDiameter > 0 && DirectionY.BarSpacing > 0;

		/// <summary>
		/// Returns true if reinforcement <see cref="DirectionX"/> and <see cref="DirectionY"/> exist.
		/// </summary>
		public bool XYReinforced => XReinforced && YReinforced;

		/// <summary>
		/// Get reinforcement <see cref="StressState"/>, in MPa.
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
		/// Get current <see cref="WebReinforcement"/> stiffness <see cref="Matrix"/>.
		/// </summary>
		public Matrix<double> Stiffness
		{
			get
			{
				// Steel matrix
				var Ds = Matrix<double>.Build.Dense(3, 3);

				Ds[0, 0] = DirectionX?.Stiffness ?? 0;
				Ds[1, 1] = DirectionY?.Stiffness ?? 0;

				return Ds;
			}
		}

		/// <summary>
		/// Get initial <see cref="WebReinforcement"/> stiffness <see cref="Matrix"/>.
		/// </summary>
		public Matrix<double> InitialStiffness
		{
			get
			{
				// Steel matrix
				var Ds = Matrix<double>.Build.Dense(3, 3);

				Ds[0, 0] = DirectionX?.InitialStiffness ?? 0;
				Ds[1, 1] = DirectionY?.InitialStiffness ?? 0;

				return Ds;
			}
		}

        /// <summary>
        /// Web reinforcement for biaxial calculations, for equal horizontal (X) and vertical (Y) directions.
        /// </summary>
        /// <param name="barDiameter">The bar diameter (in mm) for directions X and Y.</param>
        /// <param name="barSpacing">The bar spacing (in mm) for directions X and Y.</param>
        /// <param name="steel">The steel objects for directions X and Y.</param>
        /// <param name="width">The width (in mm) of cross-section.</param>
        public WebReinforcement(double barDiameter, double barSpacing, Steel steel, double width)
            : this(Length.FromMillimeters(barDiameter), Length.FromMillimeters(barSpacing), steel, Length.FromMillimeters(width))
        {
        }

        /// <summary>
        /// Web reinforcement for biaxial calculations, for equal horizontal (X) and vertical (Y) directions.
        /// </summary>
        /// <param name="barDiameter">The bar diameter for directions X and Y.</param>
        /// <param name="barSpacing">The bar spacing for directions X and Y.</param>
        /// <param name="steel">The steel objects for directions X and Y.</param>
        /// <param name="width">The width of cross-section.</param>
        public WebReinforcement(Length barDiameter, Length barSpacing, Steel steel, Length width)
            : this(barDiameter, barSpacing, steel, barDiameter, barSpacing, steel.Copy(), width)
        {
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
        public WebReinforcement(double barDiameterX, double barSpacingX, Steel steelX, double barDiameterY, double barSpacingY, Steel steelY, double width)
            : this(Length.FromMillimeters(barDiameterX), Length.FromMillimeters(barSpacingX), steelX, Length.FromMillimeters(barDiameterY), Length.FromMillimeters(barSpacingY), steelY, Length.FromMillimeters(width))
        {
        }

        /// <summary>
        /// Web reinforcement for biaxial calculations, for different horizontal (X) and vertical (Y) directions.
        /// </summary>
        /// <param name="barDiameterX">The bar diameter for horizontal (X) direction.</param>
        /// <param name="barSpacingX">The bar spacing for horizontal (X) direction.</param>
        /// <param name="steelX">The steel objects for horizontal (X) direction.</param>
        /// <param name="barDiameterY">The bar diameter for vertical (Y) direction.</param>
        /// <param name="barSpacingY">The bar spacing for vertical (Y) direction.</param>
        /// <param name="steelY">The steel objects for vertical (Y) direction.</param>
        /// <param name="width">The width of cross-section.</param>
        public WebReinforcement(Length barDiameterX, Length barSpacingX, Steel steelX, Length barDiameterY, Length barSpacingY, Steel steelY, Length width)
        {
            DirectionX = ReadReinforcementDirection(barDiameterX, barSpacingX, steelX, width);
            DirectionY = ReadReinforcementDirection(barDiameterY, barSpacingY, steelY, width);
            _w = width;
        }

        /// <summary>
        /// Read the <see cref="WebReinforcementDirection"/>.
        /// <para>Returns null if <paramref name="barDiameter"/> or <paramref name="barSpacing"/> are zero, or if <paramref name="steel"/> is null.</para>
        /// </summary>
        /// <param name="barDiameter">The bar diameter (in mm) for directions X and Y.</param>
        /// <param name="barSpacing">The bar spacing (in mm) for directions X and Y.</param>
        /// <param name="steel">The steel objects for directions X and Y.</param>
        /// <param name="width">The width (in mm) of cross-section.</param>
        private WebReinforcementDirection ReadReinforcementDirection(double barDiameter, double barSpacing, Steel steel, double width)
		{
			if (barDiameter == 0 || barSpacing == 0 || steel is null)
				return null;

			return
				new WebReinforcementDirection(barDiameter, barSpacing, steel, width);
		}

		/// <summary>
        /// Read the <see cref="WebReinforcementDirection"/>.
        /// <para>Returns null if <paramref name="barDiameter"/> or <paramref name="barSpacing"/> are zero, or if <paramref name="steel"/> is null.</para>
        /// </summary>
        /// <param name="barDiameter">The bar diameter for directions X and Y.</param>
        /// <param name="barSpacing">The bar spacing for directions X and Y.</param>
        /// <param name="steel">The steel objects for directions X and Y.</param>
        /// <param name="width">The width of cross-section.</param>
		private WebReinforcementDirection ReadReinforcementDirection(Length barDiameter, Length barSpacing, Steel steel, Length width)
		{
			if (barDiameter == Length.Zero || barSpacing == Length.Zero || steel is null)
				return null;

			return
				new WebReinforcementDirection(barDiameter, barSpacing, steel, width);
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
		/// Calculate current <see cref="StressState"/>, in MPa.
		/// </summary>
		/// <param name="strainsState">Current <see cref="StrainState"/>.</param>
		public void CalculateStresses(StrainState strainsState)
		{
			Strains = strainsState;
				
			// Calculate stresses in steel
			SetStrainsAndStresses(Strains);
		}

		/// <summary>
		/// Set steel <see cref="StrainState"/>.
		/// </summary>
		/// <param name="strainsState">Current <see cref="StrainState"/>.</param>
		public void SetStrains(StrainState strainsState)
		{
			DirectionX?.Steel?.SetStrain(strainsState.EpsilonX);
			DirectionY?.Steel?.SetStrain(strainsState.EpsilonY);
		}

		/// <summary>
		/// Set steel <see cref="StressState"/>, given <see cref="StrainState"/>.
		/// </summary>
		/// <param name="strainsState">Current <see cref="StrainState"/>.</param>
		public void SetStresses(StrainState strainsState)
		{
			DirectionX?.Steel?.SetStress(strainsState.EpsilonX);
			DirectionY?.Steel?.SetStress(strainsState.EpsilonY);
		}

		/// <summary>
		/// Set steel <see cref="StrainState"/> and calculate <see cref="StressState"/>, in MPa.
		/// </summary>
		/// <param name="strainsState">Current <see cref="StrainState"/>.</param>
		public void SetStrainsAndStresses(StrainState strainsState)
		{
			SetStrains(strainsState);
			SetStresses(strainsState);
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
			var (thetaNx, thetaNy) = Angles(theta1);

			double den = 0;

			if (!(DirectionX is null))
			{
				double
					psx   = DirectionX.Ratio,
					phiX  = DirectionX.BarDiameter,
					cosNx = thetaNx.DirectionCosines(true).cos;

				den += psx / phiX * cosNx;
			}

			if (!(DirectionY is null))
			{
				double
					psy   = DirectionY.Ratio,
					phiY  = DirectionY.BarDiameter,
					cosNy = thetaNy.DirectionCosines(true).cos;

				den += psy / phiY * cosNy;
			}

			// Return m
			return
				0.25 / den;
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
				cosNx = thetaNx.DirectionCosines(true).cos,
				cosNy = thetaNy.DirectionCosines(true).cos;

			// Check the maximum value of fc1 that can be transmitted across cracks
			double
				cos2x = cosNx * cosNx,
				cos2y = cosNy * cosNy;

			return
				fcx * cos2x + fcy * cos2y;
		}

        /// <summary>
        /// Return a copy of this <see cref="WebReinforcement"/>.
        /// </summary>
        public WebReinforcement Copy()
        {
	        if (DirectionX is null && DirectionY is null)
		        return null;

            if (DirectionX is null)
	            return TransversalOnly(DirectionX.BarDiameter, DirectionX.BarSpacing, DirectionX.Steel.Copy(), DirectionX.Width);

            if (DirectionY is null)
	            return HorizontalOnly(DirectionY.BarDiameter, DirectionY.BarSpacing, DirectionY.Steel.Copy(), DirectionY.Width);
			
			return
				new WebReinforcement(DirectionX.BarDiameter, DirectionX.BarSpacing, DirectionX.Steel.Copy(), DirectionY.BarDiameter, DirectionY.BarSpacing, DirectionY.Steel.Copy(), Width);
		}

        /// <summary>
        /// Return a <see cref="WebReinforcement"/> with <see cref="DirectionX"/> only..
        /// </summary>
        /// <param name="barDiameter">The bar diameter (in mm) for X direction.</param>
        /// <param name="barSpacing">The bar spacing (in mm) for  X direction.</param>
        /// <param name="steel">The steel objects for X direction.</param>
        /// <param name="width">The width (in mm) of cross-section.</param>
        public static WebReinforcement HorizontalOnly(double barDiameter, double barSpacing, Steel steel, double width) => new WebReinforcement(barDiameter, barSpacing, steel, 0, 0, null, width);
        
        /// <summary>
        /// Return a <see cref="WebReinforcement"/> with <see cref="DirectionX"/> only..
        /// </summary>
        /// <param name="barDiameter">The bar diameter for X direction.</param>
        /// <param name="barSpacing">The bar spacing for X direction.</param>
        /// <param name="steel">The steel objects for X direction.</param>
        /// <param name="width">The width of cross-section.</param>
        public static WebReinforcement HorizontalOnly(Length barDiameter, Length barSpacing, Steel steel, Length width) => new WebReinforcement(barDiameter, barSpacing, steel, Length.Zero, Length.Zero, null, width);

        /// <summary>
        /// Return a <see cref="WebReinforcement"/> with <see cref="DirectionY"/> only..
        /// </summary>
        /// <param name="barDiameter">The bar diameter (in mm) for Y direction.</param>
        /// <param name="barSpacing">The bar spacing (in mm) for  Y direction.</param>
        /// <param name="steel">The steel objects for Y direction.</param>
        /// <param name="width">The width (in mm) of cross-section.</param>
        public static WebReinforcement TransversalOnly(double barDiameter, double barSpacing, Steel steel, double width) => new WebReinforcement(0, 0, null, barDiameter, barSpacing, steel, width);
        
        /// <summary>
        /// Return a <see cref="WebReinforcement"/> with <see cref="DirectionY"/> only..
        /// </summary>
        /// <param name="barDiameter">The bar diameter for Y direction.</param>
        /// <param name="barSpacing">The bar spacing for  Y direction.</param>
        /// <param name="steel">The steel objects for Y direction.</param>
        /// <param name="width">The width of cross-section.</param>
        public static WebReinforcement TransversalOnly(Length barDiameter, Length barSpacing, Steel steel, Length width) => new WebReinforcement(Length.Zero, Length.Zero, null, barDiameter, barSpacing, steel, width);

		public override string ToString()
		{
			return
				"Reinforcement (x): " + "\n" +
				(DirectionX?.ToString() ?? "null") + "\n\n" +
				"Reinforcement (y): " + "\n" +
				(DirectionY?.ToString() ?? "null");
		}

        /// <summary>
        /// Compare two reinforcement objects.
        /// <para>Returns true if parameters are equal.</para>
        /// </summary>
        /// <param name="other">The other reinforcement object.</param>
        public virtual bool Equals(WebReinforcement other) => !(other is null) && (DirectionX == other.DirectionX && DirectionY == other.DirectionY);

        public override bool Equals(object other) => other is WebReinforcement reinforcement && Equals(reinforcement);

        public override int GetHashCode() => DirectionX?.GetHashCode()?? 1 * DirectionY?.GetHashCode() ?? 1 * (int)Width;

		/// <summary>
		/// Returns true if steel parameters are equal.
		/// </summary>
		public static bool operator == (WebReinforcement left, WebReinforcement right) => !(left is null) && left.Equals(right);

		/// <summary>
		/// Returns true if steel parameters are different.
		/// </summary>
		public static bool operator != (WebReinforcement left, WebReinforcement right) => !(left is null) && !left.Equals(right);

    }
}
