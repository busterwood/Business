using BusterWood.Collections;
using BusterWood.Contracts;
using System.Collections;
using System.Collections.Generic;

namespace BusterWood.Business
{
    class DataModelReader : IEnumerable<Table>
    {
        public const string DataModel = "data model";
        private readonly IEnumerable<Line> _lines;

        public DataModelReader(IEnumerable<Line> lines)
        {
            Contract.RequiresNotNull(lines);
            _lines = lines;
        }

        public IEnumerator<Table> GetEnumerator()
        {
            var lines = new LookAheadEnumerator<Line>(_lines.GetEnumerator());

            bool got = lines.MoveNext();
            if (!got)
                throw new ParseException("Unexpected end of file when expecting a data model declaration");

            var dm = lines.Current as Identifier;
            if (dm == null || !dm.Is(DataModel))
                throw new ParseException($"Expected data model declaration but got {lines.Current}");

            Table table = null;
            for(;;)
            {
                got = lines.MoveNext();
                if (!got)
                    break;

                if (lines.Current is Identifier)
                {
                    if (table != null)
                        yield return table;
                    table = new Table(lines.Current);
                }
                else
                {
                    if (table == null)
                       throw new ParseException($"Expected table declaration but got {lines.Current}");
                    table.Fields.Add(new Field(lines.Current));
                }

                if (lines.Next is Identifier i && i.Is(ProcessModelReader.ProcessModel))
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
