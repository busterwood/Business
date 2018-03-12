using BusterWood.Testing;
using System;
using System.IO;
using System.Linq;

namespace BusterWood.Business
{
    public class LexerTests
    {
        public static void can_read_identitifer(Test t)
        {
            foreach (var input in new[] { "data model:", "data model:\r\n", " data model:\n", "   data model:   ", "\tdata model:\t" } )
            {
                var reader = new StringReader(input);
                var lex = new LineLexer(reader);
                var tokens = lex.ToList();

                if (tokens.Count != 1)
                {
                    t.Error($"Expected 1 token but got {tokens.Count} for input '{input}'");
                    continue;
                }

                var first = tokens[0];
                if (!(first is Identifier))
                {
                    t.Error($"Expected first token to be a {nameof(Identifier)} but was a {first?.GetType()} for input '{input}'");
                    continue;
                }

                var id = (Identifier)first;
                const string expected = "data model";
                if (id.Text != expected)
                    t.Error($"Expected '{expected}' but got '{id.Text}' for input '{input}'");
            }
        }


        public static void can_read_lines(Test t)
        {
            foreach (var input in new[] { "name", "name\r\n", " name\n", "   name   ", "\tname\t" })
            {
                TestLine(t, input);
            }
        }

        public static void can_skip_empty_lines(Test t)
        {
            foreach (var input in new[] { "\r\nname", "\r\nname\r\n", " \r\nname\n", "\r\n   name   \r\n\r\n", "\tname\t\r\n\r\n\r\n" })
            {
                TestLine(t, input);
            }
        }

        public static void can_skip_comments_on_separate_lines(Test t)
        {
            foreach (var input in new[] { "name\n//fred", "name\r\n//fred", "    name   \r\n\r\n    //fred\r\n" })
            {
                TestLine(t, input);
            }
        }

        public static void can_skip_comments_on_end_of_line(Test t)
        {
            foreach (var input in new[] { "name  //fred", "name\t//fred"})
            {
                TestLine(t, input);
            }
        }

        static void TestLine(Test t, string input, string expected = "name")
        {
            var reader = new StringReader(input);
            var lex = new LineLexer(reader);
            var tokens = lex.ToList();

            if (tokens.Count != 1)
            {
                t.Error($"Expected 1 token but got {tokens.Count} for input '{input}'");
                return;
            }

            var first = tokens[0];
            if (first.GetType() != typeof(Line))
            {
                t.Error($"Expected first token to be a {nameof(Line)} but was a {first?.GetType()} for input '{input}'");
                return;
            }

            if (first.Text != expected)
            {
                t.Error($"Expected '{expected}' but got '{first.Text}' for input '{input}'");
            }
        }
    }
}
