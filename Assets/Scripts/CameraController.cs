using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform lookAt;
    public Transform camTransform;

    private Camera cam;

    private float distanceX = 0.0f;
    private float distanceY = 4.0f;
    private float distanceZ = 2.0f;
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float sensivityX = 4.0f;
    private float sensivityY = 1.0f;

    private void Start() {
        camTransform = transform;
        cam = Camera.main;

    }

    private void LateUpdate() {
        Vector3 dir = new Vector3(distanceX, distanceY, -distanceZ);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        camTransform.position = lookAt.position;

        camTransform.LookAt(lookAt.position);
    }

    private void Update() {
        currentX = lookAt.position.x;
        currentY = lookAt.position.y;
    }
}
