using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Interactive3DObjectBehaviour : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData) {
        GameObject.Find("Core Controller").GetComponent<CoreController>().openInteractive3DInterface();
        Debug.Log("on Pointer Click 3D Object Touched");
    }

    private void OnMouseDown() {
        GameObject.Find("Core Controller").GetComponent<CoreController>().openInteractive3DInterface();
        Debug.Log("on Mouse Down 3D Object Touched");
    }
}
