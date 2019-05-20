using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircuitAnalyzer : MonoBehaviour
{
    public static void AnalyzeCircuit(Element entryPoint)
    {
        // 右方向に
        entryPoint.GetSearchParam().PrevStack.Push(entryPoint.left);

        var circuitInfo = SearchCircuit(entryPoint, null, null);

        if (circuitInfo.Circles.Count == 0)
        {
            throw new Exception("回路になっていない！！");
        }

        bool hasPower = Enumerable.Any(circuitInfo.Circles, c => Enumerable.Any(c, e => e.IsPower()));
        if (!hasPower)
        {
            throw new Exception("電池がない！！");
        }

        bool hasResistance = Enumerable.Any(circuitInfo.Circles, c => Enumerable.Any(c, e => e._resistance > 0));
        if (!hasResistance)
        {
            throw new Exception("抵抗がない（ショート回路）！！");
        }

        var circles = RemoveUnnecessaryCircles(circuitInfo.Circles);
        var currents = RemoveUnnecessaryCurrents(circles, circuitInfo.Currents);

        // 立式
        Formula formula = CreateFormula(circles, currents, circuitInfo.Elements);

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
                matrix[row, i] = Enumerable.Sum(Enumerable.Select(
                    Enumerable.Where(circles[row], e => current.Equals(e.Current) && !e.IsPower()),
                    e => e.GetResistance()));

                vector[row] = Enumerable.Sum(Enumerable.Select(Enumerable.Where(circles[row], e => e.IsPower()),
                    e => e._voltage * (e.Prev.Equals(e.left) ? 1 : -1)));
            }
        }

        // 電流の立式（キルヒホッフ第1）
        List<Element> branchElements = circuitElements
            .Where(e => currents.Contains(e.Current) && e.Next._connections.Count > 1)
            .Distinct(elem => // 同じ分岐点を除外する
            {
                // 分岐点に接続しているすべてのElementの名前を文字列にして比較
                // TODO: やり方が汚いので余裕があれば修正する
                var list = new List<Element>();
                list.Add(elem);
                elem.Next._connections.ForEach(n => { list.Add(n); });
                return string.Join("", list.OrderBy(e => e._name).ToList());
            })
            .ToList();

        var diff = Math.Abs(currents.Count - circles.Count);
        int rowIndex = dimension - diff;
        branchElements.ForEach(e =>
        {
            matrix[rowIndex, currents.IndexOf(e.Current)] = -1;
            e.Next._connections.ForEach(next =>
            {
                var currentIndex = currents.IndexOf(next.Current);
                if (currentIndex >= 0)
                {
                    matrix[rowIndex, currentIndex] = 1;
                }
            });
            rowIndex++;
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
    private static List<List<Element>> RemoveUnnecessaryCircles(List<List<Element>> circles)
    {
        return circles.Where(circle => circle.Any(element => element._resistance > 0)).ToList();
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
        circles.ForEach(circle =>
        {
            circle.ForEach(element => { unnecessaryCurrents.Remove(element.GetSearchParam().current); });
        });

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

        // target の電流を決める
        var prevTerminal = target.GetSearchParam().GetPrev();
        var nextTerminal = target.GetSearchParam().GetNext();
        if (target.GetSearchParam().current != null)
        {
            // ignore
        }
        else if (target.GetSearchParam().GetPrev()._connections.Count == 1)
        {
            var prev = prevTerminal._connections[0];
            if (prev.GetSearchParam().current == null)
            {
                // 新しい電流
                target.GetSearchParam().current = new Current($"電流 {(circuitInfo.Currents.Count + 1).ToString()}");
                circuitInfo.Currents.Add(target.GetSearchParam().current);
            }
            else
            {
                // prevと同じ電流
                target.GetSearchParam().current = prev.GetSearchParam().current;
            }
        }
        else if (prevTerminal._connections.Count > 0)
        {
            // 新しい電流
            target.GetSearchParam().current = new Current($"電流 {(circuitInfo.Currents.Count + 1).ToString()}");
            circuitInfo.Currents.Add(target.GetSearchParam().current);
        }

        // target で分岐している場合、新たな電流の定義・分岐点のマークを行う
        for (var i = 0; i < nextTerminal._connections.Count; i++)
        {
            var nextElement = nextTerminal._connections[i];
            if (!circle.Contains(nextElement))
            {
                nextElement.GetSearchParam().PushPrev(target);
            }

            if (nextTerminal._connections.Count > 1)
            {
                if (i != 0)
                {
                    target.GetSearchParam().ForkCount++;
                }

                if (nextElement.GetSearchParam().current == null)
                {
                    nextElement.GetSearchParam().current =
                        new Current($"電流 {(circuitInfo.Currents.Count + 1).ToString()}");
                    circuitInfo.Currents.Add(nextElement.GetSearchParam().current);
                }
            }
        }

        // target の先を探索する
        foreach (var next in nextTerminal._connections)
        {
            var c = new List<Element>(circle);

            // 最初だったら
            if (c.Count == 1)
            {
                return SearchCircuit(next, circuitInfo, c);
            }

            // 戻ってきたら
            if (c.Contains(next))
            {
                // EntryPoint に戻った場合
                if (c[0] == next)
                {
                    circuitInfo.Circles.Add(c);
                    if (!c.First().GetSearchParam().current.Equals(c.Last().GetSearchParam().current))
                    {
                        var duplicatedCurrent = c.Last().GetSearchParam().current;
                        ReplaceCurrent(c, duplicatedCurrent, c.First().GetSearchParam().current);
                        circuitInfo.Currents.Remove(duplicatedCurrent);
                    }

                    c.ForEach(e => { e.FixParams(); });
                }

                // 分岐点までさかのぼって探索要パラメータをCleanする
                for (var k = c.Count - 1; k > -1; k--)
                {
                    var element = c[k];
                    var searchParam = element.GetSearchParam();
                    if (searchParam.ForkCount > 0)
                    {
                        searchParam.ForkCount--;
                        break;
                    }

                    if (c[0] != next && !element.isFixed)
                    {
                        searchParam.PrevStack.Pop();
                        element.GetSearchParam().current = null;
                    }
                }

                continue;
            }

            circuitInfo = SearchCircuit(next, circuitInfo, c);
        }

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
            if (from.Equals(element.GetSearchParam().current))
            {
                element.GetSearchParam().current = to;
            }
        });
    }
}

/// <summary>
///  LINQ の Distinct関数の引数にラムダ式を使えるようにする
/// 別ファイルにしてもよい
/// </summary>
public static class IEnumerableExtensions
{
    private sealed class CommonSelector<T, TKey> : IEqualityComparer<T>
    {
        private Func<T, TKey> m_selector;

        public CommonSelector(Func<T, TKey> selector)
        {
            m_selector = selector;
        }

        public bool Equals(T x, T y)
        {
            return m_selector(x).Equals(m_selector(y));
        }

        public int GetHashCode(T obj)
        {
            return m_selector(obj).GetHashCode();
        }
    }

    public static IEnumerable<T> Distinct<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> selector
    )
    {
        return source.Distinct(new CommonSelector<T, TKey>(selector));
    }
}
