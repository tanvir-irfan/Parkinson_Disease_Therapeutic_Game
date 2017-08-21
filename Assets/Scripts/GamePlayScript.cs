using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GamePlayScript
{

	public const int NUMBER_OF_RUN = 4;
	public int[] NUMBER_OF_TASK = { 0, 2, 2, 3 };

	public const int TASK_FAMILIARIZATION = 0;
	public const int TASK_SINGLE = TASK_FAMILIARIZATION + 1;
	public const int TASK_DUEL = TASK_SINGLE + 1;

	#region NEW CODE

	public Task[] allTasks;
	public GameObject clockGO;
	Clock clockSc;

	#endregion

	public int currentRunNumber;
	public int currentTask;
	public bool isTaskPhone, isTaskDoor, isTaskRedMedecine;

	public bool playInstruction = false;

	//Scoring
	public float timeToCompleteTrial;
	public bool isInBtnStartPointAndDoor;
	public bool isInBtnDoorAndJunction;
	public bool isInBtnJunctionAndMedicine;

	public int currentPosition;
	//
	private bool isGameOver = false;

	public GamePlayScript ()
	{		
		#region NEW CODE
		allTasks = new Task[NUMBER_OF_RUN];
		#endregion

		Debug.Log ("GamePlayScript : allTasks " + allTasks.Length);
		currentRunNumber = -1;
		currentTask = 0;
		//initializeTask ();

		populateConfiguration ();
	}

	private void populateConfiguration ()
	{
		Dictionary<String, double> conf = UtilitiesScript.readConfigurationFile ();

		conf.TryGetValue ("WALK_DURATION", out Configuration.WALK_DURATION);
		conf.TryGetValue ("TURN_ANGLE", out Configuration.TURN_ANGLE);
		conf.TryGetValue ("FOOT_MOVEMENT_THRESHOLD", out Configuration.FOOT_MOVEMENT_THRESHOLD);
		conf.TryGetValue ("LEAN_BACK_THRESHOLD", out Configuration.LEAN_BACK_THRESHOLD);
		conf.TryGetValue ("HAND_FORWARD_THRESHOLD", out Configuration.HAND_FORWARD_THRESHOLD);
		conf.TryGetValue ("HAND_SIDEWISE_THRESHOLD", out Configuration.HAND_SIDEWISE_THRESHOLD);
		conf.TryGetValue ("TURN_LEAN_THRESHOLD", out Configuration.TURN_LEAN_THRESHOLD);
		conf.TryGetValue ("CALIBRATION_TIME", out Configuration.CALIBRATION_TIME);

	}

	public void pickUpMedecine (bool isProperMedicine)
	{
		if (isProperMedicine) {
			//this.isMedecineTaskDone = true;
		} else {
			//wrongMedicinePicked++;
		}        
	}

	public void pickUpPhone ()
	{
		if (this.currentRunNumber == 0 || this.currentRunNumber == 5) {
			//if (isPhoneTaskDone == false) {
			//	isPhoneTaskDone = true;
			//}

		} else {
			if (isTaskPhone) {
				//isPhoneTaskDone = true;
			}
		}
	}

	public void pickUpDoor (string callingFunc)
	{
		if (this.currentRunNumber == 0 || this.currentRunNumber == 5) {
			//if (isDoorTaskDone == false) {
			//	isDoorTaskDone = true;
			//}
		} else {
			if (isTaskDoor /*&& !isDuplicate*/) {
				//numberOfTaskCompleted++;
				//isDuplicate = true;
			}
		}
	}

	public Task getCurrentTask ()
	{
		return allTasks [this.currentRunNumber];
	}

	public void setPhase (GamePhase gamephase)
	{
		allTasks [this.currentRunNumber].gamePhase = gamephase;
	}

	public void setTask ()
	{
		this.currentRunNumber++;
		allTasks [this.currentRunNumber] = new Task (this.currentRunNumber);

		initializeTask ();
	}

	public int getIndexOfTask ()
	{
		if (isTaskPhone) {
			return 1;   // RIGHT_PHONE_RED
		} else if (isTaskDoor) {
			return 0;   // LEFT_DOOR_YELLOW
		}
		return 0;
	}

	private void initializeTask ()
	{
		Debug.Log ("initializeTask : this.currentRunNumber = " + this.currentRunNumber);
		// runNumber and trialStart is ste when Task is initialized.
		allTasks [this.currentRunNumber].isTrialStarted = false;
		allTasks [this.currentRunNumber].numberofSteps = 0;

		allTasks [this.currentRunNumber].isPhone = false;
		allTasks [this.currentRunNumber].isDoor = false;

		allTasks [this.currentRunNumber].isPhoneDone = false;
		allTasks [this.currentRunNumber].isDoorDone = false;

		allTasks [this.currentRunNumber].door = new List<long> ();
		allTasks [this.currentRunNumber].phone = new List<long> ();

		allTasks [this.currentRunNumber].timeSpentin1st3M = 0;
		allTasks [this.currentRunNumber].timeSpentin2nd3M = 0;
		allTasks [this.currentRunNumber].timeSpentin3rd3M = 0;

		allTasks [this.currentRunNumber].medicine = new Dictionary<MyColor, long> ();

		isInBtnStartPointAndDoor = false;
		isInBtnDoorAndJunction = false;
		isInBtnJunctionAndMedicine = false;
	}

	public int getRandomNumber (int min, int max)
	{
		return UnityEngine.Random.Range (min, max);
	}

	public void SetTrialStartFlag (bool trSt)
	{
		this.allTasks [this.currentRunNumber].isTrialStarted = trSt;
	}

	public bool GetTrialStartFlag ()
	{
		return this.allTasks [this.currentRunNumber].isTrialStarted;
	}

	public void setIsGameOver (bool gameOver)
	{
		isGameOver = gameOver;
	}

	public bool getIsGameOver ()
	{
		return isGameOver;
	}

	public static double GetCurrentMilli ()
	{
		DateTime Jan1970 = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		TimeSpan javaSpan = DateTime.UtcNow - Jan1970;
		return javaSpan.TotalMilliseconds;
	}
}

public class Configuration
{
	public static double WALK_DURATION;
	public static double TURN_ANGLE;
	public static double FOOT_MOVEMENT_THRESHOLD;
	public static double LEAN_BACK_THRESHOLD;

	public static double HAND_FORWARD_THRESHOLD;
	public static double HAND_SIDEWISE_THRESHOLD;
	public static double TURN_LEAN_THRESHOLD;
	public static double CALIBRATION_TIME;

}

public class Task
{

	public Task (int runNumber)
	{
		this.runNumber = runNumber;
		this.trialStart = GamePlayScript.GetCurrentMilli ();

		switch (runNumber) {
		case 0:
			this.gamePhase = GamePhase.Familiarization;
			break;
		case 1:
		case 2:
			this.gamePhase = GamePhase.SingleTask;
			break;
		case 3:
			this.gamePhase = GamePhase.DuelTask;
			break;
		}
		switch (this.gamePhase) {
		case GamePhase.Familiarization:
			this.isPhone = true;
			this.isDoor = true;
			break;
		case GamePhase.DuelTask:
		case GamePhase.SingleTask:			
			SetCorrectMedColor ();
			break;
		}
	}

	private void SetCorrectMedColor ()
	{
		this.time = UnityEngine.Random.Range (0, 23);
		if (this.time % 2 == 0) {
			this.correctMed = MyColor.Red;
		} else {
			this.correctMed = MyColor.Yellow;
		}
	}
	// i will end trial when this flag is true and player entered in the start position.
	// this flag will be on when the player cross some distance from the start position.
	public bool isTrialStarted;

	public GamePhase gamePhase;
	public int time;

	public int runNumber;
	public int numberofSteps;
	public double trialStart;
	public double trialEnd;

	public bool isDoor;
	public bool isPhone;

	public bool isDoorDone;
	public bool isPhoneDone;

	public List<long> door;
	public List<long> phone;
	//START => DOOR
	public float timeSpentin1st3M;
	//DOOR => JUNCTION;
	public float timeSpentin2nd3M;
	//JUNCTION => MADECINE;
	public float timeSpentin3rd3M;

	public MyColor correctMed;
	public Dictionary <MyColor, long> medicine;
}

public enum MyColor
{
	Red,
	Yellow,
	Pink,
	Blue
}

public enum GamePhase
{
	Familiarization,
	SingleTask,
	DuelTask
}