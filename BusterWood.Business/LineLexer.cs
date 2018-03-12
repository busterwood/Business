using System.Collections.Generic;
using System.Collections;
using System.IO;
using BusterWood.Contracts;

namespace BusterWood.Business
{
    public class LineLexer : IEnumerable<Line>
    {
        readonly TextReader reader;

        public LineLexer(TextReader reader)
        {
            Contract.Assert(reader != null);
            this.reader = reader;
        }

        public IEnumerator<Line> GetEnumerator()
        {
            int lineNumber = 0;
            for (;;)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;

                lineNumber++;

                var commentStart = line.IndexOf("//");
                if (commentStart >= 0)
                    line = line.Substring(0, commentStart);

                line = line.Trim();

                if (line.Length == 0) // empty line
                    continue;

                if (line.EndsWith(":"))
                    yield return new Identifier(line.TrimEnd(':'), lineNumber);
                else
                    yield return new Statement(line, lineNumber);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Line
    {
        public string Text { get; }
        public int LineNumber { get; }

        public Line(string text, int line)
        {
            Text = text;
            LineNumber = line;
        }

        public override string ToString() => Text;
    }

    public class Identifier : Line
    {
        public Identifier(string text, int line) : base(text, line)
        {
        }
    }

    public class Statement : Line
    {
        public Statement(string text, int line) : base(text, line)
        {
        }
    }

}
