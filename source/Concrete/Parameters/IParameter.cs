using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnPlaneComponents;
using UnitsNet;
using UnitsNet.Units;

namespace Material.Concrete
{
    /// <summary>
    /// Interface for concrete parameters.
    /// </summary>
    public interface IParameter : IUnitConvertible<IParameter, PressureUnit>, IUnitConvertible<IParameter, LengthUnit>
    {
        /// <summary>
        ///     Get/set the unit of concrete strength parameters.
        /// </summary>
        PressureUnit StressUnit { get; set; }

        /// <summary>
        ///     Get/set the unit of concrete aggregate diameter.
        /// </summary>
        LengthUnit DiameterUnit { get; set; }

        /// <summary>
        ///     Get the compressive strength of concrete (positive value).
        /// </summary>
        Pressure Strength { get; }

        /// <summary>
        ///     Get the type of concrete aggregate.
        /// </summary>
        AggregateType Type { get; }

        /// <summary>
        ///     Get/set maximum diameter of concrete aggregate.
        /// </summary>
        Length AggregateDiameter { get; }

        /// <summary>
        ///     Get tensile strength.
        /// </summary>
        Pressure TensileStrength { get; }

        /// <summary>
        ///     Get initial elastic module.
        /// </summary>
        Pressure ElasticModule { get; }

        /// <summary>
        ///     Get secant module.
        /// </summary>
        Pressure SecantModule { get; }

        /// <summary>
        ///     Get concrete plastic (peak) strain (negative value).
        /// </summary>
        double PlasticStrain { get; }

        /// <summary>
        ///     Get concrete ultimate strain (negative value).
        /// </summary>
        double UltimateStrain { get; }

        /// <summary>
        ///     Get concrete cracking strain.
        /// </summary>
        double CrackingStrain { get; }

        /// <summary>
        ///     Get transverse (shear) module.
        /// </summary>
        Pressure TransverseModule { get; }

        /// <summary>
        ///     Get fracture parameter, in N/mm.
        /// </summary>
        ForcePerLength FractureParameter { get; }
    }
}
