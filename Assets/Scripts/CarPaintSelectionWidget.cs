using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CarPaintSelectionWidget : MonoBehaviour, IPointerClickHandler {
    [SerializeField] int imageId;
    public void OnPointerClick(PointerEventData eventData) {
        GameObject.Find("Core Manager").GetComponent<CoreController>().onPressCarPaintWidgetImage(imageId);
    }
    private void OnMouseDown() {
        GameObject.Find("Core Manager").GetComponent<CoreController>().onPressCarPaintWidgetImage(imageId);
    }
}
