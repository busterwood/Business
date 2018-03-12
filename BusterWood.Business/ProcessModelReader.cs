using BusterWood.Collections;
using System.Collections;
using System.Collections.Generic;
using BusterWood.Contracts;

namespace BusterWood.Business
{
    class ProcessModelReader : IEnumerable<BusinessProcess>
    {
        public const string ModelName = "process";

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
            if (pm == null || !pm.Is(ModelName))
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
                    process.Steps.Add(new Step(lines.Current));
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

        public UniqueList<Step> Steps { get; } = new UniqueList<Step>();
    }

    public class Step : Line
    {
        internal Step(Line l) : base(l.Text, l.LineNumber) { }

        public Step(string text, int line) : base(text, line)
        {
        }
    }


}
