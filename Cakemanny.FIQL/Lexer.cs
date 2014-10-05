using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cakemanny.FIQL
{
    public class Lexer
    {
        private readonly string characters;
        private int pos = 0;
        private bool canBeIdent = true;

        public event Action<String> trace = msg => { };

        public Lexer(string characters)
        {
            this.characters = characters;
        }

        public List<Token> lex()
        {
            var result = new List<Token>();
            while (!eof()) {
                var token = readToken();
                trace(token.ToString());
                result.Add(token);
                var s = token.symbol;
                canBeIdent = (s == Symbol.comma
                        || s == Symbol.semicolon
                        || s == Symbol.lparen);
            }
            return result;
        }

        private bool eof()
        {
            return pos >= characters.Length;
        }

        private char peekChar()
        {
            return !eof() ? characters[pos] : '\0';
        }

        private char readChar()
        {
            return !eof() ? characters[pos++] : '\0';
        }

        private char next()
        {
            ++pos;
            return (pos < characters.Length) ? characters[pos] : '\0';
        }

        private LexException error(string message)
        {
            return new LexException(message, characters, pos);
        }

        private bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool isAlpha(char c) {
            return (c >= 'a' && c <= 'z')
                    || (c >= 'A' && c <= 'Z')
                    || c == '_'
                    || c == '$';
        }

        private bool isAlphaNum(char c) {
            return (c >= 'a' && c <= 'z')
                    || (c >= 'A' && c <= 'Z')
                    || (c >= '0' && c <= '9')
                    || c == '_'
                    || c == '$';
        }

        private bool isString(char c) {
            return !(c == '=' || c == '*' || c == '(' || c == ')' ||
                        c == '!' || c == ',' || c == ';' || c =='\'' ||
                        c == '\0');
        }

        private bool isWild(char c) {
            return c == '*' || isString(c);
        }

        private Token readToken() {
            StringBuilder data = new StringBuilder("");
            int start = pos;
            char c = peekChar();

            switch (c) {
                case ';': readChar(); return new Token(Symbol.semicolon, ";");
                case ',': readChar(); return new Token(Symbol.comma, ",");
                case '(': readChar(); return new Token(Symbol.lparen, "(");
                case ')': readChar(); return new Token(Symbol.rparen, ")");
                case '!':
                    c = next();
                    if (c == '=') {
                        readChar();
                        return new Token(Symbol.notequal, "!=");
                    }
                    throw error("Expected = after !");
                case '=': {
                    c = next();
                    switch (c) {
                        case '=':
                            readChar();
                            return new Token(Symbol.equal, "==");
                        case 'g':
                            c = next();
                            switch (c) {
                                case 'e':
                                    if ('=' == next()) {
                                        readChar();
                                        return new Token(Symbol.greaterequal, "=ge=");
                                    }
                                    break;
                                case 't':
                                    if ('=' == next()) {
                                        readChar();
                                        return new Token(Symbol.greaterthan, "=gt=");
                                    }
                                    break;
                            }
                            break;
                        case 'l':
                            c = next();
                            switch (c) {
                                case 'e':
                                    if ('=' == next()) {
                                        readChar();
                                        return new Token(Symbol.lessequal, "=le=");
                                    }
                                    break;
                                case 't':
                                    if ('=' == next()) {
                                        readChar();
                                        return new Token(Symbol.lessthan, "=lt=");
                                    }
                                    break;
                            }
                            break;
                    }
                    throw error("Expected comparison ==,=ge=,=gt=,=le=,=lt= given first =");
                }
                default: {
                    if (canBeIdent) {
                        // process identifier
                        if (isAlpha(c)) {
                            // identifier or string
                            while (isAlphaNum(c)) {
                                data.Append(c);
                                c = next();
                            }
                            return new Token(Symbol.ident, data.ToString());
                        } else {
                            throw error("Expected identifier here");
                        }
                    } else {
                        return readValue(data, start, c);
                    }
                }
            }
        }

        private Token readValue(StringBuilder data, int start, char c) {
            if (isDigit(c)) {
                // process date
                // process number
                while (isDigit(c)) {
                    data.Append(c);
                    c = next();
                }
                if (c == '-' && (pos - start) == 4) { // date
                    data.Append(c);
                    c = next();
                    if (isDigit(c) && isDigit(next())
                            && '-' == next()
                            && isDigit(next()) && isDigit(next())) {
                        readChar();
                        return new Token(Symbol.date, characters.Substring(start, pos - start));
                    }
                    throw error("Incorrectly formatted date, expected yyyy-mm-dd");
                } else if (!isWild(c)) {
                    return new Token(Symbol.number, data.ToString());
                } else {
                    return readWild(data, start, c);
                }
            } else if (isWild(c)) {
                return readWild(data, start, c);
            } else {
                throw error("Unexpected character!");
            }
        }

        private Token readWild(StringBuilder data, int start, char c) {
                bool boolean = true, stringtype = true;
                // process bool
                // process string
                // process wild
                int x = 0;
                char[] tru = {'t','r','u','e'};
                char[] fals = {'f','a','l','s','e'};
                if (c == 't') while (x < 4 && c == tru[x++]) {
                    data.Append(c); c = next();
                }
                else if (c == 'f') while (x < 5 && c == fals[x++]) {
                    data.Append(c); c = next();
                }
                while (isWild(c)) {
                    boolean = false;
                    stringtype = stringtype && (c != '*');
                    data.Append(c);
                    c = next();
                }
                return boolean ? new Token(Symbol.boolean, data.ToString())
                        : stringtype ? new Token(Symbol.stringtype, data.ToString())
                        : new Token(Symbol.wildstring, data.ToString());
        }

    }

}

