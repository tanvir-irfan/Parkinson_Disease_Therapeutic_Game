using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class UIVA_Tutorial : MonoBehaviour {

	UIVA_Client theClient;
	string ipUIVAServer = "127.0.0.1";
	
	public double[] pos_Joint1 = new double[3];
	public double[] quat_Joint1 = new double[4];
	public double[] pos_Joint2 = new double[3];
	public double[] quat_Joint2 = new double[4];
	public double pitch = 0.0, roll = 0.0;
	public string butt = "";
	public double gravX = 0.0, gravY = 0.0, weight = 0.0;
	public string fitbutt = "";

	//tanvir.irfan
	public double topL = 0.0, topR = 0.0, bottomL = 0.0, bottomR = 0.0;
	public double topLC = 0.0, topRC = 0.0, bottomLC = 0.0, bottomRC = 0.0;
	private double calibrationCounterTL = 0, calibrationCounterTR = 0, calibrationCounterBL = 0, calibrationCounterBR = 0;
	private double calibrationTh = 30;
	private bool isCalibrationStarted = false;
	public string pressOrRelease = "";
	void Awake() {
		if(Application.platform == RuntimePlatform.WindowsWebPlayer ||
		   Application.platform == RuntimePlatform.OSXWebPlayer) {
			if(Security.PrefetchSocketPolicy(ipUIVAServer, 843, 500)) {
				Debug.Log("Got socket policy");	
			}else {
				Debug.Log("Cannot get socket policy");	
			}
		}
	}
	
	void Start()
	{
		theClient = new UIVA_Client(ipUIVAServer);
	}
	
	void Update() 
	{
		//theClient.GetKinectJointData(6, ref pos_Joint1, ref quat_Joint1);	//Left hand  Check the FAAST Joint List.txt
		//theClient.GetKinectJointData(10, ref pos_Joint2, ref quat_Joint2);	//Right hand
		//theClient.GetWiimoteTiltData(out pitch, out roll, out butt);
		//theClient.GetWiiFitGravityData(out weight, out gravX, out gravY, out fitbutt);
		//theClient.GetWiiFitRawData(out topL, out topR, out bottomL, out bottomR, out pressOrRelease);
	}

	void FixedUpdate() {
		theClient.GetWiiFitGravityData(out weight, out gravX, out gravY, out fitbutt);
		theClient.GetWiiFitRawData(out topL, out topR, out bottomL, out bottomR, out pressOrRelease);
		if (isCalibrationStarted) {
			if(topL>calibrationTh) {
				calibrationCounterTL = calibrationCounterTL + 1;
				topLC = (topLC + topL);
			}
			if(topR>calibrationTh) {
				calibrationCounterTR = calibrationCounterTR + 1;
				topRC = (topRC + topR);
			}
			if(bottomL>calibrationTh) {
				calibrationCounterBL = calibrationCounterBL + 1;
				bottomLC = (bottomLC + bottomL);
			}
			if(bottomR>calibrationTh) {
				calibrationCounterBR = calibrationCounterBR + 1;
				bottomRC = (bottomRC + bottomR);
			}
		} 
	}

	void OnGUI() {
		//GUI.Label (new Rect(0, 0, 200, 50), "Kinect Joint1 Position Data:");
		//GUI.Label (new Rect(0, 20, 200, 50), "X: " + pos_Joint1[0].ToString());
		//GUI.Label (new Rect(0, 40, 200, 50), "Y: " + pos_Joint1[1].ToString());
		//GUI.Label (new Rect(0, 60, 200, 50), "Z: " + pos_Joint1[2].ToString());
		if (GUI.Button (new Rect (0, 200, 200, 50), "Calibrate")) {
			if(isCalibrationStarted)
				isCalibrationStarted = false;
			else
				isCalibrationStarted = true;
		}

		//Debug.Log("Cannot get socket policy");
		//GUI.Label (new Rect(200, 0, 200, 50), "WiiMote Data:");
		//GUI.Label (new Rect(200, 20, 200, 50), "Pitch:" + pitch.ToString());
		//GUI.Label (new Rect(200, 40, 200, 50), "Roll:" + roll.ToString());
		//GUI.Label (new Rect(200, 60, 200, 50), "Button:" + butt.ToString());

		GUI.Label (new Rect(400, 0, 200, 50), "WiiFit Data:");
		GUI.Label (new Rect(400, 20, 200, 50), "Weight:" + weight.ToString());
		GUI.Label (new Rect(400, 40, 200, 50), "Grav X:" + gravX.ToString());
		GUI.Label (new Rect(400, 60, 200, 50), "Grav Y:" + gravY.ToString());

		GUI.Label (new Rect(400, 80, 200, 50), "WiiFit Data:");
		GUI.Label (new Rect(400, 100, 200, 50), "Top Right:" + topR);
		GUI.Label (new Rect(400, 120, 200, 50), "Bottom Right:" + bottomR);
		GUI.Label (new Rect(400, 140, 200, 50), "Top Left:" + topL);
		GUI.Label (new Rect(400, 160, 200, 50), "Bottom Left:" + bottomL);
		GUI.Label (new Rect(400, 180, 200, 50), "pressOrRelease:" + pressOrRelease);

		GUI.Label (new Rect(0, 0, 200, 50), "Calibrated Data:");
		GUI.Label (new Rect(0, 20, 200, 50), "T Right:" + topRC / calibrationCounterTR);
		GUI.Label (new Rect(0, 40, 200, 50), "B Right:" + bottomRC / calibrationCounterBR);
		GUI.Label (new Rect(0, 60, 200, 50), "T Left:" + topLC / calibrationCounterTL);
		GUI.Label (new Rect(0, 80, 200, 50), "B Left:" + bottomLC / calibrationCounterBL);

		//GUI.Label (new Rect(0, 80, 200, 50), "Kinect Joint2 Position Data:");
		//GUI.Label (new Rect(0, 100, 200, 50), "X: " + pos_Joint2[0].ToString());
		//GUI.Label (new Rect(0, 120, 200, 50), "Y: " + pos_Joint2[1].ToString());
		//GUI.Label (new Rect(0, 140, 200, 50), "Z: " + pos_Joint2[2].ToString());
	}
}



