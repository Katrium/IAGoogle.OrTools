using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp;
using Sudoku.Shared;

namespace Sudoku.GeneticAlgorithm
{
    /// <summary>
    /// This more elaborated chromosome manipulates rows instead of cells, and each of its 9 gene holds an integer for the index of the row's permutation amongst all that respect the target mask.
    /// Permutations are computed once when a new Sudoku is encountered, and stored in a static dictionary for further reference.
    /// </summary>
    public class SudokuPermutationsChromosome :  SudokuChromosomeBase, ISudokuChromosome
    {

        // Ajoutez une propriété pour stocker la référence à la grille Sudoku cible

        // Modifiez la méthode IsValidValueForCell pour utiliser la grille Sudoku cible
        private bool IsValidValueForCell(int rowIndex, int columnIndex, int value)
        {
            // Vérifiez si la valeur est dans le domaine de la cellule dans la grille Sudoku
            return SudokuGrid.CellNeighbours[rowIndex][columnIndex].All(neighbour => TargetSudokuGrid.Cells[neighbour.row][neighbour.column] != value);
        }

        /// <summary>
        /// The list of row permutations accounting for the mask
        /// </summary>
        private IList<IList<IList<int>>> _targetRowsPermutations;


        /// <summary>
        /// This constructor assumes no mask
        /// </summary>
        public SudokuPermutationsChromosome() : this(null) { }

        /// <summary>
        /// Constructor with a mask sudoku to solve, assuming a length of 9 genes
        /// </summary>
        /// <param name="targetSudokuGrid">the target sudoku to solve</param>
        public SudokuPermutationsChromosome(SudokuGrid targetSudokuGrid) : this(targetSudokuGrid, 9) { }

        /// <summary>
        /// Constructor with a mask and a number of genes
        /// </summary>
        /// <param name="targetSudokuGrid">the target sudoku to solve</param>
        /// <param name="length">the number of genes</param>
        public SudokuPermutationsChromosome(SudokuGrid targetSudokuGrid, int length) : this(targetSudokuGrid, null, length) { }

        /// <param name="targetSudokuGrid">the target sudoku to solve</param>
        /// <param name="extendedMask">The cell domains after initial constraint propagation</param>
        /// <param name="length">The number of genes for the sudoku chromosome</param>
        public SudokuPermutationsChromosome(SudokuGrid targetSudokuGrid, Dictionary<int, List<int>> extendedMask, int length) : base(targetSudokuGrid, length) { }


        /// <summary>
        /// generates a chromosome gene from its index containing a random row permutation
        /// amongst those respecting the target mask. 
        /// </summary>
        /// <param name="geneIndex">the index for the gene</param>
        /// <returns>a gene generated for the index</returns>
        public override Gene GenerateGene(int geneIndex)
        {

            var rnd = RandomizationProvider.Current;
            //we randomize amongst the permutations that account for the target mask.
            var permIdx = rnd.GetInt(0, TargetRowsPermutations[geneIndex].Count);
            return new Gene(permIdx);
        }

        public override IChromosome CreateNew()
        {
            var toReturn = new SudokuPermutationsChromosome(TargetSudokuGrid, Length);
            return toReturn;
        }
       


        /// <summary>
        /// builds a single Sudoku from the given row permutation genes
        /// </summary>
        /// <returns>a list with the single Sudoku built from the genes</returns>
        public override IList<SudokuGrid> GetSudokus()
        {
            var listInt = new List<int>(81);
            for (int i = 0; i < 9; i++)
            {
                var perm = GetPermutation(i);
                listInt.AddRange(perm);
            }
            var sudoku = new SudokuGrid();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    sudoku.Cells[i][j] = listInt[i * 9 + j];
                }
            }
            return new List<SudokuGrid>(new[] { sudoku });
        }



        /// <summary>
        /// Gets the permutation to apply from the index of the row concerned
        /// </summary>
        /// <param name="rowIndex">the index of the row to permute</param>
        /// <returns>the index of the permutation to apply</returns>
        protected virtual List<int> GetPermutation(int rowIndex)
        {
            int permIDx = GetPermutationIndex(rowIndex);
            return GetPermutation(rowIndex, permIDx);
        }


        /// <summary>
        /// Gets the permutation for a row and given a permutation index, according to the corresponding row's available permutations
        /// </summary>
        /// <param name="rowIndex">the row index for the permutation</param>
        /// <param name="permIDx">the permutation index to retrieve</param>
        /// <returns></returns>
        protected virtual List<int> GetPermutation(int rowIndex, int permIDx)
        {

			// we use a modulo operator in case the gene was swapped:
			// It may contain a number higher than the number of available permutations. 
			List<int> perm = null;
			try
            {
	            
	            perm = TargetRowsPermutations[rowIndex][permIDx % TargetRowsPermutations[rowIndex].Count].ToList();
            }
            catch (Exception e)
            {
	           Debugger.Break();
            }
            
            return perm;
        }



        /// <summary>
        /// Gets the permutation to apply from the index of the row concerned
        /// </summary>
        /// <param name="rowIndex">the index of the row to permute</param>
        /// <returns>the index of the permutation to apply</returns>
        protected virtual int GetPermutationIndex(int rowIndex)
        {
            return (int)GetGene(rowIndex).Value;
        }


        /// <summary>
        /// This method computes for each row the list of digit permutations that respect the target mask, that is the list of valid rows discarding columns and boxes
        /// </summary>
        /// <param name="sudokuBoard">the target sudoku to account for</param>
        /// <returns>the list of permutations available</returns>
        public IList<IList<IList<int>>> GetRowsPermutations()
        {
            if (TargetSudokuGrid == null)
            {
                return UnfilteredPermutations;
            }

            // we store permutations to compute them once only for each target Sudoku
            if (!_rowsPermutations.TryGetValue(TargetSudokuGrid, out var toReturn))
            {
                // Since this is a static member we use a lock to prevent parallelism.
                // This should be computed once only.
                lock (_rowsPermutations)
                {
                    if (!_rowsPermutations.TryGetValue(TargetSudokuGrid, out toReturn))
                    {
                        toReturn = GetRowsPermutationsUncached();
                        _rowsPermutations[TargetSudokuGrid] = toReturn;
                    }
                }
            }
            return toReturn;
        }

        private IList<IList<IList<int>>> GetRowsPermutationsUncached()
        {
            var toReturn = new List<IList<IList<int>>>(9);

            for (int i = 0; i < 9; i++)
            {
                var tempList = new List<IList<int>>();
                foreach (var perm in AllPermutations)
                {
                    // Permutation should be compatible with current row domains
                    var validPermutation = true;
                    for (int j = 0; j < 9; j++)
                    {
                        var cellValue = perm[j];
                        if (!IsValidValueForCell(i, j, cellValue))
                        {
                            validPermutation = false;
                            break;
                        }
                    }

                    if (validPermutation)
                    {
                        tempList.Add(perm.ToList()); // Copy the permutation to avoid modifying the original list
                    }
                }
                toReturn.Add(tempList);
            }

            return toReturn;
        }




        /// <summary>
        /// Produces 9 copies of the complete list of permutations
        /// </summary>
        public static IList<IList<IList<int>>> UnfilteredPermutations
        {
            get
            {
                if (!_unfilteredPermutations.Any())
                {
                    lock (_unfilteredPermutations)
                    {
                        if (!_unfilteredPermutations.Any())
                        {
                            _unfilteredPermutations = Range9.Select(i => AllPermutations).ToList();
                        }
                    }
                }
                return _unfilteredPermutations;
            }
        }

        /// <summary>
        /// Builds the complete list permutations for {1,2,3,4,5,6,7,8,9}
        /// </summary>
        public static IList<IList<int>> AllPermutations
        {
            get
            {
                if (!_allPermutations.Any())
                {
                    lock (_allPermutations)
                    {
                        if (!_allPermutations.Any())
                        {
                            _allPermutations = GetPermutations(Enumerable.Range(1, 9), 9);
                        }
                    }
                }
                return _allPermutations;
            }
        }

        /// <summary>
        /// The list of row permutations accounting for the mask
        /// </summary>
        public IList<IList<IList<int>>> TargetRowsPermutations
        {
            get
            {
                if (_targetRowsPermutations == null)
                {
                    _targetRowsPermutations = GetRowsPermutations();
                }
                return _targetRowsPermutations;
            }
        }

        /// <summary>
        /// The list of compatible permutations for a given Sudoku is stored in a static member for fast retrieval
        /// </summary>
        private static readonly IDictionary<SudokuGrid, IList<IList<IList<int>>>> _rowsPermutations = new Dictionary<SudokuGrid, IList<IList<IList<int>>>>();

        /// <summary>
        /// The list of row indexes is used many times and thus stored for quicker access.
        /// </summary>
        private static readonly List<int> Range9 = Enumerable.Range(0, 9).ToList();

        /// <summary>
        /// The complete list of unfiltered permutations is stored for quicker access
        /// </summary>
        private static IList<IList<int>> _allPermutations = (IList<IList<int>>)new List<IList<int>>();
        private static IList<IList<IList<int>>> _unfilteredPermutations = (IList<IList<IList<int>>>)new List<IList<IList<int>>>();

        /// <summary>
        /// Computes all possible permutation for a given set
        /// </summary>
        /// <typeparam name="T">the type of elements the set contains</typeparam>
        /// <param name="list">the list of elements to use in permutations</param>
        /// <param name="length">the size of the resulting list with permuted elements</param>
        /// <returns>a list of all permutations for given size as lists of elements.</returns>
        static IList<IList<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => (IList<T>)(new T[] { t }.ToList())).ToList();

            var enumeratedList = list.ToList();
            return (IList<IList<T>>)GetPermutations(enumeratedList, length - 1)
              .SelectMany(t => enumeratedList.Where(e => !t.Contains(e)),
                (t1, t2) => (IList<T>)t1.Concat(new T[] { t2 }).ToList()).ToList();
        }



    }
}
