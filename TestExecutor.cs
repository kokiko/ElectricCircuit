using UnityEngine;

public class TestExecutor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CircuitAnalyzer.AnalyzeCircuit(CreateCircuit2());
    }

    private Element CreateCircuit1()
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

        return power;
    }

    private Element CreateCircuit2()
    {
        var power1 = new Element();
        power1._name = "電源1";
        power1._voltage = 1.5;
        power1.prev = power1.left;

        var power2 = new Element();
        power2._name = "電源2";
        power2._voltage = 1.5;
        power2.prev = power2.left;

        var light1 = new Element();
        light1._name = "豆電球1";
        light1._resistance = 7.5;

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

        power1.right._connections.Add(copperWire1);
        copperWire1.left._connections.Add(power1);

        power2.right._connections.Add(copperWire1);
        power2.right._connections.Add(copperWire2);
        copperWire1.right._connections.Add(power2);
        copperWire2.left._connections.Add(power2);

        copperWire1.right._connections.Add(copperWire2);
        copperWire2.left._connections.Add(copperWire1);

        copperWire2.right._connections.Add(copperWire3);
        copperWire3.left._connections.Add(copperWire2);

        copperWire3.right._connections.Add(light1);
        light1.left._connections.Add(copperWire3);

        light1.right._connections.Add(copperWire4);
        copperWire4.left._connections.Add(light1);

        copperWire4.right._connections.Add(power1);
        copperWire4.right._connections.Add(copperWire5);
        power1.left._connections.Add(copperWire4);
        copperWire5.left._connections.Add(copperWire4);

        copperWire5.left._connections.Add(power1);
        power1.left._connections.Add(copperWire5);
        copperWire5.left._connections.Add(power2);
        power2.left._connections.Add(copperWire5);

        return power1;
    }
}
