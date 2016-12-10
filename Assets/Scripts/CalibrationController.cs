using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CalibrationController : MonoBehaviour {
    public GameObject calStartB, calDoneB, instructionB, pauseB,
        calScreenPanel, welcomePanel, goodByePanel, player, isKinectControlledT, isDemoRunningT, isThirdPersonCamT, timetTextT;
    public Text calScreenText;
    PlayerMovementController pmc;
    TimerScript ts;
    bool isGamePaused = false;
    AudioSource instAudioSource;

    public double CALIBRATION_TIME;// = Configuration.CALIBRATION_TIME;

    public void Start() {
        calScreenPanel.SetActive(false);
        calStartB.SetActive(false);
        calDoneB.SetActive(false);

        welcomePanel.SetActive(true);
        instructionB.SetActive(true);

        pauseB.SetActive(false);
        goodByePanel.SetActive(false);
        pmc = ( PlayerMovementController ) player.GetComponent("PlayerMovementController");

        CALIBRATION_TIME = Configuration.CALIBRATION_TIME;
        instAudioSource = ( AudioSource ) this.GetComponent<AudioSource>();
        //Debug.Log("instAudioSource = " + instAudioSource);

        pmc.isPlayerMovementAllowed = false;

        ts = ( TimerScript ) timetTextT.GetComponent("TimerScript");
    }

    public void Update() {
        if ( pmc.gp.getIsGameOver() ) {
            goodByePanel.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void onCalibtaionStart() {
        pmc.isCalibrationStarted = true;
        pmc.isCalibrationDone = false;

        calScreenText.text = "Please stand Still. Calibration is going on.";
        calDoneB.SetActive(true);
        calStartB.SetActive(false);

        pmc.isPlayerMovementAllowed = false;

        //tanvirirfan.utsa mew code
        calDoneB.SetActive(false);
        StartCoroutine(onCalibtaionDone((float)CALIBRATION_TIME));
    }

    public IEnumerator onCalibtaionDone(float waitTime) {
        yield return new WaitForSeconds(waitTime);

        calScreenPanel.SetActive(false);
        calStartB.SetActive(false);
        calDoneB.SetActive(false);

        pmc.isCalibrationDone = true;
        pmc.calibrationDoneTime = Time.time;
        //pmc.isCalibrationStarted = false;   // I do not want the calibration to go on after Done button is pressed!

        pmc.isPlayerMovementAllowed = true;
        pauseB.SetActive(true);

        ts.showTime(true); // now it's time to show the time.
    }

    public void onClickInstructionButton() {
        instructionB.SetActive(false);
        pmc.isPlayerMovementAllowed = false;

        bool isKinectControlled = isKinectControlledT.GetComponent<Toggle>().isOn;
        bool isdemo = isDemoRunningT.GetComponent<Toggle>().isOn;
        bool isThirdPersonCam = isThirdPersonCamT.GetComponent<Toggle>().isOn;

        //Debug.Log("isKinectControlled = " + isKinectControlled);
        isKinectControlledT.SetActive(false);
        isDemoRunningT.SetActive(false);
        isThirdPersonCamT.SetActive(false);

        pmc.setKinectControlled(isKinectControlled);
        pmc.setIsDemo(isdemo);
        pmc.isThirdPersonCam(isThirdPersonCam);

        if ( isdemo ) {
            AudioFinished();
        } else {
            // play the audio.
            PlaySoundWithCallback(instAudioSource.clip, AudioFinished);
            // when audio is done playing, show the calibration window!
        }
    }


    public delegate void AudioCallback();
    private void PlaySoundWithCallback(AudioClip clip, AudioCallback callback) {
        if ( pmc.isDemo ) {
            Debug.Log("isDemo " + pmc.isDemo);
            AudioFinished();
            return;
        }
        instAudioSource.Play();
        StartCoroutine(DelayedCallback(clip.length, callback));
    }
    private IEnumerator DelayedCallback(float time, AudioCallback callback) {
        yield return new WaitForSeconds(time);
        callback();
    }

    void AudioFinished() {
        // Hide the welcome screen
        welcomePanel.SetActive(false);
        instructionB.SetActive(false);

        // Show the calibration screen
        calScreenPanel.SetActive(true);
        calStartB.SetActive(true);

        if ( instAudioSource.isPlaying ) {
            instAudioSource.Stop();
        }

    }

    public void onClickPauseButton() {
        string text = "";
        if ( isGamePaused ) {
            Time.timeScale = 1;
            text = "Pause";
        } else {
            Time.timeScale = 0;
            text = "Resume";
        }
        pauseB.GetComponentInChildren<Text>().text = text;
        isGamePaused = !isGamePaused;
    }
}
