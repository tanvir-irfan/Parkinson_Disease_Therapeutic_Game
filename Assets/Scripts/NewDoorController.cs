using UnityEngine;
using System.Collections;

public class NewDoorController : MonoBehaviour {

    Animator animator;
    public bool isDoorOpen = false;
    public GameObject doorKnob, player, delivaryBoy;
    PlayerMovementController pmc;
    public UnityEngine.AudioSource doorSound;
	void Start () {        
        animator = GetComponent<Animator> ( );
        pmc = (PlayerMovementController) player.GetComponent ( "PlayerMovementController" );
	}
		
	void OnTriggerEnter (Collider col) {        
        if ( col.gameObject.tag == "SKELETON_HAND" && pmc.reachForObject) {
            bool isKnobActive = false;
            if ( this.gameObject.tag == "DOOR_OUTSIDE" ) {
                isKnobActive = true;
            }
            controllDoor ( "DoorOpenT", isKnobActive );
            if ( this.gameObject.tag == "DOOR_OUTSIDE" ) {
                AudioSource doorBell = (AudioSource) this.GetComponent<AudioSource> ( ) as AudioSource;
                if ( doorBell.isPlaying ) {
                    doorBell.Stop ( );
                }
                //Debug.Log ( "NewDoorController : pickUpDoor" );
                pmc.gp.pickUpDoor ( "NewDoorController" );
                pmc.closeHallway(1, false);
                Invoke("deliverPackage", 3);
            }
        }        
	}

    public void controllDoor ( string direction, bool knob ) {
        if ( !isDoorOpen && direction.Equals ( "DoorOpenT" ) ) {            
            if ( doorSound != null && !doorSound.isPlaying ) {
                doorSound.PlayOneShot ( doorSound.clip );
            }
            doorKnob.SetActive ( knob );
            isDoorOpen = true;
            animator.SetTrigger ( direction );
            //Debug.Log ( "direction = " + direction );
        } else if ( direction.Equals ( "DoorCloseT" ) ) {
            doorKnob.SetActive ( true );
            isDoorOpen = false;
            animator.SetTrigger ( direction );
        }                
    }

    private bool packageDelivered = false;

    private void deliverPackage() {
        if (!packageDelivered) {
            pmc.outsideAvatar.GetComponent<Animator>().SetBool("deliverThePackage", true);
            pmc.receivePackage();
            packageDelivered = true;
        }        
    }
}
