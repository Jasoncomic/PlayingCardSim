using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using BlackJackBattleTest;

public class CardDrawer : MonoBehaviour
{
    public enum CardOffsetDirection
    {
        Player1_X,
        Player2_NegativeZ,
        Player3_PositiveZ
    }

    [Header("Card Setup")]
    public GameObject cardPrefab;
    public Transform deckPosition;

    [Header("Player Card Spawns")]
    public Transform playerSpawn;      // Player 1
    public Transform player2Spawn;     // Player 2
    public Transform player3Spawn;     // Player 3

    [Header("Card Layout")]
    public float cardSpacing = 0.35f;
    public float cardYOffset = 0.01f;

    private Deck testDeck;
    private int testCardsDrawn = 0;

    private readonly List<GameObject> spawnedCards = new List<GameObject>();

    private void Start()
    {
        testDeck = new Deck();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        // Test draw for Player 1
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            DrawTestCard();
        }

        // Optional test draws
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            DrawTestCardForPlayer(2);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            DrawTestCardForPlayer(3);
        }
    }

    public void DrawTestCard()
    {
        if (testDeck == null)
        {
            testDeck = new Deck();
        }

        Card drawnCard = testDeck.Draw();

        SpawnCardVisual(
            drawnCard,
            playerSpawn,
            testCardsDrawn,
            CardOffsetDirection.Player1_X
        );

        testCardsDrawn++;

        Debug.Log("Test drew: " + drawnCard);
    }

    public void DrawTestCardForPlayer(int playerNumber)
    {
        if (testDeck == null)
        {
            testDeck = new Deck();
        }

        Card drawnCard = testDeck.Draw();

        if (playerNumber == 1)
        {
            SpawnCardVisual(
                drawnCard,
                playerSpawn,
                testCardsDrawn,
                CardOffsetDirection.Player1_X
            );
        }
        else if (playerNumber == 2)
        {
            SpawnCardVisual(
                drawnCard,
                player2Spawn,
                testCardsDrawn,
                CardOffsetDirection.Player2_NegativeZ
            );
        }
        else if (playerNumber == 3)
        {
            SpawnCardVisual(
                drawnCard,
                player3Spawn,
                testCardsDrawn,
                CardOffsetDirection.Player3_PositiveZ
            );
        }

        testCardsDrawn++;

        Debug.Log("Test drew for Player " + playerNumber + ": " + drawnCard);
    }

    // Keeps old scripts working.
    // Default behavior is Player 1, spreading on X axis.
    public void SpawnCardVisual(Card card, Transform spawnPoint, int cardIndex)
    {
        SpawnCardVisual(
            card,
            spawnPoint,
            cardIndex,
            CardOffsetDirection.Player1_X
        );
    }

    public void SpawnCardVisual(
        Card card,
        Transform spawnPoint,
        int cardIndex,
        CardOffsetDirection offsetDirection
    )
    {
        if (cardPrefab == null)
        {
            Debug.LogWarning("CardDrawer has no card prefab assigned.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("CardDrawer has no spawn point assigned.");
            return;
        }

        Vector3 offset = GetCardOffset(cardIndex, offsetDirection);

        GameObject cardObject = Instantiate(
            cardPrefab,
            spawnPoint.position + offset,
            spawnPoint.rotation
        );

        spawnedCards.Add(cardObject);

        CardDisplay display = cardObject.GetComponent<CardDisplay>();

        if (display != null)
        {
            display.SetCard(
                card.Rank + "\n" +
                card.Suit
            );
        }

        CardTextureDisplay textureDisplay =
            cardObject.GetComponent<CardTextureDisplay>();

        if (textureDisplay != null)
        {
            textureDisplay.SetCard(card);
        }
    }

    private Vector3 GetCardOffset(
        int cardIndex,
        CardOffsetDirection offsetDirection
    )
    {
        float spacing = cardIndex * cardSpacing;

        switch (offsetDirection)
        {
            case CardOffsetDirection.Player1_X:
                return new Vector3(spacing, cardYOffset, 0f);

            case CardOffsetDirection.Player2_NegativeZ:
                return new Vector3(0f, cardYOffset, -spacing);

            case CardOffsetDirection.Player3_PositiveZ:
                return new Vector3(0f, cardYOffset, spacing);

            default:
                return new Vector3(spacing, cardYOffset, 0f);
        }
    }

    public void ClearSpawnedCards()
    {
        for (int i = spawnedCards.Count - 1; i >= 0; i--)
        {
            if (spawnedCards[i] != null)
            {
                Destroy(spawnedCards[i]);
            }
        }

        spawnedCards.Clear();
        testCardsDrawn = 0;
    }
}