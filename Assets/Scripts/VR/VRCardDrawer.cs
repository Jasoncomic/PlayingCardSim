using System.Collections.Generic;
using UnityEngine;
using BlackJackBattleTest;

public class VRCardDrawer : MonoBehaviour
{

[Header("Card Prefab")]
    public GameObject cardPrefab; // Vorlage für jede Karte, die erzeugt wird

    // =====================================
    // Kartenrückseite
    // =====================================

    [Header("Card Back")]
    public Material cardBackMaterial; // Material für verdeckte Karten

    // =====================================
    // Spawn-Punkte
    // =====================================

    [Header("Spawn Points")]
    public Transform playerSpawn; // Position für Spielerkarten
    public Transform dealerSpawn; // Position für Dealerkarten
    public Transform deckPosition; // Position des Kartendecks

    // =====================================
    // Karten-Layout
    // =====================================

    [Header("Card Layout")]
    public float cardSpacing = 0.32f; // Abstand ZW Karten
    public float cardHeightOffset = 0.004f; // kleiner Höhenversatz, damit Karten nicht flackern

    [Tooltip("Tilts cards upright toward/away from the player. Try -15, 15, -25 or 25 depending on orientation.")]
    public float cardUprightTiltAngle = -15.0f; // Neigung der Karten

    [Tooltip("Optional fan rotation around Y. Set to 0 if you want straight centered rows.")]
    public float cardFanAngle = 0.0f; // Fächerwinkel für Karten

    // =====================================
    // Gespeicherte Kartenobjekte
    // =====================================

    private readonly List<GameObject> spawnedCards = new List<GameObject>(); // alle erzeugten Karten
    private readonly List<GameObject> playerCardObjects = new List<GameObject>(); // Karten des Spielers
    private readonly List<GameObject> dealerCardObjects = new List<GameObject>(); // Karten des Dealers

    // =====================================
    // Spielerkarte erzeugen
    // =====================================

    public GameObject SpawnPlayerCard(Card card, int cardIndex)
    {
        GameObject cardObject = CreateCardObject(card, false);
        playerCardObjects.Add(cardObject);
        LayoutHand(playerCardObjects, playerSpawn);
        return cardObject;
    }

    // =====================================
    // Dealerkarte erzeugen
    // =====================================

    public GameObject SpawnDealerCard(Card card, int cardIndex)
    {
        GameObject cardObject = CreateCardObject(card, false);
        dealerCardObjects.Add(cardObject);
        LayoutHand(dealerCardObjects, dealerSpawn);
        return cardObject;
    }

    // =====================================
    // Verdeckte Dealerkarte erzeugen
    // =====================================

    public GameObject SpawnHiddenDealerCard(Card card, int cardIndex)
    {
        GameObject cardObject = CreateCardObject(card, true);
        dealerCardObjects.Add(cardObject);
        LayoutHand(dealerCardObjects, dealerSpawn);
        return cardObject;
    }

    // =====================================
    // Dealerkarte aufdecken
    // =====================================

    public GameObject RevealDealerCard(GameObject hiddenCardObject, Card card, int cardIndex)
    {
        int index = dealerCardObjects.IndexOf(hiddenCardObject); // merkt sich die Position der verdeckten Karte

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

    // =====================================
    // Kartenobjekt erstellen
    // =====================================

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

  
// =====================================
// Kartenhand ausrichten
// =====================================

private void LayoutHand(List<GameObject> cardObjects, Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("VRCardDrawer: No spawn point assigned.");
            return;
        }

        int count = cardObjects.Count; // speichert, wie viele Karten in dieser Hand liegen

        for (int i = 0; i < count; i++)
        {
            GameObject cardObject = cardObjects[i]; // nimmt aktuelle Karte aus der Liste

            if (cardObject == null)
            {
                continue;
            }

            float centeredIndex = i - ((count - 1) / 2.0f); // zentriert Kartenreihe

            Vector3 localOffset = new Vector3(
                centeredIndex * cardSpacing,
                i * cardHeightOffset,
                0f
            ); // berechnet Abstand und kleinen Höhenversatz der Karte

            cardObject.transform.position =
                spawnPoint.position + spawnPoint.TransformDirection(localOffset); 

            float fanRotation = centeredIndex * cardFanAngle; // berechnet die Fächerrotation der Karte

            cardObject.transform.rotation =
                spawnPoint.rotation *
                Quaternion.Euler(cardUprightTiltAngle, fanRotation, 0f); // richtet Karte mit Neigung und Fächerwinkel aus
        }
    }

    // =====================================
    // Vorderseite der Karte anwenden
    // =====================================

    private void ApplyFrontCard(GameObject cardObject, Card card)
    {
        CardDisplay display = cardObject.GetComponent<CardDisplay>(); // sucht Textanzeige der Karte

        if (display != null)
        {
            display.SetCard(card.Rank + "\n" + card.Suit); // setzt Rang und Farbe als Text auf die Karte
        }

        CardTextureDisplay textureDisplay = cardObject.GetComponent<CardTextureDisplay>();

        if (textureDisplay != null)
        {
            textureDisplay.SetCard(card); // setzt die passende Kartentextur
        }
    }

    // =====================================
    // Rückseite der Karte anwenden
    // =====================================

    private void ApplyBackMaterial(GameObject cardObject)
    {
        if (cardBackMaterial == null)
        {
            Debug.LogWarning("VRCardDrawer: No card back material assigned.");
            return;
        }

        Renderer[] renderers = cardObject.GetComponentsInChildren<Renderer>(); // sucht alle Renderer der Karte

        foreach (Renderer renderer in renderers)
        {
            renderer.material = cardBackMaterial; // setzt bei allen Teilen das Rückseitenmaterial
        }
    }

    // =====================================
    // Alle Karten löschen
    // =====================================

    public void ClearCards()
    {
        for (int i = spawnedCards.Count - 1; i >= 0; i--)
        {
            if (spawnedCards[i] != null)
            {
                Destroy(spawnedCards[i]); // löscht Karte aus der Szene
            }
        }

        spawnedCards.Clear(); // leert Liste aller erzeugten Karten
        playerCardObjects.Clear(); 
        dealerCardObjects.Clear(); 
    }


}
