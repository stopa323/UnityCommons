using UnityEngine;


namespace k323.Commons.NetworkActionSystem {
    /// <summary>
    /// Small utility to better understand action start and stop conclusion
    /// </summary>
    public static class ActionConclusion {
        public const bool Stop = false;
        public const bool Continue = true;
    }

    public static class ActionUtils {
        // cache Physics Cast hits, to minimize allocs.
        static RaycastHit[] hits = new RaycastHit[4];

        /// <summary>
        /// Given the coordinates of two entities, checks to see if there is an obstacle between them.
        /// (Since character coordinates are beneath the feet of the visual avatar, we add a small amount of height to
        /// these coordinates to simulate their eye-line.)
        /// </summary>
        /// <param name="position1">first character's position</param>
        /// <param name="position2">second character's position</param>
        /// <param name="missPosition">the point where an obstruction occurred (or if no obstruction, this is just position2)</param>
        /// <returns>true if no obstructions, false if there is a Ground-layer object in the way</returns>
        public static bool HasLineOfSight(Vector3 position1, Vector3 position2, out Vector3 missPos, LayerMask obstacleLayer) {
            position1 += Vector3.up;
            position2 += Vector3.up;
            var rayDirection = position2 - position1;
            var distance = rayDirection.magnitude;

            var numHits = Physics.RaycastNonAlloc(new Ray(position1, rayDirection), hits, distance, obstacleLayer);
            if (numHits == 0) {
                missPos = position2;
                return true;
            }
            else {
                missPos = hits[0].point;
                return false;
            }
        }
    }
}
