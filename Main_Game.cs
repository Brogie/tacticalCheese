//intentional setup for merge confilict
﻿using Gameboard_Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UserInputClass;

namespace MainGame
{
    /// <summary>
    /// This holds the main game loop and some methods for presentation
    /// </summary>
    public class TacticalCheese
    {
        #region Global Variables

        /// <summary>
        /// The current dubug state of the program
        /// </summary>
        private static bool debugMode             = false;

        /// <summary>
        /// The randomiser that all the dice will get their rolls from
        /// </summary>
        private static Random diceRandomiser      = new Random();

        /// <summary>
        /// The list that holds all the players after they have been created
        /// </summary>
        private static List<Player> listOfPlayers = new List<Player>();

        /// <summary>
        /// If the game is loaded from a save state this will be the starting player
        /// </summary>
        private static int resumePlayer           = 0;

        /// <summary>
        /// The height of the screen in characters
        /// </summary>
        private static int SCREENHEIGHT           = 58;

        /// <summary>
        /// The width of the screen in characters
        /// </summary>
        private static int SCREENWIDTH            = 128;

        /// <summary>
        /// The current gamestate the program is in (will be used in a switch in
        /// the main loop)
        /// </summary>
        private static GameState state            = GameState.GAMEMENU;

        /// <summary>
        /// The name of the player who wins
        /// </summary>
        private static string winnerName          = "";

        #endregion Global Variables

        /// <summary>
        /// The game state enumerator is what I use to track what part of the
        /// game the user wants to be in.
        /// </summary>
        private enum GameState
        {
            /// <summary>
            /// The game menu state makes the game open the main menu method so
            /// the player can play/load the game, toggle the debug settings or
            /// quit the game
            /// </summary>
            GAMEMENU,

            /// <summary>
            /// The play game state is where the game actually begins, all the
            /// relevent game logic will be running when the game is set to this state.
            /// </summary>
            PLAYGAME,

            /// <summary>
            /// The player selection state is used to allow players to input
            /// their player information
            /// </summary>
            PLAYERSELECTION,

            /// <summary>
            /// The Load game state will load the previous game from a text file
            /// </summary>
            LOADGAME,

            /// <summary>
            /// When the game finishes this will display the winner of the game
            /// </summary>
            GAMEOVER
        }

        /// <summary>
        /// The Player struct holds the information regarding each player on the board
        /// </summary>
        public struct Player
        {
            /// <summary>
            /// Is the colour of the Ship that the player will have
            /// </summary>
            public int colour;

            /// <summary>
            /// Holds the name of the player
            /// </summary>
            public string name;

            /// <summary>
            /// Will hold a unique number for each player on the board
            /// </summary>
            public int number;

            /// <summary>
            /// Holds the cell poition of the player (where they are on the board)
            /// </summary>
            public int position;

            /// <summary>
            /// Will be the player character (what the player looks like on the board)
            /// </summary>
            public char ship;

            /// <summary>
            /// This constructor sets up all the values in the player struct.
            /// </summary>
            /// <param name="inPlayerNumber">
            /// Will set a unique number for each player on the board
            /// </param>
            /// <param name="inName">Sets name of player</param>
            /// <param name="inShip">Sets how the ship will look</param>
            /// <param name="inPosition">
            /// Sets the position of the player (defaults to 0)
            /// </param>
            public Player(int inPlayerNumber, string inName, char inShip, int inPosition = 0)
            {
                number = inPlayerNumber;
                name = inName;
                position = inPosition;
                colour = inPlayerNumber + 3;
                ship = inShip;
            }
        }

        /// <summary>
        /// The Main method is where the console is setup and where the main
        /// loop is executed.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>0 so that i can tell the program exited correctly.</returns>
        private static int Main(string[] args)
        {
            //Declare variables (most variables are already declared in the global variable section)
            bool quitGame   = false;

            //Setup the console window and the gameboard
            Console.Title = "Tactical Space Cheese Racer";
            Console.WindowHeight = SCREENHEIGHT;
            Console.WindowWidth = SCREENWIDTH;
            Console.SetBufferSize(SCREENWIDTH, SCREENHEIGHT);

            GameBoard.CreateBoard(windowWidth: SCREENWIDTH);

            //Now that everything has been setup begin the main loop
            while (!quitGame)
            {
                //Don't clear the board whilst playing the game else the game board and players will get cleared.
                if (state != GameState.PLAYGAME)
                    Console.Clear();

                switch (state)
                {
                    case GameState.GAMEMENU:
                        MainMenu(quitSetting: ref quitGame);
                        break;

                    case GameState.PLAYERSELECTION:
                        CreatePlayers();
                        SetupBoard();
                        state = GameState.PLAYGAME;
                        break;

                    case GameState.PLAYGAME:
                        GameTurn();
                        break;

                    case GameState.LOADGAME:
                        //try and load the save file, if that fails send the player to the player selection screen
                        if (LoadGame())
                        {
                            SetupBoard();
                            state = GameState.PLAYGAME;
                        }
                        else
                        {
                            Console.WriteLine("Save file loading error (Have you got any save files saved yet?)");
                            Console.ReadKey();
                            state = GameState.GAMEMENU;
                        }
                        break;

                    case GameState.GAMEOVER:
                        UserInput.Header(inHeader: "GameOver");
                        Console.WriteLine("{0} Has won the game! (Press the any key to continue)", winnerName);
                        Console.ReadKey();
                        ResetGame();
                        state = GameState.GAMEMENU;
                        break;
                }
            }
            return 0;
        }

        #region Tactical Methods

        /// <summary>
        /// This will perform a "tactical roll" which will roll a D6 and will
        /// perform the tactic assosiated with that dice roll.
        /// </summary>
        /// <param name="playerNumber">
        /// The number of the player who made the tactical roll
        /// </param>
        private static void TacticalRoll(int playerNumber)
        {
            int tacticDiceRoll;
            int explodedCell;
            int selectedPlayer;
            int tempPosition;

            //roll dice
            Console.Write("\nPress the any key to roll the tactical dice:");
            Console.ReadKey(true);

            tacticDiceRoll = RollDice();
            Console.WriteLine(" You rolled {0}\n", tacticDiceRoll);

            //use switch statement to execute a tactic
            switch (tacticDiceRoll)
            {
                case 1: // If the player rolls a 1 their rocket engines explode and they are sent back to the start

                    Console.WriteLine("Oh no!!! Your engines exploded! (Move back to the start)");
                    PlayerMoveToCell(playerToMove: playerNumber, cellNumber: 0);
                    break;

                case 2:/* If the player rolls a 2 they set off a cheese chain reaction causing the engines in all the rockets on their square
                        * to explode, sending everyone on that square back to the start*/

                    Console.WriteLine("Oh no!!! You started a cheese chain-reaction! (All players on your square move to the start)");

                    explodedCell = listOfPlayers[playerNumber].position;

                    for (int i = 0; i < listOfPlayers.Count; i++)
                    {
                        if (listOfPlayers[i].position == explodedCell)
                        {
                            PlayerMoveToCell(playerToMove: i, cellNumber: 0);
                        }
                    }
                    break;

                case 3: /*If the player rolls a 3 they set off a “gamma cheese chain” reaction causing the engines in every rocket on their square
                         * to explode and sending them back to the start, but their rocket is not affected by this roll*/

                    Console.WriteLine("Oh yeah!!! You started a gamma cheese chain-reaction! (All players on your square move to the start apart from you)");

                    explodedCell = listOfPlayers[playerNumber].position;

                    for (int i = 0; i < listOfPlayers.Count; i++)
                    {
                        if ((listOfPlayers[i].position == explodedCell) && (listOfPlayers[i].number != listOfPlayers[playerNumber].number))
                        {
                            PlayerMoveToCell(playerToMove: i, cellNumber: 0);
                        }
                    }
                    break;

                case 4: /*If the player rolls a 4 their engines are given a “chedder power boost” and their ship moves six squares forwards.
                         * If the rocket passes the last square the player wins the game.*/

                    Console.WriteLine("Oh yeah!!! Your engines are given a Cheddar power boost! (Move forwards 6 cells)");
                    PlayerMover(playerToMove: playerNumber, distance: 6);
                    break;

                case 5: //If the player rolls a 5 they use “cheese transference power” to move to the same square as any rocket on the board

                    Console.WriteLine("Oh yeah!!! You have cheese transference power! (Move to any other player)");
                    //Get the name of the player that the tactic roller wants to move to
                    Console.WriteLine("Who do you want to move to?");
                    selectedPlayer = PlayerSelector();

                    //move the player to that cell
                    PlayerMoveToCell(playerToMove: playerNumber, cellNumber: listOfPlayers[selectedPlayer].position);
                    break;

                case 6: //If the player rolls a 6 they use “gamma cheese transference power” to swap position with any rocket on the board

                    Console.WriteLine("Oh yeah!!! You have gamma cheese transference power! (Swap with any other player)");
                    //get the player the current player wants to swap with
                    Console.WriteLine("Who do you want to swap with?");
                    selectedPlayer = PlayerSelector();

                    //store the position of the current player
                    tempPosition = listOfPlayers[playerNumber].position;

                    //move the current player to that cell
                    PlayerMoveToCell(playerToMove: playerNumber, cellNumber: listOfPlayers[selectedPlayer].position);

                    //move the swapee to the current players oridgenal position
                    PlayerMoveToCell(playerToMove: selectedPlayer, cellNumber: tempPosition);
                    break;

                default:
                    break;
            }
        }

        #endregion Tactical Methods

        #region Presentation related methods

        /// <summary>
        /// This method clears the "information area" of the screen. (The
        /// information area is located under the board area.)
        /// </summary>
        private static void ClearInformationArea()
        {
            int linesToWipe = SCREENHEIGHT - GameBoard.GetEndOfBoard();

            Console.SetCursorPosition(0, GameBoard.GetEndOfBoard());

            //generate a clearing string
            StringBuilder clearingString = new StringBuilder();

            //if we wipe one line less than we need to (linesToWipe - 1) then we will not push the game
            //out of the buffer, but we won't clear the last line of the console (bad workaround but I
            //never write to the last line so it will work)
            for (int i = 0; i < linesToWipe - 1; i++)
            {
                for (int j = 0; j < SCREENWIDTH; j++)
                {
                    clearingString.Append(" ");
                }
            }

            //Note: we made a buffer clearing string that we we only need to write once (oridgenally
            //i printed each space onto the console)
            Console.Write(clearingString);

            Console.SetCursorPosition(0, GameBoard.GetEndOfBoard());
        }

        /// <summary>
        /// The main menu will print a selection menu so the user can choose
        /// where to navigate to next, it will also have a colour coded header
        /// depending on if the program is in debug mode or not
        /// </summary>
        /// <param name="quitSetting">
        /// The quit setting of the program (will be made true if the user
        /// selects quit)
        /// </param>
        private static void MainMenu(ref bool quitSetting)
        {
            //setup a string array with the different menu options, and an int to store what the user selects.
            string[] mainMenuItems = new string[4] { "Play Game", "Load Game", "Toggle Debug", "Quit" };
            int menuSelection;

            if (debugMode == false)
                UserInput.Header(inHeader: "Welcome to Tactical Cheese Racer");
            else
                UserInput.Header(inHeader: "Welcome to Tactical Cheese Racer - Debug Mode", colour: 5);

            Console.WriteLine(@" _______         _   _           _    _____                      ");
            Console.WriteLine(@"|__   __|       | | (_)         | |  / ____|                     ");
            Console.WriteLine(@"   | | __ _  ___| |_ _  ___ __ _| | | (___  _ __   __ _  ___ ___ ");
            Console.WriteLine(@"   | |/ _` |/ __| __| |/ __/ _` | |  \___ \| '_ \ / _` |/ __/ _ \");
            Console.WriteLine(@"   | | (_| | (__| |_| | (_| (_| | |  ____) | |_) | (_| | (_|  __/");
            Console.WriteLine(@"   |_|\__,_|\___|\__|_|\___\__,_|_| |_____/| .__/ \__,_|\___\___|");
            Console.WriteLine(@"                                           | |                   ");
            Console.WriteLine(@"                                           |_|                   ");
            Console.WriteLine(@"  _____ _                           _____                     ");
            Console.WriteLine(@" / ____| |                         |  __ \                    ");
            Console.WriteLine(@"| |    | |__   ___  ___  ___  ___  | |__) |__ _  ___ ___ _ __ ");
            Console.WriteLine(@"| |    | '_ \ / _ \/ _ \/ __|/ _ \ |  _  // _` |/ __/ _ \ '__|");
            Console.WriteLine(@"| |____| | | |  __/  __/\__ \  __/ | | \ \ (_| | (_|  __/ |   ");
            Console.WriteLine(@" \_____|_| |_|\___|\___||___/\___| |_|  \_\__,_|\___\___|_|  ");

            Console.WriteLine("\n\nWhat would you like to do?");

            //Get the option the user wants to go to
            menuSelection = UserInput.SelectionMenu(selectionArray: mainMenuItems);

            //Use a switch statement to act appropriately.
            switch (menuSelection)
            {
                case 0: //Play Game
                    state = GameState.PLAYERSELECTION;
                    break;

                case 1: //Load Game
                    state = GameState.LOADGAME;
                    break;

                case 2: //Toggle Debug
                    if (debugMode == false)
                        debugMode = true;
                    else
                        debugMode = false;
                    break;

                case 3: //Quit
                    quitSetting = true;
                    break;
            }
        }

        /// <summary>
        /// This method draws the game board in the correct place and then draws
        /// the players onto it
        /// </summary>
        private static void SetupBoard()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 3);
            GameBoard.DrawBoard();
            GameBoard.ClearAllCells();
            GameBoard.DrawPlayers(players: listOfPlayers);
        }

        #endregion Presentation related methods

        #region Player related methods

        /// <summary>
        /// This method is for setting up the players in the player list, it
        /// will ask the users how many players will be playing and what each
        /// players name and ship will be.
        /// </summary>
        private static void CreatePlayers()
        {
            string name;
            char ship;
            int selectedShip;
            char[] ships = new char[14] { 'A', 'B', 'C', 'D', '$', '&', '~', '%', '0', '©', '=', '¼', 'Æ', '†' };

            //Clear the list of players incase the user has already completed a game
            listOfPlayers.Clear();

            UserInput.Header("Player Selection");

            //GET THE NUMBER OF PLAYERS
            Console.Write("Please enter the amount of players (2-4): ");
            int amountOfPlayers = UserInput.ReadRange(min: 2, max: 4);
            Console.Clear();

            //GET INFORMATION FOR EACH INDEVIDUAL PLAYER AND ADD THEM TO THE PLAYER LIST
            for (int i = 0; i < amountOfPlayers; i++)
            {
                //get information
                UserInput.Header("Enter info for Player " + (i + 1));

                Console.Write("Please enter name of Player {0} (Max 20 characters): ", i + 1);
                name = UserInput.ValidString(minLength: 1, maxLength: 20);

                Console.WriteLine("Please select ship of Player {0}: ", i + 1);
                selectedShip = UserInput.SelectionMenu(ships);
                ship = ships[selectedShip];

                Console.Clear();

                //construct the player with the information collected
                var p = new Player(inPlayerNumber: i, inName: name, inShip: ship);
                //add them to the player list
                listOfPlayers.Add(p);
            }
        }

        /// <summary>
        /// This method moves a player a set distance and makes the player look
        /// like they are moving through each cell of the board to get there.
        /// </summary>
        /// <param name="playerToMove">The player to be moved</param>
        /// <param name="distance">
        /// The distance you want to move the player (negative to go backwards)
        /// </param>
        private static void PlayerMover(int playerToMove, int distance)
        {
            int animationSpeed;

            Console.CursorVisible = false;

            //if the player needs to travel more than 8 spaces each way speed up the player (to save time)
            if (distance >= -6 && distance <= 6)
                animationSpeed = 150;
            else
                animationSpeed = 75;

            //If the player needs to be moved forward we do the If, but if the player needs to be moved backwards we do the else.
            if (distance >= 0)
            {
                for (int i = 0; i < distance; i++)
                {
                    //if the user presses a key skip the movement
                    if (Console.KeyAvailable == true)
                    {
                        animationSpeed = 0;
                        Console.ReadKey(true);
                    }

                    //Clear the cell that the last player was on
                    GameBoard.ClearOneCell(position: listOfPlayers[playerToMove].position);

                    //Create a temp player, modify it, then overwrite the oridgenal player on the list
                    //If the player was an object instead of a struct we would not have to do this.
                    Player tempPlayer = listOfPlayers[playerToMove];
                    tempPlayer.position++;
                    listOfPlayers[playerToMove] = tempPlayer;

                    //If the player tries to go backwards beyond 0 keep them at 0. (this is mostly for the debug mode)
                    if (listOfPlayers[playerToMove].position > 64)
                    {
                        break;
                    }

                    //update the players on the board so that it looks like they have moved one square
                    GameBoard.DrawPlayers(players: listOfPlayers);

                    if (animationSpeed != 0)
                        Thread.Sleep(animationSpeed);
                }
            }
            else
            {
                for (int i = 0; i > distance; i--)
                {
                    //if the user presses a key skip the movement
                    if (Console.KeyAvailable == true)
                    {
                        animationSpeed = 0;
                        Console.ReadKey(true);
                    }

                    //Clear the cell that the last player was on
                    GameBoard.ClearOneCell(position: listOfPlayers[playerToMove].position);

                    //Create a temp player, modify it, then overwrite the oridgenal player on the list
                    //If the player was an object instead of a struct we would not have to do this.
                    Player tempPlayer = listOfPlayers[playerToMove];
                    tempPlayer.position--;

                    //If the player tries to go backwards beyond 0 keep them at 0. (this is mostly for the debug mode)
                    if (tempPlayer.position < 0)
                    {
                        break;
                    }

                    listOfPlayers[playerToMove] = tempPlayer;

                    //update the players on the board so that it looks like they have moved one square
                    GameBoard.DrawPlayers(players: listOfPlayers);

                    if (animationSpeed != 0)
                        Thread.Sleep(animationSpeed);
                }
            }

            Console.CursorVisible = true;
        }

        /// <summary>
        /// This method is similar to the PlayerMover method although instead of
        /// moveing the player a set distance it moves the player to a set cell.
        /// This will throw an exception if the cell number you give to it is
        /// not within the GameBoard.
        /// </summary>
        /// <param name="playerToMove">The player to move</param>
        /// <param name="cellNumber">The cell to move the player to</param>
        private static void PlayerMoveToCell(int playerToMove, int cellNumber)
        {
            int distanceToTravel;

            if (cellNumber < 0 || cellNumber > 64)
                throw new Exception("Invalid Cell location");

            //get distance to travel
            distanceToTravel = cellNumber - listOfPlayers[playerToMove].position;

            //feed distance into playerMover method
            PlayerMover(playerToMove: playerToMove, distance: distanceToTravel);
        }

        /// <summary>
        /// This method will print out a selection menu which lists each
        /// player's ship, name and colour. The user can select one of the players
        /// </summary>
        /// <returns>The player that the user selected</returns>
        private static int PlayerSelector()
        {
            //make a string array with enough elements to hold all the player names
            string [] playerNamesArray = new string[listOfPlayers.Count];
            int selectedPlayer;

            //make array of player ships, names and colours and format them correctly
            for (int i = 0; i < listOfPlayers.Count; i++)
            {
                playerNamesArray[i] = string.Format(
                    " {0} | {1,-20} | {2}",
                    listOfPlayers[i].ship,
                    listOfPlayers[i].name,
                    (ConsoleColor)listOfPlayers[i].colour
                    );
            }

            //get the selected player
            selectedPlayer = UserInput.SelectionMenu(selectionArray: playerNamesArray);

            //return selected player number
            return selectedPlayer;
        }

        #endregion Player related methods

        #region Game related methods

        /// <summary>
        /// This method is what organises each turn of the game and holds the
        /// vast majority of the game logic
        /// </summary>
        private static void GameTurn()
        {
            int diceRoll;

            //this for loop starts at the resume player so that if the game is loaded from a save it will start at the correct player
            for (int i = resumePlayer; i < listOfPlayers.Count; i++)
            {
                ConsoleKeyInfo kb;
                SaveGame(currentPlayer: i, fileName: "autosave");

                UserInput.Header(inHeader: string.Format("{0}'s Turn", listOfPlayers[i].name), colour: listOfPlayers[i].colour);

                //set the cursor to write at the bottom of the board (in the information area)
                Console.SetCursorPosition(0, GameBoard.GetEndOfBoard());

                //MOVEMENT ROLL
                Console.WriteLine("[MOVEMENT PHASE]");
                Console.Write("Press the any key to roll the dice or Escape to quit:");
                kb = Console.ReadKey(true);

                //if the user pressed escape then move the game back to the the main menu.
                if (kb.Key == ConsoleKey.Escape)
                {
                    ExitMidGame(playerNumber: i);
                    break;
                }

                diceRoll = RollDice();
                Console.WriteLine(" You rolled {0}", diceRoll);

                //move the player depending on how far they rolled
                PlayerMover(playerToMove: i, distance: diceRoll);

                //CHECK WON
                if (listOfPlayers[i].position >= 64)
                {
                    winnerName = listOfPlayers[i].name;
                    state = GameState.GAMEOVER;
                    break;
                }

                //TACTICS LOGIC
                //If player is on a cheese square force tactics, else ask if the user wants to roll anyway
                Console.WriteLine("\n[TACTICS PHASE]");
                if (GameBoard.IsCheese(listOfPlayers[i].position))
                {
                    Console.WriteLine("You landed on a cheese square!!!, you must roll tactical dice");
                    TacticalRoll(playerNumber: i);
                }
                else
                {
                    //Ask for tactical dice roll (if it has not been forced on the player)
                    Console.WriteLine("Do you want to roll the tactical dice?");

                    if (UserInput.YesNo())
                    {
                        TacticalRoll(playerNumber: i);
                    }
                }

                //CHECK WON
                if (listOfPlayers[i].position >= 64)
                {
                    winnerName = listOfPlayers[i].name;
                    state = GameState.GAMEOVER;
                    break;
                }

                Console.Write("\nEnd of turn (Pass to the next player)");
                Console.ReadKey();
                ClearInformationArea();
            }
            //set resume player to 0, so that if the player resumed from a loaded state the start player next round will be player 0
            resumePlayer = 0;
        }

        /// <summary>
        /// This method will ask the player if they want to save the game or not
        /// and then exits to the main menu
        /// </summary>
        /// <param name="playerNumber">
        /// the player number that the game was exited on
        /// </param>
        private static void ExitMidGame(int playerNumber)
        {
            string saveFileName;
            Console.Clear();

            UserInput.Header("Exiting game");

            Console.WriteLine("Do you want to save this game for later?");

            if (UserInput.YesNo())
            {
                Console.Write("\nEnter file name(max 20 characters): ");
                saveFileName = UserInput.ValidString(minLength: 1, maxLength: 20);
                SaveGame(currentPlayer: playerNumber, fileName: saveFileName);
            }

            state = GameState.GAMEMENU;
        }

        /// <summary>
        /// This method resets all the player positions to zero and then saves
        /// the game. so that when the user loads the game again they wont have
        /// to retype everyones name into the system.
        /// </summary>
        private static void ResetGame()
        {
            //Set every players position to 0 and then save the game so that you can play the game again using load
            for (int i = 0; i < listOfPlayers.Count; i++)
            {
                Player tempPlayer = listOfPlayers[i];
                tempPlayer.position = 0;
                listOfPlayers[i] = tempPlayer;
            }

            SaveGame(currentPlayer: 0, fileName: "autosave");
        }

        /// <summary>
        /// This method will roll a D6 dice, but if the program is in debug mode
        /// then it will ask for a debug roll (the user simply tpyes what they
        /// want to roll)
        /// </summary>
        /// <returns>The dice roll</returns>
        private static int RollDice()
        {
            if (debugMode == false)
                return diceRandomiser.Next(1, 7);
            else
            {
                Console.Write("Debug Roll: ");
                return UserInput.ValidInteger();
            }
        }

        #endregion Game related methods

        #region File Handleing

        /// <summary>
        /// The load game method will take the save.tcr file that is found next
        /// to the .exe and will read the values into the list of players (this
        /// is essentially the player selection method but automatic. If the
        /// file read errors then it will return false.
        /// </summary>
        /// <returns>True if file read was successful and false if it failed</returns>
        private static bool LoadGame()
        {
            StreamReader reader = null;
            string[] filePaths;
            string fileSelected;
            int amountOfPlayers = 0;
            int position = 0;
            string name;
            string tempShip;
            char ship;

            /*we need to check if there is a save directory made for tyhe player to load from.
            else we need to return false to indicate file loading error.
             */
            if (Directory.Exists(@"saves\"))
            {
                filePaths = Directory.GetFiles(@"saves\", "*.tcr");
            }
            else
            {
                return false;
            }

            listOfPlayers.Clear();

            //if there are no files available return false to indicate file loading error
            if (filePaths.Length == 0)
                return false;

            UserInput.Header(inHeader: "Pick a save file");
            //this makes the menu that prints out the available files, and it assigned the selected file to fileSelected
            fileSelected = filePaths[UserInput.SelectionMenu(selectionArray: filePaths)];

            try
            {
                //Load the file that the user selected
                reader = new StreamReader(fileSelected);

                //load the ammount of players and check validity
                amountOfPlayers = int.Parse(reader.ReadLine());

                if (amountOfPlayers < 2 || amountOfPlayers > 4)
                    throw new Exception("Save file error: invalid number of players");

                //set the resume player and check validity
                resumePlayer = int.Parse(reader.ReadLine());

                if (resumePlayer < 0 || resumePlayer > 4)
                    throw new Exception("Save file error: invalid resume player");

                //construct the player list
                for (int i = 0; i < amountOfPlayers; i++)
                {
                    name = reader.ReadLine();

                    //error if the user has edited the save file 
                    if(name.Length > 20)
                        throw new Exception ("Save file error: Name in save file is too long");

                    tempShip = reader.ReadLine();
                    ship = tempShip[0];

                    position = int.Parse(reader.ReadLine());

                    //construct the player from the information gathered and then add them to the player list
                    var p = new Player(inPlayerNumber: i, inName: name, inShip: ship, inPosition: position);
                    listOfPlayers.Add(p);
                }
            }
            catch
            {
                //return false to indicate loading failed
                return false;
            }
            finally
            {
                //this makes sure the reader closes no matter what happens
                if (reader != null)
                    reader.Close();
            }

            //return true to indicate loading successful
            return true;
        }

        /// <summary>
        /// The save game method will take important information from the game
        /// (the player list and the current player turn) and will convert it
        /// into a text file that the loadgame method can parse
        /// </summary>
        /// <param name="currentPlayer">
        /// The player turn that the game was saved on
        /// </param>
        /// <param name="fileName">The name of the save file</param>
        private static void SaveGame(int currentPlayer, string fileName)
        {
            //first we need to check is the save game directory has been setup.
            if (!Directory.Exists(@"saves\"))
            {
                Directory.CreateDirectory(@"saves\");
            }

            StreamWriter writer = null;
            //We make a string that will buffer what the output file will have, this means we have less writing to the file
            string outputBuffer = "";

            outputBuffer += listOfPlayers.Count + "\n";
            outputBuffer += currentPlayer + "\n";

            //Convert list of players into a string so we only need to write to a file once (not 16 times)
            for (int i = 0; i < listOfPlayers.Count; i++)
            {
                outputBuffer += listOfPlayers[i].name + "\n";
                outputBuffer += listOfPlayers[i].ship + "\n";
                outputBuffer += listOfPlayers[i].position + "\n";
            }

            try
            {
                writer = new StreamWriter(string.Format(@"saves\{0}.tcr", fileName));
                writer.Write(outputBuffer);
                writer.Close();
            }
            catch //this will error if the file name has escape characters in it.
            {
                if (writer != null)
                    writer.Close();

                Console.WriteLine("Error writing file: All data is lost in time forever");
                Console.ReadKey();
            }
        }

        #endregion File Handleing
    }
}
