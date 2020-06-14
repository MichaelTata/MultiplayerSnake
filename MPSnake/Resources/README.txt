
//Authors: Michael Tata
//Josh Vidmar

--------CLIENT-------- 

Design:

Scoreboard is drawn as each snake is drawn in world.draw, most of the tracking for the game is done by the world class, where it has a dictionary for colors, snakes, and food. 

The only thing the snake class is currently tracking are its segments(from one vertex or turn to the next, starting from tail to head) and an id and name. Color is saved in world based on the individual snake.

For drawing snakes, decided to use pen and drawline as it makes drawing the segments quite easy, only slight problem is with a weird pixel offset making it difficult to determine when a food is eaten. Adding a slight 
Offset to compensate works fine, but it is not as clean. 
As snakes are being drawn, their name and scoreboard are being drawn in a similar manner with draw string, which works fine. Colors are randomly generated from rgb, there is no default colors. 

Shortcomings: No zoom feature(as of now), no reconnect button on death, scoreboard is seperated simply by a line(for now) and is of same color background which is not ideal. 

No extra features that are noteworthy.


Notes:

In network handler, on a failure to connect to the server, it will return a failed socket state with ID-1 and a null socket same with any failed connection or failed socket. 



--------SERVER---------

Design: Still using list implementation(representing each turn or vertex) for snake. Snake handles its own growth as determined by snake world. 

World handles all updating of snakes and food. so if a food dies, the world is where we trigger it to die, same with snake for growth and everything else. 

Snake movement: snake moves by a change cell method in cell, which moves the cell in the direction designated. So if direction request is inputted, will change the snakes head cell to a cell one away. Tail follows by finding direction to next vertex
and following it, once it is reached, it is deleted, therefore a new tail is set.

Food spawns randomly anywhere on map, added as needed in world update.




Extra Game Mode: Tron. Snake starts growing indefinitely, goal is to take up as much of the world as possible. No food or anything else, just the snakes. Kind of pointless without snake and self collision handling though. 




Features Not Implemented: snake collision(including self), also does not check if a cell is occupied so spawning can be weird, also no snake recycling. No unit testing.