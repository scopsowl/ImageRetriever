using System;
using System.Drawing;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace ImageRetriever
{
    public class WebImage
    {
        public delegate void DisplayString(string text);

        // This delegate is called whenever we need to display something to the user.  Overridden for testing.
        public DisplayString OutputMethod;

        // Host that has the image on it, as best we could figure out
        public string Referer
        {
            get { return referer.OriginalString; }
            set { referer = new Uri(value);      }
        }

        // This is a complete <img ... > tag retrieved from an HTML document
        public string ImageTag { get { return image_tag; } }

        // Wrapper for console display method - used by default if not overridden
        void DisplayStringToConsole(string text)
        {
            Console.WriteLine(text);
        }

        private WebImage()
        {
            Referer = "http://www.google.com/";
            url     = null;
        }

        public WebImage(string referer_url, string tag)
        {
            Referer   = referer_url;
            image_tag = tag;

            HTMLScanner scanner = new HTMLScanner(tag);

            int start = 0;
            int length = 0;
            Dictionary<string, string> attributes;
            if (scanner.FindTag("<img", ref start, out length, out attributes))
            {
                if (attributes != null)
                {
                    string value;
                    if (attributes.TryGetValue("src", out value) &&
                        Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
                    {
                        bool success = false;
                        try { url = new Uri(value); success = true;  } catch (UriFormatException) { }
                        if (!success)
                        {
                            try { url = new Uri(referer, value); success = true; } catch (UriFormatException) { }
                            if (!success)
                            {
                                if (!referer_url.EndsWith("/"))
                                {
                                    referer_url = referer_url + "/";
                                }

                                if (value.StartsWith("/"))
                                {
                                    value = value.Substring(1);
                                }

                                try { url = new Uri(referer_url + value); success = true; } catch (UriFormatException) { }
                            }
                        }
                    }
                }
            }
        }

        // Create a local file in the folder passed in that contains the remote image data
        public string SaveLocal(string path)
        {
            string filename = null;

            if (url != null && url.IsWellFormedOriginalString())
            {
                try
                {
                    // prepare to make a request using the string's URI scheme
                    HttpWebResponse response = null;
                    WebRequest      request  = WebRequest.Create(url); 
                    try
                    {
                        // ask nicely
                        response = (HttpWebResponse)request.GetResponse();

                        if ((response.StatusCode == HttpStatusCode.OK    ||
                             response.StatusCode == HttpStatusCode.Moved ||
                             response.StatusCode == HttpStatusCode.Redirect) &&
                            response.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        {
                            // get a stream of bytes
                            using (Stream receiveStream = response.GetResponseStream())
                            {
                                try
                                {
                                    using (var image = Image.FromStream(receiveStream))
                                    {
                                        // make a unique filename
                                        filename = ConstructLocalFileName(path, response.ContentType);

                                        // create any subdirectories along the path
                                        string directory = Path.GetDirectoryName(filename);
                                        if (!Directory.Exists(directory))
                                        {
                                            Directory.CreateDirectory(directory);
                                        }

                                        // save the image file
                                        image.Save(filename);
                                    }
                                }
                                catch (ArgumentException e)
                                {
                                    OutputMethod("Some exception occurred: " + e.Message);
                                    filename = null;
                                }
                                finally
                                {
                                    receiveStream.Close();
                                }
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
                    filename = null;
                }
            }

            return filename;
        }

        private string ConstructLocalFileName(string path, string content_type)
        {
            string filename = Path.GetFullPath(path);
            for (int index = 0; index < url.Segments.Length; ++index)
            {
                if (index > 0 || !url.Segments[index].Equals("/"))
                {
                    filename = Path.Combine(filename, url.Segments[index].Replace('/', Path.DirectorySeparatorChar));
                }
            }

            string datestamp = DateTime.Now.Year.ToString("D4") + "-" + DateTime.Now.Month.ToString("D2") + "-" + DateTime.Now.Day.ToString("D2");
            string base_name = filename + "\\" + "Image " + datestamp;

            // chop off the "image/" from the content type
            string extension = content_type.Substring(6).ToLower();
            if (extension.Equals("jpeg"))
            {
                extension = "jpg";  // yes, "jpeg" works, but it offends me.
            }
            filename = base_name + "." + extension;
            
            // handle the case of duplicate files by incrementing an integer and appending it to the name, like "foo(1).png", etc.      
            if (File.Exists(filename))
            {
                int count = 0;
                string test_file;
                do
                {
                    test_file = base_name + "(" + (++count).ToString() + ")." + extension;
                }
                while (File.Exists(test_file));
                filename = test_file;
            }

            return filename;
        }

        //---------------- Private data

        private string image_tag;
        private Uri    referer;
        private Uri    url;
    }
}