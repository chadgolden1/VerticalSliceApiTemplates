using System.Runtime.CompilerServices;

namespace TodoApi.Tests;

public static class SampleData
{
    public static string SampleString([CallerMemberName] string caller = "") => $"{caller.Replace("Sample", "")}-{Guid.NewGuid()}";
}
