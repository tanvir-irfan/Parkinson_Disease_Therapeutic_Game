﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

using ReadWriteCsv;
//using Microsoft.Samples.Kinect.BodyBasics;
using Windows.Kinect;

public class PlayerMovementController : MonoBehaviour {
    private Animator animator;
    private float moveH;
    private float moveV;

    //	these variables are used for WiiFit board and Kinnect
    UIVA_Client theClient;
    //string ipUIVAServer = "127.0.0.1";
    string ipUIVAServer = "localhost";

    private double[] LEFT_WRIST_POS = new double[3];
    private double[] quat_Joint1 = new double[4];
    private double[] RIGHT_WRIST_POS = new double[3];
    private double[] quat_Joint2 = new double[4];

    private double[] LEFT_FOOT_POS = new double[3];
    private double[] quat_Joint3 = new double[4];
    private double[] RIGHT_FOOT_POS = new double[3];
    private double[] quat_Joint4 = new double[4];

    private double[] SPINE_SHOLDER_POS = new double[3];

    private double pitch = 0.0, roll = 0.0;
    private string butt = "";
    private double gravX = 0.0, gravY = 0.0, weight = 0.0;
    private string fitbutt = "";


    //tanvir.irfan
    private double gravXC = 0.0, gravYC = 0.0;
    private double gravCalibrationTh = 0.05;
    private double gravCalibrationCX = 1, gravCalibrationCY = 1;
    private double gravMovementTh = 10;
    private double footMovementTh = 0.10;

    //	WiiFit board Control
    private double topL = 0.0, topR = 0.0, bottomL = 0.0, bottomR = 0.0;
    private double topLC = 0.0, topRC = 0.0, bottomLC = 0.0, bottomRC = 0.0;
    private double calibrationCounterTL = 0, calibrationCounterTR = 0, calibrationCounterBL = 0, calibrationCounterBR = 0;
    private double calibrationTh = 30;
    private double walkMultiplier = 2.8;
    private double smallTurnMultiplier = 2.3;
    public bool isCalibrationStarted = false;
    public bool isCalibrationDone = false;
    public bool isKeyboardControlled = false;
    public bool isDemo = false;
    public float calibrationDoneTime = 0f;

    private string pressOrRelease = "";

    //	Kinnect Control
    public bool reachForObject = false;
    private String kinnectDataLH = String.Format("{0,-10} {1,-10} {0, -10}\n\n", "X", "Y", "Z");
    private double leftWristC = 0, rightWristC = 0;
    private double wristCounter = 0;


    private double rightFootC = 0, leftFootC = 0;
    private double footCounter = 0;

    private bool walkOnLeftF = true;
    private bool walkOnRightF = true;

    // Game Controller
    private int objectCounter = 0;
    private static float WALK_DURATION = 25f;
    private float walkTime = WALK_DURATION;
    private bool walkTimerStarted = false;

    private static float TURN_DURATION = 10f;
    private float turnTime = TURN_DURATION;
    private bool turnTimerStarted = false;

    private static float MOVE_LEFT_OR_RIGHT_DURATION = 30f;
    private float moveLeftOrRightTime = MOVE_LEFT_OR_RIGHT_DURATION;
    private bool moveLeftOrRightTimerStarted = false;

    public GamePlayScript gp;

    bool walk = false;
    bool walkBackward = false;
    bool turn = false;
    bool moveLeftOrRight = false;

    // Phone Controller
    public GameObject phoneNormal, phonePickUp, handset, phonebooth, junctionEmptObject, timerGameObject;
    public GameObject[] medicinePack = new GameObject[4];
    public GameObject[] blockWall = new GameObject[3];//0 = phone, 1 = medecine, 2 = door
    private string[] medColor = { "MEDICINE_RED", "MEDICINE_YELLOW", "MEDICINE_PINK", "MEDICINE_BLUE" };
    bool isPhonePicked = false;
    bool isPhoneRingNeeded = true;	// phone will ring only when player approaching to the junction
    // phone will stop ringing when player picked up the phone first time
    // phone will not ring again later in the game!
    public bool isPlayerMovementAllowed = false;

    public GameObject outsideDoorBell, insideDoor, outsideAvatar;
    NewDoorController ndc;

    public bool packageReady = false;
    private bool resetTimer = false;

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

    public void setKinectControlled(bool isKinCont) {
        isKeyboardControlled = !isKinCont;
    }

    public void setIsDemo(bool isDemo) {
        this.isDemo = isDemo;
    }

    public void isThirdPersonCam(bool isThirdPersonCamOn) {
        cam.GetComponent<CameraController>().enabled = isThirdPersonCamOn;
    }

    void Awake() {
        if ( Application.platform == RuntimePlatform.WindowsWebPlayer ||
           Application.platform == RuntimePlatform.OSXWebPlayer ) {
            if ( Security.PrefetchSocketPolicy(ipUIVAServer, 843, 500) ) {
                Debug.Log("Got socket policy");
            } else {
                Debug.Log("Cannot get socket policy");
            }
        }
    }

    // Use this for initialization
    void Start() {
        animator = GetComponent<Animator>();

        /*try {
            theClient = new UIVA_Client(ipUIVAServer);
        } catch (Exception ex) {
            Debug.Log("Exception in UIVA_Client");
        }*/

        //################################ NEW KINECT CODE ##########################
        kinectSensor = KinectSensor.GetDefault();

        if ( kinectSensor != null ) {
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();

            if ( !kinectSensor.IsOpen ) {
                kinectSensor.Open();
            }
            if ( bodyFrameReader != null ) {
                bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        } else {
            setKinectControlled(false);
        }
        //################################ NEW KINECT CODE ##########################        

        gp = new GamePlayScript();
        initializeGame();

    }

    private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e) {
        //Debug.Log ( "Reader_FrameArrived" );
    }

    // Assign value in the inspector. These are the color of the medicine
    public Texture2D[] textureToUse = new Texture2D[4];

    void initializeGame() {

        showPhone(true);	// showing normal state.
        for ( int i = 0; i < blockWall.Length; i++ ) {
            blockWall[i].SetActive(true);
        }
        //initialize medicine pack
        for ( int i = 0; i < medicinePack.Length; i++ ) {
            medicinePack[i].SetActive(true);
        }

        int medNumber = gp.getRandomNumber(0, medicinePack.Length);
        //initialize the color tag
        for ( int i = 0; i < medicinePack.Length; i++ ) {
            medNumber += i;
            if ( medNumber >= medicinePack.Length ) {
                medNumber -= medicinePack.Length;
            }
            medicinePack[medNumber].tag = medColor[i];
            medicinePack[medNumber].GetComponent<Renderer>().material.mainTexture = textureToUse[i];
        }

        int randomN;
        //Debug.Log ( "gp.currentRunNumber = " + gp.currentRunNumber );
        if ( gp.currentRunNumber == -1 ) {
            randomN = 0;
        } else {
            randomN = gp.getRandomNumber(1, 3);
        }

        //Debug.Log ( "Current Task = " + randomN );
        gp.setTask(randomN);
        isPhoneRingNeeded = true;
        junctionEmptObject.SetActive(true);
        gp.isDuplicate = false;
        gp.playInstruction = true;

        outsideAvatar.SetActive(false);

        if ( gp.currentRunNumber > 0 ) {
            ( ( TimerScript ) timerGameObject.GetComponent("TimerScript") ).showTime(true);
        }
    }

    void showPhone(bool isNormalState) {
        phoneNormal.SetActive(isNormalState);
        phonePickUp.SetActive(!isNormalState);
        handset.SetActive(!isNormalState);
    }


    void hideDeliveredPackate() {
        GameObject g = GameObject.FindGameObjectWithTag("PACKATE_RECEIVED");
        if ( g != null ) g.SetActive(false);

        packageReady = false;
    }
    // Update is called once per frame
    long fileCounter = 0;
    void Update() {
        fileCounter += 1;

        /*if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            Debug.Log("SHIFT");
            if (Input.GetKeyDown(KeyCode.S))
                Debug.Log("SHIFT + S");
        }*/

        if ( Input.GetKeyDown(KeyCode.Space) ) {

            if ( !reachForObject ) {
                reachForObject = true;
                animator.SetFloat("_Forward", 0);
                animator.SetFloat("_Turn", 0);
                animator.SetFloat("_Strafe", 0);
                animator.SetFloat("_Grab", 1);
                animator.speed = 0.75f;
            } else {
                reachForObject = false;
                animator.SetFloat("_Forward", 0);
                animator.SetFloat("_Turn", 0);
                animator.SetFloat("_Strafe", 0);
                animator.SetFloat("_Grab", 0);
                animator.speed = 0.75f;
            }

            if ( reachForObject && packageReady ) {
                outsideAvatar.GetComponent<Animator>().SetBool("deliverThePackage", false);
                GameObject g = GameObject.FindGameObjectWithTag("PACKATE_DELIVERY");
                if ( g != null )
                    g.SetActive(false);
                g = GameObject.FindGameObjectWithTag("PACKATE_RECEIVED");
                if ( g != null )
                    g.SetActive(true);
                Invoke("hideDeliveredPackate", 3);
            }
        }

        if ( Input.GetKeyDown(KeyCode.K) ) {
            moveLeftOrRightTimerStarted = true;
            animator.SetFloat("_Forward", 0);
            animator.SetFloat("_Turn", 0);
            animator.SetFloat("_Strafe", 1);


        } else if ( Input.GetKeyDown(KeyCode.H) ) {
            moveLeftOrRightTimerStarted = true;
            animator.SetFloat("_Forward", 0);
            animator.SetFloat("_Turn", 0);
            animator.SetFloat("_Strafe", -1);
        }

        if ( walkTimerStarted ) {
            walkTime -= 1;
            if ( walkTime <= 0.0f ) {
                walkTimerEnded();
            }
        }
        if ( turnTimerStarted ) {
            turnTime -= 1;
            if ( turnTime <= 0.0f ) {
                turnTimerEnded();
            }
        }

        if ( moveLeftOrRightTimerStarted ) {
            moveLeftOrRightTime -= 1;
            if ( moveLeftOrRightTime <= 0.0f ) {
                moveLeftOrRightTimerEnded();
            }
        }

    }

    void FixedUpdate() {
        //Debug.Log("isKeyboardControlled = " + isKeyboardControlled + " isPlayerMovementAllowed = " + isPlayerMovementAllowed);
        if ( isKeyboardControlled && isPlayerMovementAllowed ) {
            moveH = Input.GetAxis("Horizontal");
            moveV = Input.GetAxis("Vertical");
            //animator.SetFloat("Walk", moveV);
            //animator.SetFloat("SmallTurnDirection", moveH);
            animator.SetFloat("_Forward", moveV);
            animator.SetFloat("_Turn", moveH);

        } else {
            // the order of the function calling is important.
            // 1. first get the data from wii and kinect
            // 2. if calibration stage, calibrate data
            // 3. when in game mode, use wii and kinect data to control the avatar

            getWiiAndKinectData();    //1
            //isCalibrationDone = true;
            if ( isCalibrationStarted && !isCalibrationDone ) {
                calibrateData();      //2
                return;
            }

            string calData = "";
            //if (isCalibrationDone) {
            controllPlayer();		//3
            if ( count < 2 ) {
                count++;
                calData += "wristCounter = " + wristCounter +
                           " leftWristC = " + ( leftWristC / wristCounter ) +
                           ", rightWristC = " + ( rightWristC / wristCounter ) +
                           ", leftFootC = " + ( leftFootC / footCounter ) +
                           ", rightWristC = " + ( rightWristC / wristCounter ) +
                           ", gravX = " + ( gravXC / gravCalibrationCX ) +
                           ", gravY = " + ( gravYC / gravCalibrationCX ) + "\n";
                //Debug.Log("Writing Caldata!");
                UtilitiesScript.writeTest(calData, "Data\\calData.csv", true);
            }
            //}
        }

        if ( gp.isInBtnStartPointAndDoor ) {
            gp.timeToCrossFirst3M[gp.currentRunNumber] += Time.deltaTime;
        } else if ( gp.isInBtnDoorAndJunction ) {
            gp.timeToCrossSecond3M[gp.currentRunNumber] += Time.deltaTime;
        } else if ( gp.isInBtnJunctionAndMedicine ) {
            gp.timeToCrossThird3M[gp.currentRunNumber] += Time.deltaTime;
        }
    }

    private int count = 0;

    private void getWiiAndKinectData() {
        if ( theClient != null ) {
            theClient.GetWiiFitGravityData(out weight, out gravX, out gravY, out fitbutt);
            theClient.GetWiiFitRawData(out topL, out topR, out bottomL, out bottomR, out pressOrRelease);
            if ( isCalibrationDone && isPlayerMovementAllowed ) {
                logData += weight + ",[X:Y] = [" + gravX + " : " + gravY + "]," + topL + "," + topR + "," + bottomL + "," + bottomR + "\n";
                //Debug.Log ( logData );
            }
        }

        if ( bodyFrameReader != null ) {
            var frame = bodyFrameReader.AcquireLatestFrame();

            if ( frame != null ) {
                if ( bodies == null ) {
                    bodies = new Body[kinectSensor.BodyFrameSource.BodyCount];
                }

                frame.GetAndRefreshBodyData(bodies);

                frame.Dispose();
                frame = null;

                int idx = -1;
                for ( int i = 0; i < kinectSensor.BodyFrameSource.BodyCount; i++ ) {
                    if ( bodies[i].IsTracked ) {
                        idx = i;
                    }
                }

                if ( idx > -1 ) {

                    float multiplier = 1;

                    RIGHT_WRIST_POS[0] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.WristRight].Position.X * multiplier ));
                    RIGHT_WRIST_POS[1] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.WristRight].Position.Y * multiplier ));
                    RIGHT_WRIST_POS[2] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.WristRight].Position.Z * multiplier ));

                    LEFT_WRIST_POS[0] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.WristLeft].Position.X * multiplier ));
                    LEFT_WRIST_POS[1] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.WristLeft].Position.Y * multiplier ));
                    LEFT_WRIST_POS[2] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.WristLeft].Position.Z * multiplier ));

                    RIGHT_FOOT_POS[0] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.FootRight].Position.X * multiplier ));
                    RIGHT_FOOT_POS[1] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.FootRight].Position.Y * multiplier ));
                    RIGHT_FOOT_POS[2] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.FootRight].Position.Z * multiplier ));

                    LEFT_FOOT_POS[0] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.FootLeft].Position.X * multiplier ));
                    LEFT_FOOT_POS[1] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.FootLeft].Position.Y * multiplier ));
                    LEFT_FOOT_POS[2] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.FootLeft].Position.Z * multiplier ));

                    SPINE_SHOLDER_POS[0] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.SpineShoulder].Position.X * multiplier ));
                    SPINE_SHOLDER_POS[1] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.SpineShoulder].Position.Y * multiplier ));
                    SPINE_SHOLDER_POS[2] = Math.Abs(( float ) ( bodies[idx].Joints[JointType.SpineShoulder].Position.Z * multiplier ));

                    leanX = bodies[idx].Lean.X;
                    leanY = bodies[idx].Lean.Y;
                }
            }
        }
    }

    float leanX = 0.0f;
    float leanY = 0.0f;
    public void calibrateData() {
        if ( isCalibrationStarted ) {
            //Debug.Log ( "isCalibrationStarted = " + isCalibrationStarted );
            //if ( gravX > gravCalibrationTh ) {
            gravCalibrationCX = gravCalibrationCX + 1;
            gravXC = ( gravXC + gravX );
            //}
            //if ( gravY > gravCalibrationTh ) {
            gravCalibrationCY = gravCalibrationCY + 1;
            gravYC = ( gravYC + gravY );
            //}

            leftWristC = LEFT_WRIST_POS[2] + leftWristC;
            rightWristC = RIGHT_WRIST_POS[2] + rightWristC;
            wristCounter = wristCounter + 1;

            leftFootC = LEFT_FOOT_POS[1] + leftFootC;
            rightFootC = RIGHT_FOOT_POS[1] + rightFootC;
            footCounter = footCounter + 1;
        }
    }

    public void controllPlayer() {
        //Debug.Log("isPlayerMovementAllowed = " + isPlayerMovementAllowed); 
        if ( !isPlayerMovementAllowed ) {
            animator.SetFloat("_Forward", 0.0f);
            animator.SetFloat("_Turn", 0.0f);
            return;
        }
        //Debug.Log("controllPlayer");
        setMoveParameter(gravX, gravY, LEFT_FOOT_POS[1], RIGHT_FOOT_POS[1], out moveV, out moveH);
        animator.SetFloat("_Forward", moveV);
        //animator.SetFloat("_Turn", Math.Abs(moveH));
        animator.SetFloat("_Turn", moveH);

    }

    public void setMoveParameter(double gX, double gY, double leftFoot, double rightFoot, out float moveV, out float moveH) {

        /*##########################    HAND MOVEMENT  ##################################*/
        float handForward = 0.4f;
        float handSideWise = 0.4f;

        if ( ( SPINE_SHOLDER_POS[2] - LEFT_WRIST_POS[2] >= handForward ) ) {
            reachForObject = true;
            animator.SetFloat("_Forward", 0);
            animator.SetFloat("_Turn", 0);
            animator.SetFloat("_Strafe", 0);
            animator.SetFloat("_Grab", -1);
        } else if ( ( SPINE_SHOLDER_POS[2] - RIGHT_WRIST_POS[2] >= handForward ) ) {
            reachForObject = true;
            animator.SetFloat("_Forward", 0);
            animator.SetFloat("_Turn", 0);
            animator.SetFloat("_Strafe", 0);
            animator.SetFloat("_Grab", 1);
        } else {
            reachForObject = false;
            animator.SetFloat("_Grab", 0);
        }

        if ( reachForObject && packageReady ) {
            outsideAvatar.GetComponent<Animator>().SetBool("deliverThePackage", false);
            GameObject g = GameObject.FindGameObjectWithTag("PACKATE_DELIVERY");
            if ( g != null )
                g.SetActive(false);
            g = GameObject.FindGameObjectWithTag("PACKATE_RECEIVED");
            if ( g != null )
                g.SetActive(true);
            Invoke("hideDeliveredPackate", 3);
        }

        if ( moveLeftOrRightTimerStarted == false ) {
            if ( ( LEFT_WRIST_POS[0] - SPINE_SHOLDER_POS[0] ) >= handSideWise ) {
                animator.SetFloat("_Forward", 0);
                animator.SetFloat("_Turn", 0);
                animator.SetFloat("_Strafe", -1);
                animator.SetFloat("_Grab", 0);
                moveLeftOrRightTimerStarted = true;
            } else if ( ( RIGHT_WRIST_POS[0] - SPINE_SHOLDER_POS[0] ) >= handSideWise ) {
                animator.SetFloat("_Forward", 0);
                animator.SetFloat("_Turn", 0);
                animator.SetFloat("_Strafe", 1);
                animator.SetFloat("_Grab", 0);
                moveLeftOrRightTimerStarted = true;
            }
        }

        /*##########################    WALK    ##################################*/

        leftFoot = Math.Abs(leftFoot);
        rightFoot = Math.Abs(rightFoot);


        float leftFootDisplacement = ( float ) ( leftFoot - rightFoot );
        float rightFootDisplacement = ( float ) ( rightFoot - leftFoot );

        if ( leftFootDisplacement >= footMovementTh ) {	//footMovementTh = 0.03
            if ( walkOnLeftF && !walkTimerStarted ) {
                walk = true;
                walkOnLeftF = false;
                walkOnRightF = true;
                walkTimerStarted = true;
                //Debug.Log ( "TIMER : START" );
            }
        } else if ( rightFootDisplacement >= footMovementTh ) {
            if ( walkOnRightF && !walkTimerStarted ) {
                walk = true;
                walkOnRightF = false;
                walkOnLeftF = true;
                walkTimerStarted = true;
                //Debug.Log ( "TIMER : START" );
            }
        }

        if ( walk ) {
            moveV = 1.0f;
        } else {
            /*##########################    WALK BACKWARD  KKKKKKKKKKKKKKKKKKKKKKKK  ##################################*/
            if ( Math.Abs(leanY) >= 0.5 ) {
                moveV = -1;
            } else {
                moveV = 0.0f;
            }
        }



        /*##########################    TURN    ##################################*/
        if ( Math.Abs(leanX) >= 0.5 ) {
            moveH = leanX / 2;
            //Debug.Log ( "moveH = " + moveH );
        } else {
            moveH = 0;
        }

    }

    string turnData = "";

    void walkTimerEnded() {
        walkTimerStarted = false;
        walk = false;
        //Debug.Log ( "TIMER : ENDED" );
        walkTime = WALK_DURATION;
        gp.numberOfSteps[gp.currentRunNumber]++;
    }

    void turnTimerEnded() {
        turnTimerStarted = false;
        turn = false;
        //Debug.Log ( "TIMER : ENDED" );
        turnTime = WALK_DURATION;
    }

    void moveLeftOrRightTimerEnded() {
        moveLeftOrRightTimerStarted = false;
        moveLeftOrRight = false;
        moveLeftOrRightTime = MOVE_LEFT_OR_RIGHT_DURATION;

        animator.SetFloat("_Forward", 0);
        animator.SetFloat("_Turn", 0);
        animator.SetFloat("_Strafe", 0);
        animator.SetFloat("_Grab", 0);
    }


    void OnTriggerEnter(Collider other) {
        //Destroy(other.gameObject);        
        if ( other.gameObject.tag == "MEDICINE_RED"
            || other.gameObject.tag == "MEDICINE_YELLOW"
            || other.gameObject.tag == "MEDICINE_PINK"
            || other.gameObject.tag == "MEDICINE_BLUE" ) {
            if ( reachForObject ) {
                bool isProperMedicine = false;
                if ( gp.isTaskRedMedecine && other.gameObject.tag == "MEDICINE_RED" ) {
                    isProperMedicine = true;
                } else if ( !gp.isTaskRedMedecine && other.gameObject.tag == "MEDICINE_YELLOW" ) {
                    isProperMedicine = true;
                }
                if ( isProperMedicine ) {
                    other.gameObject.SetActive(false);
                    gp.pickUpMedecine();
                    if ( gp.currentRunNumber < GamePlayScript.NUMBER_OF_RUN - 1 ) {
                        closeHallway(0, true);
                        closeHallway(2, true);
                    }
                } else {
                    string hint = "Please pick up " + ( gp.isTaskRedMedecine ? "RED " : "Yellow " ) + " medicine!";
                    Debug.Log(hint);
                }

            }
        }
        if ( other.gameObject.tag == "PHONE_PICKUP_BOX" && reachForObject ) {
            showPhone(isPhonePicked);	// showing normal state.

            //rign the phone.
            UnityEngine.AudioSource phoneRing = ( UnityEngine.AudioSource ) phonebooth.GetComponent<UnityEngine.AudioSource>() as UnityEngine.AudioSource;
            if ( phoneRing.isPlaying ) {
                phoneRing.Stop();
                //Debug.Log ( "pickUpPhone" );
                gp.pickUpPhone();
                closeHallway(1, false);
            }
            isPhonePicked = !isPhonePicked;
        }

        if ( other.gameObject.tag == "START_POINT" ) {
            if ( ( gp.currentRunNumber == GamePlayScript.NUMBER_OF_RUN - 1 ) && gp.isAllTaskDone() ) {
                //GAME OVER!
                Debug.Log("GAME OVER!");
                gp.setIsGameOver(true);
                UtilitiesScript.writeTest(resultData, "Data\\ResultDataGameOver.csv", true);
            } else if ( ( gp.currentRunNumber == 0 && gp.isAllTaskDone() ) ||
                  ( gp.currentRunNumber > 0 && gp.isAllTaskDone() && gp.currentRunNumber < GamePlayScript.NUMBER_OF_RUN ) ) {

                //write the current data and then initialize the game for next run                               
                resultData += gp.currentRunNumber + "," + gp.timeToCrossFirst3M[gp.currentRunNumber] + "," + gp.timeToCrossSecond3M[gp.currentRunNumber] + "," + gp.timeToCrossThird3M[gp.currentRunNumber] + "\n";
                UtilitiesScript.writeTest(resultData, "Data\\ResultData.csv", true);

                UtilitiesScript.writeTest(logData, "Data\\Log.csv", true);
                initializeGame();
            }

        }

        if ( other.gameObject.tag == "BELL_OR_PHONE_RING" ) {
            if ( gp.isTaskPhone ) {
                phoneRing();
                //also open the blocked hallway
                closeHallway(0, false);
            }
            if ( gp.isTaskDoor ) {
                doorBellRing();
                outsideAvatar.SetActive(true);  // delivary boy is shown
                closeHallway(2, false);
            }
        }
        if ( other.gameObject.tag == "GAME_INSTRUCTION_POINT" ) {
            //Debug.Log("gp.playInstruction = " + gp.playInstruction + " gp.isComplexTaskTrial() = " + gp.isComplexTaskTrial());
            if ( gp.playInstruction && gp.isComplexTaskTrial() ) {
                gp.playInstruction = false;
                //Debug.Log("Time to hear what to do in this run!");
                //TODO show the hear again button!
                int indexOfTask = gp.getIndexOfTask();


                UnityEngine.AudioSource aS = ( UnityEngine.AudioSource ) this.GetComponent<UnityEngine.AudioSource>() as UnityEngine.AudioSource;
                aS.clip = instructionClip[indexOfTask];
                //Debug.Log("indexOfTask = " + indexOfTask + " aS = " + aS + " instructionClip[indexOfTask] = " + instructionClip[indexOfTask]);

                PlaySoundWithCallback(aS, AudioFinished);
            }
        }

        if ( other.gameObject.tag == "BTN_START_POINT_AND_DOOR" ) {
            gp.isInBtnStartPointAndDoor = true;
        } else if ( other.gameObject.tag == "BTN_DOOR_AND_JUNCTION" ) {
            gp.isInBtnDoorAndJunction = true;
        } else if ( other.gameObject.tag == "BTN_JUNCTION_AND_MEDICINE" ) {
            gp.isInBtnJunctionAndMedicine = true;
        }
    }

    public void closeHallway(int which, bool open) {
        blockWall[which].SetActive(open);
    }

    public delegate void AudioCallback();
    private void PlaySoundWithCallback(UnityEngine.AudioSource aS, AudioCallback callback) {
        //Time.timeScale = 0;
        aS.Play();
        StartCoroutine(DelayedCallback(aS.clip.length, callback));
        isPlayerMovementAllowed = false;
    }
    private IEnumerator DelayedCallback(float time, AudioCallback callback) {
        yield return new WaitForSeconds(time);
        callback();
    }

    void AudioFinished() {
        isPlayerMovementAllowed = true;
    }

    void phoneRing() {
        if ( isPhoneRingNeeded ) {
            isPhoneRingNeeded = false;
            //rign the phone.
            UnityEngine.AudioSource phoneRing = ( UnityEngine.AudioSource ) phonebooth.GetComponent<UnityEngine.AudioSource>() as UnityEngine.AudioSource;
            if ( !phoneRing.isPlaying ) {
                phoneRing.loop = true;
                phoneRing.Play();
            }
        }
    }

    void doorBellRing() {
        UnityEngine.AudioSource doorBell = ( UnityEngine.AudioSource ) outsideDoorBell.GetComponent<UnityEngine.AudioSource>() as UnityEngine.AudioSource;
        doorBell.PlayOneShot(doorBell.clip);
    }

    void OnTriggerExit(Collider other) {
        if ( other.gameObject.tag == "BELL_OR_PHONE_RING" ) {
            junctionEmptObject.SetActive(false);
        }
        if ( other.gameObject.tag == "DOOR_CLOSE_POSITION_OUTSIDE" ) {
            //Debug.Log ( "Time to Close Outside Door!" );
            ndc = ( NewDoorController ) other.gameObject.GetComponentInParent<NewDoorController>() as NewDoorController;
            if ( ndc.isDoorOpen )
                ndc.controllDoor("DoorCloseT", true);
        } else if ( other.gameObject.tag == "DOOR_CLOSE_POSITION_INSIDE" ) {
            //Debug.Log ( "Time to Close Inside Door!" );
            ndc = ( NewDoorController ) other.gameObject.GetComponentInParent<NewDoorController>() as NewDoorController;
            if ( ndc.isDoorOpen )
                ndc.controllDoor("DoorCloseT", true);
        }

        if ( other.gameObject.tag == "BTN_START_POINT_AND_DOOR" ) {
            gp.isInBtnStartPointAndDoor = false;
        } else if ( other.gameObject.tag == "BTN_DOOR_AND_JUNCTION" ) {
            gp.isInBtnDoorAndJunction = false;
        } else if ( other.gameObject.tag == "BTN_JUNCTION_AND_MEDICINE" ) {
            gp.isInBtnJunctionAndMedicine = false;
        }

        ndc = null;
    }

    public void receivePackage() {
        //outsideAvatar.GetComponent<Animator>().SetBool("deliverThePackage", false);
        packageReady = true;

    }

    void OnApplicationQuit() {
        if ( bodyFrameReader != null ) {
            bodyFrameReader.Dispose();
            bodyFrameReader = null;
        }

        if ( kinectSensor != null ) {
            if ( kinectSensor.IsOpen ) {
                kinectSensor.Close();
            }
            kinectSensor = null;
        }

        UtilitiesScript.writeTest(logData, "Data\\Log.csv", true);
        UtilitiesScript.writeTest(turnData, "Data\\turnData.csv", true);

    }

    /*
    public void writeTest(string p, string fileName)
    {
        // Write sample data to CSV file
        char[] delimiterChars = { '\n' };

        using (CsvFileWriter writer = new CsvFileWriter(fileName))
        {
            string[] allRows = p.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            foreach (string rowData in allRows)
            {
                delimiterChars[0] = ',';
                string[] allCol = rowData.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                CsvRow row = new CsvRow();
                foreach (string colData in allCol)
                {
                    row.Add(colData);
                }

                writer.WriteRow(row);
            }

        }
    }
    */
}