using System;
using System.Collections.Generic;

namespace ImageRetriever
{
    public class HTMLScanner
    {
        public string buffer;
        public int position;

        private HTMLScanner()
        {
            position = 0;
            buffer = "";
        }

        public HTMLScanner(string input_string)
        {
            position = 0;
            buffer = input_string;
            if (buffer == null)
            {
                buffer = "";
            }
        }

        public bool FindTag(string tag_name, ref int start_pos, out int length, out Dictionary<string, string> attributes)
        {
            bool found = false;
            int save = 0;

            Dictionary<string, string> temp_attrs = null;

            length = 0;

            if (start_pos < 0 ||
                start_pos >= buffer.Length)
            {
                start_pos = 0;
            }

            position = start_pos;
            do
            {
                save = position;
                if (skipWhitespaceCommentsEtc())
                {
                    if (matchesString(tag_name))
                    {
                        start_pos = position;
                        extractThisElement(tag_name, out temp_attrs);
                        length = position - start_pos;

                        found = true;
                    }
                    else
                    {
                        skipToNextElement();
                    }
                }
            }
            while (save < position && position < buffer.Length && !found);

            attributes = temp_attrs;

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

        private bool extractThisElement(string tag_name, out Dictionary<string, string> attributes)
        {
            attributes = new Dictionary<string, string>();

            if (matchesString(tag_name))
            {
                // skip the opening '<'
                ++position;
                skipTagName();
                extractAttributes(ref attributes);
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
            else if (matchesString("<script"))
            {
                skipScript();
            }
            else if (matchesString("<style"))
            {
                skipStyle();
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
                ++position;
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
            int save;
            do
            {
                save = position;
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
            while (save < position && position < buffer.Length && buffer[position] != '<');

            return position < buffer.Length;
        }

        private bool skipAttributes()
        {
            int save;
            do
            {
                save = position;
                skipWhitespace();
                skipAttribute();
            }
            while (save < position && !matchesString("/>") && !matchesString(">"));

            return position < buffer.Length;
        }

        private bool extractAttributes(ref Dictionary<string, string> attributes)
        {
            int save;
            do
            {
                save = position;
                skipWhitespace();
                extractAttribute(ref attributes);
            }
            while (save < position && !matchesString("/>") && !matchesString(">"));

            return position < buffer.Length;
        }

        private bool skipAttribute()
        {
            skipAttributeName();
            skipWhitespace();
            if (peekC(0) == '=')
            {
                position++;
                if (skipWhitespace() &&
                    peekC(0) == '"')
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


        private bool extractAttribute(ref Dictionary<string, string> attributes)
        {
            int start = position;
            skipAttributeName();
            string name = buffer.Substring(start, position - start);
            string value = null;

            skipWhitespace();
            if (peekC(0) == '=')
            {
                position++;
                skipWhitespace();
                if (peekC(0) == '"')
                {
                    start = position + 1;
                    skipQuotedString('"');
                    value = buffer.Substring(start, position - start - 1); ;
                }
                else if (peekC(0) == '\'')
                {
                    start = position + 1;
                    skipQuotedString('\'');
                    value = buffer.Substring(start, position - start - 1); ;
                }
                else
                {
                    start = position;
                    skipUnquotedString();
                    value = buffer.Substring(start, position - start); ;
                }
                skipWhitespace();
            }

            attributes.Add(name, value);

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
                   save < position);

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
            return target != null &&
                   (position < buffer.Length) &&
                   (target.Length <= (buffer.Length - position)) &&
                   buffer.Substring(position, target.Length).Equals(target, StringComparison.OrdinalIgnoreCase);
        }

        private bool skipCloseTag()
        {
            if (skipWhitespace())
            {
                if (peekC(0) == '/' && peekC(1) == '>')
                {
                    position += 2;
                }
                else if (peekC(0) == '>')
                {
                    position++;
                }
            }

            return position < buffer.Length;
        }

        private bool skipTagName()
        {
            while (position < buffer.Length && char.IsLetterOrDigit(buffer[position]))
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
            // remove leading quote
            if (buffer[position] == quote)
                ++position;

            // remove everything up to the trailing quote
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

            // remove trailing quote
            if (buffer[position] == quote)
                ++position;

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
                        break;
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