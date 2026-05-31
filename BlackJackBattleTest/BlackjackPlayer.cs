namespace BlackJackBattleTest;

public class BlackjackPlayer
{
    public string Name { get; }
    public int Hearts { get; private set; }
    public Hand Hand { get; } = new();

    public bool HasStood { get; private set; }
    public bool HasBustedThisRound { get; private set; }

    public bool IsDefeated => Hearts <= 0;
    public bool IsActiveThisRound => !IsDefeated && !HasStood && !HasBustedThisRound;

    public BlackjackPlayer(string name, int startingHearts)
    {
        Name = name;
        Hearts = startingHearts;
    }

    public void ResetForNewRound()
    {
        Hand.Clear();
        HasStood = false;
        HasBustedThisRound = false;
    }

    public void Stand()
    {
        HasStood = true;
    }

    public void MarkBusted()
    {
        HasBustedThisRound = true;
        LoseHeart();
    }

    public void LoseHeart()
    {
        if (Hearts > 0)
        {
            Hearts--;
        }
    }

    public override string ToString()
    {
        return $"{Name} | Hearts: {Hearts} | Hand: {Hand} | Value: {Hand.Value}";
    }
}