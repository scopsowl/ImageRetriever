using System;
using System.IO;
using System.Net;
using System.Text;

namespace ImageRetriever
{
    public class ParameterValidator
    {
        public enum Usages
        {
            UsageMessage = 0,
            CheckNumParams,
            UnrecognizedPath,
            UnrecognizedURL,
            MaxUsageMessage
        };

        public static string[] UsageMessages = { "URLTest1 <existing local path> <URL to existing HTML page>",
                                                 "Incorrect number of command line parameters provided.",
                                                 "Directory path not recognized or does not exist.",
                                                 "URL not recognized." };

        public delegate void DisplayString(string text);

        public static DisplayString OutputMethod;

        static void DisplayStringToConsole(string text)
        {
            Console.WriteLine(text);
        }

        public ParameterValidator()
        {
            // by default, use console I/O
            OutputMethod = DisplayStringToConsole;
        }

        static void Usage(Usages usage)
        {
            int which_usage = (int)usage;
            if (which_usage < 0 || which_usage > (int)Usages.MaxUsageMessage)
                which_usage = 0;

            OutputMethod(UsageMessages[which_usage]);
        }

        public static bool ParamsAreValid(string[] args)
        {
            bool is_success = false;

            if (args.Length == 0)
            {
                Usage(Usages.UsageMessage);
            }
            else if (args.Length != 2)
            {
                Usage(Usages.CheckNumParams);
            }
            else if (!System.IO.Directory.Exists(args[0]))
            {
                Usage(Usages.UnrecognizedPath);
            }
            else if (!Uri.IsWellFormedUriString(args[1], UriKind.RelativeOrAbsolute))
            {
                Usage(Usages.UnrecognizedURL);
            }
            else
            {
                is_success = true;
            }

            return is_success;
        }
    }
}
