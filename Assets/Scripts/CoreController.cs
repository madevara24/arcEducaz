using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Video;

public class CoreController : MonoBehaviour {

    #region Core Variables
    [SerializeField] TouchDetector touchDetector;
    [SerializeField] Camera arCamera;
    #endregion
    
    #region Interactive 3D Variables
    [SerializeField] GameObject canvasInteractive3D, canvasPlaneButton;
    [SerializeField] Image interactive3dBackgroundImage;
    [SerializeField] Image imgBottomBar, imgBottomBarDonut, imgBottomBarIcon, imgBottomBarNext, imgBottomBarPrev;
    [SerializeField] Image imgArrowPartInspection, imgArrowCarPaint, imgArrowTransform, imgArrowAnimation;
    [SerializeField] Image imgAnimIconBonnet, imgAnimIconDoor, imgAnimIconRoof;
    [SerializeField] Image imgTransformVerticalPlane;
    [SerializeField] Sprite spriteAnimIconOn, spriteAnimIconOff, spriteIconCarPaint, spriteIconPartInspection;
    [SerializeField] Button btnPartInspection, btnCarPaint, btnTransform, btnAnimation;
    [SerializeField] Button btnAnimRoof, btnAnimDoor, btnAnimBonnet;
    [SerializeField] Button btnBottomBarNext, btnBottomBarPrev;
    [SerializeField] Button btnPlanePlus, btnPlaneMinus;
    [SerializeField] Text txtInspectPartName;

    [SerializeField] GameObject interactive3dObject, doorLF, doorRF, doorLR, doorRR, bonnetR, bonnetL, rotateGuideObject;

    [SerializeField] List<Sprite> listOfInteractive3dPaintWidgetSprite;
    [SerializeField] List<Image> listOfInteractive3dPaintWidgetImage;
    [SerializeField] List<Material> listOfInteractive3dMaterials;
    [SerializeField] List<GameObject> listOfPaintedCarParts;
    [SerializeField] List<Vector3> listOfInspectPartRotationEuler;
    [SerializeField] List<string> listofInspectPartName;

    public enum Interactive3DViewMode { Animation, CarPaint, Transform, PartInspection, Default }
    private Interactive3DViewMode interactive3DViewMode = Interactive3DViewMode.Default;
    private bool isDoorClosed = true, isBonnetClosed = true, isRoofClosed = true, isAnimationFinished = true;
    private int triggerRoofOpenHash = Animator.StringToHash("triggerRoofOpen"), triggerRoofCloseHash = Animator.StringToHash("triggerRoofClose");
    private Vector3 interactive3dStartScale, interactive3dDefaultScale, interactive3dStartEuler;
    private int interactive3dPaintId = 4, interactive3dInspetPartId = 0;

    public bool isInteractive3dMarkerDetected = false;
    #endregion
    
    void Start () {
        DOTween.Init(true, true, LogBehaviour.Default);
        //openGalleryOverview();
    }

    // Update is called once per frame
    void Update () {
        debuggingKey();
        updateInteractive3D();
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Debug.Log("Back Pressed");
            onPressBackButton();
        }

        if (isButtonPressing(btnPlanePlus))
            GetComponent<CoreManager>().changePlaneYPosition(0.1f);

        if (isButtonPressing(btnPlaneMinus))
            GetComponent<CoreManager>().changePlaneYPosition(-0.1f);
    }

    private void debuggingKey() {
    }

    private void OnGUI() {
        GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = 20;
        //GUI.Box(new Rect(5, 600, 200, 40), string.Format("{0}", touchDetector.PressDuration));
        //GUI.Box(new Rect(50, 100, 700, 50), string.Format("{0} {1} {2}", touchDetector.SwipeDelta, interactive3dStartScale, interactive3dObject.transform.localScale));
    }

    #region Core Functions
    public void onPressBackButton() {
        Debug.Log("Back");
        resetInteractive3dInterface();
        canvasPlaneButton.SetActive(true);
        canvasInteractive3D.SetActive(false);
    }

    public void openInteractive3DInterface() {
        canvasInteractive3D.SetActive(true);
        canvasPlaneButton.SetActive(false);
    }

    private bool isButtonPressing(Button _button) {
        return _button.GetComponent<PlaneButtonBehaviour>().isPressing();
    }
    #endregion

    #region Interactive 3D Functions
    private void onDetectInteractive3D() {
        interactive3dDefaultScale = interactive3dStartScale = interactive3dObject.transform.localScale;
    }

    private void resetInteractive3dInterface() {
        changeInteractive3DViewMode(Interactive3DViewMode.Default);
        imgArrowPartInspection.color = imgArrowCarPaint.color = imgArrowTransform.color = imgArrowAnimation.color = Color.white;
        imgArrowPartInspection.transform.localEulerAngles = imgArrowCarPaint.transform.localEulerAngles = imgArrowTransform.transform.localEulerAngles = imgArrowAnimation.transform.localEulerAngles = Vector3.zero;
        StartCoroutine(interactive3dSlideButton(btnCarPaint, 377, 0.1f));
        StartCoroutine(interactive3dSlideButton(btnTransform, 279, 0.1f));
        StartCoroutine(interactive3dSlideButton(btnAnimation, 181, 0.1f, 0.1f));
        StartCoroutine(showInteractive3dAnimButton(false));
        StartCoroutine(showInteractive3dVerticalPlaneImage(false));
        showInteractive3dVerticalPlaneImage(false);
        showInteractive3dBottomBar(false);
        showInteractive3dPaintWidgetImage(false);
        showInteractive3dInspectPartName(false);    
    }

    private void updateInteractive3D() {
        if(interactive3DViewMode == Interactive3DViewMode.Transform) {
            interactive3DCheckSwipe();
        }
    }

    private void interactive3DCheckSwipe() {
        if (touchDetector.SwipeUp) {
            if (interactive3dStartScale.x * (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640)) < 2) {
                interactive3dObject.transform.localScale = interactive3dStartScale * (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640));
            }
        }
        if (touchDetector.SwipeDown) {
            if (interactive3dStartScale.x / (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640)) > 0.25f) {
                interactive3dObject.transform.localScale = interactive3dStartScale / (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640));
            }
        }
        if (touchDetector.SwipeRight) {
            interactive3dObject.transform.localRotation = Quaternion.Euler(new Vector3(interactive3dStartEuler.x, interactive3dStartEuler.y - (touchDetector.SwipeDelta.x / 2), interactive3dStartEuler.z));
        }
        if (touchDetector.SwipeLeft) {
            interactive3dObject.transform.localRotation = Quaternion.Euler(new Vector3(interactive3dStartEuler.x, interactive3dStartEuler.y - (touchDetector.SwipeDelta.x / 2), interactive3dStartEuler.z));
        }
        if (!touchDetector.IsDraging) {
            interactive3dStartScale = interactive3dObject.transform.localScale;
            interactive3dStartEuler = interactive3dObject.transform.localRotation.eulerAngles;
        }
    }

    private void interactive3DChangeTexture() {
        for (int i = 0; i < listOfPaintedCarParts.Count; i++) {
            listOfPaintedCarParts[i].GetComponent<Renderer>().material = listOfInteractive3dMaterials[interactive3dPaintId];
        }
    }

    public void onPressPartInspectionButton() {
        Debug.Log("Press Part Inspect");
        if (interactive3DViewMode == Interactive3DViewMode.Default) {
            imgBottomBarIcon.GetComponent<Image>().sprite = spriteIconPartInspection;
            changeInteractive3DViewMode(Interactive3DViewMode.PartInspection);
            imgArrowPartInspection.color = Color.cyan;
            imgArrowPartInspection.transform.localEulerAngles = new Vector3(0, 0, 180);
            interactive3dInspectPart(0);
            showInteractive3dBottomBar(true);
            showInteractive3dInspectPartName(true);
        }
        else {
            changeInteractive3DViewMode(Interactive3DViewMode.Default);
            imgArrowPartInspection.color = Color.white;
            imgArrowPartInspection.transform.localEulerAngles = Vector3.zero;
            showInteractive3dBottomBar(false);
            showInteractive3dInspectPartName(false);
        }   
    }

    public void onPressCarPaintButton() {
        Debug.Log("Press Car Paint");
        if (interactive3DViewMode == Interactive3DViewMode.Default) {
            imgBottomBarIcon.GetComponent<Image>().sprite = spriteIconCarPaint;
            changeInteractive3DViewMode(Interactive3DViewMode.CarPaint);
            imgArrowCarPaint.color = Color.cyan;
            imgArrowCarPaint.transform.localEulerAngles = new Vector3(0, 0, 180);
            StartCoroutine(interactive3dSlideButton(btnCarPaint, 475, 0.3f, 0.3f));
            interactive3dUpdatePaintWidgetImage();
            showInteractive3dPaintWidgetImage(true);
            showInteractive3dBottomBar(true);
        }
        else {
            changeInteractive3DViewMode(Interactive3DViewMode.Default);
            imgArrowCarPaint.color = Color.white;
            imgArrowCarPaint.transform.localEulerAngles = Vector3.zero;
            StartCoroutine(interactive3dSlideButton(btnCarPaint, 377, 0.3f));
            showInteractive3dPaintWidgetImage(false);
            showInteractive3dBottomBar(false);
        }
    }

    public void onPressTransformButton() {
        Debug.Log("Press Transform");
        if (interactive3DViewMode == Interactive3DViewMode.Default) {
            changeInteractive3DViewMode(Interactive3DViewMode.Transform);
            imgArrowTransform.color = Color.cyan;
            imgArrowTransform.transform.localEulerAngles = new Vector3(0, 0, 180);
            StartCoroutine(interactive3dSlideButton(btnTransform, 475, 0.3f, 0.3f));
            StartCoroutine(showInteractive3dVerticalPlaneImage(true));
        }
        else {
            changeInteractive3DViewMode(Interactive3DViewMode.Default, 0.6f);
            imgArrowTransform.color = Color.white;
            imgArrowTransform.transform.localEulerAngles = Vector3.zero;
            StartCoroutine(interactive3dSlideButton(btnTransform, 279, 0.3f, 0.3f));
            StartCoroutine(showInteractive3dVerticalPlaneImage(false));
        }
    }

    public void onPressAnimationButton() {
        Debug.Log("Press Animation");
        if (interactive3DViewMode == Interactive3DViewMode.Default) {
            changeInteractive3DViewMode(Interactive3DViewMode.Animation);
            imgArrowAnimation.color = Color.cyan;
            imgArrowAnimation.transform.localEulerAngles = new Vector3(0, 0, 180);
            StartCoroutine(interactive3dSlideButton(btnAnimation, 475, 0.3f, 0.3f));
            StartCoroutine(showInteractive3dAnimButton(true));
        }
        else {
            changeInteractive3DViewMode(Interactive3DViewMode.Default, 0.6f);
            imgArrowAnimation.color = Color.white;
            imgArrowAnimation.transform.localEulerAngles = Vector3.zero;
            StartCoroutine(interactive3dSlideButton(btnAnimation, 181, 0.3f, 0.3f));
            StartCoroutine(showInteractive3dAnimButton(false));
        }
    }

    public void onPressInteractive3DNextPartButton() {
        Debug.Log("Next");
        if(interactive3DViewMode == Interactive3DViewMode.CarPaint) {
            changeInteractive3dPaintId(interactive3dPaintId + 1);
        }else if(interactive3DViewMode == Interactive3DViewMode.PartInspection) {
            changeInteractive3dPartInspectionId(true);
        }
    }

    public void onPressInteractive3DPrevPartButton() {
        Debug.Log("Prev");
        if (interactive3DViewMode == Interactive3DViewMode.CarPaint) {
            changeInteractive3dPaintId(interactive3dPaintId - 1);
        }
        else if (interactive3DViewMode == Interactive3DViewMode.PartInspection) {
            changeInteractive3dPartInspectionId(false);
        }
    }

    public void onPressAnimRoofButton() {
        interactive3dAnimRoof();
    }

    public void onPressAnimDoorButton() {
        if (isAnimationFinished) {
            isAnimationFinished = false;
            StartCoroutine(interactive3dAnimDoor());
            StartCoroutine(waitAnimationFinish(2.5f));
        }
    }

    public void onPressAnimBonnetButton() {
        if (isAnimationFinished) {
            isAnimationFinished = false;
            interactive3dAnimBonnet();
            StartCoroutine(waitAnimationFinish(2.5f));
        }
    }

    private void changeInteractive3dPartInspectionId(bool _next) {
        if (_next)
            interactive3dInspetPartId++;
        else
            interactive3dInspetPartId--;

        if (interactive3dInspetPartId < 0)
            interactive3dInspetPartId = listOfInspectPartRotationEuler.Count - 1;
        if (interactive3dInspetPartId > listOfInspectPartRotationEuler.Count - 1)
            interactive3dInspetPartId = 0;

        interactive3dInspectPart(interactive3dInspetPartId);
    }

    public void onPressCarPaintWidgetImage(int _id) {
        changeInteractive3dPaintId(_id + interactive3dPaintId - 3);
    }

    private void changeInteractive3dPaintId(int _id) {
        interactive3dPaintId = _id;

        if (interactive3dPaintId >= listOfInteractive3dMaterials.Count)
            interactive3dPaintId = interactive3dPaintId - listOfInteractive3dMaterials.Count;

        if (interactive3dPaintId < 0)
            interactive3dPaintId = interactive3dPaintId + listOfInteractive3dMaterials.Count - 1;

        interactive3DChangeTexture();
        interactive3dUpdatePaintWidgetImage();
    }

    private IEnumerator showInteractive3dVerticalPlaneImage(bool _show) {
        if (_show) {
            yield return new WaitForSeconds(0.6f);
            imgTransformVerticalPlane.gameObject.SetActive(true);
            imgTransformVerticalPlane.GetComponent<Image>().DOFade(0.2f, 0.3f);
        }
        else {
            imgTransformVerticalPlane.GetComponent<Image>().DOFade(0f, 0.2f);
            yield return new WaitForSeconds(0.2f);
            imgTransformVerticalPlane.gameObject.SetActive(false);
        }
        imgTransformVerticalPlane.gameObject.SetActive(_show);
    }

    private void showInteractive3dBottomBar(bool _show) {
        imgBottomBar.gameObject.SetActive(_show);
        btnBottomBarNext.gameObject.SetActive(_show);
        btnBottomBarPrev.gameObject.SetActive(_show);
    }

    private void showInteractive3dInspectPartName(bool _show) {
        txtInspectPartName.gameObject.SetActive(_show);
    }

    private void showInteractive3dPaintWidgetImage(bool _show) {
        for (int i = 0; i < listOfInteractive3dPaintWidgetImage.Count; i++) {
            listOfInteractive3dPaintWidgetImage[i].gameObject.SetActive(_show);
        }
    }

    private IEnumerator showInteractive3dAnimButton(bool _show) {
        if (_show) {
            yield return new WaitForSeconds(0.5f);
            btnAnimBonnet.gameObject.SetActive(_show);
            btnAnimDoor.gameObject.SetActive(_show);
            btnAnimRoof.gameObject.SetActive(_show);
            btnAnimBonnet.GetComponent<Image>().DOFade(0.3f, 0.3f);
            btnAnimDoor.GetComponent<Image>().DOFade(0.3f, 0.3f);
            btnAnimRoof.GetComponent<Image>().DOFade(0.3f, 0.3f);
        }
        else {
            yield return new WaitForSeconds(0f);
            btnAnimBonnet.GetComponent<Image>().DOFade(0, 0.2f);
            btnAnimDoor.GetComponent<Image>().DOFade(0, 0.2f);
            btnAnimRoof.GetComponent<Image>().DOFade(0, 0.2f);
            StartCoroutine(interactive3dActivateButton(btnAnimBonnet, false, 0.3f));
            StartCoroutine(interactive3dActivateButton(btnAnimDoor, false, 0.3f));
            StartCoroutine(interactive3dActivateButton(btnAnimRoof, false, 0.3f));
        }
    }

    private void interactive3dUpdatePaintWidgetImage() {
        int paintId = interactive3dPaintId - 3;
        Debug.Log("Paint ID : " + interactive3dPaintId);
        for (int i = 0; i < listOfInteractive3dPaintWidgetImage.Count; i++) {
            if (paintId + i < 0) {
                changeInteractive3dWidgetCarPaintImageSprite(listOfInteractive3dPaintWidgetImage[i], paintId + i + listOfInteractive3dMaterials.Count);
            }
            else if (paintId + i > listOfInteractive3dMaterials.Count - 1) {
                changeInteractive3dWidgetCarPaintImageSprite(listOfInteractive3dPaintWidgetImage[i], paintId + i - listOfInteractive3dMaterials.Count);
            }
            else {
                changeInteractive3dWidgetCarPaintImageSprite(listOfInteractive3dPaintWidgetImage[i], paintId + i);
            }
        }
    }

    private void changeInteractive3dWidgetCarPaintImageSprite(Image _image, int _id) {
        Debug.Log("Sprite ID : " + _id);
        if (_id < 3)
            _image.sprite = listOfInteractive3dPaintWidgetSprite[_id];
        else
            _image.sprite = listOfInteractive3dPaintWidgetSprite[3];

        switch (_id) {
            case 3:_image.color = Color.black;break;
            case 4:_image.color = Color.gray;break;
            case 6:_image.color = Color.red;break;
            case 7:_image.color = new Color(1, .5f, 0);break;
            case 8:_image.color = Color.yellow;break;
            case 9:_image.color = new Color(.5f, 1, 0);break;
            case 10:_image.color = Color.green;break;
            case 11:_image.color = new Color(0, 1, .5f);break;
            case 12:_image.color = Color.cyan;break;
            case 13:_image.color = new Color(0, .5f, 1);break;
            case 14:_image.color = Color.blue;break;
            case 15:_image.color = new Color(.5f, 0, 1);break;
            case 16:_image.color = Color.magenta;break;
            case 17:_image.color = new Color(1, 0, .5f);break;
            default:_image.color = Color.white;break;
        }
    }

    private void changeInteractive3DViewMode(Interactive3DViewMode _newMode, float _wait = 0.3f) {
        if (_newMode == Interactive3DViewMode.Default) {
            //interactive3dBackgroundImage.gameObject.SetActive(true);
            interactive3dBackgroundImage.GetComponent<Image>().DOFade(0.5f, 0.3f);
        }
        else {
            interactive3dBackgroundImage.GetComponent<Image>().DOFade(0f, 0.3f);
        }
            //interactive3dBackgroundImage.gameObject.SetActive(false);

        interactive3DViewMode = _newMode;
        interactive3dSlideOtherButton(_wait);
    }

    private IEnumerator interactive3dSlideButton(Button _button, float _endYPos, float _duration, float _wait = 0f) {
        yield return new WaitForSeconds(_wait);
        _button.transform.DOLocalMoveY(_endYPos, _duration);
    }

    private IEnumerator interactive3dFadeButton(Button _button, bool _fadeIn, float _duration, float _wait = 0f) {
        yield return new WaitForSeconds(_wait);
        if (_fadeIn) {
            _button.gameObject.SetActive(true);
            _button.GetComponent<Image>().DOFade(0.5f, _duration);
        }
        else {
            _button.GetComponent<Image>().DOFade(0f, _duration);
            StartCoroutine(interactive3dActivateButton(_button, false, _duration));
        }
    }

    private IEnumerator interactive3dActivateButton(Button _button, bool _active, float _wait = 0f) {
        yield return new WaitForSeconds(_wait);
        _button.gameObject.SetActive(_active);
    }

    private IEnumerator interactive3dActivateImage(Image _image, bool _active, float _wait = 0f) {
        yield return new WaitForSeconds(_wait);
        _image.gameObject.SetActive(_active);
    }

    private void interactive3dSlideOtherButton(float _wait = 0.3f) {
        switch (interactive3DViewMode) {
            case Interactive3DViewMode.Animation:
                StartCoroutine(interactive3dFadeButton(btnPartInspection, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnCarPaint, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnTransform, false, 0.3f));
                StartCoroutine(interactive3dFadeBottomBar(false));
                break;
            case Interactive3DViewMode.CarPaint:
                StartCoroutine(interactive3dFadeButton(btnPartInspection, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnTransform, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnAnimation, false, 0.3f));
                StartCoroutine(interactive3dFadeBottomBar(true));
                break;
            case Interactive3DViewMode.Transform:
                StartCoroutine(interactive3dFadeButton(btnPartInspection, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnCarPaint, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnAnimation, false, 0.3f));
                StartCoroutine(interactive3dFadeBottomBar(false));
                break;
            case Interactive3DViewMode.PartInspection:
                StartCoroutine(interactive3dFadeButton(btnTransform, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnCarPaint, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnAnimation, false, 0.3f));
                StartCoroutine(interactive3dFadeBottomBar(true));
                break;
            case Interactive3DViewMode.Default:
                StartCoroutine(interactive3dFadeButton(btnPartInspection, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnTransform, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnCarPaint, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnAnimation, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeBottomBar(false));
                break;
            default:
                StartCoroutine(interactive3dFadeButton(btnPartInspection, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnTransform, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnCarPaint, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnAnimation, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeBottomBar(false));
                break;
        }
    }

    private IEnumerator interactive3dFadeBottomBar(bool _show) {
        if (_show) {
            imgBottomBar.gameObject.SetActive(true);
            imgBottomBar.DOFade(1f, 0.3f);
            imgBottomBarDonut.DOFade(1f, 0.3f);
            imgBottomBarIcon.DOFade(1f, 0.3f);
            imgBottomBarNext.DOFade(1f, 0.3f);
            imgBottomBarPrev.DOFade(1f, 0.3f);
        }
        else {
            imgBottomBar.DOFade(0f, 0.3f);
            imgBottomBarNext.DOFade(0f, 0.3f);
            imgBottomBarPrev.DOFade(0f, 0.3f);
            imgBottomBarDonut.DOFade(0f, 0.3f);
            imgBottomBarIcon.DOFade(1f, 0.3f);
            yield return new WaitForSeconds(0.3f);
            imgBottomBar.gameObject.SetActive(false);
        }
    }

    private void interactive3dAnimRoof() {
        if (isRoofClosed) {
            imgAnimIconRoof.sprite = spriteAnimIconOn;
            interactive3dObject.GetComponent<Animator>().SetTrigger(triggerRoofOpenHash);
            isRoofClosed = false;
        }
        else {
            imgAnimIconRoof.sprite = spriteAnimIconOff;
            interactive3dObject.GetComponent<Animator>().SetTrigger(triggerRoofCloseHash);
            isRoofClosed = true;
        }
    }
    
    private IEnumerator waitAnimationFinish(float _duration) {
        yield return new WaitForSeconds(_duration);
        isAnimationFinished = true;
    }

    private IEnumerator interactive3dAnimDoor() {
        if (isDoorClosed) {
            imgAnimIconDoor.sprite = spriteAnimIconOn;
            isDoorClosed = false;
            translateDoor(doorLF, -0.0005f, 0.2f);
            translateDoor(doorLR, -0.0005f, 0.2f);
            translateDoor(doorRF, 0.0005f, 0.2f);
            translateDoor(doorRR, 0.0005f, 0.2f);
            yield return new WaitForSeconds(0.1f);
            rotateDoor(doorLF, 60, 2.5f);
            rotateDoor(doorLR, -60, 2.5f);
            rotateDoor(doorRF, -60, 2.5f);
            rotateDoor(doorRR, 65, 2.5f);
        }
        else {
            imgAnimIconDoor.sprite = spriteAnimIconOff;
            isDoorClosed = true;
            rotateDoor(doorLF, 0, 2.5f);
            rotateDoor(doorLR, 0, 2.5f);
            rotateDoor(doorRF, 0, 2.5f);
            rotateDoor(doorRR, 0, 2.5f);
            yield return new WaitForSeconds(2f);
            translateDoor(doorLF, 0.0005f, 0.2f);
            translateDoor(doorLR, 0.0005f, 0.2f);
            translateDoor(doorRF, -0.0005f, 0.2f);
            translateDoor(doorRR, -0.0005f, 0.2f);
        }
    }

    private void translateDoor(GameObject _door, float _translate, float _duration) {
        _door.transform.DOLocalMoveX(_door.transform.localPosition.x + _translate, _duration);
    }

    private void rotateDoor(GameObject _door, float _zRot, float _duration) {
        _door.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(298.3f, 159.9f, -347.9f + _zRot)), _duration);
    }

    private void interactive3dAnimBonnet() {
        if (isBonnetClosed) {
            imgAnimIconBonnet.sprite = spriteAnimIconOn;
            isBonnetClosed = false;
            bonnetL.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(-19.6f, -268.2f, -24.7f)), 2.5f);
            bonnetR.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(-31.7f, -111.7f, -332.8f)), 2.5f);
        }
        else {
            imgAnimIconBonnet.sprite = spriteAnimIconOff;
            isBonnetClosed = true;
            bonnetL.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(294f, -202.5f, 284.2f)), 2.5f);
            bonnetR.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(-66f, -202.5f, -255.8f)), 2.5f);
        }
    }

    private void interactive3dInspectPart(int _id) {
        rotateGuideObject.transform.LookAt(new Vector3(arCamera.transform.position.x, 0, arCamera.transform.position.z));
        rotateGuideObject.transform.localRotation = Quaternion.Euler(new Vector3(0, rotateGuideObject.transform.localRotation.eulerAngles.y + listOfInspectPartRotationEuler[_id].y, rotateGuideObject.transform.localRotation.eulerAngles.z));
        interactive3dObject.transform.DOLocalRotateQuaternion(Quaternion.Euler(rotateGuideObject.transform.localRotation.eulerAngles), 2.5f);
        txtInspectPartName.text = listofInspectPartName[_id];
    }
    #endregion
    
}

