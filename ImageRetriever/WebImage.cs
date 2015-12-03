using System;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace ImageRetriever
{
    public class WebImage
    {
        public delegate void DisplayString(string text);

        public DisplayString OutputMethod;

        public string image_tag;

        private Uri url;
        private Uri base_url;

        public string BaseURL
        {
            get { return base_url.OriginalString; }
            set { base_url = new Uri(value);      }
        }

        void DisplayStringToConsole(string text)
        {
            Console.WriteLine(text);
        }

        private WebImage()
        {
            url = null;
            base_url = null;
        }

        public WebImage(string base_url, string tag)
        {
            BaseURL   = base_url;
            image_tag = tag;
        }

        public bool Save(string path)
        {
            bool retval = false;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string base_path = Path.GetFullPath(path);



            return retval;
        }
    }
}