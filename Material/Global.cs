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

	public abstract class Relations
	{
        /// <summary>
        /// Verify if a number is zero (true for not zero)
        /// </summary>
        /// <param name="num">The number.</param>
        /// <returns></returns>
        public bool NotZero(double num) => num != 0;

        /// <summary>
        /// Calculate the direction cosines of an angle (cos, sin)
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns></returns>
        public (double cos, double sin) DirectionCosines(double angle)
		{
			double
				cos = Trig.Cos(angle).CoerceZero(1E-6),
				sin = Trig.Sin(angle).CoerceZero(1E-6);

			return (cos, sin);
		}
	}
}
