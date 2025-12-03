using UnityEngine;

public enum AbilityType
{
    None,
    Burn,
    Freeze,
    Bleed,
    Taunt
}

[CreateAssetMenu(fileName = "NewCardAbility", menuName = "Card/Ability")]
public class CardAbilitySO : ScriptableObject
{
    public AbilityType abilityType;

    [Header("Effect Values")]
    public int value = 1;

    [Header("UI Icon")]
    public Sprite icon;
}
