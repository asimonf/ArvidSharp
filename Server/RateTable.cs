using System.Runtime.InteropServices;

namespace Arvid.Server
{
    internal static class RateTable
    {
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct LineRate
        {
            public readonly ushort Lines;
            public readonly float Rate;

            public LineRate(ushort lines, float rate)
            {
                Lines = lines;
                Rate = rate;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ModeCycles
        {
            public readonly ushort PixelsPerLine;
            public readonly ushort AsymetricPixels;

            public readonly byte PassiveCyclesPerPixel;
            public readonly byte PassiveCyclesPerPixelMod;

            public readonly byte LineEndDelay;
            public readonly byte LineEndDelayMod;

            public ModeCycles(
                ushort pixelsPerLine,
                ushort asymetricPixels,
                byte passiveCyclesPerPixel,
                byte passiveCyclesPerPixelMod,
                byte lineEndDelay,
                byte lineEndDelayMod
            )
            {
                PixelsPerLine = pixelsPerLine;
                AsymetricPixels = asymetricPixels;
                PassiveCyclesPerPixel = passiveCyclesPerPixel;
                PassiveCyclesPerPixelMod = passiveCyclesPerPixelMod;
                LineEndDelay = lineEndDelay;
                LineEndDelayMod = lineEndDelayMod;
            }

            public uint PixelData()
            {
                return (uint) (PixelsPerLine | AsymetricPixels << 16);
            }

            public uint TimingData()
            {
                return (uint) (
                    PassiveCyclesPerPixel | 
                    PassiveCyclesPerPixelMod << 8 |
                    LineEndDelay << 16 |
                    LineEndDelayMod << 24
                );
            }
        }

        public static LineRate[][] LineRates = {
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.166668f),
                new LineRate(302,  50.333332f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.816666f),
                new LineRate(298,  50.983334f),
                new LineRate(297,  51.150002f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.666668f),
                new LineRate(293,  51.833332f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.183334f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.533333f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.883335f),
                new LineRate(286,  53.066666f),
                new LineRate(285,  53.250000f),
                new LineRate(284,  53.433334f),
                new LineRate(283,  53.616665f),
                new LineRate(282,  53.799999f),
                new LineRate(281,  53.983334f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.366665f),
                new LineRate(278,  54.549999f),
                new LineRate(277,  54.750000f),
                new LineRate(276,  54.933334f),
                new LineRate(275,  55.133335f),
                new LineRate(274,  55.333332f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.716667f),
                new LineRate(271,  55.916668f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.533333f),
                new LineRate(267,  56.733334f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.150002f),
                new LineRate(264,  57.366665f),
                new LineRate(263,  57.566666f),
                new LineRate(262,  57.783333f),
                new LineRate(261,  58.000000f),
                new LineRate(260,  58.216667f),
                new LineRate(259,  58.433334f),
                new LineRate(258,  58.650002f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.099998f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.549999f),
                new LineRate(253,  59.783333f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.716667f),
                new LineRate(248,  60.950001f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.816666f),
                new LineRate(298,  50.983334f),
                new LineRate(297,  51.150002f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.883335f),
                new LineRate(286,  53.049999f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.983334f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.549999f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.933334f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.716667f),
                new LineRate(271,  55.916668f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.716667f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.566666f),
                new LineRate(262,  57.783333f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.416668f),
                new LineRate(258,  58.650002f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.816666f),
                new LineRate(298,  50.966667f),
                new LineRate(297,  51.150002f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.883335f),
                new LineRate(286,  53.049999f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.966667f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.533333f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.916668f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.716667f),
                new LineRate(271,  55.916668f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.716667f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.566666f),
                new LineRate(262,  57.766666f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.416668f),
                new LineRate(258,  58.633335f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.816666f),
                new LineRate(298,  50.983334f),
                new LineRate(297,  51.150002f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.883335f),
                new LineRate(286,  53.049999f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.983334f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.549999f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.933334f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.716667f),
                new LineRate(271,  55.916668f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.716667f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.566666f),
                new LineRate(262,  57.766666f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.416668f),
                new LineRate(258,  58.650002f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.816666f),
                new LineRate(298,  50.983334f),
                new LineRate(297,  51.150002f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.883335f),
                new LineRate(286,  53.049999f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.966667f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.533333f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.933334f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.716667f),
                new LineRate(271,  55.916668f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.716667f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.566666f),
                new LineRate(262,  57.766666f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.416668f),
                new LineRate(258,  58.650002f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.799999f),
                new LineRate(298,  50.966667f),
                new LineRate(297,  51.133335f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.866665f),
                new LineRate(286,  53.049999f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.966667f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.533333f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.916668f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.700001f),
                new LineRate(271,  55.900002f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.716667f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.549999f),
                new LineRate(262,  57.766666f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.416668f),
                new LineRate(258,  58.633335f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.799999f),
                new LineRate(298,  50.966667f),
                new LineRate(297,  51.133335f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.866665f),
                new LineRate(286,  53.049999f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.966667f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.533333f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.916668f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.700001f),
                new LineRate(271,  55.900002f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.716667f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.549999f),
                new LineRate(262,  57.766666f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.416668f),
                new LineRate(258,  58.633335f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.816666f),
                new LineRate(298,  50.983334f),
                new LineRate(297,  51.150002f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.883335f),
                new LineRate(286,  53.066666f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.983334f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.549999f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.933334f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.716667f),
                new LineRate(271,  55.916668f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.733334f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.566666f),
                new LineRate(262,  57.783333f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.433334f),
                new LineRate(258,  58.650002f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.799999f),
                new LineRate(298,  50.966667f),
                new LineRate(297,  51.133335f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.866665f),
                new LineRate(286,  53.049999f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.966667f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.533333f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.916668f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.700001f),
                new LineRate(271,  55.900002f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.716667f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.566666f),
                new LineRate(262,  57.766666f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.416668f),
                new LineRate(258,  58.633335f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.799999f),
                new LineRate(298,  50.966667f),
                new LineRate(297,  51.133335f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.866665f),
                new LineRate(286,  53.049999f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.966667f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.533333f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.916668f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.700001f),
                new LineRate(271,  55.900002f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.716667f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.549999f),
                new LineRate(262,  57.766666f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.416668f),
                new LineRate(258,  58.633335f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.799999f),
                new LineRate(298,  50.966667f),
                new LineRate(297,  51.133335f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.866665f),
                new LineRate(286,  53.049999f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.966667f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.533333f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.916668f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.700001f),
                new LineRate(271,  55.900002f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.716667f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.549999f),
                new LineRate(262,  57.766666f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.416668f),
                new LineRate(258,  58.633335f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.799999f),
                new LineRate(298,  50.966667f),
                new LineRate(297,  51.133335f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.866665f),
                new LineRate(286,  53.049999f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.966667f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.533333f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.916668f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.700001f),
                new LineRate(271,  55.900002f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.716667f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.549999f),
                new LineRate(262,  57.766666f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.416668f),
                new LineRate(258,  58.633335f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
            new[]
            {
                new LineRate(304,  50.000000f),
                new LineRate(303,  50.150002f),
                new LineRate(302,  50.316666f),
                new LineRate(301,  50.483334f),
                new LineRate(300,  50.650002f),
                new LineRate(299,  50.799999f),
                new LineRate(298,  50.966667f),
                new LineRate(297,  51.133335f),
                new LineRate(296,  51.316666f),
                new LineRate(295,  51.483334f),
                new LineRate(294,  51.650002f),
                new LineRate(293,  51.816666f),
                new LineRate(292,  52.000000f),
                new LineRate(291,  52.166668f),
                new LineRate(290,  52.349998f),
                new LineRate(289,  52.516666f),
                new LineRate(288,  52.700001f),
                new LineRate(287,  52.866665f),
                new LineRate(286,  53.049999f),
                new LineRate(285,  53.233334f),
                new LineRate(284,  53.416668f),
                new LineRate(283,  53.599998f),
                new LineRate(282,  53.783333f),
                new LineRate(281,  53.966667f),
                new LineRate(280,  54.166668f),
                new LineRate(279,  54.349998f),
                new LineRate(278,  54.533333f),
                new LineRate(277,  54.733334f),
                new LineRate(276,  54.916668f),
                new LineRate(275,  55.116665f),
                new LineRate(274,  55.316666f),
                new LineRate(273,  55.516666f),
                new LineRate(272,  55.700001f),
                new LineRate(271,  55.900002f),
                new LineRate(270,  56.116665f),
                new LineRate(269,  56.316666f),
                new LineRate(268,  56.516666f),
                new LineRate(267,  56.716667f),
                new LineRate(266,  56.933334f),
                new LineRate(265,  57.133335f),
                new LineRate(264,  57.349998f),
                new LineRate(263,  57.549999f),
                new LineRate(262,  57.766666f),
                new LineRate(261,  57.983334f),
                new LineRate(260,  58.200001f),
                new LineRate(259,  58.416668f),
                new LineRate(258,  58.633335f),
                new LineRate(257,  58.866665f),
                new LineRate(256,  59.083332f),
                new LineRate(255,  59.316666f),
                new LineRate(254,  59.533333f),
                new LineRate(253,  59.766666f),
                new LineRate(252,  60.000000f),
                new LineRate(251,  60.233334f),
                new LineRate(250,  60.466667f),
                new LineRate(249,  60.700001f),
                new LineRate(248,  60.933334f),
            },
        };

        public static ModeCycles[] ModeCyclesTable = {
            new ModeCycles(320, 0, 4, 1, 61, 0),
            new ModeCycles(256, 2, 5, 3, 61, 0),
            new ModeCycles(288, 2, 4, 3, 69, 0),
            new ModeCycles(384, 2, 2, 3, 93, 0),
            new ModeCycles(240, 0, 6, 3, 61, 0),
            new ModeCycles(392, 3, 2, 2, 79, 2),
            new ModeCycles(400, 0, 3, 0, 100, 3),
            new ModeCycles(292, 0, 5, 0, 110, 2),
            new ModeCycles(336, 0, 4, 0, 116, 3),
            new ModeCycles(416, 4, 2, 1, 64, 3),
            new ModeCycles(448, 0, 2, 1, 61, 0),
            new ModeCycles(512, 0, 1, 3, 82, 1),
            new ModeCycles(640, 0, 1, 0, 60, 3),
        };
    }
}