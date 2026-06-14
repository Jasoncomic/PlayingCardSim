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
    private int currentPlayerIndex = -1;
    private bool roundActive;
    private bool dealerHasPlayed;

    private readonly List<string> logLines = new List<string>();

    private void Start()
    {
        SetupReferences();

        playerCount = Mathf.Clamp(playerCount, 1, 3);
        game = new BlackjackBattleGame(playerCount);

        AddLog("Game created. Press Start Round.");
        RefreshUI();
    }

    private void SetupReferences()
    {
        // Always refresh CardDrawer reference.
        // This helps if Unity loses the reference after card objects are cleared.
        CardDrawer foundDrawer = FindFirstObjectByType<CardDrawer>();

        if (foundDrawer != null)
        {
            cardDrawer = foundDrawer;
        }
        else
        {
            Debug.LogWarning("No CardDrawer found in the scene.");
        }

        if (playerCardSpawn == null)
        {
            GameObject playerSpawnObject = GameObject.Find("PlayerCardSpawn");

            if (playerSpawnObject != null)
            {
                playerCardSpawn = playerSpawnObject.transform;
            }
            else
            {
                Debug.LogWarning("PlayerCardSpawn was not found.");
            }
        }

        if (dealerCardSpawn == null)
        {
            GameObject dealerSpawnObject = GameObject.Find("DealerCardSpawn");

            if (dealerSpawnObject != null)
            {
                dealerCardSpawn = dealerSpawnObject.transform;
            }
            else
            {
                Debug.LogWarning("DealerCardSpawn was not found.");
            }
        }
    }

    public void StartRound()
    {
        SetupReferences();

        if (game == null)
        {
            game = new BlackjackBattleGame(playerCount);
        }

        if (game.PlayersHaveWon() || game.DealerHasWon())
        {
            AddLog("Game is already over. Press New Game.");
            RefreshUI();
            return;
        }

        game.StartNewRound();

        currentPlayerIndex = GetNextActivePlayerIndex(0);
        roundActive = true;
        dealerHasPlayed = false;

        AddLog("Round " + game.RoundNumber + " started.");

        if (game.Dealer.Hand.Cards.Count > 0)
        {
            AddLog("Dealer shows: " + game.Dealer.Hand.Cards[0]);
        }

        RedrawAllCards();
        RefreshUI();
    }

    public void Hit()
    {
        SetupReferences();

        if (!CanCurrentPlayerAct())
        {
            AddLog("No player can hit right now.");
            RefreshUI();
            return;
        }

        BlackjackPlayer player = game.Players[currentPlayerIndex];
        Card drawnCard = game.PlayerHit(currentPlayerIndex);

        AddLog(player.Name + " drew " + drawnCard + ".");

        // Redraw the full table so the visual state always matches the real game state.
        RedrawAllCards();

        if (player.HasBustedThisRound)
        {
            AddLog(player.Name + " busted and lost 1 heart.");
            MoveToNextPlayer();
        }

        RefreshUI();
    }

    public void Stand()
    {
        SetupReferences();

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

        RedrawAllCards();
        RefreshUI();
    }

    public void DealerTurnAndResolve()
    {
        SetupReferences();

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
            game.PlayDealerTurn();
            dealerHasPlayed = true;

            AddLog("Dealer played. Dealer value: " + game.Dealer.Hand.Value);

            RedrawAllCards();
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

        RedrawAllCards();
        RefreshUI();
    }

    private void RedrawAllCards()
    {
        SetupReferences();

        if (cardDrawer == null)
        {
            AddLog("No CardDrawer assigned. Skipping card visuals.");
            Debug.LogWarning("Cannot draw cards because CardDrawer is missing.");
            return;
        }

        if (game == null)
        {
            return;
        }

        cardDrawer.ClearSpawnedCards();

        if (playerCardSpawn != null && game.Players.Count > 0)
        {
            BlackjackPlayer player = game.Players[0];

            for (int i = 0; i < player.Hand.Cards.Count; i++)
            {
                cardDrawer.SpawnCardVisual(player.Hand.Cards[i], playerCardSpawn, i);
            }
        }
        else
        {
            Debug.LogWarning("Cannot draw player cards because PlayerCardSpawn is missing.");
        }

        if (dealerCardSpawn != null)
        {
            for (int i = 0; i < game.Dealer.Hand.Cards.Count; i++)
            {
                cardDrawer.SpawnCardVisual(game.Dealer.Hand.Cards[i], dealerCardSpawn, i);
            }
        }
        else
        {
            Debug.LogWarning("Cannot draw dealer cards because DealerCardSpawn is missing.");
        }
    }

    private bool CanCurrentPlayerAct()
    {
        if (!roundActive)
        {
            return false;
        }

        if (game == null)
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
        if (game == null)
        {
            return -1;
        }

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
        SetupReferences();

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
        SetupReferences();

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