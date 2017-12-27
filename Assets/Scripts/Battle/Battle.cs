using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Battle : MonoBehaviour
{
	public Transform fighter1, fighter2;
	public Properties fighter1Properties, fighter2Properties;

	private CardDatabase _cardDatabase;
	private CardEffects _cardEffects;
	private BattleScreen _battleScreen;

	[SerializeField]
	private float maxTime;

	private float gameTimer, connectionTimer;
	private bool gameTimerRunning;
	private bool ready = true;
	private bool gameFinished;

	private void Start()
	{
		fighter1 = GameObject.Find("Fighter1").transform;
		fighter2 = GameObject.Find("Fighter2").transform;
		fighter1Properties = fighter1.GetComponent<Properties>();
		fighter2Properties = fighter2.GetComponent<Properties>();
		_cardDatabase = GameObject.Find("CardDatabase").GetComponent<CardDatabase>();
		_cardEffects = GameObject.Find("CardDatabase").GetComponent<CardEffects>();
		_battleScreen = GetComponent<BattleScreen>();

		_cardDatabase.LoadCardDatabase();
		fighter1Properties.fighterName = GameObject.Find("Client").GetComponent<Client>().clientName;
		fighter1.Find("FighterName").GetComponent<Text>().text = fighter1Properties.fighterName;
		fighter2Properties.fighterName = GameObject.Find("Client").GetComponent<Client>().opponentName;
		fighter2.Find("FighterName").GetComponent<Text>().text = fighter2Properties.fighterName;

		fighter1Properties.oldHP = fighter1Properties.maxHP;
		fighter2Properties.oldHP = fighter2Properties.maxHP;
		fighter1Properties.currentHP = fighter1Properties.maxHP;
		fighter2Properties.currentHP = fighter2Properties.maxHP;

		fighter1.Find("HPBar").transform.Find("Text").GetComponent<Text>().text = fighter1Properties.maxHP.ToString();
		fighter2.Find("HPBar").transform.Find("Text").GetComponent<Text>().text = fighter2Properties.maxHP.ToString();

		Init();
	}

	void Init()
	{
		gameTimer = maxTime;
		gameTimerRunning = true;

		fighter1Properties.blockedCards = new bool[5];
		fighter2Properties.blockedCards = new bool[5];
		fighter1Properties.cardToDrawOnFocus = "";
		fighter2Properties.cardToDrawOnFocus = "";
		fighter1Properties.cardsToDrawPreFight = new List<string>();
		fighter2Properties.cardsToDrawPreFight = new List<string>();
		fighter1Properties.cardsToDrawAfterFight = new List<string>();
		fighter2Properties.cardsToDrawAfterFight = new List<string>();
		fighter1Properties.tokensToDraw = new List<CardDatabase.Card>();
		fighter2Properties.tokensToDraw = new List<CardDatabase.Card>();
		fighter1Properties.action = "";
		fighter2Properties.action = "";
		fighter1Properties.parry = 0;
		fighter1Properties.punchParry = 0;
		fighter1Properties.kickParry = 0;
		fighter2Properties.parry = 0;
		fighter2Properties.punchParry = 0;
		fighter2Properties.kickParry = 0;

		GameObject.Find("ButtonFocus").GetComponent<Button>().interactable = true;
		GameObject.Find("ButtonBlock").GetComponent<Button>().interactable = true;
		//disable the strike button if the player has no cards
		if (fighter1.GetComponent<Properties>().numberOfCards == 0) {
			GameObject.Find("ButtonStrike").
				GetComponent<Button>().interactable = false;
		} else {
			GameObject.Find("ButtonStrike").
				GetComponent<Button>().interactable = true;
		}
		ready = true;
	}

	private void Update()
	{
		connectionTimer += Time.deltaTime;
		if (connectionTimer > 3f) {
			if (!GameObject.Find("Client")) {
				gameEnds("DISC");
			}
			connectionTimer = 0f;
		}

		if (gameTimerRunning) {
			gameTimer -= Time.deltaTime;
		}
		//force focus if timer runs out
		int timerRounded = (int) Mathf.Round(gameTimer);
		GameObject.Find("Time").GetComponent<Text>().text = timerRounded.ToString();
		if (gameTimerRunning && timerRounded == 0) {
			gameTimerRunning = false;
			if (fighter1Properties.action == "") {
				transform.GetComponent<BattleScreen>().ActionButtonPressed("Focus");
			}
		}

		//if the round ended and the game is supposed to end
		if (ready && gameFinished) {
			foreach (Canvas canvas in GameObject.Find("BattleScreen").GetComponentsInChildren<Canvas>()) {
				canvas.enabled = false;
			}
			GameObject.Find("EndGameScreen").GetComponent<Canvas>().enabled = true;
		}

		if (ready && fighter1Properties.action != "" && fighter2Properties.action != ""
			&& fighter1Properties.cardToDrawOnFocus != "" && fighter2Properties.cardToDrawOnFocus != ""
			&& fighter1Properties.cardsToDrawPreFight.Count != 0 && fighter2Properties.cardsToDrawPreFight.Count != 0
			&& fighter1Properties.cardsToDrawAfterFight.Count != 0 && fighter2Properties.cardsToDrawAfterFight.Count != 0) {
			ready = false;
			//start the fight cycle
			StartCoroutine(PlayActionAnimation());
		}
	}

	private IEnumerator PlayActionAnimation()
	{
		GameObject.Find("Fighter1Action").GetComponent<Image>().sprite
			= Resources.Load<Sprite>("Action" + fighter1Properties.action);
		GameObject.Find("Fighter2Action").GetComponent<Image>().sprite
			= Resources.Load<Sprite>("Action" + fighter2Properties.action);
		foreach (Canvas canvas in GetComponentsInChildren<Canvas>()) {
			canvas.enabled = false;
		}
		GameObject.Find("ActionScreen").GetComponent<Canvas>().enabled = true;
		GameObject.Find("Fighter1Action").GetComponent<Animator>().Play("TextMove");
		GameObject.Find("Fighter2Action").GetComponent<Animator>().Play("TextMove");
		yield return new WaitForSeconds(1.25f);
		GetComponent<Canvas>().enabled = true;
		if (fighter1Properties.protection != 0) {
			fighter1.Find("Protection").GetComponent<Canvas>().enabled = true;
		}
		if (fighter2Properties.protection != 0) {
			fighter2.Find("Protection").GetComponent<Canvas>().enabled = true;
		}
		GameObject.Find("ActionScreen").GetComponent<Canvas>().enabled = false;

		StartCoroutine(ActivateEffects("Pre"));
	}

	private IEnumerator ActivateEffects(string activation)
	{
		if (fighter1Properties.action == "Block") {
			fighter1Properties.parry += 2;
		}
		if (fighter2Properties.action == "Block") {
			fighter2Properties.parry += 2;
		}

		//set the pointers to fighter 1 and 2
		_cardEffects.player = fighter1;
		_cardEffects.opponent = fighter2;
		for (int i = 0; i < fighter1Properties.numberOfCards; i++) {
			//if the current card is supposed to activate now and is not nullified and has a nullification effect
			if (fighter1Properties.cardsInPlay[i].activation == activation
					&& !fighter1Properties.nullifiedCards[i]
					&& fighter1Properties.cardsInPlay[i].priority == 1) {
				//set the current card position
				_cardEffects.currentCardPosition = i;
				//play the animanition if needed
				if (fighter1Properties.cardsInPlay[i].effect[0]()) {
					Transform cardslot = fighter1.Find("Cardslot" + i.ToString());
					cardslot.GetComponent<Animator>().Play("EffectActivated");
					yield return new WaitForSeconds(0.5f);
				}
			}
		}
		//set the pointers to fighter 2 and 1
		_cardEffects.player = fighter2;
		_cardEffects.opponent = fighter1;
		for (int i = 0; i < fighter2Properties.numberOfCards; i++) {
			//if the current card is supposed to activate now and is not nullified and has a nullification effect
			if (fighter2Properties.cardsInPlay[i].activation == activation
					&& !fighter2Properties.nullifiedCards[i]
					&& fighter2Properties.cardsInPlay[i].priority == 1) {
				//set the current card position
				_cardEffects.currentCardPosition = i;
				//play the animanition if needed
				if (fighter2Properties.cardsInPlay[i].effect[0]()) {
					Transform cardslot = fighter2.Find("Cardslot" + i.ToString());
					cardslot.GetComponent<Animator>().Play("EffectActivated");
					yield return new WaitForSeconds(0.5f);
				}
			}
		}
		//start the nullification
		_cardEffects.StartNullify(fighter1);
		_cardEffects.StartNullify(fighter2);

		//set the pointers to fighter 1 and 2
		_cardEffects.player = fighter1;
		_cardEffects.opponent = fighter2;
		for (int i = 0; i < fighter1Properties.numberOfCards; i++) {
			//if the current card is supposed to activate now and is not nullified and has normal priority
			if (fighter1Properties.cardsInPlay[i].activation == activation
					&& !fighter1Properties.nullifiedCards[i]
					&& fighter1Properties.cardsInPlay[i].priority == 0) {
				//set the current card position
				_cardEffects.currentCardPosition = i;
				//play the animanition if needed
				if (fighter1Properties.cardsInPlay[i].effect[0]()) {
					Transform cardslot = fighter1.Find("Cardslot" + i.ToString());
					cardslot.GetComponent<Animator>().Play("EffectActivated");
					yield return new WaitForSeconds(0.5f);
				}
			}
		}
		//set the pointers to fighter 2 and 1
		_cardEffects.player = fighter2;
		_cardEffects.opponent = fighter1;
		for (int i = 0; i < fighter2Properties.numberOfCards; i++) {
			//if the current card is supposed to activate now and is not nullified and has normal priority
			if (fighter2Properties.cardsInPlay[i].activation == activation
					&& !fighter2Properties.nullifiedCards[i]
					&& fighter2Properties.cardsInPlay[i].priority == 0) {
				//set the current card position
				_cardEffects.currentCardPosition = i;
				//play the animanition if needed
				if (fighter2Properties.cardsInPlay[i].effect[0]()) {
					Transform cardslot = fighter2.Find("Cardslot" + i.ToString());
					cardslot.GetComponent<Animator>().Play("EffectActivated");
					yield return new WaitForSeconds(0.5f);
				}
			}
		}

		if (_cardEffects.ApplyDamageModifier(fighter1)
				| _cardEffects.ApplyDamageModifier(fighter2)) {
			yield return new WaitForSeconds(0.5f);
		}

		//discard in the after phase if player striked
		if (activation == "After") {
			if (fighter1.GetComponent<Properties>().action == "Strike") {
				_cardEffects.Discard(fighter1, 0, 4);
			}
			if (fighter2.GetComponent<Properties>().action == "Strike") {
				_cardEffects.Discard(fighter2, 0, 4);
			}
		}

		//activate discard effects
		if (_cardEffects.StartDiscarding(fighter1)
				 | _cardEffects.StartDiscarding(fighter2)) {
			yield return new WaitForSeconds(0.25f);
		}

		//check for mutation
		if (_cardEffects.JinMutation(fighter1)
				| _cardEffects.JinMutation(fighter2)) {
			yield return new WaitForSeconds(0.5f);
		}

		//draw cards
		if (activation == "Pre") {
			foreach (string card in fighter1Properties.cardsToDrawPreFight) {
				_cardEffects.DrawCardById(fighter1, card);
			}
			foreach (string card in fighter2Properties.cardsToDrawPreFight) {
				_cardEffects.DrawCardById(fighter2, card);
			}
			PreFight();
		} else {
			foreach (string card in fighter1Properties.cardsToDrawAfterFight) {
				_cardEffects.DrawCardById(fighter1, card);
			}
			foreach (string card in fighter2Properties.cardsToDrawAfterFight) {
				_cardEffects.DrawCardById(fighter2, card);
			}
			AfterFight();
		}
	}

	private void PreFight()
	{
		if (fighter1Properties.action == "Strike") {
			//calculate blocked cards for fighter1
			for (int i = 0; i < fighter1Properties.numberOfCards; i++) {
				if (fighter2Properties.punchParry > 0
						&& fighter1Properties.cardsInPlay[i].type == "Punch") {
					fighter1Properties.blockedCards[i] = true;
					fighter2Properties.punchParry--;
				}
			}
			for (int i = 0; i < fighter1Properties.numberOfCards; i++) {
				if (fighter2Properties.kickParry > 0
						&& fighter1Properties.cardsInPlay[i].type == "Kick") {
					fighter1Properties.blockedCards[i] = true;
					fighter2Properties.kickParry--;
				}
			}
			for (int i = 0; i < fighter1Properties.numberOfCards; i++) {
				if (fighter2Properties.parry > 0 && fighter1Properties.blockedCards[i] == false) {
					fighter1Properties.blockedCards[i] = true;
					fighter2Properties.parry--;
				}
			}
		}

		if (fighter2Properties.action == "Strike") {
			//calculate blocked cards for fighter2
			for (int i = 0; i < fighter2Properties.numberOfCards; i++) {
				if (fighter1Properties.punchParry > 0
						&& fighter2Properties.cardsInPlay[i].type == "Punch") {
					fighter2Properties.blockedCards[i] = true;
					fighter1Properties.punchParry--;
				}
			}
			for (int i = 0; i < fighter2Properties.numberOfCards; i++) {
				if (fighter1Properties.kickParry > 0
						&& fighter2Properties.cardsInPlay[i].type == "Kick") {
					fighter2Properties.blockedCards[i] = true;
					fighter1Properties.kickParry--;
				}
			}
			for (int i = 0; i < fighter2Properties.numberOfCards; i++) {
				if (fighter1Properties.parry > 0 && fighter2Properties.blockedCards[i] == false) {
					fighter2Properties.blockedCards[i] = true;
					fighter1Properties.parry--;
				}
			}
		}
		StartCoroutine(Fight());
	}

	private IEnumerator Fight()
	{
		if (fighter1Properties.action == "Strike") {
			yield return new WaitForSeconds(0.5f);
			if (fighter2Properties.action == "Focus") {
				_cardEffects.FocusBreak(fighter2);
				yield return new WaitForSeconds(1f);
			}
			//play blocked animations
			for (int i = 0; i < fighter1Properties.numberOfCards; i++) {
				if (fighter1Properties.blockedCards[i] == true) {
					Transform cardslot = fighter1.Find("Cardslot" + i.ToString());
					cardslot.GetComponent<Animator>().Play("FrameGlowRed");
					yield return new WaitForSeconds(0.5f);
				}
			}
			for (int i = 0; i < fighter1Properties.numberOfCards; i++) {
				//if card not blocked
				if (!fighter1Properties.blockedCards[i]) {
					_cardEffects.DamagePlayer(fighter2, fighter1Properties.currentCardDamage[i]);
					//play card animation
					fighter1.Find("Cardslot" + i.ToString()).GetComponent<Animator>()
						.Play("FrameGlowGreen");
					yield return new WaitForSeconds(0.5f);
				}
			}
		}

		if (fighter2Properties.action == "Strike") {
			yield return new WaitForSeconds(0.5f);
			if (fighter1Properties.action == "Focus") {
				_cardEffects.FocusBreak(fighter1);
				yield return new WaitForSeconds(1f);
			}
			for (int i = 0; i < fighter2Properties.numberOfCards; i++) {
				if (fighter2Properties.blockedCards[i] == true) {
					Transform cardslot = fighter2.Find("Cardslot" + i.ToString());
					cardslot.GetComponent<Animator>().Play("FrameGlowRed");
					yield return new WaitForSeconds(0.5f);
				}
			}
			for (int i = 0; i < fighter2Properties.numberOfCards; i++) {
				//if card not blocked
				if (!fighter2Properties.blockedCards[i]) {
					_cardEffects.DamagePlayer(fighter1, fighter2Properties.currentCardDamage[i]);
					//play card animation
					fighter2.Find("Cardslot" + i.ToString()).GetComponent<Animator>()
						.Play("FrameGlowGreen");
					yield return new WaitForSeconds(0.5f);
				}
			}
		}
		//MIDFIGHT RESET
		_cardEffects.RemoveProtection(fighter1);
		_cardEffects.RemoveProtection(fighter2);
		_cardEffects.RemoveNullify(fighter1);
		_cardEffects.RemoveNullify(fighter2);
		if (_cardEffects.ResetDamageModifier(fighter1)
				| _cardEffects.ResetDamageModifier(fighter2)) {
			yield return new WaitForSeconds(0.5f);
		}
		StartCoroutine(ActivateEffects("After"));
	}

	void AfterFight()
	{
		if (fighter1Properties.action == "Focus") {
			_cardEffects.DrawCardById(fighter1, fighter1Properties.cardToDrawOnFocus);
		}
		if (fighter2Properties.action == "Focus") {
			_cardEffects.DrawCardById(fighter2, fighter2Properties.cardToDrawOnFocus);
		}
		_cardEffects.PutTokensInPlay(fighter1);
		_cardEffects.PutTokensInPlay(fighter2);
		Init();
	}

	public void gameEnds(string outcome)
	{
		switch (outcome) {
			case "WIN":
				GameObject.Find("EndGameScreen").transform.Find("MainPanel").Find("InfoText").GetComponent<Text>().text
					= "YOU WIN";
				GameObject.Find("ResultCarrier").GetComponent<ResultCarrier>().wins++;
				break;
			case "LOSE":
				GameObject.Find("EndGameScreen").transform.Find("MainPanel").Find("InfoText").GetComponent<Text>().text
					= "You Lose";
				GameObject.Find("ResultCarrier").GetComponent<ResultCarrier>().losses++;
				break;
			case "TRUCE":
				GameObject.Find("EndGameScreen").transform.Find("MainPanel").Find("InfoText").GetComponent<Text>().text
					= "DOUBLE K.O.";
				break;
			case "LEAVE":
				try {
					Client.Instance.Send("LEAVE");
				} finally {
					GameObject.Find("EndGameScreen").transform.Find("MainPanel").Find("InfoText").GetComponent<Text>().text
						= "You left\nYou Lose";
					//dont wait for animations to finish
					foreach (Canvas canvas in GameObject.Find("BattleScreen").GetComponentsInChildren<Canvas>()) {
						canvas.enabled = false;
					}
					GameObject.Find("EndGameScreen").GetComponent<Canvas>().enabled = true;
				}
				GameObject.Find("ResultCarrier").GetComponent<ResultCarrier>().losses++;
				return;
			case "OpLEAVE":
				GameObject.Find("EndGameScreen").transform.Find("MainPanel").Find("InfoText").GetComponent<Text>().text
					= "Opponent left\nYOU WIN";
				GameObject.Find("EndGameScreen").GetComponent<Canvas>().enabled = true;
				GameObject.Find("ResultCarrier").GetComponent<ResultCarrier>().wins++;
				break;
			case "DISC":
				GameObject.Find("EndGameScreen").transform.Find("MainPanel").Find("InfoText").GetComponent<Text>().text
					= "Connection Lost";
				GameObject.Find("EndGameScreen").GetComponent<Canvas>().enabled = true;
				GameObject.Find("ResultCarrier").GetComponent<ResultCarrier>().losses++;
				break;
			case "EXIT":
				try {
					Client.Instance.Send("LEAVE");
				} finally {
					Application.Quit();
				}
				return;
		}
		//wait for the game animations to finish
		gameFinished = true;
	}
}
