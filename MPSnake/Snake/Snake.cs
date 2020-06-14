using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Drawing;
using SnakeUtilities;
using System.Windows.Forms;
using System.Drawing.Drawing2D;


//Authors: Michael Tata

namespace SnakeUtilities
{
    /// <summary>
    /// A representation of a snake for the Snake Game.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Snake
    {
        /// <summary>
        /// snakes unique ID
        /// </summary>
        [JsonProperty]
        private int ID;

        /// <summary>
        /// Name of the snake
        /// </summary>
        [JsonProperty]
        private string name;



        /// <summary>
        /// Used to generate random position for snake
        /// </summary>
        private static Random randomGen = new Random();

      

        //Sets direction snake is going, defautled to 0 so all snakes move upward at start.
        /// <summary>
        /// used to determine snakes current heading
        /// </summary>
        private int direction = 1;


        /// <summary>
        /// Used by snake class to determine whether it should be growing or not /// may hvae to change this to public
        /// </summary>
        private bool grow = true;

        
        

        /// <summary>
        /// Notifies snake to grow by a cell.
        /// </summary>
        public void beginGrow()
        {
            grow = true;

        }

        // Implement Cell
        //Might have to make a getter for this and make it private. I am not sure how this will work with drawing yet though
        /// <summary>
        /// Keeps track of all vertices where the snake is turning. So if it is a line there will be two cells for the head and the tail.
        /// </summary>
        [JsonProperty(PropertyName = "vertices")]
        public List<Cell> segments;

        /// <summary>
        /// returns the Cell representing the head of the snake
        /// </summary>
        /// <returns></returns>
        public Cell getHead()
        {
            return segments[segments.Count - 1];
        }

        /// <summary>
        /// returns the CEll representing the tail of the snake
        /// </summary>
        /// <returns></returns>
        public Cell getTail()
        {
            return segments[0];
        }
       
        /// <summary>
        /// Used to create a new snake object. Requires its id, name, and a list of turns from tail to head.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nam"></param>
        /// <param name="snake"></param>
        [JsonConstructor]
        public Snake(int id, string nam, List<Cell> snake)
        {
                     
            segments = snake;
            name = nam;
            ID = id;

        }


        /// <summary>
        /// Returns the JSON serialized snake
        /// </summary>
        /// <returns></returns>
        public string getJSON()
        {
            
                return JsonConvert.SerializeObject(this);
           
        }


        /// <summary>
        /// Used to determine the length(for score, and maybe zoom later)
        /// </summary>
        /// <returns></returns>
        public int getLength()
        {
            int totalLen = 0;
            int xLen = 0;
            int yLen = 0;

            for(int i = 0; i < segments.Count -1; i++)
            {
              xLen = Math.Abs(segments[i].getXCoord() - segments[i + 1].getXCoord());
              yLen = Math.Abs(segments[i].getYCoord() - segments[i + 1].getYCoord());

              totalLen += xLen + yLen;


            }
            return totalLen;
        }

      
        /// <summary>
        /// Signify that the snake has died by setting its tail to -1, -1
        /// </summary>
        public void killSnake()
        {
            segments = null;
            segments = new List<Cell>();
            segments.Add(new Cell(-1, -1));
            segments.Add(new Cell(-1, -1));

        }

        /// <summary>
        /// Used when drawing to determine if the snake should be drawn, as if it is dead we should not pay attention to it.
        /// </summary>
        /// <returns></returns>
        public bool isSnakeAlive()
        {
            if(this.segments[0].getXCoord() >= 0)
            {
                return true;
            }
            return false;

        }
            
        /// <summary>
        /// Returns the ID of the snake.
        /// </summary>
        /// <returns></returns>
        public int getID()
        {
            return this.ID;
        }


        /// <summary>
        /// Returns the name of the snake
        /// </summary>
        /// <returns></returns>
        public string getName()
        {
            return this.name;
        }

        /// <summary>
        /// Returns the snake direction //Unused 
        /// </summary>
        /// <returns></returns>
        public int getDir()
        {
            return this.direction;
        }


        /// <summary>
        /// Sets the new direction, disallows requests that are of current direction, or of opposite direction. 
        /// </summary>
        /// <param name="dir"></param>
        public void setDir(int dir)
        {
           
            if ((Math.Abs(direction - dir) == 2 || Math.Abs(direction - dir) == 0) && segments.Count >= 2)
                return;

            direction = dir;
        }
        

        /// <summary>
        /// Determines if the head of this snake collides with another snake
        /// Note: the other snake may be itself
        /// Used by the server only
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool collidesWithSnake(Snake other, bool selfcheck)
        {
            Cell tempHead = getHead();

            int i = 0;
            if(selfcheck)
            {
                //If we want to add self collision, add it here. 
                return false; 
            }

            for (; i < other.segments.Count-1; i++)
            {

                if(other.segments[i].getXCoord() == other.segments[i+1].getXCoord())
                {
                    int lower = -1;
                    int upper = -1;


                    if(other.segments[i].getYCoord() < other.segments[i+1].getYCoord())
                    {
                        lower = other.segments[i].getYCoord();
                        upper = other.segments[i + 1].getYCoord();
                    }
                    else
                    {
                        lower = other.segments[i + 1].getYCoord();
                        upper = other.segments[i].getYCoord();
                    }

                    if(tempHead.getXCoord() == other.segments[i].getXCoord() && (tempHead.getYCoord() <= upper && tempHead.getYCoord() >= lower))
                    {
                        return true;
                    }


                }
                else if(other.segments[i].getYCoord() == other.segments[i + 1].getYCoord())
                {
                    int lower = -1;
                    int upper = -1;

                    if (other.segments[i].getXCoord() < other.segments[i + 1].getXCoord())
                    {
                        lower = other.segments[i].getXCoord();
                        upper = other.segments[i + 1].getXCoord();
                    }
                    else
                    {
                        lower = other.segments[i + 1].getXCoord();
                        upper = other.segments[i].getXCoord();
                    }

                    if (tempHead.getYCoord() == other.segments[i].getYCoord() && (tempHead.getXCoord() <= upper && tempHead.getXCoord() >= lower))
                    {
                        return true;
                    }

                }
                

            }

            return false;
        }

      


        /// <summary>
        /// Grows the snake from the tail outward, depending on given direction.
        /// This function is also used to simply move the snake
        /// </summary>
        public void GrowSnake(int mode)
        {           
            if(this.isSnakeAlive())
            {
                //Add new vertex if we have changed direction
                if(directionToVertex(segments[segments.Count - 2], segments[segments.Count-1]) != direction)
                {
                    segments.Add(new Cell(segments[segments.Count - 1].getXCoord(), segments[segments.Count - 1].getYCoord()));
                }

                //Move head forward.
                segments[segments.Count - 1].MoveCell(direction);


                

                
                //If we aren't growing move tail
                if(grow == false)
                {
                    //Move our tail towards the next vertex.
                    segments[0].MoveCell(directionToVertex(segments[0], segments[1]));
                }
              
                //If our tail has reached the next vertex, delete it from list. 
                if(segments[0].getXCoord() == segments[1].getXCoord() && segments[0].getYCoord() == segments[1].getYCoord())
                {
                    segments.RemoveAt(0);
                }

                //Stop growing. 

                if(mode == 0)
                    grow = false;
            }
            else
            {
                return;
            }

            
           
        }

     

        /// <summary>
        /// Finds the direction to a point on the plane, specifically used for snake movement
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public int directionToVertex(Cell source, Cell target)
        {
            //Determine whether target x is to the left or right 
            int xVal = target.getXCoord() - source.getXCoord();

            //Determine whether target y is up or down.
            int yVal = target.getYCoord() - source.getYCoord();

          
            if (yVal < 0)
            {
                return 1;
            }
      
            else if (xVal > 0)
            {
                return 2;
            }
   
            else if (yVal > 0)
            {
                return 3;
            }
        
            else if (xVal < 0)
            {
                return 4;
            }

            //should not be reached..
            return 0;
        }



        /// <summary>
        /// Method to draw an individual snake. Takes the color of the snake and the paint argument. 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="d"></param>
        public void drawSnake(PaintEventArgs e, Color d)
        {
            

            //Don't draw the snake if it is dead... But the more that I think about it I don't think we will ever have to worry about this, cause we remove dead snakes in form right when we get them.
            if(isSnakeAlive())
            {
                using (System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(d))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (Pen ourPen = new Pen(drawBrush))
                    {
                        ourPen.Width = 5;
                        ourPen.Alignment = PenAlignment.Center;
                       
                        for (int i = 0; i < segments.Count - 1; i++)
                        {


                            Cell beginp = new Cell(segments[i].getXCoord(), segments[i].getYCoord());
                            Cell endp = new Cell(segments[i + 1].getXCoord(), segments[i + 1].getYCoord());

                            
                            //Rectangle fillseg = new Rectangle(beginp.getXCoord(), beginp.getYCoord(), endp.getXCoord(), endp.getYCoord());

                            
                            GraphicsPath des = new GraphicsPath();

                            des.FillMode = FillMode.Winding;

                            //Adding 1.7float because of the stupid pixel offset when drawing lines, was hard to determine if you were hitting food or not with it. Now with the 1.7 correction it works great.
                            des.AddLine((beginp.getXCoord() * 5)+1.7f, (beginp.getYCoord() * 5)+1.7f, (endp.getXCoord() * 5)+1.7f, (endp.getYCoord() * 5)+1.7f);
                            

                            //Draw the snake path and fill it. 
                            e.Graphics.DrawPath(ourPen, des);
                            e.Graphics.FillPath(drawBrush, des);


                









                        }



                    }
                    
                }





            }
            else
            {
                return;
            }




        }

    }
}
