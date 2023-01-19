using System;
using System.Collections.Generic;
using UnityEngine;


namespace Engine.City
{
    /// <summary>
    /// Represents the chunk info for compatible chunks
    /// </summary>
    public struct CompatChunkInfo
    {
        public ChunkData Chunk { get; private set; }
        public float Rotation { get; private set; }


        public CompatChunkInfo(ChunkData chunk, float rotation)
        {
            Chunk = chunk;
            Rotation = rotation.RestrictRotation();
        }

        public override bool Equals(object obj)
        {
            try
            {
                CompatChunkInfo other = (CompatChunkInfo)obj;
                return other.Chunk.Equals(Chunk) && other.Rotation == Rotation;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (Chunk.ToString() + Rotation.ToString()).GetHashCode();
        }
    }


    /// <summary>
    /// Represents the whole map of the chunks. Calculates which chunks fit together
    /// and which not, and based on that a database will be built. The data is not dynamic, 
    /// meaining that it is only calculated on start and can't be changed during runtime
    /// </summary>
    public class ChunkMap
    {
        /// <summary>
        /// The chunk map.
        /// Each chunk has in each side a list of compatible chunks
        /// </summary>
        private Dictionary<ChunkData, Dictionary<Vector2Int, List<CompatChunkInfo>>> map;


        public ChunkMap(ChunkData[] allChunks)
        {
            map = new Dictionary<ChunkData, Dictionary<Vector2Int, List<CompatChunkInfo>>>();
            CalculateMap(allChunks);
        }


        /// <summary>
        /// Given a set of chunks, this calculates the map for which chunk
        /// goes with which chunk
        /// </summary>
        private void CalculateMap(ChunkData[] allChunks)
        {
            //Loop through all chunks
            foreach (ChunkData cd1 in allChunks)
            {
                //Add to the compat table
                map.Add(cd1, new Dictionary<Vector2Int, List<CompatChunkInfo>>() {   { Vector2Int.up, new List<CompatChunkInfo>() },
                                                                                            { Vector2Int.right, new List<CompatChunkInfo>() },
                                                                                            { Vector2Int.down, new List<CompatChunkInfo>() },
                                                                                            { Vector2Int.left, new List<CompatChunkInfo>() } });

                //Fill the compat table in this loop
                foreach (ChunkData cd2 in allChunks)
                {
                    //For each cd1, check in all sides if this cd2 matches
                    TryAddMatch(cd1, cd2, Vector2Int.up);
                    TryAddMatch(cd1, cd2, Vector2Int.right);
                    TryAddMatch(cd1, cd2, Vector2Int.down);
                    TryAddMatch(cd1, cd2, Vector2Int.left);
                }
            }
        }

        /// <summary>
        /// Add a match into the compat table. When a solution to match cd2 to cd1 is found,
        /// this computes all the possible rotations and adds them as solutions.
        /// </summary>
        /// <param name="cd1"></param>
        /// <param name="cd2"></param>
        private void TryAddMatch(ChunkData cd1, ChunkData cd2, Vector2Int face)
        {
            float[] rot = null;
            if (TGAUtils.CheckChunkCompat(cd1, cd2, face, ref rot))
            {
                //If any solution was found, loop through all the possible rotations and add 
                //each one
                for (int i = 0; i < rot.Length; i++)
                {
                    //Debug.Log("Rotation[" + i + "] " + rot[i]);
                    map[cd1][face].Add(new CompatChunkInfo(cd2, rot[i]));
                }
            }
        }

        /// <summary>
        /// Get all compatible chunks for a given one, in a given direction. If there 
        /// are no chunks in that direction, returns false. Be careful, as the returned list
        /// is a direct reference to the real list and any changes(remove, add) will cause the main database to 
        /// be changed too.
        /// </summary>
        /// <param name="mainChunk"></param>
        /// <param name="face"></param>
        /// <param name="allCompatibleChunks"></param>
        public bool GetCompatChunk(ChunkData mainChunk, Vector2Int face, ref List<CompatChunkInfo> allCompatibleChunks)
        {
            if (!map.ContainsKey(mainChunk))
                throw new KeyNotFoundException("This chunk is not part of the map");

            if (!map[mainChunk].ContainsKey(face) || map[mainChunk][face].Count == 0)
                return false;

            allCompatibleChunks = map[mainChunk][face];
            return true;
        }

        /// <summary>
        /// Returns true if a `mainChunk` accepts the `checkChunk' in the provided `face`
        /// </summary>
        /// <param name="mainChunk"></param>
        /// <param name="face"></param>
        /// <param name="checkChunk"></param>
        /// <returns></returns>
        public bool IsCompatible(ChunkData mainChunk, Vector2Int face, CompatChunkInfo checkChunk)
        {
            if (!map.ContainsKey(mainChunk))
                throw new KeyNotFoundException("This chunk is not part of the map");

            /*if (map[mainChunk][face].Contains(checkChunk))
            {
                foreach (CompatChunkInfo cci in map[mainChunk][face])
                    Debug.Log(cci.Chunk.Model.name + "-" + cci.Rotation);
            }*/

            return map[mainChunk][face].Contains(checkChunk);
        }

        /// <summary>
        /// Debugs the map information by printing the data into the console
        /// </summary>
        public void DebugData()
        {
            string toPrint = "";
            foreach(KeyValuePair<ChunkData, Dictionary<Vector2Int, List<CompatChunkInfo>>> kvp in map)
            {
                toPrint += kvp.Key.Model.name + " - " + kvp.Value.Count;
                toPrint += "\n{\n";

                foreach(KeyValuePair<Vector2Int, List<CompatChunkInfo>> kvp2 in kvp.Value)
                {
                    toPrint += kvp2.Key;
                    toPrint += "-> ";

                    foreach(CompatChunkInfo cci in kvp2.Value)
                    {
                        toPrint += cci.Chunk.Model.name + " | " + cci.Rotation + ";";
                    }

                    toPrint += "\n";
                }


                toPrint += "\n}\n";
            }
        }
    }
}