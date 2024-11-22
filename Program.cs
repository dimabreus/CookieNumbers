using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public struct CookInt
{
    private static Dictionary<char, string[]> variants;
    private static readonly int bitsCount = 6;
    private static readonly int numberBase = 4;
    private static readonly string stringFormat = "cookie";

    private string _value;

    public CookInt(ulong value)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        _value = "";

        variants = new Dictionary<char, string[]>
        {
        { 'c', new[] { "c", "C", "č", "Č" } }, // cCčČ
        { 'o', new[] { "o", "O", "ǒ", "Ǒ" } }, // oOǒǑ
        { 'k', new[] { "k", "K", "ǩ", "Ǩ" } }, // kKǩǨ
        { 'i', new[] { "i", "I", "ǐ", "Ǐ" } }, // iIǐǏ
        { 'e', new[] { "e", "E", "ě", "Ě" } }  // eEěĚ
        };


        Set(value);
    }

    private void Set(ulong value)
    {
        if (value < 0 || value > ulong.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value must be between 0 and {ulong.MaxValue}.");

        _value = ToString(ToBytes(ToBits(value)));
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator CookInt(ulong value)
    {
        return new CookInt(value);
    }

    public static implicit operator CookInt(string cookieString)
    {
        if (variants == null)
            new CookInt(0);

        return new CookInt(FromBytes(StringToBytes(cookieString)));
    }

    public static explicit operator ulong(CookInt cookInt)
    {
        return FromBytes(StringToBytes(cookInt._value));
    }

    public static explicit operator long(CookInt cookInt)
    {
        return (long)FromBytes(StringToBytes(cookInt._value));
    }

    public static CookInt operator +(CookInt a, CookInt b)
    {
        return new CookInt((ulong)a + (ulong)b);
    }

    public static CookInt operator ++(CookInt a)
    {
        return a + 1;
    }

    public static CookInt operator -(CookInt a, CookInt b)
    {
        return new CookInt((ulong)a - (ulong)b);
    }

    public static CookInt operator --(CookInt a)
    {
        return a - 1;
    }

    public static CookInt operator *(CookInt a, CookInt b)
    {
        return new CookInt((ulong)a * (ulong)b);
    }

    public static CookInt operator /(CookInt a, CookInt b)
    {
        if ((ulong)b == 0)
            throw new DivideByZeroException("Division by zero is not allowed.");

        return new CookInt((ulong)a / (ulong)b);
    }

    public static CookInt operator %(CookInt a, CookInt b)
    {
        return new CookInt((ulong)a % (ulong)b);
    }

    public static bool operator ==(CookInt a, CookInt b)
    {
        return (ulong)a == (ulong)b;
    }

    public static bool operator !=(CookInt a, CookInt b)
    {
        return (ulong)a != (ulong)b;
    }

    public static bool operator >(CookInt a, CookInt b)
    {
        return (ulong)a > (ulong)b;
    }

    public static bool operator <(CookInt a, CookInt b)
    {
        return (ulong)a < (ulong)b;
    }

    public static bool operator >=(CookInt a, CookInt b)
    {
        return (ulong)a >= (ulong)b;
    }

    public static bool operator <=(CookInt a, CookInt b)
    {
        return (ulong)a >= (ulong)b;
    }

    public static CookInt operator &(CookInt a, CookInt b)
    {
        return new CookInt((ulong)a & (ulong)b);
    }

    public static CookInt operator |(CookInt a, CookInt b)
    {
        return new CookInt((ulong)a | (ulong)b);
    }

    public static CookInt operator ^(CookInt a, CookInt b)
    {
        return new CookInt((ulong)a ^ (ulong)b);
    }

    public override bool Equals(object obj)
    {
        if (obj is CookInt other)
        {
            return _value == other._value;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    private static byte[] ToBits(ulong value)
    {
        List<byte> result = new List<byte>();

        ulong number = value;

        while (number > 0)
        {
            ulong subtrahend = (ulong)Math.Floor((decimal)number / numberBase);
            ulong remainder = number - subtrahend * 4;
            number = subtrahend;

            result.Add((byte)remainder);
        }

        result.Reverse();

        return result.ToArray();
    }

    private static byte[][] ToBytes(byte[] bits)
    {
        int length = (int)(Math.Ceiling((double)bits.Count() / bitsCount) * bitsCount);
        List<byte> formattedBits = PadLeftList(bits.ToList(), length);

        return SplitList(formattedBits, bitsCount).Select(x => x.ToArray()).ToArray();
    }

    private static List<byte> PadLeftList(List<byte> list, int totalLength)
    {
        int paddingCount = totalLength - list.Count;

        if (paddingCount > 0)
        {
            list.InsertRange(0, new byte[paddingCount]);
        }

        return list;
    }

    private static List<List<T>> SplitList<T>(List<T> list, int chunkSize)
    {
        var result = new List<List<T>>();
        for (int i = 0; i < list.Count; i += chunkSize)
        {
            result.Add(list.GetRange(i, Math.Min(chunkSize, list.Count - i)));
        }
        return result;
    }

    private static string ToString(byte[][] bytes)
    {
        string result = string.Empty;

        foreach (byte[] bits in bytes)
        {
            if (result.Length > 0)
                result += "\n";

            string line = string.Empty;

            for (int i = 0; i < bits.Length; i++)
            {
                byte characterCode = bits[i];
                char neededCharacter = stringFormat[i];

                line += variants[neededCharacter][characterCode];
            }

            result += line;
        }

        return result;
    }

    private static byte[][] StringToBytes(string str)
    {
        List<byte[]> bytes = new List<byte[]>();

        foreach (string line in str.Split('\n'))
        {
            List<byte> bits = new List<byte>();

            StringInfo lineInfo = new StringInfo(line);

            for (int i = 0; i < lineInfo.LengthInTextElements; i++)
            {
                string character = lineInfo.SubstringByTextElements(i, 1);

                byte bit = BitFromChar(character).Value;
                bits.Add(bit);
            }

            if (bits.Count > 0)
                bytes.Add(bits.ToArray());
        }

        return bytes.ToArray();
    }

    private static byte? BitFromChar(string character)
    {
        foreach (char key in variants.Keys)
        {
            string[] value = variants[key];

            int index = Array.IndexOf(value, character);

            if (index != -1)
            {
                return (byte)index;
            }
        }

        return null;
    }

    private static ulong FromBytes(byte[][] bytes)
    {
        return FromBits(bytes.SelectMany(x => x).ToArray());
    }

    private static ulong FromBits(byte[] bits)
    {
        ulong result = 0;

        for (int i = 0; i < bits.Length; i++)
        {
            int power = bits.Length - i - 1;
            byte number = bits[i];

            result += number * (ulong)Math.Pow(numberBase, power);
        }

        return result;
    }
}

class Program
{
    private static CookInt myCookiesCount = "cookiĚ\ncOOKǏE";

    private static void Main()
    {
        AddCookie();
        Console.WriteLine($"I have {myCookiesCount} ({(int)myCookiesCount}) cookies");
    }

    private static void AddCookie()
    {
        myCookiesCount++;
        Console.WriteLine(
            $"Added 1 cookie, " +
            $"was - {myCookiesCount - 1} ({(int)myCookiesCount - 1}), " +
            $"now - {myCookiesCount} ({(int)myCookiesCount})"
            );
    }
}
