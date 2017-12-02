using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ChatScreen : MonoBehaviour
{
    public GameObject messagePrefab;
    public GameObject myMessagePrefab;

    public void ReceiveMessage(string message, [Optional]bool fromMe)
    {
        GameObject messageBody;
        if (fromMe) {
            messageBody = Instantiate(myMessagePrefab,
            GameObject.Find("ChatScreen").transform.Find("MainPanel").Find("MessageScrollView")
                .Find("Viewport").Find("MessagePanel"));
        }
        else {
            //display the "message dot"
            GameObject.Find("BattleScreen").transform.Find("ChatButton")
                .Find("NotificationDot").GetComponent<Image>().enabled = true;
            messageBody = Instantiate(messagePrefab,
                GameObject.Find("ChatScreen").transform.Find("MainPanel").Find("MessageScrollView")
                .Find("Viewport").Find("MessagePanel"));
        }
        messageBody.GetComponentInChildren<Text>().text = message;
        messageBody.name = message;
        messageBody.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }

    public void SendMessage()
    {
        if (GameObject.Find("ChatScreen").transform
            .Find("MainPanel").Find("MessageInputField").GetComponent<InputField>().text == "") {
            return;
        }
        Client.Instance.Send("Chat|" + GameObject.Find("ChatScreen").transform
            .Find("MainPanel").Find("MessageInputField").GetComponent<InputField>().text);
        //"receive" the message you wrote to display it on the screen
        ReceiveMessage(Client.Instance.clientName + ": " + GameObject.Find("ChatScreen").transform
            .Find("MainPanel").Find("MessageInputField").GetComponent<InputField>().text, true);
        GameObject.Find("ChatScreen").transform
            .Find("MainPanel").Find("MessageInputField").GetComponent<InputField>().text = "";
    }

    public void CloseButtonPressed()
    {
        //hide the message dot for messages until now
        GameObject.Find("BattleScreen").transform.Find("ChatButton")
            .Find("NotificationDot").GetComponent<Image>().enabled = false;
        GameObject.Find("ChatScreen").GetComponent<Canvas>().enabled = false;
    }
}
