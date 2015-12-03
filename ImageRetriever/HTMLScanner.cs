using System;

namespace ImageRetriever
{
    public class HTMLScanner
    {
        public string buffer;
        public int position;

        private HTMLScanner()
        {
        }

        public HTMLScanner(string input_string)
        {
            buffer = input_string;
            position = 0;
        }

        public bool FindTag(string tag_name, ref int start_pos, ref int length)
        {
            bool found = false;

            position = start_pos;

            do
            {
                if (skipWhitespaceCommentsEtc())
                {
                    if (matchesString(tag_name))
                    {
                        start_pos = position;
                        skipThisElement(tag_name);
                        length = position - start_pos;

                        found = true;
                    }
                    else
                    {
                        skipToNextElement();
                    }
                }
            }
            while (position < buffer.Length && !found);

            return found;
        }

        // Set the current position to point at the beginning of the buffer
        private void reset()
        {
            position = 0;
        }

        private bool skipThisElement(string tag_name)
        {
            if (matchesString(tag_name))
            {
                // skip the opening '<'
                ++position;
                skipTagName();
                skipAttributes();
                skipCloseTag();
            }

            return position < buffer.Length;
        }

        private bool skipToNextElement()
        {
            skipWhitespace();

            if (matchesString("</"))
            {
                position += 2;
                skipTagName();
                skipWhitespace();

                if (peekC(0) == '>')
                {
                    ++position;
                }
            }
            else if (matchesString("<!DOCTYPE"))
            {
                skipDOCTYPE();
            }
            else if (matchesString("<![CDATA["))
            {
                skipCDATA();
            }
            else if (matchesString("<!--"))
            {
                skipComment();
            }
            else if (matchesString("<!") || matchesString("<?"))
            {
                skipElement("<!", ">");
            }
            else if (matchesString("<"))
            {
                skipTagName();
                skipAttributes();
                skipCloseTag();
            }
            else
            {
                skipData();
            }

            return position < buffer.Length;
        }

        private bool skipData()
        {
            do
            {
                skipWhitespaceCommentsEtc();
                if (buffer[position] != '<')
                {
                    ++position;

                    if (buffer[position - 1] == '&' &&
                        position < buffer.Length    &&
                        buffer[position] == '<')
                    {
                        ++position;
                    }
                }
            }
            while (position < buffer.Length && buffer[position] != '<');

            return position < buffer.Length;
        }

        private bool skipAttributes()
        {
            do
            {
                skipWhitespace();
                skipAttribute();
            }
            while (!matchesString("/>") && !matchesString(">"));

            return position < buffer.Length;
        }

        private bool skipAttribute()
        {
            skipAttributeName();
            skipWhitespace();
            if (peekC(0) == '=')
            {
                position++;
                skipWhitespace();
                if (peekC(0) == '"')
                {
                    skipQuotedString('"');
                }
                else if (peekC(0) == '\'')
                {
                    skipQuotedString('\'');
                }
                else
                {
                    skipUnquotedString();
                }
                skipWhitespace();
            }

            return position < buffer.Length;
        }

        private bool skipWhitespaceCommentsEtc()
        {
            int save;

            // loop over the following until we stop finding stuff
            do
            {
                save = position;
            }
            while (skipWhitespace() &&
                   skipScript()     &&
                   skipCDATA()      &&
                   skipComment()    &&
                   save != position);

            return position < buffer.Length;
        }

        // Return the character positioned relative to the current position (or (char)0 if not within the buffer)
        private char peekC(int relative_offset)
        {
            int pos = position + relative_offset;
            if (pos >= 0 && pos < buffer.Length)
            {
                return buffer[pos];
            }
            else
            {
                return (char)0;
            }
        }

        // get the character at the current position in the buffer and advance to the next character.
        // returns 0 if at the end of the buffer already
        private char getC()
        {
            char c = peekC(0);
            position++;
            return c;
        }

        private bool skipCharacter(char c)
        {
            if (position < buffer.Length && buffer[position] == c)
                ++position;

            return position < buffer.Length;
        }

        // Are we positioned at the beginning of this string in the buffer?
        private bool matchesString(string target)
        {
            return position < buffer.Length && (buffer.IndexOf(target, position, 1) == 0);
        }

        private bool skipCloseTag()
        {
            if (skipWhitespace())
            {
                if (peekC(0) == '/' && peekC(1) == '>')
                    position += 2;
                else if (peekC(0) == '>')
                    position++;
            }

            return position < buffer.Length;
        }

        private bool skipTagName()
        {
            while (position < buffer.Length &&
                   char.IsLetterOrDigit(buffer[position]))
            {
                position++;
            }

            return position < buffer.Length;
        }

        private bool skipWhitespace()
        {
            while (position < buffer.Length &&
                   (buffer[position] == ' '  ||
                    buffer[position] == '\t' ||
                    buffer[position] == '\r' ||
                    buffer[position] == '\n' ||
                    buffer[position] == '\f'))
            {
                position++;
            }

            return position < buffer.Length;
        }

        private bool skipQuotedString(char quote)
        {
            while (position < buffer.Length && buffer[position] != quote)
            {
                ++position;

                if (buffer[position-1] == '&' &&
                    position < buffer.Length  &&
                    buffer[position] == quote)
                {
                    ++position;
                }
            }

            return position < buffer.Length;
        }

        private bool skipUnquotedString()
        {
            char c;
            while (position < buffer.Length)
            {
                c = buffer[position];
                if (c == '\'' ||
                    c == '\"' ||
                    c == '>'  ||
                    c == '<'  ||
                    c == '='  ||
                    c == '`'  ||
                    c == ' '  ||
                    c == '\t' ||
                    c == '\r' ||
                    c == '\n' ||
                    c == '\f')
                    break;
                ++position;
            }

            return position < buffer.Length;
        }

        private bool skipAttributeName()
        {
            char c;

            skipWhitespace();
            while (position < buffer.Length)
            {
                c = buffer[position];
                if (char.IsControl(c) || 
                    c == '\"'         ||
                    c == '\''         ||
                    c == '>'          ||
                    c == '/'          ||
                    c == '='          ||
                    c == ' '          ||
                    c == '\t'         ||
                    c == '\r'         ||
                    c == '\n'         ||
                    c == '\f')
                    break;
                ++position;
            }

            return position < buffer.Length;
        }

        private bool skipElement(string prefix, string suffix)
        {
            if (skipWhitespace() && matchesString(prefix))
            {
                position += prefix.Length;

                while (position < buffer.Length)
                {
                    if (matchesString(suffix))
                    {
                        position += suffix.Length;
                    }
                    else
                    {
                        position++;
                    }
                }
            }

            return position < buffer.Length;
        }

        private bool skipComment()
        {
            return skipElement("<!--", "-->");
        }

        private bool skipScript()
        {
            return skipElement("<script", "</script") &&
                   skipWhitespace() &&
                   skipCharacter('>');
        }

        private bool skipStyle()
        {
            return skipElement("<style", "</style") &&
                   skipWhitespace() &&
                   skipCharacter('>');
        }

        private bool skipCDATA()
        {
            return skipElement("<![CDATA[", "]]>");
        }

        private bool skipDOCTYPE()
        {
            return skipElement("<!DOCTYPE", ">");
        }
    }
}