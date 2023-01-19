using System;
using UnityEngine;

namespace Engine.City
{
    /// <summary>
    /// Represents a small chunk of the big terrain. This class only contains the data.
    /// There is another class that is used in the RunTime mode.
    /// </summary>
    [Serializable]
    public class ChunkData : IEquatable<ChunkData>
    {
        /// <summary>
        /// The gameobject that has all the buildings/structures/colliders.
        /// The pivot point of this should be the center position (RealSizeX/2, RealSizeY/2)
        /// </summary>
        public GameObject Model { get { return model; } }
        [SerializeField] private GameObject model;

        /// <summary>
        /// Chunk size in 3D Grid. (Example: A 100x100 units chunk, can be also divided into 10x10 Grid where each cell has 10x10 real units)
        /// </summary>
        public Vector2Int GridSize{ get { return gridSize; } }
        [SerializeField] private Vector2Int gridSize;

        /// <summary>
        /// The real 3D size of this chunk
        /// </summary>
        public Vector2Int RealSize { get { return realSize; } }
        [SerializeField] private Vector2Int realSize;
        
        /// <summary>
        /// All the connectors that connect this chunk to the others
        /// </summary>
        public ChunkConnector[] AllConnectors{ get { return allConnectors; } }
        [SerializeField] private ChunkConnector[] allConnectors;


        public ChunkData(GameObject terrainChunk, Vector2Int gridSize, Vector2Int realSize, ChunkConnector[] allConnectors)
        {
            this.model = terrainChunk;
            this.model.SetActive(false);

            this.gridSize = gridSize;
            this.realSize = realSize;
            this.allConnectors = allConnectors;
        }


        /// <summary>
        /// Creates a duplicate of this chunk data, by duplicating everything on it. 
        /// No references are maintained.
        /// </summary>
        /// <returns>The clone.</returns>
        public ChunkData Clone()
        {
            //Duplicate terrain chunk and size
            GameObject newTerrainChunk = GameObject.Instantiate(Model, Vector3.zero, Quaternion.identity);
            Vector2Int newChunkSize = GridSize;
            Vector2Int newRealSize = RealSize;

            //Duplicate now all the connectors
            ChunkConnector[] newConnectors = new ChunkConnector[AllConnectors.Length];
            for (int i = 0; i < AllConnectors.Length; i++)
            {
                newConnectors[i] = AllConnectors[i].Clone();
            }

            return new ChunkData(newTerrainChunk, newChunkSize, newRealSize, newConnectors);
        }

        /// <summary>
        /// Equals Implementation
        /// </summary>
        /// <returns>The to.</returns>
        /// <param name="other">Other.</param>
        public bool Equals(ChunkData other)
        {
            return other.model == model && other.gridSize == gridSize;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Engine.City.ChunkData"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return (Model != null ? Model.GetHashCode() : 0) * 10 + GridSize.GetHashCode();
        }
    }
}
