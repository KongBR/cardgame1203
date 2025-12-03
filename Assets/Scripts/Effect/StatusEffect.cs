using UnityEngine;

public class StatusEffect
{
    public CardAbilityType type;
    public int power;
    public int remainingTurns;

    public StatusEffect(CardAbilityType type, int power, int duration)
    {
        this.type = type;
        this.power = power;
        this.remainingTurns = duration;
    }
}
