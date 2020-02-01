//Zia Formuly
//ICS 3U1
//June 17, 2019
//Final Game Project

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Media;

namespace Final_Game
{
    public partial class Form1 : Form
    {
        //Rectangle to represent player
        Rectangle playerRec;
        //Declares an image variable and stores the player image file within it
        Image playerImage = Image.FromFile(Application.StartupPath + @"\playerPic.png", true);
        //Declares an image variable and stores the health point image file within it
        Image healthPointImage = Image.FromFile(Application.StartupPath + @"\coin.png", true);

        //Declares a streamreader variable for reading high score file
        StreamReader fileReader;
        //Declares a streamwriter variable for writing to high score file
        StreamWriter fileWriter;

        //Delcares a soundplayer variable for playing blockdamage sound
        SoundPlayer blockDamage;
        //Delcares a soundplayer variable for playing blockBreak sound
        SoundPlayer blockBreak;
        //Delcares a soundplayer variable for playing coinCollect sound
        SoundPlayer coinCollect;
        //Delcares a soundplayer variable for playing gameOver sound
        SoundPlayer gameOver;

        //Declares rectangle list for storing all the blocks
        IList<Rectangle> block = new List<Rectangle>();
        //Declares integer list for storing all the blocks' healths
        IList<int> blockHP = new List<int>();
        //Declares rectangle list for storing all the healthpoints
        IList<Rectangle> healthPoints = new List<Rectangle>();
        //Declares color list for storing all the blocks' colors
        IList<Color> blockColor = new List<Color>();

        //Declares game timer
        Timer moveTimer;
        //Declares random number variable for getting random numbers
        Random ranNum = new Random();
        //Declares a font variable for the text fonts
        Font hpFont = new Font("Arial", 12, FontStyle.Bold);

        //Declares integer variable for player health
        int playerHP = 25;
        //Declares integer variable for player's score
        int score = 0;
        //Declares integer variable for player's highscore
        int highScore = 0;
        //Declares integer variable for blocks' speed moving down
        int dy = 5;
        //Declares integer variable for spawnchance for randomly generating blocks
        int spawnChance = 0;
        //Declares integer variable for elapsed time to keep track of in game time
        int elapsedTime = 0;
        //Declares constant integer variable for the screen height
        const int screenHeight = 850;
        //Declares constant integer variable for the screen width
        const int screenWidth = 495;
        //Declares boolean variable for pausecheck to keep track of if game is paused 
        bool pauseCheck = false;
        //Declares string variable for the game over text
        string gameOverTxt;

        public Form1()
        {
            //Initializes component
            InitializeComponent();
        }

        //This method loads the form and sets up the window
        private void Form1_Load(object sender, EventArgs e)
        {
            //Calls the formPropertiesVarSetup method to setup all variables and form properties
            formPropertiesVarSetup();

            //Creates a mouse move event
            this.MouseMove += Form1_MouseMove;
            //Creates a keyup event
            this.KeyUp += Form1_KeyUp;
            //Creates a paint event
            this.Paint += Form1_Paint;
            //Creates a timer
            moveTimer = new Timer();
            //Creates method for timer
            moveTimer.Tick += MoveTimer_Tick;
            //Sets the timer interval to 10ms
            moveTimer.Interval = 1000 / 60;
            //Starts timer
            moveTimer.Start();
        }

        //The timer method which runs all methods needed for game to function
        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            //Checks if elapsed time is divisible by 190
            if (elapsedTime % 190 == 0)
            {
                //Calls the createBlockRow method to create a row of blocks
                createBlocksRow();
            }
            //Checks if elapsed time is divisible by 9.5
            else if (elapsedTime % 9.5 == 0)
            {
                //Calls the createBlockRow method to generate random blocks
                createBlocksRandom();
            }

            //Calls blockIntersectionCheck method for checking if player has intersected with a block
            blockIntersectionCheck();
            //Calls coinIntersectionCheck method for checking if player has intersected with a coin
            coinIntersectionCheck();
            //Calls gameOverCheck method for checking if player health is zero to end the game
            gameOverCheck();
            //Calls the moveObjects method to move all blocks and coins down on the screen
            moveObjects();
            //Calls readHighScoreFile to read and check if the highscore has been beat
            readHighScoreFile();

            this.Invalidate();
        }

        //The moveObjects method which moves all objects on screen down
        private void moveObjects()
        {
            //Declares a temprect rectangle variable which is used to move the block list
            Rectangle tempRect;

            //Runs through the block list
            for (int i = 0; i < block.Count; i++)
            {
                //Checks if block has intersected with player
                if (block[i].IntersectsWith(playerRec))
                {
                    //Returns and does not move blocks 
                    return;
                }
            }

            //Runs through the block list
            for (int i = 0; i < block.Count; i++)
            {
                //Sets temprect equal to block
                tempRect = block[i];
                //Moves temprect down by the value of dy
                tempRect.Y += dy;
                //Sets block equal to temprect
                block[i] = tempRect;

                //Checks if block's y is greater than the height of screen plus 250
                if (block[i].Y > ClientSize.Height + 250)
                {
                    //Deletes block
                    deleteBlock(i);
                    //Subtract 1 from i
                    --i;
                }
            }

            //Runs through healthPoints list
            for (int i = 0; i < healthPoints.Count; i++)
            {
                //Sets temprect equal to healthPoint
                tempRect = healthPoints[i];
                //Moves temprect down by the value of dy
                tempRect.Y += dy;
                //Sets healthPoints equal to temprect
                healthPoints[i] = tempRect;
            }

            //Adds one to elapsed time
            elapsedTime++;

            //Checks if elapsed time is greater than or equal to 190
            if (elapsedTime >= 190)
            {
                //Resets elapsed time
                elapsedTime = 0;
            }
        }

        //The createBlocksRow method which creates a row of block on the screen
        private void createBlocksRow()
        {
            //Starts a loop that repeats 5 times
            for (int i = 5; i <= 385; i += 95)
            {
                //Adds a new block at the x position of i and at a y position off screen
                block.Add(new Rectangle(i, -90, 90, 90));
                //Adds a new random blockHP from 1-40 
                blockHP.Add(ranNum.Next(1, 40));
                //Adds a new random block color
                blockColor.Add(Color.FromArgb(ranNum.Next(0, 255), ranNum.Next(0, 255), ranNum.Next(0, 255)));
            }
        }

        //The createBlocksRandom method which randomly creates blocks on screen
        private void createBlocksRandom()
        {
            //Sets spawmChance to a random number between 0-4
            spawnChance = ranNum.Next(4);

            //Checks if spawnChance is greater than 1 (60% chance)
            if (spawnChance > 1)
            {
                //Sets spawmChance to a random number between 0-5
                spawnChance = ranNum.Next(5);

                //Checks if spawnChance is 0
                if (spawnChance == 0)
                {
                    //Adds a new block in the first position of the screen
                    block.Add(new Rectangle(5, -90, 90, 90));
                }
                //Checks if spawnChance is 1
                if (spawnChance == 1)
                {
                    //Adds a new block in the second position of the screen
                    block.Add(new Rectangle(100, -90, 90, 90));
                }
                //Checks if spawnChance is 2
                if (spawnChance == 2)
                {
                    //Adds a new block in the third position of the screen
                    block.Add(new Rectangle(195, -90, 90, 90));
                }
                //Checks if spawnChance is 3
                if (spawnChance == 3)
                {
                    //Adds a new block in the forth position of the screen
                    block.Add(new Rectangle(290, -90, 90, 90));
                }
                //Checks if spawnChance is 4
                if (spawnChance == 4)
                {
                    //Adds a new block in the fifth position of the screen
                    block.Add(new Rectangle(385, -90, 90, 90));
                }

                //Adds a random blockHP between 1 and 40
                blockHP.Add(ranNum.Next(1, 40));
                //Adds a random block color
                blockColor.Add(Color.FromArgb(ranNum.Next(0, 255), ranNum.Next(0, 255), ranNum.Next(0, 255)));
                //Adds a healthpoint at a random x value of the screen
                healthPoints.Add(new Rectangle(ranNum.Next(ClientSize.Width - 50), -90, 50, 50));
            }
        }

        //The blockIntersectionCheck method which checks if the player has intersected with a block
        private void blockIntersectionCheck()
        {
            //Runs through block list
            for (int i = 0; i < block.Count; i++)
            {
                //Checks if player has intersected with the block
                if (playerRec.IntersectsWith(block[i]))
                {
                    //Checks if blockHP is divisible by 2
                    if (blockHP[i] % 2 == 0)
                    {
                        //Plays the blockDamage sound
                        blockDamage.Play();
                    }
                    //Lowers the blockHP by 1
                    blockHP[i]--;
                    //Lowers the player's health by 1
                    playerHP--;
                    //Ads one to the score
                    score++;
                }

                //Checks if blockHp is less than or equal to 0
                if (blockHP[i] <= 0)
                {
                    //Plays the blockBreak sound
                    blockBreak.Play();
                    //Adds 5 to score
                    score += 5;
                    //Calls the deleteBlock method to delete the block and all associated data
                    deleteBlock(i);

                }

            }
        }

        //The coinIntersectionCheck method which checks if the player has intersected with any coins
        private void coinIntersectionCheck()
        {
            //Runs through the healthpoints list
            for (int i = 0; i < healthPoints.Count; i++)
            {
                //Checks if player has intersected with healthpoint
                if (playerRec.IntersectsWith(healthPoints[i]))
                {
                    //Plays the coinCollect sound
                    coinCollect.Play();
                    //Removes the healthpoint
                    healthPoints.RemoveAt(i);
                    //Adds a random number between 2 and 8 to player's health
                    playerHP += ranNum.Next(2, 8);
                    //Subtracts one from i to prevent error
                    --i;
                }
            }
            //Runs through the healthpoints list
            for (int i = 0; i < healthPoints.Count; i++)
            {
                //Runs through the block list
                for (int j = 0; j < block.Count; j++)
                {
                    //Checks if healthpoint has intersects with block
                    if (healthPoints[i].IntersectsWith(block[j]))
                    {
                        //Removes healthpoint
                        healthPoints.RemoveAt(i);
                        //Subtracts one from i to prevent error
                        --i;
                    }
                }
            }
        }

        //The gameOverCheck method which checks if the game is over
        private void gameOverCheck()
        {
            //Checks if player health is less than or equal to 0
            if (playerHP <= 0)
            {
                //Plays gameover sound
                gameOver.Play();
                //Stop the game timer
                moveTimer.Stop();
                //Shows message box with the gameover text
                MessageBox.Show(gameOverTxt);
                //Exits application
                Application.Exit();

            }
        }

        //The deleteBlock method which deletes a block and all its data
        private void deleteBlock(int i)
        {
            //Deletes block
            block.RemoveAt(i);
            //Deletes the corrosponding blockHP
            blockHP.RemoveAt(i);
            //Deletes the corrosponding blockColor
            blockColor.RemoveAt(i);
        }

        //The readHighScoreFile method which reads and writes to the high score file
        private void readHighScoreFile()
        {
            //Checks if the highscore file exists
            if (File.Exists(Application.StartupPath + @"\highScore.txt"))
            {
                //Stores the high score file in the fileReader variable
                fileReader = new StreamReader(Application.StartupPath + @"\highScore.txt");

                //Runs code for every line of the data file
                while (fileReader.Peek() >= 0)
                {
                    //Reads and stores the text into the highscore variable
                    highScore = Convert.ToInt32(fileReader.ReadLine());
                }

                //Closes fileReader file
                fileReader.Close();

                //Updates the gameOver text with the current score
                gameOverTxt = "Game Over!\n" + "Your scored " + score + " points!";

                //Checks if the score is higher than the high score
                if (score > highScore)
                {
                    //Adds "Congrats, You set a new high score!" to the game over text string
                    gameOverTxt += "\nCongrats, You set a new high score!";
                    //Stores the highscore file in the fileWriter variable
                    fileWriter = new StreamWriter(Application.StartupPath + @"\highScore.txt", false);
                    //Writes the new score in the file
                    fileWriter.WriteLine(score);
                    //Closes fileWriter file
                    fileWriter.Close();
                }
            }
        }

        //The formPropertiesVarSetup method which sets up the form and variables
        private void formPropertiesVarSetup()
        {
            //Sets the text of the window to "Circle vs Square"
            this.Text = "Circle vs Square";
            //Sets the height of the window to the value of screenHeigt variable
            this.Height = screenHeight;
            //Sets the height of the window to the value of screenHeigt variable
            this.Width = screenWidth;
            //Stops the form from being maximized
            this.MaximizeBox = false;
            //Sets the maximum size of the window
            this.MaximumSize = new System.Drawing.Size(screenWidth, screenHeight);
            //Sets the minimum size of the window to the maximum size
            this.MinimumSize = this.MaximumSize;
            //Centers the form to the screen
            this.CenterToScreen();
            //Sets doubleBuffered to true to smoothed graphics
            this.DoubleBuffered = true;
            //Sets the back color of the form to black
            this.BackColor = Color.Black;

            //Creates a new soundplayer for the blockDamage sound
            blockDamage = new SoundPlayer();
            //Creates a new soundplayer for the blockBreak sound
            blockBreak = new SoundPlayer();
            //Creates a new soundplayer for the coinCollect sound
            coinCollect = new SoundPlayer();
            //Creates a new soundplayer for the gameOver sound
            gameOver = new SoundPlayer();
            //Sets the blockDamage sound to the corrosponding file in the debug folder
            blockDamage.SoundLocation = Application.StartupPath + @"\click.wav";
            //Sets the blockBreak sound to the corrosponding file in the debug folder
            blockBreak.SoundLocation = Application.StartupPath + @"\click2.wav";
            //Sets the coinCollect sound to the corrosponding file in the debug folder
            coinCollect.SoundLocation = Application.StartupPath + @"\coin.wav";
            //Sets the gameOver sound to the corrosponding file in the debug folder
            gameOver.SoundLocation = Application.StartupPath + @"\gameover.wav";

            //Creates the player rectangle and sets it to the bottom-middle of the screen
            playerRec = new Rectangle((ClientSize.Width / 2) - 25, ClientSize.Height - (ClientSize.Height / 3), 50, 50);
        }

        //The MouseMove method whch moves the player to the mouse's x location
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //Creates an integer variable for the last position of the player and sets it to the player's x coordinate
            int lastPos = playerRec.X;

            //Moves player to position of the mouse
             playerRec.X = e.X;

            //Runs through the block list
            for (int i = 0; i < block.Count; i++)
            {
                //Checks if the player is intersecting the block
                if (playerRec.IntersectsWith(block[i]))
                {
                    //Checks if the player hits the left side of the block
                    if (playerRec.Top < (block[i].Bottom - 10) && (playerRec.Right >= block[i].Left))
                    {
                        //Moves player the the last position
                        playerRec.X = lastPos;
                    }
                    //Checks if the player hits the right side of the block
                    if (playerRec.Top < (block[i].Bottom - 10) && (playerRec.Left <= block[i].Right))
                    {
                        //Moves player the the last position
                        playerRec.X = lastPos;
                    }
                }
            }

            //Checks if the player's right is greater than the screen's width
            if (playerRec.Right > this.ClientSize.Width)
            {
                //Moves the player to last position
                playerRec.X = lastPos;
            }

        }

        //The KepUp method which is used for pausing the game 
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //Checks if the escape key is pressed
            if (e.KeyCode == Keys.Escape)
            {
                //Checks if pauseCheck is false
                if (pauseCheck == false)
                {
                    //Stops game timer
                    moveTimer.Stop();
                }
                //Otherwise
                else
                {
                    //Starts game timer
                    moveTimer.Start();
                }
                //Inverses the pauseCheck boolean
                pauseCheck = !pauseCheck;
                //Refreshes the screen
                this.Invalidate();
            }
        }

        //The Paint method which is used for painting images, colours and text on the screen
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //Paints the player image in the player rectangle
            e.Graphics.DrawImage(playerImage, playerRec);

            //Runs throught the block list
            for (int j = 0; j < block.Count; j++)
            {
                //Fills block with the corrosponding block color
                e.Graphics.FillRectangle(new SolidBrush(blockColor[j]), block[j]);
                //Draws the blockHp in the middle of the block
                e.Graphics.DrawString(Convert.ToString(blockHP[j]), hpFont, Brushes.White, block[j].X+37, block[j].Y+37);
            }

            //Runs through the healthpoints list
            for (int j = 0; j < healthPoints.Count; j++)
            {
                //Paints the healthpoint image onto the healthpoint rectangle
                e.Graphics.DrawImage(healthPointImage, healthPoints[j]);
            }

            //Draws the current score on the top left of the screen
            e.Graphics.DrawString("Score: " + (Convert.ToString(score)), hpFont, Brushes.White, 0, 0);
            //Draws the highscore on the top right of the screen
            e.Graphics.DrawString("High Score: " + (Convert.ToString(highScore)), hpFont, Brushes.White, ClientSize.Width-135, 0);
            //Draws the playerHP in the middle of the player rectangle
            e.Graphics.DrawString(Convert.ToString(playerHP), hpFont, Brushes.White, playerRec.X+15, playerRec.Y+15);

            //Checks if pauseCheck is true
            if (pauseCheck == true)
            {
                //Draws "PAUSED" on the screen
                e.Graphics.DrawString("PAUSED", hpFont, Brushes.White, 205, 200);
            }
        }
    }
}
