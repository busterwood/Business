using BusterWood.Collections;
using BusterWood.Contracts;
using BusterWood.Goodies;
using BusterWood.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.StringComparison;

namespace BusterWood.Business
{
    class DataModelReader : IEnumerable<Table>
    {
        public const string ModelName = "data model";
        private readonly LookAheadEnumerator<Line> _lines;

        public DataModelReader(IEnumerable<Line> lines) : this(new LookAheadEnumerator<Line>(lines.GetEnumerator())) { }

        public DataModelReader(LookAheadEnumerator<Line> lines)
        {
            Contract.RequiresNotNull(lines);
            _lines = lines;
        }

        public IEnumerator<Table> GetEnumerator()
        {
            bool got = _lines.MoveNext();
            if (!got)
                throw new ParseException("Unexpected end of file when expecting a data model declaration");

            var dm = _lines.Current as Identifier;
            if (dm == null || !dm.Is(ModelName))
                throw new ParseException($"Expected data model declaration but got {_lines.Current}");

            Table table = null;
            for(;;)
            {
                got = _lines.MoveNext();
                if (!got)
                    break;

                if (_lines.Current is Identifier)
                {
                    if (table != null)
                        yield return table;
                    table = new Table(_lines.Current);
                }
                else
                {
                    if (table == null)
                       throw new ParseException($"Expected table declaration but got {_lines.Current}");

                    //TODO: parse relationship, e.g. "has one or more orders"
                    var f = ParseField(_lines.Current);
                    table.Fields.Add(f);
                }

                if (_lines.Next is Identifier i && i.Is(ProcessModelReader.ModelName))
                    break;
            }

            if (table != null)
                yield return table;
        }

        private Field ParseField(Line line)
        {
            var bits = line.Text
                .SplitOn(char.IsWhiteSpace)
                .Select(chars => chars.Where(c => !c.IsWhiteSpace()))
                .Where(chars => chars.Any())
                .Select(chars => new string(chars.ToArray()))
                .ToList();

            if (bits[0].Equals("has", OrdinalIgnoreCase))
                return Has(line, bits);

            var text = line.Text;
            var open = text.IndexOf('(');
            if (open > 0)
            {
                var closed = text.IndexOf(')', open);
                if (closed < 0)
                    throw new ParseException("Open bracket without matching closing bracket on line " + line.LineNumber);

                var type = text.Substring(open + 1, closed - open - 1);
                var name = text.Substring(0, open).Trim();
                return new Field(line) { Type=type, Name= name};
            }

            return new Field(line);
        }

        private Field Has(Line line, List<string> bits)
        {
            var e = new LookAheadEnumerator<string>(bits.GetEnumerator());
            e.MoveNext(); // move to has
            e.MoveNext(); // skip to next
            var many = MultiplicityParser.Parse(e, line);
            string what = ParseWhat(line, e);
            return new Relationship(line) { Many = many, What = what };
        }

        private static string ParseWhat(Line line, LookAheadEnumerator<string> e)
        {
            var sb = new StringBuilder();
            while (e.MoveNext())
                sb.Append(e.Current).Append(" ");
            if (sb.Length > 0)
                sb.Length -= 1;
            var what = sb.ToString();
            if (string.IsNullOrWhiteSpace(what))
                throw new ParseException("Expected the related thing name on " + line);
            return what;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Table : Line
    {
        internal Table(Line l) : base(l.Text, l.LineNumber) { }

        public Table(string text, int line) : base(text, line)
        {
        }

        public UniqueList<Field> Fields { get; } = new UniqueList<Field>();
    }

    public class Field : Line
    {
        public string Type { get; set; }
        public string Name { get; set; }

        internal Field(Line l) : this(l.Text, l.LineNumber) { }

        public Field(string text, int line) : base(text, line)
        {
            Name = text;
            Type = "string";
        }
    }

    public class Relationship : Field
    {
        public Multiplicity Many { get; set; }
        public string What { get; set; }

        internal Relationship(Line l) : this(l.Text, l.LineNumber) { }

        public Relationship(string text, int line) : base(text, line)
        {
        }
    }
}
