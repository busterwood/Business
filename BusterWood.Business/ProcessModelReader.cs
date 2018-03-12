using BusterWood.Collections;
using System.Collections;
using System.Collections.Generic;
using BusterWood.Contracts;

namespace BusterWood.Business
{
    class ProcessModelReader : IEnumerable<BusinessProcess>
    {
        public const string ProcessModel = "process";

        private readonly IEnumerable<Line> _lines;

        public ProcessModelReader(IEnumerable<Line> lines)
        {
            Contract.RequiresNotNull(lines);
            _lines = lines;
        }

        public IEnumerator<BusinessProcess> GetEnumerator()
        {
            var lines = new LookAheadEnumerator<Line>(_lines.GetEnumerator());

            bool got = lines.MoveNext();
            if (!got)
                throw new ParseException("Unexpected end of file when expecting a process model declaration");

            var pm = lines.Current as Identifier;
            if (pm == null || !pm.Is(ProcessModel))
                throw new ParseException($"Expected process model declaration but got {lines.Current}");

            BusinessProcess process = null;
            for(;;)
            {
                got = lines.MoveNext();
                if (!got)
                    break;

                if (lines.Current is Identifier)
                {
                    if (process != null)
                        yield return process;
                    process = new BusinessProcess(lines.Current);
                }
                else
                {
                    if (process == null)
                       throw new ParseException($"Expected table declaration but got {lines.Current}");
                    process.Statements.Add(new Statement(lines.Current));
                }
            }

            if (process != null)
                yield return process;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class BusinessProcess : Line
    {
        internal BusinessProcess(Line l) : base(l.Text, l.LineNumber) { }

        public BusinessProcess(string text, int line) : base(text, line)
        {
        }

        public UniqueList<Statement> Statements { get; } = new UniqueList<Statement>();
    }

    public class Statement : Line
    {
        internal Statement(Line l) : base(l.Text, l.LineNumber) { }

        public Statement(string text, int line) : base(text, line)
        {
        }
    }


}
