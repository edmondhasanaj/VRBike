using System;
using UnityEngine;

namespace Engine.City
{
    /// <summary>
    /// Represents a real time instantiated terrain chunk
    /// </summary>
    public class TerrainChunk{

        /// <summary>
        /// The Grid position of this terrain chunk relative
        /// to the other chunks
        /// </summary>
        public readonly Vector2Int GridPos;

        /// <summary>
        /// The 3D chunk data to use for this piece.
        /// </summary>
        public readonly ChunkData ChunkData;

        /// <summary>
        /// Current Rotation around Y Axis [0, 270]
        /// </summary>
        public readonly float Rotation;

        /// <summary>
        /// The 3D Model
        /// </summary>
        private readonly GameObject chunkDataModel;

        /// <summary>
        /// The parent that all the 3D objects will have
        /// </summary>
        private readonly Transform goParent;


        public TerrainChunk(Vector2Int gridPos, float rotation, ChunkData chunkData, Transform parent)
        {
            //Get the position
            GridPos = gridPos;

            //Get the Info from the Chunk Data
            ChunkData = chunkData;

            //Get other properties
            goParent = parent;
            Rotation = rotation.RestrictRotation();

            chunkDataModel = GameObject.Instantiate(chunkData.Model);
            chunkDataModel.name = "Block [" + gridPos + "]";
            InitRender();
        }


        /// <summary>
        /// Init the rendering
        /// </summary>
        public void InitRender()
        {
            //Calculate the start position for this chunk
            Vector3 startPos = new Vector3(GridPos.x * ChunkData.RealSize.x + ChunkData.RealSize.x / 2f, 0, GridPos.y * ChunkData.RealSize.y + ChunkData.RealSize.y / 2f);

            chunkDataModel.transform.SetParent(goParent);
            chunkDataModel.transform.localScale = Vector3.one;
            chunkDataModel.transform.position = startPos;
            chunkDataModel.transform.rotation = Quaternion.Euler(0, -Rotation, 0); // -Rotation because Unity works with the opposite way

            Disable();
        }

        /// <summary>
        /// Enables the 3D chunk
        /// </summary>
        public void Enable()
        {
            chunkDataModel.SetActive(true);
        }

        /// <summary>
        /// Disables the 3D chunk
        /// </summary>
        public void Disable(){
            chunkDataModel.SetActive(false);
        }
    }
}
