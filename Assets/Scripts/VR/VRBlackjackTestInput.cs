using UnityEngine;
using BlackJackBattleTest;

public class VRBlackjackTestInput : MonoBehaviour
{
    [Header("References")]
    public CardDrawer cardDrawer;
    public Transform playerSpawn;
    public Transform dealerSpawn;

    private Deck deck;
    private int playerCardCount = 0;
    private int dealerCardCount = 0;

    private void Start()
    {
        deck = new Deck();
    }

    private void Update()
    {
        if (cardDrawer == null)
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