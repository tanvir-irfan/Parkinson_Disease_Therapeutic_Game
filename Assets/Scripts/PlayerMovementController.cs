using ReadWriteCsv;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

using Windows.Kinect;

public class PlayerMovementController : MonoBehaviour
{
	private Animator animator;
	private float moveH;
	private float moveV;
	private bool grabR = false;
	private bool grabL = false;
	private bool isMovingSideWise = false;
	private float moveLeftRight = 0.0f;

	//	these variables are used for WiiFit board and Kinnect
	UIVA_Client theClient;
	//string ipUIVAServer = "127.0.0.1";
	string ipUIVAServer = "localhost";

	private double[] LEFT_WRIST_POS = new double[3];
	private double[] RIGHT_WRIST_POS = new double[3];
	private double[] LEFT_FOOT_POS = new double[3];
	private double[] RIGHT_FOOT_POS = new double[3];
	private double[] SPINE_SHOLDER_POS = new double[3];

	private double gravX = 0.0, gravY = 0.0, weight = 0.0;
	private string fitbutt = "";


	//tanvir.irfan
	private double gravXC = 0.0, gravYC = 0.0;
	private double gravCalibrationTh = 0.05;
	private double gravCalibrationCX = 1, gravCalibrationCY = 1;
	private double gravMovementTh = 10;

	//	WiiFit board Control
	private double topL = 0.0, topR = 0.0, bottomL = 0.0, bottomR = 0.0;
	private double topLC = 0.0, topRC = 0.0, bottomLC = 0.0, bottomRC = 0.0;
	private double calibrationCounterTL = 0, calibrationCounterTR = 0, calibrationCounterBL = 0, calibrationCounterBR = 0;

	public bool isCalibrationStarted = false;
	public bool isCalibrationDone = false;
	public bool isKeyboardControlled = false;
	public bool isDemo = false;
	public float calibrationDoneTime = 0f;

	private string pressOrRelease = "";

	//	Kinnect Control
	public bool reachForObject = false;
	private String kinnectDataLH = String.Format ("{0,-10} {1,-10} {0, -10}\n\n", "X", "Y", "Z");
	private double leftWristC = 0, rightWristC = 0;
	private double wristCounter = 0;


	private double rightFootC = 0, leftFootC = 0;
	private double footCounter = 0;

	private bool walkOnLeftF = false;
	private bool walkOnRightF = true;

	// Game Controller

	public GamePlayScript gp;

	bool walk = false;
	bool walkBackward = false;
	bool turn = false;
	Quaternion originalRotation;

	//tanvir.irfan
	private float minimumX = -360F;
	private float maximumX = 360F;
	public bool left;
	public bool right;

	double rotationX = 0F;
	//double rotationY = 0F;

	float leanX = 0.0f;
	float leanY = 0.0f;


	// Phone Controller
	public GameObject phoneNormal, phonePickUp, handset, phonebooth, junctionEmptObject, timerGameObject;
	public GameObject[] medicinePack = new GameObject[4];
    
	private string[] medColor = { "MEDICINE_RED", "MEDICINE_YELLOW", "MEDICINE_PINK", "MEDICINE_BLUE" };
	// Assign value in the inspector. These are the colors of the medicine
	public Texture2D[] textureToUse = new Texture2D[4];

	bool isPhonePicked = false;
	bool isPhoneRingNeeded = true;
	// phone will ring only when player approaching to the junction
	// phone will stop ringing when player picked up the phone first time
	// phone will not ring again later in the game!
	public bool isPlayerMovementAllowed = false;

	public GameObject outsideDoorBell, insideDoor, outsideAvatar;
	NewDoorController ndc;

	public bool packageReady = false;

	Vector3 originalPosition;

	//public KinectV2 kinectV2;
	private KinectSensor kinectSensor;
	private BodyFrameReader bodyFrameReader;
	private Body[] bodies = null;
	//theClient.GetWiiFitGravityData(out weight, out gravX, out gravY, out fitbutt);
	//theClient.GetWiiFitRawData(out topL, out topR, out bottomL, out bottomR, out pressOrRelease);
	string logData = "weight,gravity [X:Y],topL,topR,bottomL,bottomR\n";
	string resultData = "runNumber,timeToCrossFirst3M,timeToCrossSecond3M,timeToCrossThird3M\n";

	public UnityEngine.AudioClip[] instructionClip = new UnityEngine.AudioClip[2];

	public Camera cam;

	private const string SPAWN_POSITION = "SPAWN_POSITION";
	private GameObject spawn_position;

	private int calibrationDataWriteCounter = 1;

	public void setKinectControlled (bool isKinCont)
	{
		isKeyboardControlled = !isKinCont;
	}

	public void setIsDemo (bool isDemo)
	{
		this.isDemo = isDemo;
	}

	public void isThirdPersonCam (bool isThirdPersonCamOn)
	{
		cam.GetComponent<CameraController> ().enabled = isThirdPersonCamOn;
	}

	void Awake ()
	{
		if (Application.platform == RuntimePlatform.WindowsWebPlayer ||
		    Application.platform == RuntimePlatform.OSXWebPlayer) {
			if (Security.PrefetchSocketPolicy (ipUIVAServer, 843, 500)) {
				Debug.Log ("Got socket policy");
			} else {
				Debug.Log ("Cannot get socket policy");
			}
		}
	}

	// Use this for initialization
	void Start ()
	{
		animator = GetComponent<Animator> ();

		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody> ())
			GetComponent<Rigidbody> ().freezeRotation = true;
		originalRotation = transform.localRotation;

		/*try {
            theClient = new UIVA_Client(ipUIVAServer);
        } catch (Exception ex) {
            Debug.Log("Exception in UIVA_Client");
        }*/

		//################################ NEW KINECT CODE ##########################
		kinectSensor = KinectSensor.GetDefault ();

		if (kinectSensor != null) {
			bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader ();

			if (!kinectSensor.IsOpen) {
				kinectSensor.Open ();
			}
			//if ( bodyFrameReader != null ) {
			//    bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
			//}
		} else {
			setKinectControlled (false);
		}
		//################################ NEW KINECT CODE ##########################        

		gp = new GamePlayScript ();
		initializeGame ();

	}

	//private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e) {
	//Debug.Log ( "Reader_FrameArrived" );
	//}
	public GameObject clockGo;

	void initializeGame ()
	{

		ResetPlayerPosition ();

		showPhone (true);	// showing normal state.
        
		//initialize medicine pack
		for (int i = 0; i < medicinePack.Length; i++) {
			medicinePack [i].SetActive (true);
		}

		int medNumber = gp.getRandomNumber (0, medicinePack.Length);
		//initialize the color tag
		for (int i = 0; i < medicinePack.Length; i++) {
			medNumber += i;
			if (medNumber >= medicinePack.Length) {
				medNumber -= medicinePack.Length;
			}
			medicinePack [medNumber].tag = medColor [i];
			medicinePack [medNumber].GetComponent<Renderer> ().material.mainTexture = textureToUse [i];
		}

		gp.setTask ();

		if (gp.getCurrentTask ().gamePhase == GamePhase.DuelTask || gp.getCurrentTask ().gamePhase == GamePhase.SingleTask) {
			clockGo.SetActive (true);
			Clock clockScript = (Clock)clockGo.GetComponent<Clock> ();
			clockScript.hour = gp.getCurrentTask ().time;
		}

		isPhoneRingNeeded = true;
		junctionEmptObject.SetActive (true);
		//gp.playInstruction = true;

		outsideAvatar.SetActive (false);

	}

	void showPhone (bool isNormalState)
	{
		phoneNormal.SetActive (isNormalState);
		phonePickUp.SetActive (!isNormalState);
		handset.SetActive (!isNormalState);
	}


	void hideDeliveredPackate ()
	{
		GameObject g = GameObject.FindGameObjectWithTag ("PACKATE_RECEIVED");
		if (g != null)
			g.SetActive (false);

		packageReady = false;
	}

	void stopAnim ()
	{
		gameObject.GetComponent<Animator> ().StopPlayback ();
		gameObject.GetComponent<Animator> ().enabled = false;
	}

	void startAnim ()
	{
		gameObject.GetComponent<Animator> ().enabled = true;        
		gameObject.GetComponent<Animator> ().Play ("Idle", -1, 0);
	}

	void ResetPlayerPosition ()
	{
		spawn_position = GameObject.FindGameObjectWithTag ("SPAWN_POSITION");
		if (spawn_position != null) {            
			stopAnim ();
			this.gameObject.transform.position = spawn_position.transform.position;
			this.gameObject.transform.eulerAngles = spawn_position.transform.eulerAngles;
			rotationX = 0f;            
			startAnim ();
		}
	}

	// Update is called once per frame
	void Update ()
	{

		//if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
		//    Debug.Log("SHIFT");
		//    if ( Input.GetKeyDown ( KeyCode.S ) ) {
		//        Debug.Log ( "SHIFT + S" );                
		//    }
		//}

		#region KEYBOARD INPUT HANDLING REGION
		if (isKeyboardControlled && isPlayerMovementAllowed) {
			if (Input.GetKeyDown (KeyCode.Space)) {

				if (!reachForObject) {
					reachForObject = true;
					animator.SetBool ("GrabR", true);
					animator.speed = 0.75f;
				} else {
					reachForObject = false;
					animator.SetBool ("GrabR", false);
					animator.speed = 1f;
				}

				if (reachForObject && packageReady) {
					outsideAvatar.GetComponent<Animator> ().SetBool ("deliverThePackage", false);
					GameObject g = GameObject.FindGameObjectWithTag ("PACKATE_DELIVERY");
					if (g != null)
						g.SetActive (false);
					g = GameObject.FindGameObjectWithTag ("PACKATE_RECEIVED");
					if (g != null)
						g.SetActive (true);
					Invoke ("hideDeliveredPackate", 3);
				}
			}
		}
		#endregion        

	}

	void FixedUpdate ()
	{
		//Debug.Log("isKeyboardControlled = " + isKeyboardControlled + " isPlayerMovementAllowed = " + isPlayerMovementAllowed);
		if (isKeyboardControlled && isPlayerMovementAllowed) {
			moveH = Input.GetAxis ("Horizontal");
			moveV = Input.GetAxis ("Vertical");
			animator.SetFloat ("Walk", moveV);

			if (moveH > 0)
				right = true;
			else if (moveH < 0)
				left = true;
			else {
				left = false;
				right = false;
			}
			turnPlayer ();

		} else {
			// the order of the function calling is important.
			// 1. first get the data from wii and kinect
			// 2. if calibration stage, calibrate data
			// 3. when in game mode, use wii and kinect data to control the avatar


			if ((!isCalibrationStarted && !isCalibrationDone)) {
				//Debug.Log("isPlayerMovementAllowed = " + isPlayerMovementAllowed);
				return;
			}
			getWiiAndKinectData ();    //1
			//isCalibrationDone = true;
			if (isCalibrationStarted && !isCalibrationDone) {
				calibrateData ();      //2
				return;
			}


			//Debug.Log("NEW [ L, R ] = [" + leftFootC + ", " + rightFootC + " ]" + " footCounter = " + footCounter);
			string calData = "";
			controllPlayer ();		//3
			if (calibrationDataWriteCounter < 2) {
				calibrationDataWriteCounter++;

				calData += "wristCounter = " + wristCounter +
				", leftWristC = " + (leftWristC / wristCounter) +
				", rightWristC = " + (rightWristC / wristCounter) +
				", leftFootC = " + (leftFootC / footCounter) +
				", rightFootC = " + (rightFootC / footCounter) +
				", gravX = " + (gravXC / gravCalibrationCX) +
				", gravY = " + (gravYC / gravCalibrationCX) + "\n";

				UtilitiesScript.writeTest (calData, "Data\\calData.csv", true);
			}
		}

		if (gp.isInBtnStartPointAndDoor) {
			gp.allTasks [gp.currentRunNumber].timeBtnStartAndDoor += Time.deltaTime;
		} else if (gp.isInBtnDoorAndJunction) {
			gp.allTasks [gp.currentRunNumber].timeBtnDoorAndJunc += Time.deltaTime;
		} else if (gp.isInBtnJunctionAndMedicine) {
			gp.allTasks [gp.currentRunNumber].timeBtnJuncAndMed += Time.deltaTime;
		} else if (gp.isInBtnJunctionAndDoor) {
			gp.allTasks [gp.currentRunNumber].timeBtnJuncAndDoor += Time.deltaTime;
		} else if (gp.isInBtnJunctionAndPhone) {
			gp.allTasks [gp.currentRunNumber].timeBtnJuncAndPhone += Time.deltaTime;
		}
	}

	private void getWiiAndKinectData ()
	{
		if (theClient != null) {
			theClient.GetWiiFitGravityData (out weight, out gravX, out gravY, out fitbutt);
			theClient.GetWiiFitRawData (out topL, out topR, out bottomL, out bottomR, out pressOrRelease);
			//if ( isCalibrationDone && isPlayerMovementAllowed ) {
			//    logData += weight + ",[X:Y] = [" + gravX + " : " + gravY + "]," + topL + "," + topR + "," + bottomL + "," + bottomR + "\n";
			//Debug.Log ( logData );
			//}
		}

		if (bodyFrameReader != null) {
			var frame = bodyFrameReader.AcquireLatestFrame ();

			if (frame != null) {
				if (bodies == null) {
					bodies = new Body[kinectSensor.BodyFrameSource.BodyCount];
				}

				frame.GetAndRefreshBodyData (bodies);

				frame.Dispose ();
				frame = null;

				// May be this needs fixing, I need to tell kinect not to "track" other bodies.
				int idx = -1;
				for (int i = 0; i < kinectSensor.BodyFrameSource.BodyCount; i++) {
					if (bodies [i].IsTracked) {
						idx = i;
					}
				}

				if (idx > -1) {

					float multiplier = 1;

					RIGHT_WRIST_POS [0] = Math.Abs ((float)(bodies [idx].Joints [JointType.WristRight].Position.X * multiplier));
					RIGHT_WRIST_POS [1] = Math.Abs ((float)(bodies [idx].Joints [JointType.WristRight].Position.Y * multiplier));
					RIGHT_WRIST_POS [2] = Math.Abs ((float)(bodies [idx].Joints [JointType.WristRight].Position.Z * multiplier));

					LEFT_WRIST_POS [0] = Math.Abs ((float)(bodies [idx].Joints [JointType.WristLeft].Position.X * multiplier));
					LEFT_WRIST_POS [1] = Math.Abs ((float)(bodies [idx].Joints [JointType.WristLeft].Position.Y * multiplier));
					LEFT_WRIST_POS [2] = Math.Abs ((float)(bodies [idx].Joints [JointType.WristLeft].Position.Z * multiplier));

					RIGHT_FOOT_POS [0] = Math.Abs ((float)(bodies [idx].Joints [JointType.FootRight].Position.X * multiplier));
					RIGHT_FOOT_POS [1] = Math.Abs ((float)(bodies [idx].Joints [JointType.FootRight].Position.Y * multiplier));
					RIGHT_FOOT_POS [2] = Math.Abs ((float)(bodies [idx].Joints [JointType.FootRight].Position.Z * multiplier));

					LEFT_FOOT_POS [0] = Math.Abs ((float)(bodies [idx].Joints [JointType.FootLeft].Position.X * multiplier));
					LEFT_FOOT_POS [1] = Math.Abs ((float)(bodies [idx].Joints [JointType.FootLeft].Position.Y * multiplier));
					LEFT_FOOT_POS [2] = Math.Abs ((float)(bodies [idx].Joints [JointType.FootLeft].Position.Z * multiplier));

					SPINE_SHOLDER_POS [0] = Math.Abs ((float)(bodies [idx].Joints [JointType.SpineShoulder].Position.X * multiplier));
					SPINE_SHOLDER_POS [1] = Math.Abs ((float)(bodies [idx].Joints [JointType.SpineShoulder].Position.Y * multiplier));
					SPINE_SHOLDER_POS [2] = Math.Abs ((float)(bodies [idx].Joints [JointType.SpineShoulder].Position.Z * multiplier));

					leanX = bodies [idx].Lean.X;
					leanY = bodies [idx].Lean.Y;
				}
			}
		}
	}

	public void calibrateData ()
	{
		//Debug.Log("isCalibrationStarted = " + isCalibrationStarted);
		//if ( gravX > gravCalibrationTh ) {
		gravCalibrationCX = gravCalibrationCX + 1;
		gravXC = (gravXC + gravX);
		//}
		//if ( gravY > gravCalibrationTh ) {
		gravCalibrationCY = gravCalibrationCY + 1;
		gravYC = (gravYC + gravY);
		//}

		leftWristC = LEFT_WRIST_POS [2] + leftWristC;
		rightWristC = RIGHT_WRIST_POS [2] + rightWristC;
		wristCounter = wristCounter + 1;

		leftFootC = LEFT_FOOT_POS [1] + leftFootC;
		rightFootC = RIGHT_FOOT_POS [1] + rightFootC;
		footCounter++;

		//Debug.Log("[ L, R ] = [" + leftFootC/footCounter + ", " + rightFootC/footCounter + " ]" + " footCounter = " + footCounter);

	}

	public void controllPlayer ()
	{
		//Debug.Log("controllPlayer : isPlayerMovementAllowed = " + isPlayerMovementAllowed); 
		if (!isPlayerMovementAllowed) {
			animator.SetFloat ("Walk", 0.0f);
			return;
		}
		//Debug.Log("controllPlayer");
		setMoveParameter (LEFT_FOOT_POS [1], RIGHT_FOOT_POS [1], out moveV);

		animator.SetFloat ("Walk", moveV);
		animator.SetBool ("WalkBack", walkBackward);

		animator.SetBool ("GrabR", grabR);
		animator.SetBool ("GrabL", grabL);

		animator.SetBool ("isMovingSideWise", isMovingSideWise);
		animator.SetFloat ("MoveLeftRight", moveLeftRight);

		turnPlayer ();
	}

	public void turnPlayer ()
	{
		//tanvir.irfan TURN
		if (left)
			rotationX += -1 * (Configuration.TURN_ANGLE / 10);
		else if (right)
			rotationX += (Configuration.TURN_ANGLE / 10);
		else
            //rotationX += Input.GetAxis("Horizontal");
            rotationX = UtilitiesScript.clampAngle (rotationX, minimumX, maximumX);

		Quaternion xQuaternion = Quaternion.AngleAxis ((float)rotationX, Vector3.up);
		transform.localRotation = originalRotation * xQuaternion;
	}

	public void setMoveParameter (double leftFoot, double rightFoot, out float moveV)
	{

		/*##########################    WALK    ##################################*/

		leftFoot = Math.Abs (leftFoot);
		rightFoot = Math.Abs (rightFoot);


		//float leftFootDisplacement = ( float ) ( leftFoot - rightFoot );
		//float rightFootDisplacement = ( float ) ( rightFoot - leftFoot );

		float leftFootDisplacement = (float)Math.Abs (leftFoot - (leftFootC / footCounter));
		float rightFootDisplacement = (float)Math.Abs (rightFoot - (rightFootC / footCounter));

		double ftTh = Configuration.FOOT_MOVEMENT_THRESHOLD / 10;

		if (!walk) {            
			//Debug.Log("[ LD , RD ] = [ " + leftFootDisplacement + " , " + rightFootDisplacement + " ]");            
			if (leftFootDisplacement >= ftTh) {
				//Debug.Log("walkTimerStarted : leftFootDisplacement = " + leftFootDisplacement);
				walk = true;
				Invoke ("walkTimerEnded", (float)Configuration.WALK_DURATION / 1000);
			} else if (rightFootDisplacement >= ftTh) {
				//Debug.Log("walkTimerStarted : rightFootDisplacement = " + rightFootDisplacement);
				walk = true;
				Invoke ("walkTimerEnded", (float)Configuration.WALK_DURATION / 1000);
			}
		}

		if (walk) {
			moveV = 1.0f;
			return;
		} else {
			/*##########################    WALK BACKWARD  KKKKKKKKKKKKKKKKKKKKKKKK  ##################################*/
			if (Math.Abs (leanY) >= (Configuration.LEAN_BACK_THRESHOLD / 10)) {
				moveV = -1;
				walkBackward = true;
				return;
			} else {
				moveV = 0.0f;
				walkBackward = false;
			}
		}

		#region HAND MOVEMENT
		/*##########################    HAND MOVEMENT  ##################################*/
		float handForward = (float)Configuration.HAND_FORWARD_THRESHOLD / 10;
		if (!walkBackward && (SPINE_SHOLDER_POS [2] - LEFT_WRIST_POS [2] >= handForward)) {
			reachForObject = true;
			grabL = true;
		} else if (!walkBackward && (SPINE_SHOLDER_POS [2] - RIGHT_WRIST_POS [2] >= handForward)) {
			reachForObject = true;
			grabR = true;
		} else {
			grabL = false;
			grabR = false;
			reachForObject = false;
		}


		if (reachForObject && packageReady) {
			outsideAvatar.GetComponent<Animator> ().SetBool ("deliverThePackage", false);
			GameObject g = GameObject.FindGameObjectWithTag ("PACKATE_DELIVERY");
			if (g != null)
				g.SetActive (false);
			g = GameObject.FindGameObjectWithTag ("PACKATE_RECEIVED");
			if (g != null)
				g.SetActive (true);
			Invoke ("hideDeliveredPackate", 3);
		}


		float handSideWise = (float)Configuration.HAND_SIDEWISE_THRESHOLD / 10;

		if (!walkBackward && Math.Abs (Math.Abs (LEFT_WRIST_POS [0]) - Math.Abs (SPINE_SHOLDER_POS [0])) >= handSideWise) {
			isMovingSideWise = true;
			moveLeftRight = 1;
		} else if (!walkBackward && Math.Abs (Math.Abs (RIGHT_WRIST_POS [0]) - Math.Abs (SPINE_SHOLDER_POS [0])) >= handSideWise) {
			isMovingSideWise = true;
			moveLeftRight = -1;
		} else {
			isMovingSideWise = false;
			moveLeftRight = 0;
		}

		#endregion

		/*##########################    TURN    ##################################*/
		//Debug.Log("Turn = " + Configuration.TURN_LEAN_THRESHOLD / 10);
		if (Math.Abs (leanX) >= Configuration.TURN_LEAN_THRESHOLD / 10) {
			if (leanX > 0)
				right = true;
			else
				left = true;
		} else {
			left = false;
			right = false;            
		}

	}

	void walkTimerEnded ()
	{
		walk = false;
		if (walkOnLeftF) {
			walkOnLeftF = false;
			walkOnRightF = true;
		} else if (walkOnRightF) {
			walkOnRightF = false;
			walkOnLeftF = true;
		}
		//Debug.Log ( "TIMER : ENDED Walk = " + walk + " walkOnLeftF = " + walkOnLeftF + " walkOnRightF = " + walkOnRightF);
		//gp.numberOfSteps [gp.currentRunNumber]++;
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.tag.Contains ("MEDICINE_")) {
			if (reachForObject) {
				
				// removing the medicine from the table.
				other.gameObject.SetActive (false);
				gp.pickUpMedecine (other.gameObject.tag, gp.allTasks [gp.currentRunNumber].gamePhase);
			}
		}
		if (other.gameObject.tag == "PHONE_PICKUP_BOX" && reachForObject) {
			showPhone (isPhonePicked);	// showing normal state.

			//rign the phone.
			UnityEngine.AudioSource phoneRing = (UnityEngine.AudioSource)phonebooth.GetComponent<UnityEngine.AudioSource> () as UnityEngine.AudioSource;
			if (phoneRing != null && phoneRing.isPlaying) {
				phoneRing.Stop ();
				//Debug.Log ( "pickUpPhone" );
				gp.pickUpPhone ();
			}
			isPhonePicked = !isPhonePicked;
		}

		if (other.gameObject.tag == "START_POINT") {
			if ((gp.currentRunNumber == GamePlayScript.NUMBER_OF_RUN) /*&& gp.isAllTaskDone ()*/) {
				//GAME OVER!
				Debug.Log ("GAME OVER!");
				gp.setIsGameOver (true);
				UtilitiesScript.writeTest (resultData, "Data\\ResultDataGameOver.csv", true);
			} else if (gp.GetTrialStartFlag () && gp.currentRunNumber >= 0 && gp.currentRunNumber < GamePlayScript.NUMBER_OF_RUN) {

				//write the current data and then initialize the game for next run                               
				//resultData += gp.currentRunNumber + "," + gp.allTasks [gp.currentRunNumber] + "," + gp.timeToCrossSecond3M [gp.currentRunNumber] + "," + gp.timeToCrossThird3M [gp.currentRunNumber] + "\n";
				//UtilitiesScript.writeTest (resultData, "Data\\ResultData.csv", true);

				//UtilitiesScript.writeTest (logData, "Data\\Log.csv", true);
				initializeGame ();
			}

		}

		if (other.gameObject.tag == "BELL_OR_PHONE_RING") {
			if (gp.allTasks [gp.currentRunNumber].isPhone) {
				phoneRing ();                
			}
			if (gp.allTasks [gp.currentRunNumber].isDoor) {
				doorBellRing ();
				outsideAvatar.SetActive (true);  // delivary boy is shown
			}
		}
		/*
		if (other.gameObject.tag == "GAME_INSTRUCTION_POINT") {			
			if (gp.playInstruction) {
				gp.playInstruction = false;
				int indexOfTask = gp.getIndexOfTask ();

				UnityEngine.AudioSource aS = (UnityEngine.AudioSource)this.GetComponent<UnityEngine.AudioSource> () as UnityEngine.AudioSource;
				aS.clip = instructionClip [indexOfTask];

				PlaySoundWithCallback (aS, AudioFinished);
			}
		}
		*/
		if (other.gameObject.tag == "TRIAL_START_FLAG_REGION") {			
			Debug.Log ("TRIAL_START_FLAG_REGION");
			gp.SetTrialStartFlag (true);
		}

		if (other.gameObject.tag == "BTN_START_POINT_AND_DOOR") {
			gp.isInBtnStartPointAndDoor = true;
		} else if (other.gameObject.tag == "BTN_DOOR_AND_JUNCTION") {
			gp.isInBtnDoorAndJunction = true;
		} else if (other.gameObject.tag == "BTN_JUNCTION_AND_MEDICINE") {
			gp.isInBtnJunctionAndMedicine = true;
		} else if (other.gameObject.tag == "BTN_JUNCTION_AND_DOOR") {
			gp.isInBtnJunctionAndDoor = true;
		} else if (other.gameObject.tag == "BTN_JUNCTION_AND_PHONE") {
			gp.isInBtnJunctionAndPhone = true;
		} else {
			gp.isInBtnStartPointAndDoor = false;
			gp.isInBtnDoorAndJunction = false;
			gp.isInBtnJunctionAndMedicine = false;
			gp.isInBtnJunctionAndDoor = false;
			gp.isInBtnJunctionAndPhone = false;
		}
	}

	public delegate void AudioCallback ();

	private void PlaySoundWithCallback (UnityEngine.AudioSource aS, AudioCallback callback)
	{
		//Time.timeScale = 0;
		aS.Play ();
		StartCoroutine (DelayedCallback (aS.clip.length, callback));
		isPlayerMovementAllowed = false;
	}

	private IEnumerator DelayedCallback (float time, AudioCallback callback)
	{
		yield return new WaitForSeconds (time);
		callback ();
	}

	void AudioFinished ()
	{
		isPlayerMovementAllowed = true;
	}

	void phoneRing ()
	{
		if (isPhoneRingNeeded) {
			isPhoneRingNeeded = false;
			//rign the phone.
			UnityEngine.AudioSource phoneRing = (UnityEngine.AudioSource)phonebooth.GetComponent<UnityEngine.AudioSource> () as UnityEngine.AudioSource;
			if (!phoneRing.isPlaying) {
				phoneRing.loop = true;
				phoneRing.Play ();
			}
		}
	}

	void doorBellRing ()
	{
		UnityEngine.AudioSource doorBell = (UnityEngine.AudioSource)outsideDoorBell.GetComponent<UnityEngine.AudioSource> () as UnityEngine.AudioSource;
		doorBell.PlayOneShot (doorBell.clip);
	}

	void OnTriggerExit (Collider other)
	{
		if (other.gameObject.tag == "BELL_OR_PHONE_RING") {
			junctionEmptObject.SetActive (false);
		}
		if (other.gameObject.tag == "DOOR_CLOSE_POSITION_OUTSIDE" || other.gameObject.tag == "DOOR_CLOSE_POSITION_INSIDE") {
			//Debug.Log ( "Time to Close Outside Door!" );
			ndc = (NewDoorController)other.gameObject.GetComponentInParent<NewDoorController> () as NewDoorController;
			if (ndc.isDoorOpen)
				ndc.controllDoor ("DoorCloseT", true);
		} /* else if ( other.gameObject.tag == "DOOR_CLOSE_POSITION_INSIDE" ) {
            //Debug.Log ( "Time to Close Inside Door!" );
            ndc = ( NewDoorController ) other.gameObject.GetComponentInParent<NewDoorController>() as NewDoorController;
            if ( ndc.isDoorOpen )
                ndc.controllDoor("DoorCloseT", true);
        } */

		if (other.gameObject.tag == "BTN_START_POINT_AND_DOOR") {
			gp.isInBtnStartPointAndDoor = false;
		} else if (other.gameObject.tag == "BTN_DOOR_AND_JUNCTION") {
			gp.isInBtnDoorAndJunction = false;
		} else if (other.gameObject.tag == "BTN_JUNCTION_AND_MEDICINE") {
			gp.isInBtnJunctionAndMedicine = false;
		}

		ndc = null;
	}

	public void receivePackage ()
	{
		//outsideAvatar.GetComponent<Animator>().SetBool("deliverThePackage", false);
		packageReady = true;

	}

	void OnApplicationQuit ()
	{
		if (bodyFrameReader != null) {
			bodyFrameReader.Dispose ();
			bodyFrameReader = null;
		}

		if (kinectSensor != null) {
			if (kinectSensor.IsOpen) {
				kinectSensor.Close ();
			}
			kinectSensor = null;
		}

		UtilitiesScript.writeTest (logData, "Data\\Log.csv", true);

	}

}