using BusterWood.Collections;
using BusterWood.Contracts;
using System.Collections;
using System.Collections.Generic;

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

                    table.Fields.Add(new Field(_lines.Current));
                }

                if (_lines.Next is Identifier i && i.Is(ProcessModelReader.ModelName))
                    break;
            }

            if (table != null)
                yield return table;
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
        internal Field(Line l) : base(l.Text, l.LineNumber) { }

        public Field(string text, int line) : base(text, line)
        {
        }
    }


}
