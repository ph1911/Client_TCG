using System.Collections.Generic;
using UnityEngine;

public class Properties : MonoBehaviour
{
    public string fighterName;
    public int maxHP;
    public int oldHP;
    public int currentHP;
    public int protection;
    public int numberOfCards;
    public string cardToDrawOnFocus;
    public List<string> cardsToDrawPreFight = new List<string>();
    public List<string> cardsToDrawAfterFight = new List<string>();
    public List<CardDatabase.Card> tokensToDraw = new List<CardDatabase.Card>();
    public int[] currentCardDamage;
    public int[] damageModifier;
    public bool[] blockedCards;
    public bool[] nullifiedCards;
    public bool[] cardsToNullify;
    public bool[] cardsToDiscard;
    public CardDatabase.Card[] cardsInPlay = new CardDatabase.Card[5];
    public string action;
    public int parry;
    public int punchParry;
    public int kickParry;
    public bool jinToMutate;
    public bool jinMutated;
}
