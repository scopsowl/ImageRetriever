using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ImageRetriever.Tests
{
    [TestClass()]
    public class WebPageTests
    {
        private string output_text;

        void OutputMock(string text) { output_text = text; }

        [TestMethod()]
        public void WebPageTest()
        {
            string test_google = "http://www.google.com/";
            WebPage retriever = new WebPage(test_google);

            retriever.OutputMethod = OutputMock;

            Assert.IsTrue(retriever.HostAddress.Equals(test_google, StringComparison.CurrentCultureIgnoreCase));
            Assert.IsNotNull(retriever.BufferOfText);
            Assert.IsTrue(retriever.BufferOfText.IndexOf("<!doctype html>", StringComparison.CurrentCultureIgnoreCase) == 0);
        }
    }
}