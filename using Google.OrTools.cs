using Google.OrTools.Sat;
using System;

class Program
{
    static void Main(string[] args)
    {
        SolveSudoku();
    }

    static void SolveSudoku()
    {
        Solver solver = new Solver("Sudoku");

        // Définition des variables
        IntVar[,] grid = solver.MakeIntVarMatrix(9, 9, 1, 9, "grid");

        // Contraintes pour les lignes et les colonnes
        for (int i = 0; i < 9; i++)
        {
            solver.Add(grid[i, 0] + grid[i, 1] + grid[i, 2] + grid[i, 3] + grid[i, 4] + grid[i, 5] + grid[i, 6] + grid[i, 7] + grid[i, 8] == 45);
            solver.Add(grid[0, i] + grid[1, i] + grid[2, i] + grid[3, i] + grid[4, i] + grid[5, i] + grid[6, i] + grid[7, i] + grid[8, i] == 45);
        }

        // Contraintes pour les sous-grilles 3x3
        for (int i = 0; i < 9; i += 3)
        {
            for (int j = 0; j < 9; j += 3)
            {
                solver.Add(grid[i, j] + grid[i, j + 1] + grid[i, j + 2] +
                           grid[i + 1, j] + grid[i + 1, j + 1] + grid[i + 1, j + 2] +
                           grid[i + 2, j] + grid[i + 2, j + 1] + grid[i + 2, j + 2] == 45);
            }
        }

        // Initialisation des valeurs connues du Sudoku
        int[,] initialGrid = {
            {5, 3, 0, 0, 7, 0, 0, 0, 0},
            {6, 0, 0, 1, 9, 5, 0, 0, 0},
            {0, 9, 8, 0, 0, 0, 0, 6, 0},
            {8, 0, 0, 0, 6, 0, 0, 0, 3},
            {4, 0, 0, 8, 0, 3, 0, 0, 1},
            {7, 0, 0, 0, 2, 0, 0, 0, 6},
            {0, 6, 0, 0, 0, 0, 2, 8, 0},
            {0, 0, 0, 4, 1, 9, 0, 0, 5},
            {0, 0, 0, 0, 8, 0, 0, 7, 9}
        };

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (initialGrid[i, j] != 0)
                {
                    solver.Add(grid[i, j] == initialGrid[i, j]);
                }
            }
        }

        DecisionBuilder db = solver.MakePhase(grid.Flatten(), Solver.INT_VAR_SIMPLE, Solver.INT_VALUE_SIMPLE);
        solver.NewSearch(db);

        while (solver.NextSolution())
        {
            Console.WriteLine("Solution:");
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Console.Write(grid[i, j].Value() + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        solver.EndSearch();
    }
}
