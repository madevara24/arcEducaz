using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TouchDetector : MonoBehaviour {
    private bool tap, doubleTap, swipeUp, swipeDown, swipeLeft, swipeRight, pinchIn, pinchOut;
    private Vector2 startTouch, swipeDelta;
    private Vector2 touchOneStartPos, touchZeroStartPos;
    private float pinchStartMag, pinchCurrentMag, pinchDeltaMag;
    private bool isDraging = false;
    private bool isPinching = false;
    private float tapCooldown = 0.5f, pressDuration = 0f;
    Touch touchZero;
    Touch touchOne;

    public Vector2 StartTouch { get { return startTouch; } }
    public Vector2 SwipeDelta { get { return swipeDelta; } }
    public float PinchDeltaMag { get { return pinchDeltaMag; } }
    public float PinchStartMag { get { return pinchStartMag; } }
    public float PinchCurrentMag { get { return pinchCurrentMag; } }
    public float PressDuration { get { return pressDuration; } }
    public bool SwipeUp { get { return swipeUp; } }
    public bool SwipeDown { get { return swipeDown; } }
    public bool SwipeLeft { get { return swipeLeft; } }
    public bool SwipeRight { get { return swipeRight; } }
    public bool Tap { get { return tap; } }
    public bool DobuleTap { get { return doubleTap; } }
    public bool PinchIn { get { return pinchIn; } }
    public bool PinchOut { get { return pinchOut; } }
    public bool IsDraging { get { return isDraging; } }
    public bool IsPinching { get { return isPinching; } }
    
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        tap = doubleTap = swipeUp = swipeDown = swipeRight = swipeLeft = pinchIn = pinchOut = false;

        //GET INPUT
        if (Input.touches.Length == 1) {
            if (Input.touches[0].phase == TouchPhase.Began) {
                isDraging = true;
                tap = true;
                startTouch = Input.touches[0].position;
                if (tapCooldown > 0) {
                    StartCoroutine(waitForSwipe());
                    Debug.Log("Is Dragging : " + IsDraging + "\nSwipe U/D/R/L : " + SwipeUp + "; " + SwipeDown + "; " + SwipeRight + "; " + SwipeLeft + "\nSwipe Delta/Magnitude : " + SwipeDelta.ToString() + "; " + SwipeDelta.magnitude);
                }
                else {
                    tapCooldown = 0.5f;
                }

            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled) {
                isDraging = false;
                ResetSwipe();
            }
        }
        else if (Input.touches.Length == 2) {
            if (Input.touches[0].phase == TouchPhase.Began && Input.touches[1].phase == TouchPhase.Began) {
                isPinching = true;/*
                touchZero = Input.GetTouch(0);
                touchOne = Input.GetTouch(1);*/
                touchZeroStartPos = Input.touches[0].position - Input.touches[0].deltaPosition;
                touchOneStartPos = Input.touches[1].position - Input.touches[1].deltaPosition;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[1].phase == TouchPhase.Ended ||
                Input.touches[0].phase == TouchPhase.Canceled || Input.touches[1].phase == TouchPhase.Canceled) {
                isPinching = false;
                ResetPinch();
            }
        }

        if (tapCooldown > 0)
            tapCooldown -= 1 * Time.deltaTime;
        //CALCULATE PINCH DISTANCE
        if (isPinching) {
            if (Input.touches.Length == 2) {
                pinchStartMag = (touchZeroStartPos - touchOneStartPos).magnitude;
                pinchCurrentMag = (Input.touches[0].position - Input.touches[1].position).magnitude;
                pinchDeltaMag = pinchCurrentMag / pinchStartMag;
            }
        }

        //CALCULATE SWIPE DISTANCE
        swipeDelta = Vector2.zero;
        if (isDraging) {
            pressDuration += 1 * Time.deltaTime;
            if (Input.touches.Length == 1)
                swipeDelta = Input.touches[0].position - startTouch;
        }
        else {
            pressDuration = 0;
        }

        //CROSS THE ZONE
        if (swipeDelta.magnitude > 10) {
            float x = swipeDelta.x;
            float y = swipeDelta.y;

            if (Mathf.Abs(x) > Mathf.Abs(y)) {
                if (x < 0)
                    swipeLeft = true;
                else
                    swipeRight = true;
            }
            else {
                if (y < 0)
                    swipeDown = true;
                else
                    swipeUp = true;
            }
            //LEFT OR RIGHT

        }
    }

    private IEnumerator waitForSwipe() {
        yield return new WaitForSeconds(0.05f);
        Debug.Log("Mag : " + SwipeDelta.magnitude);
        if (SwipeDelta.magnitude <= 10) {
            //EventManager.TriggerEvent(CoreController.EVENT_DOUBLE_TAP);
        }
    }

    public void ResetSwipe() {
        startTouch = swipeDelta = Vector2.zero;
        isDraging = false;
    }

    public void ResetPinch() {
        pinchDeltaMag = 0f;
        isPinching = false;
    }
}

