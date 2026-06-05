namespace BlackJackBattleTest
{
    public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public enum Rank
    {
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    public class Card
    {
        public Suit Suit { get; }

        public Rank Rank { get; }

        public int Value
        {
            get
            {
                switch (Rank)
                {
                    case Rank.Ace:
                        return 11;

                    case Rank.Jack:
                    case Rank.Queen:
                    case Rank.King:
                        return 10;

                    case Rank.Two:
                        return 2;

                    case Rank.Three:
                        return 3;

                    case Rank.Four:
                        return 4;

                    case Rank.Five:
                        return 5;

                    case Rank.Six:
                        return 6;

                    case Rank.Seven:
                        return 7;

                    case Rank.Eight:
                        return 8;

                    case Rank.Nine:
                        return 9;

                    case Rank.Ten:
                        return 10;

                    default:
                        return 0;
                }
            }
        }

        public bool IsAce => Rank == Rank.Ace;

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}