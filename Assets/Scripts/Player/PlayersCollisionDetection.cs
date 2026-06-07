using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionInfo
{
    public Collider otherCollider;
    public Vector3 collisionPoint;
    public Vector3 collisionNormal;
    public float penetrationDepth;
    public bool isColliding;

    public PlayerCollisionInfo(Collider otherCollider, Vector3 collisionPoint, Vector3 collisionNormal, float penetrationDepth, bool isColliding)
    {
        this.otherCollider = otherCollider;
        this.collisionPoint = collisionPoint;
        this.collisionNormal = collisionNormal;
        this.penetrationDepth = penetrationDepth;
        this.isColliding = isColliding;
    }
}

public class PlayersCollisionDetection : MonoBehaviour
{
    public List<PlayerController> players;
    public bool executeCollisionDetection = false;

    private List<Collider> playerColliders = new List<Collider>();

    private Collider trackMainCollider;
    private RaceManager raceManager;

    [Header("Collision Settings")]
    public int solverIterations = 3;
    public float penetrationEpsilon = 0.001f;

    void Start()
    {
        raceManager = RaceManager.Instance;

        if (raceManager != null)
        {
            trackMainCollider = raceManager.trackMainCollider;
        }
    }

    public void InitializePlayersColliders(List<PlayerController> playerControllerList)
    {
        executeCollisionDetection = false;
        playerColliders.Clear();

        if (playerControllerList != null && playerControllerList.Count != 0)
        {
            players = playerControllerList;

            foreach (PlayerController player in players)
            {
                Collider col = player.GetComponent<Collider>();
                if (col != null)
                {
                    playerColliders.Add(col);
                }
                else
                {
                    Debug.LogError("Player senza Collider!");
                    return;
                }
            }

            executeCollisionDetection = true;
        }
        else
        {
            Debug.LogError("Lista player vuota o nulla");
        }
    }

    void Update()
    {
        if (!executeCollisionDetection) return;

        Physics.SyncTransforms();

        // reset collision info
        foreach (var player in players)
        {
            player.ClearPlayerCollisionInfo();
        }

        // ITERATIVE SOLVER
        for (int iteration = 0; iteration < solverIterations; iteration++)
        {
            // =========================
            // PLAYER vs PLAYER
            // =========================
            for (int i = 0; i < playerColliders.Count; i++)
            {
                for (int j = i + 1; j < playerColliders.Count; j++)
                {
                    Collider colA = playerColliders[i];
                    Collider colB = playerColliders[j];

                    if (Physics.ComputePenetration(
                        colA, colA.transform.position, colA.transform.rotation,
                        colB, colB.transform.position, colB.transform.rotation,
                        out Vector3 direction, out float distance))
                    {
                        // Position separation: world-horizontal only.
                        // Full 3D direction passed as collision normal so PlayerController
                        // can project correctly in local space at any bank angle.
                        Vector3 dirHoriz = new Vector3(direction.x, 0f, direction.z);
                        if (dirHoriz.sqrMagnitude < 0.0001f) continue;
                        dirHoriz.Normalize();

                        Vector3 separation = dirHoriz * (distance + penetrationEpsilon);

                        var pcA = colA.GetComponent<PlayerController>();
                        var pcB = colB.GetComponent<PlayerController>();

                        if (pcA != null && pcB != null)
                        {

                            float factorA = 0.5f;
                            float factorB = 0.5f;

                            colA.transform.position += separation * factorA;
                            colB.transform.position -= separation * factorB;

                            // collision info
                            Vector3 collisionPoint = colA.transform.position + direction * distance * 0.5f;

                            pcA.SetPlayerCollisionInfo(new PlayerCollisionInfo(colB, collisionPoint, direction, distance, true));
                            pcB.SetPlayerCollisionInfo(new PlayerCollisionInfo(colA, collisionPoint, -direction, distance, true));
                        }
                    }
                }
            }

            // =========================
            // PLAYER vs TRACK
            // =========================
            for (int i = 0; i < playerColliders.Count; i++)
            {
                Collider playerCol = playerColliders[i];

                if (Physics.ComputePenetration(
                    playerCol, playerCol.transform.position, playerCol.transform.rotation,
                    trackMainCollider, trackMainCollider.transform.position, trackMainCollider.transform.rotation,
                    out Vector3 direction, out float distance))
                {
                    // Position separation: world-horizontal only, so we don't fight the hover system.
                    // Collision normal passed to PlayerController is the full 3D direction — at non-flat
                    // bank angles the world-Y component maps to local-X (lateral), so stripping it here
                    // would destroy the bounce/rotation signal. PlayerController filters via local-space
                    // projection (localExitVector.y = 0 removes the transform.up component correctly).
                    Vector3 dirHoriz = new Vector3(direction.x, 0f, direction.z);
                    if (dirHoriz.sqrMagnitude > 0.0001f)
                    {
                        dirHoriz.Normalize();
                        playerCol.transform.position += dirHoriz * (distance + penetrationEpsilon);
                    }

                    Vector3 collisionPoint = playerCol.transform.position + direction * distance * 0.5f;

                    playerCol.GetComponent<PlayerController>()
                        .SetTrackCollisionInfo(
                            new PlayerCollisionInfo(trackMainCollider, collisionPoint, direction, distance, true)
                        );
                }
                else if (iteration == solverIterations - 1)
                {
                    // reset only on last iteration
                    playerCol.GetComponent<PlayerController>()
                        .SetTrackCollisionInfo(
                            new PlayerCollisionInfo(null, Vector3.zero, Vector3.zero, 0f, false)
                        );
                }
            }
        }
    }
}