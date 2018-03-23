using System.Collections.Generic;
using System.Collections;
using System.IO;
using BusterWood.Contracts;
using System.Text;
using BusterWood.Goodies;
using BusterWood.Linq;
using System.Linq;

namespace BusterWood.Business
{
    public class Scanner : IEnumerable<Line>
    {
        readonly TextReader reader;

        public Scanner(TextReader reader)
        {
            Contract.RequiresNotNull(reader);
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
                    yield return new Line(line, lineNumber);
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

        public override string ToString() => Text + " on line " + LineNumber;

        public bool Is(string other) => string.Equals(Text, other, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>A name of a data table or business logic script</summary>
    public class Identifier : Line
    {
        public Identifier(string text, int line) : base(text, line)
        {
        }
    }


    public static class Extensions
    {
        public static string ClrName(this string text)
        {
            var sb = new StringBuilder();
            foreach (var w in text.SplitOn(char.IsWhiteSpace))
            {
                var nospace = w.Where(c => !char.IsWhiteSpace(c));
                sb.Append(nospace.First().ToUpper());
                foreach (var c in nospace.Skip(1))
                {
                    sb.Append(c.ToLower());
                }
            }
            return sb.ToString();
        }

        public static string ClrName(this Line l) => l.Text.ClrName();
    }
}
