using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlaneButtonBehaviour : MonoBehaviour,IPointerDownHandler,IPointerUpHandler {
    [SerializeField] bool add;
    private bool pressing = false;
    public bool isPressing() {
        return pressing;
    }
    public void OnPointerDown(PointerEventData eventData) {
        pressing = true;
    }

    public void OnPointerUp(PointerEventData eventData) {
        pressing = false;
    }
}
