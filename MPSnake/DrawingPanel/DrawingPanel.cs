using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

//Authors: Michael Tata
//Josh Vidmar



namespace DrawingPanel
{
    /// <summary>
    /// A drawing panel used to draw a snake world and everything that it may contain.
    /// </summary>
    public class DrawingPanel : Panel
    {
        /// <summary>
        /// the snake world that we will call to draw.
        /// </summary>
        private SnakeUtilities.World world;

        /// <summary>
        /// Initialize the drawing panel by simply double buffering everything to prevent flickering.
        /// </summary>
        public DrawingPanel()
        {
            // Gets rid of potential flickering
            this.DoubleBuffered = true;
        }
        /// <summary>
        /// Pass in a reference to the world, so we can draw the objects in it
        /// </summary>
        /// <param name="_world"></param>
        public void SetWorld(SnakeUtilities.World _world)
        {
            world = _world;
        }

        /// <summary>
        /// Override the behavior when the panel is redrawn
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            lock (this.world)
            {
                // If we don't have a reference to the world yet, nothing to draw.
                if (world == null)
                    return;

                using (System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(Color.Black))
                {

                    // Turn on anti-aliasing for smooth round edges
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    // Draw the top wall
                    Rectangle topWall = new Rectangle(0, 0, Size.Width * SnakeUtilities.World.pixelsPerCell, SnakeUtilities.World.pixelsPerCell);
                    e.Graphics.FillRectangle(drawBrush, topWall);

                    // Draw the right wall
                    Rectangle rightWall = new Rectangle(((world.width - 1) * SnakeUtilities.World.pixelsPerCell), 0, SnakeUtilities.World.pixelsPerCell, world.height * SnakeUtilities.World.pixelsPerCell);
                    e.Graphics.FillRectangle(drawBrush, rightWall);

                    //Draw the left wall
                    Rectangle leftWall = new Rectangle(0, 0, SnakeUtilities.World.pixelsPerCell, world.height * SnakeUtilities.World.pixelsPerCell);
                    e.Graphics.FillRectangle(drawBrush, leftWall);
                    
                    //Draw the bottom wall
                    Rectangle bottomWall = new Rectangle(0, (world.height - 1) * SnakeUtilities.World.pixelsPerCell, Size.Width * SnakeUtilities.World.pixelsPerCell, SnakeUtilities.World.pixelsPerCell);
                    e.Graphics.FillRectangle(drawBrush, bottomWall);



                    
                    //Rectangle endRight = new Rectangle((Size.Width * SnakeUtilities.World.pixelsPerCell)-10, 0, SnakeUtilities.World.pixelsPerCell, Size.Height * SnakeUtilities.World.pixelsPerCell);
                    //e.Graphics.FillRectangle(drawBrush, bottomWall);
                 

                    


                }

                

                //Begin drawing everything in the world(snakes, food...)
                world.Draw(e, this.Size.Width, this.Size.Height);
            }
        }
    }
}
