using BusterWood.Collections;
using System.Collections;
using System.Collections.Generic;
using BusterWood.Contracts;
using System;
using BusterWood.Linq;
using System.Linq;

namespace BusterWood.Business
{
    class ProcessModelReader : IEnumerable<BusinessProcess>
    {
        public const string ModelName = "process";

        private readonly IEnumerator<Line> _lines;

        public ProcessModelReader(IEnumerable<Line> lines) : this(lines.GetEnumerator()) { }

        public ProcessModelReader(IEnumerator<Line> lines)
        {
            Contract.RequiresNotNull(lines);
            _lines = lines;
        }

        public IEnumerator<BusinessProcess> GetEnumerator()
        {
            bool got = _lines.MoveNext();
            if (!got)
                throw new ParseException("Unexpected end of file when expecting a process model declaration");

            var pm = _lines.Current as Identifier;
            if (pm == null || !pm.Is(ModelName))
                throw new ParseException($"Expected process model declaration but got {_lines.Current}");

            BusinessProcess process = null;
            for(;;)
            {
                got = _lines.MoveNext();
                if (!got)
                    break;

                if (_lines.Current is Identifier)
                {
                    if (process != null)
                        yield return process;
                    process = new BusinessProcess(_lines.Current);
                }
                else
                {
                    if (process == null)
                       throw new ParseException($"Expected table declaration but got {_lines.Current}");

                    var given = ParseGiven(_lines.Current);
                    if (given != null)
                    {
                        if (process.Given != null)
                            throw new ParseException($"You can only one given per process at the moment {_lines.Current}");
                        process.Given = given;
                        continue;
                    }

                    process.Steps.Add(new Step(_lines.Current));
                }
            }

            if (process != null)
                yield return process;
        }

        private Given ParseGiven(Line line)
        {
            //parse "Given a thing that xyz"

            var bits = line.Text.SplitOn(char.IsWhiteSpace).Select(chars => new string(chars.ToArray()).Trim()).ToList();
            if (!bits[0].Equals("given", StringComparison.OrdinalIgnoreCase))
                return null;
            bits.RemoveAt(0);

            Multiplicity many = Multiplicity.Zero;
            string word = bits.FirstOrDefault();
            if (string.Equals(word, "a", StringComparison.OrdinalIgnoreCase) || string.Equals(word, "an", StringComparison.OrdinalIgnoreCase))
            {
                many = Multiplicity.One;
                bits.RemoveAt(0);
            }
            else if (string.Equals(word, "some", StringComparison.OrdinalIgnoreCase) || string.Equals(word, "many", StringComparison.OrdinalIgnoreCase))
            {
                many = Multiplicity.OneOrMore;
                bits.RemoveAt(0);
            }

            string what;
            var thatIdx = bits.FindIndex(s => string.Equals("that", s, StringComparison.OrdinalIgnoreCase));
            if (thatIdx > 0)
                what = string.Join(" ", bits.Take(thatIdx));
            else
                what = string.Join(" ", bits);

            string cond = thatIdx > 0 ? string.Join(" ", bits.Skip(thatIdx+1)) : "";

            return new Given(line) { Many=many, What=what, Condition=cond };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class BusinessProcess : Line
    {
        internal BusinessProcess(Line l) : base(l.Text, l.LineNumber) { }

        public BusinessProcess(string text, int line) : base(text, line)
        {
        }

        public UniqueList<Step> Steps { get; } = new UniqueList<Step>();
        public Given Given { get; internal set; }
    }

    public class Given : Line
    {
        internal Given(Line l) : base(l.Text, l.LineNumber) { }

        public Given(string text, int line) : base(text, line)
        {
        }

        public Multiplicity Many { get; set; }
        public string What { get; set; }
        public string Condition { get; set; }
    }

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

    public class Step : Line
    {
        internal Step(Line l) : base(l.Text, l.LineNumber) { }

        public Step(string text, int line) : base(text, line)
        {
        }
    }


}
