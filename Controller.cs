using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        var result = new List<List<Element>>();
        var circle = new List<Element>();
        var currents = new List<Current>();
        FindCircle(result, currents, circle, power);
        
        result.ForEach(c =>
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
        });
    }

    private void FindCircle(
        List<List<Element>> result, 
        List<Current> currents, 
        List<Element> circle, 
        Element target)
    {

        circle.Add(target);
        
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
        else if(target.prev._connections.Count > 0){
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
        
        
        target.next._connections.ForEach(next =>
        {
            var c = new List<Element>(circle);
            
            // 最初だったら
            if (c.Count == 1)
            {
                FindCircle(result, currents, c, next);
                return;
            }
        
            // 戻ってきたら
            if (c[0] == next)
            {
                result.Add(c);
                return;
            }
        
            FindCircle(result, currents, c, next);
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
