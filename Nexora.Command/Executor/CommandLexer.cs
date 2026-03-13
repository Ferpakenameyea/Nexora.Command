using System.Text;

namespace Nexora.Command.Executor;

internal class CommandLexer(string input)
{
    private readonly string _input = input;
    private int _position = 0;
    public bool AtEnd => _position >= _input.Length;

    private void SkipSpaceForward()
    {
        while(_position < _input.Length && _input[_position] == ' ')
        {
            _position++;
        }
    }

    private void SkipSpaceBackward()
    {
        while(_position > 0 && _input[_position - 1] == ' ')
        {
            _position--;   
        }
    }

    public string? TryNextLiteral(string expect)
    {
        if (expect == "")
        {
            throw new InvalidOperationException("You cannot match an empty literal");
        }

        SkipSpaceForward();
        if (_position + expect.Length > _input.Length)
        {
            return null;
        }
        for (int i = 0; i < expect.Length; i++)
        {
            if (_input[_position + i] != expect[i])
            {
                return null;
            }
        }

        // a literal token should be isolated with space
        if (_position + expect.Length < _input.Length && _input[_position + expect.Length] != ' ')
        {
            return null;
        }

        _position += expect.Length;
        return expect;
    }

    public string? TryNextString()
    {
        SkipSpaceForward();
        int savedPosition = _position;
        if (_position == _input.Length)
        {
            return null;
        }
        StringBuilder stringBuilder = new();
        bool enclosed = _input[_position] == '\"';

        if (enclosed)
        {
            // skip the first quote
            _position++;
            while (_position < _input.Length)
            {
                if (_input[_position] == '\"')
                {
                    enclosed = false;
                    _position++;
                    break;
                }

                // escaping
                if (_input[_position] == '\\')
                {
                    if (_position + 1 >= _input.Length)
                    {
                        // invalid string
                        _position = savedPosition;
                        return null;
                    }

                    char next = _input[_position + 1];
                    char? escapedValue = next switch
                    {
                        '\"' => '\"',
                        '\\' => '\\',
                        'n' => '\n',
                        _ => null
                    };

                    if (escapedValue == null)
                    {
                        _position = savedPosition;
                        return null;
                    }

                    stringBuilder.Append(escapedValue.Value);
                    _position += 2;
                    continue;
                }

                stringBuilder.Append(_input[_position]);
                _position++;
            }
        }
        else
        {
            while (_position < _input.Length && _input[_position] != ' ')
            {
                stringBuilder.Append(_input[_position]);
                _position++;
            }
        }

        // the string has an open quote
        if (enclosed)
        {
            _position = savedPosition;
            return null;
        }

        return stringBuilder.ToString();
    }

    public string? TryNextNumber()
    {
        SkipSpaceForward();
        if (_position >= _input.Length)
        {
            return null;
        }

        int startPosition = _position;
        StringBuilder builder = new();
        bool dot = false;
        int dotPosition = -1;

        while (_position < _input.Length)
        {
            if (char.IsDigit(_input[_position]))
            {
                builder.Append(_input[_position]);
            }
            else if (_input[_position] == ' ')
            {
                break;
            }
            else if (_input[_position] == '.')
            {
                if (dot || builder.Length == 0)
                {
                    _position = startPosition;
                    return null;
                }

                dotPosition = _position;
                dot = true;
                builder.Append('.');
            }
            else
            {
                _position = startPosition;
                return null;
            }

            _position++;
        }

        if (dot && _position - 1 == dotPosition)
        {
            _position = startPosition;
            return null;
        }
        return builder.ToString();
    }

    public string? TryNextGreedyString()
    {
        if (_position == _input.Length)
        {
            return null;
        }

        SkipSpaceForward();

        string value = _input[_position..];
        _position = _input.Length;
        return value;
    }

    public void ReturnGreedyString(string value)
    {
        if (_position != _input.Length)
        {
            throw new InvalidOperationException(
                "Returning a Greedy string to lexer but it is not at the end. " +
                "Greedy string can only be at end.");
        }
        _position -= value.Length;
    }

    public void ReturnContinuousToken(string value)
    {
        SkipSpaceBackward();
        _position -= value.Length;
        if (_position < 0)
        {
            throw new InvalidOperationException(
                "You have returned a value too long for the lexer. Are you sure the value " +
                "that you've passed is actually from this lexer?");
        }
    }

    public void ReturnString(string value)
    {
        SkipSpaceBackward();
        if (_position <= 0)
        {
            throw new InvalidOperationException(
                "You have returned a value too long for the lexer. Are you sure the value " +
                "that you've passed is actually from this lexer?");
        }

        if (_input[_position - 1] == '\"')
        {
            _position -= 2 + value.Length;
            return;
        }

        _position -= value.Length;

        if (_position < 0)
        {
            throw new InvalidOperationException(
                "You have returned a value too long for the lexer. Are you sure the value " +
                "that you've passed is actually from this lexer?");
        }
    }
}