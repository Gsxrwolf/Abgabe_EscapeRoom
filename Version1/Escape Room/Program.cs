using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Escape_Room
{

    ///<summary>
    /// Represents objects within the play area
    ///</summary>
    internal class Objects
    {
        // Private fields to store the type and last type of the object
        private string type;
        private string lastType = "";

        ///<summary>
        /// Initializes a new instance of the Objects class with the given type
        ///</summary>
        public Objects(string _type)
        {
            type = _type;
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
        public string GetObjectType()
        {
            return type;
        }
    }


    /// <summary>
    /// Class to represent each round played to use for the scoreboard
    /// </summary>
    internal class Round
    {
        private double time;
        private int size;

        public Round(int _size, double _time)
        {
            time = _time;
            size = _size;
        }

        public double GetTime()
        {
            return time;
        }
        public int GetSize()
        {
            return size;
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

        static string cache;
        static int areaSizeX;
        static int areaSizeY;
        static int playerXPosition;
        static int playerYPosition;
        static int keyXPosition;
        static int keyYPosition;
        static int doorXPosition;
        static int doorYPosition;
        static bool menu = true;
        static bool tutorialDone = false;
        static bool settingDone = false;
        static bool FrAnt = false;
        static bool gotKey = false;
        static bool roundRunning = true;
        static bool playGame = true;
        static bool doUpdate;
        static Objects[,] playArea = new Objects[areaSizeX, areaSizeY];


        static Stopwatch stopwatch = new Stopwatch();
        static List<Round> scoreBoard = new List<Round>();
        static int size;
        static double time;

        static InputManager inputManager = new InputManager();


        ///<summary>
        /// Main entry point of the game and handles the main menu functionality and navigation
        ///</summary>
        static void Main(string[] args)
        {
            // Set console buffer size and window size for better display
            Console.SetBufferSize(2000, 2000);
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            // Hide the cursor to provide a cleaner interface
            Console.CursorVisible = false;

            // Loop until the user quits the game
            while (menu)
            {
                FrAnt = false;
                // Loop until a valid menu option is selected
                while (FrAnt == false)
                {
                    // Display the main menu options
                    TextAnimate("MENU\n\n");
                    TextAnimate("[P]lay\n");
                    TextAnimate("[S]coreboard\n");
                    TextAnimate("[T]utorial\n");
                    TextAnimate("[O]ptions\n");
                    TextAnimate("[Q]uit\n");
                    // Read user input
                    ConsoleKeyInfo input = Console.ReadKey(true);
                    switch (input.Key)
                    {
                        // Start a new round of the game
                        case ConsoleKey.P:
                            {
                                Console.Clear();
                                StartRound();
                                playGame = true;
                                FrAnt = true;
                                break;
                            }
                        // Display the scoreboard
                        case ConsoleKey.S:
                            {
                                Console.Clear();
                                PrintScoreboard();
                                FrAnt = true;
                                break;
                            }
                        // Display the tutorial
                        case ConsoleKey.T:
                            {
                                Console.Clear();
                                Introduction();
                                tutorialDone = true;
                                FrAnt = true;
                                break;
                            }
                        // Set game options
                        case ConsoleKey.O:
                            {
                                Console.Clear();
                                playArea = DeclareGameArea();
                                settingDone = true;
                                FrAnt = true;
                                break;
                            }
                        // Quit the game
                        case ConsoleKey.Q:
                            {
                                Console.Clear();
                                TextAnimateTime("Ok bye", 2000);
                                Console.Clear();
                                menu = false;
                                FrAnt = true;
                                break;
                            }
                        // Handle invalid input
                        default:
                            {
                                Console.Clear();
                                TextAnimateTime("Invalid input please try again", 1000);
                                FrAnt = false;
                                break;
                            }
                    }
                }
            }
        }


        ///<summary>
        ///Provides an introduction and tutorial for the game, explaining controls, objectives, and options.
        ///</summary>
        public static void Introduction()
        {
            Console.Write("TUTORIAL\n\n");
            TextAnimate("Controll Player (x) with w,a,s,d or the arrow keys\n");
            Thread.Sleep(1000);
            TextAnimate("Grab the key (K) and escape through the door\n");
            Thread.Sleep(1000);
            TextAnimate("Press esc to end the round and get back to the menu\n\n");
            Thread.Sleep(1000);
            TextAnimate("Your play area is a square but you can choose how big the square should be as long its bigger than 3x3\n");
            Thread.Sleep(1000);
            TextAnimate("In the options tab you can choose how big the area is you are playing on\n\n");
            Thread.Sleep(1000);
            TextAnimate("Your time to escape gets tracked and is shown after each round\n");
            Thread.Sleep(1000);
            TextAnimate("You can also compare your time in the scoreboard tab\n");
            Thread.Sleep(2000);
            TextAnimate("\n\nPress any key");
            Console.ReadKey(true);
            Console.Clear();
        }

        ///<summary>
        ///Declares the play area dimensions based on user input and initializes the play area
        ///</summary>
        ///<returns>A 2D array representing the play area</returns>
        public static Objects[,] DeclareGameArea()
        {
            Console.Clear();
            TextAnimate("OPTIONS\n\n");

            bool valid = false;

            do
            {
                // Prompt the user to set the size of the play area
                TextAnimate("Set a positive number for the size of the square bigger than 3: ");
                cache = Console.ReadLine().Trim();

                if (int.TryParse(cache, out int result))
                {
                    if (int.Parse(cache) > 3)
                    {
                        // Set the dimensions of the play area and mark input as valid
                        areaSizeX = int.Parse(cache);
                        areaSizeY = int.Parse(cache);
                        size = int.Parse(cache);
                        valid = true;
                    }
                    else
                    {
                        // Display error message if area size is too small
                        Console.Clear();
                        TextAnimate("OPTIONS\n\n");
                        TextAnimateTime("Area must be bigger", 2000);
                    }
                }
                else
                {
                    // Display error message if input is invalid
                    Console.Clear();
                    TextAnimate("OPTIONS\n\n");
                    TextAnimateTime("Set a valid size", 2000);
                }
            } while (!valid);

            // Clear the console screen and initialize the play area
            Console.Clear();
            Objects[,] playArea = new Objects[areaSizeX, areaSizeY];
            Console.Clear();
            TextAnimate("OPTIONS\n\n");
            TextAnimateTime("Saved", 1000);
            return playArea;
        }



        ///<summary>
        ///Using BuildRoom and PlaceInteractables to generate the play area
        ///</summary>
        ///<param name="playArea">The 2D array representing the play area</param>
        ///<returns>The finished play area after adding all objects</returns>
        public static Objects[,] BuildArea(Objects[,] playArea)
        {
            playArea = BuildRoom(playArea);
            playArea = PlaceInteractable(playArea);

            return playArea;
        }

        ///<summary>
        ///Generates the basic parts of the room
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
                    if (IsOuterRing(x, y))
                    {
                        Objects wall = new Objects("Wall");
                        playArea[x, y] = wall;
                    }
                    else if (IsInnerArea(x, y))
                    {
                        Objects floor = new Objects("Floor");
                        playArea[x, y] = floor;
                    }
                }
            }
            return playArea;
        }

        ///<summary>
        ///Adds interactable objects including player, key, and door
        ///</summary>
        ///<param name="playArea">The 2D array representing the play area</param>
        ///<returns>The updated play area with added interactable objects</returns>
        public static Objects[,] PlaceInteractable(Objects[,] playArea)
        {
            Random rnd = new Random();
            int x;
            int y;
            // Add Player at valid random place
            do
            {
                x = rnd.Next(areaSizeX);
                y = rnd.Next(areaSizeY);
                if (IsInnerArea(x, y))
                {
                    playArea[x, y].ChangeObjectType("Player");
                    playerXPosition = x;
                    playerYPosition = y;
                }
            } while (!IsInnerArea(x, y));



            // Add Key at valid random place
            bool done = false;
            do
            {
                x = rnd.Next(areaSizeX);
                y = rnd.Next(areaSizeY);
                if (IsInnerArea(x, y) && playerXPosition != x && playerYPosition != y)
                {
                    playArea[x, y].ChangeObjectType("Key");
                    keyXPosition = x;
                    keyYPosition = y;
                    done = true;
                }
            } while (!IsInnerArea(x, y) || done == false);


            //Add Door at valid random place
            done = false;
            do
            {
                x = rnd.Next(areaSizeX);
                y = rnd.Next(areaSizeY);
                if (IsOuterRing(x, y) && (0 < x && x < areaSizeX - 1 || 0 < y && y < areaSizeY - 1))
                {
                    playArea[x, y].ChangeObjectType("Door");
                    doorXPosition = x;
                    doorYPosition = y;
                    done = true;
                }
            } while (!IsOuterRing(x, y) || done == false);

            return playArea;
        }


        /// <summary>
        /// Controls the movement of the player within the play area based on user input
        /// </summary>
        /// <param name="playArea">The 2D array representing the play area</param>
        /// <returns>The updated play area after player movement</returns>
        public static Objects[,] PlayerController(Objects[,] playArea)
        {
            //Gets player input from the InputManager
            ConsoleKeyInfo input = new ConsoleKeyInfo();
            if (inputManager.KeyPressed())
            {
                input = inputManager.ReadKey();
            }
            //Checks the input and performs corresponding actions
            switch (input.Key)
            {
                case ConsoleKey.Escape:
                    {
                        stopwatch.Stop();
                        roundRunning = false;
                        break;
                    }
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    {
                        // Check if the player does not have the key and is trying to move to the door position
                        if (gotKey == false && playerXPosition == doorXPosition && playerYPosition - 1 == doorYPosition)
                        {
                            // Display a message prompting the player to pick up the key first
                            TextAnimateTime("First pick up the key", 1000);
                        }
                        else
                        {
                            // Check if the object above the player is a key
                            if (playArea[playerXPosition, playerYPosition - 1].GetObjectType() == "Key")
                            {
                                // Replace the key object with the player object
                                playArea[playerXPosition, playerYPosition - 1].RecoverLastType();
                                playArea[playerXPosition, playerYPosition - 1].ChangeObjectType("Player");
                                // Update player position and indicate that the player has the key
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerYPosition = playerYPosition - 1;
                                gotKey = true;
                                break;
                            }
                            // Check if the player can move to the position above
                            else if (playArea[playerXPosition, playerYPosition - 1].GetObjectType() != "Wall")
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
                            TextAnimateTime("First pick up the key", 1000);
                        }
                        else
                        {
                            if (playArea[playerXPosition, playerYPosition + 1].GetObjectType() == "Key")
                            {
                                playArea[playerXPosition, playerYPosition + 1].RecoverLastType();
                                playArea[playerXPosition, playerYPosition + 1].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerYPosition = playerYPosition + 1;
                                gotKey = true;
                                break;
                            }
                            else if (playArea[playerXPosition, playerYPosition + 1].GetObjectType() != "Wall")
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

                            if (playArea[playerXPosition + 1, playerYPosition].GetObjectType() == "Key")
                            {
                                playArea[playerXPosition + 1, playerYPosition].RecoverLastType();
                                playArea[playerXPosition + 1, playerYPosition].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerXPosition = playerXPosition + 1;
                                gotKey = true;
                                break;
                            }
                            else if (playArea[playerXPosition + 1, playerYPosition].GetObjectType() != "Wall")
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

                            if (playArea[playerXPosition - 1, playerYPosition].GetObjectType() == "Key")
                            {
                                playArea[playerXPosition - 1, playerYPosition].RecoverLastType();
                                playArea[playerXPosition - 1, playerYPosition].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerXPosition = playerXPosition - 1;
                                gotKey = true;
                                break;
                            }
                            else if (playArea[playerXPosition - 1, playerYPosition].GetObjectType() != "Wall")
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
        /// Initiates a new round of the game
        /// </summary>
        public static void StartRound()
        {
            // Check if the tutorial is completed
            if (tutorialDone != true)
            {
                TextAnimateTime("Please read the tutorial first", 1000);
                playGame = false;
            }
            // Check if play area size is set
            else if (settingDone != true)
            {
                TextAnimateTime("Please set a size for your play area in the options tab", 1000);
                playGame = false;
            }
            else
            {
                playGame = true;
            }

            // Loop for each round of the game
            while (playGame)
            {
                playArea = BuildArea(playArea);
                TextAnimateTime("5", 1000);
                TextAnimateTime("4", 1000);
                TextAnimateTime("3", 1000);
                TextAnimateTime("2", 1000);
                TextAnimateTime("1", 1000);
                TextAnimateTime("Go", 1000);
                PrintPlayArea();

                // Loop for each update of the round
                while (roundRunning)
                {
                    stopwatch.Start();
                    PlayerController(playArea);
                    if (doUpdate = true)
                    {
                        Console.SetCursorPosition(0, 0);
                        PrintPlayArea();
                        doUpdate = false;
                    }
                }

                // Stop stopwatch and calculate time taken for the round
                stopwatch.Stop();
                time = ((double)stopwatch.Elapsed.TotalSeconds);
                stopwatch.Reset();

                // Create a new round object with size and time, and add it to the scoreboard
                Round round = new Round(size, time);
                scoreBoard.Add(round);

                // Wait for a moment and then clear the console
                Thread.Sleep(500);
                Console.Clear();
                TextAnimateTime("Congratulations you successfully escaped in " + time + " seconds", 2000);

                // Prompt the player to start a new round or not
                FrAnt = false;
                while (FrAnt == false)
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
                                FrAnt = true;
                                Console.Clear();
                                break;
                            }
                        case ConsoleKey.N:
                            {
                                Console.Clear();
                                TextAnimateTime("Ok", 2000);
                                playGame = false;
                                roundRunning = true;
                                gotKey = false;
                                FrAnt = true;
                                Console.Clear();
                                break;
                            }
                        default:
                            {
                                Console.Clear();
                                TextAnimateTime("Please answer with [Y]es or [N]o", 1000);
                                FrAnt = false;
                            }
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Prints the play area grid with different objects represented by characters.
        /// </summary>
        public static void PrintPlayArea()
        {
            // Loop through each row of the play area
            for (int y = 0; y < areaSizeX; y++)
            {
                string Line1 = "";
                string Line2 = "";
                string Line3 = "";

                // Loop through each column of the play area
                for (int x = 0; x < areaSizeY; x++)
                {
                    // Get the object at the current position
                    Objects printObject = playArea[x, y];

                    // Determine the type of object and represent it accordingly
                    if (printObject.GetObjectType() == "Player")
                    {
                        Line1 = Line1 + "  ---  ";
                        Line2 = Line2 + " | X | ";
                        Line3 = Line3 + "  ---  ";
                    }
                    else if (printObject.GetObjectType() == "Key")
                    {
                        Line1 = Line1 + "  ---  ";
                        Line2 = Line2 + " | K | ";
                        Line3 = Line3 + "  ---  ";
                    }
                    else if (printObject.GetObjectType() == "Door")
                    {
                        Line1 = Line1 + "  ---  ";
                        Line2 = Line2 + " |   | ";
                        Line3 = Line3 + " |  °| ";
                    }
                    else if (printObject.GetObjectType() == "Wall")
                    {
                        Line1 = Line1 + " ■ ■ ■ ";
                        Line2 = Line2 + " ■ ■ ■ ";
                        Line3 = Line3 + " ■ ■ ■ ";
                    }
                    else if (printObject.GetObjectType() == "Floor")
                    {
                        Line1 = Line1 + "  ---  ";
                        Line2 = Line2 + " |   | ";
                        Line3 = Line3 + "  ---  ";
                    }
                }

                // Print the lines representing each row of the play area
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

                // Print the lines representing each row of the play area
                Console.WriteLine(Line3);
            }
        }


        /// <summary>
        /// Prints the scoreboard showing rounds played on different play area sizes.
        /// Allows the user to choose between viewing all rounds or rounds of a specific size.
        /// </summary>
        public static void PrintScoreboard()
        {
            int _size = 0;

            // If no rounds are saved display a message indicating so
            if (scoreBoard.Count == 0)
            {
                Console.Clear();
                Console.Write("SCOREBOARD\n\n");
                TextAnimateTime("No rounds saved", 1000);
                Console.Clear();
            }
            else
            {
                // Sort the scoreboard based on round completion time
                scoreBoard.Sort((r1, r2) => r1.GetTime().CompareTo(r2.GetTime()));

                FrAnt = false;
                // Display options until the user makes a valid choice
                while (FrAnt == false)
                {
                    Console.Clear();
                    Console.Write("SCOREBOARD\n\n");
                    TextAnimate("Do you want to see rounds with a specific play area size or all?\n");
                    TextAnimate("[A]ll, [S]pecific\n");
                    ConsoleKeyInfo input2 = Console.ReadKey(true);
                    switch (input2.Key)
                    {
                        // Display all rounds on the scoreboard
                        case ConsoleKey.A:
                            {
                                FrAnt = true;
                                Console.Clear();
                                Console.Write("SCOREBOARD\n\n");
                                int counter = 1;
                                foreach (Round round in scoreBoard)
                                {
                                    TextAnimate(counter + ": " + round.GetTime() + " seconds on a " + round.GetSize() + "x" + round.GetSize() + " field\n");
                                    counter++;
                                }
                                TextAnimate("\n\nPress any key");
                                Console.ReadKey(true);
                                counter = 1;
                                Console.Clear();
                                break;
                            }

                        // Display rounds for a specific play area size chosen by the user
                        case ConsoleKey.S:
                            {
                                FrAnt = true;
                                Console.Clear();
                                Console.Write("SCOREBOARD\n\n");
                                TextAnimate("Which play area size do you want to display?\n");
                                TextAnimate("[3],[4],[5],[...]                   press enter\n");
                                cache = Console.ReadLine().Trim();
                                if (int.TryParse(cache, out int result))
                                {
                                    _size = int.Parse(cache);
                                    int index = scoreBoard.FindIndex(Round => Round.GetSize() == _size);
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
                                                TextAnimate(counter + ": " + round.GetTime() + " seconds\n");
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

                        // Prompt the user to choose either all or specific rounds
                        default:
                            {
                                Console.Clear();
                                TextAnimateTime("Please answer with [A]ll or [S]pecific", 1000);
                                FrAnt = false;
                            }
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Checks whether the given coordinate is in the inner area
        /// </summary>
        /// <param name="_x">X coordinate</param>
        /// <param name="_y">Y coordinate</param>
        public static bool IsInnerArea(int _x, int _y)
        {
            if (_x == 0 || _x == areaSizeX - 1 || _y == 0 || _y == areaSizeY - 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks whether the given coordinate is in the outer ring
        /// </summary>
        /// <param name="_x">X coordinate</param>
        /// <param name="_y">Y coordinate</param>
        public static bool IsOuterRing(int _x, int _y)
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
                    Thread.Sleep(20);
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
            Thread.Sleep(_time);
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


    }
}