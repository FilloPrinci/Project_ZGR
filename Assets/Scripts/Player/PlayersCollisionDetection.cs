using NUnit.Framework;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        if(playerControllerList != null && playerControllerList.Count != 0)
        {
            executeCollisionDetection = true;
            players = playerControllerList;
            foreach (PlayerController player in players)
            {
                Collider playerCollider = player.GetComponent<Collider>();
                if (playerCollider != null)
                {
                    playerColliders.Add(playerCollider);
                }
                else
                {
                    Debug.LogError("PlayersCollisionDetection: PlayerController does not have a Collider component.");
                    executeCollisionDetection = false;
                }
            }
            Debug.Log("PlayersCollisionDetection: Initialization completed");
        }
        else
        {

            Debug.LogError("PlayersCollisionDetection: playerControllerList is null or empty.");
            executeCollisionDetection = false;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!executeCollisionDetection) return;
        if (executeCollisionDetection) {
            // Sincronizza i transform con la fisica
            Physics.SyncTransforms();

            foreach (var player in players)
            {
                player.ClearPlayerCollisionInfo();
            }

            // --- Replace inner pairwise collision loop: only set collision info when overlapping (do not clear on non-overlap) ---
            for (int i = 0; i < playerColliders.Count; i++)
            {
                for (int j = i + 1; j < playerColliders.Count; j++)
                {
                    Collider colA = playerColliders[i];
                    Collider colB = playerColliders[j];

                    Vector3 direction;
                    float distance;

                    bool isOverlapping = Physics.ComputePenetration(
                        colA, colA.transform.position, colA.transform.rotation,
                        colB, colB.transform.position, colB.transform.rotation,
                        out direction, out distance
                    );

                    if (isOverlapping)
                    {
                        // approximate collision point
                        Vector3 collisionPoint = colA.transform.position + direction * distance * 0.5f;

                        // set collision info for both involved players (do not clear here)
                        var pcA = colA.GetComponent<PlayerController>();
                        var pcB = colB.GetComponent<PlayerController>();
                        if (pcA != null) pcA.SetPlayerCollisionInfo(new PlayerCollisionInfo(colB, collisionPoint, direction, distance, true));
                        if (pcB != null) pcB.SetPlayerCollisionInfo(new PlayerCollisionInfo(colA, collisionPoint, -direction, distance, true));
                    }
                    // NOTE: do not set "false" here — players are cleared once at the top of Update.
                }
            }

            // PLAYER VS TRACK
            for (int i = 0; i < playerColliders.Count; i++)
            {
                Collider playerCol = playerColliders[i];

                Vector3 direction;
                float distance;

                bool isOverlapping = Physics.ComputePenetration(
                    playerCol, playerCol.transform.position, playerCol.transform.rotation,
                    trackMainCollider, trackMainCollider.transform.position, trackMainCollider.transform.rotation,
                    out direction, out distance
                );

                if (isOverlapping)
                {
                    Vector3 collisionPoint = playerCol.transform.position + direction * distance * 0.5f;

                    playerCol.GetComponent<PlayerController>()
                        .SetTrackCollisionInfo(
                            new PlayerCollisionInfo(trackMainCollider, collisionPoint, direction, distance, true)
                        );
                }
                else
                {
                    playerCol.GetComponent<PlayerController>()
                        .SetTrackCollisionInfo(
                            new PlayerCollisionInfo(null, Vector3.zero, Vector3.zero, 0f, false)
                        );
                }
            }
        }

    }
}
