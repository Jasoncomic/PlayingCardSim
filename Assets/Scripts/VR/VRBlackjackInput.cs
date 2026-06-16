using UnityEngine;

public class VRBlackjackInput : MonoBehaviour
{
    [Header("References")]
    public VRBlackjackController blackjackController;

    private void Update()
    {
        if (blackjackController == null)
        {
            return;
        }

        // Right controller A button: Hit
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            blackjackController.PlayerHit();
        }

        // Right controller B button: Stand
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            blackjackController.Stand();
        }

        // Left controller Y button: Reset round
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            blackjackController.ResetRound();
        }
    }
}