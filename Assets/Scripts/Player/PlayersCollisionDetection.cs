using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayersCollisionDetection : MonoBehaviour
{
    public List<PlayerController> players;
    public bool executeCollisionDetection = false;

    private List<Collider> playerColliders = new List<Collider>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
                        // Punto approssimativo della collisione
                        Vector3 collisionPoint = colA.transform.position + direction * distance * 0.5f;

                        Debug.Log($"Collision detected between {colA.name} and {colB.name}");
                        Debug.Log($"Direction: {direction}, Penetration distance: {distance}, Point: {collisionPoint}");


                        colA.GetComponent<PlayerController>().onOtherPlayerCollisionDetected(colB);
                        colB.GetComponent<PlayerController>().onOtherPlayerCollisionDetected(colA);
                    }
                }
            }
        }

    }
}
