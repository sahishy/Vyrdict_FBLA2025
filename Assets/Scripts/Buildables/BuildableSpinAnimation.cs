using UnityEngine;
using DG.Tweening;

public class BuildableSpinAnimation : MonoBehaviour
{
    [SerializeField] private float rotationTime;
    [SerializeField] private Vector3 targetRotation;

    void Start() {
        transform.DOLocalRotate(targetRotation, rotationTime, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }
}
