using System.Collections.Generic;
using UnityEngine;
using BlackJackBattleTest;

public class VRCardDrawer : MonoBehaviour
{
    [Header("Card Prefab")]
    public GameObject cardPrefab;

    [Header("Card Back")]
    public Material cardBackMaterial;

    [Header("Spawn Points")]
    public Transform playerSpawn;
    public Transform dealerSpawn;
    public Transform deckPosition;

    [Header("Card Layout")]
    public float cardSpacing = 0.32f;
    public float cardHeightOffset = 0.004f;

    [Tooltip("Tilts cards upright toward/away from the player. Try -15, 15, -25 or 25 depending on orientation.")]
    public float cardUprightTiltAngle = -15.0f;

    [Tooltip("Optional fan rotation around Y. Set to 0 if you want straight centered rows.")]
    public float cardFanAngle = 0.0f;

    private readonly List<GameObject> spawnedCards = new List<GameObject>();
    private readonly List<GameObject> playerCardObjects = new List<GameObject>();
    private readonly List<GameObject> dealerCardObjects = new List<GameObject>();

    public GameObject SpawnPlayerCard(Card card, int cardIndex)
    {
        GameObject cardObject = CreateCardObject(card, false);
        playerCardObjects.Add(cardObject);
        LayoutHand(playerCardObjects, playerSpawn);
        return cardObject;
    }

    public GameObject SpawnDealerCard(Card card, int cardIndex)
    {
        GameObject cardObject = CreateCardObject(card, false);
        dealerCardObjects.Add(cardObject);
        LayoutHand(dealerCardObjects, dealerSpawn);
        return cardObject;
    }

    public GameObject SpawnHiddenDealerCard(Card card, int cardIndex)
    {
        GameObject cardObject = CreateCardObject(card, true);
        dealerCardObjects.Add(cardObject);
        LayoutHand(dealerCardObjects, dealerSpawn);
        return cardObject;
    }

    public GameObject RevealDealerCard(GameObject hiddenCardObject, Card card, int cardIndex)
    {
        int index = dealerCardObjects.IndexOf(hiddenCardObject);

        if (hiddenCardObject != null)
        {
            Destroy(hiddenCardObject);
            spawnedCards.Remove(hiddenCardObject);
        }

        GameObject revealedCard = CreateCardObject(card, false);

        if (index >= 0 && index < dealerCardObjects.Count)
        {
            dealerCardObjects[index] = revealedCard;
        }
        else
        {
            dealerCardObjects.Add(revealedCard);
        }

        LayoutHand(dealerCardObjects, dealerSpawn);
        return revealedCard;
    }

    private GameObject CreateCardObject(Card card, bool hidden)
    {
        if (cardPrefab == null)
        {
            Debug.LogWarning("VRCardDrawer: No card prefab assigned.");
            return null;
        }

        GameObject cardObject = Instantiate(cardPrefab);
        spawnedCards.Add(cardObject);

        if (hidden)
        {
            ApplyBackMaterial(cardObject);
        }
        else
        {
            ApplyFrontCard(cardObject, card);
        }

        return cardObject;
    }

    private void LayoutHand(List<GameObject> cardObjects, Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("VRCardDrawer: No spawn point assigned.");
            return;
        }

        int count = cardObjects.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject cardObject = cardObjects[i];

            if (cardObject == null)
            {
                continue;
            }

            float centeredIndex = i - ((count - 1) / 2.0f);

            Vector3 localOffset = new Vector3(
                centeredIndex * cardSpacing,
                i * cardHeightOffset,
                0f
            );

            cardObject.transform.position =
                spawnPoint.position + spawnPoint.TransformDirection(localOffset);

            float fanRotation = centeredIndex * cardFanAngle;

            cardObject.transform.rotation =
                spawnPoint.rotation *
                Quaternion.Euler(cardUprightTiltAngle, fanRotation, 0f);
        }
    }

    private void ApplyFrontCard(GameObject cardObject, Card card)
    {
        CardDisplay display = cardObject.GetComponent<CardDisplay>();

        if (display != null)
        {
            display.SetCard(card.Rank + "\n" + card.Suit);
        }

        CardTextureDisplay textureDisplay = cardObject.GetComponent<CardTextureDisplay>();

        if (textureDisplay != null)
        {
            textureDisplay.SetCard(card);
        }
    }

    private void ApplyBackMaterial(GameObject cardObject)
    {
        if (cardBackMaterial == null)
        {
            Debug.LogWarning("VRCardDrawer: No card back material assigned.");
            return;
        }

        Renderer[] renderers = cardObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.material = cardBackMaterial;
        }
    }

    public void ClearCards()
    {
        for (int i = spawnedCards.Count - 1; i >= 0; i--)
        {
            if (spawnedCards[i] != null)
            {
                Destroy(spawnedCards[i]);
            }
        }

        spawnedCards.Clear();
        playerCardObjects.Clear();
        dealerCardObjects.Clear();
    }
}