using System;
using MarbleGame.Model;
using System.Collections.Generic;

namespace MarbleGame
{
    public class MarbleGame
    {
        static int numberOfSteps, maxNumSteps;

        /// <summary>
        /// Entry point for finding a solution
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        public static IEnumerable<string> FindSolution(System.IO.TextReader reader = null, System.IO.TextWriter writer = null)
        {
            // Setting I/O streams
            Console.SetIn(reader ?? Console.In);
            Console.SetOut(writer ?? Console.Out);

            int sizeBoard, numberOfMarbles, numberOfWalls, caseNumber = 1;
            string input = Console.ReadLine();

            sizeBoard = int.Parse(input.Split(' ')[0]);
            numberOfMarbles = int.Parse(input.Split(' ')[1]);
            numberOfWalls = int.Parse(input.Split(' ')[2]);

            // Looping until [0 0 0] is entered
            while (sizeBoard != 0 || numberOfMarbles != 0 || numberOfWalls != 0)
            {
                MarbleHoleLocation[] marbles = new MarbleHoleLocation[numberOfMarbles];
                for (int i = 0; i < numberOfMarbles; i++)
                {
                    input = Console.ReadLine();
                    marbles[i].Number = i;
                    marbles[i].Row = int.Parse(input.Split(' ')[0]);
                    marbles[i].Column = int.Parse(input.Split(' ')[1]);
                }

                MarbleHoleLocation[] holes = new MarbleHoleLocation[numberOfMarbles];
                for (int i = 0; i < numberOfMarbles; i++)
                {
                    input = Console.ReadLine();
                    holes[i].Number = i;
                    holes[i].Row = int.Parse(input.Split(' ')[0]);
                    holes[i].Column = int.Parse(input.Split(' ')[1]);
                }

                WallLocation[] walls = new WallLocation[numberOfWalls];
                for (int i = 0; i < numberOfWalls; i++)
                {
                    input = Console.ReadLine();
                    walls[i].FirstSide.Row = int.Parse(input.Split(' ')[0]);
                    walls[i].FirstSide.Column = int.Parse(input.Split(' ')[1]);
                    walls[i].SecondSide.Row = int.Parse(input.Split(' ')[2]);
                    walls[i].SecondSide.Column = int.Parse(input.Split(' ')[3]);
                }

                bool[,] reachable = new bool[sizeBoard * sizeBoard, sizeBoard * sizeBoard];

                // Check for all nodes connectivity
                FindReachability(reachable, sizeBoard, walls, numberOfWalls);

                // The intention is to remove all abnormal wall, hole inputs
                bool hasBasicCases = FindBasicCases(reachable, sizeBoard, marbles, holes, numberOfMarbles, walls, numberOfWalls);

                if (hasBasicCases)
                {
                    InitSol(out Solution north);
                    InitSol(out Solution south);
                    InitSol(out Solution east);
                    InitSol(out Solution west);

                    maxNumSteps = sizeBoard * sizeBoard;
                    numberOfSteps = 0;

                    LiftNorth(marbles, holes, ref numberOfMarbles, walls, numberOfWalls, sizeBoard, ref north);
                    numberOfSteps = 0;
                    LiftEast(marbles, holes, ref numberOfMarbles, walls, numberOfWalls, sizeBoard, ref east);
                    numberOfSteps = 0;
                    LiftSouth(marbles, holes, ref numberOfMarbles, walls, numberOfWalls, sizeBoard, ref south);
                    numberOfSteps = 0;
                    LiftWest(marbles, holes, ref numberOfMarbles, walls, numberOfWalls, sizeBoard, ref west);

                    if (north.HasSol && (!east.HasSol || north.Cost <= east.Cost) && (!south.HasSol || north.Cost <= south.Cost) && (!west.HasSol || north.Cost <= west.Cost))
                    {
                        Console.WriteLine($"Case {caseNumber}: {north.Cost} moves {north.Path}");
                        yield return caseNumber + "_" + north.Cost + "_" + north.Path;
                    }
                    else if (east.HasSol && (!north.HasSol || east.Cost <= north.Cost) && (!south.HasSol || east.Cost <= south.Cost) && (!west.HasSol || east.Cost <= west.Cost))
                    {
                        Console.WriteLine($"Case {caseNumber}: {east.Cost} moves {east.Path}");
                        yield return caseNumber + "_" + east.Cost + "_" + east.Path;
                    }
                    else if (south.HasSol && (!north.HasSol || south.Cost <= north.Cost) && (!east.HasSol || south.Cost <= east.Cost) && (!west.HasSol || south.Cost <= west.Cost))
                    {
                        Console.WriteLine($"Case {caseNumber}: {south.Cost} moves {south.Path}");
                        yield return caseNumber + "_" + south.Cost + "_" + south.Path;
                    }
                    else if (west.HasSol && (!north.HasSol || west.Cost <= north.Cost) && (!south.HasSol || west.Cost <= south.Cost) && (!east.HasSol || west.Cost <= east.Cost))
                    {
                        Console.WriteLine($"Case {caseNumber}: {west.Cost} moves {west.Path}");
                        yield return caseNumber + "_" + west.Cost + "_" + west.Path;
                    }
                    else
                    {
                        Console.WriteLine($"Case {caseNumber}: impossible");
                        yield return caseNumber + "_impossible";
                    }
                }
                else
                {
                    Console.WriteLine($"Case {caseNumber}: impossible");
                    yield return caseNumber + "_impossible";
                }

                input = Console.ReadLine();
                sizeBoard = int.Parse(input.Split(' ')[0]);
                numberOfMarbles = int.Parse(input.Split(' ')[1]);
                numberOfWalls = int.Parse(input.Split(' ')[2]);
                caseNumber++;
            }
        }

        #region Poles
        #region Lifting
        /// <summary>
        /// Check if can move North, if moved calculate cost
        /// </summary>
        /// <param name="marbles"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="sol"></param>
        static void LiftNorth(MarbleHoleLocation[] marbles, MarbleHoleLocation[] holes, ref int numberOfMarbles, WallLocation[] walls, int numberOfWalls, int sizeBoard, ref Solution sol)
        {
            bool safeDiscard = false, placed;
            int newNumMarbles, newId = 0, countOfPlacedBalls = 0, curStep = numberOfWalls, rowId = sizeBoard - 1, copyNOM = numberOfMarbles;
            MarbleHoleLocation[] newMarbles, newHoles, copyHoles;

            if (numberOfSteps >= maxNumSteps || numberOfMarbles == 0)
            {
                numberOfSteps--;
                return;
            }

            numberOfSteps++;

            if (!CanMoveSouth(marbles, holes, numberOfMarbles, walls, numberOfWalls, sizeBoard, ref countOfPlacedBalls))
            {
                numberOfSteps--;
                return;
            }

            if (countOfPlacedBalls > 0) newNumMarbles = numberOfMarbles - countOfPlacedBalls;
            else newNumMarbles = numberOfMarbles;

            sol.Path += "N";
            sol.Cost++;

            if (newNumMarbles == 0)
            {
                if (sol.Cost < maxNumSteps) maxNumSteps = numberOfSteps;

                sol.HasSol = true;
                numberOfSteps--;
                return;
            }

            newMarbles = new MarbleHoleLocation[newNumMarbles];
            newHoles = new MarbleHoleLocation[newNumMarbles];
            copyHoles = new MarbleHoleLocation[numberOfMarbles];

            InitSol(out Solution north);
            InitSol(out Solution south);
            InitSol(out Solution east);
            InitSol(out Solution west);

            holes.CopyTo(copyHoles, 0);

            do
            {
                for (int i = 0; i < numberOfMarbles; i++)
                    if (marbles[i].Row == rowId)
                    {
                        placed = false;
                        GetMoveSouthLocation(ref marbles[i], newMarbles, ref newId, copyHoles, ref copyNOM, walls, numberOfWalls, sizeBoard, out MarbleHoleLocation temp, ref safeDiscard, ref placed);

                        if (!placed)
                        {
                            newMarbles[newId] = temp;
                            newHoles[newId++] = holes[i];
                        }
                        else
                        {
                            for (int t = rowId; t < (copyNOM - 1); t++)
                                copyHoles[t] = copyHoles[t + 1];

                            copyNOM--;
                        }
                    }

            } while (rowId-- > 0);

            LiftEast(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref east);
            numberOfSteps = curStep;

            LiftWest(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref west);

            if (newNumMarbles != numberOfMarbles)
            {
                numberOfSteps = curStep;
                LiftNorth(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref north);
                numberOfSteps = curStep;
                LiftSouth(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref south);
            }

            if (north.HasSol && (!east.HasSol || north.Cost <= east.Cost) && (!south.HasSol || north.Cost <= south.Cost) && (!west.HasSol || north.Cost <= west.Cost))
            {
                sol.Cost += north.Cost;
                sol.HasSol = true;
                sol.Path += north.Path;
            }
            else if (east.HasSol && (!north.HasSol || east.Cost <= north.Cost) && (!south.HasSol || east.Cost <= south.Cost) && (!west.HasSol || east.Cost <= west.Cost))
            {
                sol.Cost += east.Cost;
                sol.HasSol = true;
                sol.Path += east.Path;
            }
            else if (south.HasSol && (!north.HasSol || south.Cost <= north.Cost) && (!east.HasSol || south.Cost <= east.Cost) && (!west.HasSol || south.Cost <= west.Cost))
            {
                sol.Cost += south.Cost;
                sol.HasSol = true;
                sol.Path += south.Path;
            }
            else if (west.HasSol && (!north.HasSol || west.Cost <= north.Cost) && (!south.HasSol || west.Cost <= south.Cost) && (!east.HasSol || west.Cost <= east.Cost))
            {
                sol.Cost += west.Cost;
                sol.HasSol = true;
                sol.Path += west.Path;
            }

            numberOfSteps = curStep - 1;
        }

        /// <summary>
        /// Check if can move East, if moved calculate cost
        /// </summary>
        /// <param name="marbles"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="sol"></param>
        static void LiftEast(MarbleHoleLocation[] marbles, MarbleHoleLocation[] holes, ref int numberOfMarbles, WallLocation[] walls, int numberOfWalls, int sizeBoard, ref Solution sol)
        {
            bool safeDiscard = false, placed;
            int newNumMarbles, newId = 0, countOfPlacedBalls = 0, curStep = numberOfSteps, columnId = 0, copyNOM = numberOfMarbles;
            MarbleHoleLocation[] newMarbles, newHoles, copyHoles;

            if (numberOfSteps >= maxNumSteps || numberOfMarbles == 0)
            {
                numberOfSteps--;
                return;
            }

            numberOfSteps++;

            if (!CanMoveWest(marbles, holes, numberOfMarbles, walls, numberOfWalls, sizeBoard, ref countOfPlacedBalls))
            {
                numberOfSteps--;
                return;
            }

            if (countOfPlacedBalls > 0) newNumMarbles = numberOfMarbles - countOfPlacedBalls;
            else newNumMarbles = numberOfMarbles;

            sol.Path += "E";
            sol.Cost++;

            if (newNumMarbles == 0)
            {
                if (sol.Cost < maxNumSteps) maxNumSteps = numberOfSteps;

                sol.HasSol = true;
                numberOfSteps--;
                return;
            }

            newMarbles = new MarbleHoleLocation[newNumMarbles];
            newHoles = new MarbleHoleLocation[newNumMarbles];
            copyHoles = new MarbleHoleLocation[numberOfMarbles];

            InitSol(out Solution north);
            InitSol(out Solution south);
            InitSol(out Solution east);
            InitSol(out Solution west);

            holes.CopyTo(copyHoles, 0);

            while (columnId < sizeBoard)
            {
                for (int i = 0; i < numberOfMarbles; i++)
                {
                    if (marbles[i].Column == columnId)
                    {
                        placed = false;
                        GetMoveWestLocation(ref marbles[i], newMarbles, ref newId, copyHoles, ref copyNOM, walls, numberOfWalls, sizeBoard, out MarbleHoleLocation temp, ref safeDiscard, ref placed);

                        if (!placed)
                        {
                            newMarbles[newId] = temp;
                            newHoles[newId++] = holes[i];
                        }
                        else
                        {
                            for (int t = i; t < (copyNOM - 1); t++)
                                copyHoles[t] = copyHoles[t + 1];
                            copyNOM--;
                        }
                    }
                }
                columnId++;
            }

            LiftNorth(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref north);
            numberOfSteps = curStep;
            LiftSouth(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref south);

            if (newNumMarbles != numberOfMarbles)
            {
                numberOfSteps = curStep;
                LiftEast(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref east);
                numberOfSteps = curStep;
                LiftWest(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref west);
            }

            if (north.HasSol && (!east.HasSol || north.Cost <= east.Cost) && (!south.HasSol || north.Cost <= south.Cost) && (!west.HasSol || north.Cost <= west.Cost))
            {
                sol.Cost += north.Cost;
                sol.HasSol = true;
                sol.Path += north.Path;
            }
            else if (east.HasSol && (!north.HasSol || east.Cost <= north.Cost) && (!south.HasSol || east.Cost <= south.Cost) && (!west.HasSol || east.Cost <= west.Cost))
            {
                sol.Cost += east.Cost;
                sol.HasSol = true;
                sol.Path += east.Path;
            }
            else if (south.HasSol && (!north.HasSol || south.Cost <= north.Cost) && (!east.HasSol || south.Cost <= east.Cost) && (!west.HasSol || south.Cost <= west.Cost))
            {
                sol.Cost += south.Cost;
                sol.HasSol = true;
                sol.Path += south.Path;
            }
            else if (west.HasSol && (!north.HasSol || west.Cost <= north.Cost) && (!south.HasSol || west.Cost <= south.Cost) && (!east.HasSol || west.Cost <= east.Cost))
            {
                sol.Cost += west.Cost;
                sol.HasSol = true;
                sol.Path += west.Path;
            }

            numberOfSteps = curStep - 1;
        }

        /// <summary>
        /// Check if can move West, if moved calculate cost
        /// </summary>
        /// <param name="marbles"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="sol"></param>
        static void LiftWest(MarbleHoleLocation[] marbles, MarbleHoleLocation[] holes, ref int numberOfMarbles, WallLocation[] walls, int numberOfWalls, int sizeBoard, ref Solution sol)
        {
            bool safeDiscard = false, placed;
            int countOfPlacedBalls = 0, curStep = numberOfSteps, columnId = sizeBoard - 1, copyNOM = numberOfMarbles, newNumMarbles, newId = 0;
            MarbleHoleLocation[] newMarbles, newHoles, copyHoles;

            if (numberOfSteps >= maxNumSteps || numberOfMarbles == 0)
            {
                numberOfSteps--;
                return;
            }

            numberOfSteps++;

            if (!CanMoveEast(marbles, holes, numberOfMarbles, walls, numberOfWalls, sizeBoard, ref countOfPlacedBalls))
            {
                numberOfSteps--;
                return;
            }

            if (countOfPlacedBalls > 0) newNumMarbles = numberOfMarbles - countOfPlacedBalls;
            else newNumMarbles = numberOfMarbles;

            sol.Path += "W";
            sol.Cost++;

            if (newNumMarbles == 0)
            {
                if (sol.Cost < maxNumSteps) maxNumSteps = numberOfSteps;

                sol.HasSol = true;
                numberOfSteps--;
                return;
            }

            newMarbles = new MarbleHoleLocation[newNumMarbles];
            newHoles = new MarbleHoleLocation[newNumMarbles];
            copyHoles = new MarbleHoleLocation[numberOfMarbles];

            InitSol(out Solution north);
            InitSol(out Solution south);
            InitSol(out Solution east);
            InitSol(out Solution west);

            holes.CopyTo(copyHoles, 0);

            do
            {
                for (int i = 0; i < numberOfMarbles; i++)
                {
                    if (marbles[i].Column == columnId)
                    {
                        placed = false;

                        GetMoveEastLocation(ref marbles[i], newMarbles, ref newId, copyHoles, ref copyNOM, walls, numberOfWalls, sizeBoard, out MarbleHoleLocation temp, ref safeDiscard, ref placed);
                        if (!placed)
                        {
                            newMarbles[newId] = temp;
                            newHoles[newId++] = holes[i];
                        }
                        else
                        {
                            for (int t = i; t < (copyNOM - 1); i++)
                                copyHoles[t] = copyHoles[t + 1];
                            copyNOM--;
                        }
                    }
                }

            } while (columnId-- > 0);

            LiftNorth(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref north);
            numberOfSteps = curStep;
            LiftSouth(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref south);

            if (newNumMarbles != numberOfMarbles)
            {
                numberOfSteps = curStep;
                LiftEast(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref east);
                numberOfSteps = curStep;
                LiftWest(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref west);
            }

            if (north.HasSol && (!east.HasSol || north.Cost <= east.Cost) && (!south.HasSol || north.Cost <= south.Cost) && (!west.HasSol || north.Cost <= west.Cost))
            {
                sol.Cost += north.Cost;
                sol.HasSol = true;
                sol.Path += north.Path;
            }
            else if (east.HasSol && (!north.HasSol || east.Cost <= north.Cost) && (!south.HasSol || east.Cost <= south.Cost) && (!west.HasSol || east.Cost <= west.Cost))
            {
                sol.Cost += east.Cost;
                sol.HasSol = true;
                sol.Path += east.Path;
            }
            else if (south.HasSol && (!north.HasSol || south.Cost <= north.Cost) && (!east.HasSol || south.Cost <= east.Cost) && (!west.HasSol || south.Cost <= west.Cost))
            {
                sol.Cost += south.Cost;
                sol.HasSol = true;
                sol.Path += south.Path;
            }
            else if (west.HasSol && (!north.HasSol || west.Cost <= north.Cost) && (!south.HasSol || west.Cost <= south.Cost) && (!east.HasSol || west.Cost <= east.Cost))
            {
                sol.Cost += west.Cost;
                sol.HasSol = true;
                sol.Path += west.Path;
            }

            numberOfSteps = curStep - 1;
        }

        /// <summary>
        /// Check if can move South, if moved calculate cost
        /// </summary>
        /// <param name="marbles"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="sol"></param>
        static void LiftSouth(MarbleHoleLocation[] marbles, MarbleHoleLocation[] holes, ref int numberOfMarbles, WallLocation[] walls, int numberOfWalls, int sizeBoard, ref Solution sol)
        {
            bool safeDiscard = false, placed = false;
            int countOfPlacedBalls = 0, curStep = numberOfSteps, rowId = 0, copyNOM = numberOfMarbles, newNumMarbles, newId = 0;
            MarbleHoleLocation[] newMarbles, newHoles, copyHoles;

            if (numberOfSteps >= maxNumSteps || numberOfMarbles == 0)
            {
                numberOfSteps--;
                return;
            }

            numberOfSteps++;

            if (!CanMoveNorth(marbles, holes, numberOfMarbles, walls, numberOfWalls, sizeBoard, ref countOfPlacedBalls))
            {
                numberOfSteps--;
                return;
            }

            if (countOfPlacedBalls > 0) newNumMarbles = numberOfMarbles - countOfPlacedBalls;
            else newNumMarbles = numberOfMarbles;

            sol.Path += "S";
            sol.Cost++;


            if (newNumMarbles == 0)
            {
                if (sol.Cost < maxNumSteps)
                    maxNumSteps = numberOfSteps;

                sol.HasSol = true;
                numberOfSteps--;
                return;
            }

            newMarbles = new MarbleHoleLocation[newNumMarbles];
            newHoles = new MarbleHoleLocation[newNumMarbles];
            copyHoles = new MarbleHoleLocation[numberOfMarbles];

            InitSol(out Solution north);
            InitSol(out Solution south);
            InitSol(out Solution east);
            InitSol(out Solution west);

            holes.CopyTo(copyHoles, 0);

            while (rowId < sizeBoard)
            {
                for (int i = 0; i < numberOfMarbles; i++)
                    if (rowId == marbles[i].Row)
                    {
                        GetMoveNorthLocation(ref marbles[i], newMarbles, ref newId, copyHoles, ref copyNOM, walls, numberOfWalls, sizeBoard, out MarbleHoleLocation temp, ref safeDiscard, ref placed);

                        if (!placed)
                        {
                            newMarbles[newId] = temp;
                            newMarbles[newId].Number = marbles[i].Number;
                            newHoles[newId++] = holes[i];
                        }
                        else
                        {
                            for (int t = i; t < (copyNOM - 1); t++)
                                copyHoles[t] = copyHoles[t + 1];
                            copyNOM--;
                        }
                    }
                rowId++;
            }

            LiftEast(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref east);
            numberOfSteps = curStep;
            LiftWest(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref west);

            if (newNumMarbles != numberOfMarbles)
            {
                numberOfSteps = curStep;
                LiftNorth(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref north);
                numberOfSteps = curStep;
                LiftSouth(newMarbles, newHoles, ref newNumMarbles, walls, numberOfWalls, sizeBoard, ref south);
            }

            if (north.HasSol && (!east.HasSol || north.Cost <= east.Cost) && (!south.HasSol || north.Cost <= south.Cost) && (!west.HasSol || north.Cost <= west.Cost))
            {
                sol.Cost += north.Cost;
                sol.HasSol = true;
                sol.Path += north.Path;
            }
            else if (east.HasSol && (!north.HasSol || east.Cost <= north.Cost) && (!south.HasSol || east.Cost <= south.Cost) && (!west.HasSol || east.Cost <= west.Cost))
            {
                sol.Cost += east.Cost;
                sol.HasSol = true;
                sol.Path += east.Path;
            }
            else if (south.HasSol && (!north.HasSol || south.Cost <= north.Cost) && (!east.HasSol || south.Cost <= east.Cost) && (!west.HasSol || south.Cost <= west.Cost))
            {
                sol.Cost += south.Cost;
                sol.HasSol = true;
                sol.Path += south.Path;
            }
            else if (west.HasSol && (!north.HasSol || west.Cost <= north.Cost) && (!south.HasSol || west.Cost <= south.Cost) && (!east.HasSol || west.Cost <= east.Cost))
            {
                sol.Cost += west.Cost;
                sol.HasSol = true;
                sol.Path += west.Path;
            }

            numberOfSteps = curStep - 1;
        }
        #endregion

        #region CanMovePole
        /// <summary>
        /// Check if can move to South, when lifting South
        /// </summary>
        /// <param name="marbles"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="numOfPlaced"></param>
        /// <returns></returns>
        static bool CanMoveSouth(MarbleHoleLocation[] marbles, MarbleHoleLocation[] holes, int numberOfMarbles, WallLocation[] walls, int numberOfWalls, int sizeBoard, ref int numOfPlaced)
        {
            bool wrong, placed, canMove = false;
            int rowId = sizeBoard - 1, newNum = 0, copyNOM = numberOfMarbles;
            MarbleHoleLocation[] tempMarbles, copyHoles;

            tempMarbles = new MarbleHoleLocation[numberOfMarbles];
            copyHoles = new MarbleHoleLocation[numberOfMarbles];
            holes.CopyTo(copyHoles, 0);

            do
            {
                for (int i = 0; i < numberOfMarbles; i++)
                {
                    placed = wrong = false;

                    if (marbles[i].Row == rowId)
                    {
                        GetMoveSouthLocation(ref marbles[i], tempMarbles, ref newNum, copyHoles, ref copyNOM, walls, numberOfWalls, sizeBoard, out MarbleHoleLocation temp, ref wrong, ref placed);

                        if (placed)
                        {
                            for (int t = i; t < (copyNOM - 1); t++)
                                copyHoles[t] = copyHoles[t + 1];

                            numOfPlaced++;
                            copyNOM--;
                        }
                        else tempMarbles[newNum++] = temp;

                        if (wrong) return false;

                        if (GetCurrent(temp.Row, temp.Column, sizeBoard) != GetCurrent(marbles[i].Row, marbles[i].Column, sizeBoard))
                            canMove = true;
                    }
                }
            } while (rowId-- > 0);

            return canMove;
        }

        /// <summary>
        /// Check if can move to South, when lifting North
        /// </summary>
        /// <param name="marbles"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="numOfPlaced"></param>
        /// <returns></returns>
        static bool CanMoveNorth(MarbleHoleLocation[] marbles, MarbleHoleLocation[] holes, int numberOfMarbles, WallLocation[] walls, int numberOfWalls, int sizeBoard, ref int numOfPlaced)
        {
            bool wrong, placed, canMove = false;
            int rowId = 0, newNum = 0, copyNOM = numberOfMarbles;
            MarbleHoleLocation[] tempMarbles, copyHoles;

            tempMarbles = new MarbleHoleLocation[numberOfMarbles];
            copyHoles = new MarbleHoleLocation[numberOfMarbles];

            holes.CopyTo(copyHoles, 0);

            while (rowId < sizeBoard)
            {
                for (int i = 0; i < numberOfMarbles; i++)
                {
                    placed = wrong = false;

                    if (marbles[i].Row == rowId)
                    {
                        GetMoveNorthLocation(ref marbles[i], tempMarbles, ref newNum, copyHoles, ref copyNOM, walls, numberOfWalls, sizeBoard, out MarbleHoleLocation temp, ref wrong, ref placed);

                        if (placed)
                        {
                            for (int t = i; t < (copyNOM - 1); t++)
                                copyHoles[t] = copyHoles[t + 1];

                            numOfPlaced++;
                            copyNOM--;
                        }
                        else tempMarbles[newNum++] = temp;

                        if (wrong) return true;

                        if (GetCurrent(temp.Row, temp.Column, sizeBoard) != GetCurrent(marbles[i].Row, marbles[i].Column, sizeBoard))
                        {
                            canMove = true;
                        }
                    }
                }
                rowId++;
            }

            return canMove;
        }

        /// <summary>
        /// Check if can move to South, when lifting West
        /// </summary>
        /// <param name="marbles"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="numOfPlaced"></param>
        /// <returns></returns>
        static bool CanMoveWest(MarbleHoleLocation[] marbles, MarbleHoleLocation[] holes, int numberOfMarbles, WallLocation[] walls, int numberOfWalls, int sizeBoard, ref int numOfPlaced)
        {
            bool wrong, placed, canMove = false;
            int columnId = 0, newNum = 0, copyNOM = numberOfMarbles;
            MarbleHoleLocation[] tempMarbles, copyHoles;

            tempMarbles = new MarbleHoleLocation[numberOfMarbles];
            copyHoles = new MarbleHoleLocation[numberOfMarbles];

            holes.CopyTo(copyHoles, 0);

            while (columnId < sizeBoard)
            {
                for (int i = 0; i < numberOfMarbles; i++)
                {
                    placed = wrong = false;

                    if (marbles[i].Column == columnId)
                    {
                        GetMoveWestLocation(ref marbles[i], tempMarbles, ref newNum, copyHoles, ref copyNOM, walls, numberOfWalls, sizeBoard, out MarbleHoleLocation temp, ref wrong, ref placed);

                        if (placed)
                        {
                            for (int t = i; t < (copyNOM - 1); t++)
                                copyHoles[t] = copyHoles[t + 1];

                            numOfPlaced++;
                            copyNOM--;
                        }
                        else tempMarbles[newNum++] = temp;

                        if (wrong) return false;

                        if (GetCurrent(temp.Row, temp.Column, sizeBoard) != GetCurrent(marbles[i].Row, marbles[i].Column, sizeBoard))
                            canMove = true;
                    }
                }
                columnId++;
            }

            return canMove;
        }

        /// <summary>
        /// Check if can move to South, when lifting East
        /// </summary>
        /// <param name="marbles"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="numOfPlaced"></param>
        /// <returns></returns>
        static bool CanMoveEast(MarbleHoleLocation[] marbles, MarbleHoleLocation[] holes, int numberOfMarbles, WallLocation[] walls, int numberOfWalls, int sizeBoard, ref int numOfPlaced)
        {
            bool wrong, placed, canMove = false;
            int columnId = sizeBoard - 1, newNum = 0, copyNOM = numberOfMarbles;
            MarbleHoleLocation[] tempMarbles, copyHoles;

            tempMarbles = new MarbleHoleLocation[numberOfMarbles];
            copyHoles = new MarbleHoleLocation[numberOfMarbles];

            holes.CopyTo(copyHoles, 0);

            do
            {
                for (int i = 0; i < numberOfMarbles; i++)
                {
                    wrong = false;
                    placed = false;
                    if (marbles[i].Column == columnId)
                    {
                        GetMoveEastLocation(ref marbles[i], tempMarbles, ref newNum, holes, ref numberOfMarbles, walls, numberOfWalls, sizeBoard, out MarbleHoleLocation temp, ref wrong, ref placed);

                        if (placed)
                        {
                            for (int t = i; t < (copyNOM - 1); t++)
                                copyHoles[t] = copyHoles[t + 1];

                            copyNOM--;
                            numOfPlaced++;
                        }
                        else tempMarbles[newNum++] = temp;

                        if (wrong) return false;

                        if (GetCurrent(temp.Row, temp.Column, sizeBoard) != GetCurrent(marbles[i].Row, marbles[i].Column, sizeBoard))
                            canMove = true;
                    }
                }
            } while (columnId-- > 0);

            return canMove;
        }
        #endregion

        #region GetMovePoleLocation
        /// <summary>
        /// Get new location if moving South
        /// </summary>
        /// <param name="marble"></param>
        /// <param name="marbles"></param>
        /// <param name="newNum"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="newLocation"></param>
        /// <param name="wrong"></param>
        /// <param name="placed"></param>
        static void GetMoveSouthLocation(ref MarbleHoleLocation marble, MarbleHoleLocation[] marbles, ref int newNum, MarbleHoleLocation[] holes, ref int numberOfMarbles, WallLocation[] walls,
           int numberOfWalls, int sizeBoard, out MarbleHoleLocation newLocation, ref bool wrong, ref bool placed)
        {
            int max = sizeBoard - 1;
            newLocation.Row = marble.Row;
            newLocation.Column = marble.Column;
            newLocation.Number = marble.Number;

            placed = wrong = false;

            if (newLocation.Row == (sizeBoard - 1)) return;

            for (int i = 0; i < newNum; i++)
                if (marbles[i].Column == marble.Column && marbles[i].Row > marble.Row && (marbles[i].Row <= max))
                    max = marbles[i].Row - 1;

            for (int i = 0; i < numberOfWalls; i++)
            {
                if ((walls[i].FirstSide.Column - walls[i].SecondSide.Column) != 0) continue;

                if (walls[i].FirstSide.Column == marble.Column && walls[i].FirstSide.Row > marble.Row && walls[i].SecondSide.Row >= marble.Row && walls[i].FirstSide.Row > walls[i].SecondSide.Row && walls[i].SecondSide.Row < max)
                    max = walls[i].SecondSide.Row;

                if (walls[i].FirstSide.Column == marble.Column && walls[i].SecondSide.Row > marble.Row && walls[i].FirstSide.Row >= marble.Row && walls[i].SecondSide.Row > walls[i].FirstSide.Row && walls[i].FirstSide.Row < max)
                    max = walls[i].FirstSide.Row;
            }

            for (int i = 0; i < numberOfMarbles; i++)
                if (holes[i].Column == marble.Column && holes[i].Row > marble.Row && (holes[i].Row <= max))
                {
                    max = holes[i].Row;
                    if (marble.Number != holes[i].Number) wrong = true;
                    else placed = true;
                }

            if (max > newLocation.Row) newLocation.Row = max;
        }

        /// <summary>
        /// Get new location if moving North
        /// </summary>
        /// <param name="marble"></param>
        /// <param name="marbles"></param>
        /// <param name="newNum"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="newLocation"></param>
        /// <param name="wrong"></param>
        /// <param name="placed"></param>
        static void GetMoveNorthLocation(ref MarbleHoleLocation marble, MarbleHoleLocation[] marbles, ref int newNum, MarbleHoleLocation[] holes, ref int numberOfMarbles, WallLocation[] walls,
       int numberOfWalls, out MarbleHoleLocation newLocation, ref bool wrong, ref bool placed)
        {
            int max = 0;

            newLocation.Row = marble.Row;
            newLocation.Column = marble.Column;
            newLocation.Number = marble.Number;

            placed = wrong = false;

            if (newLocation.Row == 0) return;

            for (int i = 0; i < newNum; i++)
                if (marbles[i].Column == marble.Column &&
                   marbles[i].Row < marble.Row && marbles[i].Row >= max)
                    max = marbles[i].Row + 1;

            for (int i = 0; i < numberOfWalls; i++)
            {
                if ((walls[i].FirstSide.Column - walls[i].SecondSide.Column) != 0)
                    continue;

                if (walls[i].FirstSide.Column == marble.Column && walls[i].FirstSide.Row < marble.Row && walls[i].SecondSide.Row <= marble.Row && walls[i].FirstSide.Row < walls[i].SecondSide.Row && walls[i].SecondSide.Row > max)
                    max = walls[i].SecondSide.Row;

                if (walls[i].FirstSide.Column == marble.Column && walls[i].SecondSide.Row < marble.Row && walls[i].FirstSide.Row <= marble.Row && walls[i].SecondSide.Row < walls[i].FirstSide.Row && walls[i].FirstSide.Row > max)
                    max = walls[i].FirstSide.Row;
            }

            for (int i = 0; i < numberOfMarbles; i++)
                if (holes[i].Column == marble.Column && holes[i].Row < marble.Row && holes[i].Row >= max)
                {
                    max = holes[i].Row;
                    if (marble.Number != holes[i].Number) wrong = true;
                    else placed = true;
                }

            if (max < newLocation.Row) newLocation.Row = max;
        }

        /// <summary>
        /// Get new location if moving West
        /// </summary>
        /// <param name="marble"></param>
        /// <param name="marbles"></param>
        /// <param name="newNum"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="newLocation"></param>
        /// <param name="wrong"></param>
        /// <param name="placed"></param>
        static void GetMoveWestLocation(ref MarbleHoleLocation marble, MarbleHoleLocation[] marbles, ref int newNum, MarbleHoleLocation[] holes, ref int numberOfMarbles, WallLocation[] walls,
       int numberOfWalls, out MarbleHoleLocation newLocation, ref bool wrong, ref bool placed)
        {
            int min = 0;

            newLocation.Row = marble.Row;
            newLocation.Column = marble.Column;
            newLocation.Number = marble.Number;

            placed = wrong = false;

            if (newLocation.Column == 0) return;

            for (int i = 0; i < newNum; i++)
                if (marbles[i].Row == marble.Row && marbles[i].Column < marble.Column && marbles[i].Column >= min)
                    min = marbles[i].Column + 1;

            for (int i = 0; i < numberOfWalls; i++)
            {
                if ((walls[i].FirstSide.Row - walls[i].SecondSide.Row) != 0)
                    continue;

                if (walls[i].FirstSide.Row == marble.Row && walls[i].FirstSide.Column < marble.Column && walls[i].SecondSide.Column <= marble.Column && walls[i].FirstSide.Column < walls[i].SecondSide.Column && walls[i].SecondSide.Column > min)
                    min = walls[i].SecondSide.Column;

                if (walls[i].FirstSide.Row == marble.Row && walls[i].SecondSide.Column < marble.Column && walls[i].FirstSide.Column <= marble.Column && walls[i].SecondSide.Column < walls[i].FirstSide.Column && walls[i].FirstSide.Column > min)
                    min = walls[i].FirstSide.Column;
            }

            for (int i = 0; i < numberOfMarbles; i++)
                if (holes[i].Row == marble.Row && holes[i].Column < marble.Column && holes[i].Column >= min)
                {
                    min = holes[i].Column;
                    if (marble.Number != holes[i].Number) wrong = true;
                    else placed = true;
                }

            if (min < newLocation.Column) newLocation.Column = min;
        }

        /// <summary>
        /// Get new location if moving East
        /// </summary>
        /// <param name="marble"></param>
        /// <param name="marbles"></param>
        /// <param name="newNum"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="newLocation"></param>
        /// <param name="wrong"></param>
        /// <param name="placed"></param>
        static void GetMoveEastLocation(ref MarbleHoleLocation marble, MarbleHoleLocation[] marbles, ref int newNum, MarbleHoleLocation[] holes, ref int numberOfMarbles, WallLocation[] walls,
       int numberOfWalls, int sizeBoard, out MarbleHoleLocation newLocation, ref bool wrong, ref bool placed)
        {
            int min = sizeBoard - 1;

            newLocation.Row = marble.Row;
            newLocation.Column = marble.Column;
            newLocation.Number = marble.Number;

            placed = wrong = false;

            if (newLocation.Column == (sizeBoard - 1)) return;

            for (int i = 0; i < newNum; i++)
                if (marbles[i].Row == marble.Row && marbles[i].Column > marble.Column && marbles[i].Column <= min)
                    min = marbles[i].Column - 1;

            for (int i = 0; i < numberOfWalls; i++)
            {
                if ((walls[i].FirstSide.Row - walls[i].SecondSide.Row) != 0)
                    continue;

                if (walls[i].FirstSide.Row == marble.Row && walls[i].FirstSide.Column > marble.Column && walls[i].SecondSide.Column >= marble.Column && walls[i].FirstSide.Column > walls[i].SecondSide.Column && walls[i].SecondSide.Column < min)
                    min = walls[i].SecondSide.Column;

                if (walls[i].FirstSide.Row == marble.Row && walls[i].SecondSide.Column > marble.Column && walls[i].FirstSide.Column >= marble.Column && walls[i].SecondSide.Column > walls[i].FirstSide.Column && walls[i].FirstSide.Column < min)
                    min = walls[i].FirstSide.Column;
            }

            for (int i = 0; i < numberOfMarbles; i++)
                if (holes[i].Row == marble.Row && holes[i].Column > marble.Column && holes[i].Column <= min)
                {
                    min = holes[i].Column;

                    if (marble.Number != holes[i].Number) wrong = true;
                    else placed = true;
                }

            if (min > newLocation.Column) newLocation.Column = min;
        }
        #endregion
        #endregion

        #region OtherMethods
        /// <summary>
        /// Initialize solution structure
        /// </summary>
        /// <param name="sol"></param>
        static void InitSol(out Solution sol)
        {
            sol.Cost = 0;
            sol.HasSol = false;
            sol.Path = string.Empty;
        }

        /// <summary>
        /// The intention is to remove all abnormal wall, hole inputs
        /// </summary>
        /// <param name="reachable"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="marbles"></param>
        /// <param name="holes"></param>
        /// <param name="numberOfMarbles"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <returns></returns>
        static bool FindBasicCases(bool[,] reachable, int sizeBoard, MarbleHoleLocation[] marbles, MarbleHoleLocation[] holes, int numberOfMarbles, WallLocation[] walls, int numberOfWalls)
        {
            for (int i = 0; i < numberOfMarbles; i++)
                if (!reachable[GetCurrent(marbles[i].Row, marbles[i].Column, sizeBoard), GetCurrent(holes[i].Row, holes[i].Column, sizeBoard)])
                    return false;

            for (int i = 0; i < numberOfMarbles; i++)
                if (!IsAnyWallInRowColumn(holes[i].Row, holes[i].Column, walls, numberOfWalls, sizeBoard))
                    return false;

            return true;
        }

        /// <summary>
        /// Check if the hole can be reached by marble with support of any of the wall
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <param name="sizeBoard"></param>
        /// <returns></returns>
        static bool IsAnyWallInRowColumn(int row, int column, WallLocation[] walls, int numberOfWalls, int sizeBoard)
        {
            if (row == 0 || column == 0 || row == (sizeBoard - 1) || column == (sizeBoard - 1))
                return true;

            for (int i = 0; i < numberOfWalls; i++)
                if (walls[i].FirstSide.Row == row || walls[i].FirstSide.Column == column || walls[i].SecondSide.Row == row || walls[i].SecondSide.Column == column)
                    return true;

            return false;
        }

        /// <summary>
        /// Check for all nodes connectivity
        /// </summary>
        /// <param name="reachable"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        static void FindReachability(bool[,] reachable, int sizeBoard, WallLocation[] walls, int numberOfWalls)
        {
            for (int i = 0; i < sizeBoard * sizeBoard; i++)
                for (int j = 0; j < sizeBoard * sizeBoard; j++)
                    reachable[i, j] = CheckConnected(i, j, sizeBoard, walls, numberOfWalls);

            for (int i = 0; i < sizeBoard * sizeBoard; i++)
                for (int j = 0; j < sizeBoard * sizeBoard; j++)
                    for (int k = 0; k < sizeBoard * sizeBoard; k++)
                        reachable[j, k] |= (reachable[j, i] & reachable[i, k]);
        }

        /// <summary>
        /// Check connectivity for row/column represented node
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <returns></returns>
        static bool CheckConnected(int i, int j, int sizeBoard, WallLocation[] walls, int numberOfWalls)
        {
            int row = i / sizeBoard, column = i % sizeBoard;
            int left = GetLeft(row, column, sizeBoard, walls, numberOfWalls);
            int right = GetRight(row, column, sizeBoard, walls, numberOfWalls);
            int top = GetTop(row, column, sizeBoard, walls, numberOfWalls);
            int bottom = GetBottom(row, column, sizeBoard, walls, numberOfWalls);

            return left == j || right == j || top == j || bottom == j;
        }

        /// <summary>
        /// Return node number of row, column box
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="sizeBoard"></param>
        /// <returns></returns>
        static int GetCurrent(int row, int column, int sizeBoard)
        {
            return (row * sizeBoard) + column;
        }
        #endregion

        #region GettingSides
        /// <summary>
        /// Returns node number of left box
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <returns></returns>
        static int GetLeft(int row, int column, int sizeBoard, WallLocation[] walls, int numberOfWalls)
        {
            if (column == 0) GetCurrent(row, column, sizeBoard);

            for (int i = 0; i < numberOfWalls; i++)
                if (row == walls[i].FirstSide.Row && row == walls[i].SecondSide.Row && ((walls[i].FirstSide.Column == column && walls[i].SecondSide.Column < column) || (walls[i].SecondSide.Column == column && walls[i].FirstSide.Column < column)))
                    return GetCurrent(row, column, sizeBoard);

            return GetCurrent(row, column - 1, sizeBoard);
        }

        /// <summary>
        /// Returns node number of right box
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <returns></returns>
        static int GetRight(int row, int column, int sizeBoard, WallLocation[] walls, int numberOfWalls)
        {
            if (column == (sizeBoard - 1)) return GetCurrent(row, column, sizeBoard);

            for (int i = 0; i < numberOfWalls; i++)
                if (row == walls[i].FirstSide.Row && row == walls[i].SecondSide.Row && ((walls[i].FirstSide.Column == column && walls[i].SecondSide.Column > column) || (walls[i].SecondSide.Column == column && walls[i].FirstSide.Column > column)))
                    return GetCurrent(row, column, sizeBoard);

            return GetCurrent(row, column + 1, sizeBoard);
        }

        /// <summary>
        /// Returns node number of top box
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <returns></returns>
        static int GetTop(int row, int column, int sizeBoard, WallLocation[] walls, int numberOfWalls)
        {
            if (row == 0) return GetCurrent(row, column, sizeBoard);

            for (int i = 0; i < numberOfWalls; i++)
                if (column == walls[i].FirstSide.Column && column == walls[i].SecondSide.Column && ((walls[i].FirstSide.Row == row && walls[i].SecondSide.Row < row) || (walls[i].SecondSide.Row == row && walls[i].FirstSide.Row < row)))
                    return GetCurrent(row, column, sizeBoard);
            return GetCurrent(row - 1, column, sizeBoard);
        }

        /// <summary>
        /// Returns node number of bottom box
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="sizeBoard"></param>
        /// <param name="walls"></param>
        /// <param name="numberOfWalls"></param>
        /// <returns></returns>
        static int GetBottom(int row, int column, int sizeBoard, WallLocation[] walls, int numberOfWalls)
        {
            if (row == (sizeBoard - 1)) return GetCurrent(row, column, sizeBoard);

            for (int i = 0; i < numberOfWalls; i++)
                if (column == walls[i].FirstSide.Column && column == walls[i].SecondSide.Column && ((walls[i].FirstSide.Row == row && walls[i].SecondSide.Row > row) || (walls[i].SecondSide.Row == row && walls[i].FirstSide.Row > row)))
                    return GetCurrent(row, column, sizeBoard);

            return GetCurrent(row + 1, column, sizeBoard);
        }
        #endregion
    }
}
