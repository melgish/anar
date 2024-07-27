
using Anar.Services.Gateway;
using Anar.Services.Locator;
using Anar.Services.Worker;

internal static class TestData
{
    public const string CorruptFileJSON = "this is not json";

    public const string LayoutFileJSON = """
    {
        "arrays": [
            {
            "label": "Array 1",
            "azimuth": 180,
            "modules": [
                {   "inverter": { "serial_num": "SN123" } },
                {   "inverter": { "serial_num": "SN124" } }
            ]
            },
            {
            "label": "Array 2",
            "azimuth": 90,
            "modules": [
                {   "inverter": { "serial_num": "SN125" } }
            ]
            }
        ]
    }
    """;

    public static readonly LayoutDTO LayoutFileData = new()
    {
        Arrays = [
            new()
            {
                ArrayName = "Array 1",
                Azimuth = 180,
                Modules = [
                    new() { Inverter = new() { SerialNumber = "SN123" } },
                    new() { Inverter = new() { SerialNumber = "SN124" } }
                ]
            },
            new()
            {
                ArrayName = "Array 2",
                Azimuth = 90,
                Modules = [
                    new() { Inverter = new() { SerialNumber = "SN125" } }
                ]
            }
        ]
    };

    public static readonly IList<Location> LayoutFileLocations = [
        new Location("SN123", "Array 1", 180),
        new Location("SN124", "Array 1", 180),
        new Location("SN125", "Array 2", 90)
    ];

    public static readonly string InverterJSON = """
    [
        {
            "serial_num": "SN123",
            "last_report_date": 1722139200,
            "last_report_watts": 1000
        },
        {
            "serial_num": "SN124",
            "last_report_date": 1722149200,
            "last_report_watts": 250
        },
        {
            "serial_num": "SN125",
            "last_report_date": 1722139300,
            "last_report_watts": 500
        }
    ]
    """;

    public static readonly IList<Inverter> InverterResponse = [
        new Inverter
        {
            SerialNumber = "SN123",
            LastReportDate = 1722139200,
            LastReportWatts = 1000
        },
        new Inverter
        {
            SerialNumber = "SN124",
            LastReportDate = 1722149200,
            LastReportWatts = 250
        },
        new Inverter
        {
            SerialNumber = "SN125",
            LastReportDate = 1722139300,
            LastReportWatts = 500
        }
    ];

    public static readonly IList<Reading> Readings = [
        new(InverterResponse[0], LayoutFileLocations[0]),
        new(InverterResponse[1], LayoutFileLocations[1]),
        new(InverterResponse[2], LayoutFileLocations[2])
    ];

    public static readonly IList<string> ReadingsLines = [
        @"inverter,arrayName=Array\ 1,facing=south,serialNumber=SN123 watts=1000i 1722139200",
        @"inverter,arrayName=Array\ 1,facing=south,serialNumber=SN124 watts=250i 1722149200",
        @"inverter,arrayName=Array\ 2,facing=east,serialNumber=SN125 watts=500i 1722139300"
    ];

    public static readonly Totals Totals = new(1750, 1722149200);

    public const string TotalsLine = @"totals wattsNow=1750i 1722149200";
}