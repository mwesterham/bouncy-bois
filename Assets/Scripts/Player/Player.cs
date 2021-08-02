using MLAPI;
using UnityEngine;
using System.Collections.Generic;
using Cinemachine;
using MLAPI.Messaging;
using MLAPI.Spawning;
using MLAPI.Connection;

public class Player : NetworkBehaviour
{
    public Rigidbody playerRb;
    public Transform cam;
    public CinemachineFreeLook cincam;
    public GameObject hammer;

    public float spinAcceleration = 10f, movementAcceleration = 30f;
    public float bunkerDownAcceleration = 30f;
    public float boostMagnitude = 20f, boostCooldown = 5f;

    // private Vector3 inputDirection;
    private Vector3 inputDirection;
    private float nextTimeToBoost;
    private List<Hammer> hammerScripts = new List<Hammer>();

    private void Start()
    {
        if(IsLocalPlayer) {
            Cursor.lockState = CursorLockMode.Locked;
            if(IsHost) {
                UIManager.Instance.setPauseText("Stop Hosting and Quit Game");
                // GlobalGameManager.Instance.hammerController.addHammersServerRpc(1, OwnerClientId);
            }
            else if(IsClient) {
                UIManager.Instance.setPauseText("Quit Game");
                // GlobalGameManager.Instance.hammerController.addHammersServerRpc(1, OwnerClientId);
            }
            // addHammersServerRpc(1);
        }
        else {
            cam.gameObject.SetActive(false);
            cincam.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if(IsLocalPlayer) {
            inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
            movePlayerServerRpc();
            actionFixedServerRpc();
            weildHammers();
        }
    }

    private void Update()
    {
        keyboardHotkeys();
        actionNotFixedServerRpc();
    }

    private void keyboardHotkeys() {
        if(IsLocalPlayer) {
            if(Input.GetKeyDown(KeyCode.Escape)) {
                UIManager.Instance.setPausePanelActive(!UIManager.Instance.pausePanel.activeSelf);
                if(UIManager.Instance.pausePanel.activeSelf) {
                    Cursor.lockState = CursorLockMode.None;
                }
                else {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
        if(IsServer && Input.GetKeyDown(KeyCode.H)) // Add hammers to all clients
            addHammersServerRpc(1);
    }

    [ServerRpc] // Send to server from client
    private void movePlayerServerRpc() {
        movePlayerClientRpc();
    }

    [ClientRpc] // Server sends to all clients
    private void movePlayerClientRpc() {
        if(inputDirection.magnitude > 0f) {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            playerRb.AddForce(moveDirection.normalized * movementAcceleration);
        }
    }

    [ServerRpc]
    private void actionFixedServerRpc() {
        actionFixedClientRpc();
    }

    [ClientRpc]
    private void actionFixedClientRpc() {
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            playerRb.AddForce(bunkerDownAcceleration * Vector3.down);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void actionNotFixedServerRpc() {
        actionNotFixedClientRpc();
    }

    [ClientRpc]
    private void actionNotFixedClientRpc() {
        if(Input.GetKeyDown(KeyCode.Space) && Time.time >= nextTimeToBoost) {
            // Stop player
            playerRb.velocity = new Vector3();

            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            Vector3 fireDirection = Quaternion.Euler(-10f, targetAngle, 0f) * Vector3.forward;
            playerRb.AddForce(fireDirection * boostMagnitude, ForceMode.Impulse);
            nextTimeToBoost = Time.time + boostCooldown;
        }
    }

    // [ServerRpc]
    // private void weildHammerServerRpc() {
    //     weildHammerClientRpc();
    // }

    // [ClientRpc]
    // private void weildHammerClientRpc() {
    //     if(Input.GetButton("Fire1")) {
    //         foreach (Hammer h in hammerScripts) {
    //             h.speed.Value += 1f;
    //         }
    //     }
    //     else if(Input.GetButton("Fire2")) {
    //         foreach (Hammer h in hammerScripts) {
    //             h.speed.Value -= 1f;
    //         }
    //     }
    //     else {
    //         foreach (Hammer h in hammerScripts) {
    //             h.speed.Value *= .995f;
    //         }
    //     }
    // }

    private void weildHammers() {
        if(Input.GetButton("Fire1")) {
            foreach (Hammer h in hammerScripts) {
                h.speed.Value += 1f;
            }
        }
        else if(Input.GetButton("Fire2")) {
            foreach (Hammer h in hammerScripts) {
                h.speed.Value -= 1f;
            }
        }
        else {
            foreach (Hammer h in hammerScripts) {
                h.speed.Value *= .995f;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)] // anyone can call
    private void addHammersServerRpc(int number) {
        if(IsServer) {
            for (int i = 0; i < number; i++) {
                GameObject h = Instantiate(hammer, new Vector3(Random.Range(-5,5),Random.Range(-5,5),Random.Range(-5,5)), new Quaternion());
                h.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
                h.GetComponent<Hammer>().target = transform;

                ulong itemNetID = h.GetComponent<NetworkObject>().NetworkObjectId;
                addHammerClientRpc(itemNetID);
            }
        }
    }

    [ClientRpc]
    public void addHammerClientRpc(ulong hammerId) {
        Hammer h = NetworkSpawnManager.SpawnedObjects[hammerId].gameObject.GetComponent<Hammer>();
        h.target = transform;
        hammerScripts.Add(h);
    }
}
