using System.Collections.Generic;
using System.Security;
using System.Text;

namespace OrchardCore.Environment.Commands.Parameters
{
    public interface ICommandParser
    {
        IEnumerable<string> Parse(string commandLine);
    }

    public class CommandParser : ICommandParser
    {
        [SecurityCritical]
        public IEnumerable<string> Parse(string commandLine)
        {
            return SplitArgs(commandLine);
        }

        public class State
        {
            private readonly string _commandLine;
            private readonly StringBuilder _stringBuilder;
            private readonly List<string> _arguments;
            private int _index;

            public State(string commandLine)
            {
                _commandLine = commandLine;
                _stringBuilder = new StringBuilder();
                _arguments = new List<string>();
            }

            public StringBuilder StringBuilder { get { return _stringBuilder; } }
            public bool EOF { get { return _index >= _commandLine.Length; } }
            public char Current { get { return _commandLine[_index]; } }
            public IEnumerable<string> Arguments { get { return _arguments; } }

            public void AddArgument()
            {
                _arguments.Add(StringBuilder.ToString());
                StringBuilder.Clear();
            }

            public void AppendCurrent()
            {
                StringBuilder.Append(Current);
            }

            public void Append(char ch)
            {
                StringBuilder.Append(ch);
            }

            public void MoveNext()
            {
                if (!EOF)
                    _index++;
            }
        }

        /// <summary>
        /// Implement the same logic as found at
        /// http://msdn.microsoft.com/en-us/library/17w5ykft.aspx
        /// The 3 special characters are quote, backslash and whitespaces, in order
        /// of priority.
        /// The semantics of a quote is: whatever the state of the lexer, copy
        /// all characters verbatim until the next quote or EOF.
        /// The semantics of backslash is: If the next character is a backslash or a quote,
        /// copy the next character. Otherwise, copy the backslash and the next character.
        /// The semantics of whitespace is: end the current argument and move on to the next one.
        /// </summary>
        private static IEnumerable<string> SplitArgs(string commandLine)
        {
            var state = new State(commandLine);
            while (!state.EOF)
            {
                switch (state.Current)
                {
                    case '"':
                        ProcessQuote(state);
                        break;

                    case '\\':
                        ProcessBackslash(state);
                        break;

                    case ' ':
                    case '\t':
                        if (state.StringBuilder.Length > 0)
                            state.AddArgument();
                        state.MoveNext();
                        break;

                    default:
                        state.AppendCurrent();
                        state.MoveNext();
                        break;
                }
            }
            if (state.StringBuilder.Length > 0)
                state.AddArgument();
            return state.Arguments;
        }

        private static void ProcessQuote(State state)
        {
            state.MoveNext();
            while (!state.EOF)
            {
                if (state.Current == '"')
                {
                    state.MoveNext();
                    break;
                }
                state.AppendCurrent();
                state.MoveNext();
            }

            state.AddArgument();
        }

        private static void ProcessBackslash(State state)
        {
            state.MoveNext();
            if (state.EOF)
            {
                state.Append('\\');
                return;
            }

            if (state.Current == '"')
            {
                state.Append('"');
                state.MoveNext();
            }
            else
            {
                state.Append('\\');
                state.AppendCurrent();
                state.MoveNext();
            }
        }
    }
}
