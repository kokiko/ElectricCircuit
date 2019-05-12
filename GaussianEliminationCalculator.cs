using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GaussianEliminationCalculator : MonoBehaviour
{
    public static void Calculate(double[,] matrix, double[] vector, int dimension, double[] solution)
    {
        pivot(matrix, vector, dimension);
        Forwardeliminate(matrix, vector, dimension);
        BackwardSubstitute(matrix, vector, dimension, solution);
        Debug.Log(string.Join("\n", solution.Select(x => $"{x,8:F4}")));

    }

    private static void pivot(double[,] matrix, double[] vector, int dimension)
    {
        for (var p = 0; p < dimension; p++)
        {
            int maxRow = p;
            double maxValue = 0;
            for (var row = p; row < dimension; row++)
            {
                if (Math.Abs(matrix[row, p]) > maxValue)
                {
                    maxValue = Math.Abs(matrix[row, p]);
                    maxRow = row;
                }
            }

            if (maxRow != p)
            {
                double tmp;
                for (var col = 0; col < dimension; col++)
                {
                    tmp = matrix[maxRow, col];
                    matrix[maxRow, col] = matrix[p, col];
                    matrix[p, col] = tmp;
                }
                tmp = vector[maxRow];
                vector[maxRow] = vector[p];
                vector[p] = tmp;
            }
        }
    }
    
    private static void Forwardeliminate(double[,] matrix, double[] vector, int dimension)
    {
        for (int i = 0; i < dimension - 1; i++)
        {
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
