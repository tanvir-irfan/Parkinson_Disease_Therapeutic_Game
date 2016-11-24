using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour {

    public Text counterText;
    public float seconds, minutes, hours;
    public GameObject player;

    public float hourOffset = 0;
    private float timeStartOffset = 0;
    private bool isCountingTime;   

    PlayerMovementController pmc;

    // Use this for initialization
    void Start() {
        counterText = GetComponent<Text>() as Text;
        pmc = (PlayerMovementController)player.GetComponent("PlayerMovementController");              
    }

    // Update is called once per frame
    void Update() {        
        if (isCountingTime) {
            hours = (int)((Time.time - timeStartOffset) / 3600f);
            switch (pmc.gp.currentRunNumber) {
                case 0:
                case 1:
                hourOffset = 0;
                break;
                case 2:
                case 3:
                case 4:
                case 5:
                if(pmc.gp.currentTask == GamePlayScript.TASK_PHONE)
                    hourOffset = GamePlayScript.TIME_PHONE;
                else
                    hourOffset = GamePlayScript.TIME_DOOR;
                break;
                default:
                hourOffset = 0;
                break;
            }

            hours = hours + hourOffset;
            minutes = (int)((Time.time - timeStartOffset) / 60f);
            seconds = (int)((Time.time - timeStartOffset) % 60f);

            if (hours >= 24)
                hours %= 24;
            if (minutes >= 60)
                minutes %= 60;

            counterText.text = pmc.gp.currentRunNumber.ToString("00") + " : " + hours.ToString("00") + " : " + minutes.ToString("00") + " : " + seconds.ToString("00");
        } else {
            counterText.text = "00 : 00 : 00 : 00";            
        }        
    }

    public void showTime(bool enabled) {
        
        timeStartOffset = 0;

        this.gameObject.SetActive ( enabled );
        isCountingTime = enabled;
        if (enabled)
            timeStartOffset = Time.time;
        else timeStartOffset = 0;
    }

}
