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
                        // Position separation: strip the component along the vehicles' local
                        // "up" axis (the track-normal axis owned by the hover system) instead
                        // of world Y. On banked/rotated track sections world Y does not match
                        // "vertical relative to the vehicle" — stripping world Y there discards
                        // real lateral penetration instead of the redundant vertical one, so the
                        // overlap never gets resolved. Use the averaged up of both vehicles as a
                        // shared reference axis so the symmetric push stays consistent.
                        // Full 3D direction is still passed as collision normal so PlayerController
                        // can project correctly in local space at any bank angle.
                        Vector3 sharedUp = colA.transform.up + colB.transform.up;
                        if (sharedUp.sqrMagnitude < 0.0001f) continue;
                        sharedUp.Normalize();

                        Vector3 dirLateral = direction - sharedUp * Vector3.Dot(direction, sharedUp);
                        if (dirLateral.sqrMagnitude < 0.0001f) continue;
                        dirLateral.Normalize();

                        Vector3 separation = dirLateral * (distance + penetrationEpsilon);

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
                    // Position separation: strip the component along the vehicle's local "up" axis
                    // (owned by the hover system) instead of world Y, so we don't fight the hover system
                    // while still resolving penetration correctly on banked/rotated track sections — see
                    // the PLAYER vs PLAYER pass above for why world Y is the wrong axis to strip there.
                    // Collision normal passed to PlayerController stays the full 3D direction — at non-flat
                    // bank angles the world-Y component maps to local-X (lateral), so stripping it here
                    // would destroy the bounce/rotation signal. PlayerController filters via local-space
                    // projection (localExitVector.y = 0 removes the transform.up component correctly).
                    Vector3 up = playerCol.transform.up;
                    Vector3 dirLateral = direction - up * Vector3.Dot(direction, up);
                    if (dirLateral.sqrMagnitude > 0.0001f)
                    {
                        dirLateral.Normalize();
                        playerCol.transform.position += dirLateral * (distance + penetrationEpsilon);
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