using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using GPL;

namespace Engine.City
{
    /// <summary>
    /// The general manager that controls the whole process 
    /// of the terrain. Must be attached on a ChunkDatabase GO.
    /// </summary>
    public class ProceduralTerrain : SerializedMonoBehaviour { 

        /// <summary>
        /// The size of each chunk. This is the same as the database
        /// </summary>
        public Vector2Int ChunkSize { get { return chunkDB.ChunkGridSize; } }

        /// <summary>
        /// The real size of each chunk. This is the same as the database
        /// </summary>
        public Vector2Int ChunkRealSize { get { return chunkDB.ChunkRealSize; } }

        /// <summary>
        /// The map of all the chunks
        /// </summary>
        private ChunkMap chunkMap;

        /// <summary>
        /// The real instantiated terrain data
        /// </summary>
        private Dictionary<Vector2Int, TerrainChunk> terrainMap;

        /// <summary>
        /// The database
        /// </summary>
        [SerializeField, Required] private ChunkDatabase chunkDB;

        /// <summary>
        /// The trigger of the terrain generation
        /// </summary>
        [SerializeField, Required] private ITerrainTrigger terrainTrigger;

        /// <summary>
        /// The amount of blocks 
        /// </summary>
        [SerializeField] private int blockRadius;

        /// <summary>
        /// The parent for all the map GameObjects
        /// </summary>
        [SerializeField, Required] private Transform mapParent;

        /// <summary>
        /// Specifies the central chunk. Used to correctly position the bike
        /// </summary>
        [SerializeField, Required] private ChunkData centralChunk;

        /// <summary>
        /// The current active chunks in the terrain
        /// </summary>
        private HashSet<Vector2Int> activeChunks;

        /// <summary>
        /// Not needed chunks. Subset of active chunks each frame
        /// </summary>
        private List<Vector2Int> unneededChunks;

        /// <summary>
        /// A map to calculate the matches when a new chunk wants to be spawned
        /// </summary>
        private List<Vector2Int> filledSlots;

        /// <summary>
        /// The chunks that match all the neighbours in all directions
        /// </summary>
        private List<CompatChunkInfo> matchingAllChunks;


        private void Awake()
        {
            terrainMap = new Dictionary<Vector2Int, TerrainChunk>();
            activeChunks = new HashSet<Vector2Int>();
            unneededChunks = new List<Vector2Int>();
            filledSlots = new List<Vector2Int>();
            matchingAllChunks = new List<CompatChunkInfo>();
        }

        private void Start()
        {
            chunkMap = new ChunkMap(chunkDB.GetAllChunks());
            chunkMap.DebugData();

            //Spawn default chunk
            SpawnChunk(Vector2Int.zero, centralChunk, 0f);
        }

        private void Update()
        {
            Vector3 playerPos = terrainTrigger.GetPosition();
            if (playerPos.x < 0)
                playerPos.x -= ChunkSize.x;
            if (playerPos.z < 0)
                playerPos.z -= ChunkSize.y;

            Vector2Int playerGridPos = new Vector2Int(Mathf.CeilToInt(playerPos.x / ChunkRealSize.x) - 1, Mathf.CeilToInt(playerPos.z / ChunkRealSize.y) - 1);
            ComputeTerrain(playerGridPos);
        }

        /// <summary>
        /// Based on the trigger position, this generates the terrain around
        /// </summary>
        /// <param name="terrainTriggerPos"></param>
        private void ComputeTerrain(Vector2Int terrainTriggerPos)
        {
            Vector2Int trExtent = terrainTriggerPos + new Vector2Int(blockRadius, blockRadius);
            Vector2Int blExtent = terrainTriggerPos - new Vector2Int(blockRadius, blockRadius);

            //Clear the unneeded chunks so that we can start fresh
            unneededChunks.Clear();

            //Check which of the current activated chunks need to be deactivated
            foreach(Vector2Int pos in activeChunks)
            {
                if (!IsWithinBounds(pos, blExtent, trExtent))
                    unneededChunks.Add(pos);
            }

            //Delete from the active chunks
            foreach(Vector2Int toBeDeleted in unneededChunks)
            {
                activeChunks.Remove(toBeDeleted);
            }

            //Loop through all the new positions and add them to the active map (if they are not yet added)
            for(int x = blExtent.x; x <= trExtent.x; x++)
            {
                for(int y = blExtent.y; y <= trExtent.y; y++)
                {
                    Vector2Int chunkPos = new Vector2Int(x, y);
                    activeChunks.Add(chunkPos);
                }
            }

            ApplyRenderChanges();
        }

        /// <summary>
        /// Apply the rendering changes on a single frame
        /// </summary>
        private void ApplyRenderChanges()
        {
            //Disable the unneeded chunks at this point
            DisableChunks(unneededChunks);

            //Activate all the new chunks
            foreach (Vector2Int c in activeChunks)
            {
                //If there is no chunk in here, create one
                if(!terrainMap.ContainsKey(c))
                    CreateChunk(c);

                terrainMap[c].Enable();
            }
        }

        /// <summary>
        /// Returns true if a position in grid is within the bounds
        /// </summary>
        /// <param name="v"></param>
        /// <param name="blExtent"></param>
        /// <param name="trExtent"></param>
        /// <returns></returns>
        private bool IsWithinBounds(Vector2Int v, Vector2Int blExtent, Vector2Int trExtent)
        {
            return (v.x >= blExtent.x && v.x <= trExtent.x) && (v.y >= blExtent.y && v.y <= trExtent.y);
        }

        /// <summary>
        /// Disables all the chunks provided
        /// </summary>
        private void DisableChunks(List<Vector2Int> chunks)
        {
            //Debug.Log("Disabled chunks : " + chunks.Count);
            for (int i = 0; i < chunks.Count; i++)
            {
                terrainMap[chunks[i]].Disable();
            }
        }

        #region Chunk Spawn
        /// <summary>
        /// Creates a chunk in the provided position by making sure that it fits with all 
        /// the neighbours around.
        /// </summary>
        /// <param name="pos"></param>
        private void CreateChunk(Vector2Int pos)
        {
            filledSlots.Clear();

            //Find the neighbouring slots that already have chunks
            Vector2Int neighbourDir = Vector2Int.up;
            for (int i = 0; i < 4; i++)
            {
                //Check for the neighbour in the calculated direction
                TerrainChunk neighbour = terrainMap.ContainsKey(pos + neighbourDir) ? terrainMap[pos + neighbourDir] : null;

                if (neighbour != null)
                    filledSlots.Add(neighbourDir);

                neighbourDir = Vector2Int.RoundToInt(Math2D.Rotate(neighbourDir, 90));
            }

            //If there are no neighbours, just spawn something random
            if (filledSlots.Count == 0)
            {
                //Random chunk
                ChunkData[] allChunks = chunkDB.GetAllChunks();
                ChunkData tmpChunk = allChunks[UnityEngine.Random.Range(0, allChunks.Length)];
                SpawnChunk(pos, tmpChunk, 0f);
            }
            //Otherwise spawn something that fits with all the neighbours
            else
            {
                SpawnFittingChunk(pos, filledSlots);
            }
        }

        /// <summary>
        /// Given an empty slot position and a list of neighbour directions,
        /// this function calculates all the chunks that can fit in this slot.
        /// Also spawns a correct chunk.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="directions"></param>
        private void SpawnFittingChunk(Vector2Int pos, List<Vector2Int> directions)
        {
            if (directions == null || directions.Count == 0)
                throw new ArgumentException("The directions list must not be empty");

            //Debug.Log("Spawning chunk at " + pos);
            //For the first direction, get the possible slots that match
            //with that neighbour
            Vector2Int first_neighbour_dir = filledSlots[0];
            TerrainChunk first_neighbour_terrain = terrainMap[pos + first_neighbour_dir];
            Vector2Int first_neighbour_out = new Vector2Int(-first_neighbour_dir.x, -first_neighbour_dir.y);
            float addedRotation = first_neighbour_terrain.Rotation;
            Vector2Int first_neighbour_match_dir = Vector2Int.RoundToInt(Math2D.Rotate(first_neighbour_out, -addedRotation));

            //This is the list of all the chunks that match the first neighbour
            List<CompatChunkInfo> matchingChunks = null;
            chunkMap.GetCompatChunk(first_neighbour_terrain.ChunkData, first_neighbour_match_dir, ref matchingChunks);
            //Debug.Log("Possible matches for " + first_neighbour_dir + " neighbour : " + matchingChunks.Count);

            //Clear the chunks 
            matchingAllChunks.Clear();

            //Filter the chunks and see if they match with all neighbours
            for (int i = 0; i < matchingChunks.Count; i++)
            {
                //For each found chunk, add the extra rotation of the first neighbour to adapt it to our situation
                CompatChunkInfo chunkInfo = new CompatChunkInfo(matchingChunks[i].Chunk, (matchingChunks[i].Rotation + addedRotation).RestrictRotation());

                //Debug.Log(chunkInfo.Chunk.Model.name + " at rotation " + chunkInfo.Rotation + " is being checked");

                //Add by default to the set
                matchingAllChunks.Add(chunkInfo);

                //Debug.Log("Checking chunk [Neighbour=" + directions[0] + "; Name=" + chunkInfo.Chunk.Model.name + "; Rotation=" + chunkInfo.Rotation + "; Match Dir=" + first_neighbour_match_dir + "]");
                for (int j = 1; j < directions.Count; j++)
                {
                    //For each other neighbour, find whether the current chunk also fits
                    TerrainChunk neighbour_terrain = terrainMap[pos + directions[j]];
                    Vector2Int neighbour_out = new Vector2Int(-directions[j].x, -directions[j].y);
                    Vector2Int neighbour_match_dir = Vector2Int.RoundToInt(Math2D.Rotate(neighbour_out, -neighbour_terrain.Rotation));
                    CompatChunkInfo tmpChunkInfo = new CompatChunkInfo(chunkInfo.Chunk, (chunkInfo.Rotation - neighbour_terrain.Rotation).RestrictRotation());

                    //Debug.Log("Checking chunk [Neighbour=" + directions[j] + "; Name=" + tmpChunkInfo.Chunk.Model.name + "; Rotation=" + tmpChunkInfo.Rotation + "; Match Dir=" + neighbour_match_dir + "]");

                    //If it doesn't fit, just remove it from the set
                    if (!chunkMap.IsCompatible(neighbour_terrain.ChunkData, neighbour_match_dir, tmpChunkInfo))
                    {
                        //Debug.Log("Chunk at " + directions[j] + " doesn't fit with " + chunkInfo.Chunk.Model.name + " with rotation " + chunkInfo.Rotation);
                        matchingAllChunks.RemoveAt(matchingAllChunks.Count - 1);
                        break;
                    }
                    else
                    {
                        //Debug.Log("Fits");
                    }
                }
            }

            //If nowthing was found, throw an exception
            if (matchingAllChunks.Count == 0)
                throw new Exception("At " + pos + " no chunk was able to be spawned");

            //Spawn a random chunk now
            int randomIndex = UnityEngine.Random.Range(0, matchingAllChunks.Count);
            //Debug.Log("Spawning chunk now " + matchingAllChunks[randomIndex].Chunk + " | " + matchingChunks[randomIndex].Rotation);
            SpawnChunk(pos, matchingAllChunks[randomIndex].Chunk, matchingAllChunks[randomIndex].Rotation);
        }

        /// <summary>
        /// Spawns a Terrain chunk with the provided details
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="cd"></param>
        /// <param name="rotation"></param>
        private void SpawnChunk(Vector2Int pos, ChunkData cd, float rotation)
        {
            TerrainChunk newChunk = new TerrainChunk(pos, rotation, cd, mapParent);
            newChunk.Disable();

            terrainMap.Add(pos, newChunk);
        }
        #endregion
    }
}
