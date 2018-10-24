using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoreManager : MonoBehaviour {
    [SerializeField] TouchDetector touchDetector;
    [SerializeField] Camera mainCamera;
    
    private Vector3 cameraRotation;
    private Quaternion cameraQuaternion;

    //public float x, y, z;

    private Gyroscope gyroscope;
    private bool gyroEnabled;
    private GameObject cameraContainer;
    private Quaternion rot;

    [SerializeField] GameObject gameObjectPlane;
    [SerializeField] GameObject canvasInteractive3d, canvasPreview;
    private Plane groundPlane;
    private Vector3 distanceFromCamera;
    private Vector3 planePos;
    private Vector3 raycastHitPos;

    [SerializeField] GameObject interactive3dObject, rotationGuideObject;
    private bool isObjectActive = false;

    [SerializeField] Button btnAdd;
    [SerializeField] Button btnSub;

    // Use this for initialization
    void Start () {
        cameraContainer = new GameObject("Camera Container");
        cameraContainer.transform.position = mainCamera.transform.position;
        mainCamera.transform.SetParent(cameraContainer.transform);
        gyroEnabled = enableGyro();

        distanceFromCamera = new Vector3(mainCamera.transform.position.x,
                                         mainCamera.transform.position.y -10,
                                         mainCamera.transform.position.z);

        /*var p = new Plane(Vector3.up, Vector3.up * 5);
        float distance;
        p.Raycast(new Ray(new Vector3(15, 10, 5), -Vector3.up), out distance);
        Debug.Log("Dist = " + distance);*/

        Debug.Log("Distance " + Vector3.Distance(gameObjectPlane.transform.position, mainCamera.transform.position));

        groundPlane = new Plane(Vector3.up, distanceFromCamera);

        planePos = gameObjectPlane.transform.position;
        //plane = gameObjectPlane.GetComponent<Plane>();
	}

    private bool enableGyro() {
        if (SystemInfo.supportsGyroscope) {
            gyroscope = Input.gyro;
            gyroscope.enabled = true;

            cameraContainer.transform.rotation = Quaternion.Euler(90, 90, 90);
            rot = new Quaternion(0, 0, 1, 0);

            return true;
        }
        return false;
    }
	
	// Update is called once per frame
	void Update () {
        if (gyroEnabled) {
            mainCamera.transform.localRotation = gyroscope.attitude * rot;
        }

        if (Input.touches.Length == 1) {
            if (!canvasInteractive3d.activeSelf && (Input.touches[0].position.y > Screen.height * 0.1 || (Input.touches[0].position.x > Screen.width * 0.6 || Input.touches[0].position.x < Screen.width * 0.4))) {
                if (isRaycastHitObject(Input.touches[0])) {
                    //StartCoroutine(openInterface());
                }
                raycastHitPos = getRaycastHitPosition(Input.touches[0]);
                if (!interactive3dObject.activeSelf) {
                    interactive3dObject.SetActive(true);
                    interactive3dObject.transform.position = new Vector3(raycastHitPos.x, gameObjectPlane.transform.position.y + 1, raycastHitPos.z);
                }
                else {
                    interactive3dObject.transform.position = new Vector3(raycastHitPos.x, gameObjectPlane.transform.position.y + 1, raycastHitPos.z);
                    //cube.transform.position = raycastHitPos;
                    interactive3dObject.transform.LookAt(new Vector3(mainCamera.transform.position.x, gameObjectPlane.transform.position.y + 1, mainCamera.transform.position.z));
                }
                rotationGuideObject.transform.position = interactive3dObject.transform.position;
                rotationGuideObject.transform.rotation = interactive3dObject.transform.rotation;
            }
        }
        Debug.DrawRay(mainCamera.transform.position, raycastHitPos, Color.red);
    }
    
    private void OnGUI() {
        GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = 20;
        /*
        GUI.Box(new Rect(5, 400, 200, 40), string.Format("{0:0.000}", Input.gyro.attitude));
        GUI.Box(new Rect(5, 450, 200, 40), string.Format("{0:0.000}", gameObjectPlane.transform.position.ToString()));
        GUI.Box(new Rect(5, 500, 200, 40), string.Format("{0} {1}", groundPlane.normal, groundPlane.GetSide(mainCamera.transform.position)));
        GUI.Box(new Rect(5, 550, 200, 40), string.Format("{0:0.000}", raycastHitPos.ToString()));
        GUI.Box(new Rect(5, 600, 200, 40), string.Format("{0:0.000}", Input.touches.Length > 0 ? Input.touches[0].position : Vector2.zero));
        if (Input.touches.Length==1) {
            GUI.Box(new Rect(5, 650, 200, 40), string.Format("{0} {1} {2}", Input.touches[0].position.x < 560, Input.touches[0].position.y > 780, Input.touches[0].position.y < 480));
        }
        */
    }

    private IEnumerator openInterface() {
        yield return new WaitForSeconds(0.2f);
        if (!touchDetector.IsDraging) {
            canvasInteractive3d.SetActive(true);
            canvasPreview.SetActive(false);
        }
    }

    private bool isRaycastHitObject(Touch _touch) {
        Ray ray = mainCamera.ScreenPointToRay(_touch.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            Debug.Log(hit.transform.name);
            if (hit.transform.name == "FFXVRoyalRegalia") {
                return true;
            }
        }
        return false;
    }
    
    private Vector3 getRaycastHitPosition(Touch _touch) {
        Ray ray = mainCamera.ScreenPointToRay(_touch.position);
        float enter = 0f;
        if(groundPlane.Raycast(ray, out enter)) {
            Vector3 hitPoint = ray.GetPoint(enter);
            //Debug.Log(hitPoint.ToString() + "\n" + mainCamera.transform.position.ToString());
            return hitPoint;
        }
        return Vector3.zero;
    }

    private Quaternion gyroToUnity(Quaternion _quaternion) {
        return new Quaternion(_quaternion.x, _quaternion.y, -_quaternion.z, -_quaternion.w);
    }

    public void setPlanePosition() {
        gameObjectPlane.transform.position = planePos;
        //gameObjectPlane.transform.position = groundPlane.normal;
    }

    public void changePlaneYPosition(float _speed) {
        planePos.y += _speed;
        groundPlane.Translate(new Vector3(0, -_speed, 0));
        interactive3dObject.transform.Translate(new Vector3(0, _speed, 0));
        setPlanePosition();
        //Debug.Log("Change\n" + planePos.ToString());
    }

    #region Button Function
    private bool isButtonPressing(Button _button) {
        return _button.GetComponent<PlaneButtonBehaviour>().isPressing();
    }
    public void onPressInterfaceButton() {
        canvasInteractive3d.SetActive(true);
        canvasPreview.SetActive(false);
    }
    #endregion
}