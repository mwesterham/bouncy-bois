using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;

public class GameManager : NetworkBehaviour
{
    public GameObject hammer;

    [ServerRpc(RequireOwnership = true)] // Only the server can spawn new ones
    public void spawnHammerServerRpc(Vector3 position) {
        GameObject h = Instantiate(hammer, position, new Quaternion());
        h.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
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
        Player player = NetworkManager.Singleton.ConnectedClients[newPlayerOwner].PlayerObject.GetComponent<Player>();
        player.addHammerClientRpc(spawnedObject, clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)] // Any Client can call this
    public void playerDiedServerRpc(ulong playerId) {
        GameObject player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.gameObject;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{playerId}
            }
        };

        player.GetComponent<Player>().playerDiedClientRpc(clientRpcParams);
    }

    [ServerRpc(RequireOwnership = true)]
    public void setOwnershipOfServerRpc(ulong newPlayerOwner, ulong spawnedObject) {
        NetworkSpawnManager.SpawnedObjects[spawnedObject].ChangeOwnership(newPlayerOwner);
    }
}
