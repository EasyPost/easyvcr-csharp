// This test checks that EasyVCR C# code can be used in NetStandard.

using EasyVCR;
using Xunit;

namespace EasyVCR.Compatibility.NetStandard
{
    public class NetStandardCompileTest
    {
        [Fact]
        public void TestCompile()
        {
            var cassette = new Cassette("fake_path", "fake_name");
            // The assert doesn't really do anything, but as long as this test can run, then the code is compiling correctly.
            Assert.NotNull(cassette);
        }
    }
}
