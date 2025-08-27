using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class CPUManager : MonoBehaviour
{
    private NativeArray<int> cpuAccelerate;
    private NativeArray<float> cpuSteer;
    private Transform[] cpuTransforms;

    public MeshCollider leftCollider;
    public MeshCollider rightCollider;

    public float borderLimitDistance = 2f;
    public float borderSafeDistance = 6f;

    private RaceManager raceManager;
    private CPUInputHandlerManager cpuInputHandlerManager;

    [SerializeField] private int cpuCount = 1;

    private float updateInterval = 0.1f; // 10 Hz
    private float timeSinceLastUpdate = 0f;

    // Debug storage: nearest points found for gizmos
    private Vector3[] nearestLeftPoints;
    private Vector3[] nearestRightPoints;

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

        UpdateCPUTransforms();
        RunCPUJob();
        
    }

    private void UpdateCPUTransforms()
    {
        List<GameObject> players = raceManager.GetAllPlayerInstances();

        Debug.LogWarning($"CPUManager: got {players.Count} players");

        List<Transform> cpuTransformList = new List<Transform>();

        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerStructure>().data.playerInputIndex == InputIndex.CPU)
            {
                cpuTransformList.Add(player.transform);
            }
        }

        if(cpuTransformList.Count != cpuCount)
        {
            Debug.LogWarning($"CPUManager: Expected {cpuCount} CPU players, but found {cpuTransformList.Count}.");
        }
        else
        {
            cpuTransforms = cpuTransformList.ToArray();
        }
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
            if (cpuTransforms[i] != null)
                positions[i] = cpuTransforms[i].position;
                forwardDirections[i] = cpuTransforms[i].forward;
                rightDirections[i] = cpuTransforms[i].right;
        }

        // Arrays to receive nearest points
        NativeArray<Vector3> nearestLeft = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);
        NativeArray<Vector3> nearestRight = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);

        CPUJob job = new CPUJob
        {
            accelerate = cpuAccelerate,
            steer = cpuSteer,
            positions = positions,
            forwardDirections = forwardDirections,
            rightDirections = rightDirections,

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

        for (int i = 0; i < cpuTransforms.Length; i++)
        {
            if (cpuTransforms[i] == null) continue;

            Vector3 carPos = cpuTransforms[i].position;

            // Red → left boundary
            Gizmos.color = Color.red;
            Gizmos.DrawLine(carPos, nearestLeftPoints[i]);
            Gizmos.DrawSphere(nearestLeftPoints[i], 0.2f);

            // Blue → right boundary
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(carPos, nearestRightPoints[i]);
            Gizmos.DrawSphere(nearestRightPoints[i], 0.2f);
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
        public NativeArray<float> steer;
        [ReadOnly] public NativeArray<Vector3> positions;
        [ReadOnly] public NativeArray<Vector3> forwardDirections;
        [ReadOnly] public NativeArray<Vector3> rightDirections;

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

            // === Compute closest points & squared distances ===
            float leftDist = ComputeClosestDistance(pos, leftVertices, leftTriangles, leftLocalToWorld, out Vector3 closestLeft);
            float rightDist = ComputeClosestDistance(pos, rightVertices, rightTriangles, rightLocalToWorld, out Vector3 closestRight);

            nearestLeft[index] = closestLeft;
            nearestRight[index] = closestRight;

            // === Define car orientation
            Vector3 forward = forwardDirections[index];
            Vector3 right = rightDirections[index];

            // === Classify boundaries ===
            var leftZone = ClassifyZone(pos, forward, right, closestLeft);
            var rightZone = ClassifyZone(pos, forward, right, closestRight);

            // === Decide steering ===
            steer[index] = DecideSteering(leftZone, rightZone, leftDist, rightDist);
        }

        // =========================
        // NEW: Central decision logic
        // =========================
        private float DecideSteering(
            (VerticalZone v, HorizontalZone h) leftZone,
            (VerticalZone v, HorizontalZone h) rightZone,
            float leftDist, float rightDist)
        {
            float steerLeft = ComputeSteeringFromLeft(leftZone.v, leftZone.h, leftDist);
            float steerRight = ComputeSteeringFromRight(rightZone.v, rightZone.h, rightDist);

            return Mathf.Clamp(steerLeft + steerRight, -1f, 1f);
        }

        // =========================
        // ZONE CLASSIFICATION
        // =========================
        private (VerticalZone, HorizontalZone) ClassifyZone(Vector3 carPos, Vector3 forward, Vector3 right, Vector3 point)
        {
            // Transform into car-local space
            Vector3 local = Quaternion.Inverse(Quaternion.LookRotation(forward, Vector3.up)) * (point - carPos);

            VerticalZone vZone;
            if (local.z < -1f) vZone = VerticalZone.Behind;
            else if (local.z > 1f) vZone = VerticalZone.Ahead;
            else vZone = VerticalZone.Center;

            HorizontalZone hZone;
            if (local.x < -1f) hZone = HorizontalZone.Left;
            else if (local.x > 1f) hZone = HorizontalZone.Right;
            else hZone = HorizontalZone.Center;

            return (vZone, hZone);
        }

        // =========================
        // STEERING LOGIC
        // =========================
        private float ComputeSteeringFromLeft(VerticalZone vZone, HorizontalZone hZone, float dist)
        {
            float steer = 0f;
            if (vZone == VerticalZone.Behind) return 0f; // Ignore if behind

            if (vZone == VerticalZone.Center)
            {
                if (hZone == HorizontalZone.Left || hZone == HorizontalZone.Center)
                {
                    if (dist < limitDistance * limitDistance) steer = +1f;   // Hard steer right
                    else if (dist < safeDistance * safeDistance) steer = +0.25f; // Soft steer right
                }
                else if (hZone == HorizontalZone.Right)
                {
                    steer = +1f; // Always steer away from left border
                }
            }
            else if (vZone == VerticalZone.Ahead)
            {
                if (dist < limitDistance * limitDistance) steer = +1f;
                else steer = +0.25f;
            }
            return steer;
        }

        private float ComputeSteeringFromRight(VerticalZone vZone, HorizontalZone hZone, float dist)
        {
            float steer = 0f;
            if (vZone == VerticalZone.Behind) return 0f; // Ignore if behind

            if (vZone == VerticalZone.Center)
            {
                if (hZone == HorizontalZone.Right || hZone == HorizontalZone.Center)
                {
                    if (dist < limitDistance * limitDistance) steer = -1f;   // Hard steer left
                    else if (dist < safeDistance * safeDistance) steer = -0.25f; // Soft steer left
                }
                else if (hZone == HorizontalZone.Left)
                {
                    steer = -1f; // Always steer away from right border
                }
            }
            else if (vZone == VerticalZone.Ahead)
            {
                if (dist < limitDistance * limitDistance) steer = -1f;
                else steer = -0.25f;
            }
            return steer;
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
    }




}
