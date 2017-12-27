using System;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    public struct Card
    {
        public string id;                   //"#001", "#002"...
        public string name;                 //"Oni Stomp", "Twin Pistons"...
        public int damage;                  //e.g. 50
        public string textEN;               //the card's descriptive text in English
        public string textRU;               //the card's descriptive text in Russian
        public int priority;                //set to 1 for nullification effects
        public string activation;           //"onAppear", "Pre", "After", "Nothing"
        public List<Func<bool>> effect;     //CardEffects.Method(), null
        public string rarity;               //"Bronze", "Silver", "Gold", "SR"
        public string type;                 //"Punch", "Kick"
        public bool limited;
        public bool unique;

        //jin only
        public string mutatedTextEN;        //colored text for mutated status in English
        public string mutatedTextRU;        //colored text for mutated status in Russian

        //only for cards in play
        public bool tenacity;               //cards with tenacity are not discarded
    }

    Card[] cardsInDatabase;

    public Card GetCardById(string id)
    {
        //search card by id
        for (int i = 1; i < cardsInDatabase.Length; i++) {
            if (cardsInDatabase[i].id == id)
                return cardsInDatabase[i];
        }
        //return error
        Card errorcard = new Card();
        errorcard.id = "error";
        return errorcard;
    }

    public void LoadCardDatabase()
    {
        CardEffects ce = GameObject.Find("CardDatabase").GetComponent<CardEffects>();
        cardsInDatabase = new Card[100];

        string grey = "#BBBBBBFF";
        string purple = "#B535FFFF";

        cardsInDatabase[1].id = "#001";
        cardsInDatabase[1].name = "Thrusting Uppercut";
        cardsInDatabase[1].damage = 20;
        cardsInDatabase[1].textEN
            = "Focus:\n+1 K Parry\n<color=" + grey + ">+1 P Parry</color>";
        cardsInDatabase[1].mutatedTextEN
            = "Focus:\n+1 K Parry\n<color=" + purple + ">+1 P Parry</color>";
        cardsInDatabase[1].textRU
            = "RUSSIAN TEXT";
        cardsInDatabase[1].mutatedTextRU
            = "MUTATED RUSSIAN TEXT";
        cardsInDatabase[1].activation = "Pre";
        cardsInDatabase[1].effect = new List<Func<bool>>();
        cardsInDatabase[1].effect.Add(() => ce.JinThrustingUppercut());
        cardsInDatabase[1].rarity = "Gold";
        cardsInDatabase[1].type = "Punch";
        cardsInDatabase[1].unique = true;

        cardsInDatabase[2].id = "#002";
        cardsInDatabase[2].name = "Revolving Hands";
        cardsInDatabase[2].damage = 20;
        cardsInDatabase[2].textEN
            = "Strike:\n+25 Prot\n<color=" + grey + ">Opp Cards null</color>";
        cardsInDatabase[2].mutatedTextEN
            = "Strike:\n+25 Prot\n<color=" + purple + ">Opp Cards null</color>";
        cardsInDatabase[2].priority = 1;
        cardsInDatabase[2].activation = "Pre";
        cardsInDatabase[2].effect = new List<Func<bool>>();
        cardsInDatabase[2].effect.Add(() => ce.JinRevolvingHands());
        cardsInDatabase[2].rarity = "Gold";
        cardsInDatabase[2].type = "Kick";
        cardsInDatabase[2].unique = true;

        cardsInDatabase[3].id = "#003";
        cardsInDatabase[3].name = "Stinger";
        cardsInDatabase[3].damage = 17;
        cardsInDatabase[3].textEN
            = "Both Focus:\nMutation\n+18 Prot";
        cardsInDatabase[3].activation = "After";
        cardsInDatabase[3].effect = new List<Func<bool>>();
        cardsInDatabase[3].effect.Add(() => ce.JinStinger());
        cardsInDatabase[3].rarity = "Gold";
        cardsInDatabase[3].type = "Kick";
        cardsInDatabase[3].limited = true;
        cardsInDatabase[3].unique = true;

        cardsInDatabase[4].id = "#004";
        cardsInDatabase[4].name = "Spinning Side Kick";
        cardsInDatabase[4].damage = 13;
        cardsInDatabase[4].textEN
            = "Diff action:\nCards +5 | <color=" + grey + ">7</color> Dmg\nOpp Cards -5 | <color=" + grey + ">7</color> Dmg";
        cardsInDatabase[4].mutatedTextEN
            = "Diff action:\nCards +5 | <color=" + purple + ">7</color> Dmg\nOpp Cards -5 | <color=" + purple + ">7</color> Dmg";
        cardsInDatabase[4].activation = "Pre";
        cardsInDatabase[4].effect = new List<Func<bool>>();
        cardsInDatabase[4].effect.Add(() => ce.JinSpinningSideKick());
        cardsInDatabase[4].rarity = "Gold";
        cardsInDatabase[4].type = "Kick";
        cardsInDatabase[4].unique = true;

        cardsInDatabase[5].id = "#005";
        cardsInDatabase[5].name = "Power Stance";
        cardsInDatabase[5].damage = 14;
        cardsInDatabase[5].textEN
            = "Opp Block won:\n+1 | <color=" + grey + ">2</color> Cards\n<color=" + grey + ">Mutation</color>";
        cardsInDatabase[5].mutatedTextEN
            = "Opp Block won:\n+1 | <color=" + purple + ">2</color> Cards\n<color=" + purple + ">Mutation</color>";
        cardsInDatabase[5].activation = "After";
        cardsInDatabase[5].effect = new List<Func<bool>>();
        cardsInDatabase[5].effect.Add(() => ce.JinPowerStance());
        cardsInDatabase[5].rarity = "Gold";
        cardsInDatabase[5].type = "Punch";

        cardsInDatabase[6].id = "#006";
        cardsInDatabase[6].name = "Median Line Destruction";
        cardsInDatabase[6].damage = 17;
        cardsInDatabase[6].textEN
            = "Both Strike | <color=" + grey + ">Strike</color>:\n+1 Card\n+1 Token 15P";
        cardsInDatabase[6].mutatedTextEN
            = "Both Strike | <color=" + purple + ">Strike</color>:\n+1 Card\n+1 Token 15P";
        cardsInDatabase[6].activation = "After";
        cardsInDatabase[6].effect = new List<Func<bool>>();
        cardsInDatabase[6].effect.Add(() => ce.JinMedianLineDestruction());
        cardsInDatabase[6].rarity = "Gold";
        cardsInDatabase[6].type = "Punch";
        cardsInDatabase[6].unique = true;

        cardsInDatabase[7].id = "#007";
        cardsInDatabase[7].name = "Right Spinning Axe Kick";
        cardsInDatabase[7].damage = 35;
        cardsInDatabase[7].textEN
            = "Opp Block failed:\n-5 HP | <color=" + grey + ">+1 Card\nMutation</color>";
        cardsInDatabase[7].mutatedTextEN
            = "Opp Block failed:\n-5 HP | <color=" + purple + ">+1 Card\nMutation</color>";
        cardsInDatabase[7].activation = "After";
        cardsInDatabase[7].effect = new List<Func<bool>>();
        cardsInDatabase[7].effect.Add(() => ce.JinRightSpinningAxeKick());
        cardsInDatabase[7].rarity = "Gold";
        cardsInDatabase[7].type = "Kick";

        cardsInDatabase[8].id = "#008";
        cardsInDatabase[8].name = "Left Knee";
        cardsInDatabase[8].damage = 18;
        cardsInDatabase[8].textEN
            = "Strike:\nCard discarded\nKeep 4th | <color=" + grey + ">1st</color> Card";
        cardsInDatabase[8].mutatedTextEN
            = "Strike:\nCard discarded\nKeep 4th | <color=" + purple + ">1st</color> Card";
        cardsInDatabase[8].activation = "After";
        cardsInDatabase[8].effect = new List<Func<bool>>();
        cardsInDatabase[8].effect.Add(() => ce.JinLeftKnee());
        cardsInDatabase[8].rarity = "Gold";
        cardsInDatabase[8].type = "Kick";

        cardsInDatabase[9].id = "#009";
        cardsInDatabase[9].name = "Left Jab to Double Low";
        cardsInDatabase[9].damage = 15;
        cardsInDatabase[9].textEN
            = "Opp Block won\n<color=" + grey + ">Strike</color>:\n+1 Card";
        cardsInDatabase[9].mutatedTextEN
            = "Opp Block won\n<color=" + purple + ">Strike</color>:\n+1 Card";
        cardsInDatabase[9].activation = "After";
        cardsInDatabase[9].effect = new List<Func<bool>>();
        cardsInDatabase[9].effect.Add(() => ce.JinLeftJabToDoubleLow());
        cardsInDatabase[9].rarity = "Gold";
        cardsInDatabase[9].type = "Kick";
        cardsInDatabase[9].unique = true;

        cardsInDatabase[10].id = "#010";
        cardsInDatabase[10].name = "Left Drill Punch";
        cardsInDatabase[10].damage = 17;
        cardsInDatabase[10].textEN
            = "<color=" + grey + ">Any</color> Cards ≤ 1:\nOpp must Focus\n-7 HPmax";
        cardsInDatabase[10].mutatedTextEN
            = "<color=" + purple + ">Any</color> Cards ≤ 1:\nOpp must Focus\n-7 HPmax";
        cardsInDatabase[10].activation = "After";
        cardsInDatabase[10].effect = new List<Func<bool>>();
        //cardsInDatabase[10].effect.Add(() => ce.JinLeftDrillPunch());
        cardsInDatabase[10].rarity = "Gold";
        cardsInDatabase[10].type = "Punch";
        cardsInDatabase[10].unique = true;

        cardsInDatabase[11].id = "#011";
        cardsInDatabase[11].name = "Knee Popper to Sidekick";
        cardsInDatabase[11].damage = 13;
        cardsInDatabase[11].textEN
            = "Strike:\nMutation\n<color=" + grey + ">+2 Cards</color>";
        cardsInDatabase[11].mutatedTextEN
            = "Strike:\nMutation\n<color=" + purple + ">+2 Cards</color>";
        cardsInDatabase[11].activation = "After";
        cardsInDatabase[11].effect = new List<Func<bool>>();
        cardsInDatabase[11].effect.Add(() => ce.JinKneePopperToSidekick());
        cardsInDatabase[11].rarity = "Gold";
        cardsInDatabase[11].type = "Kick";
        cardsInDatabase[11].limited = true;
        cardsInDatabase[11].unique = true;

        cardsInDatabase[12].id = "#012";
        cardsInDatabase[12].name = "Double Thrust Roundhouse";
        cardsInDatabase[12].damage = 17;
        cardsInDatabase[12].textEN
            = "Strike\nOpp Cards ≥ 3 | <color=" + grey + ">2</color>:\n+1 Card";
        cardsInDatabase[12].mutatedTextEN
            = "Strike\nOpp Cards ≥ 3 | <color=" + purple + ">2</color>:\n+1 Card";
        cardsInDatabase[12].activation = "After";
        cardsInDatabase[12].effect = new List<Func<bool>>();
        cardsInDatabase[12].effect.Add(() => ce.JinDoubleThrustRoundhouse());
        cardsInDatabase[12].rarity = "Gold";
        cardsInDatabase[12].type = "Kick";
        cardsInDatabase[12].unique = true;

        cardsInDatabase[13].id = "#013";
        cardsInDatabase[13].name = "Double Lift Kick";
        cardsInDatabase[13].damage = 18;
        cardsInDatabase[13].textEN
            = "Diff action:\n+8 Protection\nMutation";
        cardsInDatabase[13].activation = "After";
        cardsInDatabase[13].effect = new List<Func<bool>>();
        cardsInDatabase[13].effect.Add(() => ce.JinDoubleLiftKick());
        cardsInDatabase[13].rarity = "Gold";
        cardsInDatabase[13].type = "Kick";

        cardsInDatabase[14].id = "#014";
        cardsInDatabase[14].name = "Double Chamber Punch";
        cardsInDatabase[14].damage = 25;
        cardsInDatabase[14].textEN
            = "Strike:\nCards +7 | <color=" + grey + ">14</color> Damage";
        cardsInDatabase[14].mutatedTextEN
            = "Strike:\nCards +7 | <color=" + purple + ">14</color> Damage";
        cardsInDatabase[14].activation = "Pre";
        cardsInDatabase[14].effect = new List<Func<bool>>();
        cardsInDatabase[14].effect.Add(() => ce.JinDoubleChamberPunch());
        cardsInDatabase[14].rarity = "SR";
        cardsInDatabase[14].type = "Punch";

        cardsInDatabase[15].id = "#015";
        cardsInDatabase[15].name = "Corpse Thrust";
        cardsInDatabase[15].damage = 12;
        cardsInDatabase[15].textEN
            = "Strike:\n+1 Card | <color=" + grey + ">Keep Card</color>\n-6 HPmax";
        cardsInDatabase[15].mutatedTextEN
            = "Strike:\n+1 Card | <color=" + purple + ">Keep Card</color>\n-6 HPmax";
        cardsInDatabase[15].activation = "After";
        cardsInDatabase[15].effect = new List<Func<bool>>();
        //cardsInDatabase[15].effect.Add(() => ce.JinCorpseThrust());
        cardsInDatabase[15].rarity = "Gold";
        cardsInDatabase[15].type = "Punch";
        cardsInDatabase[15].limited = true;
        cardsInDatabase[15].unique = true;
    }
}
