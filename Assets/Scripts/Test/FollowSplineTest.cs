using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Transform))]
public class FollowSplineTest : MonoBehaviour
{
    public SplineContainer splineContainer;
    public float speed = 50f;
    public bool loop = true;
    public bool snapToStart = true;
    public bool rotateAlongSpline = true;

    private Spline spline;
    private float splineLength;
    [Range(0f, 1f)] public float t = 0f;

    void Start()
    {
        if (splineContainer == null)
        {
            Debug.LogError("Assegna un SplineContainer.");
            enabled = false;
            return;
        }

        spline = splineContainer.Spline;
        if (spline == null || spline.Count < 2)
        {
            Debug.LogError("Spline non valida.");
            enabled = false;
            return;
        }

        splineLength = spline.GetLength();
        if (splineLength <= Mathf.Epsilon)
        {
            Debug.LogError("Spline ha lunghezza zero.");
            enabled = false;
            return;
        }

        if (snapToStart)
        {
            Vector3 startPos = splineContainer.transform.TransformPoint(spline.EvaluatePosition(t));
            transform.position = startPos;

            if (rotateAlongSpline)
            {
                Vector3 tangent = splineContainer.transform.TransformDirection(spline.EvaluateTangent(t)).normalized;
                transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
            }
        }
    }

    void Update()
    {
        if (spline == null) return;

        t += (speed * Time.deltaTime) / splineLength;

        if (loop)
            t %= 1f;
        else
            t = Mathf.Clamp01(t);

        // posizione in world space
        Vector3 targetPos = splineContainer.transform.TransformPoint(spline.EvaluatePosition(t));
        transform.position = targetPos;

        if (rotateAlongSpline)
        {
            Vector3 tangent = splineContainer.transform.TransformDirection(spline.EvaluateTangent(t)).normalized;
            transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
        }
    }
}
