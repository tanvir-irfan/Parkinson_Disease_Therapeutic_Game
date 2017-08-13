using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GamePlayScript {

    public const int NUMBER_OF_RUN = 6;
    public int[] NUMBER_OF_TASK = { 2, 2, 2, 2, 2, 3 };

    public const int TASK_TRIAL = 0;
    public const int TASK_PHONE = 1;
    public const int TASK_DOOR = 2;

    public const int TIME_TRIAL = 0;
    public const int TIME_PHONE = 8;
    public const int TIME_DOOR = 11;

    public int currentRunNumber;
    public int currentTask;
    public bool isTaskPhone, isTaskDoor, isTaskRedMedecine;
    private bool isPhoneTaskDone = false, isDoorTaskDone = false, isMedecineTaskDone = false;
    public int numberOfTaskCompleted;
    
    //NEW
    public int wrongMedicinePicked;

    public bool isDuplicate = false;
    public bool playInstruction = false;

    //Scoring
    public int[] numberOfSteps;
    public float timeToCompleteTrial;
    public bool isInBtnStartPointAndDoor;
    public bool isInBtnDoorAndJunction;
    public bool isInBtnJunctionAndMedicine;
    public float[] timeToCrossFirst3M;      //START => DOOR;
    public float[] timeToCrossSecond3M;     //DOOR => JUNCTION;
    public float[] timeToCrossThird3M;      //JUNCTION => MADECINE;
    public int currentPosition;             //
    private bool isGameOver = false;

    public GamePlayScript() {
        currentRunNumber = -1;
        currentTask = 0;
        initializeScore();
        timeToCrossFirst3M = new float[NUMBER_OF_RUN];      //START => DOOR;
        timeToCrossSecond3M = new float[NUMBER_OF_RUN];     //DOOR => JUNCTION;
        timeToCrossThird3M = new float[NUMBER_OF_RUN];      //JUNCTION => MADECINE;
        numberOfSteps = new int[NUMBER_OF_RUN];


        populateConfiguration();
    }

    private void populateConfiguration() {
        Dictionary<String, double> conf = UtilitiesScript.readConfigurationFile();

        conf.TryGetValue("WALK_DURATION", out Configuration.WALK_DURATION);
        conf.TryGetValue("TURN_ANGLE", out Configuration.TURN_ANGLE);
        conf.TryGetValue("FOOT_MOVEMENT_THRESHOLD", out Configuration.FOOT_MOVEMENT_THRESHOLD);
        conf.TryGetValue("LEAN_BACK_THRESHOLD", out Configuration.LEAN_BACK_THRESHOLD);
        conf.TryGetValue("HAND_FORWARD_THRESHOLD", out Configuration.HAND_FORWARD_THRESHOLD);
        conf.TryGetValue("HAND_SIDEWISE_THRESHOLD", out Configuration.HAND_SIDEWISE_THRESHOLD);
        conf.TryGetValue("TURN_LEAN_THRESHOLD", out Configuration.TURN_LEAN_THRESHOLD);
        conf.TryGetValue("CALIBRATION_TIME", out Configuration.CALIBRATION_TIME);

    }

    public void pickUpMedecine ( bool isProperMedicine ) {
        //if ( isMedecineTaskDone == false ) {
        if ( isProperMedicine ) {
            this.isMedecineTaskDone = true;
            numberOfTaskCompleted++;
            Debug.Log ( "numberOfTaskCompleted = " + numberOfTaskCompleted + " / " + NUMBER_OF_TASK [ currentRunNumber ] );
        } else {
            wrongMedicinePicked++;
        }
        //} else {
        //    Debug.Log("Too many madecine!");
        //}
    }

    public void pickUpPhone() {
        if ( this.currentRunNumber == 0 || this.currentRunNumber == 5 ) {
            if ( isPhoneTaskDone == false ) {
                isPhoneTaskDone = true;
                numberOfTaskCompleted++;
                Debug.Log ( "numberOfTaskCompleted = " + numberOfTaskCompleted + " / " + NUMBER_OF_TASK [ currentRunNumber ] );
            }

        } else {
            if ( isTaskPhone && !isDuplicate ) {
                isPhoneTaskDone = true;
                numberOfTaskCompleted++;
                isDuplicate = true;
            }
        }
    }

    public void pickUpDoor(string callingFunc) {
        //Debug.Log ( callingFunc );
        //Debug.Log ( "pickUpDoor : numberOfTaskCompleted = " + numberOfTaskCompleted );
        if ( this.currentRunNumber == 0 || this.currentRunNumber == 5 ) {
            if ( isDoorTaskDone == false ) {
                isDoorTaskDone = true;
                numberOfTaskCompleted++;
		Debug.Log ( "numberOfTaskCompleted = " + numberOfTaskCompleted + " / " + NUMBER_OF_TASK [currentRunNumber]);
            }
        } else {
            if ( isTaskDoor && !isDuplicate ) {
                numberOfTaskCompleted++;
                isDuplicate = true;
            }
        }
    }

    public void setTask(int typeOfTask) {
        this.numberOfTaskCompleted = 0;
        this.currentRunNumber++;
        this.currentTask = typeOfTask;
        isPhoneTaskDone = false;
        isDoorTaskDone = false;
        isMedecineTaskDone = false;

        initializeScore();
        if ( currentRunNumber == 0 ) {
            typeOfTask = 2;
        } else if ( currentRunNumber == 1 ) {
            typeOfTask = 1;
        } else if ( currentRunNumber == 5 ) {
            typeOfTask = 0;
        }
        switch ( typeOfTask ) {
            case 0:     //Trial
                isTaskPhone = true;
                isTaskDoor = true;
                isTaskRedMedecine = true;
                break;
            case 1:     //Answer the Phone
                isTaskPhone = true;
                isTaskDoor = false;
                isTaskRedMedecine = true;
                break;
            case 2:     //Answer the door
                isTaskPhone = false;
                isTaskDoor = true;
                isTaskRedMedecine = false;
                break;
        }
    }

    public int getIndexOfTask() {
        if ( isTaskPhone ) {
            return 1;   // RIGHT_PHONE_RED
        } else if ( isTaskDoor ) {
            return 0;   // LEFT_DOOR_YELLOW
        }
        return 0;
    }

    private void initializeScore() {
        //numberOfSteps = 0;
        //timeToCompleteTrial = 0;
        wrongMedicinePicked = 0;
        isInBtnStartPointAndDoor = false;
        isInBtnDoorAndJunction = false;
        isInBtnJunctionAndMedicine = false;
    }

    public int getRandomNumber(int min, int max) {
        return UnityEngine.Random.Range(min, max);
    }

    public bool isAllTaskDone() {
        return this.numberOfTaskCompleted == this.NUMBER_OF_TASK[this.currentRunNumber];
    }

    public void setIsGameOver(bool gameOver) {
        isGameOver = gameOver;
    }

    public bool getIsGameOver() {
        return isGameOver;
    }

    public bool isComplexTaskTrial() {
        return this.currentRunNumber >= 2 && this.currentRunNumber <= 4;
    }
}

public class Configuration {
    public static double WALK_DURATION;
    public static double TURN_ANGLE;
    public static double FOOT_MOVEMENT_THRESHOLD;
    public static double LEAN_BACK_THRESHOLD;

    public static double HAND_FORWARD_THRESHOLD;
    public static double HAND_SIDEWISE_THRESHOLD;
    public static double TURN_LEAN_THRESHOLD;
    public static double CALIBRATION_TIME;

}