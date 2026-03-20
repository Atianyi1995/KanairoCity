using UnityEngine;
using System.Collections.Generic;

namespace KanairoCity.Systems.Traffic
{
    public class TrafficWaypoints : MonoBehaviour
    {
        public List<Transform> waypoints = new List<Transform>();
        public Color gizmoColor = Color.yellow;

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count < 2) return;
            Gizmos.color = gizmoColor;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (waypoints[i] && waypoints[i+1])
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
            }
        }
    }
}
