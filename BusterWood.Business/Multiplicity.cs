using BusterWood.Collections;
using System;
using static System.StringComparison;

namespace BusterWood.Business
{

    [Flags]
    public enum Multiplicity
    {
        Zero = 1,
        One = 2,
        Many = 4,
        ZeroOrOne = Zero | One,
        ZeroOrMore = Zero | Many,
        OneOrMore = One | Many,
    }

    static class MultiplicityParser
    {
        public static Multiplicity Parse(LookAheadEnumerator<string> e, Line line)
        {
            Multiplicity many = ParseZeroOrOne(e, line);

            if (string.Equals("or", e.Next, OrdinalIgnoreCase))
            {
                e.MoveNext();
                if (!e.MoveNext())
                    throw new ParseException("Expected the 'more' after 'or' " + line);

                if (ParseMany(e, line))
                    many |= Multiplicity.Many;
            }

            return many;
        }

        private static Multiplicity ParseZeroOrOne(LookAheadEnumerator<string> e, Line line)
        {
            if (string.Equals("zero", e.Current, OrdinalIgnoreCase))
                return Multiplicity.Zero;
            else if (string.Equals("one", e.Current, OrdinalIgnoreCase))
                return Multiplicity.One;
            else
                throw new ParseException("Expected the zero or one after 'has ' " + line);
        }

        private static bool ParseMany(LookAheadEnumerator<string> e, Line line)
        {
            if (string.Equals("more", e.Current, OrdinalIgnoreCase))
                return true;
            else
                throw new ParseException("Expected the 'more' after 'or' " + line);
        }
    }
}
