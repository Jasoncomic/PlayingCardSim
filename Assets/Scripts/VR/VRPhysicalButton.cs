using System.Collections;
using UnityEngine;

public class VRPhysicalButton : MonoBehaviour
{
    public enum ButtonAction
    {
        Hit,
        Stand,
        NewRound,
        HelpToggle
    }

    [Header("References")]
    public NetworkBlackjackTable networkBlackjackTable;

    [Header("Help Paper")]
    public GameObject helpPaper;
    public bool helpPaperStartsHidden = true;

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

    private void Start()
    {
        if (action == ButtonAction.HelpToggle &&
            helpPaper != null &&
            helpPaperStartsHidden)
        {
            helpPaper.SetActive(false);
        }
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

        switch (action)
        {
            case ButtonAction.HelpToggle:
                ToggleHelpPaper();
                Debug.Log("VR Button pressed: HELP");
                break;

            case ButtonAction.Hit:
                if (networkBlackjackTable == null)
                {
                    Debug.LogWarning("VRPhysicalButton: No NetworkBlackjackTable assigned.");
                    return;
                }

                networkBlackjackTable.HitButton();
                Debug.Log("VR Button pressed: HIT");
                break;

            case ButtonAction.Stand:
                if (networkBlackjackTable == null)
                {
                    Debug.LogWarning("VRPhysicalButton: No NetworkBlackjackTable assigned.");
                    return;
                }

                networkBlackjackTable.StandButton();
                Debug.Log("VR Button pressed: STAND");
                break;

            case ButtonAction.NewRound:
                if (networkBlackjackTable == null)
                {
                    Debug.LogWarning("VRPhysicalButton: No NetworkBlackjackTable assigned.");
                    return;
                }

                networkBlackjackTable.StartRoundButton();
                Debug.Log("VR Button pressed: NEW ROUND");
                break;
        }

        StopAllCoroutines();
        StartCoroutine(PlayPressAnimation());
    }

    private void ToggleHelpPaper()
    {
        if (helpPaper == null)
        {
            Debug.LogWarning("VRPhysicalButton: Help Paper is not assigned.");
            return;
        }

        helpPaper.SetActive(!helpPaper.activeSelf);
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