using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class CPUManager : MonoBehaviour
{
    #region PUBLIC VARIABLES
    [Header("Input Data")]
    public MeshCollider leftCollider;
    public MeshCollider rightCollider;

    [Header("Configuration")]
    public float forwardLookAheadMultiplier = 10f;
    public float forwardLookSteerMultiplier = 2f;
    public float forwardLookotherVeichlesMultiplier = 1.5f;

    public float borderLimitDistance = 2f;
    public float borderSafeDistance = 6f;
    public float otherVeichleSafeDistance = 3f;
    #endregion

    #region PRIVATE VARIABLES
    
    private NativeArray<int> JOB_IO_cpuAccelerate;
    private NativeArray<float> JOB_IO_cpuSteer;
    private NativeArray<float> JOB_I_cpuCurrentSpeed;
    private NativeArray<bool> JOB_I_cpuInCorner;
    private Transform[] JOB_I_cpuTransforms;
    private Transform[] JOB_I_nextRaceLineTransforms;
    private float JOB_I_maxSpeed;

    // Debug storage: nearest points found for gizmos
    private Vector3[] JOB_O_nearestLeftPoints;
    private Vector3[] JOB_O_nearestRightPoints;
    private Vector3[] JOB_O_nearestRaceLinePoints;

    private RaceManager raceManager;
    private RaceData raceData;
    List<GameObject> checkPointList;
    private CPUInputHandlerManager cpuInputHandlerManager;

    [SerializeField] private int cpuCount = 1;

    private float updateInterval = 0.1f; // 10 Hz
    private float timeSinceLastUpdate = 0f;

    private Coroutine cpuJobCycle;
    #endregion

    #region DEFAULT METHODS
    void Start()
    {
        raceManager = RaceManager.Instance;
        if (raceManager == null)
        {
            Debug.LogError("RaceManager instance not found.");
            return;
        }
        raceData = raceManager.GetRaceData();
        checkPointList = raceManager.checkPointList;

        cpuInputHandlerManager = CPUInputHandlerManager.Instance;
        if (cpuInputHandlerManager == null)
        {
            Debug.LogError("CPUInputHandlerManager instance not found.");
            return;
        }

        cpuCount = cpuInputHandlerManager.cpuPlayerAmount;


        JOB_IO_cpuAccelerate = new NativeArray<int>(cpuCount, Allocator.Persistent);
        JOB_IO_cpuSteer = new NativeArray<float>(cpuCount, Allocator.Persistent);
        JOB_I_cpuTransforms = new Transform[cpuCount];
        JOB_I_nextRaceLineTransforms = new Transform[cpuCount];
        JOB_I_cpuInCorner = new NativeArray<bool>(cpuCount, Allocator.Persistent);

        JOB_O_nearestLeftPoints = new Vector3[cpuCount];
        JOB_O_nearestRightPoints = new Vector3[cpuCount];
        JOB_O_nearestRaceLinePoints = new Vector3[cpuCount];

        for (int i = 0; i < cpuCount; i++)
        {
            JOB_IO_cpuAccelerate[i] = 1;
            JOB_IO_cpuSteer[i] = 0f;
        }

    }

    void FixedUpdate()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate < updateInterval) return;
        timeSinceLastUpdate = 0f;

        if(cpuJobCycle == null)
        {
            cpuJobCycle = StartCoroutine(CPUJobCycle());
        }
    }

    IEnumerator CPUJobCycle()
    {
        var wait = new WaitForSeconds(0.1f); // 10 Hz
        while (true)
        {
            UpdateCPUData();
            RunCPUJob();
            yield return wait;
        }
    }

    void OnDestroy()
    {
        if (JOB_IO_cpuAccelerate.IsCreated) JOB_IO_cpuAccelerate.Dispose();
        if (JOB_IO_cpuSteer.IsCreated) JOB_IO_cpuSteer.Dispose();
    }

    // Draw gizmos to debug nearest distances
    void OnDrawGizmos()
    {
        if (JOB_I_cpuTransforms == null) return;

        Gizmos.color = Color.white;

        for (int i = 0; i < JOB_I_cpuTransforms.Length; i++)
        {
            if (JOB_I_cpuTransforms[i] == null) continue;

            Vector3 carPos = JOB_I_cpuTransforms[i].position;

            // Disegno direzioni principali della macchina
            Vector3 forward = JOB_I_cpuTransforms[i].forward;
            Vector3 right = JOB_I_cpuTransforms[i].right;

            float speed = 0f;
            if (i < JOB_I_cpuCurrentSpeed.Length) speed = JOB_I_cpuCurrentSpeed[i]; // se hai l'array delle velocità

            float speedFactor = Mathf.Clamp01(speed / JOB_I_maxSpeed);

            Vector3 forwardSensorPoistion = carPos + (forward * (1 + (speedFactor * forwardLookAheadMultiplier))) + (right * JOB_IO_cpuSteer[i] * forwardLookSteerMultiplier * speedFactor);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(forwardSensorPoistion, forwardSensorPoistion + forward * 5f); // forward
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(forwardSensorPoistion, forwardSensorPoistion + right * 5f);   // right

            // Disegno nearest points (calcolati dal Job)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(forwardSensorPoistion, JOB_O_nearestLeftPoints[i]);
            Gizmos.DrawSphere(JOB_O_nearestLeftPoints[i], 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(forwardSensorPoistion, JOB_O_nearestRightPoints[i]);
            Gizmos.DrawSphere(JOB_O_nearestRightPoints[i], 0.2f);

            // Nearest RaceLine point
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(carPos, JOB_O_nearestRaceLinePoints[i]);
            Gizmos.DrawSphere(JOB_O_nearestRaceLinePoints[i], 0.2f);

            // === Disegno le zone Safe e Limit ===

            float scaledLimit = borderLimitDistance * (1f + speedFactor);
            float scaledSafe = borderSafeDistance * (1f + speedFactor * 1.5f);

            // Limit zone (arancione)
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
            Gizmos.DrawWireSphere(forwardSensorPoistion, scaledLimit);

            // Safe zone (verde chiaro)
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Gizmos.DrawWireSphere(forwardSensorPoistion, scaledSafe);

            Vector3 forwardOtherVeichleSensorPosition = carPos + (forward * forwardLookotherVeichlesMultiplier);

            // Other veichle safe zone
            Gizmos.color = new Color(0.9f, 0.1f, 0.1f, 0.25f);
            Gizmos.DrawWireSphere(forwardOtherVeichleSensorPosition, otherVeichleSafeDistance);
        }
    }
    #endregion

    #region PRIVATE METHODS
    private void UpdateCPUData()
    {
        // Update CPU transforms
        List<GameObject> players = raceManager.GetAllPlayerInstances();

        List<Transform> cpuTransformList = new List<Transform>();
        List<float> cpuSpeedList = new List<float>();
        List<Transform> nextRaceLinePositionList = new List<Transform>();
        List<bool> cpuInCornerList = new List<bool>();

        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerStructure>().data.playerInputIndex == InputIndex.CPU)
            {
                // collect CPU transforms
                cpuTransformList.Add(player.transform);
                // collect CPU current speed
                cpuSpeedList.Add(player.GetComponent<PlayerController>().GetCurrentSpeed());
                // collect CPU next checkpoint transform
                
            }
        }

        for(int i = 0; i < cpuTransformList.Count; i++)
        {
            GameObject player = cpuTransformList[i].gameObject;
            int playerDataToUpdateIndex = raceData.playerRaceDataList.FindIndex(p => p.playerData.name == player.GetComponent<PlayerController>().playerData.name);
            int nextCheckpointIndex = raceData.playerRaceDataList[playerDataToUpdateIndex].nextCheckpointIndex;

            GameObject nextCheckpoint = checkPointList[nextCheckpointIndex];

            if (nextCheckpoint != null)
            {
                nextRaceLinePositionList.Add(nextCheckpoint.transform);
                CheckpointType checkpointType = nextCheckpoint.GetComponent<CheckpointType>();
                if (checkpointType != null)
                {
                    if (checkpointType.checkpointType == CheckpointTypeEnum.CornerMid || checkpointType.checkpointType == CheckpointTypeEnum.CornerEnd)
                        cpuInCornerList.Add(true);
                    else
                        cpuInCornerList.Add(false);
                }
            }
        }


        if (cpuTransformList.Count != cpuCount && cpuSpeedList.Count != cpuCount)
        {
            Debug.LogWarning($"CPUManager: Expected {cpuCount} CPU players, but found {cpuTransformList.Count} and {cpuSpeedList.Count}.");
        }
        else
        {
            JOB_I_cpuTransforms = cpuTransformList.ToArray();
            JOB_I_cpuCurrentSpeed = new NativeArray<float>(cpuSpeedList.ToArray(), Allocator.Persistent);
            JOB_I_nextRaceLineTransforms = nextRaceLinePositionList.ToArray();
            JOB_I_cpuInCorner = new NativeArray<bool>(cpuInCornerList.ToArray(), Allocator.Persistent);
        }

        // TODO : get maxSpeed
        JOB_I_maxSpeed = players[0].GetComponent<PlayerController>().maxSpeed;

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

        NativeArray<Vector3> raceLinePoints = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);

        for (int i = 0; i < cpuCount; i++)
        {

            raceLinePoints[i] = JOB_I_nextRaceLineTransforms[i].transform.position;


            if (JOB_I_cpuTransforms[i] != null) {
                positions[i] = JOB_I_cpuTransforms[i].position;
                forwardDirections[i] = JOB_I_cpuTransforms[i].forward;
                rightDirections[i] = JOB_I_cpuTransforms[i].right;
            }
                
        }

        // Arrays to receive nearest points
        NativeArray<Vector3> nearestLeft = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);
        NativeArray<Vector3> nearestRight = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);
        NativeArray<Vector3> nearestRaceLinePoint = new NativeArray<Vector3>(cpuCount, Allocator.TempJob);

        CPUJob job = new CPUJob
        {
            accelerate = JOB_IO_cpuAccelerate,
            currentSpeed = JOB_I_cpuCurrentSpeed,
            steer = JOB_IO_cpuSteer,
            positions = positions,
            lookAheadForwardMultiplier = forwardLookAheadMultiplier,
            lookAheadRightMultiplier = forwardLookSteerMultiplier,
            lookAheadVeichleMultiplier = forwardLookotherVeichlesMultiplier,
            forwardDirections = forwardDirections,
            rightDirections = rightDirections,
            raceLinePoints = raceLinePoints,
            inCorner = JOB_I_cpuInCorner,

            maxSpeed = JOB_I_maxSpeed,

            leftVertices = leftVertices,
            leftTriangles = leftTriangles,
            rightVertices = rightVertices,
            rightTriangles = rightTriangles,

            leftLocalToWorld = leftCollider.transform.localToWorldMatrix,
            rightLocalToWorld = rightCollider.transform.localToWorldMatrix,

            nearestLeft = nearestLeft,
            nearestRight = nearestRight,
            nearestRaceLinePoint = nearestRaceLinePoint,

            limitDistance = borderLimitDistance,
            safeDistance = borderSafeDistance,
            otherVeichleSafeDistance = otherVeichleSafeDistance
        };

        JobHandle handle = job.Schedule(cpuCount, 1);
        handle.Complete();

        for (int i = 0; i < cpuCount; i++)
        {
            cpuInputHandlerManager.GetCPUInput(i).OnCPUAccelerate(JOB_IO_cpuAccelerate[i] == 1 ? 1.0f : 0.0f);
            cpuInputHandlerManager.GetCPUInput(i).OnCPUSteer(JOB_IO_cpuSteer[i]);

            // Save nearest points for gizmos
            JOB_O_nearestLeftPoints[i] = nearestLeft[i];
            JOB_O_nearestRightPoints[i] = nearestRight[i];
            JOB_O_nearestRaceLinePoints[i] = nearestRaceLinePoint[i];
        }

        leftVertices.Dispose();
        leftTriangles.Dispose();
        rightVertices.Dispose();
        rightTriangles.Dispose();
        positions.Dispose();
        nearestLeft.Dispose();
        nearestRight.Dispose();
        nearestRaceLinePoint.Dispose();
    }
    #endregion

}
