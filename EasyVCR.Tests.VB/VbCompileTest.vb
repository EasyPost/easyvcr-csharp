'This test checks that EasyVCR C# code can be used in Visual Basic.
'This test project is running on .NET 6.0, although a success here should mean a success in all versions of .NET.
Imports Xunit

Public Class VbCompileTest
    <Fact>
    Public Sub TestCompile()
        Dim cassette = New EasyVCR.Cassette("fake_path", "fake_name")

        'Do not need to actually assert anything here. If it runs, it means it compiled successfully.
        Assert.NotNull(cassette)
    End Sub
End Class
