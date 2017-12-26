using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : MonoBehaviour
{
    public bool alreadyConnected;

    private void Start()
    {
        transform.Find("Wins").GetComponent<Text>().text
             = GameObject.Find("ResultCarrier").GetComponent<ResultCarrier>().wins.ToString();
        transform.Find("Losses").GetComponent<Text>().text
            = GameObject.Find("ResultCarrier").GetComponent<ResultCarrier>().losses.ToString();
    }

    private float timer = 0f;
    private void Update()
    {
        //check if the client still exists - if not return to ConnectScreen
        timer += Time.deltaTime;
        if (timer > 3f) {
            if (!GameObject.Find("Client")) {
                GameObject.Find("MainMenuScreen").transform.Find("SearchGameButton").GetComponent<Button>().interactable = true;
                GameObject.Find("MainMenuScreen").transform.Find("StopSearchingButton").GetComponent<Button>().interactable = false;
                GameObject.Find("ConnectScreen").GetComponent<Canvas>().enabled = true;
                GameObject.Find("MainMenuScreen").GetComponent<Canvas>().enabled = false;
                if (alreadyConnected) {
                    GameObject.Find("ConnectScreen").transform.Find("InfoText").GetComponent<Text>().text
                        = "Connection Lost";
                }
            }
            timer = 0f;
        }
    }

    public void SearchGameButton()
    {
        GameObject.Find("MainMenuScreen").transform.Find("SearchGameButton").GetComponent<Button>().interactable = false;
        GameObject.Find("MainMenuScreen").transform.Find("StopSearchingButton").GetComponent<Button>().interactable = true;
        Client.Instance.Send("CSearchGame");
        GameObject.Find("MainMenuScreen").transform.Find("InfoText").GetComponent<Text>().text = "Waiting for a Game...";
    }

    public void StopSearchingButton()
    {
        GameObject.Find("MainMenuScreen").transform.Find("SearchGameButton").GetComponent<Button>().interactable = true;
        GameObject.Find("MainMenuScreen").transform.Find("StopSearchingButton").GetComponent<Button>().interactable = false;
        Client.Instance.Send("CStopSearching");
        GameObject.Find("MainMenuScreen").transform.Find("InfoText").GetComponent<Text>().text = "Connected";
    }

    public void Disconnect()
    {
        GameObject.Find("MainMenuScreen").transform.Find("SearchGameButton").GetComponent<Button>().interactable = true;
        GameObject.Find("MainMenuScreen").transform.Find("StopSearchingButton").GetComponent<Button>().interactable = false;
        GameObject.Find("ConnectScreen").GetComponent<Canvas>().enabled = true;
        GameObject.Find("MainMenuScreen").GetComponent<Canvas>().enabled = false;
        Client.Instance.CloseConnection(true);
    }
}
