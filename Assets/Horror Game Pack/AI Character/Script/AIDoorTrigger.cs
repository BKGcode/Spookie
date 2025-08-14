using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDoorTrigger : MonoBehaviour
{
    public DoorManager cabinetdoor;
    public string AITag;
    private GameObject AI;

    private void Start()
    {
        AI = GameObject.FindGameObjectWithTag(AITag);
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject == AI && !cabinetdoor.doorOpened)
        {
            cabinetdoor.ToggleCabinet();
        }
    }
}
