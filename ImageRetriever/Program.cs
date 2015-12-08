using System;

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
                    // Got the page, now grab each image represented by an <img ... > tag
                    int filecount = 0;
                    foreach (string image_link in page)
                    {
                        WebImage image = new WebImage(page.HostAddress, image_link);

                        // Here is where we actually retrieve the image data and write it to the filesystem
                        string filename = image.SaveLocal(args[0]);
                        if (filename != null)
                        {
                            Console.WriteLine("Downloaded {0}.", filename);
                            ++filecount;
                        }
                    }

                    Console.WriteLine("{0} files downloaded from {1}.", filecount, page.HostAddress);
                }
            }
        }
    }
}
