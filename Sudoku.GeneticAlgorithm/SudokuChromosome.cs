using System.Collections.Generic;
using Sudoku.Shared;

namespace Sudoku.GeneticAlgorithm
{
    /// <summary>
    /// Each type of chromosome for solving a sudoku is simply required to output a list of candidate sudokus
    /// </summary>
    public interface ISudokuChromosome
    {
        IList<SudokuGrid> GetSudokus();
    }
}