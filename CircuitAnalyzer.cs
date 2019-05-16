using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircuitAnalyzer : MonoBehaviour
{
    public static void AnalyzeCircuit(Element entryPoint)
    {
        var circuitInfo = SearchCircuit(entryPoint, null, null);

        if (circuitInfo.Circles.Count == 0)
        {
            throw new Exception("回路になっていない！！");
        }

        bool hasPower = circuitInfo.Circles.Any(c => c.Any(e => e.IsPower()));
        if (!hasPower)
        {
            throw new Exception("電池がない！！");
        }

        bool hasResistance = circuitInfo.Circles.Any(c => c.Any(e => e._resistance > 0));
        if (!hasResistance)
        {
            throw new Exception("抵抗がない（ショート回路）！！");
        }

        var currents = RemoveUnnecessaryCurrents(circuitInfo.Circles, circuitInfo.Currents);

        Formula formula = CreateFormula(circuitInfo.Circles, currents, circuitInfo.Elements);

        var dimension = formula.ConstVector.Length;
        var solution = GaussianEliminationCalculator.Calculate(formula.Matrix, formula.ConstVector, dimension);

        for (int i = 0; i < solution.Length; i++)
        {
            currents[i]._intensity = solution[i];
        }

        currents.ForEach(i => { Debug.Log($"{i._name}: {i._intensity}"); });
    }

    /// <summary>
    /// 計算に必要な行列、定数ベクトルを生成する
    /// </summary>
    /// <param name="circles">回路のリスト</param>
    /// <param name="currents">全電流</param>
    /// <returns></returns>
    private static Formula CreateFormula(List<List<Element>> circles, List<Current> currents,
        List<Element> circuitElements
    )
    {
        var dimension = Math.Max(currents.Count, circles.Count);
        double[,] matrix = new double[dimension, dimension];

        var vector = new double[dimension];

        // 電流の立式（キルヒホッフ第2）
        for (var i = 0; i < currents.Count; i++) // 列ごとに
        {
            var current = currents[i];
            for (var row = 0; row < circles.Count; row++) // 行ごと
            {
                matrix[row, i] = circles[row]
                    .Where(e => current.Equals(e._current) && !e.IsPower())
                    .Select(e => e.GetResistance())
                    .Sum();

                vector[row] = circles[row]
                    .Where(e => e.IsPower())
                    .Select(e => e._voltage)
                    .Sum();
            }
        }

        var diff = Math.Abs(currents.Count - circles.Count);

        // 電流の立式（キルヒホッフ第1）
        List<Element> branchElements = circuitElements
            .Where(e =>
            {
                return e.next._connections
                           .Where(next => { return currents.IndexOf(next._current) >= 0; })
                           .Count() > 1;
            }).ToList();

        branchElements.ForEach(e =>
        {
            var row = dimension - diff++;
            matrix[row, currents.IndexOf(e._current)] = -1;
            e.next._connections.ForEach(next =>
            {
                var currentIndex = currents.IndexOf(next._current);
                if (currentIndex >= 0)
                {
                    matrix[row, currentIndex] = 1;
                }
            });
        });

        // デバッグ出力
        Debug.Log("----- matrix -----");
        for (var r = 0; r < currents.Count; r++)
        {
            string row = "";
            for (var c = 0; c < currents.Count; c++)
            {
                row += $" {matrix[r, c]}";
            }

            Debug.Log(row);
        }

        Debug.Log("------------------");

        Debug.Log("----- vector -----");
        foreach (var d in vector)
        {
            Debug.Log(d);
        }

        Debug.Log("------------------");

        return new Formula(matrix, vector);
    }

    class Formula
    {
        public double[,] Matrix { get; private set; }
        public double[] ConstVector { get; private set; }

        public Formula(double[,] matrix, double[] constVector)
        {
            Matrix = matrix;
            ConstVector = constVector;
        }
    }

    /// <summary>
    /// 不要な電流を削除した電流リストを返す
    /// </summary>
    /// <param name="circles"></param>
    /// <param name="currents"></param>
    private static List<Current> RemoveUnnecessaryCurrents(List<List<Element>> circles, List<Current> currents)
    {
        var result = new List<Current>(currents);
        var unnecessaryCurrents = new List<Current>(currents);
        circles.ForEach(circle => { circle.ForEach(element => { unnecessaryCurrents.Remove(element._current); }); });

        unnecessaryCurrents.ForEach(current => { result.Remove(current); });
        return result;
    }

    /// <summary>
    /// 回路の各要素を再帰的に探索して情報を解析する
    /// </summary>
    /// <param name="target">探索対象要素</param>
    /// <param name="circuitInfo">回路情報（初回呼び出しはnullを指定する）</param>
    /// <param name="circle">探索サークル（初回呼び出しはnullを指定する）</param>
    private static CircuitInfo SearchCircuit(
        Element target,
        CircuitInfo circuitInfo,
        List<Element> circle)
    {
        if (circuitInfo == null)
        {
            circuitInfo = new CircuitInfo(
                new List<List<Element>>(),
                new List<Current>(),
                new List<Element>()
            );
        }

        if (circle == null)
        {
            circle = new List<Element>();
        }

        circle.Add(target);
        circuitInfo.Elements.Add(target);

        if (target._current != null)
        {
            // ignore
        }
        else if (target.prev._connections.Count == 1)
        {
            var prev = target.prev._connections[0];
            if (prev._current == null)
            {
                // 新しい電流
                target._current = new Current($"電流 {(circuitInfo.Currents.Count + 1).ToString()}");
                circuitInfo.Currents.Add(target._current);
            }
            else
            {
                // leftと同じ電流
                target._current = prev._current;
            }
        }
        else if (target.prev._connections.Count > 0)
        {
            // 新しい電流
            target._current = new Current($"電流 {(circuitInfo.Currents.Count + 1).ToString()}");
            circuitInfo.Currents.Add(target._current);
        }

        // 新しい電流
        target.next._connections.ForEach(nextElement =>
        {
            nextElement.SetPrev(target);
            if (target.next._connections.Count > 1)
            {
                if (nextElement._current == null)
                {
                    nextElement._current = new Current($"電流 {(circuitInfo.Currents.Count + 1).ToString()}");
                    circuitInfo.Currents.Add(nextElement._current);
                }
            }
        });

        // next の要素のうちどれかが circle[0] と一致していれば戻ってきたフラグを立てる
        var gonAround = target.next._connections.Any(next => circle.Count > 1 && circle[0].Equals(next));
        for (var i = 0; i < target.next._connections.Count; i++)
        {
            var next = target.next._connections[i];

            var c = new List<Element>(circle);

            // 最初だったら
            if (c.Count == 1)
            {
                return SearchCircuit(next, circuitInfo, c);
            }

            // 戻ってきたら
            if (c[0] == next)
            {
                circuitInfo.Circles.Add(c);
                if (!c.First()._current.Equals(c.Last()._current))
                {
                    var duplicatedCurrent = c.Last()._current;
                    ReplaceCurrent(c, duplicatedCurrent, c.First()._current);
                    circuitInfo.Currents.Remove(duplicatedCurrent);
                }

                continue;
            }

            if (!gonAround)
            {
                circuitInfo = SearchCircuit(next, circuitInfo, c);
            }
        }

        /*if (circuitInfo.Circles.Count == 0)
        {
            throw new Exception("回路になっていない！！");
        }

        bool hasPower = circuitInfo.Circles.Any(c => c.Any(e => e.IsPower()));
        if (!hasPower)
        {
            throw new Exception("電池がない！！");
        }

        bool hasResistance = circuitInfo.Circles.Any(c => c.Any(e => e._resistance > 0));
        if (!hasResistance)
        {
            throw new Exception("抵抗がない（ショート回路）！！");
        }*/

        return circuitInfo;
    }

    public class CircuitInfo
    {
        public List<List<Element>> Circles { get; private set; }
        public List<Current> Currents { get; private set; }
        public List<Element> Elements { get; private set; }

        public CircuitInfo(
            List<List<Element>> circles,
            List<Current> currents,
            List<Element> elements)
        {
            Circles = circles;
            Currents = currents;
            Elements = elements;
        }
    }

    /// <summary>
    /// サークル内の電流を入れ替える
    /// </summary>
    /// <param name="circle">サークル</param>
    /// <param name="from">入れ替え前の電流</param>
    /// <param name="to">入れ替え後の電流</param>
    private static void ReplaceCurrent(List<Element> circle, Current from, Current to)
    {
        circle.ForEach(element =>
        {
            if (from.Equals(element._current))
            {
                element._current = to;
            }
        });
    }
}
