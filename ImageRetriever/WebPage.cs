using System;
using System.Net;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ImageRetriever
{
    public class WebPage: IEnumerable
    {
        public delegate void DisplayString(string text);

        public DisplayString OutputMethod;

        public string BufferOfText;
        public Uri Address;

        private Uri url;
        private List<string> image_links;

        public string Url { get { return url.OriginalString; }
                            set { url = new Uri(value); BufferOfText = null; Fetch(); Collect("<img"); } }

        void DisplayStringToConsole(string text)
        {
            Console.WriteLine(text);
        }

        private WebPage()
        {
            // by default, use console I/O
            OutputMethod = DisplayStringToConsole;
            image_links = new List<string>();
        }

        public WebPage(string input_url)
        {
            // by default, use console I/O
            OutputMethod = DisplayStringToConsole;
            image_links  = new List<string>();
            Url          = input_url;
        }

        public bool Collect(string tag_name)
        {
            bool retval    = true;
            int  start_pos = -1;
            int  length    = -1;

            HTMLScanner scanner = new HTMLScanner(BufferOfText);

            while (scanner.FindTag(tag_name, ref start_pos, ref length) && start_pos >= 0 && length > 0)
            {
                image_links.Add(BufferOfText.Substring(start_pos, length));
            }

            return retval;
        }

        public bool Fetch()
        {
            bool is_success = false;

            if (url.IsWellFormedOriginalString())
            {
                try
                {
                    WebRequest      request  = WebRequest.Create(url);  // prepare to make a request using the string's URI scheme
                    HttpWebResponse response = null;

                    is_success = true;
                    try
                    {
                        response = (HttpWebResponse)request.GetResponse();

                        // set up to do the read
                        Stream       receiveStream = response.GetResponseStream();
                        StreamReader readStream    = new StreamReader(receiveStream, Encoding.UTF8);

                        // Pipes the stream to a higher level stream reader with the required encoding format.
                        try
                        {
                            BufferOfText = readStream.ReadToEnd();

                            if (BufferOfText != null)
                            {
                                OutputMethod(BufferOfText);
                            }

                            is_success = true;
                        }
                        finally
                        {
                            if (readStream != null)
                            {
                                readStream.Close();
                            }
                        }
                    }
                    finally
                    {
                        if (response != null)
                        {
                            response.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    OutputMethod("Some exception occurred: " + e.Message);
                }
            }

            return is_success;
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)image_links).GetEnumerator();
        }
    }
}
