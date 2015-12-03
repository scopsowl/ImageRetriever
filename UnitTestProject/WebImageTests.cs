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
    public class WebImageTests
    {
        [TestMethod()]
        public void WebImageTest()
        {
            string test_base_url = "https://www.google.com";
            string test_tag      = "<img alt=\"foo\" src=\"/images/nav_logo242.png\" />";

            WebImage image = new WebImage(test_base_url, test_tag);

            Assert.IsTrue(test_tag.Equals(image.image_tag));
        }

        [TestMethod()]
        public void SaveTest()
        {
            string test_base_url = "https://www.google.com";
            string test_tag      = "<img alt=\"foo\" src=\"/images/nav_logo242.png\" />";

            WebImage image = new WebImage(test_base_url, test_tag);

            image.Save("c:\\Temp");
        }
    }
}