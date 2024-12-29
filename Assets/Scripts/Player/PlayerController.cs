using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float speed;
    public int bounds;

    private Vector2 moveVector;

    void Update() //test
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


}
