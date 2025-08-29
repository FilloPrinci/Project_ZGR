using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


// ============================
// Job definition
// ============================
public struct CPUJob : IJobParallelFor
{
    // =========================
    // Input/Output arrays
    // =========================
    public NativeArray<int> accelerate;
    public NativeArray<float> steer;
    [ReadOnly] public NativeArray<float> currentSpeed;
    [ReadOnly] public float maxSpeed;
    [ReadOnly] public float lookAheadForwardMultiplier;
    [ReadOnly] public float lookAheadRightMultiplier;
    [ReadOnly] public NativeArray<Vector3> positions;
    [ReadOnly] public NativeArray<Vector3> forwardDirections;
    [ReadOnly] public NativeArray<Vector3> rightDirections;
    [ReadOnly] public NativeArray<Vector3> raceLinePoints;

    // Track boundaries
    [ReadOnly] public NativeArray<Vector3> leftVertices;
    [ReadOnly] public NativeArray<int> leftTriangles;
    [ReadOnly] public NativeArray<Vector3> rightVertices;
    [ReadOnly] public NativeArray<int> rightTriangles;

    [ReadOnly] public Matrix4x4 leftLocalToWorld;
    [ReadOnly] public Matrix4x4 rightLocalToWorld;

    // Results for debug gizmos
    public NativeArray<Vector3> nearestLeft;
    public NativeArray<Vector3> nearestRight;
    public NativeArray<Vector3> nearestRaceLinePoint;

    // Distances thresholds
    public float limitDistance;
    public float safeDistance;

    // =========================
    // ENUM DEFINITIONS
    // =========================
    private enum VerticalZone { Behind, Center, Ahead }
    private enum HorizontalZone { Left, Center, Right }

    // =========================
    // MAIN JOB EXECUTION
    // =========================
    public void Execute(int index)
    {
        Vector3 pos = positions[index];

        // === Define car orientation
        Vector3 forward = forwardDirections[index];
        Vector3 right = rightDirections[index];

        // === Scale steering thresholds and intensity with speed ===
        float speed = currentSpeed[index];
        float speedFactor = Mathf.Clamp01(speed / maxSpeed);

        float currentSteer = steer[index];


        // Use a position slightly ahead of the car for better zone classification
        pos += (forward * (1 + (speedFactor * lookAheadForwardMultiplier))) + (right * currentSteer * lookAheadRightMultiplier * speedFactor);

        // === Compute closest points & squared distances ===
        float leftDist = ComputeClosestDistance(pos, leftVertices, leftTriangles, leftLocalToWorld, out Vector3 closestLeft);
        float rightDist = ComputeClosestDistance(pos, rightVertices, rightTriangles, rightLocalToWorld, out Vector3 closestRight);

        nearestLeft[index] = closestLeft;
        nearestRight[index] = closestRight;

        // === Classify boundaries ===
        var leftZone = ClassifyZone(pos, forward, right, closestLeft);
        var rightZone = ClassifyZone(pos, forward, right, closestRight);


        // supponiamo 50 = velocità "alta" (da tarare in base al tuo gioco)

        float scaledLimit = limitDistance * (1f + speedFactor); // aumenta la distanza limite
        float scaledSafe = safeDistance * (1f + speedFactor * 1.5f); // aumenta la distanza sicura

        float steerIntensity = Mathf.Lerp(0.5f, 1f, speedFactor);
        // a bassa velocità sterza forte, ad alta velocità sterza più morbido

        // === Decide steering con parametri scalati ===
        steer[index] = DecideSteering(leftZone, rightZone, leftDist, rightDist, scaledLimit, scaledSafe, steerIntensity);
        accelerate[index] = 1; // always accelerate
        nearestRaceLinePoint[index] = NearestPointFromList(positions[index], raceLinePoints);
    }

    // =========================
    // NEW: Central decision logic
    // =========================
    private float DecideSteering(
        (VerticalZone v, HorizontalZone h) leftZone,
        (VerticalZone v, HorizontalZone h) rightZone,
        float leftDist, float rightDist,
        float limit, float safe, float steerIntensity)
    {
        float steer = 0f;

        if (leftDist < rightDist)
        {
            steer = ComputeSteeringFromLesftEasy(leftZone, rightZone, leftDist, limit, safe, steerIntensity);
        }
        else
        {
            steer = ComputeSteeringFromRightEasy(leftZone, rightZone, rightDist, limit, safe, steerIntensity);
        }

        return steer;
    }

    // =========================
    // ZONE CLASSIFICATION
    // =========================
    private (VerticalZone, HorizontalZone) ClassifyZone(Vector3 carPos, Vector3 forward, Vector3 right, Vector3 point)
    {
        float verticalCenterDepth = 2f; // Depth of the central vertical zone
        float verticalOffset = 0.5f; // Offset to shift the center zone forward
        float horizontalCenterWidth = 2f; // Width of the central horizontal zone

        // Transform into car-local space
        Vector3 local = Quaternion.Inverse(Quaternion.LookRotation(forward, Vector3.up)) * (point - carPos);

        VerticalZone vZone;
        if (local.z < -(verticalCenterDepth / 2f) + verticalOffset) vZone = VerticalZone.Behind;
        else if (local.z > verticalCenterDepth / 2f + verticalOffset) vZone = VerticalZone.Ahead;
        else vZone = VerticalZone.Center;

        HorizontalZone hZone;
        if (local.x < -(horizontalCenterWidth / 2f)) hZone = HorizontalZone.Left;
        else if (local.x > horizontalCenterWidth / 2f) hZone = HorizontalZone.Right;
        else hZone = HorizontalZone.Center;

        return (vZone, hZone);
    }

    // =========================
    // STEERING LOGIC (UPDATED WITH CLAMP)
    // =========================

    private float ComputeSteeringFromLesftEasy((VerticalZone v, HorizontalZone h) leftZone,
        (VerticalZone v, HorizontalZone h) rightZone, float dist, float limit, float safe, float intensity)
    {
        float steer = 0f;

        if (leftZone.v != VerticalZone.Behind)
        {
            // --- Compute distance-based factor ---
            float distanceFactor = ComputeDistanceFactorEasy(dist, limit, safe);


            if (dist < safe * safe)
            {
                steer = 1 * distanceFactor;
            }

        }

        return steer;
    }

    private float ComputeSteeringFromRightEasy((VerticalZone v, HorizontalZone h) leftZone,
        (VerticalZone v, HorizontalZone h) rightZone, float dist, float limit, float safe, float intensity)
    {
        float steer = 0f;

        if (rightZone.v != VerticalZone.Behind)
        {
            // --- Compute distance-based factor ---
            float distanceFactor = ComputeDistanceFactorEasy(dist, limit, safe);
            if (dist < safe * safe)
            {
                steer = -1f * distanceFactor;
            }
        }

        return steer;
    }

    private float ComputeSteeringFromLeft(VerticalZone vZone, HorizontalZone hZone, float dist, float limit, float safe, float intensity)
    {
        float steer = 0f;
        if (vZone == VerticalZone.Behind && hZone == HorizontalZone.Center) return 0f;

        if (vZone == VerticalZone.Center || vZone == VerticalZone.Ahead)
        {
            if (hZone == HorizontalZone.Center)
            {
                steer = +0.25f * intensity;
            }
            else
            {
                // --- Compute distance-based factor ---
                float distanceFactor = ComputeDistanceFactor(dist, limit, safe);

                if (dist < limit * limit) steer = +1f * intensity * distanceFactor;
                else if (dist < safe * safe) steer = +0.5f * intensity * distanceFactor;
            }
        }

        return Mathf.Clamp(steer, -1f, 1f);
    }

    private float ComputeSteeringFromRight(VerticalZone vZone, HorizontalZone hZone, float dist, float limit, float safe, float intensity)
    {
        float steer = 0f;
        if (vZone == VerticalZone.Behind && hZone == HorizontalZone.Center) return 0f;

        if (vZone == VerticalZone.Center || vZone == VerticalZone.Ahead)
        {
            if (hZone == HorizontalZone.Center)
            {
                steer = -0.25f * intensity;
            }
            else
            {
                // --- Compute distance-based factor ---
                float distanceFactor = ComputeDistanceFactor(dist, limit, safe);

                if (dist < limit * limit) steer = -1f * intensity * distanceFactor;
                else if (dist < safe * safe) steer = -0.25f * intensity * distanceFactor;
            }
        }

        return Mathf.Clamp(steer, -1f, 1f);
    }

    // =========================
    // NEW: Distance factor helper
    // =========================
    private float ComputeDistanceFactor(float sqrDist, float limit, float safe)
    {
        float dist = Mathf.Sqrt(sqrDist);

        if (dist >= safe) return 1f;   // far enough → normal steer
        if (dist <= limit) return 2f;  // too close → double steer

        // Between safe and limit → smooth interpolation
        float t = Mathf.InverseLerp(safe, limit, dist);
        return Mathf.Lerp(1f, 2f, 1f - t);
    }

    private float ComputeDistanceFactorEasy(float quadDist, float limit, float safe)
    {
        float distanceFactor = 0.1f;

        float dist = Mathf.Sqrt(quadDist);
        if (dist > safe)
        {
            dist = safe;
        }

        distanceFactor = 1f - (dist / safe);

        return distanceFactor;
    }


    // =========================
    // GEOMETRY HELPERS
    // =========================
    private float ComputeClosestDistance(Vector3 pos, NativeArray<Vector3> vertices, NativeArray<int> triangles, Matrix4x4 localToWorld, out Vector3 closestPoint)
    {
        float minDist = float.MaxValue;
        Vector3 nearest = pos;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = localToWorld.MultiplyPoint3x4(vertices[triangles[i]]);
            Vector3 b = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 1]]);
            Vector3 c = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 2]]);

            float dist = PointTriangleDistance(pos, a, b, c, out Vector3 candidate);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = candidate;
            }
        }

        closestPoint = nearest;
        return minDist;
    }

    private float PointTriangleDistance(Vector3 point, Vector3 a, Vector3 b, Vector3 c, out Vector3 closestPoint)
    {
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 ap = point - a;

        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0f && d2 <= 0f) { closestPoint = a; return (point - a).sqrMagnitude; }

        Vector3 bp = point - b;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0f && d4 <= d3) { closestPoint = b; return (point - b).sqrMagnitude; }

        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0f && d1 >= 0f && d3 <= 0f) { float v = d1 / (d1 - d3); closestPoint = a + v * ab; return (point - closestPoint).sqrMagnitude; }

        Vector3 cp = point - c;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0f && d5 <= d6) { closestPoint = c; return (point - c).sqrMagnitude; }

        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0f && d2 >= 0f && d6 <= 0f) { float w = d2 / (d2 - d6); closestPoint = a + w * ac; return (point - closestPoint).sqrMagnitude; }

        float va = d3 * d6 - d5 * d4;
        if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f) { float w = (d4 - d3) / ((d4 - d3) + (d5 - d6)); closestPoint = b + w * (c - b); return (point - closestPoint).sqrMagnitude; }

        Vector3 n = Vector3.Cross(ab, ac).normalized;
        closestPoint = point - Vector3.Dot(point - a, n) * n;
        return (point - closestPoint).sqrMagnitude;
    }

    private Vector3 NearestPointFromList(Vector3 position, NativeArray<Vector3> positionList)
    {
        Vector3 closestPoint = position;
        float minDist = float.MaxValue;
        for (int i = 0; i < positionList.Length; i++)
        {
            float dist = (position - positionList[i]).magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = positionList[i];
            }
        }

        return closestPoint;
    }
}