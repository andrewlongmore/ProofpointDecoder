using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProofpointDecoder;

namespace ProofpointDecoderTests
{
    [TestClass]
    public class ProofpointUtilsTests
    {
        [TestMethod]
        public void GetProofpointUrlV1BasicExample()
        {
            // Arrange
            string v1Url = "https://urldefense.proofpoint.com/v1/url?u=http://www.example.com&k=0.";
            string expected = "http://www.example.com";

            // Act
            string result = ProofpointUtils.GetProofpointUrl(v1Url);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetProofpointUrlV2BasicExample()
        {
            // Arrange
            string v2Url = "https://urldefense.proofpoint.com/v2/url?u=https-3A-2F-2Fexample.com-2F&d=CwIGCg";
            string expected = "https://example.com/";

            // Act
            string result = ProofpointUtils.GetProofpointUrl(v2Url);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetProofpointUrlV2CharacterTranslation()
        {
            // Arrange (- becomes % and _ becomes /)
            string v2Url = "https://urldefense.proofpoint.com/v2/url?u=http-3A-2F-2Fexample.com-2Ftest-file_page&d=CwIGCg";
            string expected = "http://example.com/test%file/page";

            // Act
            string result = ProofpointUtils.GetProofpointUrl(v2Url);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetProofpointUrlV3BasicExample()
        {
            // Arrange
            string v3Url = "https://urldefense.proofpoint.com/v3/__https://example.com/__;!!";
            string expected = "https://example.com/";

            // Act
            string result = ProofpointUtils.GetProofpointUrl(v3Url);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetProofpointUrlV3SingleSlashSubstitution()
        {
            // Arrange (single slash after protocol becomes double slash)
            string v3Url = "https://urldefense.proofpoint.com/v3/__http:/example.com__;!!";
            string expected = "http://example.com";

            // Act
            string result = ProofpointUtils.GetProofpointUrl(v3Url);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetProofpointUrlV3SingleTokenReplacement()
        {
            // Arrange (single * token replacement)
            string v3Url = "https://urldefense.proofpoint.com/v3/__https://example.com/*__;dGVzdA!";
            string expected = "https://example.com/t";

            // Act
            string result = ProofpointUtils.GetProofpointUrl(v3Url);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetProofpointUrlV3MultipleTokenReplacement()
        {
            // Arrange (multiple ** token replacements)
            string v3Url = "https://urldefense.proofpoint.com/v3/__https://example.com/**A**B__;ZXhhbXA!";
            string expected = "https://example.com/examp";

            // Act
            string result = ProofpointUtils.GetProofpointUrl(v3Url);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}