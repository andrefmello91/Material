using System;
using System.Runtime.CompilerServices;
using System.Xml;
using Extensions.Number;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnitsNet;
using UnitsNet.Units;
using OnPlaneComponents;
using static OnPlaneComponents.StrainRelations;

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
        /// Get/set reinforcement <see cref="StrainState"/>, at horizontal plane.
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
		/// Get reinforcement <see cref="StressState"/>, transformed to horizontal plane, in MPa.
		/// </summary>
		public StressState Stresses
		{
			get
			{
				double
					fsx = DirectionX?.Stress ?? 0,
					fsy = DirectionY?.Stress ?? 0;

				var stresses = new StressState(fsx, fsy, 0);

				if ((DirectionX is null || DirectionX.IsHorizontal) && (DirectionY is null || DirectionY.IsVertical))
					return stresses;

                return
					StressState.Transform(stresses, -DirectionX?.Angle ?? 0);
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

				if ((DirectionX is null || DirectionX.IsHorizontal) && (DirectionY is null || DirectionY.IsVertical))
					return Ds;

				// Transform
				var t = TransformationMatrix(DirectionX.Angle);

				return
					t.Transpose() * Ds * t;
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

				if ((DirectionX is null || DirectionX.IsHorizontal) && (DirectionY is null || DirectionY.IsVertical))
					return Ds;

				// Transform
				var t = TransformationMatrix(DirectionX.Angle);

				return
					t.Transpose() * Ds * t;
			}
        }

        /// <summary>
        /// Web reinforcement for biaxial calculations, for equal X and Y directions.
        /// </summary>
        /// <param name="barDiameter">The bar diameter (in mm) for directions X and Y.</param>
        /// <param name="barSpacing">The bar spacing (in mm) for directions X and Y.</param>
        /// <param name="steel">The steel objects for directions X and Y.</param>
        /// <param name="width">The width (in mm) of cross-section.</param>
        /// <param name="angleX">The angle (in radians) of <see cref="DirectionX"/>, related to horizontal axis.
        /// <para><paramref name="angleX"/> is positive if counterclockwise.</para></param>
        public WebReinforcement(double barDiameter, double barSpacing, Steel steel, double width, double angleX = 0)
            : this(WebReinforcementDirection.Read(barDiameter, barSpacing, steel, width, angleX), WebReinforcementDirection.Read(barDiameter, barSpacing, steel.Copy(), width, angleX + Constants.PiOver2), width)
        {
        }

        /// <summary>
        /// Web reinforcement for biaxial calculations, for equal X and Y directions.
        /// </summary>
        /// <param name="barDiameter">The bar diameter for directions X and Y.</param>
        /// <param name="barSpacing">The bar spacing for directions X and Y.</param>
        /// <param name="steel">The steel objects for directions X and Y.</param>
        /// <param name="width">The width of cross-section.</param>
        /// <param name="angleX">The angle (in radians) of <see cref="DirectionX"/>, related to horizontal axis.
        /// <para><paramref name="angleX"/> is positive if counterclockwise.</para></param>
        public WebReinforcement(Length barDiameter, Length barSpacing, Steel steel, Length width, double angleX = 0)
            : this(WebReinforcementDirection.Read(barDiameter, barSpacing, steel, width, angleX), WebReinforcementDirection.Read(barDiameter, barSpacing, steel.Copy(), width, angleX + Constants.PiOver2), width)
        {
        }

        /// <summary>
        /// Web reinforcement for biaxial calculations, for different X and Y directions.
        /// </summary>
        /// <param name="barDiameterX">The bar diameter (in mm) for X direction.</param>
        /// <param name="barSpacingX">The bar spacing (in mm) for X direction.</param>
        /// <param name="steelX">The steel objects for X direction.</param>
        /// <param name="barDiameterY">The bar diameter (in mm) for Y direction.</param>
        /// <param name="barSpacingY">The bar spacing (in mm) for Y direction.</param>
        /// <param name="steelY">The steel objects for Y direction.</param>
        /// <param name="width">The width (in mm) of cross-section.</param>
        /// <param name="angleX">The angle (in radians) of <see cref="DirectionX"/>, related to horizontal axis.
        /// <para><paramref name="angleX"/> is positive if counterclockwise.</para></param>
        public WebReinforcement(double barDiameterX, double barSpacingX, Steel steelX, double barDiameterY, double barSpacingY, Steel steelY, double width, double angleX = 0)
            : this(WebReinforcementDirection.Read(barDiameterX, barSpacingX, steelX, width, angleX), WebReinforcementDirection.Read(barDiameterY, barSpacingY, steelY, width, angleX + Constants.PiOver2), Length.FromMillimeters(width))
        {
        }

        /// <summary>
        /// Web reinforcement for biaxial calculations, for different X and Y directions.
        /// </summary>
        /// <param name="barDiameterX">The bar diameter for X direction.</param>
        /// <param name="barSpacingX">The bar spacing for X direction.</param>
        /// <param name="steelX">The steel objects for X direction.</param>
        /// <param name="barDiameterY">The bar diameter for Y direction.</param>
        /// <param name="barSpacingY">The bar spacing for Y direction.</param>
        /// <param name="steelY">The steel objects for Y direction.</param>
        /// <param name="width">The width of cross-section.</param>
        /// <param name="angleX">The angle (in radians) of <see cref="DirectionX"/>, related to horizontal axis.
        /// <para><paramref name="angleX"/> is positive if counterclockwise.</para></param>
        public WebReinforcement(Length barDiameterX, Length barSpacingX, Steel steelX, Length barDiameterY, Length barSpacingY, Steel steelY, Length width, double angleX = 0)
			: this (WebReinforcementDirection.Read(barDiameterX, barSpacingX, steelX,  width, angleX), WebReinforcementDirection.Read(barDiameterY, barSpacingY, steelY, width, angleX + Constants.PiOver2), width)
        {
        }

        /// <summary>
        /// Web reinforcement for biaxial calculations, for different X and Y directions.
        /// </summary>
        /// <param name="directionX">The <see cref="WebReinforcementDirection"/> of X direction</param>
        /// <param name="directionY"></param>
        /// <param name="width">The width of cross-section, in mm.</param>
        public WebReinforcement(WebReinforcementDirection directionX, WebReinforcementDirection directionY, double width)
			: this (directionX, directionY, Length.FromMillimeters(width))
        {
        }

        /// <summary>
        /// Web reinforcement for biaxial calculations, for different X and Y directions.
        /// </summary>
        /// <param name="directionX">The <see cref="WebReinforcementDirection"/> of X direction</param>
        /// <param name="directionY"></param>
        /// <param name="width">The width of cross-section.</param>
        public WebReinforcement(WebReinforcementDirection directionX, WebReinforcementDirection directionY, Length width)
        {
            DirectionX = directionX;
            DirectionY = directionY;
            _w = width;
        }


		/// <summary>
		/// Calculate angles (in radians) related to crack angle.
		/// </summary>
		/// <param name="theta1">Principal tensile strain angle, in radians.</param>
		public (double X, double Y) Angles(double theta1)
		{
			// Calculate angles
			double
				thetaNx = theta1 - (DirectionX?.Angle ?? 0),
				thetaNy = theta1 - (DirectionY?.Angle ?? Constants.PiOver2);

			return
				(thetaNx, thetaNy);
		}

		/// <summary>
		/// Calculate current <see cref="StressState"/>, in MPa.
		/// </summary>
		/// <param name="strainsState">Current <see cref="StrainState"/>.</param>
		public void CalculateStresses(StrainState strainsState)
		{
			// Set strains
			Strains = strainsState.Copy();
				
			// Transform directions and calculate stresses in steel
			SetStrainsAndStresses(StrainState.Transform(Strains, DirectionX?.Angle ?? 0));
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
					cosNx = thetaNx.Cos(true);

				den += psx / phiX * cosNx;
			}

			if (!(DirectionY is null))
			{
				double
					psy   = DirectionY.Ratio,
					phiY  = DirectionY.BarDiameter,
					cosNy = thetaNy.Cos(true);

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
				cosNx = thetaNx.Cos(true),
				cosNy = thetaNy.Cos(true);

			// Check the maximum value of fc1 that can be transmitted across cracks
			return
				fcx * cosNx * cosNx + fcy * cosNy * cosNy;
		}

        /// <summary>
        /// Return a copy of this <see cref="WebReinforcement"/>.
        /// </summary>
        public WebReinforcement Copy()
        {
	        if (DirectionX is null && DirectionY is null)
		        return null;

            if (DirectionX is null)
	            return DirectionYOnly(DirectionY.BarDiameter, DirectionY.BarSpacing, DirectionY.Steel.Copy(), DirectionY.Width, DirectionY.Angle);

            if (DirectionY is null)
	            return DirectionXOnly(DirectionX.BarDiameter, DirectionX.BarSpacing, DirectionX.Steel.Copy(), DirectionX.Width, DirectionX.Angle);

            return
				new WebReinforcement(DirectionX.Copy(), DirectionY.Copy(), Width);
		}

        /// <summary>
        /// Return a <see cref="WebReinforcement"/> with <see cref="DirectionX"/> only.
        /// </summary>
        /// <param name="barDiameter">The bar diameter (in mm) for X direction.</param>
        /// <param name="barSpacing">The bar spacing (in mm) for  X direction.</param>
        /// <param name="steel">The steel objects for X direction.</param>
        /// <param name="width">The width (in mm) of cross-section.</param>
        /// <param name="angle">The angle (in radians) of <see cref="DirectionX"/>, related to horizontal axis.
        /// <para><paramref name="angle"/> is positive if counterclockwise.</para></param>
        public static WebReinforcement DirectionXOnly(double barDiameter, double barSpacing, Steel steel, double width, double angle = 0) => new WebReinforcement(new WebReinforcementDirection(barDiameter, barSpacing, steel, width, angle), null, width);

        /// <summary>
        /// Return a <see cref="WebReinforcement"/> with <see cref="DirectionX"/> only..
        /// </summary>
        /// <param name="barDiameter">The bar diameter for X direction.</param>
        /// <param name="barSpacing">The bar spacing for X direction.</param>
        /// <param name="steel">The steel objects for X direction.</param>
        /// <param name="width">The width of cross-section.</param>
        /// <param name="angle">The angle (in radians) of <see cref="DirectionX"/>, related to horizontal axis.
        /// <para><paramref name="angle"/> is positive if counterclockwise.</para></param>
        public static WebReinforcement DirectionXOnly(Length barDiameter, Length barSpacing, Steel steel, Length width, double angle = 0) => new WebReinforcement(new WebReinforcementDirection(barDiameter, barSpacing, steel, width, angle), null, width);

        /// <summary>
        /// Return a <see cref="WebReinforcement"/> with <see cref="DirectionY"/> only..
        /// </summary>
        /// <param name="barDiameter">The bar diameter (in mm) for Y direction.</param>
        /// <param name="barSpacing">The bar spacing (in mm) for  Y direction.</param>
        /// <param name="steel">The steel objects for Y direction.</param>
        /// <param name="width">The width (in mm) of cross-section.</param>
        /// <param name="angle">The angle (in radians) of <see cref="DirectionY"/>, related to horizontal axis.
        /// <para><paramref name="angle"/> is positive if counterclockwise.</para></param>
        public static WebReinforcement DirectionYOnly(double barDiameter, double barSpacing, Steel steel, double width, double angle = Constants.PiOver2) => new WebReinforcement(null, new WebReinforcementDirection(barDiameter, barSpacing, steel, width, angle), width);

        /// <summary>
        /// Return a <see cref="WebReinforcement"/> with <see cref="DirectionY"/> only..
        /// </summary>
        /// <param name="barDiameter">The bar diameter for Y direction.</param>
        /// <param name="barSpacing">The bar spacing for  Y direction.</param>
        /// <param name="steel">The steel objects for Y direction.</param>
        /// <param name="width">The width of cross-section.</param>
        /// <param name="angle">The angle (in radians) of <see cref="DirectionY"/>, related to horizontal axis.
        /// <para><paramref name="angle"/> is positive if counterclockwise.</para></param>
        public static WebReinforcement DirectionYOnly(Length barDiameter, Length barSpacing, Steel steel, Length width, double angle = Constants.PiOver2) => new WebReinforcement(null, new WebReinforcementDirection(barDiameter, barSpacing, steel, width, angle), width);

		public override string ToString()
		{
			return
				"Reinforcement (x):\n" +
				$"{(DirectionX?.ToString() ?? "null")}\n\n" +
				"Reinforcement (y):\n" +
				$"{(DirectionY?.ToString() ?? "null")}";
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
