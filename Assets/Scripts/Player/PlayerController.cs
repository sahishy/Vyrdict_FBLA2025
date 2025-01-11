using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("Settings")]
    public float speed;
    public int bounds;

    private Vector2 moveVector;

    [HideInInspector] public bool zoomed;
    private Vector3 originalZoomPos;

    private void Awake() {
        instance = this;
    }

    void Update()
    {
        if(!DecisionHandler.instance.inDecisionMode) {
            PlayerMovement();
        }
    }

    public void OnMove(InputAction.CallbackContext context) {
        moveVector = context.ReadValue<Vector2>();
    }
    private void PlayerMovement() {
        Vector3 movement = speed * Time.deltaTime * new Vector3(moveVector.x, 0f, moveVector.y);
        transform.Translate(movement, Space.World);

        //boundaries
        float clampedX = Mathf.Clamp(transform.position.x, -bounds, bounds);
        float clampedZ = Mathf.Clamp(transform.position.z, -bounds, bounds);
        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }
    
    public void CameraZoom(Vector3 targetPos, float time, float startTime = 1f, float endTime = 1f, float startDelay = 0f) {
        zoomed = true;

        StartCoroutine(CameraZoomHelper(targetPos, time, startTime, endTime, startDelay));
    }
    private IEnumerator CameraZoomHelper(Vector3 targetPos, float time, float startTime, float endTime, float startDelay) {
        yield return new WaitForSeconds(startDelay);

        if(zoomed) {
            originalZoomPos = transform.position;
            Vector3 offset = new Vector3(0, 11, -6.5f);

            transform.DOMove(targetPos + offset, startTime).SetEase(Ease.InOutCubic).OnComplete(() => {
                transform.DOMove(originalZoomPos, endTime).SetEase(Ease.InOutCubic).SetDelay(time);
            });
        }
    }
    public void EndZoom() {
        zoomed = false;
        transform.DOMove(originalZoomPos, 0.5f).SetEase(Ease.InOutCubic);
    }
}
