using System.Collections.Generic;
using System.Text;
using BlackJackBattleTest;
using TMPro;
using UnityEngine;

public class BlackjackUnityTestController : MonoBehaviour
{
    [Header("UI Text")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text logText;

    [Header("Card Visuals")]
    [SerializeField] private CardDrawer cardDrawer;
    [SerializeField] private Transform playerCardSpawn;
    [SerializeField] private Transform dealerCardSpawn;

    [Header("Settings")]
    [SerializeField] private int playerCount = 1;

    private BlackjackBattleGame game;
    private int currentPlayerIndex;
    private bool roundActive;
    private bool dealerHasPlayed;

    private readonly List<string> logLines = new List<string>();

    private void Start()
    {
        playerCount = Mathf.Clamp(playerCount, 1, 3);
        game = new BlackjackBattleGame(playerCount);

        AddLog("Game created. Press Start Round.");
        RefreshUI();
    }

    public void StartRound()
    {
        if (game.PlayersHaveWon() || game.DealerHasWon())
        {
            AddLog("Game is already over. Restart Play Mode to test again.");
            RefreshUI();
            return;
        }

        game.StartNewRound();

        currentPlayerIndex = GetNextActivePlayerIndex(0);
        roundActive = true;
        dealerHasPlayed = false;

        AddLog("Round " + game.RoundNumber + " started.");
        AddLog("Dealer shows: " + game.Dealer.Hand.Cards[0]);

        SpawnRoundCards();

        RefreshUI();
    }

    public void Hit()
    {
        if (!CanCurrentPlayerAct())
        {
            AddLog("No player can hit right now.");
            RefreshUI();
            return;
        }

        BlackjackPlayer player = game.Players[currentPlayerIndex];
        Card drawnCard = game.PlayerHit(currentPlayerIndex);

        AddLog(player.Name + " drew " + drawnCard + ".");

        SpawnPlayerCard(drawnCard, player.Hand.Cards.Count - 1);

        if (player.HasBustedThisRound)
        {
            AddLog(player.Name + " busted and lost 1 heart.");
            MoveToNextPlayer();
        }

        RefreshUI();
    }

    public void Stand()
    {
        if (!CanCurrentPlayerAct())
        {
            AddLog("No player can stand right now.");
            RefreshUI();
            return;
        }

        BlackjackPlayer player = game.Players[currentPlayerIndex];
        game.PlayerStand(currentPlayerIndex);

        AddLog(player.Name + " stands with " + player.Hand.Value + ".");

        MoveToNextPlayer();
        RefreshUI();
    }

    public void DealerTurnAndResolve()
    {
        if (!roundActive)
        {
            AddLog("Start a round first.");
            RefreshUI();
            return;
        }

        if (!game.AreAllPlayersDone())
        {
            AddLog("Players are not done yet.");
            RefreshUI();
            return;
        }

        if (!dealerHasPlayed)
        {
            int oldDealerCardCount = game.Dealer.Hand.Cards.Count;

            game.PlayDealerTurn();
            dealerHasPlayed = true;

            AddLog("Dealer played. Dealer value: " + game.Dealer.Hand.Value);

            SpawnNewDealerCards(oldDealerCardCount);
        }

        List<string> results = game.ResolveRound();

        foreach (string result in results)
        {
            AddLog(result);
        }

        roundActive = false;

        if (game.PlayersHaveWon())
        {
            AddLog("GAME OVER: Players win!");
        }
        else if (game.DealerHasWon())
        {
            AddLog("GAME OVER: Dealer wins!");
        }
        else
        {
            AddLog("Round finished. Press Start Round again.");
        }

        RefreshUI();
    }

    private void SpawnRoundCards()
    {
        if (cardDrawer == null)
        {
            AddLog("No CardDrawer assigned. Skipping card visuals.");
            return;
        }

        cardDrawer.ClearSpawnedCards();

        if (playerCardSpawn == null)
        {
            AddLog("No PlayerCardSpawn assigned.");
        }

        if (dealerCardSpawn == null)
        {
            AddLog("No DealerCardSpawn assigned.");
        }

        // For now we only show Player 1 visually.
        // Later, for multiplayer, we can add spawn points for Player 2 and Player 3.
        BlackjackPlayer player = game.Players[0];

        for (int i = 0; i < player.Hand.Cards.Count; i++)
        {
            SpawnPlayerCard(player.Hand.Cards[i], i);
        }

        for (int i = 0; i < game.Dealer.Hand.Cards.Count; i++)
        {
            SpawnDealerCard(game.Dealer.Hand.Cards[i], i);
        }
    }

    private void SpawnPlayerCard(Card card, int cardIndex)
    {
        if (cardDrawer == null || playerCardSpawn == null)
        {
            return;
        }

        cardDrawer.SpawnCardVisual(card, playerCardSpawn, cardIndex);
    }

    private void SpawnDealerCard(Card card, int cardIndex)
    {
        if (cardDrawer == null || dealerCardSpawn == null)
        {
            return;
        }

        cardDrawer.SpawnCardVisual(card, dealerCardSpawn, cardIndex);
    }

    private void SpawnNewDealerCards(int oldDealerCardCount)
    {
        for (int i = oldDealerCardCount; i < game.Dealer.Hand.Cards.Count; i++)
        {
            SpawnDealerCard(game.Dealer.Hand.Cards[i], i);
        }
    }

    private bool CanCurrentPlayerAct()
    {
        if (!roundActive)
        {
            return false;
        }

        if (currentPlayerIndex < 0 || currentPlayerIndex >= game.Players.Count)
        {
            return false;
        }

        return game.Players[currentPlayerIndex].IsActiveThisRound;
    }

    private void MoveToNextPlayer()
    {
        int nextIndex = GetNextActivePlayerIndex(currentPlayerIndex + 1);

        if (nextIndex == -1)
        {
            AddLog("All players are done. Press Dealer Turn / Resolve.");
            currentPlayerIndex = -1;
            return;
        }

        currentPlayerIndex = nextIndex;
        AddLog("Now it is " + game.Players[currentPlayerIndex].Name + "'s turn.");
    }

    private int GetNextActivePlayerIndex(int startIndex)
    {
        for (int i = startIndex; i < game.Players.Count; i++)
        {
            if (game.Players[i].IsActiveThisRound)
            {
                return i;
            }
        }

        return -1;
    }

    private void AddLog(string message)
    {
        logLines.Add(message);

        if (logLines.Count > 12)
        {
            logLines.RemoveAt(0);
        }
    }

    private void RefreshUI()
    {
        if (statusText != null)
        {
            statusText.text = BuildStatusText();
        }

        if (logText != null)
        {
            logText.text = string.Join("\n", logLines);
        }
    }

    private string BuildStatusText()
    {
        if (game == null)
        {
            return "No game created.";
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Round: " + game.RoundNumber);
        builder.AppendLine("Dealer HP: " + game.Dealer.Hp + "/" + game.Dealer.MaxHp);
        builder.AppendLine("Dealer Hand: " + game.Dealer.Hand + " | Value: " + game.Dealer.Hand.Value);
        builder.AppendLine();

        for (int i = 0; i < game.Players.Count; i++)
        {
            BlackjackPlayer player = game.Players[i];
            string turnMarker = i == currentPlayerIndex && roundActive ? " <-- TURN" : "";

            builder.AppendLine(player.Name + turnMarker);
            builder.AppendLine("Hearts: " + player.Hearts);
            builder.AppendLine("Hand: " + player.Hand);
            builder.AppendLine("Value: " + player.Hand.Value);
            builder.AppendLine("Stood: " + player.HasStood + " | Busted: " + player.HasBustedThisRound);
            builder.AppendLine();
        }

        return builder.ToString();
    }

    public void RestartGame()
    {
        playerCount = Mathf.Clamp(playerCount, 1, 3);

        game = new BlackjackBattleGame(playerCount);
        currentPlayerIndex = -1;
        roundActive = false;
        dealerHasPlayed = false;

        logLines.Clear();

        if (cardDrawer != null)
        {
            cardDrawer.ClearSpawnedCards();
        }

        AddLog("New game created. Press Start Round.");
        RefreshUI();
    }

    public void StopCurrentGame()
    {
        currentPlayerIndex = -1;
        roundActive = false;
        dealerHasPlayed = false;

        if (cardDrawer != null)
        {
            cardDrawer.ClearSpawnedCards();
        }

        game = new BlackjackBattleGame(playerCount);

        logLines.Clear();
        AddLog("Game stopped.");
        RefreshUI();
    }
}