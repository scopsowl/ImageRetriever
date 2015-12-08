using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageRetriever;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageRetriever.Tests
{
    [TestClass()]
    public class HTMLScannerTests
    {
        [TestMethod()]
        public void HTMLScannerTest()
        {
            string test_string  = "foo";
            HTMLScanner scanner = new HTMLScanner(test_string);

            Assert.IsTrue(scanner.buffer.Equals(test_string));
        }

        [TestMethod()]
        public void FindTagTest()
        {
            string test_style_name   = "style";
            string test_style_value  = "padding - top:112px";
            string test_style        = test_style_name + "=\'" + test_style_value + "\'";
            string test_height_name  = "height";
            string test_height_value = "92";
            string test_height       = test_height_name + " = " + test_height_value;
            string test_width_name   = "width";
            string test_width_value  = "272";
            string test_width        = test_width_name + "=\"" + test_width_value + "\"";
            string test_alt_name     = "alt";
            string test_alt_value    = "Google";
            string test_alt          = test_alt_name + "=\"" + test_alt_value + "\"";
            string test_id_name      = "id";
            string test_id_value     = "hplogo";
            string test_id           = test_id_name + "=\"" + test_id_value + "\"";
            string test_title_name   = "title";
            string test_title_value  = "Google";
            string test_title        = test_title_name + "=\"" + test_title_value + "\"";
            string test_src_name     = "src";
            string test_src_value    = "/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png";
            string test_src          = test_src_name + "=\"" + test_src_value + "\"";
            string test_img          = "<img" + " " + test_style  +
                                                " " + test_height + 
                                                " " + test_src    + 
                                                " " + test_width  + 
                                                " " + test_alt    + 
                                                " " + test_id     + 
                                                " " + test_title  + 
                                       ">";
            string test_string       = "<!doctype html> <html> " + test_img + " </html>";
            Dictionary<string, string> attributes;

            HTMLScanner scanner = new HTMLScanner(test_string);

            int start  = 0;
            int length = 0;

            Assert.IsTrue(scanner.FindTag("<img", ref start, out length, out attributes));
            Assert.IsTrue(test_string.Substring(start, length).Equals(test_img));
            Assert.IsTrue(attributes[test_style_name].Equals(test_style_value));
            Assert.IsTrue(attributes[test_height_name].Equals(test_height_value));
            Assert.IsTrue(attributes[test_width_name].Equals(test_width_value));
            Assert.IsTrue(attributes[test_alt_name].Equals(test_alt_value));
            Assert.IsTrue(attributes[test_id_name].Equals(test_id_value));
            Assert.IsTrue(attributes[test_title_name].Equals(test_title_value));
            Assert.IsTrue(attributes[test_src_name].Equals(test_src_value));
        }
    }
}