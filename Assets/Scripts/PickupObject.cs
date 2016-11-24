using UnityEngine;
using System.Collections;

public class PickupObject : MonoBehaviour {
    public Transform hand;

    void OnTriggerEnter(Collider other) {
        if( other.gameObject.tag == "SKELETON_HAND") {
            GetComponent<Rigidbody>().useGravity = false;
            this.transform.position = hand.position;
            this.transform.parent = GameObject.FindGameObjectWithTag("SKELETON_HAND").transform;
        }
    }
}
