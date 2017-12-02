using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class CardEffects : MonoBehaviour
{
    public Transform player, opponent;
    public int currentCardPosition;

    private string language;

    private void Start()
    {
        language = GameObject.Find("Client").GetComponent<Client>().language;
    }

    public void DrawCardById(Transform player, string id)
    {
        if (player.GetComponent<Properties>().numberOfCards > 4 || id == "Nothing") {
            return;
        }
        CardDatabase.Card card = GameObject.Find("CardDatabase").GetComponent<CardDatabase>().GetCardById(id);
        Transform cardslot = player.Find("Cardslot" + player.GetComponent<Properties>().numberOfCards.ToString());
        //if less than 5 cards, actually draw the new card
        if (player.GetComponent<Properties>().numberOfCards < 5) {
            cardslot.Find("Name").GetComponent<Text>().text = card.name;
            cardslot.Find("Damage").GetComponent<Text>().text = card.damage.ToString();
            cardslot.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" + card.name + "/Image");
            cardslot.Find("Frame").GetComponent<Image>().sprite = Resources.Load<Sprite>(card.rarity + "Frame");
            cardslot.Find("Type").GetComponent<Image>().sprite = Resources.Load<Sprite>(card.type);
            cardslot.Find("Activation").GetComponent<Image>().sprite = Resources.Load<Sprite>(card.activation);
            if (card.limited) {
                cardslot.Find("Limited").GetComponent<Image>().sprite = Resources.Load<Sprite>("LimitedSymbol");
            }
            else {
                cardslot.Find("Limited").GetComponent<Image>().sprite = Resources.Load<Sprite>("Nothing");
            }
            if (card.unique) {
                cardslot.Find("Unique").GetComponent<Image>().sprite = Resources.Load<Sprite>("UniqueSymbol");
            }
            else if (card.rarity == "SR") {
                cardslot.Find("Unique").GetComponent<Image>().sprite = Resources.Load<Sprite>("SRSymbol");
            }
            else {
                cardslot.Find("Limited").GetComponent<Image>().sprite = Resources.Load<Sprite>("Nothing");
            }
            if (player.GetComponent<Properties>().jinMutated && card.mutatedTextEN != null) {
                switch (language) {
                    case "EN":
                        cardslot.Find("Text").GetComponent<Text>().text = card.mutatedTextEN;
                        break;
                }
            }
            else {
                switch (language) {
                    case "EN":
                        cardslot.Find("Text").GetComponent<Text>().text = card.textEN;
                        break;
                }
            }
            if (GameObject.Find("BattleScreen").GetComponent<BattleScreen>().view == "Name") {
                cardslot.Find("Name").GetComponent<Text>().enabled = true;
                cardslot.Find("Text").GetComponent<Text>().enabled = false;
            }
            else {
                cardslot.Find("Name").GetComponent<Text>().enabled = false;
                cardslot.Find("Text").GetComponent<Text>().enabled = true;
            }

            player.GetComponent<Properties>().currentCardDamage[player.GetComponent<Properties>().numberOfCards]
                = card.damage;
            player.GetComponent<Properties>().cardsInPlay[player.GetComponent<Properties>().numberOfCards] = card;
            player.GetComponent<Properties>().numberOfCards++;

            if (card.activation == "onAppear")
                foreach (Func<bool> effect in card.effect)
                    effect();

            cardslot.GetComponent<Animator>().Play("FadeIn");
        }
    }
    public void DrawToken(Transform player, int damage, string type)
    {
        CardDatabase.Card token = new CardDatabase.Card();
        token.id = "Token";
        token.name = "Token";
        token.damage = damage;
        token.type = type;
        player.GetComponent<Properties>().tokensToDraw.Add(token);
    }
    public void PutTokensInPlay(Transform player)
    {
        foreach (CardDatabase.Card token in player.GetComponent<Properties>().tokensToDraw) {
            Transform cardslot = player.Find("Cardslot" + player.GetComponent<Properties>().numberOfCards.ToString());
            if (player.GetComponent<Properties>().numberOfCards < 5) {
                cardslot.Find("Name").GetComponent<Text>().text = token.name;
                cardslot.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/Jin Default");
                cardslot.Find("Frame").GetComponent<Image>().sprite = Resources.Load<Sprite>("GoldFrame");
                cardslot.Find("Type").GetComponent<Image>().sprite = Resources.Load<Sprite>(token.type);
                cardslot.Find("Damage").GetComponent<Text>().text = token.damage.ToString();
                cardslot.Find("Limited").GetComponent<Image>().sprite = Resources.Load<Sprite>("Nothing");
                cardslot.Find("Unique").GetComponent<Image>().sprite = Resources.Load<Sprite>("Nothing");
                cardslot.Find("Activation").GetComponent<Image>().sprite = Resources.Load<Sprite>("Nothing");
                cardslot.Find("Text").GetComponent<Text>().text = "";

                player.GetComponent<Properties>().currentCardDamage[player.GetComponent<Properties>().numberOfCards]
                    = token.damage;
                player.GetComponent<Properties>().cardsInPlay[player.GetComponent<Properties>().numberOfCards] = token;
                player.GetComponent<Properties>().numberOfCards++;

                cardslot.GetComponent<Animator>().Play("FadeIn");
            }
        }
    }

    public void Discard(Transform player, int from, [Optional]int to)
    {
        for (int i = from; i <= to; i++) {
            player.GetComponent<Properties>().cardsToDiscard[i] = true;
        }
    }
    public void FocusBreak(Transform player)
    {
        player.GetComponent<Properties>().cardsToDiscard[0] = true;
        StartDiscarding(player);
    }
    public bool StartDiscarding(Transform player)
    {
        if (player.GetComponent<Properties>().numberOfCards == 0
                || !player.GetComponent<Properties>().cardsToDiscard.Contains(true))
            return false;
        //discard the given cards
        int cardsDiscarded = 0;
        for (int i = 0; i < player.GetComponent<Properties>().numberOfCards; i++) {
            if (player.GetComponent<Properties>().cardsToDiscard[i]
                    && !player.GetComponent<Properties>().cardsInPlay[i].tenacity) {
                Transform cardslot = player.Find("Cardslot" + i.ToString());
                cardslot.GetComponent<Animator>().Play("FadeOut");
                player.GetComponent<Properties>().currentCardDamage[i] = 0;
                player.GetComponent<Properties>().cardsInPlay[i] = default(CardDatabase.Card);
                cardsDiscarded++;
            }
            else if (player.GetComponent<Properties>().cardsToDiscard[i]
                    && player.GetComponent<Properties>().cardsInPlay[i].tenacity) {
                player.GetComponent<Properties>().cardsInPlay[i].tenacity = false;
            }
        }
        //move until blocked - thx to toni
        if (player.GetComponent<Properties>().numberOfCards > cardsDiscarded) {
            for (int n = 0; n < 4; n++) {
                for (int i = player.GetComponent<Properties>().numberOfCards - 1; i > 0; i--) {
                    if (player.GetComponent<Properties>().cardsInPlay[i - 1].Equals(default(CardDatabase.Card))) {
                        MoveCard(player, i, i - 1);
                        Transform cardslot = player.Find("Cardslot" + (i - 1).ToString());
                    }
                }
            }
            for (int i = 0; i < player.GetComponent<Properties>().numberOfCards; i++) {
                Transform cardslot = player.Find("Cardslot" + i.ToString());
                if (player.GetComponent<Properties>().cardsInPlay[i].Equals(default(CardDatabase.Card))) {
                    cardslot.GetComponent<Animator>().Play("FadeOut");
                }
                else {
                    cardslot.GetComponent<Animator>().Play("FadeIn");
                }
            }
        }
        player.GetComponent<Properties>().numberOfCards -= cardsDiscarded;
        player.GetComponent<Properties>().cardsToDiscard = new bool[5];

        return true;
    }

    private void MoveCard(Transform player, int from, int to)
    {
        player.GetComponent<Properties>().currentCardDamage[to]
            = player.GetComponent<Properties>().currentCardDamage[from];
        player.GetComponent<Properties>().cardsInPlay[to]
            = player.GetComponent<Properties>().cardsInPlay[from];
        player.GetComponent<Properties>().currentCardDamage[from] = 0;
        player.GetComponent<Properties>().cardsInPlay[from] = default(CardDatabase.Card);

        Transform moveHere = player.Find("Cardslot" + to.ToString());
        Transform fromHere = player.Find("Cardslot" + from.ToString());
        moveHere.Find("Image").GetComponent<Image>().sprite = fromHere.Find("Image").GetComponent<Image>().sprite;
        moveHere.Find("Frame").GetComponent<Image>().sprite = fromHere.Find("Frame").GetComponent<Image>().sprite;
        moveHere.Find("Type").GetComponent<Image>().sprite = fromHere.Find("Type").GetComponent<Image>().sprite;
        moveHere.Find("Damage").GetComponent<Text>().text = fromHere.Find("Damage").GetComponent<Text>().text;
        moveHere.Find("Limited").GetComponent<Image>().sprite = fromHere.Find("Limited").GetComponent<Image>().sprite;
        moveHere.Find("Unique").GetComponent<Image>().sprite = fromHere.Find("Unique").GetComponent<Image>().sprite;
        moveHere.Find("Activation").GetComponent<Image>().sprite = fromHere.Find("Activation").GetComponent<Image>().sprite;
        moveHere.Find("Text").GetComponent<Text>().text = fromHere.Find("Text").GetComponent<Text>().text;
        moveHere.Find("Name").GetComponent<Text>().text = fromHere.Find("Name").GetComponent<Text>().text;
    }

    public void DamagePlayer(Transform player, int damage)
    {
        int originalDamage = damage;
        int newDamage = originalDamage - player.GetComponent<Properties>().protection;
        //if protection not broken, remove protection equivalent to the original damage
        if (newDamage <= 0) {
            newDamage = 0;
            AddProtection(player, -originalDamage);
        }
        //else if protection broken, put it to 0
        else {
            RemoveProtection(player);
        }

        player.GetComponent<Properties>().currentHP -= newDamage;
        //no hp over maxHP
        if (player.GetComponent<Properties>().currentHP > player.GetComponent<Properties>().maxHP)
            player.GetComponent<Properties>().currentHP = player.GetComponent<Properties>().maxHP;
        //no hp under 0
        if (player.GetComponent<Properties>().currentHP < 0) {
            player.GetComponent<Properties>().currentHP = 0;
        }
        //scale hp bar
        player.Find("HPBar").Find("HP").GetComponent<RectTransform>().sizeDelta
                            = new Vector2((float)player.GetComponent<Properties>().currentHP
                            / (float)player.GetComponent<Properties>().oldHP
                            * player.Find("HPBar").Find("HP").GetComponent<RectTransform>().sizeDelta.x,
                                player.Find("HPBar").Find("HP").GetComponent<RectTransform>().sizeDelta.y);
        player.Find("HPBar").Find("Text").GetComponent<Text>().text
            = player.GetComponent<Properties>().currentHP.ToString();
        player.GetComponent<Properties>().oldHP = player.GetComponent<Properties>().currentHP;
    }

    private void AddProtection(Transform player, int value)
    {
        player.GetComponent<Properties>().protection += value;
        player.Find("Protection").GetComponent<Canvas>().enabled = true;
        if (player.GetComponent<Properties>().protection < 0)
            player.GetComponent<Properties>().protection = 0;

        player.Find("Protection").Find("Text").GetComponent<Text>().text
               = player.GetComponent<Properties>().protection.ToString();
    }
    public void RemoveProtection(Transform player)
    {
        player.GetComponent<Properties>().protection = 0;
        player.Find("Protection").GetComponent<Canvas>().enabled = false;
        player.Find("Protection").Find("Text").GetComponent<Text>().text = "0";
    }

    private void Nullify(Transform player, int from, [Optional]int to)
    {
        if (to == 0) {
            to = from;
        }
        for (int i = from; i <= to; i++) {
            player.GetComponent<Properties>().cardsToNullify[i] = true;
        }
    }
    public void StartNullify(Transform player)
    {
        for (int i = 0; i < player.GetComponent<Properties>().numberOfCards; i++) {
            if (player.GetComponent<Properties>().cardsToNullify[i]) {
                player.GetComponent<Properties>().nullifiedCards[i] = true;
                Transform cardslot = player.Find("Cardslot" + i);
                cardslot.Find("Activation").GetComponent<Image>().sprite = Resources.Load<Sprite>("StopSign");
            }
        }
    }
    public void RemoveNullify(Transform player)
    {
        player.GetComponent<Properties>().cardsToNullify = new bool[5];
        player.GetComponent<Properties>().nullifiedCards = new bool[5];
        for (int slot = 0; slot < player.GetComponent<Properties>().numberOfCards; slot++) {
            Transform cardslot = player.Find("Cardslot" + slot);
            cardslot.Find("Activation").GetComponent<Image>().sprite
                = Resources.Load<Sprite>(player.GetComponent<Properties>().cardsInPlay[slot].activation);
        }
    }

    private void ChangeDamage(Transform player, int value, int from, [Optional]int to)
    {
        if (to == 0) {
            to = from;
        }
        if (to >= player.GetComponent<Properties>().numberOfCards) {
            to = player.GetComponent<Properties>().numberOfCards - 1;
        }
        for (int i = from; i <= to; i++) {
            player.GetComponent<Properties>().damageModifier[i] += value;
        }

    }
    public bool ApplyDamageModifier(Transform player)
    {
        bool damageChanged = false;
        for (int i = 0; i < player.GetComponent<Properties>().numberOfCards; i++) {
            int newDamage = (player.GetComponent<Properties>().currentCardDamage[i]
                = player.GetComponent<Properties>().cardsInPlay[i].damage
                + player.GetComponent<Properties>().damageModifier[i]);
            Transform cardslot = player.Find("Cardslot" + i.ToString());
            if (newDamage > player.GetComponent<Properties>().cardsInPlay[i].damage
                    && cardslot.GetComponent<Animator>().GetInteger("Damage State") != 1) {
                cardslot.GetComponent<Animator>().SetInteger("Damage State", 1);
                damageChanged = true;
            }
            else if (newDamage < player.GetComponent<Properties>().cardsInPlay[i].damage
                    && cardslot.GetComponent<Animator>().GetInteger("Damage State") != -1) {
                cardslot.GetComponent<Animator>().SetInteger("Damage State", -1);
                damageChanged = true;
            }
            else if ((newDamage == player.GetComponent<Properties>().cardsInPlay[i].damage
                    && cardslot.GetComponent<Animator>().GetInteger("Damage State") != 0)) {
                cardslot.GetComponent<Animator>().SetInteger("Damage State", 0);
                damageChanged = true;
            }
            if (damageChanged) {
                cardslot.Find("Damage").GetComponent<Text>().text = newDamage.ToString();
            }
        }
        if (damageChanged) {
            return true;
        }
        else {
            return false;
        }
    }
    public bool ResetDamageModifier(Transform player)
    {
        player.GetComponent<Properties>().damageModifier = new int[5];
        if (ApplyDamageModifier(player)) {
            return true;
        }
        else {
            return false;
        }
    }

    public bool JinMutation(Transform player)
    {
        if (!player.GetComponent<Properties>().jinToMutate) {
            return false;
        }
        player.GetComponent<Properties>().jinToMutate = false;

        if (player.GetComponent<Properties>().jinMutated) {
            player.GetComponent<Properties>().jinMutated = false;
            player.Find("Avatar").GetComponent<Image>().sprite = Resources.Load<Sprite>("AvatarJin");
            for (int i = 0; i < player.GetComponent<Properties>().numberOfCards; i++) {
                Transform cardslot = player.Find("Cardslot" + i.ToString());
                switch (language) {
                    case "EN":
                        cardslot.Find("Text").GetComponent<Text>().text
                            = player.GetComponent<Properties>().cardsInPlay[i].textEN;
                        break;
                }
            }
        }
        else {
            player.GetComponent<Properties>().jinMutated = true;
            player.Find("Avatar").GetComponent<Image>().sprite = Resources.Load<Sprite>("AvatarJinMutated");
            for (int i = 0; i < player.GetComponent<Properties>().numberOfCards; i++) {
                Transform cardslot = player.Find("Cardslot" + i.ToString());
                if (player.GetComponent<Properties>().cardsInPlay[i].mutatedTextEN != null) {
                    switch (language) {
                        case "EN":
                            cardslot.Find("Text").GetComponent<Text>().text
                                = player.GetComponent<Properties>().cardsInPlay[i].mutatedTextEN;
                            break;
                    }
                }
            }
        }
        return true;
    }

    /*  /////////////   */
    /*      CARDS       */
    /* //////////////   */
    public bool JinThrustingUppercut()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action == "Focus"
                    && opponent.GetComponent<Properties>().action == "Strike") {
                player.GetComponent<Properties>().kickParry++;
                if (player.GetComponent<Properties>().jinMutated) {
                    player.GetComponent<Properties>().punchParry++;
                }
                return true;
            }
        }
        return false;
    }
    public bool JinRevolvingHands()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action == "Strike") {
                AddProtection(player, 25);
                if (player.GetComponent<Properties>().jinMutated) {
                    Nullify(opponent, 0, 4);
                }
                return true;
            }
        }
        return false;
    }
    public bool JinStinger()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action == "Focus" && opponent.GetComponent<Properties>().action == "Focus") {
                player.GetComponent<Properties>().jinToMutate = true;
                AddProtection(player, 18);
                return true;
            }
        }
        return false;
    }
    public bool JinSpinningSideKick()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action != opponent.GetComponent<Properties>().action) {
                if (player.GetComponent<Properties>().jinMutated) {
                    ChangeDamage(player, 7, 0, 4);
                    ChangeDamage(opponent, -7, 0, 4);
                }
                else {
                    ChangeDamage(player, 5, 0, 4);
                    ChangeDamage(opponent, -5, 0, 4);
                }
                return true;
            }
        }
        return false;
    }
    public bool JinPowerStance()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action == "Strike"
                    && opponent.GetComponent<Properties>().action == "Block") {
                //DrawCard(player)
                if (player.GetComponent<Properties>().jinMutated) {
                    //DrawCard(player)
                    player.GetComponent<Properties>().jinToMutate = true;
                }
                return true;
            }
        }
        return false;
    }
    public bool JinMedianLineDestruction()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action == "Strike"
                       && opponent.GetComponent<Properties>().action == "Strike") {
                //DrawCard(player)
                DrawToken(player, 15, "Punch");
                return true;
            }
            else if (player.GetComponent<Properties>().jinMutated && player.GetComponent<Properties>().action == "Strike") {
                //DrawCard(player)
                DrawToken(player, 15, "Punch");
                return true;
            }
        }
        return false;
    }
    public bool JinRightSpinningAxeKick()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (opponent.GetComponent<Properties>().action == "Block"
                    && player.GetComponent<Properties>().action != "Strike") {
                DamagePlayer(player, 5);
                if (player.GetComponent<Properties>().jinMutated) {
                    //DrawCard(player)
                    player.GetComponent<Properties>().jinToMutate = true;
                }
                return true;
            }
        }
        return false;
    }
    public bool JinLeftKnee()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action == "Strike"
                    && player.GetComponent<Properties>().jinMutated) {
                player.GetComponent<Properties>().cardsInPlay[0].tenacity = true;
                Discard(player, currentCardPosition);
                return true;
            }
            else if (player.GetComponent<Properties>().action == "Strike") {
                player.GetComponent<Properties>().cardsInPlay[3].tenacity = true;
                Discard(player, currentCardPosition);
                return true;
            }
        }
        return false;
    }
    public bool JinLeftJabToDoubleLow()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action == "Strike"
                    && opponent.GetComponent<Properties>().action == "Block") {
                //DrawCard(player)
                return true;
            }
            else if (player.GetComponent<Properties>().action == "Strike"
                        && player.GetComponent<Properties>().jinMutated) {
                //DrawCard(player)
                return true;
            }
        }
        return false;
    }
    public bool JinKneePopperToSidekick()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action == "Strike") {
                if (player.GetComponent<Properties>().jinMutated) {
                    //DrawCard(player, 2)
                    player.GetComponent<Properties>().jinToMutate = true;
                    return true;
                }
                else {
                    player.GetComponent<Properties>().jinToMutate = true;
                    return true;
                }
            }
        }
        return false;
    }
    public bool JinDoubleThrustRoundhouse()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action == "Strike" && player.GetComponent<Properties>().jinMutated) {
                if (opponent.GetComponent<Properties>().numberOfCards >= 2) {
                    //DrawCard(player)
                    return true;
                }
            }
            else if (player.GetComponent<Properties>().action == "Strike") {
                if (opponent.GetComponent<Properties>().numberOfCards >= 3) {
                    //DrawCard(player)
                    return true;
                }
            }
        }
        return false;
    }
    public bool JinDoubleLiftKick()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action != opponent.GetComponent<Properties>().action) {
                AddProtection(player, 8);
                player.GetComponent<Properties>().jinToMutate = true;
                return true;
            }
        }
        return false;
    }
    public bool JinDoubleChamberPunch()
    {
        if (player.GetComponent<Properties>().action == "Strike"
                && player.GetComponent<Properties>().jinMutated) {
            ChangeDamage(player, 14, 0, 4);
            return true;
        }
        else if (player.GetComponent<Properties>().action == "Strike") {
            ChangeDamage(player, 7, 0, 4);
            return true;
        }
        return false;
    }
    public bool JinCorpseThrust()
    {
        if (player.GetComponent<Properties>().cardsInPlay[currentCardPosition].rarity == "Gold") {
            if (player.GetComponent<Properties>().action == "Strike") {
                player.GetComponent<Properties>().maxHP -= 6;
                //DrawCard(player)
                return true;
            }
        }
        return false;
    }
}
