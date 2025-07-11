using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using UnityEngine.UI;

namespace UI_Spline_Renderer.Example
{
    public class DraggableSplinePointExample : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public UISplineRenderer uiSplineRenderer;
        public Image myImage;
        public int splineIndex;
        public int knotIndex;
        public Color connectedColor;
        public bool isConnected;
        BezierKnot _originalKnot;
        
        /*
         * You must set false of the raycastTarget value of the UISplineRenderer and Image of this gameObject OnBeginDrag().
         * Because it can block the raycast going to a port that pointer hovered.
         */
        
        public void OnDrag(PointerEventData eventData)
        {
            var pos = transform.parent.InverseTransformPoint(eventData.position);
            var knot = new BezierKnot(pos);
            uiSplineRenderer.splineContainer[splineIndex].SetKnot(knotIndex, knot);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            uiSplineRenderer.raycastTarget = false;
            myImage.raycastTarget = false;
            
            uiSplineRenderer.color = Color.white;
            if(!isConnected)_originalKnot = uiSplineRenderer.splineContainer[splineIndex][knotIndex];
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            foreach (var go in eventData.hovered)
            {
                if (go.GetComponent<DragPortExample>())
                {
                    Connect(go.transform);
                    uiSplineRenderer.raycastTarget = true;
                    myImage.raycastTarget = true;
                    return;
                }
            }
            Disconnect();
        }

        void Connect(Transform t)
        {
            var pos = transform.parent.InverseTransformPoint(t.position);
            var knot = new BezierKnot(pos);
            uiSplineRenderer.splineContainer[splineIndex].SetKnot(knotIndex, knot);
            uiSplineRenderer.color = connectedColor;
            isConnected = true;
        }

        void Disconnect()
        {
            uiSplineRenderer.color = Color.white;
            uiSplineRenderer.splineContainer[splineIndex].SetKnot(knotIndex, _originalKnot);
            isConnected = false;
            uiSplineRenderer.raycastTarget = true;
            myImage.raycastTarget = true;
        }
    }
}