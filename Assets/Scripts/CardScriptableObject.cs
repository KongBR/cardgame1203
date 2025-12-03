using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card", order = 1)]
public class CardScriptableObject : ScriptableObject
{
    public string cardName;

    [TextArea]
    public string actionDescription, cardLore;

    public int currentHealth, attackPower, manaCost;

    public Sprite characterSprite, bgSprite;

    // 이 카드가 가진 능력들 (0개일 수도 있음)
    public List<CardAbility> abilities = new List<CardAbility>();
}
