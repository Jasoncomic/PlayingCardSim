using UnityEngine;

public class VRButtonRayInteractor : MonoBehaviour
{
    [Header("Controller")]
    public Transform rightControllerTransform;

    [Header("Raycast Settings")]
    public float maxDistance = 5.0f;
    public float maxPressDistance = 1.0f;

    [Header("Click Outside Closes Papers")]
    public GameObject[] closeWhenClickOutside;

    private VRPhysicalButton currentTarget;
    private RaycastHit currentHit;
    private bool hasHit;

    private void Update()
    {
        if (rightControllerTransform == null)
        {
            return;
        }

        UpdateCurrentTarget();

        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            HandleTriggerPressed();
        }
    }

    private void UpdateCurrentTarget()
    {
        currentTarget = null;
        hasHit = false;

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
            hasHit = true;
            currentHit = hit;

            VRPhysicalButton button = hit.collider.GetComponent<VRPhysicalButton>();

            if (button == null)
            {
                button = hit.collider.GetComponentInParent<VRPhysicalButton>();
            }

            currentTarget = button;
        }
    }

    private void HandleTriggerPressed()
    {
        bool anyPaperOpen = AnyClosablePaperOpen();

        if (anyPaperOpen)
        {
            bool clickInsidePaper = IsRayHitInsideAnyOpenPaper();

            if (clickInsidePaper)
            {
                if (currentTarget != null)
                {
                    PressCurrentTarget();
                }

                return;
            }

            CloseAllClosablePapers();
            Debug.Log("Clicked outside paper. Closed open papers.");
            return;
        }

        if (currentTarget != null)
        {
            PressCurrentTarget();
            return;
        }

        PressNearestButtonFallback();
    }

    private void PressCurrentTarget()
    {
        currentTarget.PressButton();
        Debug.Log("Ray pressed button: " + currentTarget.gameObject.name);
    }

    private bool AnyClosablePaperOpen()
    {
        if (closeWhenClickOutside == null)
        {
            return false;
        }

        foreach (GameObject paper in closeWhenClickOutside)
        {
            if (paper != null && paper.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsRayHitInsideAnyOpenPaper()
    {
        if (!hasHit || currentHit.collider == null)
        {
            return false;
        }

        if (closeWhenClickOutside == null)
        {
            return false;
        }

        foreach (GameObject paper in closeWhenClickOutside)
        {
            if (paper == null || !paper.activeInHierarchy)
            {
                continue;
            }

            if (currentHit.collider.transform.IsChildOf(paper.transform))
            {
                return true;
            }
        }

        return false;
    }

    private void CloseAllClosablePapers()
    {
        if (closeWhenClickOutside == null)
        {
            return;
        }

        foreach (GameObject paper in closeWhenClickOutside)
        {
            if (paper != null)
            {
                paper.SetActive(false);
            }
        }
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