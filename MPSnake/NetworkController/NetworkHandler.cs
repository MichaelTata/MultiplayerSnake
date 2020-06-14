using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

//Authors: Michael Tata


namespace NetworkController
{

    /// <summary>
    /// Wraps information around a Socket. Change members to be private and add appropriate getters/setters after.
    /// </summary>
    public class SocketState
    {
        /// <summary>
        /// Used to designate a callback function for communication in a network
        /// </summary>
        public Action<SocketState> callBack;

        /// <summary>
        /// A socket used for network communication. 
        /// </summary>
        public Socket theSocket;
        
        /// <summary>
        /// An ID used to identify the socket?
        /// </summary>
        private int ID;


        /// <summary>
        /// Socket is shutting down and callbacks need to be handled.
        /// </summary>
        private bool removed;

        // This is the buffer where we will receive message data from the client
        /// <summary>
        /// Used to recieve data 
        /// </summary>
        public byte[] messageBuffer = new byte[2048];

        // This is a larger (growable) buffer, in case a single receive does not contain the full message.
        /// <summary>
        /// Used to handle recieved data without overflow and incomplete messages
        /// </summary>
        public StringBuilder sb = new StringBuilder();

        /// <summary>
        /// Basic constructor for socket which assigns a socket and its id.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="id"></param>
        public SocketState(Socket s, int id)
        {

            theSocket = s;
            ID = id;
            
        }


        public void shutdownSocket()
        {

            try
            {
                Console.WriteLine("Shutting down client socket");
                theSocket.Shutdown(SocketShutdown.Both);
                theSocket.Close();
            }
            catch(ObjectDisposedException e)
            {
                return;
            }
        }


        public bool getFailStatus()
        {
            return removed;
        }

        public void setFailStatus(bool rem)
        {

            removed = rem;
        }


        /// <summary>
        /// Used to return ID and check for failed connection
        /// </summary>
        /// <returns></returns>
        public int getID()
        {

            return ID;
        }

        public void setID(int id)
        {
            ID = id;

        }


       

    }


    /// <summary>
    /// Used by server to establish connections
    /// </summary>
    public class NewConnectionState
    {
        public Action<SocketState> callBack;

        public TcpListener Listener;

    }

   
    /// <summary>
    /// Network Handler class which 
    /// </summary>
    public static class NetworkHandler
    {
        /// <summary>
        /// Default port of 11000.
        /// </summary>
        public const int DEFAULT_PORT = 11000;

        
        public static void StartServer(Action<SocketState> call)
        {

            //byte[] byteip = Encoding.ASCII.GetBytes(Dns.GetHostName());

            /* IPAddress localaddr = GetLocalIPAddress();

             Console.WriteLine("Local address is " + localaddr.ToString());

             TcpListener listener = new TcpListener(localaddr, DEFAULT_PORT);
             */
            TcpListener listener = new TcpListener(DEFAULT_PORT);


            listener.Start();

            NewConnectionState cs = new NewConnectionState();
            cs.Listener = listener;
            cs.callBack = call;

            Console.WriteLine("Listening...");

            listener.BeginAcceptSocket(new AsyncCallback(ConnectionRequested), cs);

        }


        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }




        private static void ConnectionRequested(IAsyncResult ar)
        {
            NewConnectionState cs = (NewConnectionState)ar.AsyncState;

            Socket socket = cs.Listener.EndAcceptSocket(ar);

            //Will have to change this ID to the client specific ID in server
            SocketState ss = new SocketState(socket , 0);

            ss.theSocket = socket;
            ss.callBack = cs.callBack;

            //??? isn't this redundant
            ss.callBack(ss);

            cs.Listener.BeginAcceptSocket(ConnectionRequested, cs);

        }



        /// <summary>
        /// Start attempting to connect to the server
        /// </summary>
        /// <param name="host_name"> server to connect to </param>
        /// <returns></returns>
        public static SocketState ConnectToServer(Action<SocketState> connecEst, string hostName)
        {

            SocketState theServer;

            System.Diagnostics.Debug.WriteLine("connecting  to " + hostName);

            // Connect to a remote device.
            try
            {

                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo;
                IPAddress ipAddress = IPAddress.None;

                // Determine if the server address is a URL or an IP
                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostName);
                    bool foundIPV4 = false;
                    foreach (IPAddress addr in ipHostInfo.AddressList)
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }
                    // Didn't find any IPV4 addresses
                    if (!foundIPV4)
                    {
                        SocketState d = new SocketState(null, -1);

                        System.Diagnostics.Debug.WriteLine("Invalid addres: " + hostName);
                        return d;
                    }
                }
                catch (Exception e1)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    System.Diagnostics.Debug.WriteLine("using IP");
                    ipAddress = IPAddress.Parse(hostName);
                }

                // Create a TCP/IP socket.
                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                
                theServer = new SocketState(socket, 0);

                //Save CallBack Delegate for connection established..                            
                theServer.callBack = connecEst;

                theServer.theSocket.BeginConnect(ipAddress, DEFAULT_PORT, ConnectedToServer, theServer);

               // theServer.theSocket.Close();
                return theServer;

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                return null;
            }
        }


        /// <summary>
        /// This function is "called" by the operating system when the remote site acknowledges connect request
        /// </summary>
        /// <param name="ar"></param>
        public static void ConnectedToServer(IAsyncResult ar)
        {
            SocketState ss = (SocketState)ar.AsyncState;

            try
            {
                // Complete the connection.
                ss.theSocket.EndConnect(ar);

                //Send to the callback.
                ss.callBack(ss);

                //Gets data from the server as it arrives.
                AwaitDataFromServer(ss);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);

                IPAddress ipAddress = IPAddress.None;
                Socket tempsock = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //I am not sure it is a good idea to send null back, but it is the only way I can  think of checking for failed connect right now.
                //This indicates failure connecting. (ID:-1 and null socket for failure)
                SocketState d = new SocketState(tempsock, -1);

                //Send to the client our failed socketstate. (In the snake client case this should be first contact...)
                ss.callBack(d);

                return;
            }




        }


        /// <summary>
        /// A function used when we receive a callback and can begin reading data sent
        /// </summary>
        /// <param name="ar"></param>
        public static void ReceiveCallback(IAsyncResult ar)
        {
            SocketState ss = (SocketState)ar.AsyncState;

            try
            {
                

                //Ends the reading operation so we can save it and process the data.
                int bytesRead = ss.theSocket.EndReceive(ar);


                // If the socket is still open
                if (bytesRead > 0)
                {
                    //Save the data for processing.
                    string theMessage = Encoding.UTF8.GetString(ss.messageBuffer, 0, bytesRead);
                    // Append the received data to the growable buffer.
                    // It may be an incomplete message, so we need to start building it up piece by piece
                    ss.sb.Append(theMessage);

                    //Send to the callback.
                    ss.callBack(ss);
                }
                else
                {
                    ss.theSocket.Disconnect(false);
                    ss.shutdownSocket();
                    ss.callBack = null;

                    return;
                }

                // Continue the "event loop" that was started on line 154.
                // Start listening for more parts of a message, or more new messages
                //ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, ReceiveCallback, ss);
            }
            catch(Exception ex)
            {

                //Console.WriteLine("Receive + " + ex.Message);
                //IPAddress ipAddress = IPAddress.None;
                //Socket tempsock = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //SocketState d = new SocketState(tempsock, -1);

                //Send to the client our failed socketstate. (In the snake client case this should be first contact...)
                //ss.callBack(d);

                ss.shutdownSocket();


            }
            

        }

        
        /// <summary>
        /// This starts an event loop to continuously listen for messages from the server.
        /// </summary>
        /// <param name="ss">The state representing the server connection</param>
        public static void AwaitDataFromServer(SocketState ss)
        {
            try
            {


                // Start listening for a message
                // When a message arrives, handle it on a new thread with ReceiveCallback
                ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, new AsyncCallback(NetworkHandler.ReceiveCallback), ss);

            }
            catch(Exception ex)
            {

                Console.WriteLine("AwaitDataFromServer Exception");
                SocketState d = new SocketState(null, -1);

                //Send to the client our failed socketstate. (In the snake client case this should be first contact...)
                ss.callBack(d);


            }


        }

        /// <summary>
        /// Used to begin sending data by client
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void Send(SocketState socket, string data)
        {
            //try catch, will help indicate if a server randomly disconnected.
            try
            {
                //Save the given data to a byte and send it
                byte[] totalByte = Encoding.UTF8.GetBytes(data);
                socket.theSocket.BeginSend(totalByte, 0, totalByte.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }
            catch(Exception e)
            {

                Console.WriteLine("Send Exception");
                if (socket.getFailStatus() == true)
                {
                    socket.shutdownSocket();
                }
                else
                {
                    
                }
            }
        }
        

     
        /// <summary>
        /// A callback invoked when a send operation completes
        /// </summary>
        /// <param name="ar"></param>
        public static void SendCallback(IAsyncResult ar)
        {
            SocketState ss = (SocketState)ar.AsyncState;
            // Nothing much to do here, just conclude the send operation so the socket is happy.
            ss.theSocket.EndSend(ar);
        }



    }


}
