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
    public class WebPageTests
    {
        private string output_text;

        void OutputMock(string text) { output_text = text; }

        [TestMethod()]
        public void WebPageTest()
        {
            WebPage retriever = new WebPage("http://www.google.com/");
        }

        [TestMethod()]
        public void FetchTest()
        {
            WebPage retriever = new WebPage("http://www.google.com/");

            retriever.OutputMethod = OutputMock;

            output_text = null;
            Assert.IsTrue(retriever.Fetch());
            Assert.IsNotNull(retriever.BufferOfText);
            Assert.IsTrue(retriever.BufferOfText.IndexOf("<!doctype html>", StringComparison.CurrentCultureIgnoreCase) == 0);
        }
    }
}