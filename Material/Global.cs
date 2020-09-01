using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace Material
{
	// Special characters
	public enum Characters
	{
		Alpha   = '\u03B1',
		Epsilon = '\u03B5',
		Gamma   = '\u03B3',
		Phi     = '\u00F8',
		Rho     = '\u03C1',
		Times   = '\u00D7'
	}

	/// <summary>
    /// Auxiliary class.
    /// </summary>
	public abstract class Relations
	{
        /// <summary>
        /// Verify if a number is zero (true for not zero)
        /// </summary>
        /// <param name="number">The number.</param>
        public bool NotZero(double number) => number != 0;

        /// <summary>
        /// Calculate the direction cosines of an angle (cos, sin).
        /// </summary>
        /// <param name="angle">Angle, in radians.</param>
        /// <param name="absoluteValue">Return absolute values? (default: false).</param>
        public (double cos, double sin) DirectionCosines(double angle, bool absoluteValue = false)
		{
			double
				cos = Trig.Cos(angle).CoerceZero(1E-6),
				sin = Trig.Sin(angle).CoerceZero(1E-6);

			if (!absoluteValue)
				return (cos, sin);

			return
				(Math.Abs(cos), Math.Abs(sin));
		}
	}
}
