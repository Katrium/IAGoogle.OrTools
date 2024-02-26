using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using GeneticSharp;
using Sudoku.Shared;

namespace Sudoku.GeneticAlgorithm
{
    /// <summary>
    /// This abstract chromosome accounts for the target mask if given, and generates an extended mask with cell domains updated according to original mask
    /// </summary>
    public abstract class SudokuChromosomeBase : ChromosomeBase, ISudokuChromosome
    {

        /// <summary>
        /// The target sudoku board to solve
        /// </summary>
        private readonly SudokuGrid _targetSudokuGrid;

        /// <summary>
        /// The cell domains updated from the initial mask for the board to solve
        /// </summary>
        //private Dictionary<int, List<int>> _extendedMask;

        /// <summary>
        /// Constructor that accepts an additional extended mask for quick cloning
        /// </summary>
        /// <param name="targetSudokuBoard">the target sudoku to solve</param>
        //// <param name="extendedMask">The cell domains after initial constraint propagation</param>
        /// <param name="length">The number of genes for the sudoku chromosome</param>
        protected SudokuChromosomeBase(SudokuGrid targetSudokuGrid, int length) : base(length)
        {
            _targetSudokuGrid = targetSudokuGrid;
            //_extendedMask = extendedMask;
            CreateGenes();
        }

        /// <summary>
        /// The target sudoku board to solve
        /// </summary>
        public SudokuGrid TargetSudokuGrid => _targetSudokuGrid;

        public virtual IList<SudokuGrid> GetSudokus()
        {
            return new List<SudokuGrid> { _targetSudokuGrid };
        }

    }
}
