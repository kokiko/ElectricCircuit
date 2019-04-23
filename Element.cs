using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Element
{
    public string _name;
    public double _voltage = 0;
    public double _resistance = 0;
    public Current _current = null;

    public List<Element> rightElements = new List<Element>();
    public List<Element> leftElements = new List<Element>();

    double GetVoltage()
    {
        if (_current == null)
        {
            throw new Exception();
        }
        return _voltage != 0 ? _voltage : _resistance * (double) _current._intensity;
    }

    double GetResistance()
    {
        if (IsPower())
        {
            throw new Exception();
        }
        return _resistance;
    }

    double GetCurrent()
    {
        if (_current == null)
        {
            throw new Exception();
        }

        return (double) _current._intensity;
    }

    public bool IsPower()
    {
        return _voltage != 0;
    }
    
    public bool IsCopperWire()
    {
        return !IsPower() && _resistance == 0;
    }
}
