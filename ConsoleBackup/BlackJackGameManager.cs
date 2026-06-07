using UnityEngine;
using TMPro;
using BlackJackBattleTest;

public class BlackjackGameManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text playerHandText;
    public TMP_Text dealerHandText;
    public TMP_Text livesText;
    public TMP_Text resultText;

    private BlackjackBattleGame game;
    private int playerIndex = 0;

    void Start()
    {
        game = new BlackjackBattleGame(1);
        StartRound();
    }

    public void StartRound()
    {
        game.StartNewRound();
        resultText.text = "New round started.";
        UpdateUI();
    }

    public void Hit()
    {
        BlackjackPlayer player = game.Players[playerIndex];

        if (!player.IsActiveThisRound)
        {
            return;
        }

        Card card = game.PlayerHit(playerIndex);

        if (player.HasBustedThisRound)
        {
            resultText.text = "You drew " + card + ". You busted and lost 1 life!";
            EndRound();
        }
        else
        {
            resultText.text = "You drew " + card + ".";
        }

        UpdateUI();
    }

    public void Stand()
    {
        game.PlayerStand(playerIndex);
        resultText.text = "You stood.";
        EndRound();
    }

    private void EndRound()
    {
        game.PlayDealerTurn();
        game.ResolveRound();

        if (game.DealerHasWon())
        {
            resultText.text += "\nGame Over! You lost all lives.";
        }
        else if (game.PlayersHaveWon())
        {
            resultText.text += "\nYou win! Dealer defeated.";
        }
        else
        {
            resultText.text += "\nPress Next Round.";
        }

        UpdateUI();
    }

    public void NextRound()
    {
        if (!game.DealerHasWon() && !game.PlayersHaveWon())
        {
            StartRound();
        }
    }

    private void UpdateUI()
    {
        BlackjackPlayer player = game.Players[playerIndex];

        playerHandText.text =
            "Player Hand: " + player.Hand +
            "\nValue: " + player.Hand.Value;

        dealerHandText.text =
            "Dealer Hand: " + game.Dealer.Hand +
            "\nValue: " + game.Dealer.Hand.Value +
            "\nHP: " + game.Dealer.Hp + "/" + game.Dealer.MaxHp;

        livesText.text = "Lives: " + player.Hearts + "/3";
    }
}