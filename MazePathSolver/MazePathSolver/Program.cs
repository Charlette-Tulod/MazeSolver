﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MazePathSolver
{
    delegate void MazeChangedHandler(int iChanged, int jChanged);
    class Program
    {

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }

        /// <summary>
        /// Class attributes/members
        /// </summary>
        int[,] m_iMaze;
        int m_iRows;
        int m_iCols;
        int iPath = 100;
        bool gbfs = false;
        public event MazeChangedHandler OnMazeChangedEvent;

        /// <summary>
        /// Constructor 1: takes a 2D integer array
        /// </summary>
        public Program(int[,] iMaze)
        {
            m_iMaze = iMaze;
            m_iRows = iMaze.GetLength(0);
            m_iCols = iMaze.GetLength(1);
        }

        /// <summary>
        /// Constructor 2: initializes the dimensions of maze, 
        /// later, indexers may be used to set individual elements' values
        /// </summary>
        public Program(int iRows, int iCols)
        {
            m_iMaze = new int[iRows, iCols];
            m_iRows = iRows;
            m_iCols = iCols;
        }

        /// <summary>
        /// Properites:
        /// </summary>
        public int Rows
        {
            get { return m_iRows; }
        }

        public int Cols
        {
            get { return m_iCols; }
        }
        public int[,] GetMaze
        {
            get { return m_iMaze; }
        }
        public int PathCharacter
        {
            get { return iPath; }
            set
            {
                if (value == 0)
                    throw new Exception("Invalid path character specified");
                else
                    iPath = value;
            }
        }
        public bool GBFS
        {
            get { return gbfs; }
            set { gbfs = value; }
        }


        /// <summary>
        /// Indexer
        /// </summary>
        public int this[int iRow, int iCol]
        {
            get { return m_iMaze[iRow, iCol]; }
            set
            {
                m_iMaze[iRow, iCol] = value;
                if (this.OnMazeChangedEvent != null)    // trigger event
                    this.OnMazeChangedEvent(iRow, iCol);
            }
        }

        /// <summary>
        /// The function is used to get the contents of a given node in a given maze,
        ///  specified by its node no.
        /// </summary>
        private int GetNodeContents(int[,] iMaze, int iNodeNo)
        {
            int iCols = iMaze.GetLength(1);
            return iMaze[iNodeNo / iCols, iNodeNo - iNodeNo / iCols * iCols];
        }

        /// <summary>
        /// The function is used to change the contents of a given node in a given maze,
        ///  specified by its node no.
        /// </summary>
        private void ChangeNodeContents(int[,] iMaze, int iNodeNo, int iNewValue)
        {
            int iCols = iMaze.GetLength(1);
            iMaze[iNodeNo / iCols, iNodeNo - iNodeNo / iCols * iCols] = iNewValue;
        }

        /// <summary>
        /// This public function finds the shortest path between two points
        /// in the maze and return the solution as an array with the path traced 
        /// by "iPath" (can be changed using property "PathCharacter")
        /// if no path exists, the function returns null
        /// </summary>
        public int[,] FindPath(int iFromY, int iFromX, int iToY, int iToX)
        {
            int iBeginningNode = iFromY * this.Cols + iFromX;
            int iEndingNode = iToY * this.Cols + iToX;
            return (Search(iBeginningNode, iEndingNode));
        }

        


        /// <summary>
        /// Internal function for that finds the shortest path using a technique
        /// similar to breadth-first search.
        /// It assigns a node no. to each node(2D array element) and applies the algorithm
        /// </summary>
        private enum Status
        { Ready, Waiting, Processed }
        private int[,] Search(int iStart, int iStop)
        {
            const int empty = 0;

            int iRows = m_iRows;
            int iCols = m_iCols;
            int iMax = iRows * iCols;
            int[] Queue = new int[iMax];
            int[] Origin = new int[iMax];
            int iFront = 0, iRear = 0;

            //check if starting and ending points are valid (open)
            if (GetNodeContents(m_iMaze, iStart) != empty || GetNodeContents(m_iMaze, iStop) != empty)
            {
                return null;
            }

            //create dummy array for storing status
            int[,] iMazeStatus = new int[iRows, iCols];
            //initially all nodes are ready
            for (int i = 0; i < iRows; i++)
                for (int j = 0; j < iCols; j++)
                    iMazeStatus[i, j] = (int)Status.Ready;

            Queue[iRear] = iStart;
            Origin[iRear] = -1;
            iRear++;
            int iCurrent, iLeft, iRight, iTop, iDown;

            if (gbfs == true)    //GREEDY BEST FIRST SEARCH
            {

                int X = iStop / 10, Y = iStop % 10;
                int[,] heuristic = new int[iRows, iCols];
                for (int x = 0; x < 13; x++)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        heuristic[x, y] = Math.Abs(X - x) + Math.Abs(Y - y);
                    }
                }
              
                while (iFront != iRear)                 // while Q is not empty	
                {
                    if (Queue[iFront] == iStop)         // maze is solved
                        break;

                    iCurrent = Queue[iFront];

                    int hTop = 999, hDown = 999, hLeft = 999, hRight = 999; // default heuristic value

                    // checking the heuristic values
                    iTop = iCurrent - iCols;
                    if (iTop >= 0)
                        hTop = GetNodeContents(heuristic, iTop);

                    iDown = iCurrent + iCols;
                    if (iDown < iMax)
                        hDown = GetNodeContents(heuristic, iDown);

                    iLeft = iCurrent - 1;
                    if (iLeft >= 0 && iLeft / iCols == iCurrent / iCols)
                        hLeft = GetNodeContents(heuristic, iLeft);

                    iRight = iCurrent + 1;
                    if (iRight < iMax && iRight / iCols == iCurrent / iCols)
                        hRight = GetNodeContents(heuristic, iRight);


                    int[] sequence = { hTop, hDown, hLeft, hRight };
                    //sort heuristic values in assending order
                    Array.Sort(sequence);          

                    for (int i = 0; i < sequence.Length; i++) 
                    {
                        if (iTop >= 0)                  //if top node exists
                            if (GetNodeContents(m_iMaze, iTop) == empty)    //if top node is open(a path exists)
                                if (GetNodeContents(iMazeStatus, iTop) == (int)Status.Ready)    //if top node is ready
                                {
                                    if (sequence[i] == hTop)
                                    {
                                        Queue[iRear] = iTop;        //add to Q
                                        Origin[iRear] = iCurrent;
                                        ChangeNodeContents(iMazeStatus, iTop, (int)Status.Waiting); //change status to waiting
                                        iRear++;
                                    }
                                }

                        if (iDown < iMax)               //if bottom node exists
                            if (GetNodeContents(m_iMaze, iDown) == empty)   //if bottom node is open(a path exists)
                                if (GetNodeContents(iMazeStatus, iDown) == (int)Status.Ready)   //if bottom node is ready
                                {
                                    if (sequence[i] == hDown)
                                    {
                                        Queue[iRear] = iDown;   //add to Q
                                        Origin[iRear] = iCurrent;
                                        ChangeNodeContents(iMazeStatus, iDown, (int)Status.Waiting); //change status to waiting
                                        iRear++;
                                    }
                                }

                        if (iLeft >= 0 && iLeft / iCols == iCurrent / iCols)    //if left node exists
                            if (GetNodeContents(m_iMaze, iLeft) == empty)       //if left node is open(a path exists)
                                if (GetNodeContents(iMazeStatus, iLeft) == (int)Status.Ready)   //if left node is ready
                                {
                                    if (sequence[i] == hLeft)
                                    {
                                        Queue[iRear] = iLeft;       //add to Q
                                        Origin[iRear] = iCurrent;
                                        ChangeNodeContents(iMazeStatus, iLeft, (int)Status.Waiting); //change status to waiting
                                        iRear++;
                                    }
                                }

                        if (iRight < iMax && iRight / iCols == iCurrent / iCols)    //if right node exists
                            if (GetNodeContents(m_iMaze, iRight) == empty)  //if right node is open(a path exists)
                                if (GetNodeContents(iMazeStatus, iRight) == (int)Status.Ready)  //if right node is ready
                                {
                                    if (sequence[i] == hRight)
                                    {
                                        Queue[iRear] = iRight;      //add to Q
                                        Origin[iRear] = iCurrent;
                                        ChangeNodeContents(iMazeStatus, iRight, (int)Status.Waiting); //change status to waiting
                                        iRear++;
                                    }
                                }
                    }

                    //change status of current node to processed
                    ChangeNodeContents(iMazeStatus, iCurrent, (int)Status.Processed);
                    iFront++;

                }
                //create an array(maze) for solution
                int[,] iMazeSolved = new int[iRows, iCols];
                for (int i = 0; i < iRows; i++)
                    for (int j = 0; j < iCols; j++)
                        iMazeSolved[i, j] = m_iMaze[i, j];

                //make a path in the Solved Maze
                iCurrent = iStop;
                ChangeNodeContents(iMazeSolved, iCurrent, iPath);
                for (int i = iFront; i >= 0; i--)
                {
                    if (Queue[i] == iCurrent)
                    {
                        iCurrent = Origin[i];
                        if (iCurrent == -1)     // maze is solved
                            return (iMazeSolved);
                        ChangeNodeContents(iMazeSolved, iCurrent, iPath);
                    }
                }
            }
            else // BREADTH FIRST SEARCH
            {
                while (iFront != iRear) // while Q is not empty	
                {
                    if (Queue[iFront] == iStop)     // maze is solved
                        break;

                    iCurrent = Queue[iFront];

                    iTop = iCurrent - iCols;
                    if (iTop >= 0)  //if top node exists
                        if (GetNodeContents(m_iMaze, iTop) == empty)    //if top node is open(a path exists)
                            if (GetNodeContents(iMazeStatus, iTop) == (int)Status.Ready)    //if top node is ready
                            {
                                Queue[iRear] = iTop; //add to Q
                                Origin[iRear] = iCurrent;
                                ChangeNodeContents(iMazeStatus, iTop, (int)Status.Waiting); //change status to waiting
                                iRear++;
                            }

                    iDown = iCurrent + iCols;
                    if (iDown < iMax)   //if bottom node exists
                        if (GetNodeContents(m_iMaze, iDown) == empty)   //if bottom node is open(a path exists)
                            if (GetNodeContents(iMazeStatus, iDown) == (int)Status.Ready)   //if bottom node is ready
                            {
                                Queue[iRear] = iDown; //add to Q
                                Origin[iRear] = iCurrent;
                                ChangeNodeContents(iMazeStatus, iDown, (int)Status.Waiting); //change status to waiting
                                iRear++;
                            }


                    iLeft = iCurrent - 1;
                    if (iLeft >= 0 && iLeft / iCols == iCurrent / iCols)    //if left node exists
                        if (GetNodeContents(m_iMaze, iLeft) == empty)   //if left node is open(a path exists)
                            if (GetNodeContents(iMazeStatus, iLeft) == (int)Status.Ready)   //if left node is ready
                            {
                                Queue[iRear] = iLeft; //add to Q
                                Origin[iRear] = iCurrent;
                                ChangeNodeContents(iMazeStatus, iLeft, (int)Status.Waiting); //change status to waiting
                                iRear++;
                            }

                    iRight = iCurrent + 1;
                    if (iRight < iMax && iRight / iCols == iCurrent / iCols)    //if right node exists
                        if (GetNodeContents(m_iMaze, iRight) == empty)  //if right node is open(a path exists)
                            if (GetNodeContents(iMazeStatus, iRight) == (int)Status.Ready)  //if right node is ready
                            {
                                Queue[iRear] = iRight; //add to Q
                                Origin[iRear] = iCurrent;
                                ChangeNodeContents(iMazeStatus, iRight, (int)Status.Waiting); //change status to waiting
                                iRear++;
                            }

                    //change status of current node to processed
                    ChangeNodeContents(iMazeStatus, iCurrent, (int)Status.Processed);
                    iFront++;

                }
                //create an array(maze) for solution
                int[,] iMazeSolved = new int[iRows, iCols];
                for (int i = 0; i < iRows; i++)
                    for (int j = 0; j < iCols; j++)
                        iMazeSolved[i, j] = m_iMaze[i, j];

                //make a path in the Solved Maze
                iCurrent = iStop;
                ChangeNodeContents(iMazeSolved, iCurrent, iPath);
                for (int i = iFront; i >= 0; i--)
                {
                    if (Queue[i] == iCurrent)
                    {
                        iCurrent = Origin[i];
                        if (iCurrent == -1)     // maze is solved
                            return (iMazeSolved);
                        ChangeNodeContents(iMazeSolved, iCurrent, iPath);
                    }
                }
            }

            //no path exists
            return null;

        }
    }
}
