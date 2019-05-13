using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class Controller : MonoBehaviour
{
    void Start()
    {
        var power = new Element();
        power._name = "電源";
        power._voltage = 1.5;
        power.prev = power.left;

        var light1 = new Element();
        light1._name = "豆電球1";
        light1._resistance = 7.5;

        var light2 = new Element();
        light2._name = "豆電球2";
        light2._resistance = 7.5;

        var copperWire1 = new Element();
        copperWire1._name = "導線1";

        var copperWire2 = new Element();
        copperWire2._name = "導線2";

        var copperWire3 = new Element();
        copperWire3._name = "導線3";

        var copperWire4 = new Element();
        copperWire4._name = "導線4";

        var copperWire5 = new Element();
        copperWire5._name = "導線5";

        var copperWire6 = new Element();
        copperWire6._name = "導線6";

        var copperWire7 = new Element();
        copperWire7._name = "導線7";


        power.right._connections.Add(copperWire1);
        copperWire1.left._connections.Add(power);

        // 導線が分岐する
        copperWire1.right._connections.Add(copperWire2);
        copperWire2.left._connections.Add(copperWire1);
        copperWire1.right._connections.Add(copperWire3);
        copperWire3.left._connections.Add(copperWire1);

        // 分岐したそれぞれの導線に豆電球をつなぐ
        copperWire2.right._connections.Add(light1);
        light1.left._connections.Add(copperWire2);
        light1.right._connections.Add(copperWire4);
        copperWire4.left._connections.Add(light1);

        copperWire3.right._connections.Add(light2);
        light2.left._connections.Add(copperWire3);
        light2.right._connections.Add(copperWire5);
        copperWire5.left._connections.Add(light2);

        // 分岐した導線が合流する
        copperWire4.right._connections.Add(copperWire6);
        copperWire6.left._connections.Add(copperWire4);
        copperWire5.right._connections.Add(copperWire6);
        copperWire6.left._connections.Add(copperWire5);

        // 合流した導線が電源に戻る
        copperWire6.right._connections.Add(power);
        power.left._connections.Add(copperWire6);

        // 合流した導線に余計なヒゲを生やす
        copperWire6.right._connections.Add(copperWire7);
        copperWire7.left._connections.Add(copperWire6);

        var circles = new List<List<Element>>();
        var circuitElements = new List<Element>();
        var currents = new List<Current>();
        FindCircle(circles, currents, circuitElements, power, null);


        Debug.Log("---------- Currents ----------");
        currents.ForEach(current => { Debug.Log(current._name); });
        //TODO: 不要な電流を削除する
        RemoveUnnecessaryCurrents(circles, currents);
        Debug.Log("---------- Currents(after remove unnecessary currents) ----------");
        currents.ForEach(current => { Debug.Log(current._name); });


        double[,] matrix = CreateMatrix(circles, currents, circuitElements);

        var b = new double[] {1.5, 1.5, 0};
        var dimension = 3;
        var solution = new double[dimension];
        GaussianEliminationCalculator.Calculate(matrix, b, dimension, solution);

        for (int i = 0; i < solution.Length; i++)
        {
            Debug.Log(currents[i]._name + " = " + solution[i]);
        }

/*        circuit.ForEach(c =>
        {
            Debug.Log("回路");
            String s = "";
            for (var i = 0; i < c.Count; i++)
            { 
                var e = c[i];
                if (e.IsCopperWire())
                {
                    continue;
                }

                if (e.IsPower())
                {
                    Debug.Log($"{e._name}");
                    Debug.Log($"{e._current._name}");
                    s += $"+ {e._voltage}";
                    continue;
                }
                
                Debug.Log($"{e._name}");
                Debug.Log($"{e._current._name}");
                s += $"- {e._resistance}i";
            }
            Debug.Log($"式 {s} = 0");
        });*/
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="circles">回路のリスト</param>
    /// <param name="currents">全電流</param>
    /// <returns></returns>
    private double[,] CreateMatrix(List<List<Element>> circles, List<Current> currents, List<Element> circuitElements
    )
    {
        var rank = Math.Max(currents.Count, circles.Count);
        double[,] matrix = new double[rank, rank];

        // r1-i1 r1-i2 r1-i3
        // r2-i1 r2-i2 r2-i3
        // r3-i1 r3-i2 r3-i3
        for (var i = 0; i < currents.Count; i++) // 列ごとに
        {
            var current = currents[i];
            for (var k = 0; k < circles.Count; k++) // 行ごと
            {
                matrix[k, i] = circles[k]
                    .Where(e => current.Equals(e._current) && !e.IsPower())
                    .Select(e => e.GetResistance())
                    .Sum();
            }
        }

        var diff = Math.Abs(currents.Count - circles.Count);

        List<Element> branchElements = circuitElements
            .Where(e =>
            {
                return e.next._connections
                           .Where(next => { return currents.IndexOf(next._current) >= 0; })
                           .Count() > 1;
            }).ToList();

        branchElements.ForEach(e =>
        {
            var row = rank - diff++;
            matrix[row, currents.IndexOf(e._current)] = -1;
            e.next._connections.ForEach(next =>
            {
                var currentIndex = currents.IndexOf(next._current);
                if (currentIndex >= 0)
                {
                    Debug.Log($"row: {row.ToString()}, column: {currentIndex.ToString()}");
                    matrix[row, currentIndex] = 1;
                }
            });
        });

        for (var r = 0; r < currents.Count; r++)
        {
            string row = "";
            for (var c = 0; c < currents.Count; c++)
            {
                row += $" {matrix[r, c]}";
            }

            Debug.Log(row);
        }

        return matrix;
    }

    private void RemoveUnnecessaryCurrents(List<List<Element>> circles, List<Current> currents)
    {
        var unnecessaryCurrents = new List<Current>(currents);
        circles.ForEach(circle => { circle.ForEach(element => { unnecessaryCurrents.Remove(element._current); }); });

        unnecessaryCurrents.ForEach(current => { currents.Remove(current); });
    }

    private List<Current> OptimizeCurrentList(List<List<Element>> circles, List<Current> currents)
    {
        var result = new List<Current>(currents);

        circles.ForEach(circle =>
        {
            var lastCurrent = circle.Last()._current;
        });

        // 重複を除外する
        // 電流の流れていないものを除外する
        return null;
    }

    /// <summary>
    ///
    /// 
    /// </summary>
    /// <param name="result">回路の構成要素をもつリストのリスト</param>
    /// <param name="currents">電流を入れるリスト</param>
    /// <param name="circuitElements">全回路の構成要素を入れるリスト</param>
    /// <param name="target">探っていく要素</param>
    /// <param name="circle">対象の循環</param>
    private void FindCircle(
        List<List<Element>> result,
        List<Current> currents,
        List<Element> circuitElements,
        Element target,
        List<Element> circle)
    {
        if (circle == null)
        {
            circle = new List<Element>();
        }

        circle.Add(target);
        circuitElements.Add(target);

        Debug.Log($"target is {target._name}");

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
                target._current = new Current($"電流 {(currents.Count + 1).ToString()}");
                currents.Add(target._current);
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
            target._current = new Current($"電流 {(currents.Count + 1).ToString()}");
            currents.Add(target._current);
        }

        // 新しい電流
        target.next._connections.ForEach(nextElement =>
        {
            nextElement.SetPrev(target);
            if (target.next._connections.Count > 1)
            {
                if (nextElement._current == null)
                {
                    nextElement._current = new Current($"電流 {(currents.Count + 1).ToString()}");
                    currents.Add(nextElement._current);
                }
            }
        });

        Debug.Log($"target current {target._current?._name}");


        for (var i = 0; i < target.next._connections.Count; i++)
        {
            var next = target.next._connections[i];

            var c = new List<Element>(circle);

            // 最初だったら
            if (c.Count == 1)
            {
                FindCircle(result, currents, circuitElements, next, c);
                return;
            }

            // 戻ってきたら
            if (c[0] == next)
            {
                result.Add(c);
                if (!c.First()._current.Equals(c.Last()._current))
                {
                    var duplicatedCurrent = c.Last()._current;
                    ReplaceCurrent(c, duplicatedCurrent, c.First()._current);
                    Debug.Log($"delete {duplicatedCurrent._name}");
                    currents.Remove(duplicatedCurrent);
                }

                continue;
            }

            FindCircle(result, currents, circuitElements, next, c);
        }
    }

    private void ReplaceCurrent(List<Element> circle, Current from, Current to)
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

static class DictionaryExt
{
    public static void AddDistinctly<T, E>(this Dictionary<T, E> current, T key, E value)
    {
        if (!current.ContainsKey(key))
        {
            current[key] = value;
        }
    }
}