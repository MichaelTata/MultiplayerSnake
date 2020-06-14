using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using SnakeUtilities;
using System.Net.Sockets;

//Author:Michael Tata


namespace SnakeUtilities
{
    /// <summary>
    /// Used to package info in the world and initialize the snake world
    /// Had to create this to get around static methods and instantiate the world.
    /// </summary>
    public struct SnakeParams
    {
        public int height;
        public int width;
        public int frame;
        public int density;
        public double recycle;
        public int mode;
    }

    /// <summary>
    /// A server which handles all interactions and client alert in our snake game. 
    /// </summary>
    class Server
    {
             
        private const string SETTINGS_FILE = "settings.xml";

        private static System.Timers.Timer aTimer ;

        public static LinkedList<NetworkController.SocketState> connectedClients;

        public static SnakeUtilities.World world;


        /// <summary>
        /// Entry point for our server. 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("BEGIN");
            //Begin the server process by reading in the file.
            ReadInFile();
                         

        }

        /// <summary>
        /// Used to read in XML file and begin world
        /// </summary>
        private static void ReadInFile()
        {
            //Used to keep track of each new client.
            connectedClients = new LinkedList<NetworkController.SocketState>();

            //Used to package information about the world as we cant in a static class
            SnakeParams worldData = new SnakeParams();

            //If we failed to read in the file, just exits.
            if (!ReadSettings(ref worldData))
            {
                Console.WriteLine("Failed.");
                return;
            }

            //Declare the world and begin the server.
            world = new World(worldData.width, worldData.height, worldData.recycle, worldData.density, worldData.mode);
            BeginServer(worldData);

            Console.WriteLine("Awaiting Clients");
            Console.ReadLine();

        }

        /// <summary>
        /// Used to begin the server and start the refresh timer.
        /// </summary>
        private static void BeginServer(SnakeParams data)
        {
            //Begin timer and declare our elapsed timer event handler, which is sending the world each frame interval.
            aTimer = new System.Timers.Timer();
            aTimer.Enabled = true;
            aTimer.Interval = data.frame;
            
            aTimer.Elapsed += new ElapsedEventHandler(SendWorld);

            aTimer.Start();

            
            //Start the server.
            NetworkController.NetworkHandler.StartServer(HandleNewClient);

        }


        /// <summary>
        /// Reads the settings for the game in the XML File.
        /// </summary>
        private static bool ReadSettings(ref SnakeParams data)
        {
            //Check if version passed matches version in file, else we have to throw an error
            XmlReader reader = null;
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;


            string tempWidth = "";
            string tempHeight = "";
            string tempFrame = "";
            string tempRecycle = "";
            string tempDensity = "";
            string tempMode = "";

            int resultWidth;
            int resultHeight;
            int resultFrame;
            int resultDensity;
            double resultRec;
            int resultMode;


            try
            {
                using (reader = XmlReader.Create(SETTINGS_FILE, settings))
                {
                    while (reader.Read())
                    {

                        //Assures what we read in are start elements.
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "SnakeSettings":
                                    break;

                                case "BoardWidth":
                                    if (reader.Read())
                                    {
                                        tempWidth = reader.Value;
                                    }
                                    else
                                    {

                                    }
                                    break;

                                case "BoardHeight":
                                    if (reader.Read())
                                    {
                                        tempHeight = reader.Value;
                                    }

                                    break;

                                case "MSPerFrame":
                                    if (reader.Read())
                                    {
                                        tempFrame = reader.Value;
                                    }


                                    break;

                                case "FoodDensity":
                                    if (reader.Read())
                                    {
                                        tempDensity = reader.Value;
                                    }

                                    break;

                                case "SnakeRecycleRate":
                                    if (reader.Read())
                                    {
                                        tempRecycle = reader.Value;
                                    }
                                    break;
                                case "Mode":
                                    if(reader.Read())
                                    {
                                        tempMode = reader.Value;
                                    }                                   
                                    break;

                            }


                        }


                    }

                }

                //Parse into respective values and initialize world.  
                if (int.TryParse(tempFrame, out resultFrame))
                {
                   
                }
                else
                {
                    return false;
                }

                if(int.TryParse(tempHeight, out resultHeight))
                {

                }
                else
                {
                    return false;
                }

                if(int.TryParse(tempWidth, out resultWidth))
                {

                }
                else
                {
                    return false;
                }

                if(int.TryParse(tempDensity, out resultDensity))
                {

                }
                else
                {
                    return false;
                }

                if(double.TryParse(tempDensity, out resultRec))
                {


                }
                else
                {
                    return false;
                }

                if(int.TryParse(tempMode, out resultMode))
                {

                }
                else
                {
                    return false;
                }

                //Here change the values in the struct and then we can use them in main.
                data.density = resultDensity;
                data.height = resultHeight;
                data.width = resultWidth;
                data.frame = resultFrame;
                data.recycle = resultRec;
                data.mode = resultMode;

            }
            catch(Exception)
            {
                //Failed to read data in from file
                return false;
            }
            



            return true;
        }


        /// <summary>
        /// Triggered at each specified timer interval, used to signal refresh to client.
        /// </summary>
        private static void SendWorld(object sender, EventArgs e)
        {
            List<int> died = null;
            //Using string builder to easily add the messages to the buffer and then send it when we have serialized everything..
            StringBuilder sendInfo = new StringBuilder();
            //Lock so we can send the world 
            lock(world)
            {
                world.Update();

                //Send each snake
                foreach (Snake snake in world.getAllSnakes().Values)
                {
                    //Send the JSON serialized snake object
                    sendInfo.Append(snake.getJSON() + "\n");

                   
                   // Console.WriteLine(sendInfo.ToString());
                }

                //Send each food
                foreach (Food food in world.getAllFood().Values)
                {
                    //Send the JSON serialized food object
                    sendInfo.Append(food.getJSON() + "\n");
                  

                }
                


                //Update world, which will handle snake and food updating.
                

                //Delete dead food/snakes
                died = world.Delete();

                


            }


            // Console.WriteLine(sendInfo.ToString());

           
         
                LinkedListNode<NetworkController.SocketState> clients = connectedClients.First;
                while (clients != null)
                {
                    StringBuilder temp = new StringBuilder();
                    try
                    {
                        if (died.Count != 0)
                        {
                            temp.Append(sendInfo.ToString());

                            foreach (int ids in died)
                            {

                                temp.Append("{\"ID\":" + ids +",\"rem\":true}\n");


                            }
                            

                            foreach (int id in died)
                            {
                              if(clients.Value.getID() == id)
                              {
                                


                                
                                temp.Append("!DIED!\n");
                                Console.WriteLine("STRING:" + temp.ToString());

                                break;
                                


                              }
                            }
                        }
                        else
                        {
                            temp.Append(sendInfo.ToString());
                        }

                    NetworkController.NetworkHandler.Send(clients.Value, temp.ToString());
                    clients = clients.Next;



                    }
                    catch (Exception )
                    {
                        clients = clients.Next;


                    }
                
            }
        
        }

        /// <summary>
        /// Sends the start up information of the world to the client.
        /// </summary>
        private static void SendStartUp(NetworkController.SocketState state)
        {

            
            state.callBack = ProcessMessage;

            //Sends the world to the current socket
            NetworkController.NetworkHandler.Send(state, state.getID() + "\n" + world.width + "\n" + world.height + "\n");

            

            Console.WriteLine("Sending startup info...");
        }

        /// <summary>
        /// Handles when a new client connects by beginning recieve name
        /// </summary>
        /// <param name="state"></param>
        private static void HandleNewClient(NetworkController.SocketState state)
        {
            
                state.callBack = ReceiveName;
                NetworkController.NetworkHandler.AwaitDataFromServer(state);
            
        }


        /*: it is important that the server sends the startup info before adding the client to the list of all clients.
         *  This guarantees that the startup info is sent before any world info. Remember that the server is running a timer that may send world info to the list of clients at any time.
         */


        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private static void ReceiveName(NetworkController.SocketState state)
        {
            //
            Socket getData = state.theSocket;

            

            state.callBack = (SendStartUp);


            Console.WriteLine(state.getID());

            //This will only work for testing the FIRST client. We will have to change to something that determines the name among other possible messages.....
            string name = state.sb.ToString();

            //create the new player and add him to the world.
            Snake player;
            //Locking this up to try and fix race cond
            lock (world)
            {
              
                player = world.AddNewSnake(name);
            }
            state.setID(player.getID());

            SendStartUp(state);

           // Console.WriteLine(name);
            //Also locking to fix bug with two clients trying to connect.
            lock (connectedClients)
            {
                connectedClients.AddLast(state);
            }
           // Console.WriteLine(state.getID());
            //Again, will have to change this to send player specific ID.
           

          

            NetworkController.NetworkHandler.AwaitDataFromServer(state);


        }

        /// <summary>
        /// Used to read client input and react accordingly. 
        /// </summary>
        private static void ProcessMessage(NetworkController.SocketState state)
        {
            //Save current ID to determine if we continue reading directions(If this ID snake is dead, do not read messages)
            int ID = state.getID();

            //Saves the input to our message
            string message = state.sb.ToString();

            //Used to seperate by newline
            string[] remove = { "\n" };

          
            //Used to save direction from our string
            char direction;

            //Used to parse into int for passing direction to our world
            int temp;

            //Split each command to move
            string[] theMsg = message.Split(remove, StringSplitOptions.RemoveEmptyEntries);

           
            
                  
            try
            {
                
                foreach(string dirC in theMsg)
                {
                    //Assure is a valid and complete direction request
                    if(dirC[0] == '(' && dirC[dirC.Length-1] == ')')
                    {
                        direction = dirC[1];


                        lock (world)
                        {
                            //   Console.WriteLine(direction);

                            if (world.getAllSnakes().ContainsKey(ID))
                            {

                                int.TryParse(direction.ToString(), out temp);

                              //  Console.WriteLine(temp);
                                //Move the snake in the given direction. 

                                world.getAllSnakes()[ID].setDir(temp);
                            //    Console.WriteLine(world.getAllSnakes()[ID].getDir());

                            }
                            else
                            {

                            }


                        }

                        
                    }
                    else if(dirC[0] == 'E' && dirC[1] == 'N' && dirC[2] == 'D')
                    {
                        state.sb.Clear();
                        state.callBack = null;

                        Console.WriteLine("STATE ID:" + state.getID());
                        Console.WriteLine("CONNECTED CLIENT ID:" + connectedClients.First.Value.getID());

                        NetworkController.SocketState tempdel = null;
                        foreach(NetworkController.SocketState el in connectedClients)
                        {
                            if(el.getID() == state.getID())
                            {
                                tempdel = el;
                            }
                        }

                        connectedClients.Remove(tempdel);
                        Console.WriteLine("SIZE:" + connectedClients.Count);

                    }
                    //Remove processed message from buffer.
                    state.sb.Remove(0, dirC.Length + 1);

                }

                
                if(state.getID() == -1)
                {
                }
                else
                {
                    NetworkController.NetworkHandler.AwaitDataFromServer(state);
                }
                

            }
            catch(Exception)
            {

            }



          
        }





        


    }
}
