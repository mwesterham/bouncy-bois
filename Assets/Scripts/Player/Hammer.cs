using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Spawning;

public class Hammer : NetworkBehaviour
{
    public Transform target;
    public NetworkVariable<float> radius = new NetworkVariable<float>(new NetworkVariableSettings {WritePermission = NetworkVariablePermission.Everyone}, .1f);
    public NetworkVariable<float> speed = new NetworkVariable<float>(new NetworkVariableSettings {WritePermission = NetworkVariablePermission.Everyone}, 0f);
    public NetworkVariable<float> minSpeed = new NetworkVariable<float>(new NetworkVariableSettings {WritePermission = NetworkVariablePermission.Everyone}, 15f);
    public NetworkVariable<float> maxSpeed = new NetworkVariable<float>(new NetworkVariableSettings {WritePermission = NetworkVariablePermission.Everyone}, 25f);

    public float lifeSpan = 60f;
    private float originalY, originalSin, rotationSpeed = 50f, floatStrength = 0.3f;

    private void Start() {
        if(Random.Range(1,3) == 1)
            rotationSpeed *= -1;
        originalY = transform.position.y + floatStrength;
        originalSin = Random.Range(-3.1415926f * 2, 3.1415926f * 2);
    }
     
    void FixedUpdate ()
    {
        if(target == null) {
            if(IsServer) {
                transform.Rotate(0, Time.deltaTime * rotationSpeed, 0, Space.Self);
                transform.localPosition = new Vector3(transform.position.x, originalY + (Mathf.Sin(originalSin + Time.time) * floatStrength), transform.position.z);
            
                if(lifeSpan <= 0)
                    Destroy(this.gameObject);
                else
                    lifeSpan -= Time.deltaTime;
            }
            return;
        }
        
        speed.Value = Mathf.Clamp(speed.Value, minSpeed.Value, maxSpeed.Value);

        Vector3 relativePos = (target.position + new Vector3(0, 0f, 0)) - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos);
        
        Quaternion current = transform.localRotation;
        
        transform.localRotation = Quaternion.Slerp(current, rotation, radius.Value);
        transform.Translate(0, 0, speed.Value * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if(target == null && other.gameObject.tag == "Players") {
            target = other.gameObject.transform;
            NetworkObject player = other.gameObject.GetComponent<NetworkObject>();
            if(player.IsOwner) {
                GlobalGameManager.Instance.GameManager.addHammerServerRpc(
                    player.OwnerClientId, 
                    this.GetComponent<NetworkObject>().NetworkObjectId
                );
            }
        }

        if(other.gameObject.tag == "Players" || other.gameObject.tag == "Interactables") {
            if(other.gameObject.transform != target) {
                Vector3 fireDirection = other.gameObject.transform.position - target.position;
                fireDirection.Normalize();
                other.gameObject.GetComponent<Rigidbody>().AddForce(fireDirection * 40f, ForceMode.Impulse);
            }
        }
    }
}
