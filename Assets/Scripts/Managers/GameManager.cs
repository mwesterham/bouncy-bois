using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using MLAPI.Spawning;
using MLAPI.NetworkVariable;

public class GameManager : NetworkBehaviour
{
    public GameObject hammer;

    public UIManager uiManager;

    public NetworkVariable<float> highScore;

    [ServerRpc(RequireOwnership = false)] // Only the server can spawn new ones
    public void spawnHammerServerRpc(Vector3 position, Quaternion rotation = new Quaternion()) {
        GameObject h = Instantiate(hammer, position, rotation);
        h.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void addHammerServerRpc(ulong newPlayerOwner, ulong spawnedObject) {
        setOwnershipOfServerRpc(newPlayerOwner, spawnedObject);
        
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{newPlayerOwner}
            }
        };
        NetworkObject player = player = getServerPlayerFromId(newPlayerOwner);
        Player playerScript = player.GetComponent<Player>();

        // Add hammers to server side, this is done automatically for hosts since they are the server
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

    [ServerRpc(RequireOwnership = false)]
    public void updateHighScoreServerRpc(ulong playerId, float score) {
        NetworkObject player = getServerPlayerFromId(playerId);
        Player playerScript = player.GetComponent<Player>();
        playerScript.points = score;

        playerScript.removePlayerScoresClientRpc();

        highScore.Value = 0;
        foreach (NetworkClient p in NetworkManager.Singleton.ConnectedClientsList) {
            float thisPlayerPoints = p.PlayerObject.GetComponent<Player>().points;
            if(thisPlayerPoints > highScore.Value) {
                highScore.Value = thisPlayerPoints;
            }

            playerScript.addPlayerScoreClientRpc(p.ClientId, "[Player " + p.ClientId + " unique name]", thisPlayerPoints);
        }
    }

    // Returns the server's copy of this player
    private NetworkObject getServerPlayerFromId(ulong playerId) {
        return NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject;
    }
}
