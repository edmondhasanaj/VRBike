using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Engine.City
{
    [Serializable]
    public class PlayerTrigger : MonoBehaviour, ITerrainTrigger
    {
        public Vector3 GetPosition()
        {
            //Return the actual player position
            return transform.position;
        }
    }
}
