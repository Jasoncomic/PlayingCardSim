using UnityEngine;

public class VRCardInput : MonoBehaviour
{
    public CardDrawer cardDrawer;

    private void Update()
    {
        if (cardDrawer == null)
        {
            return;
        }

        // A-Button am rechten Quest Controller: Karte ziehen
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            cardDrawer.DrawTestCard();
        }

        // B-Button am rechten Quest Controller: Karten löschen
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            cardDrawer.ClearSpawnedCards();
        }
    }
}