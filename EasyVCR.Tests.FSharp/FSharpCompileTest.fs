// This test checks that EasyVCR C# code can be used in F#.
// This test project is running on .NET 6.0, although a success here should mean a success in all versions of .NET.'

namespace EasyVCR.Tests.FSharp

open EasyVCR
open Xunit

type FSharpCompileTest() =
    [<Fact>]
    member this.TestCompile() =
        let cassette = EasyVCR.Cassette("fake_path", "fake_name")
        // The assert doesn't really do anything, but as long as this test can run, then the code is compiling correctly.
        Assert.NotNull(cassette)
