# Material
**Concrete and steel implementation for nonlinear analysis.**

Models for uniaxial and biaxial state of stresses.

Code parameters implemented for concrete: [*fib* Model Code 2010](https://www.fib-international.org/publications/fib-bulletins/model-code-2010-final-draft,-volume-1-detail.html) and [ABNT NBR 6118:2014](https://www.abntcatalogo.com.br/norma.aspx?ID=317027).

Concrete behavior implemented: [MCFT by Vecchio and Collins (1986)](https://www.concrete.org/publications/internationalconcreteabstractsportal.aspx?m=details&ID=10416) and [DSFM by Vecchio (2000)](https://ascelibrary.org/doi/10.1061/(ASCE)0733-9445(2000)126%3A9(1070)).

Steel behavior: Bilinear.

This library uses:

- [MathNet.Numerics](https://github.com/mathnet/mathnet-numerics) for Linear Algebra operations;

- [Units.NET](https://github.com/angularsen/UnitsNet) for simple unit conversions;

- [OnPlaneComponents](https://github.com/andrefmello91/On-Plane-Components) for stress and strain transformations;

- [Extensions](https://github.com/andrefmello91/Extensions) for some numeric extensions.
