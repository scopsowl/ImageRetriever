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
            string test_string = "foo";
            HTMLScanner scanner = new HTMLScanner(test_string);

            Assert.IsTrue(scanner.buffer.Equals(test_string));
        }

        [TestMethod()]
        public void FindTagTest()
        {
            throw new NotImplementedException();
        }
    }
}