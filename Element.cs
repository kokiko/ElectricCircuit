using System;
using System.Collections.Generic;

public class Element
{
    public class SearchParameter
    {
        public SearchParameter(Terminal right, Terminal left)
        {
            _right = right;
            _left = left;
        }

        private readonly Terminal _right;
        private readonly Terminal _left;

        public int ForkCount = 0;
        public readonly Stack<Terminal> PrevStack = new Stack<Terminal>();

        public Current current;

        public void PushPrev(Element prevConnection)
        {
            if (_right._connections.Contains(prevConnection))
            {
                if (_left._connections.Contains(prevConnection))
                {
                    throw new Exception("参照関係がおかしい");
                }

                PrevStack.Push(_right);
                return;
            }

            if (_left._connections.Contains(prevConnection))
            {
                if (_right._connections.Contains(prevConnection))
                {
                    throw new Exception("参照関係がおかしい");
                }

                PrevStack.Push(_left);
                return;
            }

            throw new Exception("参照関係がおかしい");
        }

        public Terminal GetNext()
        {
            var prev = PrevStack.Peek();

            if (prev == null)
            {
                return null;
            }

            return _right.Equals(prev) ? _left : _right;
        }

        public Terminal GetPrev()
        {
            return PrevStack.Peek();
        }
    }

    public class Terminal
    {
        public List<Element> _connections = new List<Element>();
    }

    public string _name;
    public double _voltage = 0;
    public double _resistance = 0;

    public Terminal right = new Terminal();
    public Terminal left = new Terminal();

    private SearchParameter searchParam;

    public SearchParameter GetSearchParam()
    {
        if (searchParam == null)
        {
            searchParam = new SearchParameter(right, left);
        }

        return searchParam;
    }


    public Terminal Prev { get; private set; }
    public Terminal Next { get; private set; }
    public Current Current { get; private set; }
    public bool isFixed { get; private set; }

    public void FixParams()
    {
        var searchParam = GetSearchParam();
        Next = searchParam.GetNext();
        Prev = searchParam.GetPrev();
        Current = searchParam.current;
        isFixed = true;
    }


    double GetVoltage()
    {
        if (Current == null)
        {
            throw new Exception();
        }

        return _voltage != 0 ? _voltage : _resistance * (double) Current._intensity;
    }

    public double GetResistance()
    {
        return _resistance;
    }

    double GetCurrent()
    {
        if (Current == null)
        {
            throw new Exception();
        }

        return (double) Current._intensity;
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
