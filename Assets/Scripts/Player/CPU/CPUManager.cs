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


    private RaceManager raceManager;
    private CPUInputHandlerManager cpuInputHandlerManager;

    [SerializeField] private int cpuCount = 1;

    private float updateInterval = 0.1f; // 10 Hz
    private float timeSinceLastUpdate = 0f;

    // Debug storage: nearest points found for gizmos
    private Vector3[] nearestLeftPoints;
    private Vector3[] nearestRightPoints;

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

}
