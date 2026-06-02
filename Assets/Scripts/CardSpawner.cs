using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public GameObject cardPrefab;

    public Transform playerArea;

    public void SpawnCard()
    {
        Instantiate(
            cardPrefab,
            playerArea.position,
            Quaternion.identity
        );
    }
}