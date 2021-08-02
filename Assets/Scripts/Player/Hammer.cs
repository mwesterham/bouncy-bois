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
    
    private void Start() {
        
    }
     
    void FixedUpdate ()
    {
        speed.Value = Mathf.Clamp(speed.Value, minSpeed.Value, maxSpeed.Value);

        Vector3 relativePos = (target.position + new Vector3(0, 0f, 0)) - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos);
        
        Quaternion current = transform.localRotation;
        
        transform.localRotation = Quaternion.Slerp(current, rotation, radius.Value);
        transform.Translate(0, 0, speed.Value * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Players" || other.gameObject.tag == "Interactables") {
            Debug.Log("1" + other.gameObject.name);
            if(other.gameObject.transform != target) {
                Debug.Log("2: " + other.gameObject.name);
                Vector3 fireDirection = other.gameObject.transform.position - target.position;
                fireDirection.Normalize();
                other.gameObject.GetComponent<Rigidbody>().AddForce(fireDirection * 40f, ForceMode.Impulse);
            }
        }
    }
}