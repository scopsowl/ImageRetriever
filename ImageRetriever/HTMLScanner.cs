using System;
using System.Collections.Generic;

namespace ImageRetriever
{
    public class HTMLScanner
    {
        // This is the host that can be used as the referer when we retrieve images later on.  It is not externally settable
        public string HTMLBuffer { get { return buffer; } }

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

        // given a tag (such as "<img") and a starting position, return a bool indicating if we found one, and if we did,
        // a dictionary containing all the attribute name/value pairs specified in that html element. 
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

        // skip over this html element.
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

        // Extract a dictionary full of attribute names and values from the current html element (only tested with <img tags)
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

        // wherever we are currently, advance until we are at the beginning of an element (or at the end of the buffer).
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

        // Skip along in the buffer until we find the beginning of an element.
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

        // skip over the list of 0 or more attributes that follow an element name.
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

        // After positioning past the name of an element, loop over the attributes and add
        // each of them to the dictionary
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

        // Skip over the current attribute (either name or name=value) in the buffer.
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

        // Add a single attribute (name or name=value) to our dictionary, and skip past it in the buffer.
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

        // For brevity, skip all the cruft we never want to look inside of.
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

        // Move the cursor position forwards by one if we are not already at the end of the buffer
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

        // skip the closing tag for an element.
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

        // skip the name of a tag, which must consist of only letters and numbers
        private bool skipTagName()
        {
            while (position < buffer.Length && char.IsLetterOrDigit(buffer[position]))
            {
                position++;
            }

            return position < buffer.Length;
        }

        // skip a contiguous run of whitespace characters
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

        // Skip over a quoted string, given the quote character to use.  Note that the quote
        // character can, itself, be escaped or 'quoted' using ampersand if it needs to be embedded.
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

        // An unquoted attribute value can only be terminated by a specific set of characters...
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

        // An attribute name can be made up of a specific set of characters.  Skip a contiguous sequence of them.
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

        // skip from the beginning of an element to the end, based on prefix and suffix strings
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

        //---------------- Private data

        // contains the html text we are scanning
        private string buffer;

        // our current position within the buffer
        private int position;
    }
}