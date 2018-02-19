// prueba
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;

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
                var lin = 0;
                
                //scintilla.Lines[line].FoldLevelFlags = FoldLevelFlags.;

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
                            else scintilla.SetStyling(1, StyleOperator);


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
                        if (Char.IsLetterOrDigit(c) || c == '-' || c == '_')
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
                            scintilla.SetStyling(1, StyleComment);
                            scintilla.SetStyling(1, StyleComment);
                            length = 0; c2 = ' ';
                            while (c != '*' || c2 != '/')
                            {
                                startPos++; length++;
                                c = (char)scintilla.GetCharAt(startPos);
                                
                                c2 = (char)scintilla.GetCharAt(startPos+1);
                            }
                            length--;
                            var style = StyleComment;
                            //length += 1;
                            //var comentario = scintilla.GetTextRange(startPos - length, length);
                            scintilla.SetStyling(length, style);
                            scintilla.SetStyling(1, StyleComment);
                            scintilla.SetStyling(1, StyleComment);
                            length = 0;
                            state = STATE_UNKNOWN;
                            startPos += 2;
                            goto REPROCESS;

                        }
                        break;
                }

                //lin = scintilla.LineFromPosition(startPos);
                //scintilla.Lines[lin].FoldLevel = lev;

                startPos++;
                


            }
        }
 
        public void Fold(Scintilla scintilla, int startPos, int endPos)
        {

            var line = startPos;
            startPos = scintilla.Lines[line].Position;
            endPos = scintilla.Lines[endPos].Position;
            var lev = 1024;
            var c = ' ';

            scintilla.Markers[1].Symbol = MarkerSymbol.Background;
            scintilla.Markers[1].SetBackColor(Color.FromArgb(0x10, 0x10, 0x10));

            scintilla.Markers[2].Symbol = MarkerSymbol.Background;
            scintilla.Markers[2].SetBackColor(Color.FromArgb(0x0f, 0x0f, 0x0a)); 

            scintilla.Markers[3].Symbol = MarkerSymbol.Background;
            scintilla.Markers[3].SetBackColor(Color.FromArgb(0x20, 0x20, 0x20));   

            scintilla.Markers[4].Symbol = MarkerSymbol.Background;
            scintilla.Markers[4].SetBackColor(Color.FromArgb(0x24, 0x24, 0x24));

            scintilla.Markers[0].Symbol = MarkerSymbol.Background;
            scintilla.Markers[0].SetBackColor(Color.Black);
 
            while (startPos < endPos)
            {
                c = (char)scintilla.GetCharAt(startPos);
                line = scintilla.LineFromPosition(startPos);
 
                if (c == '{' && scintilla.GetStyleAt(startPos) == StyleOperator)
                {
                    scintilla.Lines[line].FoldLevelFlags = FoldLevelFlags.Header;
                    lev++;
                   
                    scintilla.Lines[line + 1].FoldLevel = lev;
                    while (c != '\n') { startPos++; c = (char)scintilla.GetCharAt(startPos); }
                    startPos++;

                }
                else if (c == '}' && scintilla.GetStyleAt(startPos) == StyleOperator)
                {
                    lev--;
                    scintilla.Lines[line].FoldLevel = lev;

                    
                    while (c != '\n') { startPos++; c = (char)scintilla.GetCharAt(startPos); }
                    startPos++; line++;

                }
 
                else {
 
                    scintilla.Lines[line].FoldLevel = lev; startPos++;
 
                }
 
            }

            var offset = 0;

            for (  line = 0; line < scintilla.Lines.Count; line++)
            {

                //line = scintilla.LineFromPosition(startPos);

                //var pos1 = scintilla.Lines[line].Position;
                //var pos2 = scintilla.Lines[line].EndPosition;
                //var texto = scintilla.GetTextRange(pos1, pos2 - pos1);
                var texto = scintilla.Lines[line].Text;



                if (texto.Contains(" entonces"))
                {
                    //scintilla.Lines[line].FoldLevel++;
                    scintilla.Lines[line].FoldLevelFlags = FoldLevelFlags.Header;
                    offset++;
                }
                else if (texto.Contains("si-fin"))
                {
                    offset--;
                    lev = scintilla.Lines[line].FoldLevel + offset;
                    scintilla.Lines[line].FoldLevel = lev;
                }

                else if (texto.Contains("sino"))
                {

                    //scintilla.Lines[line].FoldLevel--;
                    scintilla.Lines[line].FoldLevelFlags = FoldLevelFlags.Header;
                    //offset++;
                }

                else {
                    lev = scintilla.Lines[line].FoldLevel + offset;
                    scintilla.Lines[line].FoldLevel = lev;
                }

                lev = scintilla.Lines[line].FoldLevel;
                //lev = scintilla.Lines[line].Indentation;


                switch (lev)
                {
                    case 1024:
                        //scintilla.Lines[line].MarkerDelete(4);
                        //scintilla.Lines[line].MarkerDelete(3);
                        //scintilla.Lines[line].MarkerDelete(2);
                        //scintilla.Lines[line].MarkerDelete(1);
                        scintilla.Lines[line].MarkerDelete(-1);
                        scintilla.Lines[line].MarkerAdd(0);
                        break;

                    case 1025:
                        //scintilla.Lines[line].MarkerDelete(4);
                        //scintilla.Lines[line].MarkerDelete(3);
                        //scintilla.Lines[line].MarkerDelete(2);
                        //scintilla.Lines[line].MarkerDelete(1);
                        scintilla.Lines[line].MarkerDelete(-1);
                        scintilla.Lines[line].MarkerAdd(1);
                        break;

                    case 1026:
                        //scintilla.Lines[line].MarkerDelete(4);
                        //scintilla.Lines[line].MarkerDelete(3);
                        //scintilla.Lines[line].MarkerDelete(2);
                        //scintilla.Lines[line].MarkerDelete(1);
                        scintilla.Lines[line].MarkerDelete(-1);
                        scintilla.Lines[line].MarkerAdd(2);
                        break;

                    case 1027:
                        //scintilla.Lines[line].MarkerDelete(4);
                        //scintilla.Lines[line].MarkerDelete(3);
                        //scintilla.Lines[line].MarkerDelete(2);
                        //scintilla.Lines[line].MarkerDelete(1);
                        scintilla.Lines[line].MarkerDelete(-1);
                        scintilla.Lines[line].MarkerAdd(3);
                        break;

                    case 1028:
                        //scintilla.Lines[line].MarkerDelete(4);
                        //scintilla.Lines[line].MarkerDelete(3);
                        //scintilla.Lines[line].MarkerDelete(2);
                        //scintilla.Lines[line].MarkerDelete(1);
                        scintilla.Lines[line].MarkerDelete(-1);
                        scintilla.Lines[line].MarkerAdd(4);
                        break;

                    default:
                        //scintilla.Lines[line].MarkerDelete(4);
                        //scintilla.Lines[line].MarkerDelete(3);
                        //scintilla.Lines[line].MarkerDelete(2);
                        //scintilla.Lines[line].MarkerDelete(1);
                        scintilla.Lines[line].MarkerDelete(-1);
                        scintilla.Lines[line].MarkerAdd(0);
                        break;

                }

                //lineas en blanco:
                //if (texto.Trim() == string.Empty)
                //{
                //    scintilla.Lines[line].MarkerDelete(4);
                //    scintilla.Lines[line].MarkerDelete(3);
                //    scintilla.Lines[line].MarkerDelete(2);
                //    scintilla.Lines[line].MarkerDelete(1);
                //    scintilla.Lines[line].MarkerDelete(0);
                //    scintilla.Lines[line].MarkerAdd(0);
                //}


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
