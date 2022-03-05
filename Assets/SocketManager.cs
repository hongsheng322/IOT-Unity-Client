using System;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;


public class SocketManager : MonoBehaviour
{
    public SocketIOUnity socket;

    public GameObject fire;


    // Start is called before the first frame update
    void Start()
    {
        //TODO: check the Uri if Valid.
        var uri = new Uri("http://192.168.1.20:8080/:");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            EIO = 3
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        /// reserved socketio events
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("socket.OnConnected");
        };
        socket.OnPing += (sender, e) =>
        {
            Debug.Log("Ping");
        };
        socket.OnPong += (sender, e) =>
        {
            Debug.Log("Pong: " + e.TotalMilliseconds);
        };
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };
        ////

        Debug.Log("Connecting...");

        socket.Connect();

        socket.OnUnityThread("light", (data) =>
        {
            if (data.GetValue().GetInt32() == 1)
                fire.SetActive(true);
            else
                fire.SetActive(false);
        });

        //socket.On("light", (data) =>
        //{
        //    Debug.Log("niceeee");
        //});
    }



 
}