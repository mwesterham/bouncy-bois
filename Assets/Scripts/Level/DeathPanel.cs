using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class DeathPanel : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Players") {
            other.gameObject.GetComponent<Rigidbody>().velocity = new Vector3();
            other.gameObject.transform.rotation = new Quaternion();
            other.gameObject.transform.position = new Vector3(Random.Range(-5f,5f),0,Random.Range(-5f,5f));
        }

        if(other.gameObject.tag == "Interactables") {
            Destroy(other.gameObject);
        }
    }
}
