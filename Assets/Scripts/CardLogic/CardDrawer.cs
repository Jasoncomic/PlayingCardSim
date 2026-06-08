using UnityEngine;
using UnityEngine.InputSystem;
using BlackJackBattleTest;

public class CardDrawer : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform deckPosition;
    public Transform playerSpawn;

    private Deck testDeck;
    private int testCardsDrawn = 0;
    private int visualCardsSpawned = 0;

    private void Start()
    {
        testDeck = new Deck();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            DrawTestCard();
        }
    }

    public void DrawTestCard()
    {
        Card drawnCard = testDeck.Draw();
        SpawnCardVisual(drawnCard, playerSpawn, testCardsDrawn);
        testCardsDrawn++;

        Debug.Log("Test drew: " + drawnCard);
    }

    public void SpawnCardVisual(Card card, Transform spawnPoint, int cardIndex)
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

        Vector3 offset = new Vector3(cardIndex * 0.26f, 0.01f, 0f);

        GameObject cardObject = Instantiate(
            cardPrefab,
            spawnPoint.position + offset,
            spawnPoint.rotation
        );

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

        visualCardsSpawned++;
    }

    public void ClearSpawnedCards()
    {
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");

        foreach (GameObject card in cards)
        {
            Destroy(card);
        }

        visualCardsSpawned = 0;
    }
}