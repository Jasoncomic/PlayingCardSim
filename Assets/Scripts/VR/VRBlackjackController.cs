using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BlackJackBattleTest;

public class VRBlackjackController : MonoBehaviour
{
    [Header("References")]
    public VRCardDrawer cardDrawer;

    [Header("VR UI")]
    public TMP_Text resultText;

    [Header("Timing")]
    public float dealerDrawDelay = 1.5f;

    private Deck deck;

    private readonly List<Card> playerHand = new List<Card>();
    private readonly List<Card> dealerHand = new List<Card>();

    private GameObject hiddenDealerCardObject;

    private bool roundOver = false;
    private bool dealerHiddenCardRevealed = false;
    private bool dealerIsPlaying = false;

    private void Start()
    {
        StartNewRound();
    }

    public void StartNewRound()
    {
        StopAllCoroutines();

        deck = new Deck();

        playerHand.Clear();
        dealerHand.Clear();

        roundOver = false;
        dealerIsPlaying = false;
        dealerHiddenCardRevealed = false;
        hiddenDealerCardObject = null;

        if (cardDrawer != null)
        {
            cardDrawer.ClearCards();
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

    public void PlayerHit()
    {
        if (roundOver || dealerIsPlaying)
        {
            return;
        }

        DrawPlayerCard();

        int playerValue = CalculateHandValue(playerHand);
        Debug.Log("Player value: " + playerValue);

        if (playerValue > 21)
        {
            SetResultText("Player bust...");
            Debug.Log("Player bust. Dealer reveals and finishes round.");
            StartCoroutine(DealerTurnRoutine());
        }
    }

    public void Stand()
    {
        if (roundOver || dealerIsPlaying)
        {
            return;
        }

        Debug.Log("Player STAND.");
        SetResultText("Dealer's turn...");
        StartCoroutine(DealerTurnRoutine());
    }

    public void ResetRound()
    {
        StartNewRound();
    }

    private IEnumerator DealerTurnRoutine()
    {
        dealerIsPlaying = true;

        RevealDealerHiddenCard();

        yield return new WaitForSeconds(dealerDrawDelay);

        while (CalculateHandValue(dealerHand) < 17)
        {
            DrawDealerOpenCard();

            yield return new WaitForSeconds(dealerDrawDelay);
        }

        FinishRound();

        dealerIsPlaying = false;
    }

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
            cardDrawer.SpawnPlayerCard(card, playerHand.Count - 1);
        }

        Debug.Log("Player drew: " + card);
    }

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
            cardDrawer.SpawnDealerCard(card, dealerHand.Count - 1);
        }

        Debug.Log("Dealer drew open card: " + card);
    }

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
            );
        }

        Debug.Log("Dealer drew hidden card.");
    }

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

        Card hiddenCard = dealerHand[1];

        if (cardDrawer != null)
        {
            hiddenDealerCardObject = cardDrawer.RevealDealerCard(
                hiddenDealerCardObject,
                hiddenCard,
                1
            );
        }

        dealerHiddenCardRevealed = true;

        Debug.Log("Dealer revealed hidden card: " + hiddenCard);
        Debug.Log("Dealer value: " + CalculateHandValue(dealerHand));
    }

    private void FinishRound()
    {
        int playerValue = CalculateHandValue(playerHand);
        int dealerValue = CalculateHandValue(dealerHand);

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

    private int CalculateVisibleDealerValue()
    {
        if (dealerHand.Count == 0)
        {
            return 0;
        }

        return GetCardValue(dealerHand[0]);
    }

    private int CalculateHandValue(List<Card> hand)
    {
        int value = 0;
        int aceCount = 0;

        foreach (Card card in hand)
        {
            int cardValue = GetCardValue(card);
            value += cardValue;

            string rank = card.Rank.ToString().ToLower();

            if (rank == "ace" || rank == "a")
            {
                aceCount++;
            }
        }

        while (value > 21 && aceCount > 0)
        {
            value -= 10;
            aceCount--;
        }

        return value;
    }

    private int GetCardValue(Card card)
    {
        string rank = card.Rank.ToString().ToLower();

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

        if (int.TryParse(rank, out int numericValue))
        {
            return numericValue;
        }

        Debug.LogWarning("Unknown card rank: " + card.Rank);
        return 0;
    }
}