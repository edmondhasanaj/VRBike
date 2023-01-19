using System;
using UnityEngine;

namespace Engine.City
{
    /// <summary>
    /// Represents a 3D Connector(Crossroad) that is used to connect 
    /// 2 chunks together. This is part of the chunk data, and it is only
    /// used in editor and saved in the database. It is not used in the real 
    /// game
    /// </summary>
    [Serializable]
    public struct ChunkConnector
    {
        /// <summary>
        /// Returns the side the connector faces.
        /// </summary>
        /// <value>The face.</value>
        public Vector2Int Face{ get { return face; }}
        [SerializeField] private Vector2Int face;

        /// <summary>
        /// The connector position relative to the chunk. 
        /// </summary>
        public Vector2Int Position{ get { return position; } }
        [SerializeField] private Vector2Int position;

        /// <summary>
        /// Connector width in Unit Size
        /// </summary>
        public int Width { get { return width; } }
        [SerializeField] private int width;


        public ChunkConnector(Vector2Int face, Vector2Int connectorPosition, int connectorWidth)
        {
            this.face = face;
            this.position = connectorPosition;
            this.width = connectorWidth;
        }

        /// <summary>
        /// Clones this connector. No references are maintained.
        /// </summary>
        /// <returns>The clone.</returns>
        public ChunkConnector Clone()
        {
            return new ChunkConnector(face, position, width);
        }
    }
    
}
