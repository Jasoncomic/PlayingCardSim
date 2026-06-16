using System.Collections;
using UnityEngine;

public class VRPhysicalButton : MonoBehaviour
{
    public enum ButtonAction
    {
        Hit,
        Stand,
        Reset
    }

    [Header("References")]
    public VRBlackjackController blackjackController;

    [Header("Button Settings")]
    public ButtonAction action;
    public float cooldown = 0.7f;

    [Header("Visual Feedback")]
    public Vector3 pressedScaleMultiplier = new Vector3(0.9f, 0.6f, 0.9f);
    public float pressAnimationTime = 0.12f;

    private Vector3 originalScale;
    private float lastPressTime = -999f;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsControllerOrHand(other.gameObject))
        {
            PressButton();
        }
    }

    public void PressButton()
    {
        if (Time.time - lastPressTime < cooldown)
        {
            return;
        }

        lastPressTime = Time.time;

        if (blackjackController == null)
        {
            Debug.LogWarning("VRPhysicalButton: No blackjack controller assigned.");
            return;
        }

        switch (action)
        {
            case ButtonAction.Hit:
                blackjackController.PlayerHit();
                Debug.Log("VR Button pressed: HIT");
                break;

            case ButtonAction.Stand:
                blackjackController.Stand();
                Debug.Log("VR Button pressed: STAND");
                break;

            case ButtonAction.Reset:
                blackjackController.ResetRound();
                Debug.Log("VR Button pressed: RESET");
                break;
        }

        StopAllCoroutines();
        StartCoroutine(PlayPressAnimation());
    }

    private bool IsControllerOrHand(GameObject obj)
    {
        string objectName = obj.name.ToLower();

        return objectName.Contains("controller") ||
               objectName.Contains("hand");
    }

    private IEnumerator PlayPressAnimation()
    {
        transform.localScale = new Vector3(
            originalScale.x * pressedScaleMultiplier.x,
            originalScale.y * pressedScaleMultiplier.y,
            originalScale.z * pressedScaleMultiplier.z
        );

        yield return new WaitForSeconds(pressAnimationTime);

        transform.localScale = originalScale;
    }
}