using System;
using andrefmello91.Material.Concrete;
using andrefmello91.OnPlaneComponents.Strain;
using andrefmello91.OnPlaneComponents.Stress;
using Extensions;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnitsNet;
using UnitsNet.Units;
using static andrefmello91.Material.Reinforcement.Uniaxial.UniaxialReinforcement;

#nullable enable

namespace andrefmello91.Material.Reinforcement
{
	/// <summary>
	///     Web reinforcement class.
	/// </summary>
	public class WebReinforcement : IUnitConvertible<WebReinforcement, LengthUnit>, IApproachable<WebReinforcement, Length>, IComparable<WebReinforcement>, IEquatable<WebReinforcement>, ICloneable<WebReinforcement>
	{
		#region Fields

		private Length _width;

		#endregion

		#region Properties

		public LengthUnit Unit
		{
			get => DirectionX?.Unit ?? DirectionY?.Unit ?? Width.Unit;
			set => ChangeUnit(value);
		}

		// Properties
		/// <summary>
		///     Get the <see cref="WebReinforcementDirection" /> on X direction.
		/// </summary>
		public WebReinforcementDirection? DirectionX { get; }

		/// <summary>
		///     Get the <see cref="WebReinforcementDirection" /> on Y direction.
		/// </summary>
		public WebReinforcementDirection? DirectionY { get; }

		/// <summary>
		///     Get initial <see cref="WebReinforcement" /> stiffness <see cref="Matrix" />.
		/// </summary>
		/// <inheritdoc cref="BiaxialConcrete.InitialStiffness"/>
		public Matrix<double> InitialStiffness
		{
			get
			{
				// Steel matrix
				var Ds = Matrix<double>.Build.Dense(3, 3);

				Ds[0, 0] = DirectionX?.InitialStiffness.Megapascals ?? 0;
				Ds[1, 1] = DirectionY?.InitialStiffness.Megapascals ?? 0;

				if ((DirectionX is null || DirectionX.IsHorizontal) && (DirectionY is null || DirectionY.IsVertical))
					return Ds;

				// Transform
				var t = StrainRelations.TransformationMatrix(DirectionX?.Angle ?? DirectionY?.Angle - Constants.PiOver2 ?? 0);

				return
					t.Transpose() * Ds * t;
			}
		}

		/// <summary>
		///     Get current <see cref="WebReinforcement" /> stiffness <see cref="Matrix" /> with elements in
		///     <see cref="PressureUnit.Megapascal" />.
		/// </summary>
		public Matrix<double> Stiffness
		{
			get
			{
				// Steel matrix
				var Ds = Matrix<double>.Build.Dense(3, 3);

				Ds[0, 0] = DirectionX?.Stiffness.Megapascals ?? 0;
				Ds[1, 1] = DirectionY?.Stiffness.Megapascals ?? 0;

				if ((DirectionX is null || DirectionX.IsHorizontal) && (DirectionY is null || DirectionY.IsVertical))
					return Ds;

				// Transform
				var t = StrainRelations.TransformationMatrix(DirectionX?.Angle ?? DirectionY?.Angle - Constants.PiOver2 ?? 0);

				return
					t.Transpose() * Ds * t;
			}
		}

		/// <summary>
		///     Get/set reinforcement <see cref="StrainState" />, at horizontal plane.
		/// </summary>
		public StrainState Strains { get; private set; }

		/// <summary>
		///     Get reinforcement <see cref="StressState" />, transformed to horizontal plane, in MPa.
		/// </summary>
		public StressState Stresses
		{
			get
			{
				Pressure
					fsx = DirectionX?.Stress ?? Pressure.Zero,
					fsy = DirectionY?.Stress ?? Pressure.Zero;

				var stresses = new StressState(fsx, fsy, Pressure.Zero);

				if ((DirectionX is null || DirectionX.IsHorizontal) && (DirectionY is null || DirectionY.IsVertical))
					return stresses;

				return
					stresses.Transform(-DirectionX?.Angle ?? 0);
			}
		}

		/// <summary>
		///     Get cross-section width.
		/// </summary>
		public Length Width
		{
			get => DirectionX?.Width ?? _width;
			set
			{
				if (!(DirectionX is null))
					DirectionX.Width = value;

				if (!(DirectionY is null))
					DirectionY.Width = value;
			}
		}

		/// <summary>
		///     Returns true if reinforcement <see cref="DirectionX" /> exists.
		/// </summary>
		public bool XReinforced  => !(DirectionX is null) && DirectionX.BarDiameter > Length.Zero && DirectionX.BarSpacing > Length.Zero;

		/// <summary>
		///     Returns true if reinforcement <see cref="DirectionX" /> and <see cref="DirectionY" /> exist.
		/// </summary>
		public bool XYReinforced => XReinforced && YReinforced;

		/// <summary>
		///     Returns true if reinforcement <see cref="DirectionY" /> exists.
		/// </summary>
		public bool YReinforced  => !(DirectionY is null) && DirectionY.BarDiameter > Length.Zero && DirectionY.BarSpacing > Length.Zero;

		#endregion

		#region Constructors

		/// <inheritdoc cref="WebReinforcement(Length, Length, Steel, Length, double)" />
		/// <param name="unit">
		///     The <see cref="LengthUnit" /> of <paramref name="barDiameter" />, <paramref name="barSpacing" /> and
		///     <paramref name="width" />.
		/// </param>
		public WebReinforcement(double barDiameter, double barSpacing, Steel steel, double width, double angleX = 0, LengthUnit unit = LengthUnit.Millimeter)
			: this(WebReinforcementDirection.Read(barDiameter, barSpacing, steel, width, angleX, unit), WebReinforcementDirection.Read(barDiameter, barSpacing, steel.Clone(), width, angleX + Constants.PiOver2, unit), width)
		{
		}

		/// <summary>
		///     Create a web reinforcement, with equal X and Y directions.
		/// </summary>
		/// <inheritdoc cref="WebReinforcement(WebReinforcementDirection, WebReinforcementDirection, Length)" />
		/// <param name="barDiameter">The bar diameter for directions X and Y.</param>
		/// <param name="barSpacing">The bar spacing for directions X and Y.</param>
		/// <param name="steel">The steel objects for directions X and Y.</param>
		/// <param name="width">The width of cross-section.</param>
		/// <param name="angleX">
		///     The angle (in radians) of <see cref="DirectionX" />, related to horizontal axis.
		///     <para><paramref name="angleX" /> is positive if counterclockwise.</para>
		/// </param>
		public WebReinforcement(Length barDiameter, Length barSpacing, Steel steel, Length width, double angleX = 0)
			: this(WebReinforcementDirection.Read(barDiameter, barSpacing, steel, width, angleX), WebReinforcementDirection.Read(barDiameter, barSpacing, steel.Clone(), width, angleX + Constants.PiOver2), width)
		{
		}

		/// <inheritdoc cref="WebReinforcement(WebReinforcementDirection, WebReinforcementDirection, Length)" />
		/// <inheritdoc cref="WebReinforcement(double, double, Steel, double, double, LengthUnit)" />
		public WebReinforcement(double barDiameterX, double barSpacingX, Steel steelX, double barDiameterY, double barSpacingY, Steel steelY, double width, double angleX = 0, LengthUnit unit = LengthUnit.Millimeter)
			: this(WebReinforcementDirection.Read(barDiameterX, barSpacingX, steelX, width, angleX, unit), WebReinforcementDirection.Read(barDiameterY, barSpacingY, steelY, width, angleX + Constants.PiOver2, unit), Length.From(width, unit))
		{
		}

		/// <inheritdoc cref="WebReinforcement(WebReinforcementDirection, WebReinforcementDirection, Length)" />
		/// <inheritdoc cref="WebReinforcement(Length, Length, Steel, Length, double)" />
		/// <param name="barDiameterX">The bar diameter for X direction.</param>
		/// <param name="barSpacingX">The bar spacing for X direction.</param>
		/// <param name="steelX">The steel objects for X direction.</param>
		/// <param name="barDiameterY">The bar diameter for Y direction.</param>
		/// <param name="barSpacingY">The bar spacing for Y direction.</param>
		/// <param name="steelY">The steel object for Y direction (not the same <paramref name="steelX" /> object).</param>
		public WebReinforcement(Length barDiameterX, Length barSpacingX, Steel steelX, Length barDiameterY, Length barSpacingY, Steel steelY, Length width, double angleX = 0)
			: this (WebReinforcementDirection.Read(barDiameterX, barSpacingX, steelX,  width, angleX), WebReinforcementDirection.Read(barDiameterY, barSpacingY, steelY, width, angleX + Constants.PiOver2), width)
		{
		}

		/// <inheritdoc cref="WebReinforcement(WebReinforcementDirection, WebReinforcementDirection, Length)" />
		/// <inheritdoc cref="WebReinforcement(double, double, Steel, double, double, LengthUnit)" />
		public WebReinforcement(WebReinforcementDirection? directionX, WebReinforcementDirection? directionY, double width, LengthUnit unit = LengthUnit.Millimeter)
			: this (directionX, directionY, Length.From(width, unit))
		{
		}

		/// <summary>
		///     Create a web reinforcement, with different X and Y directions.
		/// </summary>
		/// <remarks>
		///     Two reinforcing bars are considered at cross-section, one in each lateral face of structural element.
		/// </remarks>
		/// <param name="directionX">
		///     The <see cref="WebReinforcementDirection" /> of X direction. Can be null if there is no
		///     reinforcement in this direction.
		/// </param>
		/// <param name="directionY">
		///     The <see cref="WebReinforcementDirection" /> of Y direction. Can be null if there is no
		///     reinforcement in this direction.
		/// </param>
		/// <param name="width">The width of cross-section.</param>
		public WebReinforcement(WebReinforcementDirection? directionX, WebReinforcementDirection? directionY, Length width)
		{
			DirectionX = directionX;
			DirectionY = directionY;
			_width     = width;

			if (!(DirectionX is null) && DirectionX.Width != width)
				DirectionX.Width = width;

			if (!(DirectionY is null) && DirectionY.Width != width)
				DirectionY.Width = width;
		}

		#endregion

		#region  Methods

		/// <param name="unit">
		///     The <see cref="LengthUnit" /> of <paramref name="barDiameter" />, <paramref name="barSpacing" /> and
		///     <paramref name="width" />.
		/// </param>
		/// <inheritdoc cref="DirectionXOnly(Length,Length,Steel,Length,double)" />
		public static WebReinforcement DirectionXOnly(double barDiameter, double barSpacing, Steel steel, double width, double angleX = 0, LengthUnit unit = LengthUnit.Millimeter) => new WebReinforcement(new WebReinforcementDirection(barDiameter, barSpacing, steel, width, angleX, unit), null, width, unit);

		/// <summary>
		///     Get a <see cref="WebReinforcement" /> with <see cref="DirectionX" /> only.
		/// </summary>
		/// <param name="barDiameter">The bar diameter for X direction.</param>
		/// <param name="barSpacing">The bar spacing for X direction.</param>
		/// <param name="steel">The steel objects for X direction.</param>
		/// <inheritdoc cref="WebReinforcement(Length, Length, Steel, Length, double)" />
		public static WebReinforcement DirectionXOnly(Length barDiameter, Length barSpacing, Steel steel, Length width, double angleX = 0) => new WebReinforcement(new WebReinforcementDirection(barDiameter, barSpacing, steel, width, angleX), null, width);

		/// <param name="unit">
		///     The <see cref="LengthUnit" /> of <paramref name="barDiameter" />, <paramref name="barSpacing" /> and
		///     <paramref name="width" />.
		/// </param>
		/// <inheritdoc cref="DirectionYOnly(Length,Length,Steel,Length,double)" />
		public static WebReinforcement DirectionYOnly(double barDiameter, double barSpacing, Steel steel, double width, double angle = Constants.PiOver2, LengthUnit unit = LengthUnit.Millimeter) => new WebReinforcement(null, new WebReinforcementDirection(barDiameter, barSpacing, steel, width, angle, unit), width, unit);

		/// <summary>
		///     Get a <see cref="WebReinforcement" /> with <see cref="DirectionY" /> only.
		/// </summary>
		/// <param name="barDiameter">The bar diameter for Y direction.</param>
		/// <param name="barSpacing">The bar spacing for  Y direction.</param>
		/// <param name="steel">The steel objects for Y direction.</param>
		/// <param name="angle">
		///     The angle (in radians) of <see cref="DirectionY" />, related to horizontal axis.
		///     <para><paramref name="angle" /> is positive if counterclockwise.</para>
		/// </param>
		/// <inheritdoc cref="WebReinforcement(Length, Length, Steel, Length, double)" />
		public static WebReinforcement DirectionYOnly(Length barDiameter, Length barSpacing, Steel steel, Length width, double angle = Constants.PiOver2) => new WebReinforcement(null, new WebReinforcementDirection(barDiameter, barSpacing, steel, width, angle), width);

		/// <summary>
		///     Calculate angles (in radians) related to crack angle.
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
		///     Calculate current <see cref="StressState" />, in MPa.
		/// </summary>
		/// <param name="strainsState">Current <see cref="StrainState" />.</param>
		public void CalculateStresses(StrainState strainsState)
		{
			// Set strains
			Strains = strainsState.Clone();

			// Transform directions and calculate stresses in steel
			SetStrainsAndStresses(StrainState.Transform(Strains, DirectionX?.Angle ?? 0));
		}

		/// <summary>
		///     Set steel <see cref="StrainState" />.
		/// </summary>
		/// <param name="strainsState">Current <see cref="StrainState" />.</param>
		public void SetStrains(StrainState strainsState)
		{
			DirectionX?.Steel?.SetStrain(strainsState.EpsilonX);
			DirectionY?.Steel?.SetStrain(strainsState.EpsilonY);
		}

		/// <summary>
		///     Set steel <see cref="StressState" />, given <see cref="StrainState" />.
		/// </summary>
		/// <param name="strainsState">Current <see cref="StrainState" />.</param>
		public void SetStresses(StrainState strainsState)
		{
			DirectionX?.Steel?.SetStress(strainsState.EpsilonX);
			DirectionY?.Steel?.SetStress(strainsState.EpsilonY);
		}

		/// <summary>
		///     Set steel <see cref="StrainState" /> and calculate <see cref="StressState" />, in MPa.
		/// </summary>
		/// <param name="strainsState">Current <see cref="StrainState" />.</param>
		public void SetStrainsAndStresses(StrainState strainsState)
		{
			SetStrains(strainsState);
			SetStresses(strainsState);
		}

		/// <summary>
		///     Calculate tension stiffening coefficient (for DSFM).
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
					phiX  = DirectionX.BarDiameter.Millimeters,
					cosNx = thetaNx.Cos(true);

				den += psx / phiX * cosNx;
			}

			if (!(DirectionY is null))
			{
				double
					psy   = DirectionY.Ratio,
					phiY  = DirectionY.BarDiameter.Millimeters,
					cosNy = thetaNy.Cos(true);

				den += psy / phiY * cosNy;
			}

			// Return m
			return
				0.25 / den;
		}

		/// <summary>
		///     Calculate maximum value of principal tensile strength (fc1) that can be transmitted across cracks.
		/// </summary>
		/// <param name="theta1">Principal tensile strain angle, in radians.</param>
		public Pressure MaximumPrincipalTensileStress(double theta1)
		{
			if (DirectionX is null && DirectionY is null)
				return Pressure.Zero;

			// Get reinforcement angles and stresses
			var (thetaNx, thetaNy) = Angles(theta1);

			Pressure
				fcx = DirectionX?.CapacityReserve ?? Pressure.Zero,
				fcy = DirectionY?.CapacityReserve ?? Pressure.Zero;

			double
				cosNx = thetaNx.Cos(true),
				cosNy = thetaNy.Cos(true);

			// Check the maximum value of fc1 that can be transmitted across cracks
			return
				fcx * cosNx * cosNx + fcy * cosNy * cosNy;
		}

		public WebReinforcement Clone() => new WebReinforcement(DirectionX?.Clone(), DirectionY?.Clone(), Width);

		public bool Approaches(WebReinforcement? other, Length tolerance) => !(other is null) && (DirectionX?.Approaches(other.DirectionX, tolerance) ?? false) && (DirectionY?.Approaches(other.DirectionY, tolerance) ?? false);

		public void ChangeUnit(LengthUnit unit)
		{
			if (Unit == unit)
				return;

			if (!(DirectionX is null))
				DirectionX.Unit = unit;

			if (!(DirectionY is null))
				DirectionY.Unit = unit;

			_width = _width.ToUnit(unit);
		}

		public WebReinforcement Convert(LengthUnit unit) => new WebReinforcement(DirectionX?.Convert(unit), DirectionY?.Convert(unit), _width.ToUnit(unit));

		public int CompareTo(WebReinforcement? other)
		{
			if (other is null)
				return 1;

			int
				x = DirectionX?.CompareTo(other?.DirectionX) ?? -1,
				y = DirectionY?.CompareTo(other?.DirectionY) ?? -1;

			return x switch
			{
				1             =>  1,
				0 when y == 0 =>  0,
				0 when y > 0  =>  1,
				_             => -1
			};
		}

		/// <summary>
		///     Compare two reinforcement objects.
		///     <para>Returns true if parameters are equal.</para>
		/// </summary>
		/// <param name="other">The other reinforcement object.</param>
		public virtual bool Equals(WebReinforcement? other) => Approaches(other, Tolerance);

		public override string ToString() =>
			"Reinforcement (x):\n" +
			$"{DirectionX?.ToString() ?? "null"}\n\n" +
			"Reinforcement (y):\n" +
			$"{DirectionY?.ToString() ?? "null"}";

		public override bool Equals(object? other) => other is WebReinforcement reinforcement && Equals(reinforcement);

		public override int GetHashCode() => DirectionX?.GetHashCode() ?? 1 * DirectionY?.GetHashCode() ?? 1 * (int) Width.Millimeters;

		#endregion

		#region Operators

		/// <summary>
		///     Returns true if steel parameters are equal.
		/// </summary>
		public static bool operator == (WebReinforcement left, WebReinforcement right) => !(left is null) && left.Equals(right);

		/// <summary>
		///     Returns true if steel parameters are different.
		/// </summary>
		public static bool operator != (WebReinforcement left, WebReinforcement right) => !(left is null) && !left.Equals(right);

		#endregion
	}
}