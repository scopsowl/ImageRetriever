using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageRetriever.Tests
{
    [TestClass()]
    public class ParameterValidatorTests
    {
        private string output_text;

        void OutputMock(string text) { output_text = text; }

        [TestMethod()]
        public void ParameterValidatorTest()
        {
            ParameterValidator validator = new ParameterValidator();
        }

        [TestMethod()]
        public void ParamsAreValidTest()
        {
            ParameterValidator.OutputMethod = OutputMock;

            // test the success case
            string[] test_args1 = { "c:\\Windows", "http://www.google.com/" };

            output_text = null;
            Assert.IsTrue(ParameterValidator.ParamsAreValid(test_args1));
            Assert.IsNull(output_text);

            // test the usage case (no params)
            string[] test_args2 = { };

            output_text = null;
            Assert.IsFalse(ParameterValidator.ParamsAreValid(test_args2));
            Assert.IsNotNull(output_text);
            Assert.IsTrue(output_text.Equals(ParameterValidator.UsageMessages[(int)ParameterValidator.Usages.UsageMessage]));

            // test the 1-param case
            string[] test_args3 = { "param1" };

            output_text = null;
            Assert.IsFalse(ParameterValidator.ParamsAreValid(test_args3));
            Assert.IsNotNull(output_text);
            Assert.IsTrue(output_text.Equals(ParameterValidator.UsageMessages[(int)ParameterValidator.Usages.CheckNumParams]));

            // test the 3-or-more-param case
            string[] test_args4 = { "param1", "param2", "param3" };

            output_text = null;
            Assert.IsFalse(ParameterValidator.ParamsAreValid(test_args4));
            Assert.IsNotNull(output_text);
            Assert.IsTrue(output_text.Equals(ParameterValidator.UsageMessages[(int)ParameterValidator.Usages.CheckNumParams]));

            // test bad local path case
            string[] test_args5 = { "xyzzy:\\This.Folder.Does.Not.Exist", "param3" };

            output_text = null;
            Assert.IsFalse(ParameterValidator.ParamsAreValid(test_args5));
            Assert.IsNotNull(output_text);
            Assert.IsTrue(output_text.Equals(ParameterValidator.UsageMessages[(int)ParameterValidator.Usages.UnrecognizedPath]));

            // test bad URI case
            string[] test_args6 = { "c:\\Windows\\", "http:\\???.goggle.comm\\" };

            output_text = null;
            Assert.IsFalse(ParameterValidator.ParamsAreValid(test_args6));
            Assert.IsNotNull(output_text);
            Assert.IsTrue(output_text.Equals(ParameterValidator.UsageMessages[(int)ParameterValidator.Usages.UnrecognizedURL]));
        }
    }
}