using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using EasyVCR.Interfaces;

namespace EasyVCR.Tests
{
    public static class TestUtils
    {
        internal static Cassette GetCassette(string cassetteName, IOrderOption? order = null)
        {
            var cassettePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cassettes", cassetteName);
            return new Cassette(GetDirectoryInCurrentDirectory("cassettes"), cassetteName, order);
        }

        internal static string GetCurrentDirectory()
        {
            return _GetCurrentDirectory();
        }

        internal static string GetDirectoryInCurrentDirectory(string directoryPath)
        {
            return Path.Combine(GetCurrentDirectory(), directoryPath);
        }

        internal static HttpClient GetSimpleClient(string cassetteName, Mode mode)
        {
            var cassette = GetCassette(cassetteName);
            return HttpClients.NewHttpClient(cassette, mode);
        }

        internal static VCR GetSimpleVCR(Mode mode)
        {
            var vcr = new VCR(new AdvancedSettings
            {
                MatchRules = MatchRules.DefaultStrict
            });
            switch (mode)
            {
                case Mode.Record:
                    vcr.Record();
                    break;
                case Mode.Replay:
                    vcr.Replay();
                    break;
                case Mode.Bypass:
                    vcr.Pause();
                    break;
                case Mode.Auto:
                    vcr.RecordIfNeeded();
                    break;
            }

            return vcr;
        }

        private static string _GetCurrentDirectory([CallerFilePath] string sourceFilePath = "")
        {
            if (string.IsNullOrEmpty(sourceFilePath)) throw new ArgumentNullException(nameof(sourceFilePath));

            var path = Path.GetDirectoryName(sourceFilePath);
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Could not get directory from source file path");

            return path;
        }
    }
}
