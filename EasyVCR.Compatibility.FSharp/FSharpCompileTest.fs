// This test checks that EasyVCR C# code can be used in F#.
// This test project is running on the latest .NET, although a success here should mean a success in all versions of .NET.'

namespace EasyVCR.Compatibility.FSharp

open Xunit

type FSharpCompileTest() =
    [<Fact>]
    member this.TestCompile() =
        let cassette = EasyVCR.Cassette("fake_path", "fake_name")
        // The assert doesn't really do anything, but as long as this test can run, then the code is compiling correctly.
        Assert.NotNull(cassette)
