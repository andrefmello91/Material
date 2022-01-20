# Material
This library is part of the research published in [Structures Journal](https://authors.elsevier.com/a/1e1Wj8MoIGt99M).

It implements models for nonlinear analysis of concrete and reinforcement, for uniaxial and biaxial state of stresses.

Code parameters implemented for concrete: [*fib* Model Code 2010](https://www.fib-international.org/publications/fib-bulletins/model-code-2010-final-draft,-volume-1-detail.html) and [ABNT NBR 6118:2014](https://www.abntcatalogo.com.br/norma.aspx?ID=317027).

Concrete behavior implemented: [MCFT by Vecchio and Collins (1986)](https://doi.org/10.14359/12115), [DSFM by Vecchio (2000)](https://doi.org/10.1061/(ASCE)0733-9445(2000)126:9(1070)) and [SMM by Hsu and Zhu (2002)](https://doi.org/10.14359/12115).

Steel behavior: Bilinear.

This library uses:

- [MathNet.Numerics](https://github.com/mathnet/mathnet-numerics) for Linear Algebra operations;

- [Units.NET](https://github.com/angularsen/UnitsNet) for simple unit conversions;

- [OnPlaneComponents](https://github.com/andrefmello91/On-Plane-Components) for stress and strain transformations;

- [Extensions](https://github.com/andrefmello91/Extensions) for some numeric extensions.

## Usage

### Package reference:

`<PackageReference Include="andrefmello91.Material" Version="1.X.X" />`

### .NET CLI:

`dotnet add package andrefmello91.Material --version 1.X.X`
