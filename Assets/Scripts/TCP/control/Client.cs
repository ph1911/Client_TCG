using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using System.Collections.Generic;
using System.Linq;

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
	// this timer counts the time towards the next KeepAlive that the client must send
	private float clientKeepAliveTimer = 0f;
	// this timer keeps track of how much time has passed since the last time a message was received from the server
	private float serverKeepAliveTimer = 0f;
	// if this is false, all methods should exit
	// the actual disconnect is handled in CloseConnection()
	private bool connected;

	// new messages are enqueued from ListenOnStream()
	// to be read by OnIncomingData()
	// triggered by Update()
	private Queue<string> messageQueue = new Queue<string>();

	// connect to a server specified by the hostAddress on the given port
	public void ConnectToServer(string hostAddress, int port)
	{
		// create connection socket
		try {
			socket = new TcpClient(hostAddress, port);
			connected = true;
			ListenOnStream();
		} catch (SocketException e) {
			// enqueque the results of the failed connection in the GUI thread
			lock (ConnectScreen.connectionResultHandlingQueue) {
				ConnectScreen.connectionResultHandlingQueue.Enqueue(
					() => GameObject.Find("ConnectScreen").transform.Find("InfoText").GetComponent<Text>().text = e.Message);
				ConnectScreen.connectionResultHandlingQueue.Enqueue(
					() => Destroy(gameObject));
			}
		}
	}

	//read data if available
	private void ListenOnStream()
	{
		using (NetworkStream stream = socket.GetStream()) {
			while (stream.DataAvailable) {
				if (!connected) {
					return;
				}
				// the prefix contains the total length of the message
				// always 2 bytes long
				byte[] prefixBuffer = new byte[2];
				int prefixBytesRead = 0;
				// read one byte until prefix fully read
				while (prefixBytesRead < 2) {
					prefixBytesRead = stream.Read(prefixBuffer, prefixBytesRead, 1);
					// if the server sent 0 bytes -> he closed our connection
					if (prefixBytesRead == 0) {
						// close our side of the connection to prevent a half-open state
						CloseConnection();
						return;
					}
				}
				// total length of the message in bytes
				int messageLength = BitConverter.ToInt32(prefixBuffer, 0);
				byte[] messageBuffer = new byte[1024];
				int messageBytesRead = 0;
				while (messageBytesRead < messageLength) {
					// read bytes from the stream 
					// until the number of bytes read is equal to the total length of the message
					messageBytesRead = stream.Read(messageBuffer, messageBytesRead, messageLength - messageBytesRead);
					// if the server sent 0 bytes -> he closed our connection
					if (messageBytesRead == 0) {
						// close our side of the connection to prevent a half-open state
						CloseConnection();
						return;
					}
				}
				// enqueue the message so it can be read on the GUI thread
				// Update() handles the state of the queue
				lock (messageQueue) {
					messageQueue.Enqueue(Encoding.UTF8.GetString(messageBuffer, 0, messageLength));
				}
			}
		}
	}

	private void Update()
	{
		clientKeepAliveTimer += Time.deltaTime;
		serverKeepAliveTimer += Time.deltaTime;
		// send a KeepAlive every 5sec
		if (clientKeepAliveTimer > 5f) {
			Send("C_KeepAlive");
			clientKeepAliveTimer = 0;
		}
		// if the server hasn't sent anything for 20sec
		if (serverKeepAliveTimer > 20f) {
			// assume a disconnect
			// close our side of the connection
			CloseConnection();
			return;
		}
		// check if there are any messages in the message queue
		if (messageQueue.Count > 0) {
			// process them
			lock (messageQueue) {
				OnIncomingData(messageQueue.Dequeue());
			}
		}
	}

	//send messages to server
	public void Send(string data)
	{
		if (!connected || data == "" || data == null) {
			return;
		}
		try {
			// the message in bytes
			byte[] messageBytes = Encoding.UTF8.GetBytes(data);
			// the prefix contains the length of the message + its own length (2 bytes)
			// a prefix of 2 bytes allows messages to be up to 2^8 * 2^8 = 2^16 = 65536 bytes long
			byte[] prefixBytes = BitConverter.GetBytes(messageBytes.Length + 2);
			// the prefix must be exactly 2 bytes long
			Array.Resize(ref prefixBytes, 2);
			// assemble the whole data with prefix + message
			byte[] dataBytes = prefixBytes.Concat(messageBytes).ToArray();
			// write the whole thing on the NetworkStream
			using (NetworkStream stream = socket.GetStream()) {
				stream.Write(dataBytes, 0, dataBytes.Length);
			}
		} catch (Exception e) {
			Debug.Log("send error: " + e.Message);
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