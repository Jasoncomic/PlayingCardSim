namespace BlackJackBattleTest;

public class Deck
{
    private readonly List<Card> cards = new();
    private readonly Random random = new();

    public Deck()
    {
        Reset();
    }

    public void Reset()
    {
        cards.Clear();

        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                cards.Add(new Card(suit, rank));
            }
        }

        Shuffle();
    }

    public void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);

            Card temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    public Card Draw()
    {
        if (cards.Count == 0)
        {
            Reset();
        }

        Card drawnCard = cards[0];
        cards.RemoveAt(0);

        return drawnCard;
    }

    public int CardsRemaining => cards.Count;
}