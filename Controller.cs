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
        
        
        power.rightElements.Add(copperWire1);
        copperWire1.leftElements.Add(power);

        // 導線が分岐する
        copperWire1.rightElements.Add(copperWire2);
        copperWire2.leftElements.Add(copperWire1);
        copperWire1.rightElements.Add(copperWire3);
        copperWire3.leftElements.Add(copperWire1);
        
        // 分岐したそれぞれの導線に豆電球をつなぐ
        copperWire2.rightElements.Add(light1);
        light1.leftElements.Add(copperWire2);
        light1.rightElements.Add(copperWire4);
        copperWire4.leftElements.Add(light1);
        
        copperWire3.rightElements.Add(light2);
        light2.leftElements.Add(copperWire3);
        light2.rightElements.Add(copperWire5);
        copperWire5.leftElements.Add(light2);
        
        // 分岐した導線が合流する
        copperWire4.rightElements.Add(copperWire6);
        copperWire6.leftElements.Add(copperWire4);
        copperWire5.rightElements.Add(copperWire6);
        copperWire6.leftElements.Add(copperWire5);
        
        // 合流した導線が電源に戻る
        copperWire6.rightElements.Add(power);
        power.leftElements.Add(copperWire6);
        
        // 合流した導線に余計なヒゲを生やす
        copperWire6.rightElements.Add(copperWire7);
        copperWire7.leftElements.Add(copperWire6);

        var result = new List<List<Element>>();
        var circle = new List<Element>();
        FindCircle(result, circle, power);
        
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
                    s += $"+ {e._voltage}";
                    continue;
                }
                
                Debug.Log($"{e._name}");
                s += $"- {e._resistance}i";
            }
            Debug.Log($"式 {s} = 0");
        });
    }

/*    private List<Element> FindCircle(Element entry)
    {
        var result = new List<Element>();
        
        Element target = entry;
        do
        {
            result.Add(target);
            target = target.rightElements[0];
        } while (target != entry);

        result.ForEach(element =>
        {
            Debug.Log($"{element._name}");
        });
        
        return result;
    }*/

    private void FindCircle(List<List<Element>> result, List<Element> circle, Element target)
    {

        circle.Add(target);
        
        target.rightElements.ForEach(next =>
        {
            var c = new List<Element>(circle);
            
            // 最初だったら
            if (c.Count == 1)
            {
                FindCircle(result, c, next);
                return;
            }
        
            // 戻ってきたら
            if (c[0] == next)
            {
                result.Add(c);
                return;
            }
        
            FindCircle(result, c, next);
        });
    }
}
