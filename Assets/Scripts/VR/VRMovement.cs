using UnityEngine;

public class VRMovement : MonoBehaviour
{
// =====================================
// Bewegungseinstellungen
// =====================================

[Header("Movement Settings")]
    public float moveSpeed = 1.2f; // Geschwindigkeit der Fortbewegung

    // =====================================
    // Snap-Turn-Einstellungen
    // =====================================

    [Header("Snap Turn Settings")]
    public float snapTurnAngle = 45.0f; // Winkel pro Drehung
    public float snapTurnCooldown = 0.35f; // Wartezeit zwischen zwei Snap Turns



    [Header("References")]
    public Transform centerEyeAnchor; // Blickrichtung der VR-Kamera

    // =====================================
    // Interner Drehstatus
    // =====================================

    private float lastSnapTurnTime = 0f; // Zeitpunkt der letzten Drehung

    // =====================================
    // Eingaben prüfen
    // =====================================

    void Update()
    {
        if (centerEyeAnchor == null)
        {
            return;
        }

        HandleMovement();
        HandleSnapTurn();
    }

    // =====================================
    // Bewegung verarbeiten
    // =====================================

    private void HandleMovement()
    {
        // Linker Stick: Bewegung
        Vector2 moveInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        Vector3 forward = centerEyeAnchor.forward; // Vorwärtsrichtung aus Blickrichtung
        Vector3 right = centerEyeAnchor.right; // Rechtsrichtung aus Blickrichtung

        // Nur horizontale Bewegung, keine Bewegung nach oben/unten durch Kopfneigung
        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x; // berechnet Bewegungsrichtung
        transform.position += moveDirection * moveSpeed * Time.deltaTime; // bewegt den Spieler
    }

    // =====================================
    // Snap Turn verarbeiten
    // =====================================

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
