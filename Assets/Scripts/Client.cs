using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;

public class Client : MonoBehaviour
{
	public static Client Instance;
	private void Awake()
	{
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
	}

	public string clientName;
	public string opponentName;
	public string language;

	private TcpClient socket;
	private NetworkStream stream;
	private float clientKeepAliveTimer = 0f;
	private float serverKeepAliveTimer = 0f;
	private bool connected;

	private Queue<string> messageQueue = new Queue<string>();

	public void ConnectToServer(string host, int port)
	{
		//create socket
		try {
			socket = new TcpClient(host, port);
			stream = socket.GetStream();
			connected = true;
			ListenOnStream();
		} catch (SocketException e) {
			// enqueque the results of the failed connection in the GUI thread
			ConnectScreen.connectionResultHandling.Enqueue(
				() => GameObject.Find("ConnectScreen").transform.Find("InfoText").GetComponent<Text>().text = e.Message);
			ConnectScreen.connectionResultHandling.Enqueue(
				() => Destroy(gameObject));
		}
	}

	//read data if available
	private void ListenOnStream()
	{
		while (true) {
			if (!connected) {
				break;
			}
			if (stream.DataAvailable) {
				byte[] data = new byte[512];
				stream.Read(data, 0, 1);
				byte dataLength = data[0];
				stream.Read(data, 0, dataLength + 1);
				// TODO replace with queue to return to GUI thread
				OnIncomingData(Encoding.UTF8.GetString(data, 0, dataLength));
			}
		}
	}

	private void Update()
	{
		clientKeepAliveTimer += Time.deltaTime;
		serverKeepAliveTimer += Time.deltaTime;
		if (serverKeepAliveTimer > 20f) {
			CloseConnection();
			return;
		}
		if (clientKeepAliveTimer > 5f) {
			Send("CKeepAlive");
			clientKeepAliveTimer = 0;
		}
		// check if there are any messages in the message queue
		if (messageQueue.Count > 0) {
			// process them
			OnIncomingData(messageQueue.Dequeue());
		}
	}

	//send messages to server
	public void Send(string data)
	{
		if (!connected) {
			return;
		}
		try {
			byte[] messageBytes = Encoding.UTF8.GetBytes(data);
			byte[] prefixBytes = BitConverter.GetBytes(messageBytes.Length);
			// assemble the whole data with prefix + message
			byte[] dataBytes = prefixBytes;
			Array.Copy(messageBytes, dataBytes, messageBytes.Length);

			stream.Write(dataBytes, 0, messageBytes.Length);
		} catch (Exception e) {
			Debug.Log("send error " + e.Message);
		}
	}

	//Read messages from server
	private void OnIncomingData(string data)
	{
		Debug.Log(data);
		serverKeepAliveTimer = 0f;
		string[] allData = data.Split('|');
		switch (allData[0]) {
			case "S_Who":
				Send("C_Name|" + clientName);
				break;
			case "S_ClientAuthenticated":
				GameObject.Find("MainMenuScreen").transform.Find("InfoText").GetComponent<Text>().text = "Connected";
				GameObject.Find("ConnectScreen").GetComponent<Canvas>().enabled = false;
				GameObject.Find("MainMenuScreen").GetComponent<Canvas>().enabled = true;
				GameObject.Find("ConnectScreen").transform.Find("InfoText").GetComponent<Text>().text = "Welcome back";
				break;
			case "S_WrongName":
				GameObject.Find("ConnectScreen").transform.Find("InfoText").GetComponent<Text>().text
					= "This name is not available.";
				CloseConnection();
				break;
			case "S_GameReady":
				opponentName = allData[1];
				SceneManager.LoadScene("Battle");
				break;
			case "S_Focus":
				GameObject.Find("Fighter1").GetComponent<Properties>().cardToDrawOnFocus = allData[1];
				break;
			case "S_CardDraw":
				if (allData[1] == "Pre" && allData.Length > 2) {
					for (int i = 2; i < allData.Length; i++) {
						GameObject.Find("Fighter1").GetComponent<Properties>().cardsToDrawPreFight.Add(allData[i]);
					}
				} else if (allData[1] == "After" && allData.Length > 2) {
					for (int i = 2; i < allData.Length; i++) {
						GameObject.Find("Fighter1").GetComponent<Properties>().cardsToDrawAfterFight.Add(allData[i]);
					}
				} else if (allData[1] == "Pre") {
					GameObject.Find("Fighter1").GetComponent<Properties>().cardsToDrawPreFight.Add("Nothing");
				} else if (allData[1] == "After") {
					GameObject.Find("Fighter1").GetComponent<Properties>().cardsToDrawAfterFight.Add("Nothing");
				}
				break;
			case "S_OpponentFocus":
				GameObject.Find("Fighter2").GetComponent<Properties>().cardToDrawOnFocus = allData[1];
				GameObject.Find("Fighter2").GetComponent<Properties>().action = "Focus";
				break;
			case "S_OpponentStrike":
				GameObject.Find("Fighter2").GetComponent<Properties>().cardToDrawOnFocus = "Nothing";
				GameObject.Find("Fighter2").GetComponent<Properties>().action = "Strike";
				break;
			case "S_OpponentBlock":
				GameObject.Find("Fighter2").GetComponent<Properties>().cardToDrawOnFocus = "Nothing";
				GameObject.Find("Fighter2").GetComponent<Properties>().action = "Block";
				break;
			case "S_OpponentCardDraw":
				if (allData[1] == "Pre" && allData.Length > 2) {
					for (int i = 2; i < allData.Length; i++) {
						GameObject.Find("Fighter2").GetComponent<Properties>().cardsToDrawPreFight.Add(allData[i]);
					}
				} else if (allData[1] == "After" && allData.Length > 2) {
					for (int i = 2; i < allData.Length; i++) {
						GameObject.Find("Fighter2").GetComponent<Properties>().cardsToDrawAfterFight.Add(allData[i]);
					}
				} else if (allData[1] == "Pre") {
					GameObject.Find("Fighter2").GetComponent<Properties>().cardsToDrawPreFight.Add("Nothing");
				} else if (allData[1] == "After") {
					GameObject.Find("Fighter2").GetComponent<Properties>().cardsToDrawAfterFight.Add("Nothing");
				}
				break;
			case "S_Chat":
				GameObject.Find("ChatScreen").GetComponent<ChatScreen>().ReceiveMessage(allData[1]);
				break;
			case "S_GameEnd":
				GameObject.Find("BattleScreen").GetComponent<Battle>().gameEnds(allData[1]);
				break;
		}
	}

	//close connection if application closed ect
	public void CloseConnection()
	{
		if (!connected) {
			return;
		}
		try {
			socket.Close();
		} finally {
			connected = false;
			Destroy(gameObject);
		}
	}
	private void OnApplicationQuit()
	{
		CloseConnection();
	}
	private void OnDisable()
	{
		CloseConnection();
	}
}