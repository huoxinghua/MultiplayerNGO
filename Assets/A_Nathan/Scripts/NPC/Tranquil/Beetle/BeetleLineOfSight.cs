
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using System.Collections;

public class BeetleLineOfSight : MonoBehaviour
{
    [SerializeField] float fieldOfView;
    public float viewDistance;
    [SerializeField] float eyeOffset;
    public List<GameObject> players = new List<GameObject>();
    [SerializeField] float fieldOfViewCheckFrequency;
    [SerializeField] LayerMask viewCastLayerMask;
    [SerializeField] BeetleHealth beetleHealthScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PeriodicCheckFOV()); 
    }
  

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    public void AddPlayerInProximity(GameObject playerToAdd)
    {
        bool playerAlreadyInList = false;
        foreach (var player in players)
        {
            if (player == playerToAdd)
            {
                playerAlreadyInList = true;
            }
        }
        if (playerAlreadyInList) return;

        players.Add(playerToAdd);

    }
    public void RemovePlayerFromProximity(GameObject playerToRemove)
    {
        bool doRemove = false;
        foreach(var player in players)
        {
            if(player == playerToRemove)
            {
                doRemove = true;
                
            }
        }
        if(doRemove) players.Remove(playerToRemove);
    }
    private void CheckFOV()
    {
      //  Debug.Log("Checking");
        foreach (var player in players)
        {
            if (InFOV(player) && HasLineOfSight(player))
            {
                Debug.Log("Player Spotted");
                if(beetleHealthScript.IsPlayerHostile(player))
                {
                    //player is hostile - RUN!
                }
                else
                {
                    //player is friendly. Follow
                }                // Player is visible, engage!
            }
        }
    }
    IEnumerator PeriodicCheckFOV()
    {
        while (true)
        {
            CheckFOV();
            yield return new WaitForSeconds(fieldOfViewCheckFrequency);
        }
    }
    private bool InFOV(GameObject player)
    {
        Vector3 dirToTarget = ((player.transform.position + new Vector3(0, 0.5f, 0)) - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToTarget);

        if (angle < fieldOfView / 2 && Vector3.Distance(transform.position, player.transform.position) < viewDistance)
        {
            return true;
        }
        return false;
    }
    private bool HasLineOfSight(GameObject player)
    {
        Vector3 dirToTarget = ((player.transform.position + new Vector3(0,0.5f,0)) - transform.position).normalized;
        Debug.DrawRay(transform.position + new Vector3(0, eyeOffset, 0), dirToTarget * viewDistance, Color.red, 1f);
        if (Physics.Raycast(transform.position + new Vector3(0,eyeOffset,0), dirToTarget, out RaycastHit hit, viewDistance,~viewCastLayerMask))
        {
            if (hit.transform.gameObject == player)
            {
                // Player is visible (LOS confirmed)
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
}

}
