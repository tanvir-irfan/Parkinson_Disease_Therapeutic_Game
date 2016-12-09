using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;

public class ProcessPersonalInformation : MonoBehaviour {

    private string patientName, age, gender, HandY, updrs, moca;
    public InputField nameI, ageI, HandYI, updrsI, mocaI;
    public Dropdown genderD;
    public Text output;
    // Use this for initialization
    void Start() {

    }

    public void onEndNameEdit(string str) {

    }

    public void onEndAgeEdit() {

    }

    public void onEndHYEdit() {

    }

    public void onEndUpdrsEdit() {

    }

    public void onEndMocaEdit() {

    }

    public void onClickDoneButton() {
        output.text = "";
        patientName = nameI.text;
        age = ageI.text;
        HandY = HandYI.text;
        updrs = updrsI.text;
        moca = mocaI.text;
        gender = genderD.value == 0 ? "Male" : "Female";

        if ( UtilitiesScript.isValid(patientName) && UtilitiesScript.isValid(age)
            && UtilitiesScript.isValid(gender) && UtilitiesScript.isValid(HandY)
            && UtilitiesScript.isValid(updrs) && UtilitiesScript.isValid(moca) ) {

            StringBuilder outputStringBuilder = new StringBuilder("");

            String delim = ",";

            outputStringBuilder.Append(patientName + delim + age + delim + gender + delim + HandY + delim + updrs + delim + moca);

            //UtilitiesScript.writeToFile(outputStringBuilder);
            UtilitiesScript.writeTest(outputStringBuilder.ToString(), "Data\\" + patientName + ".csv");

            SceneManager.LoadScene("_MainScene");
        } else {
            output.text = "Before Continue, Please Fill All The Fields Above";
        }

    }


}
