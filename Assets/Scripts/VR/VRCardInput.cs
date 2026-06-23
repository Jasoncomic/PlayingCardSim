using UnityEngine;

public class VRCardInput : MonoBehaviour
{


public CardDrawer cardDrawer; // verbindet die Controller-Eingaben mit dem CardDrawer

    // =====================================
    // Eingaben prüfen
    // =====================================

    private void Update()
    {
        if (cardDrawer == null) // bricht ab, wenn kein CardDrawer zugewiesen ist
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
