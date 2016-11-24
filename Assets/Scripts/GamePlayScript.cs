using UnityEngine;
using System.Collections;

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
        //setTask ( currentTask );
    }

    public void pickUpMedecine() {
        if (isMedecineTaskDone == false) {
            this.isMedecineTaskDone = true;
            numberOfTaskCompleted++;
        } else {
            Debug.Log("Too many madecine!");
        }
    }

    public void pickUpPhone() {
        if (this.currentRunNumber == 0 || this.currentRunNumber == 5) {
            if (isPhoneTaskDone == false) {
                isPhoneTaskDone = true;
                numberOfTaskCompleted++;
            }

        } else {
            if (isTaskPhone && !isDuplicate) {
                isPhoneTaskDone = true;
                numberOfTaskCompleted++;
                isDuplicate = true;
            }
        }
    }

    public void pickUpDoor(string callingFunc) {
        //Debug.Log ( callingFunc );
        //Debug.Log ( "pickUpDoor : numberOfTaskCompleted = " + numberOfTaskCompleted );
        if (this.currentRunNumber == 0 || this.currentRunNumber == 5) {
            if (isDoorTaskDone == false) {
                isDoorTaskDone = true;
                numberOfTaskCompleted++;
            }
        } else {
            if (isTaskDoor && !isDuplicate) {
                numberOfTaskCompleted++;
                isDuplicate = true;
            }
        }
        //Debug.Log ( "pickUpDoor : currentRunNumber = " + currentRunNumber );
        //Debug.Log ( "pickUpDoor : numberOfTaskCompleted = " + numberOfTaskCompleted );
    }

    public void setTask(int typeOfTask) {
        this.numberOfTaskCompleted = 0;
        this.currentRunNumber++;
        this.currentTask = typeOfTask;
        isPhoneTaskDone = false;
        isDoorTaskDone = false;
        isMedecineTaskDone = false;

        initializeScore();
        if (currentRunNumber == 0) {
            typeOfTask = 2;
        } else if (currentRunNumber == 1) {
            typeOfTask = 1;
        } else if (currentRunNumber == 5) {
            typeOfTask = 0;
        }
        switch (typeOfTask) {
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
        if (isTaskPhone) {
            return 1;   // RIGHT_PHONE_RED
        } else if (isTaskDoor) {
            return 0;   // LEFT_DOOR_YELLOW
        }
        return 0;
    }

    private void initializeScore() {
        //numberOfSteps = 0;
        //timeToCompleteTrial = 0;
        isInBtnStartPointAndDoor = false;
        isInBtnDoorAndJunction = false;
        isInBtnJunctionAndMedicine = false;
    }

    public int getRandomNumber(int min, int max) {
        return Random.Range(min, max);
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
