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

                if (page.BufferOfText != null)
                {
                    // Got the page, now look for images
                    foreach (string image_link in page)
                    {
                        WebImage image = new WebImage(page.HostAddress, image_link);

                        image.SaveLocal(args[0]);
                    }

                    Console.WriteLine(page.BufferOfText);  // no abstraction in main for unit testing I/O
                }
            }
        }
    }
}
