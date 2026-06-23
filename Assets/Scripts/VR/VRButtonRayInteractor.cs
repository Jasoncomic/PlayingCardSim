using UnityEngine;

public class VRButtonRayInteractor : MonoBehaviour
{

[Header("Controller")]
    public Transform rightControllerTransform; // Transform des rechten Controllers für den Ray

    // ==================================
    // Raycast-Einstellungen
    // ================================

    [Header("Raycast Settings")]
    public float maxDistance = 5.0f; // max Raycast-Reichweite
    public float maxPressDistance = 1.0f; // max Entfernung für Fallback-Buttondruck

    // ==================================
    // Papier-Fenster schließen
    // ==================================

    [Header("Click Outside Closes Papers")]
    public GameObject[] closeWhenClickOutside; // offene Paper-Objekte, die bei Klick außerhalb geschlossen werden

    // ====================================
    // Aktueller Raycast-Zustand
    // ====================================

    private VRPhysicalButton currentTarget; // aktuell vom Ray getroffener Button
    private RaycastHit currentHit; // letzter Raycast-Treffer
    private bool hasHit; // merkt sich, ob der Ray etwas getroffen hat

    // ================================
    // Eingabe prüfen
    // ================================

    private void Update()
    {
        if (rightControllerTransform == null)
        {
            return;
        }

        UpdateCurrentTarget();

        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)) // rechter Trigger drückt aktuellen Buttn
        {
            HandleTriggerPressed();
        }
    }

    // =====================================
    // Aktuelles Ziel suchen
    // =====================================

    private void UpdateCurrentTarget()
    {
        currentTarget = null;
        hasHit = false;

        Vector3 start = rightControllerTransform.position; // Startpunkt des Rays
        Vector3 direction = rightControllerTransform.forward; // Richtung des Rays

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

            VRPhysicalButton button = hit.collider.GetComponent<VRPhysicalButton>(); // sucht Button direkt am Collider

            if (button == null)
            {
                button = hit.collider.GetComponentInParent<VRPhysicalButton>(); // sucht Button am Parent-Objekt
            }

            currentTarget = button;
        }
    }

    // =====================================
    // Triggerdruck verarbeiten
    // =====================================

    private void HandleTriggerPressed()
    {
        bool anyPaperOpen = AnyClosablePaperOpen(); // prüft, ob ein Paper offen ist

        if (anyPaperOpen)
        {
            bool clickInsidePaper = IsRayHitInsideAnyOpenPaper(); // prüft, ob der Klick innerhalb eines offenen Papers ist

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

    // =====================================
    // Aktuellen Button drücken
    // =====================================

    private void PressCurrentTarget()
    {
        currentTarget.PressButton();
        Debug.Log("Ray pressed button: " + currentTarget.gameObject.name);
    }

    // =====================================
    // Prüfen ob Paper offen ist
    // =====================================

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

    // =============================================
    // Prüfen ob Ray innerhalb eines Papers trifft
    // =============================================

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

    // =================================
    // Alle offenen Paper schließen
    // ================================

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

    // =====================================
    // Nächsten Button als Fallback drücken
    // =====================================

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
            ); // Entfernung zw Controller und Button

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
