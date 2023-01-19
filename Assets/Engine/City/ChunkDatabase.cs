using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Linq;

namespace Engine.City
{
    /// <summary>
    /// The database that contains all the chunks of the terrain. It is filled in editor,
    /// and used in run-time.
    /// </summary>
    [CreateAssetMenu(fileName = "ChunkDB", menuName = "City/ChunkDB", order = 1)]
    public class ChunkDatabase : SerializedScriptableObject 
    {
        /// <summary>
        /// The size of each chunk in the grid
        /// </summary>
        public Vector2Int ChunkGridSize { get { return chunkSize; } }
        [SerializeField] private Vector2Int chunkSize;

        /// <summary>
        /// The real 3D size of the chunk
        /// </summary>
        public Vector2Int ChunkRealSize { get { return chunkRealSize; } }
        [SerializeField] private Vector2Int chunkRealSize;

        /// <summary>
        /// The chunk database.
        /// </summary>
        [OdinSerialize, TableList] private HashSet<ChunkData> allChunks;


        /// <summary>
        /// Adds the chunk data.
        /// </summary>
        /// <param name="cd">Cd.</param>
        public void AddChunkData(ChunkData cd)
        {
            //If the Grid Size of the new Object is not hte same as the required one, throw Exception
            if (cd.GridSize != chunkSize || cd.RealSize != chunkRealSize)
                throw new ArgumentException("The chunk data must have the same size as defined in the procedural terrain manager");
            allChunks.Add(cd);
        }

        /// <summary>
        /// Removes the chunk data.
        /// </summary>
        ///  <param name="cd">Cd.</param>
        public void RemoveChunkData(ChunkData cd)
        {
            //If the Grid Size of the new Object is not hte same as the required one, throw Exception
            if (cd.GridSize != chunkSize || cd.RealSize != chunkRealSize)
                throw new ArgumentException("The chunk data must have the same size as defined in the procedural terrain manager");

            allChunks.Remove(cd);
        }

        /// <summary>
        /// Gets all the chunk data.
        /// </summary>
        /// <returns>The all chunk data.</returns>
        public ChunkData[] GetAllChunks()
        {
            return allChunks.ToArray();
        }
    }
}
