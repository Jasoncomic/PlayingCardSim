namespace BlackJackBattleTest;

public class Hand
{
    private readonly List<Card> cards = new();

    public IReadOnlyList<Card> Cards => cards;

    public int Value
    {
        get
        {
            int total = cards.Sum(card => card.Value);
            int aceCount = cards.Count(card => card.IsAce);

            while (total > 21 && aceCount > 0)
            {
                total -= 10;
                aceCount--;
            }

            return total;
        }
    }

    public bool IsBust => Value > 21;

    public bool IsBlackjack => cards.Count == 2 && Value == 21;

    public void AddCard(Card card)
    {
        cards.Add(card);
    }

    public void Clear()
    {
        cards.Clear();
    }

    public override string ToString()
    {
        if (cards.Count == 0)
        {
            return "No cards";
        }

        return string.Join(", ", cards);
    }
}