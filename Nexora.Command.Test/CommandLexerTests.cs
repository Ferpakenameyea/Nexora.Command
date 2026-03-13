using Nexora.Command.Executor;

namespace Nexora.Command.Test;

public class CommandLexerTests
{

    [Test]
    public void TryStringAtTheEnd_ShouldReturnNull()
    {
        CommandLexer lexer = new("");
        string? s = lexer.TryNextString();
        Assert.That(s, Is.Null);
    }

    [Test]
    public void TryGreedyStringAtTheEnd_ShouldReturnNull()
    {
        CommandLexer lexer = new("");
        string? s = lexer.TryNextGreedyString();
        Assert.That(s, Is.Null);
    }

    [Test]
    public void TryStringWithoutQuote_ShouldSuccess()
    {
        CommandLexer lexer = new("somestring somestring2");
        string? s1 = lexer.TryNextString();
        string? s2 = lexer.TryNextString();

        Assert.Multiple(() =>
        {
            Assert.That(s1, Is.Not.Null);
            Assert.That(s2, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(s1, Is.EqualTo("somestring"));
            Assert.That(s2, Is.EqualTo("somestring2"));
        });
    }

    [Test]
    public void TryStringWithQuote_ShouldIgnoreQuote()
    {
        CommandLexer lexer = new("\"somestring\"");
        string? s = lexer.TryNextString();

        Assert.That(s, Is.Not.Null);
        Assert.That(s, Is.EqualTo("somestring"));
    }

    [Test]
    public void TryStringWithQuote_ShouldNotBeSplitBySpace()
    {
        CommandLexer lexer = new("\"some string\"");
        string? s = lexer.TryNextString();
        
        Assert.That(s, Is.Not.Null);
        Assert.That(s, Is.EqualTo("some string"));
    }

    [Test]
    public void TryStringWithQuote_ShouldEscapeCorrectly()
    {
        // raw input: "some string \" \\ \n"
        CommandLexer lexer = new("\"some string \\\" \\\\ \\n\"");
        string? s = lexer.TryNextString();
        
        Assert.That(s, Is.Not.Null);
        Assert.That(s, Is.EqualTo("some string \" \\ \n"));
    }

    [Test]
    public void StringWithNotClosedQuote_ShouldReturnNull()
    {
        CommandLexer lexer = new("\"somestring");
        string? s = lexer.TryNextString();
        
        Assert.That(s, Is.Null);
    }

    [Test]
    public void StringWithInvalidEscape_ShouldReturnNull()
    {
        CommandLexer lexer = new("\"somestring \\l");
        string? s = lexer.TryNextString();
        
        Assert.That(s, Is.Null);
    }

    [Test]
    public void StringWithIncompleteEscape_ShouldReturnNull()
    {
        CommandLexer lexer = new("\"somestring \\");
        string? s = lexer.TryNextString();
        
        Assert.That(s, Is.Null);
    }

    [Test]
    public void TryMatchLiteral_ShouldSuccess()
    {
        CommandLexer lexer = new("literal1 literal2");
        string? s1 = lexer.TryNextLiteral("literal1");
        string? s2 = lexer.TryNextLiteral("literal2");

        Assert.Multiple(() =>
        {
            Assert.That(s1, Is.EqualTo("literal1"));
            Assert.That(s2, Is.EqualTo("literal2"));
        });
    }

    [Test]
    public void TryMatchUnmatchedLiteral_ShouldReturnNull()
    {
        CommandLexer lexer = new("abc abc");
        string? s1 = lexer.TryNextLiteral("cba");
        string? s2 = lexer.TryNextLiteral("cba");

        Assert.Multiple(() =>
        {
            Assert.That(s1, Is.Null);
            Assert.That(s2, Is.Null);
        });
    }

    [Test]
    public void MatchLiteralFailed_ShouldReturnToStart()
    {
        CommandLexer lexer = new("abc");
        string? beNull = lexer.TryNextLiteral("cba");
        string? notNull = lexer.TryNextLiteral("abc");

        Assert.Multiple(() =>
{
            Assert.That(beNull, Is.Null);
            Assert.That(notNull, Is.Not.Null);
        });
    }

    [Test]
    public void MatchLiteralTooLong_ShouldFail()
    {
        CommandLexer lexer = new("ab");
        string? node = lexer.TryNextLiteral("tooolong");
        Assert.That(node, Is.Null);
    }


    [Test]
    public void TryNextLiteral_WithLettersAfter_ShouldFail()
    {
        CommandLexer lexer = new("abcdef");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithNumbersAfter_ShouldFail()
    {
        CommandLexer lexer = new("abc123");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithSpecialCharAfter_ShouldFail()
    {
        CommandLexer lexer = new("abc!");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithDotAfter_ShouldFail()
    {
        CommandLexer lexer = new("abc.xyz");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithUnderscoreAfter_ShouldFail()
    {
        CommandLexer lexer = new("abc_def");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithAtSignAfter_ShouldFail()
    {
        CommandLexer lexer = new("tell@user");
        string? literal = lexer.TryNextLiteral("tell");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithHyphenAfter_ShouldFail()
    {
        CommandLexer lexer = new("abc-def");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithColonAfter_ShouldFail()
    {
        CommandLexer lexer = new("abc:def");
        string? literal = lexer.TryNextLiteral("abc");

        // "abc" 后面紧跟 ":"，不是空格或末尾，应该失败
        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithSlashAfter_ShouldFail()
    {
        CommandLexer lexer = new("abc/def");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithBackslashAfter_ShouldFail()
    {
        CommandLexer lexer = new("abc\\def");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithCommaAfter_ShouldFail()
    {
        CommandLexer lexer = new("abc,def");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithSpaceAfter_ShouldSuccess()
    {
        CommandLexer lexer = new("abc def");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.EqualTo("abc"));
    }

    [Test]
    public void TryNextLiteral_AtEnd_ShouldSuccess()
    {
        CommandLexer lexer = new("abc");
        string? literal = lexer.TryNextLiteral("abc");

        // "abc" 在末尾，应该成功
        Assert.That(literal, Is.EqualTo("abc"));
    }

    [Test]
    public void TryNextLiteral_MultipleWithSpacesBetween_ShouldSuccess()
    {
        CommandLexer lexer = new("cmd1 cmd2 cmd3");
        string? l1 = lexer.TryNextLiteral("cmd1");
        string? l2 = lexer.TryNextLiteral("cmd2");
        string? l3 = lexer.TryNextLiteral("cmd3");

        Assert.Multiple(() =>
        {
            Assert.That(l1, Is.EqualTo("cmd1"));
            Assert.That(l2, Is.EqualTo("cmd2"));
            Assert.That(l3, Is.EqualTo("cmd3"));
        });
    }

    [Test]
    public void TryNextLiteral_MultipleSpacesAfter_ShouldSuccess()
    {
        CommandLexer lexer = new("abc   def");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.EqualTo("abc"));
    }

    [Test]
    public void TryNextLiteral_WithTabAfter_ShouldFail()
    {
        CommandLexer lexer = new("abc\tdef");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_WithNewlineAfter_ShouldFail()
    {
        CommandLexer lexer = new("abc\ndef");
        string? literal = lexer.TryNextLiteral("abc");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_PrefixMatch_ShouldFail()
    {
        CommandLexer lexer = new("abc123");
        string? literal = lexer.TryNextLiteral("a");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_PrefixMatchAtEnd_ShouldSuccess()
    {
        CommandLexer lexer = new("a");
        string? literal = lexer.TryNextLiteral("a");

        Assert.That(literal, Is.EqualTo("a"));
    }

    [Test]
    public void TryNextLiteral_FailedThenTryString_ShouldParseWord()
    {
        CommandLexer lexer = new("abcdef");
        string? literal = lexer.TryNextLiteral("abc");
        string? s = lexer.TryNextString();

        Assert.Multiple(() =>
        {
            Assert.That(literal, Is.Null);
            Assert.That(s, Is.EqualTo("abcdef"));
        });
    }

    [Test]
    public void TryNextLiteral_AtSign_ShouldFailWithoutSpaceAfter()
    {
        CommandLexer lexer = new("@user123");
        string? literal = lexer.TryNextLiteral("@user");

        // "@user" 后面紧跟 "1"，不是空格或末尾，应该失败
        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_AtSign_WithSpaceAfter_ShouldSuccess()
    {
        CommandLexer lexer = new("@user message");
        string? literal = lexer.TryNextLiteral("@user");

        // "@user" 后面紧跟空格，应该成功
        Assert.That(literal, Is.EqualTo("@user"));
    }

    [Test]
    public void TryNextLiteral_MixedCaseAfter_ShouldFail()
    {
        CommandLexer lexer = new("cmdXyz");
        string? literal = lexer.TryNextLiteral("cmd");

        // "cmd" 后面紧跟 "X"，不是空格或末尾，应该失败
        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_DuplicatePattern_ShouldMatchFirst()
    {
        CommandLexer lexer = new("abc abc");
        string? l1 = lexer.TryNextLiteral("abc");
        string? l2 = lexer.TryNextLiteral("abc");

        Assert.Multiple(() =>
        {
            Assert.That(l1, Is.EqualTo("abc"));
            Assert.That(l2, Is.EqualTo("abc"));
        });
    }

    [Test]
    public void TryNextLiteral_EmptyLiteral_ShouldAlwaysReturnLiteral()
    {
        CommandLexer lexer = new("anything");

        Assert.Throws<InvalidOperationException>(() =>
        {
            string? literal = lexer.TryNextLiteral("");
            Assert.That(literal, Is.EqualTo(""));
        });
    }

    [Test]
    public void TryNextLiteral_SingleCharWithCharAfter_ShouldFail()
    {
        CommandLexer lexer = new("ab");
        string? literal = lexer.TryNextLiteral("a");

        Assert.That(literal, Is.Null);
    }

    [Test]
    public void TryNextLiteral_SingleCharAtEnd_ShouldSuccess()
    {
        CommandLexer lexer = new("a");
        string? literal = lexer.TryNextLiteral("a");

        Assert.That(literal, Is.EqualTo("a"));
    }

    [Test]
    public void ReturnGreedyStringAtEnd_ShouldSuccess()
    {
        CommandLexer lexer = new("tell @a some greedy string");
        var tellNode = lexer.TryNextLiteral("tell");
        var @aNode = lexer.TryNextLiteral("@a");
        var stringNode = lexer.TryNextGreedyString();

        Assert.Multiple(() =>
        {
            Assert.That(tellNode, Is.Not.Null);
            Assert.That(@aNode, Is.Not.Null);
            Assert.That(stringNode, Is.Not.Null);
        });

        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnGreedyString(stringNode);
        });

        stringNode = lexer.TryNextGreedyString();

        Assert.That(stringNode, Is.EqualTo("some greedy string"));
    }

    [Test]
    public void ReturnContinuousTokenThatsTooLong_ShouldThrowInvalidOperation()
    {
        CommandLexer lexer = new("string");
        string? s = lexer.TryNextGreedyString();

        Assert.Throws<InvalidOperationException>(() =>
        {
            lexer.ReturnContinuousToken("looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong");
        });
    }

    [Test]
    public void ReturnGreedyStringAtMid_ShouldThrowInvalidOperation()
    {
        CommandLexer lexer = new("string value");
        _ = lexer.TryNextString();

        Assert.Throws<InvalidOperationException>(() =>
        {
            lexer.ReturnGreedyString("string");
        });
    }

    [Test]
    public void TryNextNumber_EmptyInput_ShouldReturnNull()
    {
        CommandLexer lexer = new("");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_SingleInteger_ShouldReturnNumber()
    {
        CommandLexer lexer = new("123");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.EqualTo("123"));
    }

    [Test]
    public void TryNextNumber_Zero_ShouldReturnZero()
    {
        CommandLexer lexer = new("0");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.EqualTo("0"));
    }

    [Test]
    public void TryNextNumber_LargeInteger_ShouldReturnNumber()
    {
        CommandLexer lexer = new("999999999");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.EqualTo("999999999"));
    }

    [Test]
    public void TryNextNumber_SingleDecimal_ShouldReturnNumber()
    {
        CommandLexer lexer = new("3.14");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.EqualTo("3.14"));
    }

    [Test]
    public void TryNextNumber_DecimalWithLeadingZero_ShouldReturnNumber()
    {
        CommandLexer lexer = new("0.5");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.EqualTo("0.5"));
    }

    [Test]
    public void TryNextNumber_DecimalWithTrailingZero_ShouldReturnNumber()
    {
        CommandLexer lexer = new("123.0");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.EqualTo("123.0"));
    }

    [Test]
    public void TryNextNumber_MultipleDecimals_ShouldReturnNumbers()
    {
        CommandLexer lexer = new("3.14 2.718 1.414");
        string? n1 = lexer.TryNextNumber();
        string? n2 = lexer.TryNextNumber();
        string? n3 = lexer.TryNextNumber();

        Assert.Multiple(() =>
        {
            Assert.That(n1, Is.EqualTo("3.14"));
            Assert.That(n2, Is.EqualTo("2.718"));
            Assert.That(n3, Is.EqualTo("1.414"));
        });
    }

    [Test]
    public void TryNextNumber_WithLeadingSpaces_ShouldReturnNumber()
    {
        CommandLexer lexer = new("   123");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.EqualTo("123"));
    }

    [Test]
    public void TryNextNumber_WithMultipleLeadingSpaces_ShouldReturnNumber()
    {
        CommandLexer lexer = new("     42");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.EqualTo("42"));
    }

    [Test]
    public void TryNextNumber_MultipleNumbers_ShouldReturnSequentially()
    {
        CommandLexer lexer = new("123 456 789");
        string? n1 = lexer.TryNextNumber();
        string? n2 = lexer.TryNextNumber();
        string? n3 = lexer.TryNextNumber();

        Assert.Multiple(() =>
        {
            Assert.That(n1, Is.EqualTo("123"));
            Assert.That(n2, Is.EqualTo("456"));
            Assert.That(n3, Is.EqualTo("789"));
        });
    }

    [Test]
    public void TryNextNumber_OnlySpace_ShouldReturnNull()
    {
        CommandLexer lexer = new("   ");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    // ==================== Invalid Number Tests ====================

    [Test]
    public void TryNextNumber_DoubleDot_ShouldReturnNull()
    {
        CommandLexer lexer = new("1.2.3");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_TripleDot_ShouldReturnNull()
    {
        CommandLexer lexer = new("1.2.3.4");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_StartWithDotAndNumber_ShouldReturnNull()
    {
        CommandLexer lexer = new(".123");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_StartWithDot_ShouldReturnNull()
    {
        CommandLexer lexer = new(".");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_EndWithDot_ShouldReturnNull()
    {
        CommandLexer lexer = new("123.");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithLetters_ShouldReturnNull()
    {
        CommandLexer lexer = new("123abc");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithLettersInMiddle_ShouldReturnNull()
    {
        CommandLexer lexer = new("12a34");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithDollarSign_ShouldReturnNull()
    {
        CommandLexer lexer = new("12$34");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithHyphen_ShouldReturnNull()
    {
        CommandLexer lexer = new("12-34");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithPlusSign_ShouldReturnNull()
    {
        CommandLexer lexer = new("12+34");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithUnderscore_ShouldReturnNull()
    {
        CommandLexer lexer = new("12_34");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithExclamation_ShouldReturnNull()
    {
        CommandLexer lexer = new("123!");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_StartWithAtSign_ShouldReturnNull()
    {
        CommandLexer lexer = new("@123");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_StartWithHash_ShouldReturnNull()
    {
        CommandLexer lexer = new("#123");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithComma_ShouldReturnNull()
    {
        CommandLexer lexer = new("1,234");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithSlash_ShouldReturnNull()
    {
        CommandLexer lexer = new("12/34");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithColon_ShouldReturnNull()
    {
        CommandLexer lexer = new("12:34");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithSemicolon_ShouldReturnNull()
    {
        CommandLexer lexer = new("12;34");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithParentheses_ShouldReturnNull()
    {
        CommandLexer lexer = new("(123)");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithBrackets_ShouldReturnNull()
    {
        CommandLexer lexer = new("[123]");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_WithBraces_ShouldReturnNull()
    {
        CommandLexer lexer = new("{123}");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_DecimalWithLettersAfter_ShouldReturnNull()
    {
        CommandLexer lexer = new("3.14abc");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_DecimalWithSpecialCharAfter_ShouldReturnNull()
    {
        CommandLexer lexer = new("3.14%");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.Null);
    }

    [Test]
    public void TryNextNumber_FailedThenNextString_ShouldSucceed()
    {
        CommandLexer lexer = new("abc 123");
        string? failedNumber = lexer.TryNextNumber();
        string? successString = lexer.TryNextString();

        Assert.Multiple(() =>
        {
            Assert.That(failedNumber, Is.Null);
            Assert.That(successString, Is.EqualTo("abc"));
        });
    }

    [Test]
    public void ReturnString_WithSingleSpaceAfter_ShouldSkipSpace()
    {
        CommandLexer lexer = new("abc");
        string? s = lexer.TryNextString();

        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnString(s!);
        });

        // After return, lexer should be at position 0, can parse again
        s = lexer.TryNextString();
        Assert.That(s, Is.EqualTo("abc"));
    }

    [Test]
    public void ReturnString_WithMultipleSpacesAfter_ShouldSkipAllSpaces()
    {
        CommandLexer lexer = new("abc");
        string? s = lexer.TryNextString();

        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnString(s!);
        });

        // After return, lexer should be at position 0
        s = lexer.TryNextString();
        Assert.That(s, Is.EqualTo("abc"));
    }

    [Test]
    public void ReturnString_WithoutSpaceAfter_ShouldNotSkip()
    {
        CommandLexer lexer = new("abc123");
        string? s = lexer.TryNextString();

        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnString(s!);
        });

        // After return, lexer should be at position 0
        s = lexer.TryNextString();
        Assert.That(s, Is.EqualTo("abc123"));
    }

    [Test]
    public void ReturnString_AtStringStart_ShouldHandleCorrectly()
    {
        CommandLexer lexer = new("abc");
        string? s = lexer.TryNextString();

        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnString(s!);
        });

        // Position should be 0 (at start), no more to skip
        s = lexer.TryNextString();
        Assert.That(s, Is.EqualTo("abc"));
    }

    [Test]
    public void ReturnString_MultipleTokens_ReturnMiddleToken()
    {
        CommandLexer lexer = new("first second third");
        string? s1 = lexer.TryNextString();
        string? s2 = lexer.TryNextString();

        Assert.Multiple(() =>
        {
            Assert.That(s1, Is.EqualTo("first"));
            Assert.That(s2, Is.EqualTo("second"));
        });

        // Return the second token
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnString(s2);
        });

        // Now lexer should be at position after "first " (6)
        // TryNextString should return "second" again
        s2 = lexer.TryNextString();
        Assert.That(s2, Is.EqualTo("second"));
    }

    [Test]
    public void ReturnString_MultipleTokens_WithMultipleSpacesBetween()
    {
        CommandLexer lexer = new("first  second   third");
        string? s1 = lexer.TryNextString();
        string? s2 = lexer.TryNextString();

        Assert.Multiple(() =>
        {
            Assert.That(s1, Is.EqualTo("first"));
            Assert.That(s2, Is.EqualTo("second"));
        });

        // Return the second token
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnString(s2);
        });

        // SkipSpaceBackward should skip all spaces before "second"
        s2 = lexer.TryNextString();
        Assert.That(s2, Is.EqualTo("second"));
    }

    [Test]
    public void ReturnString_ReturnMultipleTokensInSequence()
    {
        CommandLexer lexer = new("first second third");
        string? s1 = lexer.TryNextString();
        string? s2 = lexer.TryNextString();
        string? s3 = lexer.TryNextString();

        Assert.Multiple(() =>
        {
            Assert.That(s1, Is.EqualTo("first"));
            Assert.That(s2, Is.EqualTo("second"));
            Assert.That(s3, Is.EqualTo("third"));
        });

        // Return tokens in reverse order
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnString(s3);
            lexer.ReturnString(s2);
            lexer.ReturnString(s1);
        });

        // Now lexer should be at position 0
        s1 = lexer.TryNextString();
        s2 = lexer.TryNextString();
        s3 = lexer.TryNextString();

        Assert.Multiple(() =>
        {
            Assert.That(s1, Is.EqualTo("first"));
            Assert.That(s2, Is.EqualTo("second"));
            Assert.That(s3, Is.EqualTo("third"));
        });
    }

    [Test]
    public void ReturnContinuousToken_MixedTokenTypes_ShouldWork()
    {
        CommandLexer lexer = new("tell 123 @user");
        var literal = lexer.TryNextLiteral("tell");
        var number = lexer.TryNextNumber();
        var atUser = lexer.TryNextLiteral("@user");

        Assert.Multiple(() =>
        {
            Assert.That(literal, Is.EqualTo("tell"));
            Assert.That(number, Is.EqualTo("123"));
            Assert.That(atUser, Is.EqualTo("@user"));
        });

        // Return all tokens
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnContinuousToken(atUser);
            lexer.ReturnContinuousToken(number);
            lexer.ReturnContinuousToken(literal);
        });

        // Parse again
        literal = lexer.TryNextLiteral("tell");
        number = lexer.TryNextNumber();
        atUser = lexer.TryNextLiteral("@user");

        Assert.Multiple(() =>
        {
            Assert.That(literal, Is.EqualTo("tell"));
            Assert.That(number, Is.EqualTo("123"));
            Assert.That(atUser, Is.EqualTo("@user"));
        });
    }

    [Test]
    public void ReturnString_QuotedStringWithSpaces_ShouldReturnCorrectly()
    {
        CommandLexer lexer = new("\"hello world\" next");
        string? quoted = lexer.TryNextString();

        Assert.That(quoted, Is.EqualTo("hello world"));

        // Return the quoted string
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnString(quoted);
        });

        // Parse again
        quoted = lexer.TryNextString();
        Assert.That(quoted, Is.EqualTo("hello world"));
    }

    [Test]
    public void ReturnContinuousToken_DecimalNumber_ShouldReturnCorrectly()
    {
        CommandLexer lexer = new("3.14159");
        string? number = lexer.TryNextNumber();

        Assert.That(number, Is.EqualTo("3.14159"));

        // Return the number
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnContinuousToken(number);
        });

        // Parse again
        number = lexer.TryNextNumber();
        Assert.That(number, Is.EqualTo("3.14159"));
    }

    [Test]
    public void ReturnString_LiteralFailedThenReturn_ShouldNotAffect()
    {
        CommandLexer lexer = new("abc xyz");
        string? s1 = lexer.TryNextString();
        string? failed = lexer.TryNextLiteral("wrong");  // This should fail
        string? s2 = lexer.TryNextString();

        Assert.Multiple(() =>
        {
            Assert.That(s1, Is.EqualTo("abc"));
            Assert.That(failed, Is.Null);  // Literal match failed
            Assert.That(s2, Is.EqualTo("xyz"));
        });

        // Return s2
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnString(s2);
        });

        s2 = lexer.TryNextString();
        Assert.That(s2, Is.EqualTo("xyz"));
    }

    [Test]
    public void ReturnString_WithLeadingSpaces_ShouldSkipAll()
    {
        CommandLexer lexer = new("   abc");
        string? s = lexer.TryNextString();

        Assert.That(s, Is.EqualTo("abc"));

        // Return
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnString(s);
        });

        // Should skip all leading spaces
        s = lexer.TryNextString();
        Assert.That(s, Is.EqualTo("abc"));
    }

    [Test]
    public void ReturnString_ThatsTooLong_ShouldThrowInvalidOperation()
    {
        CommandLexer lexer = new("abc");

        string? s = lexer.TryNextString();
        Assert.That(s, Is.EqualTo("abc"));

        Assert.Throws<InvalidOperationException>(() =>
        {
            lexer.ReturnString("looooooooooooooooooooooooong");
        });
    }

    [Test]
    public void ReturnString_ToLexerThatsEmpty_ShouldThrowInvalidOperation()
    {
        CommandLexer lexer = new("");

        Assert.Throws<InvalidOperationException>(() =>
        {
            lexer.ReturnString("looooooooooooooooooooooooong");
        });
    }

    [Test]
    public void ReturnContinuousToken_ReturnThenParseDifferentToken_ShouldWork()
    {
        CommandLexer lexer = new("abc 123");
        string? s = lexer.TryNextString();
        string? n = lexer.TryNextNumber();

        Assert.Multiple(() =>
        {
            Assert.That(s, Is.EqualTo("abc"));
            Assert.That(n, Is.EqualTo("123"));
        });

        // Return both
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnContinuousToken(n);
            lexer.ReturnString(s);
        });

        // Parse in different order
        n = lexer.TryNextNumber();  // Should fail because "abc" is not a number
        s = lexer.TryNextString();

        Assert.Multiple(() =>
        {
            Assert.That(n, Is.Null);
            Assert.That(s, Is.EqualTo("abc"));
        });
    }


    [Test]
    public void ReturnContinuousToken_NumberWithSpaces_ShouldWork()
    {
        CommandLexer lexer = new("123 456");
        string? n1 = lexer.TryNextNumber();
        string? n2 = lexer.TryNextNumber();

        Assert.Multiple(() =>
        {
            Assert.That(n1, Is.EqualTo("123"));
            Assert.That(n2, Is.EqualTo("456"));
        });

        // Return second number
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnContinuousToken(n2);
        });

        n2 = lexer.TryNextNumber();
        Assert.That(n2, Is.EqualTo("456"));
    }

    [Test]
    public void ReturnContinuousToken_LiteralWithSpaces_ShouldWork()
    {
        CommandLexer lexer = new("cmd1 cmd2");
        string? l1 = lexer.TryNextLiteral("cmd1");
        string? l2 = lexer.TryNextLiteral("cmd2");

        Assert.Multiple(() =>
        {
            Assert.That(l1, Is.EqualTo("cmd1"));
            Assert.That(l2, Is.EqualTo("cmd2"));
        });

        // Return second literal
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnContinuousToken(l2);
        });

        l2 = lexer.TryNextLiteral("cmd2");
        Assert.That(l2, Is.EqualTo("cmd2"));
    }

    [Test]
    public void ReturnContinuousToken_DecimalNumber_ShouldWork()
    {
        CommandLexer lexer = new("3.14159");
        string? n = lexer.TryNextNumber();

        Assert.That(n, Is.EqualTo("3.14159"));

        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnContinuousToken(n);
        });

        n = lexer.TryNextNumber();
        Assert.That(n, Is.EqualTo("3.14159"));
    }

    [Test]
    public void ReturnContinuousToken_MultipleLiterals_ShouldReturnInSequence()
    {
        CommandLexer lexer = new("give take drop");
        string? l1 = lexer.TryNextLiteral("give");
        string? l2 = lexer.TryNextLiteral("take");
        string? l3 = lexer.TryNextLiteral("drop");

        Assert.Multiple(() =>
        {
            Assert.That(l1, Is.EqualTo("give"));
            Assert.That(l2, Is.EqualTo("take"));
            Assert.That(l3, Is.EqualTo("drop"));
        });

        // Return in reverse order
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnContinuousToken(l3);
            lexer.ReturnContinuousToken(l2);
            lexer.ReturnContinuousToken(l1);
        });

        // Parse again
        l1 = lexer.TryNextLiteral("give");
        l2 = lexer.TryNextLiteral("take");
        l3 = lexer.TryNextLiteral("drop");

        Assert.Multiple(() =>
        {
            Assert.That(l1, Is.EqualTo("give"));
            Assert.That(l2, Is.EqualTo("take"));
            Assert.That(l3, Is.EqualTo("drop"));
        });
    }

    [Test]
    public void ReturnContinuousToken_MultipleNumbers_ShouldReturnInSequence()
    {
        CommandLexer lexer = new("1 2 3");
        string? n1 = lexer.TryNextNumber();
        string? n2 = lexer.TryNextNumber();
        string? n3 = lexer.TryNextNumber();

        Assert.Multiple(() =>
        {
            Assert.That(n1, Is.EqualTo("1"));
            Assert.That(n2, Is.EqualTo("2"));
            Assert.That(n3, Is.EqualTo("3"));
        });

        // Return in reverse order
        Assert.DoesNotThrow(() =>
        {
            lexer.ReturnContinuousToken(n3);
            lexer.ReturnContinuousToken(n2);
            lexer.ReturnContinuousToken(n1);
        });

        // Parse again
        n1 = lexer.TryNextNumber();
        n2 = lexer.TryNextNumber();
        n3 = lexer.TryNextNumber();

        Assert.Multiple(() =>
        {
            Assert.That(n1, Is.EqualTo("1"));
            Assert.That(n2, Is.EqualTo("2"));
            Assert.That(n3, Is.EqualTo("3"));
        });
    }

}