using UnityEngine;
using BlackJackBattleTest;

public class CardTextureDisplay : MonoBehaviour
{
    private Renderer cardRenderer;

    private void Awake()
    {
        cardRenderer = GetComponent<Renderer>();
    }

public void SetCard(Card card)
{
    string path =
        "Textures/" +
        card.Suit +
        "/" +
        GetRankFileName(card.Rank);

    Texture2D texture = Resources.Load<Texture2D>(path);

    if (texture == null)
    {
        Debug.LogWarning("Missing texture: " + path);
        return;
    }

    Material material = cardRenderer.material;

    material.SetTexture("_BaseMap", texture);
    material.SetColor("_BaseColor", Color.white);
}

    private string GetRankFileName(Rank rank)
    {
        if (rank == Rank.Two) return "2";
        if (rank == Rank.Three) return "3";
        if (rank == Rank.Four) return "4";
        if (rank == Rank.Five) return "5";
        if (rank == Rank.Six) return "6";
        if (rank == Rank.Seven) return "7";
        if (rank == Rank.Eight) return "8";
        if (rank == Rank.Nine) return "9";
        if (rank == Rank.Ten) return "10";

        return rank.ToString();
    }
}