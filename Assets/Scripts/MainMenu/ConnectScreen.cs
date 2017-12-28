using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ConnectScreen : MonoBehaviour
{
	public GameObject clientPrefab;
	public GameObject resultCarrierPrefab;
	public string language = "EN";
	public string connectAddress = "52.59.59.89";
	public int connectPort = 1911;

	public static readonly Queue<Action> connectionResultHandlingQueue = new Queue<Action>();

	private Client client;

	private void Start()
	{
		Transform resultCarrier = Instantiate(resultCarrierPrefab).transform;
		resultCarrier.name = "ResultCarrier";

		if (GameObject.Find("Client")) {
			GameObject.Find("MainMenuScreen").transform.Find("InfoText").GetComponent<Text>().text = "Main Menu";
			GameObject.Find("MainMenuScreen").GetComponent<MainMenuScreen>().alreadyConnected = true;
			GameObject.Find("ConnectScreen").GetComponent<Canvas>().enabled = false;
			GameObject.Find("MainMenuScreen").GetComponent<Canvas>().enabled = true;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
			ConnectButton();
		}
		// return focus to name input field
		GameObject.Find("InputNameField").GetComponent<InputField>().ActivateInputField();
		// check if results of a possible attempted connection need to be handled
		if (connectionResultHandlingQueue.Count > 0) {
			while (connectionResultHandlingQueue.Count > 0) {
				connectionResultHandlingQueue.Dequeue().Invoke();
			}
		}
	}

	public void ConnectButton()
	{
		if (GameObject.Find("InputNameField").GetComponent<InputField>().text == "") {
			transform.Find("InfoText").GetComponent<Text>().text = "Please enter a name.";
		} else {
			GameObject.Find("ConnectScreen").transform.Find("InfoText").GetComponent<Text>().text = "Connecting...";
			client = Instantiate(clientPrefab).GetComponent<Client>();
			client.name = "Client";
			client.clientName = GameObject.Find("InputNameField").GetComponent<InputField>().text;
			client.language = language;
			// start connection on a new thread
			new Thread(() => client.ConnectToServer(connectAddress, connectPort)).Start();
		}
	}

	public void LanguageButton()
	{
		if (language == "RU") {
			language = "EN";
			GameObject.Find("LanguageButton").transform.Find("Text").GetComponent<Text>().text = "EN";
		} else {
			language = "RU";
			GameObject.Find("LanguageButton").transform.Find("Text").GetComponent<Text>().text = "RU";
		}
	}

	public void DisconnectButton()
	{
		try {
			GameObject.Find("Client").GetComponent<Client>().CloseConnection();
			GameObject.Find("ConnectScreen").transform.Find("InfoText").GetComponent<Text>().text = "Disconnected";
		} catch (Exception e) {
			Debug.Log(e.Message);
		}
	}
	public void QuitGameButton()
	{
		try {
			GameObject.Find("Client").GetComponent<Client>().CloseConnection();
			GameObject.Find("ConnectScreen").transform.Find("InfoText").GetComponent<Text>().text = "Disconnected";
		} catch (Exception e) {
			Debug.Log(e.Message);
		} finally {
			Application.Quit();
		}
	}
}