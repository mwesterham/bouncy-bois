using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class DeathPanel : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Players") {
            GlobalGameManager.Instance.GameManager.playerDiedServerRpc(other.gameObject.GetComponent<NetworkObject>().OwnerClientId);
        }

        if(other.gameObject.tag == "Interactables") {
            Destroy(other.gameObject);
        }
    }
}
