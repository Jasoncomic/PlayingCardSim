using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BlackJackBattleTest;

public class VRBlackjackController : MonoBehaviour
{
// =====================================
// Referenzen
// =====================================


[Header("References")]
    public VRCardDrawer cardDrawer; // zeichnet und platziert die Karten im VR-Raum

    [Header("VR UI")]
    public TMP_Text resultText; // zeigt Ergebnis und Spielstatus an

    [Header("Timing")]
    public float dealerDrawDelay = 1.5f; // Wartezeit zwischen Dealer-Aktionen

    // =====================================
    // Karten und Hände
    // =====================================

    private Deck deck; // aktuelles Kartendeck

    private readonly List<Card> playerHand = new List<Card>(); // Karten des Spielers
    private readonly List<Card> dealerHand = new List<Card>(); // Karten des Dealers

    private GameObject hiddenDealerCardObject; // verdeckte Dealer-Karte als Objekt in der Szene

    // =====================================
    // Rundenstatus
    // =====================================

    private bool roundOver = false; // merkt sich, ob die Runde beendet ist
    private bool dealerHiddenCardRevealed = false; // merkt sich, ob die verdeckte Dealer-Karte aufgedeckt wurde
    private bool dealerIsPlaying = false; // verhindert Eingaben, während der Dealer spielt

    // =====================================
    // Start
    // =====================================

    private void Start()
    {
        StartNewRound(); // startet beim Szenenstart direkt eine neue Runde
    }

    // =====================================
    // Neue Runde starten
    // =====================================

    public void StartNewRound()
    {
        StopAllCoroutines(); // stoppt alte Dealer-Abläufe

        deck = new Deck(); // erstellt ein neues Deck

        playerHand.Clear();
        dealerHand.Clear();

        roundOver = false;
        dealerIsPlaying = false;
        dealerHiddenCardRevealed = false;
        hiddenDealerCardObject = null;

        if (cardDrawer != null)
        {
            cardDrawer.ClearCards(); // entfernt alte Karten aus der Szene
        }

        SetResultText("");

        // Initial deal:
        // Player gets two open cards.
        DrawPlayerCard();
        DrawPlayerCard();

        // Dealer gets one open card and one hidden card.
        DrawDealerOpenCard();
        DrawDealerHiddenCard();

        Debug.Log("VR Blackjack: New round started.");
        Debug.Log("Player value: " + CalculateHandValue(playerHand));
        Debug.Log("Dealer visible value: " + CalculateVisibleDealerValue());
    }

    // =====================================
    // Spieler zieht Karte
    // =====================================

    public void PlayerHit()
    {
        if (roundOver || dealerIsPlaying) // verhindert Hit nach Rundenende oder während des Dealer-Zugs
        {
            return;
        }

        DrawPlayerCard();

        int playerValue = CalculateHandValue(playerHand); // berechnet aktuellen Spielerwert
        Debug.Log("Player value: " + playerValue);

        if (playerValue > 21) // Spieler ist über 21
        {
            SetResultText("Player bust...");
            Debug.Log("Player bust. Dealer reveals and finishes round.");
            StartCoroutine(DealerTurnRoutine());
        }
    }

    // =====================================
    // Spieler bleibt stehen
    // =====================================

    public void Stand()
    {
        if (roundOver || dealerIsPlaying) // verhindert Stand, wenn die Runde nicht mehr aktiv ist
        {
            return;
        }

        Debug.Log("Player STAND.");
        SetResultText("Dealer's turn...");
        StartCoroutine(DealerTurnRoutine());
    }

    // =====================================
    // Runde zurücksetzen
    // =====================================

    public void ResetRound()
    {
        StartNewRound();
    }

    // =====================================
    // Dealer-Zug
    // =====================================

    private IEnumerator DealerTurnRoutine()
    {
        dealerIsPlaying = true;

        RevealDealerHiddenCard();

        yield return new WaitForSeconds(dealerDrawDelay);

        while (CalculateHandValue(dealerHand) < 17) // Dealer zieht bis mindestens 17
        {
            DrawDealerOpenCard();

            yield return new WaitForSeconds(dealerDrawDelay);
        }

        FinishRound();

        dealerIsPlaying = false;
    }

    // =====================================
    // Spielerkarte ziehen
    // =====================================

    private void DrawPlayerCard()
    {
        if (deck == null)
        {
            deck = new Deck();
        }

        Card card = deck.Draw();
        playerHand.Add(card);

        if (cardDrawer != null)
        {
            cardDrawer.SpawnPlayerCard(card, playerHand.Count - 1); // erzeugt die Karte visuell beim Spieler
        }

        Debug.Log("Player drew: " + card);
    }

    // =====================================
    // Offene Dealer-Karte ziehen
    // =====================================

    private void DrawDealerOpenCard()
    {
        if (deck == null)
        {
            deck = new Deck();
        }

        Card card = deck.Draw();
        dealerHand.Add(card);

        if (cardDrawer != null)
        {
            cardDrawer.SpawnDealerCard(card, dealerHand.Count - 1); // erzeugt die offene Dealer-Karte
        }

        Debug.Log("Dealer drew open card: " + card);
    }

    // =====================================
    // Verdeckte Dealer-Karte ziehen
    // =====================================

    private void DrawDealerHiddenCard()
    {
        if (deck == null)
        {
            deck = new Deck();
        }

        Card card = deck.Draw();
        dealerHand.Add(card);

        if (cardDrawer != null)
        {
            hiddenDealerCardObject = cardDrawer.SpawnHiddenDealerCard(
                card,
                dealerHand.Count - 1
            ); // erzeugt die verdeckte Dealer-Karte
        }

        Debug.Log("Dealer drew hidden card.");
    }

    // =====================================
    // Verdeckte Dealer-Karte aufdecken
    // =====================================

    private void RevealDealerHiddenCard()
    {
        if (dealerHiddenCardRevealed)
        {
            return;
        }

        if (dealerHand.Count < 2)
        {
            return;
        }

        Card hiddenCard = dealerHand[1]; // zweite Dealer-Karte ist die verdeckte Karte

        if (cardDrawer != null)
        {
            hiddenDealerCardObject = cardDrawer.RevealDealerCard(
                hiddenDealerCardObject,
                hiddenCard,
                1
            );
        }

        dealerHiddenCardRevealed = true;

        Debug.Log("Dealer revealed hidden card: " + hiddenCard); // schreibt aufgedeckte Karte in die Console
        Debug.Log("Dealer value: " + CalculateHandValue(dealerHand)); // schreibt Dealerwert in die Console
    }

    // =====================================
    // Runde auswerten
    // =====================================

    private void FinishRound()
    {
        int playerValue = CalculateHandValue(playerHand); // berechnet finalen Spielerwert
        int dealerValue = CalculateHandValue(dealerHand); // berechnet finalen Dealerwert

        roundOver = true;

        Debug.Log("Final Player value: " + playerValue);
        Debug.Log("Final Dealer value: " + dealerValue);

        if (playerValue > 21 && dealerValue > 21)
        {
            SetResultText("Both bust\nPush\n" + playerValue + " vs " + dealerValue);
        }
        else if (playerValue > 21)
        {
            SetResultText("Player bust\nDealer wins\n" + playerValue + " vs " + dealerValue);
        }
        else if (dealerValue > 21)
        {
            SetResultText("Dealer bust\nPlayer wins\n" + playerValue + " vs " + dealerValue);
        }
        else if (playerValue > dealerValue)
        {
            SetResultText("Player wins\n" + playerValue + " vs " + dealerValue);
        }
        else if (playerValue < dealerValue)
        {
            SetResultText("Dealer wins\n" + dealerValue + " vs " + playerValue);
        }
        else
        {
            SetResultText("Push\n" + playerValue + " vs " + dealerValue);
        }
    }

    // =====================================
    // Ergebnistext setzen
    // =====================================

    private void SetResultText(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }

        if (!string.IsNullOrEmpty(message))
        {
            Debug.Log(message);
        }
    }

    // =====================================
    // Sichtbaren Dealerwert berechnen
    // =====================================

    private int CalculateVisibleDealerValue()
    {
        if (dealerHand.Count == 0)
        {
            return 0;
        }

        return GetCardValue(dealerHand[0]); // nur die erste offene Dealer-Karte zählt sichtbar
    }

    // =====================================
    // Handwert berechnen
    // =====================================

    private int CalculateHandValue(List<Card> hand)
    {
        int value = 0; // aktueller Gesamtwert der Hand
        int aceCount = 0; 
         
        foreach (Card card in hand)
        {
            int cardValue = GetCardValue(card); // berechnet Wert der aktuellen Karte
            value += cardValue;

            string rank = card.Rank.ToString().ToLower();

            if (rank == "ace" || rank == "a")
            {
                aceCount++;
            }
        }

        while (value > 21 && aceCount > 0) // Ass wird von 11 auf 1 reduziert
        {
            value -= 10;
            aceCount--;
        }

        return value;
    }

    // =====================================
    // Kartenwert bestimmen
    // =====================================

    private int GetCardValue(Card card)
    {
        string rank = card.Rank.ToString().ToLower(); // wandelt den Kartenrang in Kleinbuchstaben um

        if (rank == "ace" || rank == "a")
        {
            return 11;
        }

        if (rank == "king" || rank == "queen" || rank == "jack" ||
            rank == "k" || rank == "q" || rank == "j")
        {
            return 10;
        }

        if (rank == "ten") return 10;
        if (rank == "nine") return 9;
        if (rank == "eight") return 8;
        if (rank == "seven") return 7;
        if (rank == "six") return 6;
        if (rank == "five") return 5;
        if (rank == "four") return 4;
        if (rank == "three") return 3;
        if (rank == "two") return 2;

        if (int.TryParse(rank, out int numericValue)) // versucht numerische Kartenwerte direkt zu lesen
        {
            return numericValue;
        }

        Debug.LogWarning("Unknown card rank: " + card.Rank); // warnt bei unbekanntem Kartenrang
        return 0;
    }
}