using UnityEngine;
using UnityEngine.UI;

public class BattleScreen : MonoBehaviour
{
    public GameObject cardslotPrefab;
    public string view;

    private Transform fighter1;
    private Transform fighter2;
    private Battle _battle;
    private string language;

    private void Start()
    {
        fighter1 = GameObject.Find("Fighter1").transform;
        fighter2 = GameObject.Find("Fighter2").transform;
        _battle = GetComponent<Battle>();
        language = GameObject.Find("Client").GetComponent<Client>().language;

        //instantiate cardslot prefabs for fighter1
        GameObject cardslot = Instantiate(cardslotPrefab, transform.Find("Fighter1"));
        cardslot.transform.localPosition = new Vector3(-850, -320);
        cardslot.name = "Cardslot0";
        cardslot = Instantiate(cardslotPrefab, transform.Find("Fighter1"));
        cardslot.transform.localPosition = new Vector3(-500, -320);
        cardslot.name = "Cardslot1";
        cardslot = Instantiate(cardslotPrefab, transform.Find("Fighter1"));
        cardslot.transform.localPosition = new Vector3(-150, -320);
        cardslot.name = "Cardslot2";
        cardslot = Instantiate(cardslotPrefab, transform.Find("Fighter1"));
        cardslot.transform.localPosition = new Vector3(200, -320);
        cardslot.name = "Cardslot3";
        cardslot = Instantiate(cardslotPrefab, transform.Find("Fighter1"));
        cardslot.transform.localPosition = new Vector3(550, -320);
        cardslot.name = "Cardslot4";

        //instantiate cardslot prefabs for fighter2
        cardslot = Instantiate(cardslotPrefab, transform.Find("Fighter2"));
        cardslot.transform.localPosition = new Vector3(-850, 20);
        cardslot.name = "Cardslot0";
        cardslot = Instantiate(cardslotPrefab, transform.Find("Fighter2"));
        cardslot.transform.localPosition = new Vector3(-500, 20);
        cardslot.name = "Cardslot1";
        cardslot = Instantiate(cardslotPrefab, transform.Find("Fighter2"));
        cardslot.transform.localPosition = new Vector3(-150, 20);
        cardslot.name = "Cardslot2";
        cardslot = Instantiate(cardslotPrefab, transform.Find("Fighter2"));
        cardslot.transform.localPosition = new Vector3(200, 20);
        cardslot.name = "Cardslot3";
        cardslot = Instantiate(cardslotPrefab, transform.Find("Fighter2"));
        cardslot.transform.localPosition = new Vector3(550, 20);
        cardslot.name = "Cardslot4";
    }

    public void ActionButtonPressed(string buttonName)
    {
        switch (buttonName) {
            case "Focus":
                fighter1.GetComponent<Properties>().action = "Focus";
                break;
            case "Strike":
                fighter1.GetComponent<Properties>().cardToDrawOnFocus = "Nothing";
                fighter1.GetComponent<Properties>().action = "Strike";
                break;
            case "Block":
                fighter1.GetComponent<Properties>().cardToDrawOnFocus = "Nothing";
                fighter1.GetComponent<Properties>().action = "Block";
                break;
        }
        Client client = GameObject.Find("Client").GetComponent<Client>();
        client.Send("Pl" + buttonName);
        GameObject.Find("ButtonFocus").GetComponent<Button>().interactable = false;
        GameObject.Find("ButtonStrike").GetComponent<Button>().interactable = false;
        GameObject.Find("ButtonBlock").GetComponent<Button>().interactable = false;
    }

    public void ChangeView()
    {
        Transform cardslot;
        if (view == "Name") {
            view = "Text";
            GameObject.Find("ViewButton").GetComponent<Image>().sprite = Resources.Load<Sprite>("EyeOpen");
            for (int i = 0; i < _battle.fighter1Properties.numberOfCards; i++) {
                cardslot = _battle.fighter1.Find("Cardslot" + i.ToString());
                cardslot.Find("Text").GetComponent<Text>().enabled = true;
                cardslot.Find("Name").GetComponent<Text>().enabled = false;
            }
            for (int i = 0; i < _battle.fighter2Properties.numberOfCards; i++) {
                cardslot = _battle.fighter2.Find("Cardslot" + i.ToString());
                cardslot.Find("Text").GetComponent<Text>().enabled = true;
                cardslot.Find("Name").GetComponent<Text>().enabled = false;
            }
        }
        else {
            view = "Name";
            GameObject.Find("ViewButton").GetComponent<Image>().sprite = Resources.Load<Sprite>("EyeClosed");
            for (int i = 0; i < _battle.fighter1Properties.numberOfCards; i++) {
                cardslot = _battle.fighter1.Find("Cardslot" + i.ToString());
                cardslot.Find("Text").GetComponent<Text>().enabled = false;
                cardslot.Find("Name").GetComponent<Text>().enabled = true;
            }
            for (int i = 0; i < _battle.fighter2Properties.numberOfCards; i++) {
                cardslot = _battle.fighter2.Find("Cardslot" + i.ToString());
                cardslot.Find("Text").GetComponent<Text>().enabled = false;
                cardslot.Find("Name").GetComponent<Text>().enabled = true;
            }
        }
    }

    public void ChatButtonPressed()
    {
        GameObject.Find("BattleScreen").transform.Find("ChatButton")
            .Find("NotificationDot").GetComponent<Image>().enabled = false;
        GameObject.Find("ChatScreen").GetComponent<Canvas>().enabled = true;
    }

    public void OpenMenuScreen()
    {
        GameObject.Find("MenuScreen").GetComponent<Canvas>().enabled = true;
    }
}