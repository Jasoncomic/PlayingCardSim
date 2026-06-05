using TMPro;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    public TextMeshPro rankText;

    public void SetCard(string rank)
    {
        rankText.text = rank;
    }
}