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
    public GameObject newGameButtonRoot;

    [Header("Audio")]
    public AudioSource sfxAudioSource;
    public AudioClip gunshotClip;

    [Range(0f, 1f)]
    public float gunshotVolume = 1f;

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
    private bool gameOver;

    private bool playerOneCardsShown;
    private bool playerTwoCardsShown;
    private bool playerThreeCardsShown;

    private int currentPlayerIndex;

    private float normalStatusFontSize = -1f;
    private TextAlignmentOptions normalTextAlignment = TextAlignmentOptions.TopLeft;
    private bool normalTextStyleCached;

    private void Awake()
    {
        CacheNormalStatusTextStyle();

        if (newGameButtonRoot != null)
        {
            newGameButtonRoot.SetActive(false);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            configuredPlayerCount = Mathf.Clamp(configuredPlayerCount, 1, 3);

            game = new BlackjackBattleGame(configuredPlayerCount);

            currentPlayerIndex = 0;
            roundStarted = false;
            roundResolved = false;
            gameOver = false;

            SetNewGameButtonVisibleClientRpc(false);

            SendStatusToAll(
                "Lobby\n\n" +
                "Players: " + configuredPlayerCount + "\n" +
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

        if (!roundStarted && !gameOver)
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

    public void NewGameButton()
    {
        NewGameServerRpc();
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
        if (gameOver)
        {
            return;
        }

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

        SetNewGameButtonVisibleClientRpc(false);
        ClearCardsClientRpc();

        game.StartNewRound();

        SpawnDealerOpeningCard();

        SendStatusToAll(GetRoundStatusBody());
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void HitOrRevealServerRpc(int playerIndex)
    {
        if (game == null || !roundStarted || roundResolved || gameOver)
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
            SendStatusToAll(
                "Action failed:\n" +
                exception.Message +
                "\n\n" +
                GetRoundStatusBody()
            );
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void StandServerRpc(int playerIndex)
    {
        if (game == null || !roundStarted || roundResolved || gameOver)
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
            SendStatusToAll(GetRoundStatusBody());
            return;
        }

        game.PlayerStand(playerIndex);

        AdvanceToNextPlayerOrDealer();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void NewGameServerRpc()
    {
        configuredPlayerCount = Mathf.Clamp(configuredPlayerCount, 1, 3);

        game = new BlackjackBattleGame(configuredPlayerCount);

        roundStarted = false;
        roundResolved = false;
        gameOver = false;
        currentPlayerIndex = 0;

        playerOneCardsShown = false;
        playerTwoCardsShown = false;
        playerThreeCardsShown = false;

        playerOneCardIndex = 0;
        playerTwoCardIndex = 0;
        playerThreeCardIndex = 0;
        dealerCardIndex = 0;

        ClearCardsClientRpc();
        SetNewGameButtonVisibleClientRpc(false);

        SendStatusToAll(
            "Lobby\n\n" +
            "Players: " + configuredPlayerCount + "\n" +
            "Press Y to start the round."
        );
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
        if (game == null || roundResolved || gameOver)
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

        SpawnDealerFullHand();

        for (int i = 0; i < game.Players.Count; i++)
        {
            SpawnHandForPlayer(i);
            SetPlayerCardsShown(i, true);
        }

        int dealerHpBeforeResolve = game.Dealer.Hp;

        int[] playerHeartsBeforeResolve = new int[game.Players.Count];

        for (int i = 0; i < game.Players.Count; i++)
        {
            playerHeartsBeforeResolve[i] = game.Players[i].Hearts;
        }

        game.ResolveRound();

        if (DidAnyLifeChange(dealerHpBeforeResolve, playerHeartsBeforeResolve))
        {
            PlayGunshotClientRpc();
        }

        if (game.PlayersHaveWon())
        {
            gameOver = true;
            SetNewGameButtonVisibleClientRpc(true);
            SendStatusToAll("Players Win", true);
            return;
        }

        if (game.DealerHasWon())
        {
            gameOver = true;
            SetNewGameButtonVisibleClientRpc(true);
            SendStatusToAll("Dealer Wins", true);
            return;
        }

        SendStatusToAll(GetRoundStatusBody());
    }

    private bool DidAnyLifeChange(int dealerHpBeforeResolve, int[] playerHeartsBeforeResolve)
    {
        if (game == null)
        {
            return false;
        }

        if (game.Dealer.Hp < dealerHpBeforeResolve)
        {
            return true;
        }

        for (int i = 0; i < game.Players.Count; i++)
        {
            if (i >= playerHeartsBeforeResolve.Length)
            {
                continue;
            }

            if (game.Players[i].Hearts < playerHeartsBeforeResolve[i])
            {
                return true;
            }
        }

        return false;
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

    private void SendStatusToAll(string bodyText, bool isGameOver = false)
    {
        string finalText = bodyText;

        if (!isGameOver)
        {
            finalText += GetControlsText();
        }

        UpdateStatusClientRpc(finalText, isGameOver);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateStatusClientRpc(string bodyText, bool isGameOver)
    {
        Debug.Log(bodyText);

        if (statusText == null)
        {
            return;
        }

        CacheNormalStatusTextStyle();

        statusText.text = bodyText;

        if (isGameOver)
        {
            if (normalStatusFontSize > 0f)
            {
                statusText.fontSize = normalStatusFontSize * 2.5f;
            }

            statusText.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            if (normalStatusFontSize > 0f)
            {
                statusText.fontSize = normalStatusFontSize;
            }

            statusText.alignment = normalTextAlignment;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetNewGameButtonVisibleClientRpc(bool visible)
    {
        if (newGameButtonRoot != null)
        {
            newGameButtonRoot.SetActive(visible);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayGunshotClientRpc()
    {
        if (gunshotClip == null)
        {
            Debug.LogWarning("NetworkBlackjackTable: No gunshot clip assigned.");
            return;
        }

        if (sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(gunshotClip, gunshotVolume);
            return;
        }

        AudioSource.PlayClipAtPoint(gunshotClip, transform.position, gunshotVolume);
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

        text += "Round Number: " + game.RoundNumber + "\n\n";

        text += "Dealer HP: " + game.Dealer.Hp + "/" + game.Dealer.MaxHp + "\n";

        if (roundResolved)
        {
            text += "Dealer Points: " + game.Dealer.Hand.Value + "\n";
        }
        else
        {
            text += "Dealer Points: Hidden\n";
        }

        text += "\n\n";

        for (int i = 0; i < game.Players.Count; i++)
        {
            text += GetPlayerStatusText(i);

            if (i < game.Players.Count - 1)
            {
                text += "\n\n";
            }
        }

        return text;
    }

    private string GetPlayerStatusText(int playerIndex)
    {
        BlackjackPlayer player = game.Players[playerIndex];

        string playerState = GetPlayerStateText(playerIndex, player);

        string text = "";

        text += "Player " + (playerIndex + 1) + " (" + playerState + ")\n";
        text += "HP: " + player.Hearts + "\n";

        if (HasPlayerCardsShown(playerIndex) || roundResolved)
        {
            text += "Total Points: " + player.Hand.Value;
        }
        else
        {
            text += "Total Points: Hidden";
        }

        return text;
    }

    private string GetPlayerStateText(int playerIndex, BlackjackPlayer player)
    {
        if (roundResolved)
        {
            if (player.HasBustedThisRound)
            {
                return "Bust";
            }

            if (player.HasStood)
            {
                return "Stand";
            }

            return "Finished";
        }

        if (player.HasBustedThisRound)
        {
            return "Bust";
        }

        if (player.HasStood)
        {
            return "Stand";
        }

        if (playerIndex == currentPlayerIndex)
        {
            return "Your Turn";
        }

        return "Wait for your turn";
    }

    private string GetControlsText()
    {
        return "\n\nY = Start Round / New Round\nA = Hit\nB = Stand";
    }

    private void CacheNormalStatusTextStyle()
    {
        if (normalTextStyleCached)
        {
            return;
        }

        if (statusText == null)
        {
            return;
        }

        normalStatusFontSize = statusText.fontSize;
        normalTextAlignment = statusText.alignment;
        normalTextStyleCached = true;
    }
}