using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

//Authors: Michael Tata
//Josh Vidmar



namespace SnakeUtilities
{
    /// <summary>
    /// A cell class which represents a point on a plane. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Cell
    {

        /// <summary>
        /// Represents the x-coordinate on a plane
        /// </summary>
        [JsonProperty]
        private int x;



        /// <summary>
        /// Represents the y-coordinate on a plane
        /// </summary>
        [JsonProperty]
        private int y;

     
        //I don't think we need direction for client

        /// <summary>
        /// Basic Constructor to set the cells coordinates, and defaults the direction to 0
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [JsonConstructor]
        public Cell(int x, int y)
        {
           this.x = x;
           this.y = y;

          
        }

    
        /// <summary>
        /// Moves the cell towards a vertex by one(specifically used for moving snake parts in given direction
        /// putting this in cell class rather than snake because it is easier to directly change a cell rather than create a new one 
        /// each time, even though this method only pertains to snake class.
        /// </summary>
        /// <param name="dir"></param>
        public void MoveCell(int dir)
        {   
            //Move up
            if(dir == 1 )
            {
                y = y - 1;
            }
            //Move right
            else if(dir == 2)
            {
                x = x + 1; 
            }
            //Move down
            else if(dir == 3)
            {
                y = y + 1;
       
            }
            //Move left
            else if(dir == 4)
            {
                x = x - 1;
            }

        }
        
       
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int getXCoord()
        {
            return x;

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int getYCoord()
        {
            return y;
        }

    }
}
