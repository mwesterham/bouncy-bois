using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;

public class GameManager : NetworkBehaviour
{
    public GameObject hammer;

    [ServerRpc(RequireOwnership = false)] // Only the server can spawn new ones
    public void spawnHammerServerRpc(Vector3 position) {
        GameObject h = Instantiate(hammer, position, new Quaternion());
        h.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc(RequireOwnership = true)]
    public void addHammerServerRpc(ulong newPlayerOwner, ulong spawnedObject) {
        setOwnershipOfServerRpc(newPlayerOwner, spawnedObject);
        
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{newPlayerOwner}
            }
        };
        NetworkObject player = player = NetworkManager.Singleton.ConnectedClients[newPlayerOwner].PlayerObject;
        Player playerScript = player.GetComponent<Player>();

        // Add hammers to server side
        Hammer h = NetworkSpawnManager.SpawnedObjects[spawnedObject].gameObject.GetComponent<Hammer>();
        playerScript.hammerScripts.Add(h);

        // Add hammers to speciifc client side
        playerScript.addHammerClientRpc(spawnedObject, clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)] // Any Client can call this
    public void playerDiedServerRpc(ulong playerId) {
        GameObject player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.gameObject;
        Player playerScript = player.GetComponent<Player>();

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{playerId}
            }
        };

        playerScript.playerDiedClientRpc(clientRpcParams);

        // Destroy hammers server-side => propogates to all clients
        foreach (Hammer h in playerScript.hammerScripts) {
            Destroy(h.gameObject);
        }
        playerScript.hammerScripts.Clear();
    }

    [ServerRpc(RequireOwnership = true)]
    public void setOwnershipOfServerRpc(ulong newPlayerOwner, ulong spawnedObject) {
        NetworkSpawnManager.SpawnedObjects[spawnedObject].ChangeOwnership(newPlayerOwner);
    }
}
