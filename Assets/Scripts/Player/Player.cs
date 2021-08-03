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
    private UIManager uiManager;
    private bool gamePaused = false;

    private void Start()
    {
        if(IsLocalPlayer) {
            uiManager = GlobalGameManager.Instance.UIManager;
            Cursor.lockState = CursorLockMode.Locked;
            if(IsHost) {
                uiManager.setPauseText("Stop Hosting and Quit Game");
            }
            else if(IsClient) {
                uiManager.setPauseText("Quit Game");
            }
        }
        else {
            cam.gameObject.SetActive(false);
            cincam.gameObject.SetActive(false);
        }
    }

    private void Update() {
        if(IsLocalPlayer) {
            // Game pausing takes precendence
            if(Input.GetKeyDown(KeyCode.Escape)) {
                gamePaused = !gamePaused;
                uiManager.setPausePanelActive(gamePaused);
                if(gamePaused) {
                    Cursor.lockState = CursorLockMode.None;
                    cam.gameObject.SetActive(false);
                }
                else {
                    Cursor.lockState = CursorLockMode.Locked;
                    cam.gameObject.SetActive(true);
                }
            }
            if(gamePaused)
                return;

            // Update class variables client side
            inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

            // Check for input on client side, don't want to call SerRpc every frame bc it causes a lot of network traffic
            // => minimize the number of ServerRpc calls as much as possible
            handlePlayerInput();
        }
    }

    private void FixedUpdate()
    {
        if(IsLocalPlayer) {
            if(gamePaused)
                return;
            // Check for input on client side, don't want to call SerRpc every frame bc it causes a lot of network traffic
            // => minimize the number of ServerRpc calls as much as possible
            handleFixedPlayerInput();
        }
    }

    /*
     * Handle Player input
     * Anyone manipulation of transforms should be done client side since it is automatically synced
     * Also applies to manipulation of rigidbodies since rbs are dependent on transforms
     */
    private void handlePlayerInput() {
        if(Input.GetKeyDown(KeyCode.Space) && Time.time >= nextTimeToBoost) {
            // Stop player
            playerRb.velocity = new Vector3();

            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            Vector3 fireDirection = Quaternion.Euler(-25f, targetAngle, 0f) * Vector3.forward;
            playerRb.AddForce(fireDirection * boostMagnitude, ForceMode.Impulse);
            nextTimeToBoost = Time.time + boostCooldown;
        }

        if(Input.GetKeyDown(KeyCode.H)) // Call a ServerRpc to spawn a hammer
            GlobalGameManager.Instance.GameManager.spawnHammerServerRpc(new Vector3(0, -1f, 0));

        if(Input.GetKeyDown(KeyCode.L))
            addPersonalHammersServerRpc(1);
    }

    /*
     * Handle Player input (Fixed)
     * Same as above but this is at a fixed rate so physics are consistent
     */
    private void handleFixedPlayerInput() {
        if(inputDirection.magnitude > 0f) {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            playerRb.AddForce(moveDirection.normalized * movementAcceleration);
        }

        if(Input.GetKeyDown(KeyCode.Space) && Time.time >= nextTimeToBoost) {
            // Stop player
            playerRb.velocity = new Vector3();

            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            Vector3 fireDirection = Quaternion.Euler(-10f, targetAngle, 0f) * Vector3.forward;
            playerRb.AddForce(fireDirection * boostMagnitude, ForceMode.Impulse);
            nextTimeToBoost = Time.time + boostCooldown;
        }

        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            playerRb.AddForce(bunkerDownAcceleration * Vector3.down);
        }

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
                h.speed.Value *= .990f;
            }
        }
    }

    [ClientRpc]
    public void addHammerClientRpc(ulong hammerId, ClientRpcParams clientRpcParams = default) {
        Hammer h = NetworkSpawnManager.SpawnedObjects[hammerId].gameObject.GetComponent<Hammer>();
        h.target = transform;
        hammerScripts.Add(h);
    }

    [ClientRpc]
    public void playerDiedClientRpc(ClientRpcParams clientRpcParams = default) {
        // Remember, rigid bodies and transforms must be modified client side
        playerRb.velocity = new Vector3();
        transform.rotation = new Quaternion();
        transform.position = new Vector3(Random.Range(-5f,5f),10,Random.Range(-5f,5f));
        float x = Random.Range(1,4);
        if(x == 1)
            transform.position = new Vector3(Random.Range(-5f,5f),10,5);
        else if (x == 2)
            transform.position = new Vector3(Random.Range(-5f,5f),10,-5);
        else if (x == 3)
            transform.position = new Vector3(-5+Random.Range(-1f,1f),10,Random.Range(-5f,5f));
        else if (x == 4)
            transform.position = new Vector3(5+Random.Range(-1f,1f),10,Random.Range(-5f,5f));

        foreach (Hammer h in hammerScripts) {
            Destroy(h.gameObject);
        }
        hammerScripts.Clear();
    }

    // Client helper function to spawn our own hammers already attached (unused)
    [ServerRpc(RequireOwnership = false)] 
    private void addPersonalHammersServerRpc(int number) {
        for (int i = 0; i < number; i++) {
            GameObject h = Instantiate(hammer, new Vector3(Random.Range(-5,5),Random.Range(-5,5),Random.Range(-5,5)), new Quaternion());
            h.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
            h.GetComponent<Hammer>().target = transform;

            ulong itemNetID = h.GetComponent<NetworkObject>().NetworkObjectId;
            addHammerClientRpc(itemNetID);
        }
    }
}
