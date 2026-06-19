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

    private BlackjackBattleGame game;

    private int playerOneCardIndex;
    private int playerTwoCardIndex;
    private int playerThreeCardIndex;
    private int dealerCardIndex;

    private bool roundStarted;
    private bool playerOneCardsShown;
    private bool playerTwoCardsShown;
    private bool playerThreeCardsShown;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            game = new BlackjackBattleGame(3);
            UpdateStatusClientRpc("Host ready. Press R to start the round.");
        }
    }

    public void StartRoundButton()
    {
        StartRoundServerRpc();
    }

    public void HitButton()
    {
        int playerIndex = GetLocalPlayerIndex();

        if (!HasPlayerCardsShown(playerIndex))
        {
            ShowPlayerCardsServerRpc(playerIndex);
            return;
        }

        HitServerRpc(playerIndex);
    }

    public void StandButton()
    {
        int playerIndex = GetLocalPlayerIndex();
        StandServerRpc(playerIndex);
    }

    public void DebugShowPlayerCards(int playerIndex)
    {
        ShowPlayerCardsServerRpc(playerIndex);
    }

    public void DebugStandPlayer(int playerIndex)
    {
        StandServerRpc(playerIndex);
    }

    public void DebugHitPlayer(int playerIndex)
    {
        HitServerRpc(playerIndex);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void StartRoundServerRpc()
    {
        if (game == null)
        {
            game = new BlackjackBattleGame(3);
        }

        roundStarted = true;

        playerOneCardsShown = false;
        playerTwoCardsShown = false;
        playerThreeCardsShown = false;

        playerOneCardIndex = 0;
        playerTwoCardIndex = 0;
        playerThreeCardIndex = 0;
        dealerCardIndex = 0;

        ClearCardsClientRpc();

        game.StartNewRound();

        SpawnDealerHand();

        UpdateStatusClientRpc(
            "Round " + game.RoundNumber +
            "\nDealer HP: " + game.Dealer.Hp + "/" + game.Dealer.MaxHp +
            "\nDealer value: " + game.Dealer.Hand.Value +
            "\n\nDealer has drawn first." +
            "\nPress 1 to reveal Player 1 cards." +
            "\nPress 3 to reveal Player 2 cards." +
            "\nPress 5 to reveal Player 3 cards."
        );
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ShowPlayerCardsServerRpc(int playerIndex)
    {
        if (game == null || !roundStarted)
        {
            return;
        }

        if (playerIndex < 0 || playerIndex >= game.Players.Count)
        {
            return;
        }

        if (playerIndex == 0 && playerOneCardsShown)
        {
            return;
        }

        if (playerIndex == 1 && playerTwoCardsShown)
        {
            return;
        }

        if (playerIndex == 2 && playerThreeCardsShown)
        {
            return;
        }

        SpawnHandForPlayer(playerIndex);

        if (playerIndex == 0)
        {
            playerOneCardsShown = true;
        }
        else if (playerIndex == 1)
        {
            playerTwoCardsShown = true;
        }
        else
        {
            playerThreeCardsShown = true;
        }

        UpdateStatusClientRpc(GetFullRoundStatusText());
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void HitServerRpc(int playerIndex)
    {
        if (game == null || !roundStarted)
        {
            return;
        }

        if (playerIndex < 0 || playerIndex >= game.Players.Count)
        {
            return;
        }

        if (!HasPlayerCardsShown(playerIndex))
        {
            UpdateStatusClientRpc("Player " + (playerIndex + 1) + " cards are not revealed yet.");
            return;
        }

        try
        {
            Card card = game.PlayerHit(playerIndex);

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
            else
            {
                SpawnCardClientRpc((int)card.Suit, (int)card.Rank, 2, playerThreeCardIndex);
                playerThreeCardIndex++;
            }

            UpdateStatusClientRpc(GetFullRoundStatusText());

            TryDealerTurnAndResolve();
        }
        catch (System.Exception exception)
        {
            UpdateStatusClientRpc(exception.Message);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void StandServerRpc(int playerIndex)
    {
        if (game == null || !roundStarted)
        {
            return;
        }

        if (playerIndex < 0 || playerIndex >= game.Players.Count)
        {
            return;
        }

        if (!HasPlayerCardsShown(playerIndex))
        {
            UpdateStatusClientRpc("Player " + (playerIndex + 1) + " cards are not revealed yet.");
            return;
        }

        game.PlayerStand(playerIndex);

        UpdateStatusClientRpc(GetFullRoundStatusText());

        TryDealerTurnAndResolve();
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

        return playerThreeCardsShown;
    }

    private void TryDealerTurnAndResolve()
    {
        if (!playerOneCardsShown || !playerTwoCardsShown || !playerThreeCardsShown)
        {
            return;
        }

        if (!game.AreAllPlayersDone())
        {
            return;
        }

        game.PlayDealerTurn();

        ClearCardsClientRpc();

        playerOneCardIndex = 0;
        playerTwoCardIndex = 0;
        playerThreeCardIndex = 0;
        dealerCardIndex = 0;

        SpawnDealerHand();
        SpawnHandForPlayer(0);
        SpawnHandForPlayer(1);
        SpawnHandForPlayer(2);

        var results = game.ResolveRound();

        string resultText = GetFullRoundStatusText();
        resultText += "\n\nRound Result:\n";
        resultText += string.Join("\n", results);

        if (game.PlayersHaveWon())
        {
            resultText += "\n\nPlayers win the battle!";
        }
        else if (game.DealerHasWon())
        {
            resultText += "\n\nDealer wins the battle!";
        }

        resultText += "\n\nPress R for next round.";

        UpdateStatusClientRpc(resultText);
    }

    private void SpawnHandForPlayer(int playerIndex)
    {
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
        else
        {
            playerThreeCardIndex = player.Hand.Cards.Count;
        }
    }

    private void SpawnDealerHand()
    {
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

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateStatusClientRpc(string message)
    {
        Debug.Log(message);

        if (statusText != null)
        {
            statusText.text = message;
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

    private string GetFullRoundStatusText()
    {
        string text = "";
        text += "Round " + game.RoundNumber + "\n";
        text += "Dealer HP: " + game.Dealer.Hp + "/" + game.Dealer.MaxHp + "\n";
        text += "Dealer value: " + game.Dealer.Hand.Value + "\n\n";

        for (int i = 0; i < game.Players.Count; i++)
        {
            BlackjackPlayer player = game.Players[i];

            text += player.Name +
                    " Hearts: " + player.Hearts;

            if (HasPlayerCardsShown(i))
            {
                text += " | Hand: " + player.Hand.Value;

                if (player.HasBustedThisRound)
                {
                    text += " | BUST";
                }
                else if (player.HasStood)
                {
                    text += " | STAND";
                }
            }
            else
            {
                text += " | Cards hidden";
            }

            text += "\n";
        }

        return text;
    }
}