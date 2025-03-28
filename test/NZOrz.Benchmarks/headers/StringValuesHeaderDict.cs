using Microsoft.Extensions.Primitives;
using NZ.Orz.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZOrz.Benchmarks.headers;

public class StringValuesHeaderDict2 : Dictionary<string, StringValues>
{
    public StringValuesHeaderDict2() : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public StringValuesHeaderDict2(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase)
    {
    }

    public void Append(string key, string value)
    {
        if (TryGetValue(key, out var v))
        {
            this[key] = StringValues.Concat(v, value);
        }
        else
        {
            Add(key, value);
        }
    }
}

public class StringValuesHeaderDict : Dictionary<string, StringV>
{
    public StringValuesHeaderDict() : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public StringValuesHeaderDict(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase)
    {
    }

    public void Append(string key, string value)
    {
        if (TryGetValue(key, out var v))
        {
            v.Append(value);
        }
        else
        {
            Add(key, value);
        }
    }
}

public class StringV
{
    private readonly string Value;

    private int length;

    private StringVNode? next;

    public int Count => length;

    public StringV(string v)
    {
        Value = v;
        length++;
    }

    public void Append(string v)
    {
        if (next == null)
        {
            next = new StringVNode(null, v);
        }
        else
        {
            next = new StringVNode(next, v);
        }
        length++;
    }

    private class StringVNode
    {
        public readonly string _value;
        public readonly StringVNode _next;

        public StringVNode(StringVNode vNode, string v)
        {
            _value = v;
            _next = vNode;
        }
    }

    public static implicit operator StringV(string? value)
    {
        return new StringV(value);
    }

    public override string ToString()
    {
        switch (length)
        {
            case 0: return null;
            case 1: return Value;
            default: return string.Join(',', GetStrings());
        }
    }

    public IEnumerable<string> GetStrings()
    {
        if (length == 0) yield break;
        yield return Value;
        var n = next;
        while (n != null)
        {
            yield return n._value;
            n = n._next;
        }
    }
}