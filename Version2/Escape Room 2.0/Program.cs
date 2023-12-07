using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Escape_Room_2._0
{
    internal class Objects
    {
        private string type;
        private string lastType = "";
        private int xPos;
        private int yPos;


        public Objects(string _type, int x, int y)
        {
            type = _type;
            xPos = x;
            yPos = y;

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
        public int GetXPos()
        {
            return xPos;
        }

        public int GetYPos()
        {
            return yPos;
        }

    }

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

        static void Main(string[] args)
        {
            /*
            #region Debug settings
            speed = 40;
            tutorialDone = true;
            #endregion
            */

            Console.SetBufferSize(2000, 2000);
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            while (menu)
            {
                Console.CursorVisible = false;
                validInput = false;
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
                                    TextAnimateTime("Please set a size for your game area in the options tab", 1000);
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
                                Console.CursorVisible = false;
                                Console.Clear();
                                PrintScoreboard();
                                scoreboard = true;
                                validInput = true;
                                break;
                            }


                        case ConsoleKey.T:
                            {
                                Console.CursorVisible = false;
                                Console.Clear();
                                Tutorial();
                                tutorialDone = true;
                                validInput = true;
                                break;
                            }
                        case ConsoleKey.O:
                            {
                                Console.CursorVisible = false;
                                Console.Clear();
                                playArea = Options();
                                options = true;
                                validInput = true;
                                break;
                            }

                        case ConsoleKey.Q:
                            {
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
                                Console.CursorVisible = false;
                                Console.Clear();
                                Console.Write("MENU\n\n");
                                TextAnimateTime("Invalid input please try again", 1000);
                                validInput = false;
                            }
                            break;
                    }
                }
            }
        }

        #region Play
        public static void StartRound()
        {
            while (playGame)
            {
                playArea = BuildArea(playArea);
                TextAnimateTime("3", 1000);
                TextAnimateTime("2", 1000);
                TextAnimateTime("1", 1000);
                TextAnimateTime("Go", 1000);
                DisplayManeger(difficultyLevel);
                saveTime = true;
                while (roundRunning)
                {
                    stopwatch.Start();
                    PlayerController(playArea);
                    if (doUpdate = true)
                    {
                        Console.SetCursorPosition(0, 0);
                        DisplayManeger(difficultyLevel);
                        doUpdate = false;
                    }
                }
                stopwatch.Stop();
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
                    TextAnimateTime("You did'n escaped the room", 2000);
                }
                stopwatch.Reset();



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
                                TextAnimateTime("Pleas answer with [Y]es or [N]o", 1000);
                                validInput = false;
                            }
                            break;
                    }
                }


            }
        }
        public static Objects[,] BuildArea(Objects[,] playArea)
        {
            playArea = BuildRoom(playArea);
            if (difficultyLevel > 1)
            {
                BuildMaze(playArea);
            }
            playArea = BuildInteractable(playArea);



            return playArea;
        }
        public static Objects[,] BuildRoom(Objects[,] playArea)
        {
            // Add Walls and Floor
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

            // Add Door
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
        public static Objects[,] BuildInteractable(Objects[,] playArea)
        {
            Random rnd = new Random();
            int x;
            int y;
            // Add Player
            bool done = false;
            do
            {
                x = rnd.Next(areaSizeX);
                y = rnd.Next(areaSizeY);
                if (IsInnerArea(x, y) && playArea[x, y].GetObjectType() == "Floor")
                {
                    playArea[x, y].ChangeObjectType("Player");
                    playerXPosition = x;
                    playerYPosition = y;
                    done = true;
                }
            } while (!IsInnerArea(x, y) || done == false);



            // Add Key
            done = false;
            do
            {
                x = rnd.Next(areaSizeX);
                y = rnd.Next(areaSizeY);
                if (IsInnerArea(x, y) && playArea[x, y].GetObjectType() == "Floor")
                {
                    playArea[x, y].ChangeObjectType("Key");
                    done = true;
                }
            } while (!IsInnerArea(x, y) || done == false);
            return playArea;
        }


        public static Objects[,] BuildMaze(Objects[,] playArea)
        {
            int mazeBlocksToPlace = ((areaSizeX - 2) * (areaSizeY - 2) * 2);
            bool xPlusChecked = false;
            bool xMinusChecked = false;
            bool yPlusChecked = false;
            bool yMinusChecked = false;


            FillInnerArea();
            GetMazeStartPosition();
            ghostXPos = mazeStartXPosition;
            ghostYPos = mazeStartYPosition;
            counterPlaced = 0;

            SafeGhostMovement();

            while (counterPlaced != mazeBlocksToPlace)
            {
                GenerateValidRandomMove(ref xPlusChecked, ref xMinusChecked, ref yPlusChecked, ref yMinusChecked, mazeBlocksToPlace);
                if (IsInnerArea(ghostXPos, ghostYPos) && !visitedObjects.Contains(playArea[ghostXPos, ghostYPos]) && !visitedObjects.Contains(playArea[ghostXPos + ghostYDifferent, ghostYPos + ghostXDifferent]))
                {
                    SafeGhostMovement();
                    xPlusChecked = false;
                    xMinusChecked = false;
                    yPlusChecked = false;
                    yMinusChecked = false;

                    RepeatMove();
                    if (IsInnerArea(ghostXPos, ghostYPos) && !visitedObjects.Contains(playArea[ghostXPos, ghostYPos]))
                    {
                        SafeGhostMovement();
                    }
                    else
                    {
                        ReverseMove();
                    }
                }
                else
                {
                    ReverseMove();
                }


            }

            visitedObjects.Clear();
            mazeObjectsStack.Clear();
            xPlusChecked = false;
            xMinusChecked = false;
            yPlusChecked = false;
            yMinusChecked = false;
            return playArea;
        }
        public static void GenerateValidRandomMove(ref bool _yPlusChecked, ref bool _yMinusChecked, ref bool _xPlusChecked, ref bool _xMinusChecked, int _mazeBlocksToPlace)
        {
            Random rnd = new Random();
            int newghostXPos = 0;
            int newghostYPos = 0;
            int errorOverflow = 0;
            newghostXPos = ghostXPos;
            newghostYPos = ghostYPos;


            do
            {
                if (counterPlaced == _mazeBlocksToPlace)
                {
                    break;
                }

                ghostPlusOrMinus = rnd.Next(0, 2);
                ghostUpDownOrLeftRight = rnd.Next(0, 2);

                if (ghostUpDownOrLeftRight == 1)
                {
                    if (ghostPlusOrMinus == 1 && _xMinusChecked == false)
                    {
                        newghostXPos = ghostXPos - 1;
                        ghostXDifferent = -1;
                        _xMinusChecked = true;
                    }
                    if (ghostPlusOrMinus == 0 && _xPlusChecked == false)
                    {
                        newghostXPos = ghostXPos + 1;
                        ghostXDifferent = 1;
                        _xPlusChecked = true;
                    }
                }
                if (ghostUpDownOrLeftRight == 0)
                {
                    if (ghostPlusOrMinus == 1 && _yMinusChecked == false)
                    {
                        newghostYPos = ghostYPos - 1;
                        ghostYDifferent = -1;
                        _yMinusChecked = true;
                    }
                    if (ghostPlusOrMinus == 0 && _yPlusChecked == false)
                    {
                        newghostYPos = ghostYPos + 1;
                        ghostYDifferent = 1;
                        _yPlusChecked = true;
                    }
                }

                if (!IsInnerArea(newghostXPos, newghostYPos) || visitedObjects.Contains(playArea[newghostXPos, newghostYPos]))
                {
                    newghostXPos = ghostXPos;
                    newghostYPos = ghostYPos;
                }
                if (_xPlusChecked == true && _xMinusChecked == true && _yPlusChecked == true && _yMinusChecked == true)
                {
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
        public static void SafeGhostMovement()
        {
            if (ghostXPos > 0 && ghostXPos < areaSizeX && ghostYPos > 0 && ghostYPos < areaSizeY)
            {
                playArea[ghostXPos, ghostYPos].ChangeObjectType("Floor");
                visitedObjects.Add(playArea[ghostXPos, ghostYPos]);
                mazeObjectsStack.Push(playArea[ghostXPos, ghostYPos]);
                counterPlaced++;
            }
        }
        public static void RepeatMove()
        {
            if (ghostUpDownOrLeftRight == 1)
            {
                if (ghostPlusOrMinus == 1)
                {
                    ghostXPos--;
                }
                if (ghostPlusOrMinus == 0)
                {
                    ghostXPos++;
                }
            }
            if (ghostUpDownOrLeftRight == 0)
            {
                if (ghostPlusOrMinus == 1)
                {
                    ghostYPos--;
                }
                if (ghostPlusOrMinus == 0)
                {
                    ghostYPos++;
                }
            }
        }
        public static void ReverseMove()
        {
            if (ghostUpDownOrLeftRight == 1)
            {
                if (ghostPlusOrMinus == 1)
                {
                    ghostXPos++;
                }
                if (ghostPlusOrMinus == 0)
                {
                    ghostXPos--;
                }
            }
            if (ghostUpDownOrLeftRight == 0)
            {
                if (ghostPlusOrMinus == 1)
                {
                    ghostYPos++;
                }
                if (ghostPlusOrMinus == 0)
                {
                    ghostYPos--;
                }
            }
        }
        public static void FillInnerArea()
        {
            for (int y = 1; y < areaSizeX - 2; y++)
            {
                for (int x = 1; x < areaSizeY - 2; x++)
                {
                    playArea[x, y].ChangeObjectType("Wall");
                }
            }
        }
        public static void GetMazeStartPosition()
        {
            bool found = false;
            while (!found)
            {
                for (int y = 1; y < areaSizeX - 1; y++)
                {
                    for (int x = 1; x < areaSizeY - 1; x++)
                    {
                        if (playArea[x - 1, y].GetObjectType() == "Door" || playArea[x + 1, y].GetObjectType() == "Door" || playArea[x, y - 1].GetObjectType() == "Door" || playArea[x, y + 1].GetObjectType() == "Door")
                        {
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
                        saveTime = false;
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


        public static void DisplayManeger(int _difficulty)
        {
            if (_difficulty == 1)
            {
                PrintPlayerArea(4);
            }
            else if (_difficulty == 2)
            {
                PrintPlayerArea(6);
            }
            else if (_difficulty == 3)
            {
                PrintPlayerArea(2.5);
            }
            else if (_difficulty == 4)
            {
                PrintPlayerArea(2);
            }
            else if (_difficulty == 5)
            {
                PrintPlayerArea(1.4);
            }
        }
        public static void PrintPlayerArea(double _difficulty)
        {

            for (int y = 0; y < areaSizeX; y++)
            {
                string Line1 = "";
                string Line2 = "";
                string Line3 = "";
                for (int x = 0; x < areaSizeY; x++)
                {
                    int XDistance = playerXPosition - x;
                    int YDistance = playerYPosition - y;
                    double Distance = Math.Sqrt(Math.Pow(XDistance, 2) + Math.Pow(YDistance, 2));
                    if (Distance < _difficulty)
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
                    else
                    {
                        Line1 = Line1 + "       ";
                        Line2 = Line2 + "       ";
                        Line3 = Line3 + "       ";
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
        #endregion

        #region Scoreboard
        public static void PrintScoreboard()
        {
            while (scoreboard)

            {
                int _size = 0;
                int _difficulty = 0;
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
                    scoreBoard.Sort((r1, r2) => r1.GetTime().CompareTo(r2.GetTime()));

                    validInput = false;
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
                                }
                                break;
                        }
                    }
                }
            }
        }
        #endregion

        #region Tutorial
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
        public static Objects[,] Options()
        {
            while (options)
            {


                validInput = false;
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
                                playArea = SetGameAreaSize();
                                settingDone = true;
                                validInput = true;
                                break;
                            }


                        case ConsoleKey.D:
                            {
                                SetGameDifficulty();
                                validInput = true;
                                break;
                            }

                        case ConsoleKey.M:
                            {
                                Console.Clear();
                                options = false;
                                validInput = true;
                                break;
                            }


                        default:
                            {

                                Console.Clear();
                                Console.Write("OPTIONS\n\n");
                                TextAnimateTime("Invalid input please try again", 1000);
                                validInput = false;
                            }
                            break;
                    }
                }
            }

            return playArea;
        }
        public static Objects[,] SetGameDifficulty()
        {

            bool valid = false;
            do
            {
                Console.Clear();
                Console.Write("SETTING GAME DIFFICULTY\n\n");
                TextAnimate("Set a positiv number for the difficulty 1-5: ");
                cache = Console.ReadLine().Trim();
                if (int.TryParse(cache, out int result))
                {
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


            Objects[,] playArea = new Objects[areaSizeX, areaSizeY];
            Console.Clear();
            Console.Write("SETTING GAME DIFFICULTY\n\n");
            TextAnimateTime("Saved", 1000);
            return playArea;

        }
        public static Objects[,] SetGameAreaSize()
        {

            bool valid = false;
            do
            {
                Console.Clear();
                Console.Write("SETTING GAME AREA\n\n");
                TextAnimate("Set a positiv number for the size of the square between 3 and 20: ");
                cache = Console.ReadLine().Trim();
                if (int.TryParse(cache, out int result))
                {
                    if (int.Parse(cache) > 3 && int.Parse(cache) < 20)
                    {
                        areaSizeX = int.Parse(cache);
                        areaSizeY = int.Parse(cache);
                        size = int.Parse(cache);
                        valid = true;
                    }
                    else if (int.Parse(cache) < 4)
                    {
                        Console.Clear();
                        Console.Write("SETTING GAME AREA\n\n");
                        TextAnimateTime("Area must be bigger", 2000);
                    }
                    else if (int.Parse(cache) > 19)
                    {
                        Console.Clear();
                        Console.Write("SETTING GAME AREA\n\n");
                        TextAnimateTime("Area must be smaller", 2000);
                    }
                }
                else
                {
                    Console.Clear();
                    Console.Write("SETTING GAME AREA\n\n");
                    TextAnimateTime("Invalid input", 2000);
                }
            } while (!valid);

            Console.Clear();


            Objects[,] playArea = new Objects[areaSizeX, areaSizeY];
            Console.Clear();
            Console.Write("SETTING GAME AREA\n\n");
            TextAnimateTime("Saved", 1000);
            return playArea;

        }
        #endregion

        #region Special Methods
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
                    Thread.Sleep(15 / speed);
                }
            }

        }
        public static void TextAnimateTime(string _input, int _time)
        {
            TextAnimate(_input);
            Thread.Sleep(_time / speed);
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
        #endregion
    }
}