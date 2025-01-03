using DG.Tweening;
using UnityEngine;

public class ConnectionLine : MonoBehaviour
{
    public bool initialized = false;
    private Transform target0;
    private Transform target1;
    private LineRenderer lineRenderer;
    private Vector3 offset = new Vector3(0, 0.25f, 0);

    void Awake() {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    public void Initialize(Transform t0, Transform t1) {
        target0 = t0;
        target1 = t1;
        
        initialized = true;
        
        //DOTween.To(x => lineRenderer.widthMultiplier = x, 0f, 0.25f, 0.5f).SetDelay(0.5f);
    }

    void Update()
    {
        if(!initialized) {
            return;
        }
        
        lineRenderer.SetPosition(0, target0.position + offset);
        lineRenderer.SetPosition(1, target1.position + offset);
    }
}
