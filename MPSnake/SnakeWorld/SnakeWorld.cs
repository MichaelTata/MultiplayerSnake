using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;


//Authors: Michael Tata
//Josh Vidmar



namespace SnakeUtilities
{

    /// <summary>
    /// A world used to track, draw, and hold all materials for Snakes and food for Snake. 
    /// </summary>
    public class World
    {
        // Determines the size in pixels of each grid cell in the world
        /// <summary>
        /// The designated amount of pixels per cell or point
        /// </summary>
        public const int pixelsPerCell = 5;

        /// <summary>
        /// Default world size
        /// </summary>
        public const int DEFAULT_SIZE = 150;

        /// <summary>
        /// Default snake size
        /// </summary>
        public const int DEFAULT_SNAKE = 10;

        /// <summary>
        /// Used to keep track of snakes
        /// </summary>
        private Dictionary<int, Snake> allSnakes;

        /// <summary>
        /// Used to keep track of food
        /// </summary>
        private Dictionary<int, Food> allFood;

        //Putting this here rather than in snake class so we don't have a unnecessary dictionary for each snake object.
        /// <summary>
        /// Used to keep track of each snakes color
        /// </summary>
        private Dictionary<int, Color> snakeColors = new Dictionary<int, Color>();

        /// <summary>
        /// Simply used to generate a random color
        /// </summary>
        private static Random randomGen = new Random();

        /// <summary>
        /// Used to keep track of all food entered in teh world
        /// </summary>
        private int foodID = 0;

        /// <summary>
        /// Used to keep track of all Snakes entered in the world
        /// </summary>
        private int snakeID = 0;

        /// <summary>
        /// Used to save the world recycle rate, which determines how much of a snake is turned into food on death
        /// </summary>
        private double recycleRate;

        /// <summary>
        /// used to determine the food density, specifically how much food per snake in world.
        /// </summary>
        private int foodDensity;

        /// <summary>
        /// Used to determine whether tron mode is activated 
        /// </summary>
        private int gameMode;

        /// <summary>
        /// Assigns Colors to all snakes
        /// </summary>
        public void AssignColor(Snake snake)
        {

            Color randomColor = Color.FromArgb(randomGen.Next(256), randomGen.Next(256), randomGen.Next(256)); 

            snakeColors.Add(snake.getID(), randomColor);
  
        }

        /// <summary>
        /// returns recycle rate that was given to world
        /// </summary>
        /// <returns></returns>
        public double GetRecycle()
        {
            return recycleRate;
        }

        /// <summary>
        /// Returns density of food that was given to world.
        /// </summary>
        /// <returns></returns>
        public int GetDensity()
        {
            return foodDensity;
        }



        // Width of the world in cells (not pixels)
        /// <summary>
        /// Property for the width of the world.
        /// </summary>
        public int width
        {
            get;
            private set;
        }

        // Height of the world in cells (not pixels)
        /// <summary>
        /// Property for the height of the world.
        /// </summary>
        public int height
        {
            get;
            private set;
        }

        /// <summary>
        /// Default constructor used to initialize the world
        /// </summary>
        public World()
        {
            width = World.DEFAULT_SIZE;
            height = World.DEFAULT_SIZE;

            allSnakes = new Dictionary<int, Snake>();
            allFood = new Dictionary<int, Food>();

            
        }


        /// <summary>
        /// A function used to get and set snakes.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int,Snake> getAllSnakes()
        {
            return allSnakes;
        }

        /// <summary>
        /// A function used to get and set food
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Food> getAllFood()
        {
            return allFood;
        }

        /// <summary>
        /// A parameterized constructor that takes the width and height of the world
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public World(int w, int h)
        {
            width = w;
            height = h;

            allSnakes = new Dictionary<int, Snake>();
            allFood = new Dictionary<int, Food>();

        }

        /// <summary>
        /// A parameterized constructor that takes the width and height of the world
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public World(int w, int h, double recy, int dens, int mode)
        {
            width = w;
            height = h;

            recycleRate = recy;

            gameMode = mode;

            foodDensity = dens;

            allSnakes = new Dictionary<int, Snake>();
            allFood = new Dictionary<int, Food>();

            

        }






        /// <summary>
        /// Helper method for DrawingPanel
        /// Given the PaintEventArgs that comes from DrawingPanel, draw the contents of the world on to the panel.
        /// </summary>
        /// <param name="e"></param>
        public void Draw(PaintEventArgs e, int width, int height)
        {
            //Used to keep track of names on scoreboard and align them.
            int startpixel = 5;
            //Used to save persons name followed by their score, and then used to display
            string snakeScore;

            //Font for displaying the string on our drawing panel
            Font font = new Font(FontFamily.GenericSansSerif, 15, FontStyle.Bold);

            using (System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(Color.Black))
            {

               // StringBuilder scoreboard = new StringBuilder();

                //Display each food object
                foreach(Food food in getAllFood().Values)
                {


                    food.drawFood(e);

                }


                
                //Display each snake and each snakes score.
                foreach(Snake snake in getAllSnakes().Values)
                {
                    
                    //If the snake has no color yet, assign it one. otherwise draw the snake with designated color.
                    if (snakeColors.ContainsKey(snake.getID()))
                    {
                        snake.drawSnake(e, snakeColors[snake.getID()]);

                    }
                    else
                    {
                        AssignColor(snake);

                    }


                    //Save snake color to use for scoreboard, save the snakes name and score, then display. 
                    drawBrush.Color = snakeColors[snake.getID()];
                    snakeScore = snake.getName() + ":" + snake.getLength();

                    //Draw the name with designated color and score.
                    e.Graphics.DrawString(snakeScore, font, drawBrush, width - 200, startpixel);

                    //Start pixel is used to seperate names onto new lines on the scoreboard. It is not the most efficient way, but it works and it will grow continuously.
                    startpixel += 20;

                }
           
            }
        }


        /// <summary>
        /// Updates the world frame by frame, refreshing snake position and food. Updates whether food was eaten, whether a snake collides with anything and dies.. etc...
        /// </summary>
        public void Update()
        {
         

            //Go throuch each snake we have saved in our dictioanry
            foreach(Snake snake in allSnakes.Values)
            {
                //Check each food in the world and see if it is being eaten by the current snake. 
                foreach (Food food in allFood.Values)
                {
                    if (snake.getHead().getXCoord() == food.getLoc().getXCoord() && snake.getHead().getYCoord() == food.getLoc().getYCoord())
                    {
                
                        food.getsEaten();
                        snake.beginGrow();
                    }




                }

                //If snake is alive check for collision and kill it if so.
                if (snake.isSnakeAlive())
                {
                    //Check collision with food or wall or snake.

                    
                    //Kill the snake if it is colliding with a wall.
                    if(snake.getHead().getXCoord() == 0 || snake.getHead().getXCoord() >= this.width -1 || snake.getHead().getYCoord() == 0 || snake.getHead().getYCoord() >= this.height-1)
                    {
                        
                      
                        snake.killSnake();
                    }


                    //if snake collides with itself or other snake here

                    foreach(Snake tempsnake in allSnakes.Values)
                    {
                        bool check = false;

                        if(tempsnake == snake)
                        {
                            check = true;
                        }

                        if (snake.collidesWithSnake(tempsnake, check))
                        {
                            //Console.WriteLine("Snake has died.");
                            snake.killSnake();
                        }
                    }

                    
                    

                    snake.GrowSnake(gameMode);
                }
                

            }

            if (gameMode == 0)
            {
                //Add food until we have the correct amount in relation to each snake.
                while (allFood.Count < this.allSnakes.Count * foodDensity)
                {
                    AddRandomFood();

                }
            }

        }

        /// <summary>
        /// Used to delete all food and snakes that have died. 
        /// </summary>
        public List<int> Delete()
        {
            List<Food> deleteFood = allFood.Values.ToList();
            List<Snake> deleteSnake = allSnakes.Values.ToList();

            List<int> died = new List<int>();

            //Delete all food in the world that is dead.
            foreach(Food food in deleteFood)
            {
                if(!allFood[food.getID()].isFoodAlive())
                {
                    allFood.Remove(food.getID());
                }

            }

            foreach(Snake snake in deleteSnake)
            {
                if(!allSnakes[snake.getID()].isSnakeAlive())
                {

                    allSnakes.Remove(snake.getID());
                    died.Add(snake.getID());
                  


                }
            }


            return died;

        }


        /// <summary>
        /// Adds a new snake to the world in a random starting position. 
        /// </summary>
        public Snake AddNewSnake(string newSnake)
        {
            //make sure we don't spawn on another snake here, or maybe we could allow it..
            Cell snakeTail;
            Cell snakeHeadTEMP;
            List<Cell> tempSnake = new List<Cell>();

            
            snakeTail = new Cell(randomGen.Next(width - 50), randomGen.Next(height - 50));
            tempSnake.Add(snakeTail);

            snakeHeadTEMP = new Cell(snakeTail.getXCoord(), snakeTail.getYCoord() + DEFAULT_SNAKE);
                                                 
            tempSnake.Add(snakeHeadTEMP);

            Snake nextSnake = new Snake(snakeID, newSnake, tempSnake);
                      
            allSnakes.Add(snakeID, nextSnake );
            snakeID++;
            return nextSnake;

        }


        public void RespawnSnake()
        {

        }




        /// <summary>
        /// Adds a  food in a random location to the world. 
        /// </summary>
        public void AddRandomFood()
        {

            Cell newfood;

            newfood = new Cell(randomGen.Next(width - 1), randomGen.Next(height - 1));

            foodID++;

            allFood.Add(foodID, new Food(foodID, newfood));

        }


      

    }





    // For server we are going to have to add functions to add new snakes, add new food, in random positions on the board. 
    //Also will have to write function to determine how much food we leave behind and snake collisions and everything. 



    





}
