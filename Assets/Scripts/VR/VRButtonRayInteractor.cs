using UnityEngine;

public class VRButtonRayInteractor : MonoBehaviour
{
    [Header("Controller")]
    public Transform rightControllerTransform;

    [Header("Raycast Settings")]
    public float maxDistance = 5.0f;
    public float maxPressDistance = 1.0f;

    private VRPhysicalButton currentTarget;

    private void Update()
    {
        if (rightControllerTransform == null)
        {
            return;
        }

        UpdateCurrentTarget();

        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            PressCurrentTarget();
        }
    }

    private void UpdateCurrentTarget()
    {
        currentTarget = null;

        Vector3 start = rightControllerTransform.position;
        Vector3 direction = rightControllerTransform.forward;

        if (Physics.Raycast(
                start,
                direction,
                out RaycastHit hit,
                maxDistance,
                ~0,
                QueryTriggerInteraction.Collide))
        {
            VRPhysicalButton button = hit.collider.GetComponent<VRPhysicalButton>();

            if (button == null)
            {
                button = hit.collider.GetComponentInParent<VRPhysicalButton>();
            }

            currentTarget = button;
        }
    }

    private void PressCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.PressButton();
            Debug.Log("Ray pressed button: " + currentTarget.gameObject.name);
            return;
        }

        PressNearestButtonFallback();
    }

    private void PressNearestButtonFallback()
    {
        VRPhysicalButton[] buttons = FindObjectsByType<VRPhysicalButton>(
            FindObjectsSortMode.None
        );

        VRPhysicalButton nearestButton = null;
        float nearestDistance = maxPressDistance;

        foreach (VRPhysicalButton button in buttons)
        {
            float distance = Vector3.Distance(
                rightControllerTransform.position,
                button.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestButton = button;
            }
        }

        if (nearestButton != null)
        {
            nearestButton.PressButton();
            Debug.Log("Nearest VR button pressed: " + nearestButton.gameObject.name);
        }
    }
}