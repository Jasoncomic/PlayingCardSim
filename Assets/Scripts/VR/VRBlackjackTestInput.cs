using UnityEngine;
using BlackJackBattleTest;

public class VRBlackjackTestInput : MonoBehaviour
{

[Header("References")]
    public CardDrawer cardDrawer; // zeichnet Testkarten in Szene
    public Transform playerSpawn; // Position für Spielerkarten
    public Transform dealerSpawn; // Position für Dealerkarten

    // =====================================
    // Testkarten-Zustand
    // =====================================

    private Deck deck; // Test-Kartendeck
    private int playerCardCount = 0; // zählt gezogene Spielerkarten
    private int dealerCardCount = 0; // zählt gezogene Dealerkarten

    // =====================================
    // Start
    // =====================================

    private void Start()
    {
        deck = new Deck(); // erstellt ein neues Testdeck
    }

    // =====================================
    // Eingaben prüfen
    // =====================================

    private void Update()
    {
        if (cardDrawer == null) // bricht ab, wenn kein CardDrawer zugewiesen ist
        {
            return;
        }

        // A-Button rechts: Player zieht Karte
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            DrawPlayerCard();
        }

        // B-Button rechts: Dealer zieht Karte
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            DrawDealerCard();
        }
    }

    // =====================================
    // Spieler-Testkarte ziehen
    // =====================================

    private void DrawPlayerCard()
    {
        if (deck == null)
        {
            deck = new Deck();
        }

        Card card = deck.Draw();
        cardDrawer.SpawnCardVisual(card, playerSpawn, playerCardCount);
        playerCardCount++;

        Debug.Log("Player drew: " + card);
    }

    // =====================================
    // Dealer-Testkarte ziehen
    // =====================================

    private void DrawDealerCard()
    {
        if (deck == null)
        {
            deck = new Deck();
        }

        Card card = deck.Draw();
        cardDrawer.SpawnCardVisual(card, dealerSpawn, dealerCardCount);
        dealerCardCount++;

        Debug.Log("Dealer drew: " + card);
    }


}
