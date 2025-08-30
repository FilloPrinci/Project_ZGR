using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class CPUManager : MonoBehaviour
{
    private NativeArray<int> cpuAccelerate;
    private NativeArray<float> cpuSteer;
    private NativeArray<float> cpuCurrentSpeed;
    private Transform[] cpuTransforms;

    public MeshCollider leftCollider;
    public MeshCollider rightCollider;

    public float forwardLookAheadMultiplier = 10f;
    public float forwardLookSteerMultiplier = 2f;

    public float borderLimitDistance = 2f;
    public float borderSafeDistance = 6f;

    public List<Transform> raceTrackPointList;

    private RaceManager raceManager;
    private CPUInputHandlerManager cpuInputHandlerManager;

    [SerializeField] private int cpuCount = 1;

    private float updateInterval = 0.1f; // 10 Hz
    private float timeSinceLastUpdate = 0f;

    // Debug storage: nearest points found for gizmos
    private Vector3[] nearestLeftPoints;
    private Vector3[] nearestRightPoints;

    private NativeArray<Vector3> nearestRsceLinePositions;

    private float maxSpeed;

    void Start()
    {
        raceManager = RaceManager.Instance;
        if (raceManager == null)
        {
            Debug.LogError("RaceManager instance not found.");
            return;
        }

        cpuInputHandlerManager = CPUInputHandlerManager.Instance;
        if (cpuInputHandlerManager == null)
        {
            Debug.LogError("CPUInputHandlerManager instance not found.");
            return;
        }

        cpuCount = cpuInputHandlerManager.cpuPlayerAmount;

        cpuAccelerate = new NativeArray<int>(cpuCount, Allocator.Persistent);
        cpuSteer = new NativeArray<float>(cpuCount, Allocator.Persistent);
        cpuTransforms = new Transform[cpuCount];

        nearestLeftPoints = new Vector3[cpuCount];
        nearestRightPoints = new Vector3[cpuCount];
        nearestRsceLinePositions = new NativeArray<Vector3>(cpuCount, Allocator.Persistent);

        for (int i = 0; i < cpuCount; i++)
        {
            cpuAccelerate[i] = 1;
            cpuSteer[i] = 0f;
        }
    }

    void FixedUpdate()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate < updateInterval) return;
        timeSinceLastUpdate = 0f;

        UpdateCPUData();
        RunCPUJob();
        
    }

    private void UpdateCPUData()
    {
        // Update CPU transforms
        List<GameObject> players = raceManager.GetAllPlayerInstances();

        Debug.LogWarning($"CPUManager: got {players.Count} players");

        List<Transform> cpuTransformList = new List<Transform>();
        List<float> cpuSpeedList = new List<float>();

        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerStructure>().data.playerInputIndex == InputIndex.CPU)
            {
                // collect CPU transforms
                cpuTransformList.Add(player.transform);
                // collect CPU current speed
                cpuSpeedList.Add(player.GetComponent<PlayerController>().GetCurrentSpeed());
            }
        }

        if(cpuTransformList.Count != cpuCount && cpuSpeedList.Count != cpuCount)
        {
            Debug.LogWarning($"CPUManager: Expected {cpuCount} CPU players, but found {cpuTransformList.Count} and {cpuSpeedList.Count}.");
        }
        else
        {
            cpuTransforms = cpuTransformList.ToArray();
            cpuCurrentSpeed = new NativeArray<float>(cpuSpeedList.ToArray(), Allocator.Persistent);
        }

        // TODO : get maxSpeed
        maxSpeed = players[0].GetComponent<PlayerController>().maxSpeed;

    }

    private void RunCPUJob()
    {
        if (leftCollider == null || rightCollider == null) return;

        Mesh leftMesh = leftCollider.sharedMesh;
        Mesh rightMesh = rightCollider.sharedMesh;

        NativeArray<Vector3> leftVertices = new NativeArray<Vector3>(leftMesh.vertices, Allocator.TempJob);
        NativeArray<int> leftTriangles = new NativeArray<int>(leftMesh.triangles, Allocator.TempJob);

        NativeArray<Vector3> rightVertices = new NativeArray<Vector3>(rightMesh.vertices, Allocator.TempJob);
        NativeArray<int> rightTriangles = new NativeArray<int>(rightMesh.triangles, Allocator.TempJob);

        NativeArray<Vector3> positions = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);
        NativeArray<Vector3> forwardDirections = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);
        NativeArray<Vector3> rightDirections = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);
        for (int i = 0; i < cpuCount; i++)
        {
            if (cpuTransforms[i] != null) {
                positions[i] = cpuTransforms[i].position;
                forwardDirections[i] = cpuTransforms[i].forward;
                rightDirections[i] = cpuTransforms[i].right;
            }
                
        }

        // Arrays to receive nearest points
        NativeArray<Vector3> nearestLeft = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);
        NativeArray<Vector3> nearestRight = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);
        NativeArray<Vector3> raceLinePointsPositions = new NativeArray<Vector3>(raceTrackPointList.Count, Allocator.TempJob);
        NativeArray<Vector3> nearestRaceLinePoints  = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);

        for (int i = 0; i < raceTrackPointList.Count; i++)
        {
            raceLinePointsPositions[i] = raceTrackPointList[i].position;
        }



        CPUJob job = new CPUJob
        {
            accelerate = cpuAccelerate,
            currentSpeed = cpuCurrentSpeed,
            steer = cpuSteer,
            positions = positions,
            lookAheadForwardMultiplier = forwardLookAheadMultiplier,
            lookAheadRightMultiplier = forwardLookSteerMultiplier,
            forwardDirections = forwardDirections,
            rightDirections = rightDirections,
            raceLinePoints = raceLinePointsPositions,
            nearestRaceLinePoints = nearestRaceLinePoints,

            maxSpeed = maxSpeed,

            leftVertices = leftVertices,
            leftTriangles = leftTriangles,
            rightVertices = rightVertices,
            rightTriangles = rightTriangles,

            leftLocalToWorld = leftCollider.transform.localToWorldMatrix,
            rightLocalToWorld = rightCollider.transform.localToWorldMatrix,

            nearestLeft = nearestLeft,
            nearestRight = nearestRight,

            limitDistance = borderLimitDistance,
            safeDistance = borderSafeDistance
        };

        JobHandle handle = job.Schedule(cpuCount, 1);
        handle.Complete();

        for (int i = 0; i < cpuCount; i++)
        {
            cpuInputHandlerManager.GetCPUInput(i).OnCPUAccelerate(cpuAccelerate[i] == 1 ? 1.0f : 0.0f);
            cpuInputHandlerManager.GetCPUInput(i).OnCPUSteer(cpuSteer[i]);

            // Save nearest points for gizmos
            nearestLeftPoints[i] = nearestLeft[i];
            nearestRightPoints[i] = nearestRight[i];
            nearestRsceLinePositions[i] = nearestRsceLinePositions[i];
        }

        leftVertices.Dispose();
        leftTriangles.Dispose();
        rightVertices.Dispose();
        rightTriangles.Dispose();
        positions.Dispose();
        nearestLeft.Dispose();
        nearestRight.Dispose();
    }

    public void SetColliders(MeshCollider left, MeshCollider right)
    {
        leftCollider = left;
        rightCollider = right;
    }

    void OnDestroy()
    {
        if (cpuAccelerate.IsCreated) cpuAccelerate.Dispose();
        if (cpuSteer.IsCreated) cpuSteer.Dispose();
    }

    // Draw gizmos to debug nearest distances
    private void OnDrawGizmos()
    {
        if (cpuTransforms == null) return;

        Gizmos.color = Color.white;

        for (int i = 0; i < cpuTransforms.Length; i++)
        {
            if (cpuTransforms[i] == null) continue;

            Vector3 carPos = cpuTransforms[i].position;

            // Disegno direzioni principali della macchina
            Vector3 forward = cpuTransforms[i].forward;
            Vector3 right = cpuTransforms[i].right;

            float speed = 0f;
            if (i < cpuCurrentSpeed.Length) speed = cpuCurrentSpeed[i]; // se hai l'array delle velocità

            float speedFactor = Mathf.Clamp01(speed / maxSpeed );

            carPos += (forward * (1 + (speedFactor * forwardLookAheadMultiplier))) + (right * cpuSteer[i] * forwardLookSteerMultiplier * speedFactor);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(carPos, carPos + forward * 5f); // forward
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(carPos, carPos + right * 5f);   // right

            // Disegno nearest points (calcolati dal Job)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(carPos, nearestLeftPoints[i]);
            Gizmos.DrawSphere(nearestLeftPoints[i], 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(carPos, nearestRightPoints[i]);
            Gizmos.DrawSphere(nearestRightPoints[i], 0.2f);

            // Disegno nearest race line
            

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(carPos, nearestRsceLinePositions[i]);
            Gizmos.DrawSphere(nearestRightPoints[i], 0.2f);

            // === Disegno le zone Safe e Limit ===

            float scaledLimit = borderLimitDistance * (1f + speedFactor);
            float scaledSafe = borderSafeDistance * (1f + speedFactor * 1.5f);

            // Limit zone (arancione)
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
            Gizmos.DrawWireSphere(carPos, scaledLimit);

            // Safe zone (verde chiaro)
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Gizmos.DrawWireSphere(carPos, scaledSafe);
        }
    }

    // ============================
    // Job definition
    // ============================
    public struct CPUJob : IJobParallelFor
    {
        // =========================
        // Input/Output arrays
        // =========================
        [ReadOnly] public NativeArray<int> accelerate;
        [ReadOnly] public NativeArray<float> currentSpeed;
        [ReadOnly] public float maxSpeed;
        [ReadOnly] public float lookAheadForwardMultiplier;
        [ReadOnly] public float lookAheadRightMultiplier;
        public NativeArray<float> steer;
        [ReadOnly] public NativeArray<Vector3> nearestRaceLinePoints;
        [ReadOnly] public NativeArray<Vector3> positions;
        [ReadOnly] public NativeArray<Vector3> forwardDirections;
        [ReadOnly] public NativeArray<Vector3> rightDirections;
         public NativeArray<Vector3> raceLinePoints;

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

            nearestRaceLinePoints[index] = FindNearestPoint(pos, raceLinePoints);

            // === Decide steering con parametri scalati ===
            steer[index] = DecideSteering(leftZone, rightZone, leftDist, rightDist, scaledLimit, scaledSafe, steerIntensity);
            accelerate[index] = 1; // always accelerate
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

            if (leftDist < rightDist) { 
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
            if (local.z < -(verticalCenterDepth/2f) + verticalOffset) vZone = VerticalZone.Behind;
            else if (local.z > verticalCenterDepth/2f + verticalOffset) vZone = VerticalZone.Ahead;
            else vZone = VerticalZone.Center;

            HorizontalZone hZone;
            if (local.x < -(horizontalCenterWidth/2f)) hZone = HorizontalZone.Left;
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

            if(leftZone.v != VerticalZone.Behind)
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
        private Vector3 FindNearestPoint(Vector3 pos, NativeArray<Vector3> vertices)
        {
            float minDist = float.MaxValue;
            Vector3 closestPoint = pos;

            for (int i = 0; i < vertices.Length; i++) { 
                float dist = (pos - vertices[i]).magnitude;

                if (dist < minDist) { 
                    minDist = dist; 
                    closestPoint = vertices[i];
                }
            }

            return closestPoint;
        }

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
    }




}
