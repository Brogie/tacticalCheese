using Main_Game;
using System;
using System.Collections.Generic;

namespace Gameboard_Drawing
{
    /// <summary>
    /// This method library is for drawing the board and players. It is only
    /// able to draw the players, it cannot move them around the board.
    /// </summary>
    internal class GameBoard
    {
        #region Global Variables

        /// <summary>
        /// This will be the board after it has been constructed (so to draw the
        /// board we only have to print this string)
        /// </summary>
        static private string board;

        /// <summary>
        /// This will store where the end of the board starts in the Y axis
        /// </summary>
        static private int boardEndTop;

        /// <summary>
        /// This is the whitespace between the left side of the console and the
        /// left side of the game board
        /// </summary>
        static private int boardMargin;

        /// <summary>
        /// This will store where the top of the board starts in the Y axis
        /// </summary>
        static private int boardOriginTop;
        #endregion

        /// <summary>
        /// Clears one cell from the board.
        /// </summary>
        /// <param name="position">The cell you want to clear</param>
        static public void ClearOneCell(int position)
        {
            int cursorTop = Console.CursorTop;
            int cursorLeft = Console.CursorLeft;

            if (position > 0 && position <= 64)
            {
                LocateCell(toFind: position, isPlayer: true);
                Console.Write("        ");
            }

            Console.SetCursorPosition(cursorLeft, cursorTop);
        }

        /// <summary>
        /// This clears all the cells on the board.
        /// </summary>
        static public void ClearAllCells()
        {
            int cursorTop = Console.CursorTop;
            int cursorLeft = Console.CursorLeft;

            for (int i = 1; i <= 64; i++)
            {
                LocateCell(toFind: i, isPlayer: true);
                Console.Write("        ");
            }

            Console.SetCursorPosition(cursorLeft, cursorTop);
        }

        /// <summary>
        /// This will create the game board as a string.
        /// </summary>
        static public void CreateBoard(int windowWidth)
        {
            string whitespaceLeft = "";
            string whitespaceRight = "";

            boardMargin = (windowWidth - 73) / 2;

            for (int i = 0; i < boardMargin; i++)
                whitespaceLeft += " ";

            for (int i = 0; i < boardMargin; i++)
                whitespaceRight += " ";

            whitespaceRight += " ";

            /*This bit is aweful but I can't think of a better way to do this, it simply constructs the board and puts it
             * into a string. This means when we draw the board we only need to do one draw call instead of doing a draw
             * call for each line.*/
            board = String.Format("{0}╔════════╦════════╦════════╦════════╦════════╦════════╦════════╦════════╗{1}", whitespaceLeft, whitespaceRight);

            for (int i = 0; i < 8; i++)
            {
                board += String.Join("",
                String.Format("{0}║        ║        ║        ║        ║        ║        ║        ║        ║{1}", whitespaceLeft, whitespaceRight),
                String.Format("{0}║────────║────────║────────║────────║────────║────────║────────║────────║{1}", whitespaceLeft, whitespaceRight),
                String.Format("{0}║        ║        ║        ║        ║        ║        ║        ║        ║{1}", whitespaceLeft, whitespaceRight));

                if (i == 7)
                {
                    board += String.Format("{0}╚════════╩════════╩════════╩════════╩════════╩════════╩════════╩════════╝{1}", whitespaceLeft, whitespaceRight);
                }
                else
                {
                    board += String.Format("{0}╟════════╫════════╫════════╫════════╫════════╫════════╫════════╫════════╢{1}", whitespaceLeft, whitespaceRight);
                }
            }
        }

        /// <summary>
        /// Prints out the board, this is not complete yet.
        /// </summary>
        static public void DrawBoard()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkBlue;

            boardOriginTop = Console.CursorTop;

            Console.Write(board);

            boardEndTop = Console.CursorTop;

            //What is this for? it has no purpose... but it might, i have no recolection of
            //writing this, i'll leave it in comments for now... Stop programming at 3am.
            //for (int i = 1; i <= 64; i++)
            //{
            //    LocateCell(toFind: i, isPlayer: true);
            //    Console.Write("        ");
            //}

            DrawNumbers();

            //Draw the line at the bottom of the board
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkBlue;

            Console.SetCursorPosition(0, boardEndTop);

            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Console.Write("─");
            }
            Console.ResetColor();
        }

        /// <summary>
        /// Clears the player sections on the board and then prints out the
        /// players in the correct location
        /// </summary>
        /// <param name="players">list of all the players in the game</param>
        static public void DrawPlayers(List<TacticalCheese.Player> players)
        {
            //we need to record the cursor location so we can put it back at the end of the method
            int cursorTop = Console.CursorTop;
            int cursorLeft = Console.CursorLeft;
            int ammountOnZero = 0;

            ////first we clear all the cell locations to wipe any players off the board
            //for (int i = 1; i <= 64; i++)
            //{
            //    LocateCell(toFind: i, isPlayer: true);
            //    Console.Write("        ");
            //}

            //Now we draw each player character in their own colour
            foreach (TacticalCheese.Player p in players)
            {
                //We locate the cell that the player is in, then we offest it  by 2+playerNumber so that
                //multiple players can be in the same space without being printed over eachother
                if (p.position > 0 && p.position <= 64)
                {
                    LocateCell(toFind: p.position, isPlayer: true);
                    Console.CursorLeft = Console.CursorLeft + (2 + p.number);

                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = (ConsoleColor)(p.colour);
                    Console.Write(p.ship);
                    Console.ResetColor();
                }
            }

            //THE FOLLOWING CODE IS FOR DRAWING THE OFF THE BOARD SECTION
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkBlue;

            //set the position to the bottom corner of the board
            Console.SetCursorPosition(boardMargin, boardEndTop);

            Console.Write("[Off the Board: ");

            //if a player is on position 0 (off the board) then draw them
            foreach (TacticalCheese.Player p in players)
            {
                if (p.position == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = (ConsoleColor)(p.colour);
                    Console.Write(p.ship);
                    Console.ResetColor();

                    ammountOnZero++;
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkBlue;

            //draw some dashes after the players (this means that the game will clear any players who moved out of the 0 position
            Console.Write("]────────");

            Console.ResetColor();

            //reset cursor location
            Console.SetCursorPosition(cursorLeft, cursorTop);
        }

        /// <summary>
        /// Get where the end of the board is
        /// </summary>
        /// <returns>
        /// integer representing where the board ends (from the top of the console)
        /// </returns>
        static public int GetEndOfBoard()
        {
            return boardEndTop + 1;
        }

        /// <summary>
        /// Checks if a cell is a cheese square, and returns true if it is.
        /// </summary>
        /// <param name="cellNumber">The cell you want to check</param>
        /// <returns>True is cell is cheese and false if cell is not cheese</returns>
        static public bool IsCheese(int cellNumber)
        {
            switch (cellNumber)
            {
                case 8:
                case 15:
                case 19:
                case 28:
                case 33:
                case 45:
                case 55:
                case 59:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// draws the numbers onto the board (will probably merge this with
        /// drawboard as it no longer need parameters passing to it
        /// </summary>
        static private void DrawNumbers()
        {
            //Loop for each cell on the board (start from 1 as the cells start from one)
            for (int i = 1; i <= 64; i++)
            {
                //Locate the start of the cell
                LocateCell(toFind: i, isPlayer: false);

                //input the cell number using fancy printing for extra flair
                if (IsCheese(cellNumber: i))
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.Write("   {0:00}   ", i);
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write("   {0:00}   ", i);
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Locates a cell on the game board, will default to the player section
        /// of the cell but can select the cell number section by overloading
        /// the method with a bool set to false.
        /// </summary>
        /// <param name="toFind">The cell that you want to find</param>
        /// <param name="isPlayer">
        /// if you want to select the player area (defaults to true)
        /// </param>
        static private void LocateCell(int toFind, bool isPlayer)
        {
            int row, column;

            //LOCATE X AND Y CO-ORDINATE OF THE CELL
            //If we are looking for a cell that does not exist on the board throw an exception
            if (toFind < 1 || toFind > 64)
                throw new Exception("Invalid Cell location");

            //first locate the row that the number is in
            row = ((toFind - 1) / 8);

            //now locate the column that the number is in
            column = (toFind % 8);

            if (column == 0)
                column = 8;

            // - 1 so that the column is indexed from 0
            column--;

            //MAP THE X AND Y TO THE BOARD
            //now map the X and Y locations to the game board
            if (isPlayer)
                row = (row * 4) + (boardOriginTop + 3);
            else
                row = (row * 4) + (boardOriginTop + 1);

            column = ((column * 9) + 1) + boardMargin;

            //place the cursor in the cell
            Console.SetCursorPosition(column, row);
        }
    }
}