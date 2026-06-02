using UnityEngine;
using UnityEngine.InputSystem;
using BlackJackBattleTest;

public class CardDrawer : MonoBehaviour
{
    public GameObject cardPrefab;

    public Transform deckPosition;

    public Transform playerSpawn;

    private Deck deck;

    private int cardsDrawn = 0;

    void Start()
    {
        deck = new Deck();
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        Card drawnCard = deck.Draw();

        GameObject cardObject =
            Instantiate(
                cardPrefab,
                playerSpawn.position +
                new Vector3(cardsDrawn * 0.08f, 0, 0),
                Quaternion.identity
            );

        CardDisplay display =
            cardObject.GetComponent<CardDisplay>();

        if (display != null)
        {
            display.SetCard(
                drawnCard.Rank + "\n" +
                drawnCard.Suit
            );
        }

        Debug.Log("Drew: " + drawnCard);

        cardsDrawn++;
    }
}