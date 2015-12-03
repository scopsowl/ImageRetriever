using System;
using System.IO;
using System.Net;
using System.Text;

namespace ImageRetriever
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ParameterValidator.ParamsAreValid(args))
            {
                WebPage page = new WebPage(args[1]);

                bool result = page.Fetch();

                if (page.BufferOfText != null)
                {
                    // Got the page, now look for images
                    foreach (string image_link in page)
                    {
                        WebImage image = new WebImage(args[1], image_link);

                        image.Save(args[0]);
                    }

                    Console.WriteLine(page.BufferOfText);  // no abstraction in main for unit testing I/O
                }
            }
        }
    }
}
