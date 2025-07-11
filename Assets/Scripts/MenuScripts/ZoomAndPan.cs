using DG.Tweening;
using Rewired;
using UnityEngine;

public class ZoomAndPan : MonoBehaviour
{
    public CanvasGroup controlletPanel;
    public RectTransform panelRectTransform;
    public RectTransform treePanel;
    public float zoomSpeed = 0.5f;
    public float panSpeed = 10f;
    public float maxZoom = 3f;
    public float minZoom = 1f;

    [SerializeField] private Vector2 panLimitMin = new Vector2(-2500, -1100);
    [SerializeField] private Vector2 panLimitMax = new Vector2(2500, 1100);
    private Vector3 previousMousePosition;

    public Camera cam;
    private Player player { get { return ReInput.players.GetPlayer(0); } }

    float currentScale;
    bool canZoomOnPosition;

    public enum ZOOM_LOCATION
    {
        LEFT,
        CENTER,
        RIGHT
    }

    public ZOOM_LOCATION zoomLocation = ZOOM_LOCATION.CENTER;

    public float lerpSpeed = 5f;

    private float previousTouchDistance;

    private void Start()
    {
        CalculatePanLimits();

        if (cam == null)
        {
            cam = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
        }
    }

    private void Update()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }

        // === Mouse Scroll Zoom (used for PC + mobile emulation) ===
        float scroll = player.GetAxis("ZoomScroll");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Zoom(scroll);
        }

#if UNITY_EDITOR
        // === Simulate Pinch Zoom with Mouse Scroll for Mobile Testing in Editor ===
        float simulatedPinch = Input.mouseScrollDelta.y;
        if (Mathf.Abs(simulatedPinch) > 0.01f)
        {
            Zoom(simulatedPinch);
        }
#endif

#if UNITY_ANDROID || UNITY_IOS
        HandleTouchZoomAndPan();
#endif

        // === Mouse Drag Panning ===
        if (player.GetButtonDown("MouseLeftClick"))
        {
            previousMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (player.GetButton("MouseLeftClick"))
        {
            canZoomOnPosition = false;
            panSpeed = 350f;
            Vector3 direction = previousMousePosition - cam.ScreenToWorldPoint(Input.mousePosition);
            PanImage(direction);
        }

        // === Controller/Keyboard Zooming ===
        if (player.GetButtonDown("ZoomIn"))
        {
            Zoom(zoomSpeed);
        }

        if (player.GetButtonDown("ZoomOut"))
        {
            Zoom(-zoomSpeed);
        }

        // === Controller/Keyboard/Joystick Panning ===
        if (ReInput.controllers.joystickCount > 0)
        {
            panSpeed = 600f;
            float horizontalDirec = player.GetAxis("PanHorizontal");
            float verticalDirec = player.GetAxis("PanVertical");
            PanImage(new Vector3(horizontalDirec, verticalDirec, 0));
        }
        else
        {
            panSpeed = 600f;
            float horizontalDirec = 0;
            float verticalDirec = 0;

            if (player.GetButton("PanHorizontal"))
            {
                canZoomOnPosition = false;
                horizontalDirec = 1;
            }

            if (player.GetNegativeButton("PanHorizontal"))
            {
                canZoomOnPosition = false;
                horizontalDirec = -1;
            }

            if (player.GetButton("PanVertical"))
            {
                canZoomOnPosition = false;
                verticalDirec = 1;
            }

            if (player.GetNegativeButton("PanVertical"))
            {
                canZoomOnPosition = false;
                verticalDirec = -1;
            }

            PanImage(new Vector3(horizontalDirec, verticalDirec, 0));
        }
    }

    void HandleTouchZoomAndPan()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentTouchDeltaMag = (touch0.position - touch1.position).magnitude;

            float deltaMagnitudeDiff = currentTouchDeltaMag - prevTouchDeltaMag;

            float zoomAmount = deltaMagnitudeDiff * 0.01f; // Adjust the multiplier to control sensitivity
            if (Mathf.Abs(zoomAmount) > 0.001f)
            {
                Zoom(zoomAmount);
            }
        }
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;
                canZoomOnPosition = false;
                panSpeed = 1.5f;
                PanImage(new Vector3(delta.x * -1, delta.y * -1, 0));
            }
        }
    }

    void PanImage(Vector3 direction)
    {
        panelRectTransform.localPosition -= panSpeed * Time.unscaledDeltaTime * direction;
        panelRectTransform.localPosition = ClampPosition(panelRectTransform.localPosition);
    }

    void Zoom(float increment)
    {
        float factor = Mathf.Clamp(panelRectTransform.transform.localScale.x + (increment * zoomSpeed), minZoom, maxZoom);
        panelRectTransform.transform.localScale = new Vector3(factor, factor, 0);

        currentScale = factor;

        if (factor == 1)
        {
            canZoomOnPosition = true;
        }

        if (increment > 0 && canZoomOnPosition)
        {
            if (zoomLocation == ZOOM_LOCATION.LEFT)
            {
                panelRectTransform.localPosition = new Vector3(10000, panelRectTransform.localPosition.y, panelRectTransform.localPosition.z);
            }
            else if (zoomLocation == ZOOM_LOCATION.RIGHT)
            {
                panelRectTransform.localPosition = new Vector3(-10000, panelRectTransform.localPosition.y, panelRectTransform.localPosition.z);
            }
        }

        if (increment < 0)
        {
            canZoomOnPosition = false;
        }

        CalculatePanLimits();

        if (increment != 0)
        {
            panelRectTransform.localPosition = ClampPosition(panelRectTransform.localPosition);
        }

        SetControllerPanel();
    }

    void SetControllerPanel()
    {
#if UNITY_IOS
        return;
#else
        if (panelRectTransform.gameObject.activeSelf)
        {
            if (currentScale > 5)
            {
                if (controlletPanel.alpha == 1)
                    controlletPanel.DOFade(0, 0.2f).From(1).SetUpdate(true);
            }
            else
            {
                if (controlletPanel.alpha == 0)
                    controlletPanel.DOFade(1, 0.2f).From(0).SetUpdate(true);
            }
        }
        else
        {
            if (controlletPanel.alpha == 1)
                controlletPanel.DOFade(0, 0.2f).From(1).SetUpdate(true);
        }
#endif
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        float clampedX = Mathf.Clamp(position.x, panLimitMin.x, panLimitMax.x);
        float clampedY = Mathf.Clamp(position.y, panLimitMin.y, panLimitMax.y);
        return new Vector3(clampedX, clampedY, position.z);
    }

    private void CalculatePanLimits()
    {
        RectTransform parentRect = panelRectTransform.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 panelSize = panelRectTransform.rect.size * panelRectTransform.localScale.x;
        Vector2 parentSize = parentRect.rect.size;

        panLimitMin = new Vector2(-((panelSize.x - parentSize.x) * 0.5f), -((panelSize.y - parentSize.y) * 0.5f));
        panLimitMax = new Vector2((panelSize.x - parentSize.x) * 0.5f, (panelSize.y - parentSize.y) * 0.5f);
    }

    public void SetPivot(float value)
    {
        panelRectTransform.pivot = new Vector2(0.5f, value);
    }
}



//using DG.Tweening;
//using Rewired;
//using UnityEngine;

//public class ZoomAndPan : MonoBehaviour
//{
//    public CanvasGroup controlletPanel;
//    public RectTransform panelRectTransform;
//    public RectTransform treePanel;
//    public float zoomSpeed = 0.5f;
//    public float panSpeed = 10f;
//    public float maxZoom = 3f;
//    public float minZoom = 1f;

//    [SerializeField] private Vector2 panLimitMin = new Vector2(-2500, -1100);
//    [SerializeField] private Vector2 panLimitMax = new Vector2(2500, 1100);
//    private Vector3 previousMousePosition;

//    public Camera cam;
//    private Player player { get { return ReInput.players.GetPlayer(0); } }

//    float currentScale;
//    bool canZoomOnPosition;

//    public enum ZOOM_LOCATION
//    {
//        LEFT,
//        CENTER,
//        RIGHT
//    }

//    public ZOOM_LOCATION zoomLocation = ZOOM_LOCATION.CENTER;

//    private void Start()
//    {
//        CalculatePanLimits();

//        if (cam == null)
//        {
//            cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
//        }
//    }

//    private void Update()
//    {
//        // Zooming
//        Zoom(player.GetAxis("ZoomScroll"));

//        if (cam == null)
//        {
//            cam = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
//        }

//        // Panning
//        if (player.GetButtonDown("MouseLeftClick"))
//        {
//            previousMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
//        }
//        else if (player.GetButton("MouseLeftClick"))
//        {
//            canZoomOnPosition = false;
//            panSpeed = 350f;
//            Vector3 direction = previousMousePosition - cam.ScreenToWorldPoint(Input.mousePosition);

//            PanImage(direction);

//        }


//        if (player.GetButtonDown("ZoomIn"))
//        {
//            Zoom(zoomSpeed);
//        }

//        if (player.GetButtonDown("ZoomOut"))
//        {
//            Zoom(-zoomSpeed);
//        }

//        if (ReInput.controllers.joystickCount > 0)
//        {
//            panSpeed = 600f;
//            float horizontalDirec = player.GetAxis("PanHorizontal");
//            float verticalDirec = player.GetAxis("PanVertical");

//            PanImage(new Vector3(horizontalDirec, verticalDirec, 0));
//        }
//        else
//        {
//            panSpeed = 600f;

//            float horizontalDirec = 0;
//            float verticalDirec = 0;

//            if (player.GetButton("PanHorizontal"))
//            {
//                canZoomOnPosition = false;
//                horizontalDirec = 1;
//            }

//            if (player.GetNegativeButton("PanHorizontal"))
//            {
//                canZoomOnPosition = false;
//                horizontalDirec = -1;
//            }

//            if (player.GetButton("PanVertical"))
//            {
//                canZoomOnPosition = false;
//                verticalDirec = 1;
//            }

//            if (player.GetNegativeButton("PanVertical"))
//            {
//                canZoomOnPosition = false;
//                verticalDirec = -1;
//            }

//            PanImage(new Vector3(horizontalDirec, verticalDirec, 0));
//        }

//    }

//    void PanImage(Vector3 direction)
//    {
//        panelRectTransform.localPosition -= panSpeed * Time.unscaledDeltaTime * direction;
//        panelRectTransform.localPosition = ClampPosition(panelRectTransform.localPosition);
//    }

//    public float lerpSpeed = 5f;
//    void Zoom(float increment)
//    {
//        float factor = Mathf.Clamp(panelRectTransform.transform.localScale.x + (increment * zoomSpeed), minZoom, maxZoom);
//        panelRectTransform.transform.localScale = new Vector3(factor, factor, 0);

//        currentScale = factor;

//        if (factor == 1)
//        {
//            canZoomOnPosition = true;
//        }

//        if (increment > 0 && canZoomOnPosition)
//        {
//            if (zoomLocation == ZOOM_LOCATION.LEFT)
//            {
//                panelRectTransform.localPosition = new Vector3(10000, panelRectTransform.localPosition.y, panelRectTransform.localPosition.z);
//            }
//            else if (zoomLocation == ZOOM_LOCATION.RIGHT)
//            {
//                panelRectTransform.localPosition = new Vector3(-10000, panelRectTransform.localPosition.y, panelRectTransform.localPosition.z);
//            }
//        }

//        if (increment < 0)
//        {
//            canZoomOnPosition = false;
//        }

//        CalculatePanLimits();
//        if (increment != 0)
//        {
//            panelRectTransform.localPosition = ClampPosition(panelRectTransform.localPosition);
//        }

//        SetControllerPanel();
//    }

//    void SetControllerPanel()
//    {
//        #if UNITY_IOS
//            return;
//        #else
//        if (panelRectTransform.gameObject.activeSelf)
//        {
//            if (currentScale > 5)
//            {
//                if (controlletPanel.alpha == 1)
//                    controlletPanel.DOFade(0, 0.2f).From(1).SetUpdate(true);
//            }
//            else
//            {
//                if (controlletPanel.alpha == 0)
//                    controlletPanel.DOFade(1, 0.2f).From(0).SetUpdate(true);
//            }
//        }
//        else
//        {
//            if (controlletPanel.alpha == 1)
//                controlletPanel.DOFade(0, 0.2f).From(1).SetUpdate(true);
//        }
//        #endif
//    }

//    private Vector3 ClampPosition(Vector3 position)
//    {
//        float clampedX = Mathf.Clamp(position.x, panLimitMin.x, panLimitMax.x);
//        float clampedY = Mathf.Clamp(position.y, panLimitMin.y, panLimitMax.y);
//        return new Vector3(clampedX, clampedY, position.z);
//    }

//    private void CalculatePanLimits()
//    {
//        RectTransform parentRect = panelRectTransform.parent as RectTransform;
//        if (parentRect == null) return;

//        // Get the scaled size of the panel
//        Vector2 panelSize = panelRectTransform.rect.size * panelRectTransform.localScale.x;
//        Vector2 parentSize = parentRect.rect.size;

//        // Calculate limits to keep the panel fully visible inside its parent
//        panLimitMin = new Vector2(-((panelSize.x - parentSize.x) * 0.5f), -((panelSize.y - parentSize.y) * 0.5f));
//        panLimitMax = new Vector2((panelSize.x - parentSize.x) * 0.5f, (panelSize.y - parentSize.y) * 0.5f);
//    }

//    public void SetPivot(float value)
//    {
//        panelRectTransform.pivot = new Vector2(0.5f, value);
//    }
//}