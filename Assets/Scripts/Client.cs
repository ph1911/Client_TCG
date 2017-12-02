using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using System.Text;

public class Client : MonoBehaviour
{
    public static Client Instance;
    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
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

    public void ConnectToServer(string host, int port)
    {
        //create socket
        try {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            connected = true;
            //listen every 1ms
            InvokeRepeating("ListenOnStream", 0f, 0.001f);
        }
        catch (Exception e) {
            GameObject.Find("ConnectScreen").transform.Find("InfoText").GetComponent<Text>().text = e.Message;
            Destroy(gameObject);
        }
    }

    //read data if available
    private void ListenOnStream()
    {
        if (!connected) {
            return;
        }
        if (stream.DataAvailable) {
            byte[] data = new byte[512];
            stream.Read(data, 0, 1);
            byte dataLength = data[0];
            stream.Read(data, 0, dataLength);
            OnIncomingData(Encoding.UTF8.GetString(data, 0, dataLength));
        }
    }

    private void Update()
    {
        clientKeepAliveTimer += Time.deltaTime;
        serverKeepAliveTimer += Time.deltaTime;
        if (serverKeepAliveTimer > 20f) {
            CloseConnection(true);
            return;
        }
        if (clientKeepAliveTimer > 5f) {
            Send("CKeepAlive");
            clientKeepAliveTimer = 0;
        }
    }

    //send messages to server
    public void Send(string data)
    {
        if (!connected) {
            return;
        }
        try {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] sendBytes = new byte[dataBytes.Length + 1];
            Array.Copy(dataBytes, 0, sendBytes, 1, dataBytes.Length);
            sendBytes[0] = Convert.ToByte(dataBytes.Length);
            stream.Write(sendBytes, 0, sendBytes.Length);
        }
        catch (Exception e) {
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
            case "SWHO":
                Send("CWHO|" + clientName);
                break;
            case "SAuthenticated":
                GameObject.Find("MainMenuScreen").transform.Find("InfoText").GetComponent<Text>().text = "Connected";
                GameObject.Find("ConnectScreen").GetComponent<Canvas>().enabled = false;
                GameObject.Find("MainMenuScreen").GetComponent<Canvas>().enabled = true;
                GameObject.Find("ConnectScreen").transform.Find("InfoText").GetComponent<Text>().text = "Welcome back";
                break;
            case "SWrongName":
                GameObject.Find("ConnectScreen").transform.Find("InfoText").GetComponent<Text>().text
                    = "This name is not available.";
                CloseConnection();
                break;
            case "SRDY":
                opponentName = allData[1];
                SceneManager.LoadScene("Battle");
                break;
            case "CardDrawFocus":
                GameObject.Find("Fighter1").GetComponent<Properties>().cardToDrawOnFocus = allData[1];
                break;
            case "CardDraw":
                if (allData[1] == "Pre" && allData.Length > 2) {
                    for (int i = 2; i < allData.Length; i++) {
                        GameObject.Find("Fighter1").GetComponent<Properties>().cardsToDrawPreFight.Add(allData[i]);
                    }
                }
                else if (allData[1] == "After" && allData.Length > 2) {
                    for (int i = 2; i < allData.Length; i++) {
                        GameObject.Find("Fighter1").GetComponent<Properties>().cardsToDrawAfterFight.Add(allData[i]);
                    }
                }
                else if (allData[1] == "Pre") {
                    GameObject.Find("Fighter1").GetComponent<Properties>().cardsToDrawPreFight.Add("Nothing");
                }
                else if (allData[1] == "After") {
                    GameObject.Find("Fighter1").GetComponent<Properties>().cardsToDrawAfterFight.Add("Nothing");
                }
                break;
            case "OpFocus":
                GameObject.Find("Fighter2").GetComponent<Properties>().cardToDrawOnFocus = allData[1];
                GameObject.Find("Fighter2").GetComponent<Properties>().action = "Focus";
                break;
            case "OpStrike":
                GameObject.Find("Fighter2").GetComponent<Properties>().cardToDrawOnFocus = "Nothing";
                GameObject.Find("Fighter2").GetComponent<Properties>().action = "Strike";
                break;
            case "OpBlock":
                GameObject.Find("Fighter2").GetComponent<Properties>().cardToDrawOnFocus = "Nothing";
                GameObject.Find("Fighter2").GetComponent<Properties>().action = "Block";
                break;
            case "OpDraw":
                if (allData[1] == "Pre" && allData.Length > 2) {
                    for (int i = 2; i < allData.Length; i++) {
                        GameObject.Find("Fighter2").GetComponent<Properties>().cardsToDrawPreFight.Add(allData[i]);
                    }
                }
                else if (allData[1] == "After" && allData.Length > 2) {
                    for (int i = 2; i < allData.Length; i++) {
                        GameObject.Find("Fighter2").GetComponent<Properties>().cardsToDrawAfterFight.Add(allData[i]);
                    }
                }
                else if (allData[1] == "Pre") {
                    GameObject.Find("Fighter2").GetComponent<Properties>().cardsToDrawPreFight.Add("Nothing");
                }
                else if (allData[1] == "After") {
                    GameObject.Find("Fighter2").GetComponent<Properties>().cardsToDrawAfterFight.Add("Nothing");
                }
                break;
            case "SDISC":
                CloseConnection();
                break;
            case "Chat":
                GameObject.Find("ChatScreen").GetComponent<ChatScreen>().ReceiveMessage(allData[1]);
                break;
            case "END":
                GameObject.Find("BattleScreen").GetComponent<Battle>().gameEnds(allData[1]);
                break;
        }
    }

    //close connection if application closed ect
    public void CloseConnection([Optional]bool informServer)
    {
        if (!connected) {
            return;
        }
        if (informServer) {
            Send("CDISC");
        }
        try {
            socket.Close();
        }
        finally {
            connected = false;
            Destroy(gameObject);
        }
    }
    private void OnApplicationQuit()
    {
        CloseConnection(true);
    }
    private void OnDisable()
    {
        CloseConnection(true);
    }
}