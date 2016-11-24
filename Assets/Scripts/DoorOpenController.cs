using UnityEngine;
using System.Collections;

public class DoorOpenController : MonoBehaviour {
	public bool open = false;
	public float doorOpenAnger;// = 270f;
	public float doorCloseAnger;// = 0f;
	public float smooth = 2f;

    public GameObject player;
    PlayerMovementController pmc;

	// Use this for initialization
	void Start () {
        pmc = (PlayerMovementController) player.GetComponent ( "PlayerMovementController" );
	}
	
	// Update is called once per frame
	void Update () {
		if (open) {
			Quaternion targetRotation = Quaternion.Euler(0, doorOpenAnger, 0);
			transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
            //transform.localPosition = transform.localPosition + new Vector3(-0.02f, 0f, 0f);
		} else {
			Quaternion targetRotation2 = Quaternion.Euler(0, doorCloseAnger, 0);
			transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation2, smooth * Time.deltaTime);
		}
	}

	void OnTriggerEnter(Collider other) {
        Debug.Log("OnTriggerEnter : other.gameObject.tag = " + other.gameObject.tag + " open = " + open);
        if (other.gameObject.tag == "SKELETON_HAND") {
			//Debug.Log("OnTriggerEnter");
			//this.gameObject.SetActive(false);
			changeDoorState();
		}
        
        if ( this.gameObject.tag == "DOOR_OUTSIDE" ) {
            AudioSource doorBell = (AudioSource) this.GetComponent<AudioSource> ( ) as AudioSource;
            if ( doorBell.isPlaying ) {
                doorBell.Stop ( );               
            }
            pmc.gp.pickUpDoor ( "DoorOpenController" );
        }
	}

	void OnTriggerExit(Collider other) {
		/*if (other.gameObject.tag == "Player") {
			Debug.Log("OnTriggerExit");
			this.gameObject.SetActive(true);
		}*/
	}

	public void changeDoorState() {
		open = !open;
	}
}
