using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetworkController;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using DrawingPanel;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



//Authors: Michael Tata


/// <summary>
/// This class will initialize the client form that the user will use to connect and play the snake game.
/// </summary>
namespace ClientGUI
{
    public partial class Snake : Form
    {
        /// <summary>
        /// Used to communicate with networkController and server.
        /// </summary>
        private SocketState HostServer;

        /// <summary>
        /// Used to make space for the scoreboard
        /// </summary>
        public const int SCB_OFFSET = 200;

        /// <summary>
        /// Used to default the world size to 150.
        /// </summary>
        public const int DEF_SIZE = 150;

        /// <summary>
        /// Our world object, used to keep track of, and draw every snake and food object.
        /// </summary>
        private SnakeUtilities.World world;

        


        // these 3 functions are the callbacks we will use to process data 
        /// <summary>
        /// call back used for first connection
        /// </summary>
        /// <param name="state"></param>
        private void firstContact(SocketState state)
        {
            
            //If we failed to connect, give error message here. else set up the first bit of communication
            if (state.getID() == -1 || state.theSocket == null)
            {

                           
                    //This doesn't show up. Need to fix this 
                    MessageBox.Show("Connection Failed. Please Re-Enter Host-Name", "Could not reach Host", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                    this.Invoke((Action)(() =>
                    {
                        NameTextBox.Enabled = true;

                        ServerIPTextBox.Enabled = true;

                        ConnectButton.Enabled = true;


                    }));
                return;
        
            }
            else if(!state.theSocket.Connected)
            {
                //This doesn't show up. Need to fix this 
                MessageBox.Show("Connection Failed. Please Re-Enter Host-Name", "Could not reach Host", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                this.Invoke((Action)(() =>
                {
                    NameTextBox.Enabled = true;

                    ServerIPTextBox.Enabled = true;

                    ConnectButton.Enabled = true;


                }));
                return;
            }
            else
            {
                Console.WriteLine("server connection");

                //Change callback to recieve the initial bit of our world and our given ID by server
                state.callBack = InitialReceive;
                //connect to given hostname and give our name to server
                NetworkController.NetworkHandler.Send(HostServer, NameTextBox.Text);

                              
            }



        }

        /// <summary>
        /// Used to recieve the first instructions from the server, specifically what to build in the world.
        /// </summary>
        /// <param name="state"></param>
        private void InitialReceive(SocketState state)
        {
            //I still need to figure out how to give a error message if the connection failed. This does not work
            if (state.getID() == -1 || state.theSocket == null)
            {
                MessageBox.Show("Connection with host was lost.", "Connection Failure", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                this.Invoke((Action)(() =>
                {
                    NameTextBox.Enabled = true;

                    ServerIPTextBox.Enabled = true;

                    ConnectButton.Enabled = true;

                    return;

                }));
            }

            try
            {


                //Used to change how big the world is based on what the server tells us
                int width = 0;
                int height = 0;
                int ourID = 0;

                Console.WriteLine("Server sent data. " + state.sb);

                //Parse the message here, get the id, get the width, then get the heigth and then mvoe on to receiving all data from the world.

                string[] theMsg = state.sb.ToString().Split('\n');

                // Console.WriteLine(":::" + theMsg[0] + "..." + theMsg[1] + theMsg[2]);

                //if we didn't get the whole message we should wait for more, so somehow check that we got id and world sizes.

                //Assures that we have recieved the initial data from the server, and that it is complete. Process it, and then remove it from the string builder.
                if (theMsg.Length >= 3 && state.sb.ToString()[state.sb.Length -1] == '\n') 
                {

                    ourID = int.Parse(theMsg[0]);

                   
                    state.sb.Remove(0, theMsg[0].Length);

                    width = int.Parse(theMsg[1]);

                    state.sb.Remove(0, theMsg[1].Length);

                    height = int.Parse(theMsg[2]);


                    state.sb.Remove(0, theMsg[2].Length);

                }
                else
                {
                    //If message is not complete, we wait for more data 
                    NetworkController.NetworkHandler.AwaitDataFromServer(state);
                    return;
                }


                Console.WriteLine("Our ID:" + ourID + " Our World Width and Height:" + width + "," + height);

                //Create new world based on the given h and w by server
                world = new SnakeUtilities.World(width, height);

                //Invoke so we can draw the new world on the panel

                //Need to add here how to resize the client window if the world size is too big, or maybe if it is even smaller we can resize it to scale.
                //Draw the new world with the designated size given.
                this.Invoke((Action)(() =>
                {
                  
                    drawingPanel.Size = new Size((world.width * SnakeUtilities.World.pixelsPerCell)+SCB_OFFSET, (SnakeUtilities.World.pixelsPerCell * world.height));
                    drawingPanel.SetWorld(world);

                    

                    //Size panelSize = drawingPanel.Size;
                    //int panWidth = panelSize.Width;
                    //int panHeight = panelSize.Height;
                    drawingPanel.Invalidate();



                }                   
                   
                ));

                //Change call back to receive world to continuously take data and then call receive world to begin.
                state.callBack = ReceiveWorld;
                ReceiveWorld(state);

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:" + e.Message);
            }
            



            
        }



        private void empty(SocketState state)
        {


            return;
        }

        /// <summary>
        /// Used to continually recieve data, after everything has been loaded in and drawn initially. 
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveWorld(SocketState state)
        {

            //If the conncetion was lost
            if(state.getID() == -1 || state.theSocket == null)
            {
                MessageBox.Show("Connection with host was lost.", "Connection Failure", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                this.Invoke((Action)(() =>
                {
                    NameTextBox.Enabled = true;

                    ServerIPTextBox.Enabled = true;

                    ConnectButton.Enabled = true;


                }));
                return;
            }


            //save the string to read in our data
            string message = state.sb.ToString();

            SnakeUtilities.Snake newSnake = null;
            SnakeUtilities.Food newFood = null;

            bool worldChange = false;
         
            string jsObj = null;

            //Used to seperate each line into our message array.
            string[] remove = { "\n" };

          

            try
            {

               // Console.WriteLine(state.sb.ToString());

                //Split the message in the sb, and deserialize the json objects.
                string[] theMsg = message.Split(remove, StringSplitOptions.RemoveEmptyEntries);
                
                //used to determine how much we should read.
                int totalMsg = theMsg.Length;



                //If the end of the message is not complete, we do not want to process it yet.
                if(state.sb.ToString()[state.sb.Length-1] != '\n')
                {
                    totalMsg--;
                }

                //Loop through each obj in the message until complete.
                for (int iter = 0; iter < totalMsg; iter++)
                {
                    
                    
                   
                      
                    
                    jsObj = theMsg[iter];

                    //Console.WriteLine(jsObj);

                    //If the message is contained in braces, it is a valid Json 
                    if (jsObj[0] == '{' && jsObj[jsObj.Length - 1] == '}')
                    {
                        Console.WriteLine("JSON:" + jsObj);
                        //Parse the message and check for the attributes
                        JObject findAttr = JObject.Parse(jsObj);

                        JToken verts = findAttr["vertices"];

                        JToken foodloc = findAttr["loc"];

                        JToken removesnake = findAttr["rem"]; 

                        //Check if msg was a vertice or not
                        if (verts != null)
                        {
                            newSnake = JsonConvert.DeserializeObject<SnakeUtilities.Snake>(theMsg[iter]);
                        //    Console.WriteLine(theMsg[iter]);
                        }
                        //Check if msg was a food or not
                        if (foodloc != null)
                        {
                            
                            newFood = JsonConvert.DeserializeObject<SnakeUtilities.Food>(jsObj);
                         //   Console.WriteLine(theMsg[iter]);
                        }

                        if (removesnake != null)
                        {
                            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsObj);
                            string tid = dict["ID"];

                            int tempid; 

                            if(Int32.TryParse(tid, out tempid))
                            {
                                Console.WriteLine("Removing from client world:" + tempid);

                                if(world.getAllSnakes().ContainsKey(tempid))
                                    world.getAllSnakes()[tempid].killSnake();
                            }

                        }


                        
                        //Lock here due to drawing -- source of crazy race condition and weird bug with writing the json message to console.
                        lock (this.world)
                        {

                            if (newFood != null)
                            {
                                 worldChange = true;

                                //Add food or delete it from world
                                if (newFood.isFoodAlive())
                                {

                                    //Set in our food dictionary
                                    world.getAllFood()[newFood.getID()] = newFood;
                                }
                                else
                                {
                                    //Remove from food dictionary so it is no longer redrawn.
                                    world.getAllFood().Remove(newFood.getID());
                                }


                            }
                            if (newSnake != null)
                            {
                                worldChange = true;
                                //Check if snake is alive or dead, then set it accordingly
                                if (newSnake.isSnakeAlive())
                                {
                                    world.getAllSnakes()[newSnake.getID()] = newSnake;
                                }
                                else
                                {
                                    world.getAllSnakes().Remove(newSnake.getID());
                                }
                            }
                        }

     
                            //Only remove if the message was indeed processed correctly
                            state.sb.Remove(0, jsObj.Length+1);
                        

                    }
                    else
                    {
                        //Console.WriteLine("MSG RET:" + theMsg[iter]);

                        if (theMsg[iter].Equals("!DIED!"))
                        {
                            


                            this.Invoke((Action)(() =>
                            {
                                RespawnButton.Enabled = true;
                            }));


                        }

                        state.sb.Remove(0, theMsg[iter].Length + 1);

                    }
                    
                    

                }

                                    
           }
            catch(Exception e)
            {
                Console.WriteLine("EXCEPTION:" + e.Message);
            }

            //Only draw the panel if the world was changed in some form. Used specifically to not waste time invalidating the screen if the message was empty.
            if (worldChange)
            {
                this.Invoke((Action)(() =>
                {

                    
                    drawingPanel.Invalidate();

                }

                  ));
            }
            
          

           

            //Recieve more data.
            NetworkHandler.AwaitDataFromServer(state);
        }



        public Snake()
        {
            //initialize a world object with the default size of 150x150
            world = new SnakeUtilities.World(DEF_SIZE, DEF_SIZE);

            InitializeComponent();

            //Anchors the panel to the top left to handle any bugs on drawing when maximizing. 
            drawingPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top )
                    | System.Windows.Forms.AnchorStyles.Left));


            drawingPanel.SetWorld(world);

            drawingPanel.Size = new Size(world.width * SnakeUtilities.World.pixelsPerCell, world.height * SnakeUtilities.World.pixelsPerCell);

            ////Fake panel used solely to allow key input, when we have our drawing panel we will use that for the actual key press event. 
            drawingPanel.Focus();

            //Used to catch key presses.
            KeyPreview = true;

            //Handles resizing of window. Not sure how this will work when we add the scoreboard, but I don't think it will be too hard to figure out and work around. 
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AutoSize = true;

            //Defaults the client window to handle 150x150, until we get world size from server. 
            ClientSize = new Size(991, 800);
            StartPosition = FormStartPosition.CenterScreen;

            HostServer = null;

            this.KeyDown += new KeyEventHandler(Key_Press);

            this.FormClosing += Form1_FormClosing;




        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {


            DialogResult userChoice = MessageBox.Show("Are you sure you want to quit?", "Exit", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);

            if (userChoice == DialogResult.Yes)
            {

                
            }
            else if(userChoice == DialogResult.No || userChoice == DialogResult.Cancel)
            {


                e.Cancel = true;
            }


        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (HostServer != null)
            {
                NetworkController.NetworkHandler.Send(HostServer, "E" + "N" + "D\n");
                HostServer.setFailStatus(true);
                HostServer.callBack = null;
             
            }
        }


        private void RespawnButton_Click(object sender, EventArgs e)
        {

            Console.WriteLine("RESPAWN CHECKING....");

            HostServer = NetworkController.NetworkHandler.ConnectToServer(firstContact, ServerIPTextBox.Text);

            RespawnButton.Enabled = false;


            //HostServer.callBack = InitialReceive;
            //connect to given hostname and give our name to server
            //NetworkController.NetworkHandler.Send(HostServer, NameTextBox.Text);



        }



        /// <summary>
        /// Will attempt to connect to the server once pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            string host = ServerIPTextBox.Text;
            if (string.IsNullOrEmpty(host))
            {

                MessageBox.Show("Invalid Host address. Please Re-Enter", "Invalid Host", MessageBoxButtons.OK, MessageBoxIcon.Error,  MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                return;

            }

            string name = NameTextBox.Text;
            if (string.IsNullOrEmpty(name))
            {

                MessageBox.Show("Please enter a name with at least one character.", "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);

                return;
            }

            //Do not allow the client to reconnect continuously unless the connection failed. 
            ServerIPTextBox.Enabled = false;

            NameTextBox.Enabled = false;

            RespawnButton.Enabled = false;

            ConnectButton.Enabled = false;

            

            drawingPanel.Focus();


            try
            {
                //Begin Handhsake, send our function designed to handle first contact, and then the host IP/Url              
                HostServer = NetworkController.NetworkHandler.ConnectToServer(firstContact, host);

               

                


            }
            catch(Exception )
            {
                MessageBox.Show("Connection Failed. Please Re-Enter", "Could not reach Host", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);

            }
            




        }

        /// <summary>
        /// Handles Key Input for snake movement.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Key_Press(object sender, KeyEventArgs e)
        {


            //Only do this if snake is still alive, will have to change how we send when we switch to static network handler
            //Basic directions..
            if (e.KeyCode == Keys.Down)
            {
                Console.WriteLine("Arrow was pressed");
                NetworkController.NetworkHandler.Send(HostServer, "(" + "3" + ")\n");
            }
            else if (e.KeyCode == Keys.Up)
            {
                Console.WriteLine("Arrow was pressed");
                NetworkController.NetworkHandler.Send(HostServer, "(" + "1" + ")\n");
            }
            else if (e.KeyCode == Keys.Left)
            {
                Console.WriteLine("Arrow was pressed");
                NetworkController.NetworkHandler.Send(HostServer, "(" + "4" + ")\n");
            }
            else if (e.KeyCode == Keys.Right)
            {
                Console.WriteLine("Arrow was pressed");
                NetworkController.NetworkHandler.Send(HostServer, "(" + "2" + ")\n");
            }
            //closes the form
            else if(e.KeyCode == Keys.Escape || e.KeyCode == Keys.Q)
            {
                this.Close();
            }

        }
    }
}
     

















