﻿//Code Adopt from https://gist.github.com/danielbierwirth/0636650b005834204cb19ef5ae6ccedb

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace TPC_Server
{
    public class TCP_Server : MonoBehaviour
    {
        #region private members 	
        /// TCPListener to listen for incomming TCP connection 	
        /// requests. 	
        private TcpListener tcpListener;
        /// Background thread for TcpServer workload. 	
        private Thread tcpListenerThread;
        /// Create handle to connected tcp client. 	
        private TcpClient connectedTcpClient;
       
        #endregion
        public string IPC_comms_message = "null_msg";
        public string IPC_output = "null";
        public string IP_adress = "127.0.0.1";
        public int Port_adress = 27000;
        // Update is called once per frame
        [Range(0.1F, 20)]
        public float delay_time = 2.0F;
        private float last_call = 0.0F;
        private const int MAX_RETRIES = 15;

        // Use this for initialization
        void Start()
        {
            // Start TcpServer background thread 		
            tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
            tcpListenerThread.IsBackground = true;
            tcpListenerThread.Start();
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time > last_call)
            {
                last_call = Time.time + delay_time;
                SendMessage(IPC_comms_message);
            }
        }

        public void set_msg(string msg_out)
        {
            IPC_comms_message = msg_out;
        }

        /// <summary> 	
        /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
        /// </summary> 	
        private void ListenForIncommingRequests()
        {
            try
            {
                // Create listener on localhost port 27000. 	
                Console.WriteLine(IP_adress);
                //tcpListener = new TcpListener(IPAddress.Parse(IP_adress), Port_adress);

                counter = 0;
                tcpListener = new TcpListener(IPAddress.Any, Port_adress);
                tcpListener.Start();
                //connectedTcpClient.Client.ReceiveTimeout = 50000; 
                Debug.Log("Server is listening");
                Console.WriteLine("Server started!");
                Byte[] bytes = new Byte[1024];
                while (true)
                {
                    connectedTcpClient = tcpListener.AcceptTcpClient();

                    {
                        // Get a stream object for reading 					
                        using (NetworkStream stream = connectedTcpClient.GetStream())
                        {
                            int length;
                            // Read incomming stream into byte arrary. 						
                            while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                var incommingData = new byte[length];
                                Array.Copy(bytes, 0, incommingData, 0, length);
                                // Convert byte array to string message. 							
                                string clientMessage = Encoding.ASCII.GetString(incommingData);
                                Debug.Log("client message received as: " + clientMessage);
                                IPC_output = clientMessage;
                            }
                        }
                    }
                }
            }
            catch (SocketException socketException)
            {
                Console.WriteLine("Server did not start!");
                Debug.Log("SocketException " + socketException.ToString());
            }
        }
        /// <summary> 	
        /// Send message to client using socket connection. 	
        /// </summary> 	

        private int counter;
        private void SendMessage(string serverMessage)
        {
            if (connectedTcpClient == null)
            {
                return;
            }
            
            if (counter >= MAX_RETRIES)
            {
                counter = 0; 
                connectedTcpClient = null;
                return;
            }

            try
            {
                // Get a stream object for writing. 			
                NetworkStream stream = connectedTcpClient.GetStream();
                if (stream.CanWrite)
                {
                    //Debug.Log("Hi, I'm a robot!");
                    //string serverMessage = "This is a message from your server.";
                    // Convert string message to byte array.                 
                    byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
                    // Write byte array to socketConnection stream.               
                    stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                    Debug.Log("Server sent his message - should be received by client");
                    Console.WriteLine("Msg sent");
                    counter = 0;
                }
                else if (!stream.CanWrite)
                {
                    counter++;
                    Debug.Log("Skipping message");
                }
                ///Debug.Log("Hi, I'm a job!");
                ///
            }
            catch (SocketException socketException)
            {
                Debug.Log("Socket exception: " + socketException);
            }
        }
    }
}