using Xunit;

namespace TechMoveGLMS.Tests
{
    public class FileValidationTests
    {
        private bool IsPdfFile(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return false;
            return filename.ToLower().EndsWith(".pdf");
        }

        [Fact]
        public void PdfFile_ShouldBeAccepted()
        {
            bool result = IsPdfFile("document.pdf");
            Assert.True(result);
        }

        [Fact]
        public void ExeFile_ShouldBeRejected()
        {
            bool result = IsPdfFile("virus.exe");
            Assert.False(result);
        }

        [Fact]
        public void JpgFile_ShouldBeRejected()
        {
            bool result = IsPdfFile("image.jpg");
            Assert.False(result);
        }

        [Fact]
        public void NoExtension_ShouldBeRejected()
        {
            bool result = IsPdfFile("document");
            Assert.False(result);
        }

        [Fact]
        public void EmptyFilename_ShouldBeRejected()
        {
            bool result = IsPdfFile("");
            Assert.False(result);
        }

        [Fact]
        public void UppercasePdf_ShouldBeAccepted()
        {
            bool result = IsPdfFile("DOCUMENT.PDF");
            Assert.True(result);
        }
    }
}