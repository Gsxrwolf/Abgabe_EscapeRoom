using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Escape_Room
{
    internal class Objects
    {
        private string type;
        private string lastType = "";

        public Objects(string _type)
        {
            type = _type;

        }

        public void ChangeObjectType(string _newType)
        {
            lastType = type;
            type = _newType;
        }

        public void RecoverLastType()
        {
            type = lastType;
            lastType = "";
        }

        public void DeleteLastType()
        {
            lastType = "";
        }

        public string GetLastType()
        {
            return lastType;
        }
        public string GetObjectType()
        {
            return type;
        }

    }

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

    public class InputManager
    {
        static Queue<ConsoleKeyInfo> inputBuffer = new Queue<ConsoleKeyInfo>();
        static object inputLock = new object();
        static int maxBufferSize = 1; 

        public InputManager()
        {
            Thread inputThread = new Thread(ReadInput);
            inputThread.Start();
        }

        private void ReadInput()
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    lock (inputLock)
                    {
                        inputBuffer.Enqueue(key);
                        while (inputBuffer.Count > maxBufferSize)
                        {
                            inputBuffer.Dequeue();
                        }
                    }
                }
            }
        }

        public bool KeyPressed()
        {
            lock (inputLock)
            {
                return inputBuffer.Count > 0;
            }
        }

        public ConsoleKeyInfo ReadKey()
        {
            lock (inputLock)
            {
                if (inputBuffer.Count > 0)
                {
                    return inputBuffer.Dequeue();
                }
                else
                {
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


        static void Main(string[] args)
        {

            Console.SetBufferSize(1500, 1500);
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            Console.CursorVisible = false;
            while (menu)
            {
                FrAnt = false;
                while (FrAnt == false)
                {
                    TextAnimate("MENU\n\n");
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
                                Console.Clear();
                                StartRound();
                                playGame = true;
                                FrAnt = true;
                                break;
                            }


                        case ConsoleKey.S:
                            {
                                Console.Clear();
                                PrintScoreboard();
                                FrAnt = true;
                                break;
                            }


                        case ConsoleKey.T:
                            {
                                Console.Clear();
                                Introduction();
                                tutorialDone = true;
                                FrAnt = true;
                                break;
                            }
                        case ConsoleKey.O:
                            {
                                Console.Clear();
                                playArea = DeclareGameArea();
                                settingDone = true;
                                FrAnt = true;
                                break;
                            }

                        case ConsoleKey.Q:
                            {
                                Console.Clear();
                                TextAnimateTime("Ok bye", 2000);
                                Console.Clear();
                                menu = false;
                                FrAnt = true;
                                break;
                            }
                        default:
                            {

                                Console.Clear();
                                TextAnimateTime("Invalid input please try again", 1000);
                                FrAnt = false;
                            }
                            break;
                    }
                }
            }
        }

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

        public static Objects[,] DeclareGameArea()
        {

            Console.Clear();
            TextAnimate("OPTIONS\n\n");
            bool valid = false;
            do
            {
                TextAnimate("Set a positiv number for the size of the square bigger than 3: ");
                cache = Console.ReadLine().Trim();
                if (int.TryParse(cache, out int result))
                {
                    if (int.Parse(cache) > 3)
                    {
                        areaSizeX = int.Parse(cache);
                        areaSizeY = int.Parse(cache);
                        size = int.Parse(cache);
                        valid = true;
                    }
                    else
                    {
                        Console.Clear();
                        TextAnimate("OPTIONS\n\n");
                        TextAnimateTime("Area must be bigger", 2000);
                    }
                }
                else
                {
                    Console.Clear();
                    TextAnimate("OPTIONS\n\n");
                    TextAnimateTime("Set a valid size", 2000);
                }
            } while (!valid);

            Console.Clear();


            Objects[,] playArea = new Objects[areaSizeX, areaSizeY];
            Console.Clear();
            TextAnimate("OPTIONS\n\n");
            TextAnimateTime("Saved", 1000);
            return playArea;

        }

        public static Objects[,] BuildArea(Objects[,] playArea)
        {
            playArea = BuildRoom(playArea);
            playArea = BuildInteractable(playArea);

            return playArea;
        }

        public static Objects[,] BuildRoom(Objects[,] playArea)
        {
            // Add Walls
            for (int y = 0; y < areaSizeX; y++)
            {
                for (int x = 0; x < areaSizeY; x++)
                {
                    if (IsOutterRing(x, y))
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

        public static Objects[,] BuildInteractable(Objects[,] playArea)
        {
            Random rnd = new Random();
            int x;
            int y;
            // Add Player
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



            // Add Key
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


            // Add Door
            done = false;
            do
            {
                x = rnd.Next(areaSizeX);
                y = rnd.Next(areaSizeY);
                if (IsOutterRing(x, y) && (0 < x && x < areaSizeX - 1 || 0 < y && y < areaSizeY - 1))
                {
                    playArea[x, y].ChangeObjectType("Door");
                    doorXPosition = x;
                    doorYPosition = y;
                    done = true;
                }
            } while (!IsOutterRing(x, y) || done == false);

            return playArea;
        }

        public static Objects[,] PlayerController(Objects[,] playArea)
        {
            ConsoleKeyInfo input = new ConsoleKeyInfo();
            if (inputManager.KeyPressed())
            {
                input = inputManager.ReadKey();
            }
            switch (input.Key)
            {
                case ConsoleKey.Escape:
                    {
                        stopwatch.Stop();
                        roundRunning = false;
                        break;
                    }
                case ConsoleKey.UpArrow:
                    {
                        if (gotKey == false && playerXPosition == doorXPosition && playerYPosition - 1 == doorYPosition)
                        {
                            TextAnimateTime("First pic up the key", 1000);
                        }
                        else
                        {

                            if (playArea[playerXPosition, playerYPosition - 1].GetObjectType() == "Key")
                            {
                                playArea[playerXPosition, playerYPosition - 1].RecoverLastType();
                                playArea[playerXPosition, playerYPosition - 1].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerYPosition = playerYPosition - 1;
                                gotKey = true;
                                break;
                            }
                            else if (playArea[playerXPosition, playerYPosition - 1].GetObjectType() != "Wall")
                            {
                                playArea[playerXPosition, playerYPosition - 1].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerYPosition = playerYPosition - 1;

                                doUpdate = true;
                            }
                            if (gotKey == true && playerXPosition == doorXPosition && playerYPosition == doorYPosition)
                            {
                                roundRunning = false;
                            }
                        }
                        break;
                    }
                case ConsoleKey.W:
                    {
                        if (gotKey == false && playerXPosition == doorXPosition && playerYPosition - 1 == doorYPosition)
                        {
                            TextAnimateTime("First pic up the key", 1000);
                        }
                        else
                        {

                            if (playArea[playerXPosition, playerYPosition - 1].GetObjectType() == "Key")
                            {
                                playArea[playerXPosition, playerYPosition - 1].RecoverLastType();
                                playArea[playerXPosition, playerYPosition - 1].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerYPosition = playerYPosition - 1;
                                gotKey = true;
                                break;
                            }
                            else if (playArea[playerXPosition, playerYPosition - 1].GetObjectType() != "Wall")
                            {
                                playArea[playerXPosition, playerYPosition - 1].ChangeObjectType("Player");
                                playArea[playerXPosition, playerYPosition].RecoverLastType();
                                playerYPosition = playerYPosition - 1;

                                doUpdate = true;
                            }
                            if (gotKey == true && playerXPosition == doorXPosition && playerYPosition == doorYPosition)
                            {
                                roundRunning = false;
                            }
                        }
                        break;
                    }


                case ConsoleKey.DownArrow:
                    {
                        if (gotKey == false && playerXPosition == doorXPosition && playerYPosition + 1 == doorYPosition)
                        {
                            TextAnimateTime("First pic up the key", 1000);
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
                case ConsoleKey.S:
                    {


                        if (gotKey == false && playerXPosition == doorXPosition && playerYPosition + 1 == doorYPosition)
                        {
                            TextAnimateTime("First pic up the key", 1000);
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


                case ConsoleKey.RightArrow:
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


                case ConsoleKey.LeftArrow:
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

        public static void StartRound()
        {
            if (tutorialDone != true)
            {
                TextAnimateTime("Please read the tutorial first", 1000);
                playGame = false;
            }
            else if (settingDone != true)
            {

                TextAnimateTime("Please set a size for your game area in the options tab", 1000);
                playGame = false;
            }
            else
            {
                playGame = true;
            }
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
                stopwatch.Stop();
                time = ((double)stopwatch.Elapsed.TotalSeconds);
                stopwatch.Reset();
                Round round = new Round(size, time);
                scoreBoard.Add(round);
                Thread.Sleep(500);
                Console.Clear();
                TextAnimateTime("Congratulations you successfully escaped in " + time + " seconds", 2000);


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
                                TextAnimateTime("Pleas answer with [Y]es or [N]o", 1000);
                                FrAnt = false;
                            }
                            break;
                    }
                }


            }
        }

        public static void PrintPlayArea()
        {
            for (int y = 0; y < areaSizeX; y++)
            {
                string Line1 = "";
                string Line2 = "";
                string Line3 = "";
                for (int x = 0; x < areaSizeY; x++)
                {
                    Objects printObject = playArea[x, y];
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
                Console.WriteLine(Line1);
                if (gotKey == true)
                {
                    char[] array = { 'X', 'K' };
                    ConsoleColor[] clolorArray = { ConsoleColor.Green, ConsoleColor.Blue };
                    ColorLettersInTextLine(Line2, array, clolorArray);
                }
                else
                {
                    char[] replaceArray = { 'X', 'K' };
                    ConsoleColor[] clolorArray = { ConsoleColor.Red, ConsoleColor.Blue };
                    ColorLettersInTextLine(Line2, replaceArray, clolorArray);
                }
                Console.WriteLine(Line3);
            }
        }

        public static void PrintScoreboard()
        {
            int _size = 0;
            if (scoreBoard.Count == 0)
            {
                Console.Clear();
                Console.Write("SCOREBOARD\n\n");
                TextAnimateTime("No rounds saved", 1000);
                Console.Clear();
            }
            else
            {
                scoreBoard.Sort((r1, r2) => r1.GetTime().CompareTo(r2.GetTime()));

                FrAnt = false;
                while (FrAnt == false)
                {
                    Console.Clear();
                    Console.Write("SCOREBOARD\n\n");
                    TextAnimate("Do you want to see rounds with a specific play area size or all?\n");
                    TextAnimate("[A]ll, [S]pecific\n");
                    ConsoleKeyInfo input2 = Console.ReadKey(true);
                    switch (input2.Key)
                    {
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

                        default:
                            {

                                Console.Clear();
                                TextAnimateTime("Pleas answer with [A]ll or [S]pecific", 1000);
                                FrAnt = false;
                            }
                            break;
                    }
                }
            }
        }

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

        public static void TextAnimateTime(string _input, int _time)
        {
            TextAnimate(_input);
            Thread.Sleep(_time);
            Console.Clear();
        }

        public static void ColorLettersInTextLine(string _input, char[] _changeLetters, ConsoleColor[] _colors)
        {
            bool printNormal;
            int[] positionOfReplacement = new int[_changeLetters.Length];

            for (int i = 0; i < _changeLetters.Length; i++)
            {
                positionOfReplacement[i] = _input.IndexOf(_changeLetters[i]);
            }

            char[] letters = _input.ToCharArray();

            for (int i = 0; i < letters.Length; i++)
            {
                printNormal = false;
                for (int j = 0; j < positionOfReplacement.Length; j++)
                {
                    if (i == positionOfReplacement[j])
                    {
                        ConsoleColor original = Console.ForegroundColor;
                        Console.ForegroundColor = _colors[j];
                        Console.Write(letters[i]);
                        letters[i] = '\0';
                        Console.ForegroundColor = original;
                    }
                    else
                    {
                        printNormal = true;
                    }
                }
                if (printNormal == true)
                {
                    Console.Write(letters[i]);
                }
            }
            Console.WriteLine();
        }

    }
}