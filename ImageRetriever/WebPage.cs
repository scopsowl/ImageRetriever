using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ImageRetriever
{
    public class WebPage: IEnumerable
    {
        public delegate void DisplayString(string text);

        // This delegate is called whenever we need to display something to the user.  It gets
        // overridden in unit tests so that the test can take a look at messages otherwise
        // being directed to the console.
        public DisplayString OutputMethod;

        // This is the url passed in when the WebPage object was created
        public string PageAddress { get { return page_addr.OriginalString; }
                                    set { page_addr = new Uri(value);
                                          html_buffer = null;
                                          Fetch();
                                          Collect("<img"); } }

        // This is the host that can be used as the referer when we retrieve images later on.  It is not externally settable
        public string HostAddress { get { return host_addr.OriginalString; } }

        // This is the HTML document returned by the call to fetch()
        public string BufferOfText { get { return html_buffer; } }

        // Wrapper for console display method
        void DisplayStringToConsole(string text)
        {
            Console.WriteLine(text);
        }

        // I want to loop over images by saying something like "foreach (string img_tag in page)", so I need
        // the WebPage class to be enumerable.
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)image_links).GetEnumerator();
        }

        // Don' let anyone create a WebPage object without providing a url, but if they do, use google's main page.
        private WebPage()
        {
            // by default, use console I/O (except in unit tests)
            OutputMethod = DisplayStringToConsole;

            image_links = new List<string>();
            PageAddress = "http://www.google.com";
        }

        public WebPage(string input_url)
        {
            // by default, use console I/O (except in unit tests)
            OutputMethod = DisplayStringToConsole;
 
            image_links  = new List<string>();
            PageAddress  = input_url;
        }

        // loop through the HTML document, collecting a list of tags matching the tag passed in.  This has only been
        // tested with <img tags, but the HTMLScanner class could be expanded to work with other elements.
        public bool Collect(string tag_name)
        {
            int  start_pos = 0;
            int  length    = 0;

            Dictionary<string, string> attributes;
            HTMLScanner scanner = new HTMLScanner(html_buffer);

            image_links.Clear();
            while (scanner.FindTag(tag_name, ref start_pos, out length, out attributes) && start_pos >= 0 && length > 0)
            {
                image_links.Add(BufferOfText.Substring(start_pos, length));
                start_pos += length;
            }

            return image_links.Count > 0;
        }

        //  Call this method to set up the HTTP request, and retrieve the response, reporting any errors along the way
        private bool Fetch()
        {
            bool is_success = false;

            if (page_addr != null && page_addr.IsWellFormedOriginalString())
            {
                try
                {
                    HttpWebRequest  request  = (HttpWebRequest)WebRequest.Create(page_addr);
                    HttpWebResponse response = null;

                    request.AllowAutoRedirect = true;

                    is_success = true;
                    try
                    {
                        response = (HttpWebResponse)request.GetResponse();

                        UriBuilder builder = new UriBuilder();

                        builder.Scheme = request.Address.Scheme;
                        builder.Host   = request.Address.Host;
                        builder.Port   = request.Address.Port;

                        string referer = builder.ToString();

                        host_addr = new Uri(referer);

                        // set up the read
                        Stream       receiveStream = response.GetResponseStream();
                        StreamReader readStream    = new StreamReader(receiveStream, Encoding.UTF8);

                        try
                        {
                            html_buffer = readStream.ReadToEnd();

                            if (html_buffer != null)
                            {
                                OutputMethod(html_buffer);
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

        //---------------- Private data

        // address of this web page
        private Uri page_addr;

        // address of this web page
        private Uri host_addr;

        // contains each of the <img tags found in the html document 
        private List<string> image_links;

        // buffer containing returned html document after fetch()
        private string html_buffer;
    }
}
