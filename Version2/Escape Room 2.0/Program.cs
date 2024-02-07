using System.Diagnostics;

namespace Escape_Room_2._0
{
    ///<summary>
    /// Represents objects within the play area
    ///</summary>
    internal class Objects
    {

        // Private fields to store the type, last type and position of the object
        private string type;
        private string lastType = "";
        private int xPos;
        private int yPos;


        ///<summary>
        /// Initializes a new instance of the Objects class with the given type
        ///</summary>
        public Objects(string _type, int x, int y)
        {
            type = _type;
            xPos = x;
            yPos = y;

        }

        ///<summary>
        /// Changes the type of the object and stores the previous type
        ///</summary>
        ///<param name="_newType">The new type of the object</param>
        public void ChangeObjectType(string _newType)
        {
            lastType = type;
            type = _newType;
        }

        ///<summary>
        /// Recovers the previous type of the object
        ///</summary>
        public void RecoverLastType()
        {
            type = lastType;
            lastType = "";
        }

        ///<summary>
        /// Deletes the stored value in lastType of the object
        ///</summary>
        public void DeleteLastType()
        {
            lastType = "";
        }

        ///<summary>
        /// Retrieves the last type of the object
        ///</summary>
        ///<returns>The last type of the object</returns>
        public string GetLastType()
        {
            return lastType;
        }
        ///<summary>
        /// Retrieves the current type of the object
        ///</summary>
        ///<returns>The current type of the object</returns>
        public string GetCurrentType()
        {
            return type;
        }

        /// <summary>
        /// Retrieves the X coordinate of the object
        /// </summary>
        /// <returns>X position</returns>
        public int GetXPos()
        {
            return xPos;
        }

        /// <summary>
        /// Retrieves the Y coordinate of the object
        /// </summary>
        /// <returns>Y position</returns>
        public int GetYPos()
        {
            return yPos;
        }
    }


    /// <summary>
    /// Class to represent each round played to use for the scoreboard
    /// </summary>
    internal class Round
    {
        private double time;
        private int size;
        private int difficulty;

        public Round(int _size, double _time, int _difficulty)
        {
            time = _time;
            size = _size;
            difficulty = _difficulty;
        }

        public double GetTime()
        {
            return time;
        }
        public int GetSize()
        {
            return size;
        }
        public int GetDifficulty()
        {
            return difficulty;
        }
    }

    ///<summary>
    /// Class responsible for managing user input asynchronously
    ///</summary>
    public class InputManager
    {
        // Queue to store user input
        static Queue<ConsoleKeyInfo> inputBuffer = new Queue<ConsoleKeyInfo>();
        // Lock object to ensure thread safety
        static object inputLock = new object();
        // Maximum size of the input buffer
        static int maxBufferSize = 1;

        ///<summary>
        /// Initializes a new instance of the InputManager class
        ///</summary>
        public InputManager()
        {
            // Start a new thread to continuously read user input
            Thread inputThread = new Thread(ReadInput);
            inputThread.Start();
        }

        ///<summary>
        /// Method to read user input asynchronously and add it to the input buffer
        ///</summary>
        private void ReadInput()
        {
            while (true)
            {
                // Check if a key is available
                if (Console.KeyAvailable)
                {
                    // Read the key
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    // Ensure thread safety while accessing the input buffer
                    lock (inputLock)
                    {
                        // Enqueue the key into the input buffer
                        inputBuffer.Enqueue(key);
                        // Ensure the input buffer size does not exceed the maximum
                        while (inputBuffer.Count > maxBufferSize)
                        {
                            inputBuffer.Dequeue();
                        }
                    }
                }
            }
        }

        ///<summary>
        /// Checks if a key has been pressed by the user
        ///</summary>
        ///<returns>True if a key has been pressed otherwise false</returns>
        public bool KeyPressed()
        {
            // Ensure thread safety while accessing the input buffer
            lock (inputLock)
            {
                // Check if the input buffer contains any keys
                return inputBuffer.Count > 0;
            }
        }

        ///<summary>
        /// Reads and dequeues a key from the input buffer
        ///</summary>
        ///<returns>The ConsoleKeyInfo object representing the dequeued key</returns>
        public ConsoleKeyInfo ReadKey()
        {
            // Ensure thread safety while accessing the input buffer
            lock (inputLock)
            {
                // Check if the input buffer contains any keys
                if (inputBuffer.Count > 0)
                {
                    // Dequeue and return the next key in the input buffer
                    return inputBuffer.Dequeue();
                }
                else
                {
                    // If the input buffer is empty return a default ConsoleKeyInfo object
                    return new ConsoleKeyInfo();
                }
            }
        }
    }

    internal class Program
    {
        #region Special Varables
        static string cache;
        static int speed = 1;
        static bool validInput = false;
        static InputManager inputManager = new InputManager();
        static Objects[,] playArea = new Objects[areaSizeX, areaSizeY];
        #endregion

        #region Scoreboard/Stopwatch Variables
        static List<Round> scoreBoard = new List<Round>();
        static Stopwatch stopwatch = new Stopwatch();
        static bool saveTime;
        static int size;
        static double time;
        #endregion

        #region Options Variables
        static int areaSizeX;
        static int areaSizeY;
        static int difficultyLevel = 1;
        #endregion

        #region Play Variables
        static int playerXPosition;
        static int playerYPosition;
        static int doorXPosition;
        static int doorYPosition;
        static bool gotKey = false;
        static bool roundRunning = true;
        static bool playGame = true;
        static bool doUpdate;
        #endregion

        #region Maze Variables
        static int mazeStartXPosition;
        static int mazeStartYPosition;
        static int ghostPlusOrMinus = 2;
        static int ghostUpDownOrLeftRight = 2;
        static int counterPlaced = 0;
        static int ghostXPos;
        static int ghostYPos;
        static int ghostXDifferent;
        static int ghostYDifferent;
        static Stack<Objects> mazeObjectsStack = new Stack<Objects>();
        static List<Objects> visitedObjects = new List<Objects>();
        #endregion

        #region Menu/Manager Variables
        static bool menu = true;
        static bool options = true;
        static bool scoreboard = true;
        static bool tutorialDone = false;
        static bool settingDone = false;
        #endregion

        ///<summary>
        /// Main entry point of the game and handles the main menu functionality and navigation
        ///</summary>
        static void Main(string[] args)
        {
            // Set console buffer size and window size for better display
            Console.SetBufferSize(2000, 2000);
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);

            // Loop until the user quits the game
            while (menu)
            {
                Console.CursorVisible = false;
                validInput = false;

                // Display menu options and handle user input
                while (validInput == false)
                {
                    Console.Write("MENU\n\n");
                    TextAnimate("[P]lay\n");
                    TextAnimate("[S]coreboard\n");
                    TextAnimate("[T]utorial\n");
                    TextAnimate("[O]ptions\n");
                    TextAnimate("[Q]uit\n");
                    ConsoleKeyInfo input = Console.ReadKey(true);
                    switch (input.Key)
                    {
                        case ConsoleKey.P:
                            {
                                // Start the game if tutorial and settings are done
                                if (tutorialDone != true)
                                {
                                    Console.Clear();
                                    Console.Write("MENU\n\n");
                                    TextAnimateTime("Please read the tutorial first", 1000);
                                    playGame = false;
                                }
                                else if (settingDone != true)
                                {
                                    Console.Clear();
                                    Console.Write("MENU\n\n");
                                    TextAnimateTime("Please set a size for your play area in the options tab", 1000);
                                    playGame = false;
                                }
                                else
                                {
                                    Console.CursorVisible = false;
                                    Console.Clear();
                                    StartRound();
                                    playGame = true;
                                    validInput = true;
                                }
                                break;
                            }
                        case ConsoleKey.S:
                            {
                                // Display the scoreboard
                                Console.CursorVisible = false;
                                Console.Clear();
                                PrintScoreboard();
                                scoreboard = true;
                                validInput = true;
                                break;
                            }
                        case ConsoleKey.T:
                            {
                                // Display the tutorial
                                Console.CursorVisible = false;
                                Console.Clear();
                                Tutorial();
                                tutorialDone = true;
                                validInput = true;
                                break;
                            }
                        case ConsoleKey.O:
                            {
                                // Access the options menu
                                Console.CursorVisible = false;
                                Console.Clear();
                                playArea = Options();
                                options = true;
                                validInput = true;
                                break;
                            }
                        case ConsoleKey.Q:
                            {
                                // Quit the game
                                Console.CursorVisible = false;
                                Console.Clear();
                                Console.Write("MENU\n\n");
                                TextAnimateTime("Ok bye", 2000);
                                Console.Clear();
                                menu = false;
                                validInput = true;
                                break;
                            }
                        default:
                            {
                                // Handle invalid input
                                Console.CursorVisible = false;
                                Console.Clear();
                                Console.Write("MENU\n\n");
                                TextAnimateTime("Invalid input please try again", 1000);
                                validInput = false;
                                break;
                            }
                    }
                }
            }
        }


        #region Play
        /// <summary>
        /// Starts a new round of the game
        /// </summary>
        public static void StartRound()
        {
            while (playGame)
            {
                // Build the play area
                playArea = BuildArea(playArea);

                // Display countdown animation
                TextAnimateTime("3", 1000);
                TextAnimateTime("2", 1000);
                TextAnimateTime("1", 1000);
                TextAnimateTime("Go", 1000);

                // Display the play area and start the round
                DisplayManager(difficultyLevel);
                saveTime = true;

                // Main game loop
                while (roundRunning)
                {
                    stopwatch.Start();
                    // Control player movement and interactions
                    PlayerController(playArea);

                    // Update the display if needed
                    if (doUpdate = true)
                    {
                        Console.SetCursorPosition(0, 0);
                        DisplayManager(difficultyLevel);
                        doUpdate = false;
                    }
                }
                stopwatch.Stop();

                // Save the round time and update the scoreboard
                if (saveTime)
                {
                    time = (double)stopwatch.Elapsed.TotalSeconds;
                    time = Math.Round(time, 2);
                    Round round = new Round(size, time, difficultyLevel);
                    scoreBoard.Add(round);
                    Thread.Sleep(500 / speed);
                    Console.Clear();
                    TextAnimateTime("Congratulations you successfully escaped in " + time + " seconds", 2000);
                }
                else
                {
                    Thread.Sleep(500 / speed);
                    Console.Clear();
                    TextAnimateTime("You didn't escape the room", 2000);
                }
                stopwatch.Reset();

                // Prompt for starting a new round or ending the game
                validInput = false;
                while (validInput == false)
                {
                    TextAnimate("Start a new round?\n");
                    TextAnimate("[Y]es, [N]o\n");
                    ConsoleKeyInfo input2 = Console.ReadKey(true);
                    switch (input2.Key)
                    {
                        case ConsoleKey.Y:
                            {
                                Console.Clear();
                                TextAnimateTime("Alright let's go", 500);
                                playGame = true;
                                roundRunning = true;
                                gotKey = false;
                                validInput = true;
                                Console.Clear();
                                break;
                            }
                        case ConsoleKey.N:
                            {
                                Console.Clear();
                                TextAnimateTime("Ok", 1000);
                                playGame = false;
                                roundRunning = true;
                                gotKey = false;
                                validInput = true;
                                Console.Clear();
                                break;
                            }
                        default:
                            {
                                Console.Clear();
                                TextAnimateTime("Please answer with [Y]es or [N]o", 1000);
                                validInput = false;
                            }
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Builds the play area by constructing the room, adding maze if needed, and placing interactable objects
        /// </summary>
        /// <param name="playArea">The play area grid</param>
        /// <returns>The play area grid with the constructed room, maze (if applicable), and interactable objects placed</returns>
        public static Objects[,] BuildArea(Objects[,] playArea)
        {
            // Build the room
            playArea = BuildRoom(playArea);

            // Add maze if difficulty level is higher than 1
            if (difficultyLevel > 1)
            {
                BuildMaze(playArea);
            }

            // Place interactable objects
            playArea = PlaceInteractable(playArea);

            return playArea;
        }


        ///<summary>
        ///Generates the basic parts of the room and the door
        ///</summary>
        ///<param name="playArea">The 2D array representing the play area</param>
        ///<returns>The updated play area with added walls and floor objects</returns>
        public static Objects[,] BuildRoom(Objects[,] playArea)
        {
            // Add walls and floor in the right places
            for (int y = 0; y < areaSizeX; y++)
            {
                for (int x = 0; x < areaSizeY; x++)
                {
                    if (IsOutterRing(x, y))
                    {
                        Objects wall = new Objects("Wall", x, y);
                        playArea[x, y] = wall;
                    }
                    else if (IsInnerArea(x, y))
                    {
                        Objects floor = new Objects("Floor", x, y);
                        playArea[x, y] = floor;
                    }
                }
            }

            // Add Door at valid random place
            Random rnd = new Random();
            int x1;
            int y1;
            bool done = false;
            do
            {
                x1 = rnd.Next(areaSizeX);
                y1 = rnd.Next(areaSizeY);
                if (IsOutterRing(x1, y1) && (0 < x1 && x1 < areaSizeX - 1 || 0 < y1 && y1 < areaSizeY - 1))
                {
                    playArea[x1, y1].ChangeObjectType("Door");
                    doorXPosition = x1;
                    doorYPosition = y1;
                    done = true;
                }
            } while (!IsOutterRing(x1, y1) || done == false);

            return playArea;
        }

        ///<summary>
        ///Adds interactable objects including player and key
        ///</summary>
        ///<param name="playArea">The 2D array representing the play area</param>
        ///<returns>The updated play area with added interactable objects</returns>
        public static Objects[,] PlaceInteractable(Objects[,] playArea)
        {
            Random rnd = new Random();
            int x;
            int y;
            // Add Player at valid random place
            bool done = false;
            do
            {
                x = rnd.Next(areaSizeX);
                y = rnd.Next(areaSizeY);
                if (IsInnerArea(x, y) && playArea[x, y].GetCurrentType() == "Floor")
                {
                    playArea[x, y].ChangeObjectType("Player");
                    playerXPosition = x;
                    playerYPosition = y;
                    done = true;
                }
            } while (!IsInnerArea(x, y) || done == false);



            // Add Key at valid random place
            done = false;
            do
            {
                x = rnd.Next(areaSizeX);
                y = rnd.Next(areaSizeY);
                if (IsInnerArea(x, y) && playArea[x, y].GetCurrentType() == "Floor")
                {
                    playArea[x, y].ChangeObjectType("Key");
                    done = true;
                }
            } while (!IsInnerArea(x, y) || done == false);
            return playArea;
        }


        /// <summary>
        /// Builds the maze within the play area by placing wall blocks and ensuring the ghost can navigate through it.
        /// </summary>
        /// <param name="playArea">The play area grid</param>
        /// <returns>The play area with the maze built</returns>
        public static Objects[,] BuildMaze(Objects[,] playArea)
        {
            int mazeBlocksToPlace = ((areaSizeX - 2) * (areaSizeY - 2) * 2);
            bool xPlusChecked = false;
            bool xMinusChecked = false;
            bool yPlusChecked = false;
            bool yMinusChecked = false;

            // Fill the inner area with walls
            FillInnerArea();

            // Get the starting position for the maze
            GetMazeStartPosition();
            ghostXPos = mazeStartXPosition;
            ghostYPos = mazeStartYPosition;
            counterPlaced = 0;

            // Safe the position as visited
            SafeGhostMovement();

            // Loop until all maze blocks are placed
            while (counterPlaced != mazeBlocksToPlace)
            {
                // Generate a valid random move for the ghost
                GenerateValidRandomMove(ref xPlusChecked, ref xMinusChecked, ref yPlusChecked, ref yMinusChecked, mazeBlocksToPlace);

                // Check if the new position and the position after the move are within the inner area and have not been visited before
                if (IsInnerArea(ghostXPos, ghostYPos) && !visitedObjects.Contains(playArea[ghostXPos, ghostYPos]) && !visitedObjects.Contains(playArea[ghostXPos + ghostYDifferent, ghostYPos + ghostXDifferent]))
                {
                    // Safe the position as visited
                    SafeGhostMovement();
                    xPlusChecked = false;
                    xMinusChecked = false;
                    yPlusChecked = false;
                    yMinusChecked = false;

                    // Repeat the move
                    RepeatMove();

                    // Check if the new position is within the inner area and has not been visited before
                    if (IsInnerArea(ghostXPos, ghostYPos) && !visitedObjects.Contains(playArea[ghostXPos, ghostYPos]))
                    {
                        // Safe the position as visited
                        SafeGhostMovement();
                    }
                    else
                    {
                        // Reverse the move if the new position is invalid
                        ReverseMove();
                    }
                }
                else
                {
                    // Reverse the move if the new position is invalid
                    ReverseMove();
                }
            }

            // Clear visited objects, maze objects stack and bools
            visitedObjects.Clear();
            mazeObjectsStack.Clear();
            xPlusChecked = false;
            xMinusChecked = false;
            yPlusChecked = false;
            yMinusChecked = false;

            return playArea;
        }


        /// <summary>
        /// Generates a valid random move for the ghost within the game area, ensuring that the ghost moves to a new position that has not been visited before.
        /// </summary>
        /// <param name="_yPlusChecked">Flag indicating if the positive Y direction has been checked</param>
        /// <param name="_yMinusChecked">Flag indicating if the negative Y direction has been checked</param>
        /// <param name="_xPlusChecked">Flag indicating if the positive X direction has been checked</param>
        /// <param name="_xMinusChecked">Flag indicating if the negative X direction has been checked</param>
        /// <param name="_mazeBlocksToPlace">Number of maze blocks to place</param>
        public static void GenerateValidRandomMove(ref bool _yPlusChecked, ref bool _yMinusChecked, ref bool _xPlusChecked, ref bool _xMinusChecked, int _mazeBlocksToPlace)
        {
            Random rnd = new Random();
            int newghostXPos = ghostXPos;
            int newghostYPos = ghostYPos;
            int errorOverflow = 0;

            do
            {
                // Break the loop if the required number of maze blocks have been placed
                if (counterPlaced == _mazeBlocksToPlace)
                {
                    break;
                }

                // Generate random values for direction and movement
                ghostPlusOrMinus = rnd.Next(0, 2);
                ghostUpDownOrLeftRight = rnd.Next(0, 2);

                // Check movement in the X direction
                if (ghostUpDownOrLeftRight == 1)
                {
                    if (ghostPlusOrMinus == 1 && !_xMinusChecked)
                    {
                        newghostXPos = ghostXPos - 1;
                        ghostXDifferent = -1;
                        _xMinusChecked = true;
                    }
                    if (ghostPlusOrMinus == 0 && !_xPlusChecked)
                    {
                        newghostXPos = ghostXPos + 1;
                        ghostXDifferent = 1;
                        _xPlusChecked = true;
                    }
                }
                // Check movement in the Y direction
                if (ghostUpDownOrLeftRight == 0)
                {
                    if (ghostPlusOrMinus == 1 && !_yMinusChecked)
                    {
                        newghostYPos = ghostYPos - 1;
                        ghostYDifferent = -1;
                        _yMinusChecked = true;
                    }
                    if (ghostPlusOrMinus == 0 && !_yPlusChecked)
                    {
                        newghostYPos = ghostYPos + 1;
                        ghostYDifferent = 1;
                        _yPlusChecked = true;
                    }
                }

                // Reset to the previous position if the new position is outside the inner area or has been visited before
                if (!IsInnerArea(newghostXPos, newghostYPos) || visitedObjects.Contains(playArea[newghostXPos, newghostYPos]))
                {
                    newghostXPos = ghostXPos;
                    newghostYPos = ghostYPos;
                }

                // Reset the flags if all directions have been checked
                if (_xPlusChecked && _xMinusChecked && _yPlusChecked && _yMinusChecked)
                {
                    // Pop the last object from the stack to backtrack
                    if (mazeObjectsStack.Count() != 1)
                    {
                        mazeObjectsStack.Pop();
                        Objects obj = mazeObjectsStack.Peek();
                        ghostXPos = obj.GetXPos();
                        ghostYPos = obj.GetYPos();
                        _xPlusChecked = false;
                        _xMinusChecked = false;
                        _yPlusChecked = false;
                        _yMinusChecked = false;
                    }
                    else
                    {
                        errorOverflow++;
                    }
                    // Break the loop if the error overflow threshold is reached
                    if (errorOverflow > 10)
                    {
                        counterPlaced = _mazeBlocksToPlace;
                        break;
                    }
                }
            } while (!IsInnerArea(newghostXPos, newghostYPos) || visitedObjects.Contains(playArea[newghostXPos, newghostYPos]));

            errorOverflow = 0;
            ghostXPos = newghostXPos;
            ghostYPos = newghostYPos;
        }


        /// <summary>
        /// Safes movement of the ghost within the play area and tracks visited objects.
        /// </summary>
        public static void SafeGhostMovement()
        {
            // Check if the ghost's position is within the boundaries of the play area
            if (ghostXPos > 0 && ghostXPos < areaSizeX && ghostYPos > 0 && ghostYPos < areaSizeY)
            {
                // Update the play area to mark the current position as floor (indicating the ghost's movement)
                playArea[ghostXPos, ghostYPos].ChangeObjectType("Floor");
                // Add the current object to the list of visited objects
                visitedObjects.Add(playArea[ghostXPos, ghostYPos]);
                // Push the current object onto the maze objects stack
                mazeObjectsStack.Push(playArea[ghostXPos, ghostYPos]);
                // Increment the counter for placed objects (ghost's movement)
                counterPlaced++;
            }
        }


        /// <summary>
        /// Repeats the last move made by the ghost
        /// </summary>
        public static void RepeatMove()
        {
            // Check if the last move was vertical (up or down)
            if (ghostUpDownOrLeftRight == 1)
            {
                // If the last move was positive (up), increment the ghost's X position to repeat the move
                if (ghostPlusOrMinus == 1)
                {
                    ghostXPos++;
                }
                // If the last move was negative (down), decrement the ghost's X position to repeat the move
                if (ghostPlusOrMinus == 0)
                {
                    ghostXPos--;
                }
            }
            // Check if the last move was horizontal (left or right)
            if (ghostUpDownOrLeftRight == 0)
            {
                // If the last move was positive (right), increment the ghost's Y position to repeat the move
                if (ghostPlusOrMinus == 1)
                {
                    ghostYPos++;
                }
                // If the last move was negative (left), decrement the ghost's Y position to repeat the move
                if (ghostPlusOrMinus == 0)
                {
                    ghostYPos--;
                }
            }
        }


        /// <summary>
        /// Reverses the last move made by the ghost
        /// </summary>
        public static void ReverseMove()
        {
            // Check if the last move was vertical (up or down)
            if (ghostUpDownOrLeftRight == 1)
            {
                // If the last move was positive (up), decrement the ghost's X position to reverse the move
                if (ghostPlusOrMinus == 1)
                {
                    ghostXPos--;
                }
                // If the last move was negative (down), increment the ghost's X position to reverse the move
                if (ghostPlusOrMinus == 0)
                {
                    ghostXPos++;
                }
            }
            // Check if the last move was horizontal (left or right)
            if (ghostUpDownOrLeftRight == 0)
            {
                // If the last move was positive (right), decrement the ghost's Y position to reverse the move
                if (ghostPlusOrMinus == 1)
                {
                    ghostYPos--;
                }
                // If the last move was negative (left), increment the ghost's Y position to reverse the move
                if (ghostPlusOrMinus == 0)
                {
                    ghostYPos++;
                }
            }
        }


        /// <summary>
        /// Fills the inner area of the play area with walls
        /// </summary>
        public static void FillInnerArea()
        {
            // Loop through the inner area of the game area
            for (int y = 1; y < areaSizeX - 2; y++)
            {
                for (int x = 1; x < areaSizeY - 2; x++)
                {
                    // Change the object type to "Wall"
                    playArea[x, y].ChangeObjectType("Wall");
                }
            }
        }


        /// <summary>
        /// Determines the starting position for the maze generation algorithm
        /// </summary>
        public static void GetMazeStartPosition()
        {
            bool found = false;

            // Loop through the play area to find a position next to the door
            while (!found)
            {
                for (int y = 1; y < areaSizeX - 1; y++)
                {
                    for (int x = 1; x < areaSizeY - 1; x++)
                    {
                        if (playArea[x - 1, y].GetCurrentType() == "Door" || playArea[x + 1, y].GetCurrentType() == "Door" || playArea[x, y - 1].GetCurrentType() == "Door" || playArea[x, y + 1].GetCurrentType() == "Door")
                        {
                            // Set the maze start position
                            mazeStartXPosition = x;
                            mazeStartYPosition = y;
                            found = true;
                        }
                        if (found)
                        {
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
        }



        /// <summary>
        /// Controls the movement of the player within the play area based on user input
        /// </summary>
        /// <param name="playArea">The 2D array representing the play area</param>
        /// <returns>The updated play area after player input</returns>
        public static Objects[,] PlayerController(Objects[,] playArea)
        {
            ConsoleKeyInfo input = new ConsoleKeyInfo();

            // Gets player input from the InputManager
            if (inputManager.KeyPressed())
            {
                input = inputManager.ReadKey();
            }

            // Checks the input and performs corresponding actions
            switch (input.Key)
            {
                case ConsoleKey.Escape:
                    {
                        // End the current round if the Escape key is pressed
                        stopwatch.Stop();
                        roundRunning = false;
                        saveTime = false;
                        break;
                    }
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    {
                        // Check if the player does not have the key and is trying to move to the door position
                        if (gotKey == false && playerXPosition == doorXPosition && playerYPosition - 1 == doorYPosition)
                        {
                            TextAnimateTime("First pick up the key", 1000);
                        }
                        else
                        {
                            // Check if the object above the player is a key
                            if (playArea[playerXPosition, playerYPosition - 1].GetCurrentType() == "Key")
                            {
                                // Process player interaction with the key
                                playArea[playerXPosition, playerYPosition - 1].RecoverLastType();
                                playArea[playerXPosition, playerYPosition - 1].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerYPosition = playerYPosition - 1;
                                gotKey = true;
                                break;
                            }
                            // Check if the object above the player is a wall
                            else if (playArea[playerXPosition, playerYPosition - 1].GetCurrentType() != "Wall")
                            {
                                // Move the player up by changing the object type at the new position
                                playArea[playerXPosition, playerYPosition - 1].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerYPosition = playerYPosition - 1;
                                // Indicate that an update to the play area is required
                                doUpdate = true;
                            }
                            // Check if the player has the key and has reached the door position
                            if (gotKey == true && playerXPosition == doorXPosition && playerYPosition == doorYPosition)
                            {
                                roundRunning = false;
                            }
                        }
                        break;
                    }


                // Similar to the first case but changed the position modifying correspondingly
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    {
                        if (gotKey == false && playerXPosition == doorXPosition && playerYPosition + 1 == doorYPosition)
                        {
                            TextAnimateTime("First pic up the key", 1000);
                        }
                        else
                        {
                            if (playArea[playerXPosition, playerYPosition + 1].GetCurrentType() == "Key")
                            {
                                playArea[playerXPosition, playerYPosition + 1].RecoverLastType();
                                playArea[playerXPosition, playerYPosition + 1].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerYPosition = playerYPosition + 1;
                                gotKey = true;
                                break;
                            }
                            else if (playArea[playerXPosition, playerYPosition + 1].GetCurrentType() != "Wall")
                            {
                                playArea[playerXPosition, playerYPosition + 1].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerYPosition = playerYPosition + 1;

                                doUpdate = true;
                            }
                            if (gotKey == true && playerXPosition == doorXPosition && playerYPosition == doorYPosition)
                            {
                                roundRunning = false;
                            }

                        }
                        break;
                    }

                // Similar to the first case but changed the position modifying correspondingly
                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    {

                        if (gotKey == false && playerXPosition + 1 == doorXPosition && playerYPosition == doorYPosition)
                        {
                            TextAnimateTime("First pic up the key", 1000);
                        }
                        else
                        {

                            if (playArea[playerXPosition + 1, playerYPosition].GetCurrentType() == "Key")
                            {
                                playArea[playerXPosition + 1, playerYPosition].RecoverLastType();
                                playArea[playerXPosition + 1, playerYPosition].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerXPosition = playerXPosition + 1;
                                gotKey = true;
                                break;
                            }
                            else if (playArea[playerXPosition + 1, playerYPosition].GetCurrentType() != "Wall")
                            {
                                playArea[playerXPosition + 1, playerYPosition].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerXPosition = playerXPosition + 1;

                                doUpdate = true;
                            }
                            if (gotKey == true && playerXPosition == doorXPosition && playerYPosition == doorYPosition)
                            {
                                roundRunning = false;
                            }
                        }
                        break;
                    }

                // Similar to the first case but changed the position modifying correspondingly
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    {
                        if (gotKey == false && playerXPosition - 1 == doorXPosition && playerYPosition == doorYPosition)
                        {
                            TextAnimateTime("First pic up the key", 1000);
                        }
                        else
                        {

                            if (playArea[playerXPosition - 1, playerYPosition].GetCurrentType() == "Key")
                            {
                                playArea[playerXPosition - 1, playerYPosition].RecoverLastType();
                                playArea[playerXPosition - 1, playerYPosition].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerXPosition = playerXPosition - 1;
                                gotKey = true;
                                break;
                            }
                            else if (playArea[playerXPosition - 1, playerYPosition].GetCurrentType() != "Wall")
                            {
                                playArea[playerXPosition - 1, playerYPosition].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerXPosition = playerXPosition - 1;

                                doUpdate = true;
                            }
                            if (gotKey == true && playerXPosition == doorXPosition && playerYPosition == doorYPosition)
                            {
                                roundRunning = false;
                            }
                        }
                        break;
                    }
            }
            return playArea;
        }


        /// <summary>
        /// Converts the difficulty level into the vision threshold and prints the play area
        /// </summary>
        /// <param name="_difficulty">The difficulty level of the game</param>
        public static void DisplayManager(int _difficulty)
        {
            // Determine the visibility range based on the difficulty level and print the play area
            switch (_difficulty)
            {
                case 1:
                    PrintPlayArea(4);
                    break;
                case 2:
                    PrintPlayArea(6);
                    break;
                case 3:
                    PrintPlayArea(2.5);
                    break;
                case 4:
                    PrintPlayArea(2);
                    break;
                case 5:
                    PrintPlayArea(1.4);
                    break;
                default:
                    // Handle invalid difficulty levels
                    Console.WriteLine("Invalid difficulty level.");
                    break;
            }
        }


        /// <summary>
        /// Prints the play area with visibility based on the player's distance from objects, determined by the specified difficulty level
        /// </summary>
        /// <param name="_difficulty">Difficulty level determining the visibility range around the player</param>
        public static void PrintPlayArea(double _difficulty)
        {
            // Loop through each row of the play area
            for (int y = 0; y < areaSizeX; y++)
            {
                // Initialize strings for each line of the play area
                string Line1 = "";
                string Line2 = "";
                string Line3 = "";

                // Loop through each column of the play area
                for (int x = 0; x < areaSizeY; x++)
                {
                    // Calculate distance between the current position and the player
                    int XDistance = playerXPosition - x;
                    int YDistance = playerYPosition - y;
                    double Distance = Math.Sqrt(Math.Pow(XDistance, 2) + Math.Pow(YDistance, 2));

                    // Check if the distance is within the visibility range determined by the difficulty level
                    if (Distance < _difficulty)
                    {
                        // Get the object at the current position
                        Objects printObject = playArea[x, y];

                        // Determine the type of object and represent it accordingly
                        if (printObject.GetCurrentType() == "Player")
                        {
                            Line1 = Line1 + "  ---  ";
                            Line2 = Line2 + " | X | ";
                            Line3 = Line3 + "  ---  ";
                        }
                        else if (printObject.GetCurrentType() == "Key")
                        {
                            Line1 = Line1 + "  ---  ";
                            Line2 = Line2 + " | K | ";
                            Line3 = Line3 + "  ---  ";
                        }
                        else if (printObject.GetCurrentType() == "Door")
                        {
                            Line1 = Line1 + "  ---  ";
                            Line2 = Line2 + " |   | ";
                            Line3 = Line3 + " |  °| ";
                        }
                        else if (printObject.GetCurrentType() == "Wall")
                        {
                            Line1 = Line1 + " ■ ■ ■ ";
                            Line2 = Line2 + " ■ ■ ■ ";
                            Line3 = Line3 + " ■ ■ ■ ";
                        }
                        else if (printObject.GetCurrentType() == "Floor")
                        {
                            Line1 = Line1 + "  ---  ";
                            Line2 = Line2 + " |   | ";
                            Line3 = Line3 + "  ---  ";
                        }
                    }
                    else
                    {
                        // If the distance is beyond the visibility range, display empty spaces
                        Line1 = Line1 + "       ";
                        Line2 = Line2 + "       ";
                        Line3 = Line3 + "       ";
                    }
                }

                // Print each line of the play area
                Console.WriteLine(Line1);

                // Colorize the key and player and checks which color should be used
                if (gotKey == true)
                {
                    char[] array = { 'X', 'K' };
                    ConsoleColor[] colorArray = { ConsoleColor.Green, ConsoleColor.Blue };
                    ColorLettersInTextLine(Line2, array, colorArray);
                }
                else
                {
                    char[] replaceArray = { 'X', 'K' };
                    ConsoleColor[] colorArray = { ConsoleColor.Red, ConsoleColor.Blue };
                    ColorLettersInTextLine(Line2, replaceArray, colorArray);
                }

                // Print each line of the play area
                Console.WriteLine(Line3);
            }
        }

        #endregion

        #region Scoreboard
        /// <summary>
        /// Prints the scoreboard containing information about rounds played, time taken, play area size and difficulty level.
        /// Allows filtering rounds by play area size or difficulty level.
        /// Always sorted by time taken
        /// </summary>
        public static void PrintScoreboard()
        {
            while (scoreboard)
            {
                int _size = 0;
                int _difficulty = 0;

                // If there are no rounds saved, display a message and exit the method
                if (scoreBoard.Count == 0)
                {
                    Console.Clear();
                    Console.Write("SCOREBOARD\n\n");
                    TextAnimateTime("No rounds saved", 1000);
                    scoreboard = false;
                    Console.Clear();
                }
                else
                {
                    // Sort the scoreboard based on time taken
                    scoreBoard.Sort((r1, r2) => r1.GetTime().CompareTo(r2.GetTime()));

                    validInput = false;

                    // Loop until a valid input is provided
                    while (validInput == false)
                    {
                        Console.Clear();
                        Console.Write("SCOREBOARD\n\n");
                        TextAnimate("Do you want to see all rounds or filter rounds by play area size or difficulty?\n");
                        TextAnimate("[A]ll\n");
                        TextAnimate("[P]lay area\n");
                        TextAnimate("[D]ifficulty\n");
                        TextAnimate("[M]enu\n");
                        ConsoleKeyInfo input2 = Console.ReadKey(true);

                        switch (input2.Key)
                        {
                            case ConsoleKey.A:
                                {
                                    // Display all rounds
                                    validInput = true;
                                    Console.Clear();
                                    Console.Write("SCOREBOARD\n\n");
                                    int counter = 1;
                                    foreach (Round round in scoreBoard)
                                    {
                                        TextAnimate(counter + ": " + round.GetTime() + " seconds on a " + round.GetSize() + "x" + round.GetSize() + " field and a difficulty level of " + round.GetDifficulty() + "\n");
                                        counter++;
                                    }
                                    TextAnimate("\n\nPress any key");
                                    Console.ReadKey(true);
                                    counter = 1;
                                    Console.Clear();
                                    break;
                                }

                            case ConsoleKey.P:
                                {
                                    // Filter rounds by given play area size
                                    validInput = true;
                                    Console.Clear();
                                    Console.Write("SCOREBOARD\n\n");
                                    TextAnimate("Which play area size do you want to display?\n");
                                    TextAnimate("[3],[4],[5],[...]                   press enter\n");
                                    cache = Console.ReadLine().Trim();
                                    if (int.TryParse(cache, out int result))
                                    {
                                        _size = int.Parse(cache);
                                        int index = scoreBoard.FindIndex(Round => Round.GetSize() == _size);
                                        // If there are no rounds saved, display a message
                                        if (index == -1)
                                        {
                                            Console.Clear();
                                            Console.Write("SCOREBOARD\n\n");
                                            TextAnimateTime("There are no rounds saved which were played on a " + _size + "x" + _size + " field", 2000);
                                        }
                                        else
                                        {
                                            Console.Clear();
                                            Console.Write("SCOREBOARD\n\n");
                                            _size = int.Parse(cache);

                                            int counter = 1;
                                            TextAnimate("Rounds played on a " + _size + "x" + _size + " field\n\n");
                                            foreach (Round round in scoreBoard)
                                            {
                                                if (round.GetSize() == _size)
                                                {
                                                    TextAnimate(counter + ": " + round.GetTime() + " seconds on a difficulty level of " + round.GetDifficulty() + "\n");
                                                    counter++;
                                                }
                                            }
                                            TextAnimate("\n\nPress any key");
                                            Console.ReadKey(true);
                                            counter = 1;

                                        }
                                    }
                                    else
                                    {
                                        Console.Clear();
                                        Console.Write("SCOREBOARD\n\n");
                                        TextAnimateTime("Set a valid number", 2000);
                                    }
                                    Console.Clear();
                                    break;
                                }

                            case ConsoleKey.D:
                                {
                                    // Filter rounds by given difficulty level
                                    validInput = true;
                                    Console.Clear();
                                    Console.Write("SCOREBOARD\n\n");
                                    TextAnimate("Which difficulty do you want to display?\n");
                                    TextAnimate("[1],[2],[3],[4],[5]                   press enter\n");
                                    cache = Console.ReadLine().Trim();
                                    if (int.TryParse(cache, out int result))
                                    {
                                        _difficulty = int.Parse(cache);
                                        int index = scoreBoard.FindIndex(Round => Round.GetDifficulty() == _difficulty);
                                        // If there are no rounds saved, display a message
                                        if (index == -1)
                                        {
                                            Console.Clear();
                                            Console.Write("SCOREBOARD\n\n");
                                            TextAnimateTime("There are no rounds saved which were played on a difficulty level of " + _difficulty, 2000);
                                        }
                                        else
                                        {
                                            Console.Clear();
                                            Console.Write("SCOREBOARD\n\n");
                                            _difficulty = int.Parse(cache);

                                            int counter = 1;
                                            TextAnimate("Rounds played on a difficulty level of " + _difficulty + "\n\n");
                                            foreach (Round round in scoreBoard)
                                            {
                                                if (round.GetDifficulty() == _difficulty)
                                                {
                                                    TextAnimate(counter + ": " + round.GetTime() + " seconds on a " + round.GetSize() + "x" + round.GetSize() + " field\n");
                                                    counter++;
                                                }
                                            }
                                            TextAnimate("\n\nPress any key");
                                            Console.ReadKey(true);
                                            counter = 1;

                                        }
                                    }
                                    else
                                    {
                                        Console.Clear();
                                        Console.Write("SCOREBOARD\n\n");
                                        TextAnimateTime("Set a valid number", 2000);
                                    }
                                    Console.Clear();
                                    break;
                                }

                            case ConsoleKey.M:
                                {
                                    // Return to the main menu
                                    Console.Clear();
                                    scoreboard = false;
                                    validInput = true;
                                    break;
                                }

                            default:
                                {
                                    Console.Clear();
                                    TextAnimateTime("Invalid input", 1000);
                                    validInput = false;
                                    break;
                                }
                        }
                    }
                }
            }
        }

        #endregion

        #region Tutorial
        ///<summary>
        ///Provides an introduction and tutorial for the game, explaining controls, objectives, and options
        ///</summary>
        public static void Tutorial()
        {
            Console.Write("TUTORIAL\n\n");
            TextAnimate("Controll Player (x) with w,a,s,d or the arrow keys\n");
            Thread.Sleep(500 / speed);
            TextAnimate("Find and grab the key (K) and escape through the door in the wall\n");
            Thread.Sleep(500 / speed);
            TextAnimate("Press esc to end the round and get back to the menu\n\n");
            Thread.Sleep(500 / speed);
            TextAnimate("Your play area is a square but you can choose how big the square should be as long its bigger than 3x3\n");
            Thread.Sleep(500 / speed);
            TextAnimate("In the options tab you can choose how big the area is you are playing on\n");
            Thread.Sleep(500 / speed);
            TextAnimate("You can also set the difficulty level but its defaulted on the lowest level\n");
            Thread.Sleep(500 / speed);
            TextAnimate("If you want to go with a higher difficulty level than 1 we recomend using a play area bigger than 8\n\n");
            Thread.Sleep(500 / speed);
            TextAnimate("Your time to escape get tracked and is shown after each round\n");
            Thread.Sleep(500 / speed);
            TextAnimate("You can also compare your time in the scoreboard tab\n");
            Thread.Sleep(2000 / speed);
            TextAnimate("\n\nPress any key");
            Console.ReadKey(true);
            Console.Clear();

        }
        #endregion

        #region Options
        /// <summary>
        /// Provides options for the player to customize settings such as the play area size and difficulty level
        /// </summary>
        /// <returns>A 2D array representing the play area with the customized settings</returns>
        public static Objects[,] Options()
        {
            // Loop until user closes options
            while (options)
            {
                validInput = false;

                // Loop until a valid input is provided
                while (validInput == false)
                {
                    Console.Write("OPTIONS\n\n");
                    TextAnimate("Set play [A]rea\n");
                    TextAnimate("Set [D]ifficulty level\n");
                    TextAnimate("[M]enu\n");
                    ConsoleKeyInfo input = Console.ReadKey(true);

                    switch (input.Key)
                    {
                        case ConsoleKey.A:
                            {
                                // Set the play area size
                                playArea = SetGameAreaSize();
                                settingDone = true;
                                validInput = true;
                                break;
                            }

                        case ConsoleKey.D:
                            {
                                // Set the game difficulty level
                                SetGameDifficulty();
                                validInput = true;
                                break;
                            }

                        case ConsoleKey.M:
                            {
                                // Return to the main menu
                                Console.Clear();
                                options = false;
                                validInput = true;
                                break;
                            }

                        default:
                            {
                                Console.Clear();
                                Console.Write("OPTIONS\n\n");
                                TextAnimateTime("Invalid input, please try again", 1000);
                                validInput = false;
                                break;
                            }
                    }
                }
            }
            return playArea;
        }


        /// <summary>
        /// Allows the user to set the game difficulty by specifying a positive number from 1 to 5
        /// </summary>
        public static void SetGameDifficulty()
        {
            // Loop until a valid difficulty level is entered
            bool valid = false;
            do
            {
                Console.Clear();
                Console.Write("SETTING GAME DIFFICULTY\n\n");
                TextAnimate("Set a positiv number for the difficulty 1-5: ");
                cache = Console.ReadLine().Trim();
                // Check if the input can be parsed to an integer
                if (int.TryParse(cache, out int result))
                {
                    // Check if the entered value is within the range 1-5
                    if (int.Parse(cache) > 0 && int.Parse(cache) < 6)
                    {
                        difficultyLevel = int.Parse(cache);
                        valid = true;
                    }
                    else
                    {
                        Console.Clear();
                        Console.Write("SETTING GAME DIFFICULTY\n\n");
                        TextAnimateTime("Input is out of the range", 2000);
                    }
                }
                else
                {
                    Console.Clear();
                    Console.Write("SETTING GAME DIFFICULTY\n\n");
                    TextAnimateTime("Set a valid number", 2000);
                }
            } while (!valid);
            Console.Clear();
            Console.Write("SETTING GAME DIFFICULTY\n\n");
            TextAnimateTime("Saved", 1000);
        }

        ///<summary>
        ///Declares the play area dimensions based on user input and initializes the play area
        ///</summary>
        ///<returns>A 2D array representing the play area</returns>
        public static Objects[,] SetGameAreaSize()
        {

            bool valid = false;
            do
            {
                // Prompt the user to set the size of the play area
                Console.Clear();
                Console.Write("SETTING play area\n\n");
                TextAnimate("Set a positiv number for the size of the square between 3 and 20: ");
                cache = Console.ReadLine().Trim();
                if (int.TryParse(cache, out int result))
                {
                    if (int.Parse(cache) > 3 && int.Parse(cache) < 20)
                    {
                        // Set the dimensions of the play area and mark input as valid
                        areaSizeX = int.Parse(cache);
                        areaSizeY = int.Parse(cache);
                        size = int.Parse(cache);
                        valid = true;
                    }
                    else if (int.Parse(cache) < 4)
                    {
                        Console.Clear();
                        Console.Write("SETTING play area\n\n");
                        TextAnimateTime("Area must be bigger", 2000);
                    }
                    else if (int.Parse(cache) > 19)
                    {
                        Console.Clear();
                        Console.Write("SETTING play area\n\n");
                        TextAnimateTime("Area must be smaller", 2000);
                    }
                }
                else
                {
                    Console.Clear();
                    Console.Write("SETTING play area\n\n");
                    TextAnimateTime("Invalid input", 2000);
                }
            } while (!valid);

            // Clear the console screen and initialize the play area
            Console.Clear();
            Objects[,] playArea = new Objects[areaSizeX, areaSizeY];
            Console.Clear();
            Console.Write("SETTING play area\n\n");
            TextAnimateTime("Saved", 1000);
            return playArea;

        }
        #endregion

        #region Special Methods

        /// <summary>
        /// Checks whether the given coordinate is in the inner area
        /// </summary>
        /// <param name="_x">X coordinate</param>
        /// <param name="_y">Y coordinate</param>
        public static bool IsInnerArea(int _x, int _y)
        {
            if (_x > 0 && _x < areaSizeX - 1 && _y > 0 && _y < areaSizeY - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether the given coordinate is in the outer ring
        /// </summary>
        /// <param name="_x">X coordinate</param>
        /// <param name="_y">Y coordinate</param>
        public static bool IsOutterRing(int _x, int _y)
        {
            if (_x == 0 || _x == areaSizeX - 1 || _y == 0 || _y == areaSizeY - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Prints a string letter by letter with a small delay
        /// </summary>
        /// <param name="_input">String to print</param>
        public static void TextAnimate(string _input)
        {
            if (_input != null || _input != "")
            {

                char[] letters = _input.ToCharArray();
                string output = "";

                for (int i = 0; i < letters.Length; i++)
                {
                    Console.Write(letters[i]);
                    Thread.Sleep(15 / speed);
                }
            }

        }

        /// <summary>
        /// Similar to TextAnimate but clears console after a given time
        /// </summary>
        /// <param name="_input">String to print</param>
        /// <param name="_time">Time after console clear in ms</param>
        public static void TextAnimateTime(string _input, int _time)
        {
            TextAnimate(_input);
            Thread.Sleep(_time / speed);
            Console.Clear();
        }

        /// <summary>
        /// Changes the color of specific letters in a text line and prints the modified line to the console
        /// </summary>
        /// <param name="_input">The string to print</param>
        /// <param name="_changeLetters">An array of characters whose color needs to be changed</param>
        /// <param name="_colors">An array of ConsoleColor values corresponding to the colors of the characters in _changeLetters</param>
        public static void ColorLettersInTextLine(string _input, char[] _changeLetters, ConsoleColor[] _colors)
        {
            // Variable to determine whether to print a character normally
            bool printNormal;
            // Array to store the position of each character in _changeLetters within _input
            int[] positionOfReplacement = new int[_changeLetters.Length];

            // Determine the position of each character in _changeLetters within _input
            for (int i = 0; i < _changeLetters.Length; i++)
            {
                positionOfReplacement[i] = _input.IndexOf(_changeLetters[i]);
            }

            // Convert _input to a character array
            char[] letters = _input.ToCharArray();

            // Iterate over each character in _input
            for (int i = 0; i < letters.Length; i++)
            {
                printNormal = false;
                // Check if the current character should have its color changed
                for (int j = 0; j < positionOfReplacement.Length; j++)
                {
                    if (i == positionOfReplacement[j])
                    {
                        // Change the color of the character and print it
                        ConsoleColor original = Console.ForegroundColor;
                        Console.ForegroundColor = _colors[j];
                        Console.Write(letters[i]);
                        letters[i] = '\0'; // Set the character to null to prevent double printing
                        Console.ForegroundColor = original;
                    }
                    else
                    {
                        printNormal = true;
                    }
                }
                // If the character was not changed, print it normally
                if (printNormal == true)
                {
                    Console.Write(letters[i]);
                }
            }
            // Print a new line after processing the entire input line
            Console.WriteLine();
        }
        #endregion
    }
}