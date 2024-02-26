using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using GeneticSharp;
using Sudoku.Shared;


namespace Sudoku.GeneticAlgorithm
{
    public class GeneticSolver : ISudokuSolver
    {
        //Implémente la méthode Solve de ISudokuSolver
        public SudokuGrid Solve(SudokuGrid s)
        {
            // Créez une instance de fitness pour évaluer les chromosomes Sudoku
            var fitness = new SudokuFitness(s);

            var selection = new EliteSelection();

            var crossover = new UniformCrossover();

            var mutation = new UniformMutation();

           

            var sudokuChromosome = new SudokuPermutationsChromosome(s);

            var currentPopulationSize = 100;

            var solved = false;

            SudokuGrid bestSudokuBoard = null;

			while (!solved)
            {

				// Créez une population pour le solveur génétique
				var population = new Population(currentPopulationSize, currentPopulationSize, sudokuChromosome);

				// Créez un solveur génétique avec les opérateurs de sélection, crossover et mutation appropriés
				var ga = new GeneticSharp.GeneticAlgorithm(population, fitness, selection, crossover, mutation);
				ITermination termination = new OrTermination(new FitnessThresholdTermination(0), new FitnessStagnationTermination(50));
				ga.Termination = termination;

				// Exécutez le solveur génétique
				ga.Start();

				// Récupérez le meilleur chromosome après la résolution
				var bestChromosome = ga.BestChromosome as ISudokuChromosome;

				// Récupérez la meilleure grille Sudoku à partir du meilleur chromosome
				
				bestSudokuBoard = bestChromosome.GetSudokus().First();

				solved = bestSudokuBoard.NbErrors(s) == 0;
				currentPopulationSize *= 2;
            }


            // Retournez la grille Sudoku résolue
            return bestSudokuBoard;

        }
    }
}



