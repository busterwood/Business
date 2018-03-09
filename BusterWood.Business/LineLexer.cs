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
                var line = reader.ReadLine()?.Trim();
                if (line == null)
                    break;

                lineNumber++;

                if (line.Length == 0) // empty line
                    continue;

                if (line.StartsWith("//")) // comment
                    continue;

                if (line.EndsWith(":"))
                    yield return new Identifier(line.TrimEnd(':'), lineNumber);
                else
                    yield return new Declaration(line, lineNumber);
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

    public class Declaration : Line
    {
        public Declaration(string text, int line) : base(text, line)
        {
        }
    }

}
