using UnityEngine;
using GPL;
using System;
using System.Collections.Generic;

namespace Engine.City
{
    public static class TGAUtils
    {
        /// <summary>
        /// Restricts a rotation to be only between 0 and 360 degrees
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static float RestrictRotation(this float rotation)
        {
            if (rotation >= 360)
                rotation %= 360;
            else if (rotation < 0)
                rotation = 360 - ((-rotation) % 360);

            return rotation;
        }

        /// <summary>
        /// Checks if chunk1 is compatible with chunk 2 in the provided face. That is true, if we 
        /// can get chunk 2 to somehow align and fit in that position
        /// </summary>
        /// <returns><c>true</c>, if chunk compatibility was checked, <c>false</c> otherwise.</returns>
        /// <param name="chunk1">Chunk1.</param>
        /// <param name="chunk2">Chunk2.</param>
        /// <param name="chunk1Face">Chunk1 face.</param>
        /// <param name="chunk2Rotation">If the chunks are compatible, this returns the possible rotations for the 2nd chunk to fit in, otherwise null</param>
        public static bool CheckChunkCompat(ChunkData chunk1, ChunkData chunk2, Vector2Int chunk1Face, ref float[] chunk2Rotation)
        {
            if (chunk1.GridSize != chunk2.GridSize)
                throw new ArgumentException("The chunks must be of the same size");

            //The matching table
            //Face -> {Current matched number, All Connectors, Rotation of C2}
            Dictionary<Vector2Int, int[]> matches = new Dictionary<Vector2Int, int[]>() { { Vector2Int.up, new int[] { 0, 0, 0 } },
                                                                                                { Vector2Int.right, new int[] { 0, 0, 0 } },
                                                                                                { Vector2Int.down, new int[] { 0, 0, 0 } },
                                                                                                { Vector2Int.left, new int[] { 0, 0, 0 } } };

            //Fill the connector count for each side of c2
            foreach (ChunkConnector c2Con in chunk2.AllConnectors)
            {
                //Update the connector count in the matches table
                int[] tmpData = matches[c2Con.Face];
                matches[c2Con.Face] = new int[] { tmpData[0], tmpData[1] + 1, tmpData[2] };
            }

            //Get the connector count for the side of c1
            int c1ConCount = 0;
            foreach (ChunkConnector c1Con in chunk1.AllConnectors)
            {
                //Check if the connector faces the right side
                if (c1Con.Face != chunk1Face)
                    continue;

                c1ConCount++;
            }

            //Directly eleminate possible matches if the connector count doesn't add up
            if (c1ConCount != matches[Vector2Int.left][1])
                matches.Remove(Vector2Int.left);
            if (c1ConCount != matches[Vector2Int.up][1])
                matches.Remove(Vector2Int.up);
            if (c1ConCount != matches[Vector2Int.right][1])
                matches.Remove(Vector2Int.right);
            if (c1ConCount != matches[Vector2Int.down][1])
                matches.Remove(Vector2Int.down);

            //Loop through each connector that is 
            foreach (ChunkConnector c1Con in chunk1.AllConnectors)
            {
                //Check if the connector faces the right side
                if (c1Con.Face != chunk1Face)
                    continue;
                
                //For this connector, loop through all the connectors of the other chunk, to see if it matches anything
                foreach (ChunkConnector c2Con in chunk2.AllConnectors)
                {
                    if (!matches.ContainsKey(c2Con.Face))
                        continue;

                    //Check if this connector fits
                    float rotation = 0f;
                    if (CheckConnectorCompat(c1Con, c2Con, chunk1.GridSize, ref rotation))
                    {
                        //Update the matches
                        int[] tmpData = matches[c2Con.Face];
                        matches[c2Con.Face] = new int[] { tmpData[0] + 1, tmpData[1], (int)rotation };
                    }
                }
            }

            int successfullyMatches = 0;

            //Check if for the remaining possible matching sides, all the connectors were matched
            foreach (KeyValuePair<Vector2Int, int[]> kvp in matches)
            {
                //If no connector was left unmatched, it means this is a valid choice
                if (kvp.Value[0] == kvp.Value[1])
                {
                    successfullyMatches++;

                    //If the connector count was 0, it is a valid choice, but we need to calculate the rotation
                    if (kvp.Value[0] == 0)
                    {
                        //Flip the face by 180, and get the new required face
                        Vector2Int required = new Vector2Int(-chunk1Face.x, -chunk1Face.y);

                        //Get the difference
                        int rotation = (int)(Math2D.Angle360(kvp.Key, required));
                        kvp.Value[2] = rotation;
                    }
                }
            }

            if (successfullyMatches == 0)
                return false;

            //If code reaches here, there is at least an option to fit in
            chunk2Rotation = new float[successfullyMatches];
            int index = 0;
            foreach (KeyValuePair<Vector2Int, int[]> kvp in matches)
            {
                //If no connector was left unmatched, it means this is a valid choice
                if (kvp.Value[0] != kvp.Value[1])
                    continue;

                chunk2Rotation[index] = kvp.Value[2];
                index++;
            }

            return matches.Count > 0;
        }

        /// <summary>
        /// Checks if a connector c2 of a chunk, can be snapped into the connector c1 of another chunk,
        /// if we know the direction the c1 connector is facing. If a solution can be found, this returns true and the 
        /// rotation of c2 is set into reference
        /// </summary>
        /// <returns><c>true</c>, if connector compat was checked, <c>false</c> otherwise.</returns>
        /// <param name="c1">C1.</param>
        /// <param name="c2">C2.</param>
        /// <param name="cs">Cs.</param>
        /// <param name="rotation">Rotation.</param>
        private static bool CheckConnectorCompat(ChunkConnector c1, ChunkConnector c2, Vector2Int cs, ref float rotation)
        {
            Vector2 chunkCentre = (Vector2)cs / 2f;

            //Compute the 
            Vector2Int requiredC2Face = new Vector2Int(-c1.Face.x, -c1.Face.y);
            Vector2Int requiredC2Pos = Vector2Int.zero;

            if (c1.Face == Vector2Int.left)
                requiredC2Pos = new Vector2Int(cs.x - 1, c1.Position.y);
            else if (c1.Face == Vector2Int.up)
                requiredC2Pos = new Vector2Int(c1.Position.x, 0);
            else if (c1.Face == Vector2Int.right)
                requiredC2Pos = new Vector2Int(0, c1.Position.y);
            else if (c1.Face == Vector2Int.down)
                requiredC2Pos = new Vector2Int(c1.Position.x, cs.y - 1);
                
            //Rotation to align C2 to C1
            float c2_to_c1 = Math2D.Angle360(c2.Face, requiredC2Face);

            //The current position. For this we take the center of the call(that allows precise rotation calculation)
            Vector2 currentPos = c2.Position + Vector2.one * 0.5f;

            //Rotate C2 and check if it will then align with the required connector 
            Vector2 tmpPos = Math2D.Rotate(currentPos, chunkCentre, c2_to_c1);

            //Get the new position after rotation. For this, we Floor the input to the lowest int(after the float rotation)
            Vector2Int newC2Pos = new Vector2Int((int)tmpPos.x, (int)tmpPos.y);

            if (newC2Pos == requiredC2Pos)
            {
                rotation = c2_to_c1;
                return true;
            }

            return false;
        }
    }
}