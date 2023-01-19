using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Linq;

namespace Engine.City
{
    public interface ITerrainTrigger
    {
        /// <summary>
        /// Returns the position of the reference object moving in the map.
        /// </summary>
        /// <returns></returns>
        Vector3 GetPosition();
    }
}
