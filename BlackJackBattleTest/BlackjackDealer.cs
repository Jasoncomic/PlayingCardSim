namespace BlackJackBattleTest;

public class BlackjackDealer
{
    public int MaxHp { get; }
    public int Hp { get; private set; }
    public Hand Hand { get; } = new();

    public bool IsDefeated => Hp <= 0;

    public BlackjackDealer(int maxHp)
    {
        MaxHp = maxHp;
        Hp = maxHp;
    }

    public void ResetHand()
    {
        Hand.Clear();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Hp -= amount;

        if (Hp < 0)
        {
            Hp = 0;
        }
    }

    public override string ToString()
    {
        return $"Dealer | HP: {Hp}/{MaxHp} | Hand: {Hand} | Value: {Hand.Value}";
    }
}