using BusterWood.Parsing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using static System.StringComparison;

namespace BusterWood.Business
{
    public class LanguageLexer : IEnumerable<Token>
    {
        LookAheadEnumerator<char> input;
        int line;
        int column;
        StringBuilder sb;

        public LanguageLexer(IEnumerator<char> input)
        {
            this.input = new LookAheadEnumerator<char>(input);
            sb = new StringBuilder();
        }

        public IEnumerator<Token> GetEnumerator()
        {
            line = 1;
            column = 1;
            if (!input.MoveNext())
                yield break;    // empty file
            for (; ;)
            {
                var token = ReadWord();
                if (token == null)
                    break;
                yield return token;
            }            
        }
        
        private Token ReadWord()
        {
            int startLine = line;
            int startCol = column;
            sb.Clear();
            SkipWhiteSpace();
            while (char.IsLetterOrDigit(input.Current))
            {
                sb.Append(input.Current);
                if (!MoveNext())
                    break;
            }
            return sb.Length == 0 ? null : new Token(sb.ToString(), startLine, startCol);
        }

        private void SkipWhiteSpace()
        {
            while (char.IsWhiteSpace(input.Current))
            {
                if (!MoveNext())
                    return;
            }
        }

        private bool MoveNext()
        {
            bool moved = input.MoveNext();
            if (moved)
                IncrementLineAndColumn();
            return moved;
        }

        private void IncrementLineAndColumn()
        {
            if (input.Current == '\n')
            {
                line++;
                column = 1;
            }
            else if (!char.IsControl(input.Current))
                column++;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Token
    {
        public string Text { get; }
        public int Line { get; }
        public int Column { get; }
        public TokenType Type { get; }

        public Token(string text, int line, int column)
        {
            Text = text;
            Line = line;
            Column = column;
            Type = Classify();
        }

        public TokenType Classify()
        {
            if (nameof(TokenType.Stop).Equals(Text, OrdinalIgnoreCase)) return TokenType.Stop;
            else if (nameof(TokenType.When).Equals(Text, OrdinalIgnoreCase)) return TokenType.When;
            else if (nameof(TokenType.Do).Equals(Text, OrdinalIgnoreCase)) return TokenType.Do;
            else if (nameof(TokenType.While).Equals(Text, OrdinalIgnoreCase)) return TokenType.While;
            else if (nameof(TokenType.For).Equals(Text, OrdinalIgnoreCase)) return TokenType.For;
            else if (nameof(TokenType.End).Equals(Text, OrdinalIgnoreCase)) return TokenType.End;
            else return TokenType.Identifier;
        }
    }

    public enum TokenType
    {
        Identifier,
        Stop,
        When,
        Do,
        While,
        For,
        End,
    }
}
