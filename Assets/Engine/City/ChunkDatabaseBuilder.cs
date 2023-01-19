using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Engine.City
{
    [Serializable]
    public class ChunkDatabaseBuilder : MonoBehaviour
    {
        public ChunkDatabase chunkDB;

        public ChunkData cd;
        
        [OnInspectorGUI]
        void AddChunkData()
        {
            if (GUILayout.Button("Add"))
            {
                chunkDB.AddChunkData(cd);
                Debug.Log("Added");
            }
        }
    }
}
