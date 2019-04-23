using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Element
{
    public class Terminal
    {
        public List<Element> _connections = new List<Element>();
    }
    
    
    public string _name;
    public double _voltage = 0;
    public double _resistance = 0;
    public Current _current = null;
    
    public Terminal right = new Terminal();
    public Terminal left = new Terminal();


    public Terminal prev { get; /*private*/ set; }

    public Terminal next
    {
        get
        {
            if (prev == null)
            {
                return null;
            }

            return right.Equals(prev) ? left : right;
        }
        private set{}
    }

    public void SetPrev(Element prevConnection)
    {
        if (right._connections.Contains(prevConnection))
        {
            if (left._connections.Contains(prevConnection))
            {
                throw new Exception("参照関係がおかしい");
            }

            prev = right;
            return;
        }

        if (left._connections.Contains(prevConnection))
        {
            if (right._connections.Contains(prevConnection))
            {
                throw new Exception("参照関係がおかしい");
            }
            prev = left;
            return;
        }
        
        throw new Exception("参照関係がおかしい");
    }

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
