using UnityEngine;

public class VRMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 1.2f;

    [Header("Snap Turn Settings")]
    public float snapTurnAngle = 45.0f;
    public float snapTurnCooldown = 0.35f;

    [Header("References")]
    public Transform centerEyeAnchor;

    private float lastSnapTurnTime = 0f;

    void Update()
    {
        if (centerEyeAnchor == null)
        {
            return;
        }

        HandleMovement();
        HandleSnapTurn();
    }

    private void HandleMovement()
    {
        // Linker Stick: Bewegung
        Vector2 moveInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        Vector3 forward = centerEyeAnchor.forward;
        Vector3 right = centerEyeAnchor.right;

        // Nur horizontale Bewegung, keine Bewegung nach oben/unten durch Kopfneigung
        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void HandleSnapTurn()
    {
        // Rechter Stick: Snap Turn
        Vector2 turnInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        if (Time.time - lastSnapTurnTime < snapTurnCooldown)
        {
            return;
        }

        if (turnInput.x > 0.7f)
        {
            transform.Rotate(Vector3.up, snapTurnAngle);
            lastSnapTurnTime = Time.time;
        }
        else if (turnInput.x < -0.7f)
        {
            transform.Rotate(Vector3.up, -snapTurnAngle);
            lastSnapTurnTime = Time.time;
        }
    }
}