namespace BlackJackBattleTest;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Blackjack Battle Test ===");
        Console.WriteLine();

        int playerCount = AskPlayerCount();

        BlackjackBattleGame game = new(playerCount);

        while (!game.PlayersHaveWon() && !game.DealerHasWon())
        {
            game.StartNewRound();

            Console.Clear();
            Console.WriteLine("=== New Round ===");
            Console.WriteLine(game.GetStatusText());
            Console.WriteLine();

            Console.WriteLine($"Dealer shows: {game.Dealer.Hand.Cards[0]}");
            Console.WriteLine();

            PlayPlayersTurn(game);

            Console.WriteLine();
            Console.WriteLine("=== Dealer Turn ===");
            game.PlayDealerTurn();

            Console.WriteLine(game.Dealer);
            Console.WriteLine();

            Console.WriteLine("=== Round Results ===");
            List<string> results = game.ResolveRound();

            foreach (string result in results)
            {
                Console.WriteLine(result);
            }

            Console.WriteLine();
            Console.WriteLine("=== Status ===");
            Console.WriteLine(game.GetStatusText());

            if (!game.PlayersHaveWon() && !game.DealerHasWon())
            {
                Console.WriteLine();
                Console.WriteLine("Press ENTER to start next round...");
                Console.ReadLine();
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== Game Over ===");

        if (game.PlayersHaveWon())
        {
            Console.WriteLine("Players win! Dealer has been defeated.");
        }
        else
        {
            Console.WriteLine("Dealer wins! All players are defeated.");
        }

        Console.WriteLine();
        Console.WriteLine("Press ENTER to exit...");
        Console.ReadLine();
    }

    private static int AskPlayerCount()
    {
        while (true)
        {
            Console.Write("How many players? Enter 1, 2, or 3: ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int playerCount) && playerCount >= 1 && playerCount <= 3)
            {
                return playerCount;
            }

            Console.WriteLine("Invalid input. Please enter 1, 2, or 3.");
            Console.WriteLine();
        }
    }

    private static void PlayPlayersTurn(BlackjackBattleGame game)
    {
        for (int i = 0; i < game.Players.Count; i++)
        {
            BlackjackPlayer player = game.Players[i];

            if (player.IsDefeated)
            {
                Console.WriteLine($"{player.Name} is defeated and skips this round.");
                Console.WriteLine();
                continue;
            }

            Console.WriteLine($"=== {player.Name}'s Turn ===");

            while (player.IsActiveThisRound)
            {
                Console.WriteLine(player);
                Console.WriteLine();

                Console.Write("Choose action: hit or stand: ");
                string? input = Console.ReadLine()?.Trim().ToLower();

                if (input == "hit" || input == "h")
                {
                    Card drawnCard = game.PlayerHit(i);

                    Console.WriteLine($"{player.Name} drew: {drawnCard}");
                    Console.WriteLine($"New value: {player.Hand.Value}");

                    if (player.HasBustedThisRound)
                    {
                        Console.WriteLine($"{player.Name} busted and loses 1 heart.");
                    }

                    Console.WriteLine();
                }
                else if (input == "stand" || input == "s")
                {
                    game.PlayerStand(i);
                    Console.WriteLine($"{player.Name} stands with value {player.Hand.Value}.");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Invalid action. Type hit/h or stand/s.");
                    Console.WriteLine();
                }
            }
        }
    }
}