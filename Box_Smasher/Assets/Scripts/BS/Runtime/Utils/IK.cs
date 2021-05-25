using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BS.Utils{
    public class IK : MonoBehaviour
    {
        public static Vector3[] Reach(Vector3 head, Vector3 tail, Vector3 dest){
            float currentLen = Vector2.Distance(head, tail);
            float stretchedLen = Vector2.Distance(dest, tail);
            Vector3 dir = (dest - tail).normalized;

            float lenDiff = (stretchedLen - currentLen);

            Vector3[] result = new Vector3[2];
            result[0] = dest; // new head pos
            result[1] = tail + (dir * lenDiff); // new tail pos

            return result;
        }
    }
}

