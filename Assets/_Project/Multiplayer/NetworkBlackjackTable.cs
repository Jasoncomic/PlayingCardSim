using BlackJackBattleTest;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkBlackjackTable : NetworkBehaviour
{
    [Header("Blackjack Visuals")]
    public CardDrawer cardDrawer;

    [Header("Spawn Points")]
    public Transform playerOneSpawn;
    public Transform playerTwoSpawn;
    public Transform playerThreeSpawn;
    public Transform dealerSpawn;

    [Header("UI")]
    public TMP_Text statusText;

    [Header("Game Setup")]
    [Range(1, 3)]
    public int configuredPlayerCount = 3;

    private BlackjackBattleGame game;

    private int playerOneCardIndex;
    private int playerTwoCardIndex;
    private int playerThreeCardIndex;
    private int dealerCardIndex;

    private bool roundStarted;
    private bool roundResolved;

    private bool playerOneCardsShown;
    private bool playerTwoCardsShown;
    private bool playerThreeCardsShown;

    private int currentPlayerIndex;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            configuredPlayerCount = Mathf.Clamp(configuredPlayerCount, 1, 3);

            game = new BlackjackBattleGame(configuredPlayerCount);

            currentPlayerIndex = 0;
            roundStarted = false;
            roundResolved = false;

            SendStatusToAll(
                "Host ready.\n" +
                "Players: " + configuredPlayerCount + "\n\n" +
                "Press Y to start the round."
            );
        }
    }

    public void ConfigurePlayerCount(int playerCount)
    {
        configuredPlayerCount = Mathf.Clamp(playerCount, 1, 3);

        if (game == null)
        {
            return;
        }

        if (!roundStarted)
        {
            game = new BlackjackBattleGame(configuredPlayerCount);
            currentPlayerIndex = 0;
            roundResolved = false;
        }
    }

    public void StartRoundButton()
    {
        StartRoundServerRpc();
    }

    public void HitButton()
    {
        int playerIndex = GetLocalPlayerIndex();
        HitOrRevealServerRpc(playerIndex);
    }

    public void StandButton()
    {
        int playerIndex = GetLocalPlayerIndex();
        StandServerRpc(playerIndex);
    }

    public void DebugShowPlayerCards(int playerIndex)
    {
        HitOrRevealServerRpc(playerIndex);
    }

    public void DebugStandPlayer(int playerIndex)
    {
        StandServerRpc(playerIndex);
    }

    public void DebugHitPlayer(int playerIndex)
    {
        HitOrRevealServerRpc(playerIndex);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void StartRoundServerRpc()
    {
        configuredPlayerCount = Mathf.Clamp(configuredPlayerCount, 1, 3);

        if (game == null || game.Players.Count != configuredPlayerCount)
        {
            game = new BlackjackBattleGame(configuredPlayerCount);
        }

        roundStarted = true;
        roundResolved = false;
        currentPlayerIndex = 0;

        playerOneCardsShown = false;
        playerTwoCardsShown = false;
        playerThreeCardsShown = false;

        playerOneCardIndex = 0;
        playerTwoCardIndex = 0;
        playerThreeCardIndex = 0;
        dealerCardIndex = 0;

        ClearCardsClientRpc();

        game.StartNewRound();

        // Dealer opening: only first dealer card is shown.
        // The hidden dealer card is not spawned until dealer turn.
        SpawnDealerOpeningCard();

        SendStatusToAll(GetRoundStatusBody());
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void HitOrRevealServerRpc(int playerIndex)
    {
        if (game == null || !roundStarted || roundResolved)
        {
            return;
        }

        if (!IsValidActivePlayer(playerIndex))
        {
            SendStatusToAll(GetRoundStatusBody());
            return;
        }

        if (playerIndex != currentPlayerIndex)
        {
            SendStatusToAll(GetRoundStatusBody());
            return;
        }

        if (!HasPlayerCardsShown(playerIndex))
        {
            RevealPlayerCards(playerIndex);
            SendStatusToAll(GetRoundStatusBody());
            return;
        }

        try
        {
            Card card = game.PlayerHit(playerIndex);
            SpawnSinglePlayerCard(playerIndex, card);

            BlackjackPlayer player = game.Players[playerIndex];

            if (player.HasBustedThisRound)
            {
                AdvanceToNextPlayerOrDealer();
            }
            else
            {
                SendStatusToAll(GetRoundStatusBody());
            }
        }
        catch (System.Exception exception)
        {
            SendStatusToAll("Action failed:\n" + exception.Message + "\n\n" + GetRoundStatusBody());
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void StandServerRpc(int playerIndex)
    {
        if (game == null || !roundStarted || roundResolved)
        {
            return;
        }

        if (!IsValidActivePlayer(playerIndex))
        {
            SendStatusToAll(GetRoundStatusBody());
            return;
        }

        if (playerIndex != currentPlayerIndex)
        {
            SendStatusToAll(GetRoundStatusBody());
            return;
        }

        if (!HasPlayerCardsShown(playerIndex))
        {
            SendStatusToAll(
                "Player " + (playerIndex + 1) + " must reveal cards first with A.\n\n" +
                GetRoundStatusBody()
            );

            return;
        }

        game.PlayerStand(playerIndex);

        AdvanceToNextPlayerOrDealer();
    }

    private bool IsValidActivePlayer(int playerIndex)
    {
        if (game == null)
        {
            return false;
        }

        return playerIndex >= 0 && playerIndex < game.Players.Count;
    }

    private void RevealPlayerCards(int playerIndex)
    {
        SpawnHandForPlayer(playerIndex);
        SetPlayerCardsShown(playerIndex, true);
    }

    private void SpawnSinglePlayerCard(int playerIndex, Card card)
    {
        if (playerIndex == 0)
        {
            SpawnCardClientRpc((int)card.Suit, (int)card.Rank, 0, playerOneCardIndex);
            playerOneCardIndex++;
        }
        else if (playerIndex == 1)
        {
            SpawnCardClientRpc((int)card.Suit, (int)card.Rank, 1, playerTwoCardIndex);
            playerTwoCardIndex++;
        }
        else if (playerIndex == 2)
        {
            SpawnCardClientRpc((int)card.Suit, (int)card.Rank, 2, playerThreeCardIndex);
            playerThreeCardIndex++;
        }
    }

    private void AdvanceToNextPlayerOrDealer()
    {
        if (game == null)
        {
            return;
        }

        currentPlayerIndex++;

        if (currentPlayerIndex >= game.Players.Count)
        {
            PlayDealerAndResolveRound();
            return;
        }

        SendStatusToAll(GetRoundStatusBody());
    }

    private void PlayDealerAndResolveRound()
    {
        if (game == null || roundResolved)
        {
            return;
        }

        roundResolved = true;

        game.PlayDealerTurn();

        ClearCardsClientRpc();

        playerOneCardIndex = 0;
        playerTwoCardIndex = 0;
        playerThreeCardIndex = 0;
        dealerCardIndex = 0;

        // Dealer reveal: now all dealer cards are shown.
        SpawnDealerFullHand();

        for (int i = 0; i < game.Players.Count; i++)
        {
            SpawnHandForPlayer(i);
            SetPlayerCardsShown(i, true);
        }

        var results = game.ResolveRound();

        string resultText = GetRoundStatusBody();

        resultText += "\n\nROUND RESULT\n";
        resultText += string.Join("\n", results);

        if (game.PlayersHaveWon())
        {
            resultText += "\n\nPlayers win the battle!";
        }
        else if (game.DealerHasWon())
        {
            resultText += "\n\nDealer wins the battle!";
        }

        resultText += "\n\nPress Y for next round.";

        SendStatusToAll(resultText);
    }

    private bool HasPlayerCardsShown(int playerIndex)
    {
        if (playerIndex == 0)
        {
            return playerOneCardsShown;
        }

        if (playerIndex == 1)
        {
            return playerTwoCardsShown;
        }

        if (playerIndex == 2)
        {
            return playerThreeCardsShown;
        }

        return false;
    }

    private void SetPlayerCardsShown(int playerIndex, bool shown)
    {
        if (playerIndex == 0)
        {
            playerOneCardsShown = shown;
        }
        else if (playerIndex == 1)
        {
            playerTwoCardsShown = shown;
        }
        else if (playerIndex == 2)
        {
            playerThreeCardsShown = shown;
        }
    }

    private void SpawnHandForPlayer(int playerIndex)
    {
        if (game == null)
        {
            return;
        }

        if (!IsValidActivePlayer(playerIndex))
        {
            return;
        }

        BlackjackPlayer player = game.Players[playerIndex];

        for (int i = 0; i < player.Hand.Cards.Count; i++)
        {
            Card card = player.Hand.Cards[i];

            SpawnCardClientRpc(
                (int)card.Suit,
                (int)card.Rank,
                playerIndex,
                i
            );
        }

        if (playerIndex == 0)
        {
            playerOneCardIndex = player.Hand.Cards.Count;
        }
        else if (playerIndex == 1)
        {
            playerTwoCardIndex = player.Hand.Cards.Count;
        }
        else if (playerIndex == 2)
        {
            playerThreeCardIndex = player.Hand.Cards.Count;
        }
    }

    private void SpawnDealerOpeningCard()
    {
        if (game == null)
        {
            return;
        }

        if (game.Dealer.Hand.Cards.Count <= 0)
        {
            return;
        }

        Card firstDealerCard = game.Dealer.Hand.Cards[0];

        SpawnCardClientRpc(
            (int)firstDealerCard.Suit,
            (int)firstDealerCard.Rank,
            3,
            0
        );

        dealerCardIndex = 1;
    }

    private void SpawnDealerFullHand()
    {
        if (game == null)
        {
            return;
        }

        for (int i = 0; i < game.Dealer.Hand.Cards.Count; i++)
        {
            Card card = game.Dealer.Hand.Cards[i];

            SpawnCardClientRpc(
                (int)card.Suit,
                (int)card.Rank,
                3,
                i
            );
        }

        dealerCardIndex = game.Dealer.Hand.Cards.Count;
    }

    private void SendStatusToAll(string bodyText)
    {
        int activePlayers = configuredPlayerCount;

        if (game != null)
        {
            activePlayers = game.Players.Count;
        }

        UpdatePersonalStatusClientRpc(
            bodyText,
            currentPlayerIndex,
            activePlayers,
            roundStarted,
            roundResolved
        );
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdatePersonalStatusClientRpc(
        string bodyText,
        int turnPlayerIndex,
        int activePlayers,
        bool isRoundStarted,
        bool isRoundResolved
    )
    {
        int localPlayerIndex = GetLocalPlayerIndex();

        string header = "";

        if (!isRoundStarted)
        {
            header =
                "LOBBY\n" +
                "You are Player " + (localPlayerIndex + 1) + "\n\n";
        }
        else if (isRoundResolved)
        {
            header =
                "ROUND OVER\n" +
                "You are Player " + (localPlayerIndex + 1) + "\n\n";
        }
        else if (localPlayerIndex >= activePlayers)
        {
            header =
                "SPECTATOR\n" +
                "Player " + (turnPlayerIndex + 1) + " is playing.\n\n";
        }
        else if (localPlayerIndex == turnPlayerIndex)
        {
            header =
                "YOUR TURN\n" +
                "A = Reveal / Hit\n" +
                "B = Stand\n\n";
        }
        else
        {
            header =
                "WAIT\n" +
                "Player " + (turnPlayerIndex + 1) + " is playing.\n" +
                "You are Player " + (localPlayerIndex + 1) + "\n\n";
        }

        string finalText = header + bodyText;

        Debug.Log(finalText);

        if (statusText != null)
        {
            statusText.text = finalText;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnCardClientRpc(int suitValue, int rankValue, int areaIndex, int cardIndex)
    {
        if (cardDrawer == null)
        {
            Debug.LogWarning("NetworkBlackjackTable has no CardDrawer assigned.");
            return;
        }

        Card card = new Card((Suit)suitValue, (Rank)rankValue);

        Transform spawnPoint = GetSpawnPoint(areaIndex);

        if (spawnPoint == null)
        {
            Debug.LogWarning("Missing spawn point for area index " + areaIndex);
            return;
        }

        cardDrawer.SpawnCardVisual(card, spawnPoint, cardIndex);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ClearCardsClientRpc()
    {
        if (cardDrawer != null)
        {
            cardDrawer.ClearSpawnedCards();
        }
    }

    private Transform GetSpawnPoint(int areaIndex)
    {
        if (areaIndex == 0)
        {
            return playerOneSpawn;
        }

        if (areaIndex == 1)
        {
            return playerTwoSpawn;
        }

        if (areaIndex == 2)
        {
            return playerThreeSpawn;
        }

        return dealerSpawn;
    }

    private int GetLocalPlayerIndex()
    {
        if (NetworkManager.Singleton == null)
        {
            return 0;
        }

        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        if (localClientId == 0)
        {
            return 0;
        }

        if (localClientId == 1)
        {
            return 1;
        }

        return 2;
    }

    private string GetRoundStatusBody()
    {
        if (game == null)
        {
            return "Game not initialized.";
        }

        string text = "";

        text += "Round " + game.RoundNumber + "\n";
        text += "Players: " + configuredPlayerCount + "\n";
        text += "Dealer HP: " + game.Dealer.Hp + "/" + game.Dealer.MaxHp + "\n";

        if (roundResolved)
        {
            text += "Dealer value: " + game.Dealer.Hand.Value + "\n";
        }
        else
        {
            text += "Dealer: 1 card visible, 1 card hidden\n";
        }

        text += "\n";

        for (int i = 0; i < game.Players.Count; i++)
        {
            BlackjackPlayer player = game.Players[i];

            text += "Player " + (i + 1);

            if (!roundResolved && i == currentPlayerIndex)
            {
                text += " [TURN]";
            }

            text += "\n";
            text += "Hearts: " + player.Hearts + "\n";

            if (HasPlayerCardsShown(i))
            {
                text += "Hand: " + player.Hand.Value;

                if (player.HasBustedThisRound)
                {
                    text += " | BUST";
                }
                else if (player.HasStood)
                {
                    text += " | STAND";
                }

                text += "\n";
            }
            else
            {
                text += "Cards hidden\n";
            }

            text += "\n";
        }

        return text;
    }
}