using DG.Tweening;
using UnityEngine;

public class CurrentNodeItem : MonoBehaviour
{

    [SerializeField] private float _scaleMultiplier = 1.1f;
    [SerializeField] private float _scaleDuration = 1f;
    [SerializeField] private Transform scalingObject;

    private Transform _parent;
    private Transform _instObj;
    private Transform _nodeTransform;
    private Canvas _canvas;

    private Transform _grandParent;

    private void Awake()
    {
        _parent = transform;
        _grandParent = _parent.parent;
        _canvas = GetComponent<Canvas>();
    }

    public void BounceCurrentNode()
    {
        _instObj = Instantiate(scalingObject, _parent);
        _nodeTransform = _instObj.transform;
        _nodeTransform.DOScale(Vector3.one * _scaleMultiplier, _scaleDuration).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);

        _instObj.transform.parent = _grandParent.transform;
        _instObj.SetAsFirstSibling();
    }
    public void StopBounce()
    {
        if (_nodeTransform != null)
        {
            Destroy(_nodeTransform.gameObject);
        }
    }
}
