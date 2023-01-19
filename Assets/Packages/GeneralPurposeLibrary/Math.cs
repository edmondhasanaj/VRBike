using System;
using UnityEngine;

namespace GPL
{
    /// <summary>
    /// Provides a lot of 2D Functions that can simplify 
    /// the development process a lot
    /// </summary>
    public class Math2D
    {
        /// <summary>
        /// Returns a unit vector perpendicular to the line.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="direction">1 means to the right, -1 means to the left</param>
        /// <returns>The perpendicular vector</returns>
        public static Vector2 PerpendicularUnitVector(Vector2 line, int direction)
        {
            return (new Vector2(line.y, -line.x) * direction).normalized;
        }

        /// <summary>
        /// Returns the unsigned offset from a point to a line.
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineDirection"></param>
        /// <param name="point"></param>
        /// <seealso cref="OffsetFromLine(Vector2, Vector2, Vector2)"/>
        /// <returns>Offset(shortest distance) from point to line</returns>
        public static float UOffsetFromLine(Vector2 lineStart, Vector2 lineDirection, Vector2 point)
        {
            return Math3D.UOffsetFromLine(lineStart, lineDirection, point);
        }

        /// <summary>
        /// Returns the signed offset from a point to a line. If the point to the right, its positive, otherwise its negative
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineDirection"></param>
        /// <param name="point"></param>
        /// <seealso cref="UOffsetFromLine(Vector2, Vector2, Vector2)"/>
        /// <returns>Signed Offset(shortest distance) from point to line</returns>
        public static float OffsetFromLine(Vector2 lineStart, Vector2 lineDirection, Vector2 point)
        {
            //Calc the offset from the start position
            return Math3D.OffsetFromLine(new Vector3(lineStart.x, 0, lineStart.y), new Vector3(lineDirection.x, 0, lineDirection.y), new Vector3(point.x, 0f, point.y));
        }

        /// <summary>
        /// Calculates the angle between 2 Vectors.
        /// The angle ∈ [-180, 180]. If `rhs` is to the right of `lhs` the angle is negative, 
        /// otherwise positive
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <seealso cref="Angle360(Vector2, Vector2)"/>
        /// <seealso cref="UAngle(Vector2, Vector2)"/>
        /// <returns>Angle</returns>
        public static float Angle(Vector2 lhs, Vector2 rhs)
        {
            return Vector2.SignedAngle(lhs, rhs);
        }

        /// <summary>
        /// Returns the angle between 2 Vecotrs.
        /// The angle is always positive, and is in the format [0, 360]
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <seealso cref="Angle(Vector2, Vector2)"/>
        /// <seealso cref="UAngle(Vector2, Vector2)"/>
        /// <returns>Angle</returns>
        public static float Angle360(Vector2 lhs, Vector2 rhs)
        {
            float angle = Angle(lhs, rhs);
            if (angle < 0)
                angle = 360 + angle;

            return angle;
        }

        /// <summary>
        /// Returns the unsigned angle between 2 Vectors.
        /// The angle is between [0, 180] and is always positive.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param> 
        /// <seealso cref="Angle(Vector2, Vector2)"/>
        /// <seealso cref="Angle360(Vector2, Vector2)"/>
        /// <returns>Unsigned Angle</returns>
        public static float UAngle(Vector2 lhs, Vector2 rhs)
        {
            return Vector2.Angle(lhs, rhs);
        }

        /// <summary>
        /// Returns true if a polygon contains a given point.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon">Set of vertices(`Vector2D`) that make the polygon</param>
        /// <returns>True if the given point is within polygon</returns>
        public static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            int j = polygon.Length - 1;
            var inside = false;
            for (int i = 0; i < polygon.Length; j = i++)
            {
                if (((polygon[i].y <= point.y && point.y < polygon[j].y) || (polygon[j].y <= point.y && point.y < polygon[i].y)) &&
                   (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                    inside = !inside;
            }
            return inside;
        }

        /// <summary>
        /// Rotate a `vector` by an `angle`
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="angle">Angle in degree</param>
        /// <seealso cref="Rotate(Vector2, Vector2, float)"/>
        /// <returns>New rotated vector</returns>
        public static Vector2 Rotate(Vector2 vector, float angle)
        {
            float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
            float cos = Mathf.Cos(angle * Mathf.Deg2Rad);

            return new Vector2(vector.x * cos - vector.y * sin, vector.y * cos + vector.x * sin);
        }

        /// <summary>
        /// Rotate a `vector` by an `angle` around a given `pivot`.
        /// </summary>
        /// <param name="vector">Vector to rotate</param>
        /// <param name="pivot">The Pivot</param>
        /// <param name="angle"></param>
        /// <seealso cref="Rotate(Vector2, float)"/>
        /// <returns>New rotated vector</returns>
        public static Vector2 Rotate(Vector2 vector, Vector2 pivot, float angle)
        {
            //Calculate the new vector based on the reference point
            Vector2 newPoint = vector - pivot;

            return Rotate(newPoint, angle) + pivot;
        }

        /// <summary>
        /// Returns a vector which represents the intersection point between 2 rays.
        /// If there is no intersection, the returned point is null.
        /// </summary>
        /// <param name="rayAStart"></param>
        /// <param name="rayADir"></param>
        /// <param name="rayBStart"></param>
        /// <param name="rayBDir"></param>
        /// <seealso cref="SegmentSegmentIntersection(Vector2, Vector2, Vector2, Vector2)"/>
        /// <returns>The intersection(null if none)</returns>
        public static Vector2? RayRayIntersection(Vector2 rayAStart, Vector2 rayADir, Vector2 rayBStart, Vector2 rayBDir)
        {
            float dx = rayBStart.x - rayAStart.x;
            float dy = rayBStart.y - rayAStart.y;
            float det = rayBDir.x * rayADir.y - rayBDir.y * rayADir.x;
            float u = (dy * rayBDir.x - dx * rayBDir.y) / det;

            return det != 0 ? rayAStart + rayADir * u : (Vector2?)null;
        }

        /// <summary>
        /// Returns a vector which represents the intersection point between 2 segments(lines that have a start and an end).
        /// If there is no intersection, the returned point is null.
        /// </summary>
        /// <param name="segAStart"></param>
        /// <param name="segAEnd"></param>
        /// <param name="segBStart"></param>
        /// <param name="segBEnd"></param>
        /// <seealso cref="RayRayIntersection(Vector2, Vector2, Vector2, Vector2)"/>
        /// <returns>The intersection(null if none)</returns>
        public static Vector2? SegmentSegmentIntersection(Vector2 segAStart, Vector2 segAEnd, Vector2 segBStart, Vector2 segBEnd) 
        {
            Vector2 rayADir = (segAEnd - segAStart).normalized;
            Vector2 rayBDir = (segBEnd - segBStart).normalized;

            Vector2? rayIntersection = RayRayIntersection(segAStart, rayADir, segBStart, rayBDir);
            if (rayIntersection == null) return null;

            //Check if its within the segments
            return (PointWithinSegment(segAStart, segAEnd, rayIntersection.Value) && PointWithinSegment(segBStart, segBEnd, rayIntersection.Value)) ? rayIntersection.Value : (Vector2?)null;
        }

        /// <summary>
        /// Returns true if a given `point` is within a segment' limits. The point can also be parallel, the only 
        /// restriction is that it needs to be within bounds.
        /// </summary>
        /// <param name="segStart"></param>
        /// <param name="segEnd"></param>
        /// <param name="point"></param>
        /// <returns>True if the `point` is within the segment, false otherwise</returns>
        public static bool PointWithinSegment(Vector2 segStart, Vector2 segEnd, Vector2 point)
        {
            float dist = Vector2.Distance(segStart, segEnd);
            float dot = Vector2.Dot((segEnd - segStart).normalized, point - segStart);
            return dot > 0f && dot <= dist;
        }

        /// <summary>
        /// Fires a `ray` at a single collider and checks if ray hits ist or not.
        /// 
        /// Utilizes an empty Layer(`default: 31`) to shoot the ray only against
        /// the provided collider.
        /// </summary>
        /// <param name="collider">The collider to test ray against</param>
        /// <param name="ray">Ray to shoot</param>
        /// <param name="hitInfo">Information of Hit(if it hit indeed)</param>
        /// <param name="maxDistance">Ray distance</param>
        /// <param name="tempLayer">The temp Layer to run all the calculations in(default is the last layer)</param>
        /// 
        /// \warning    The `tmpLayer` must be empty(no object assigned to that layer). This function will temporarily
        ///             place the collider in that layer to reduce calculation time
        ///
        /// <exception cref="ArgumentException">When `tmpLayer` is not empty, and the ray hits an object(other than our collider) with this layer, this exceptio nis fired</exception>
        /// <returns>True if hit, false otherwise</returns>
        public static bool Raycast(Collider2D collider, Ray2D ray, out RaycastHit2D hitInfo, float maxDistance, int tempLayer=31)
        {
            var oriLayer = collider.gameObject.layer;
            collider.gameObject.layer = tempLayer;
            hitInfo = Physics2D.Raycast(ray.origin, ray.direction, maxDistance, 1 << tempLayer);
            collider.gameObject.layer = oriLayer;

            if (hitInfo.collider && hitInfo.collider != collider)
            {
                throw new ArgumentException("The provided tmpLayer is already occupied by some other object");
            }

            return hitInfo.collider != null;
        }

        /// <summary>
        /// Maps a point to a line(by displacing it perpendiculary)
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="direction"></param>
        /// <param name="point"></param>
        /// <param name="restrictToStart">Determines what should happen if the mapped point is out of the range(out of start of the line). 
        /// If true, it will be restricted(the mapped point can't go lower than start), otherwise not.
        /// </param>
        /// <seealso cref="MapPointToSegment(Vector2, Vector2, Vector2, bool)"/>
        /// <returns>The mapped point</returns>
        public static Vector2 MapPointToLine(Vector2 lineStart, Vector2 direction, Vector2 point, bool restrictToStart = false) {
            return Math3D.MapPointToLine(lineStart, direction, point, restrictToStart);
        }

        /// <summary>
        /// Maps a point to a segment(by displacing it perpendiculary)
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <param name="point"></param>
        /// <param name="restrictOutRange">Determines what should happen if the mapped point is out of the range(out of start/end of the segment). 
        /// If true, it will be restricted(the mapped point can't go lower than start nor higher than end), otherwise not.
        /// </param>
        /// <seealso cref="MapPointToLine(Vector2, Vector2, Vector2, bool)"/>
        /// <returns>The mapped point</returns>
        public static Vector2 MapPointToSegment(Vector2 lineStart, Vector2 lineEnd, Vector2 point, bool restrictOutRange = false)
        {
            return Math3D.MapPointToSegment(lineStart, lineEnd, point, restrictOutRange);
        }

        /// <summary>
        /// Get the signed distance between 2 points in a line.
        /// If `pointB` is further from the `lineStart` than `pointA`, returns positive result, otherwise negative one
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="direction">Direction of the line. NORMALIZED</param>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <returns>The signed distance</returns>
        public static float SignedDistance(Vector2 lineStart, Vector2 direction, Vector2 pointA, Vector2 pointB)
        {
            Vector2 a2b = pointB - pointA;
            return Vector2.Distance(pointA, pointB) * (a2b.normalized == direction ? 1 : -1f);
        }

        /// <summary>
        /// Splits the given force in the X(right) and Y(up) components, by providing 
        /// the local right of the plan.
        /// (Used a lot for local force components)
        /// </summary>
        /// <param name="force"></param>
        /// <param name="rightForce"></param>
        /// <param name="upForce"></param>
        /// <param name="relativeRight">The right direction of the plan. (Global : (1|0))</param>
        /// <returns>The vector2 containing each component</returns>
        public static Vector2 SplitForce(Vector2 force, Vector2 relativeRight)
        {
            float angle = Math2D.Angle(relativeRight, force) * Mathf.Deg2Rad;
            float forceM = force.magnitude;
            return new Vector2(Mathf.Cos(angle) * forceM, Mathf.Sin(angle) * forceM);
        }
    }

    /// <summary>
    /// Provides a lot of 3D Functions that can simplify 
    /// the development process a lot
    /// </summary>
    public class Math3D
    {
        /// <summary>
        /// Maps a point to a line(by displacing it perpendiculary)
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="direction"></param>
        /// <param name="point"></param>
        /// <param name="restrictToStart">Determines what should happen if the mapped point is out of the range(out of start of the line). 
        /// If true, it will be restricted(the mapped point can't go lower than start), otherwise not.
        /// </param>
        /// <seealso cref="MapPointToSegment(Vector3, Vector3, Vector3, bool)"/>
        /// <returns>The mapped point</returns>
        public static Vector3 MapPointToLine(Vector3 lineStart, Vector3 direction, Vector3 point, bool restrictToStart = false)
        {
            //Normalize direction
            direction = direction.normalized;

            //Calc the offset from the start position
            float offset = Vector3.Dot(direction, (point - lineStart));

            //Check if we should restric or not
            return lineStart + (offset >= 0 || restrictToStart == false ? offset * direction : Vector3.zero);
        }

        /// <summary>
        /// Maps a point to a segment(by displacing it perpendiculary)
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <param name="point"></param>
        /// <param name="restrictOutRange">Determines what should happen if the mapped point is out of the range(out of start/end of the segment). 
        /// If true, it will be restricted(the mapped point can't go lower than start nor higher than end), otherwise not.
        /// </param>
        /// <seealso cref="MapPointToLine(Vector3, Vector3, Vector3, bool)"/>
        /// <returns>The mapped point</returns>
        public static Vector3 MapPointToSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point, bool restrictOutRange = false)
        {
            //Direction
            Vector3 direction = (lineEnd - lineStart);

            //Normalized Direction
            Vector3 normDir = direction.normalized;

            //Calc the offset from the start position
            float offset = Vector3.Dot(normDir, (point - lineStart));

            //Check if we should restric or not
            if (restrictOutRange == false || (offset >= 0 && offset <= direction.magnitude))
            {
                //If there is no restriction, or the point is within the 2 points, dont restrict.
                return lineStart + normDir * offset;
            }
            else
            {
                //If we must restrict, check if offset is negative or bigger than the whole line and return
                //startPoint or endPoint respectively ... 
                if (offset < 0)
                    return lineStart;
                else if (offset > direction.magnitude)
                    return lineEnd;
                else
                    //This should never be executed as it is checked before
                    return lineStart + normDir * offset;
            }
        }

        /// <summary>
        /// Returns the unsigned offset from a point to a line.
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineDirection"></param>
        /// <param name="point"></param>
        /// <seealso cref="OffsetFromLine(Vector3, Vector3, Vector3)"/>
        /// <returns>Offset(shortest distance) from point to line</returns>
        public static float UOffsetFromLine(Vector3 lineStart, Vector3 lineDirection, Vector3 point)
        {
            //Direction
            Vector3 direction = lineDirection.normalized;

            //Calc the offset from the start position
            return Vector3.Cross(direction, (point - lineStart)).magnitude;
        }

        /// <summary>
        /// Returns the signed offset from a point to a line. If the point to the right, its positive, otherwise its negative
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineDirection"></param>
        /// <param name="point"></param>
        /// <seealso cref="UOffsetFromLine(Vector3, Vector3, Vector3)"/>
        /// <returns>Signed Offset(shortest distance) from point to line</returns>
        public static float OffsetFromLine(Vector3 lineStart, Vector3 lineDirection, Vector3 point)
        {
            //Direction
            Vector3 direction = lineDirection.normalized;

            //Calc the offset from the start position
            Vector3 cross = Vector3.Cross(direction, (point - lineStart));
            return Mathf.Sign(cross.y) * cross.magnitude;
        }

        /// <summary>
        /// Calculates the angle between 2 Vectors.
        /// The angle ∈ [-180, 180]. If `rhs` is to the right of `lhs` the angle is positive, 
        /// otherwise negative
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="planeNormal">The normal of the plane</param>
        /// <seealso cref="Angle360(Vector3, Vector3)"/>
        /// <seealso cref="UAngle(Vector3, Vector3)"/>
        /// <returns>Angle</returns>
        public static float Angle(Vector3 lhs, Vector3 rhs, Vector3 planeNormal)
        {
            return Vector3.SignedAngle(lhs, rhs, planeNormal);
        }

        /// <summary>
        /// Returns the unsigned angle between 2 Vectors.
        /// The angle is between [0, 180] and is always positive.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param> 
        /// <seealso cref="Angle(Vector3, Vector3)"/>
        /// <seealso cref="Angle360(Vector3, Vector3)"/>
        /// <returns>Unsigned Angle</returns>
        public static float UAngle(Vector3 lhs, Vector3 rhs)
        {
            return Vector3.Angle(lhs, rhs);
        }

        /// <summary>
        /// Rotates a point around all axes(3D).
        /// </summary>
        /// <param name="point"></param>
        /// <param name="angle"></param>
        /// <seealso cref="RotateX(Vector3, float)"/>
        /// <seealso cref="RotateY(Vector3, float)"/>
        /// <seealso cref="RotateZ(Vector3, float)"/>
        /// <returns>The new rotated point</returns>
        public static Vector3 Rotate(Vector3 point, Vector3 angle)
        {
            Vector3 newPointX = RotateX(point, angle.x);
            Vector3 newPointY = RotateY(newPointX, angle.y);
            return RotateZ(newPointY, angle.z);
        }

        /// <summary>
        /// Rotates a point around X Axis.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="angle"></param>
        /// <seealso cref="Rotate(Vector3, Vector3)"/>
        /// <seealso cref="RotateY(Vector3, float)"/>
        /// <seealso cref="RotateZ(Vector3, float)"/>
        /// <returns>The new rotated point</returns>
        public static Vector3 RotateX(Vector3 point, float angle)
        {
            Vector2 newPoint = Math2D.Rotate(new Vector2(point.y, point.z), angle);
            return new Vector3(point.x, newPoint.x, newPoint.y);
        }

        /// <summary>
        /// Rotates a point around Y Axis.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="angle"></param>
        /// <seealso cref="Rotate(Vector3, Vector3)"/>
        /// <seealso cref="RotateX(Vector3, float)"/>
        /// <seealso cref="RotateZ(Vector3, float)"/>
        /// <returns>The new rotated point</returns>
        public static Vector3 RotateY(Vector3 point, float angle)
        {
            Vector2 newPoint = Math2D.Rotate(new Vector2(point.x, point.z), angle);
            return new Vector3(newPoint.x, point.y, newPoint.y);
        }

        /// <summary>
        /// Rotates a point around Z Axis.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="angle"></param>
        /// <seealso cref="Rotate(Vector3, Vector3)"/>
        /// <seealso cref="RotateX(Vector3, float)"/>
        /// <seealso cref="RotateY(Vector3, float)"/>
        /// <returns>The new rotated point</returns>
        public static Vector3 RotateZ(Vector3 point, float angle)
        {
            Vector2 newPoint = Math2D.Rotate(new Vector2(point.x, point.y), angle);
            return new Vector3(newPoint.x, newPoint.y, point.z);
        }
    }
}
