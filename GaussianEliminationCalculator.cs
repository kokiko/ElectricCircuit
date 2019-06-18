using System;
using System.Linq;
using UnityEngine;

public class GaussianEliminationCalculator : MonoBehaviour
{
    public static void Calculate(double[,] matrix, double[] vector, int dimension, double[] solution)
    {
        Forwardeliminate(matrix, vector, dimension);
        BackwardSubstitute(matrix, vector, dimension, solution);
        Debug.Log(string.Join("\n", solution.Select(x => $"{x,8:F4}")));
    }
    
    private static void Forwardeliminate(double[,] matrix, double[] vector, int dimension)
    {
        for (int i = 0; i < dimension - 1; i++)
        {
            // ピボットが0なら行を入れ替える
            if (matrix[i, i] == 0)
            {
                PartialPivoting(matrix, vector, i, dimension);
            }
            for (int j = i + 1; j < dimension; j++)
            {
                var s = matrix[j, i] / matrix[i, i];
                for (int k = i; k < dimension; k++)
                {
                    matrix[j, k] -= matrix[i, k] * s;
                }
                vector[j] -= vector[i] * s;
            }
        }
    }

    private static void PartialPivoting(double[,] matrix, double[] vector, int pivotIndex, int dimension)
    {
        var maxRow = pivotIndex;
        for (int i = pivotIndex; i < dimension; i++)
        {
            if (Math.Abs(matrix[i, pivotIndex]) > Math.Abs(matrix[maxRow, pivotIndex]))
            {
                maxRow = i;
            }
        }

        if (maxRow != pivotIndex)
        {
            double tmp;
            for (var j = pivotIndex; j < dimension; j++ )
            {
                tmp = matrix[maxRow, j];
                matrix[maxRow, j] = matrix[pivotIndex, j];
                matrix[pivotIndex, j] = tmp;
            }
            tmp = vector[maxRow];
            vector[maxRow] = vector[pivotIndex];
            vector[pivotIndex] = tmp;
        }
        else
        {
            Debug.Log("could not partial pivoting.");
        }
    }
    
    private static void BackwardSubstitute(double[,] matrix, double[] vector, int dimension, double[] solution)
    {
        for (int i = dimension - 1; i >= 0; i--)
        {
            var s = vector[i];
            for (int j = i + 1; j < dimension; j++)
            {
                s -= matrix[i, j] * solution[j];
            }
            solution[i] = s / matrix[i, i];
        }
    }
}
