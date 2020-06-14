using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Drawing;

//Authors: Michael Tata
//Josh Vidmar


namespace SnakeUtilities
{

    /// <summary>
    /// Defines a food object lying on a given plane
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Food
    {
        /// <summary>
        /// An id to recognize a specific food
        /// </summary>
        [JsonProperty]
        private int ID;

        /// <summary>
        /// A cell object used to determine the actual location on the plane
        /// </summary>
        [JsonProperty]
        private Cell loc;

        /// <summary>
        /// Food constructor, initializes the ID and location
        /// </summary>
        /// <param name="newID"></param>
        /// <param name="newFood"></param>
        public Food(int newID, Cell newFood)
        {
            this.loc = newFood;
            this.ID = newID;


        }


        /// <summary>
        /// Returns the json serialized food object
        /// </summary>
        /// <returns></returns>
        public string getJSON()
        {
           
                return JsonConvert.SerializeObject(this);
            
           
        }

        /// <summary>
        /// Returns the location of the food object
        /// </summary>
        /// <returns></returns>
        public Cell getLoc()
        {
            return this.loc;
        }

        /// <summary>
        /// Returns the ID of the food object.
        /// </summary>
        /// <returns></returns>
        public int getID()
        {
            return this.ID;

        }

        /// <summary>
        /// remove the food from the world by setting its loc to -1,-1
        /// </summary>
        public void getsEaten()
        {
            loc = new Cell(-1, -1);
        }
        
        /// <summary>
        /// Used to determine if the food should be drawn or not by checking if it is contained on the plane
        /// </summary>
        /// <returns></returns>
        public bool isFoodAlive()
        {
            if(this.loc.getXCoord() >= 0)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Draws the food in a particular cell
        /// </summary>
        public void drawFood(PaintEventArgs e)
        {

            //If the food is alive, draw it
            if (isFoodAlive())
            {
                using (System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(Color.Black))
                {

                    using (Pen ourPen = new Pen(drawBrush))
                    {
                        //I believe penalignment default is center anyways
                        ourPen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                        //Set width to 0.8
                        ourPen.Width = 0.8f;

                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        //Using constant 5 just for pixels per for now


                        Rectangle food = new Rectangle(loc.getXCoord() * 5, loc.getYCoord() * 5, 5, 5);

                        //This is where the source of the lag is coming from. This takes the most time, not sure how to work around it. maybe if it is possible to only redraw things that changed.
                        e.Graphics.FillEllipse(drawBrush, food);
                        e.Graphics.FillRectangle(drawBrush, food);
                       


                    }
                }
            }
            else { return; }
        }

    }



}

