using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Game.Scripts.Core.Components
{

    /// <summary>
    /// Moves this object along a cubic Bezier defined by 4 Transforms.
    /// Supports editor preview (back-and-forth oscillation).
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class BezierFollower : MonoBehaviour
    {
    [BoxGroup("Bezier Points"), Required] public Transform P0; // Start
    [BoxGroup("Bezier Points"), Required] public Transform P1; // Control 1
    [BoxGroup("Bezier Points"), Required] public Transform P2; // Control 2
    [BoxGroup("Bezier Points"), Required] public Transform P3; // End

    [BoxGroup("Motion"), Range(0f, 1f), ShowIf("@!Preview")]
    [LabelText("Progress (t)")]
    public float t = 0f;

    [BoxGroup("Motion"), LabelText("Speed (cycles/sec)")]
    [ShowIf("Preview")]
    public float speed = 0.5f;

    [BoxGroup("Motion"), LabelText("Preview in Editor")]
    [ToggleLeft] public bool Preview = true;

    [BoxGroup("Gizmo"), LabelText("Resolution")] [Range(2, 128)]
    public int resolution = 32;

    [BoxGroup("Gizmo"), LabelText("Draw Curve")]
    public bool drawCurve = true;

    void Update()
    {
        if (P0 == null || P1 == null || P2 == null || P3 == null) return;

        if (Application.isPlaying)
        {
            MoveToPosition(t);
        }
#if UNITY_EDITOR
        else if (Preview)
        {
            float osc = Mathf.PingPong((float)EditorApplication.timeSinceStartup * speed, 1f);
            MoveToPosition(osc);
        }
        else
        {
            MoveToPosition(t);
        }
#endif
    }

    void MoveToPosition(float tValue)
    {
        Vector3 pos = EvaluateCubicBezier(tValue, P0.position, P1.position, P2.position, P3.position);
        transform.position = pos;
    }

    public static Vector3 EvaluateCubicBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1f - t;
        return u * u * u * p0
             + 3f * u * u * t * p1
             + 3f * u * t * t * p2
             + t * t * t * p3;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!drawCurve || P0 == null || P1 == null || P2 == null || P3 == null) return;

        Gizmos.color = Color.cyan;
        Vector3 prev = P0.position;
        for (int i = 1; i <= resolution; i++)
        {
            float tt = i / (float)resolution;
            Vector3 pt = EvaluateCubicBezier(tt, P0.position, P1.position, P2.position, P3.position);
            Gizmos.DrawLine(prev, pt);
            prev = pt;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(P0.position, P1.position);
        Gizmos.DrawLine(P2.position, P3.position);
    }
#endif
    }

}