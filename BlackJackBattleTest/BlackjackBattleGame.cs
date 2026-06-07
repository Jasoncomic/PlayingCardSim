using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackJackBattleTest
{
    public class BlackjackBattleGame
    {
        private const int StartingPlayerHearts = 3;
        private const int DealerDamagePerWin = 1;

        private readonly Deck deck = new Deck();

        public List<BlackjackPlayer> Players { get; } = new List<BlackjackPlayer>();
        public BlackjackDealer Dealer { get; private set; }

        public int RoundNumber { get; private set; } = 0;

        public BlackjackBattleGame(int playerCount)
        {
            if (playerCount < 1)
            {
                playerCount = 1;
            }

            if (playerCount > 3)
            {
                playerCount = 3;
            }

            for (int i = 1; i <= playerCount; i++)
            {
                Players.Add(new BlackjackPlayer("Player " + i, StartingPlayerHearts));
            }

            Dealer = new BlackjackDealer(GetDealerMaxHp(playerCount));
        }

        public static int GetDealerMaxHp(int playerCount)
        {
            if (playerCount < 1)
            {
                playerCount = 1;
            }

            return 6 + ((playerCount - 1) * 4);
        }

        public void StartNewRound()
        {
            RoundNumber++;

            foreach (BlackjackPlayer player in Players)
            {
                player.ResetForNewRound();
            }

            Dealer.ResetHand();

            if (deck.CardsRemaining < 15)
            {
                deck.Reset();
            }

            foreach (BlackjackPlayer player in Players.Where(player => !player.IsDefeated))
            {
                player.Hand.AddCard(deck.Draw());
                player.Hand.AddCard(deck.Draw());
            }

            Dealer.Hand.AddCard(deck.Draw());
            Dealer.Hand.AddCard(deck.Draw());

            foreach (BlackjackPlayer player in Players.Where(player => !player.IsDefeated))
            {
                if (player.Hand.IsBlackjack)
                {
                    player.Stand();
                }
            }
        }

        public Card PlayerHit(int playerIndex)
        {
            BlackjackPlayer player = Players[playerIndex];

            if (!player.IsActiveThisRound)
            {
                throw new InvalidOperationException(player.Name + " cannot hit right now.");
            }

            Card card = deck.Draw();
            player.Hand.AddCard(card);

            if (player.Hand.IsBust)
            {
                player.MarkBusted();
            }

            return card;
        }

        public void PlayerStand(int playerIndex)
        {
            BlackjackPlayer player = Players[playerIndex];

            if (player.IsDefeated || player.HasBustedThisRound)
            {
                return;
            }

            player.Stand();
        }

        public bool AreAllPlayersDone()
        {
            return Players
                .Where(player => !player.IsDefeated)
                .All(player => player.HasStood || player.HasBustedThisRound);
        }

        public void PlayDealerTurn()
        {
            while (Dealer.Hand.Value < 17)
            {
                Dealer.Hand.AddCard(deck.Draw());
            }
        }

        public List<string> ResolveRound()
        {
            List<string> results = new List<string>();

            bool dealerBust = Dealer.Hand.IsBust;

            foreach (BlackjackPlayer player in Players.Where(player => !player.IsDefeated))
            {
                if (player.HasBustedThisRound)
                {
                    results.Add(player.Name + " busted and loses 1 heart.");
                    continue;
                }

                if (dealerBust)
                {
                    Dealer.TakeDamage(DealerDamagePerWin);
                    results.Add(player.Name + " wins because dealer busted. Dealer loses 1 HP.");
                    continue;
                }

                if (player.Hand.Value > Dealer.Hand.Value)
                {
                    Dealer.TakeDamage(DealerDamagePerWin);
                    results.Add(player.Name + " wins. Dealer loses 1 HP.");
                }
                else if (player.Hand.Value < Dealer.Hand.Value)
                {
                    player.LoseHeart();
                    results.Add(player.Name + " loses and loses 1 heart.");
                }
                else
                {
                    results.Add(player.Name + " ties with dealer. No damage.");
                }
            }

            return results;
        }

        public bool PlayersHaveWon()
        {
            return Dealer.IsDefeated;
        }

        public bool DealerHasWon()
        {
            return Players.All(player => player.IsDefeated);
        }

        public string GetStatusText()
        {
            List<string> lines = new List<string>();

            lines.Add("Round " + RoundNumber);
            lines.Add("Dealer HP: " + Dealer.Hp + "/" + Dealer.MaxHp);
            lines.Add("");

            foreach (BlackjackPlayer player in Players)
            {
                lines.Add(player.Name + " Hearts: " + player.Hearts);
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}