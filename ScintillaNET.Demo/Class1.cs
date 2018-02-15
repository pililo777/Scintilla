// prueba
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ScintillaNET.Demo
{
    public class CSharpLexer
    {
        public const int StyleDefault = 0;
        public const int StyleKeyword = 1;
        public const int StyleIdentifier = 2;
        public const int StyleNumber = 3;
        public const int StyleString = 4;
        public const int StyleOperator = 5;
        public const int StyleComment = 6;

        private const int STATE_UNKNOWN = 0;
        private const int STATE_IDENTIFIER = 1;
        private const int STATE_NUMBER = 2;
        private const int STATE_STRING = 3;
        private const int STATE_COMMENT = 4;

        private HashSet<string> keywords;

        public void Style(Scintilla scintilla, int startPos, int endPos)
        {
            // Back up to the line start
            var line = scintilla.LineFromPosition(startPos);
            startPos = scintilla.Lines[line].Position;

            var length = 0;
            var state = STATE_UNKNOWN;
            string operators = ":[]{}+-*/=();,.%&#?!|\\<>";

            // Start styling
            scintilla.StartStyling(startPos);

            var c2 = ' ';
            var c1 = ' ';

            while (startPos < endPos)
            {
                
                var c = (char)scintilla.GetCharAt(startPos);

            REPROCESS:
                switch (state)
                {
                    case STATE_UNKNOWN:
                        if (c == '"'  || c=='\'')
                        {
                            // Start of "string"
                            c2 = c;
                            scintilla.SetStyling(1, StyleOperator);
                            state = STATE_STRING;
                        }
                        else if (Char.IsDigit(c))
                        {
                            state = STATE_NUMBER;
                            goto REPROCESS;
                        }
                        else if (Char.IsLetter(c))
                        {
                            state = STATE_IDENTIFIER;
                            goto REPROCESS;
                        }
                        else if (operators.Contains(c))
                        {
                            c1 = (char)scintilla.GetCharAt(startPos+1);
                            if (c == '/' && c1 == '/')
                            {
                                state = STATE_COMMENT;
                                startPos++;
                                goto REPROCESS;
                            }
                            else if (c == '/' && c1 == '*')
                            {
                                state = STATE_COMMENT;
                                startPos++;
                                goto REPROCESS;
                            }
                            else

                            scintilla.SetStyling(1, StyleOperator);
                        }
                        else
                        {
                            // Everything else
                            scintilla.SetStyling(1, StyleDefault);
                        }
                        break;

                    case STATE_STRING:
                        if (c == c2)
                        {
                            //length-=4;
                            scintilla.SetStyling(length, StyleString);
                            length = 0;
                            scintilla.SetStyling(1, StyleOperator);
                            state = STATE_UNKNOWN;
                        }
                        else
                        {
                            length++;
                        }
                        break;

                    case STATE_NUMBER:
                        if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == 'x')
                        {
                            length++;
                        }
                        else
                        {
                            scintilla.SetStyling(length, StyleNumber);
                            length = 0;
                            state = STATE_UNKNOWN;
                            goto REPROCESS;
                        }
                        break;

                    case STATE_IDENTIFIER:
                        if (Char.IsLetterOrDigit(c))
                        {
                            length++;
                        }
                        else
                        {
                            var style = StyleIdentifier;
                            var identifier = scintilla.GetTextRange(startPos - length, length);
                            if (keywords.Contains(identifier))
                                style = StyleKeyword;

                            scintilla.SetStyling(length, style);
                            length = 0;
                            state = STATE_UNKNOWN;
                            goto REPROCESS;
                        }
                        break;


                    case STATE_COMMENT:
                        if (c1 == '/')
                        {
                            length = 0;
                            while (c != '\n' && c != '\r')
                            {
                                startPos++; length++;
                                c = (char)scintilla.GetCharAt(startPos);
                            }
                            var style = StyleComment; 
                            //var comentario = scintilla.GetTextRange(startPos - length, length);
                            scintilla.SetStyling(++length, style);
                            length = 0;
                            state = STATE_UNKNOWN;
                            goto REPROCESS;
                        }
                        else
                        if (c1 == '*')
                        {
                            length = 0; c2 = ' ';
                            while (c != '*' && c2 != '/')
                            {
                                startPos++; length++;
                                c = (char)scintilla.GetCharAt(startPos);
                                c2 = (char)scintilla.GetCharAt(startPos+1);
                            }
                            var style = StyleComment; length += 2;
                            //var comentario = scintilla.GetTextRange(startPos - length, length);
                            scintilla.SetStyling(++length, style);
                            length = 0;
                            state = STATE_UNKNOWN;
                            startPos+=2;
                            goto REPROCESS;

                        }
                        break;
                }

                startPos++;
            }
        }

        public CSharpLexer(string keywords)
        {
            // Put keywords in a HashSet
            var list = Regex.Split(keywords ?? string.Empty, @"\s+").Where(l => !string.IsNullOrEmpty(l));
            this.keywords = new HashSet<string>(list);
        }
    }
}
