using System.Collections;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using TMPro;

public class PythonUDP : MonoBehaviour
{
    Thread mThread;
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 25001;
    IPAddress localAdd;
    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
    UdpClient client;
    bool running;

    //game objects to manipulate
    public TMP_InputField ip_address_inputfield;
    public TMP_InputField port_inputfield;
    public GameObject fire;
    public GameObject livingroom_floor;
    public GameObject kitchen_floor;
    public GameObject light1;
    public GameObject light2;

    public Color inactive;
    public Color active;

    internal static PythonUDP wkr;
    Queue<Action> jobs = new Queue<Action>();

    void Awake()
    {
        wkr = this;
    }

    void Update()
    {
        while (jobs.Count > 0)
            jobs.Dequeue().Invoke();
    }

    internal void AddJob(Action newJob)
    {
        jobs.Enqueue(newJob);
    }

    public void Connect()
    {
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
    }

    async void GetInfo()
    {
        if (string.IsNullOrWhiteSpace(ip_address_inputfield.text) || string.IsNullOrWhiteSpace(port_inputfield.text))
        {
            Debug.Log("please input your ip and port");
            return;
        }
        connectionIP = ip_address_inputfield.text;
        connectionPort = int.Parse(port_inputfield.text);
        Debug.Log(connectionIP);
        Debug.Log(connectionPort);

        localAdd = IPAddress.Parse(connectionIP);
        client = new UdpClient();
        client.Connect(localAdd, connectionPort);

        running = true;
        while (running)
        {
            SendAndReceiveData();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
        }
        //listener.Stop();
        //client.Close();
    }

    void SendAndReceiveData()
    {
        //send to server
        byte[] sendBytes = Encoding.ASCII.GetBytes("Request");
        client.Send(sendBytes, sendBytes.Length);
        Debug.Log("send request server");

        //receive from server
        byte[] receiveBytes = client.Receive(ref RemoteIpEndPoint);
        string returnData = Encoding.ASCII.GetString(receiveBytes);
        Debug.Log(returnData);
        InterpreteMsg(returnData);
    }

    void InterpreteMsg(string msg)
    {
        PythonUDP.wkr.AddJob(() =>
        {
            // Will run on main thread, hence issue is solved
            string[] sArray = msg.Split(',');
            if (sArray[0] == "fire")
            {
                if (sArray[1] == "1")
                    fire.SetActive(true);
                else
                    fire.SetActive(false);
            }
            else if (sArray[0] == "light1")
            {
                if (sArray[1] == "1")
                    light1.SetActive(true);
                else
                    light1.SetActive(false);
            }
            else if (sArray[0] == "light2")
            {
                if (sArray[1] == "1")
                    light2.SetActive(true);
                else
                    light2.SetActive(false);
            }
            else if (sArray[0] == "motion1")
            {
                if (sArray[1] == "1")
                    livingroom_floor.GetComponent<Renderer>().material.color = active;
                else
                    livingroom_floor.GetComponent<Renderer>().material.color = inactive;
            }
            else if (sArray[0] == "motion2")
            {
                if (sArray[1] == "1")
                    kitchen_floor.GetComponent<Renderer>().material.color = active;
                else
                    kitchen_floor.GetComponent<Renderer>().material.color = inactive;
            }   
        });

    }

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
}