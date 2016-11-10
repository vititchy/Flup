using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace vt.extensions.Tests
{
    /// <summary>
    /// UT pouze pro odladeni
    /// </summary>
    [TestClass()]
    public class StringExtensionsTests
    {

        [TestMethod()]
        public void RemoveEndTextTest_DefaultStringComparison_Result()
        {
            string s = null;
            var result = s.RemoveEndText(null);
            Assert.IsNull(result);

            result = s.RemoveEndText(string.Empty);
            Assert.IsNull(result);

            s = string.Empty;
            result = s.RemoveEndText(null);
            Assert.AreSame(string.Empty, result);

            s = string.Empty;
            result = s.RemoveEndText(string.Empty);
            Assert.AreSame(string.Empty, result);

            s = "abcdefg";
            result = s.RemoveEndText("ef");
            Assert.AreSame(s, result);

            result = s.RemoveEndText("ab");
            Assert.AreSame(s, result);

            result = s.RemoveEndText("FG");
            Assert.AreSame(s, result);

            result = s.RemoveEndText("fg");
            Assert.AreEqual("abcde", result);

            result = s.RemoveEndText("efg");
            Assert.AreEqual("abcd", result);

            result = s.RemoveEndText("abcdefg");
            Assert.AreEqual("", result);

            result = s.RemoveEndText("Abcdefg");
            Assert.AreEqual(s, result);
        }


        [TestMethod()]
        public void RemoveEndTextTest_IgnoreCaseStringComparison_Result()
        {
            string s = null;
            var result = s.RemoveEndText(null, StringComparison.InvariantCultureIgnoreCase);
            Assert.IsNull(result);

            result = s.RemoveEndText(string.Empty, StringComparison.InvariantCultureIgnoreCase);
            Assert.IsNull(result);

            s = string.Empty;
            result = s.RemoveEndText(null, StringComparison.InvariantCultureIgnoreCase);
            Assert.AreSame(string.Empty, result);

            s = string.Empty;
            result = s.RemoveEndText(string.Empty, StringComparison.InvariantCultureIgnoreCase);
            Assert.AreSame(string.Empty, result);

            s = "abcdefg";
            result = s.RemoveEndText("ef", StringComparison.InvariantCultureIgnoreCase);
            Assert.AreSame(s, result);

            result = s.RemoveEndText("ab", StringComparison.InvariantCultureIgnoreCase);
            Assert.AreSame(s, result);

            result = s.RemoveEndText("FG", StringComparison.InvariantCultureIgnoreCase);
            Assert.AreEqual("abcde", result);

            result = s.RemoveEndText("fg", StringComparison.InvariantCultureIgnoreCase);
            Assert.AreEqual("abcde", result);

            result = s.RemoveEndText("efg", StringComparison.InvariantCultureIgnoreCase);
            Assert.AreEqual("abcd", result);

            result = s.RemoveEndText("abcdefg", StringComparison.InvariantCultureIgnoreCase);
            Assert.AreEqual("", result);

            result = s.RemoveEndText("Abcdefg", StringComparison.InvariantCultureIgnoreCase);
            Assert.AreEqual("", result);
        }

    }
}