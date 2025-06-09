using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThirdPartyLicenseGenerator;
internal class Program
{
    // Basic names (do not include path here)
    private const string AUTO_LICENSE_JSON = "Auto.json";
    private const string MANUAL_LICENSE_JSON = "Manual.json";
    private const string AUTO_LICENSE_FOLDER = "Auto";
    private const string MANUAL_LICENSE_FOLDER = "Manual";
    private const string OUTPUT_FILE = "ThirdPartyLicenses.txt";

    private const string SEPARATOR_LINE = "==============================";
    private const int SEPARATOR_LENGTH = 60;
    private const string VERSION_PREFIX = "v";

    private const string INVALID_LICENSE_MESSAGE = "Invalid or unsupported license(s) detected:";
    private const string MISSING_FILES_MESSAGE = "Missing license file(s):";
    private const string EXPECTED_PREFIX = "Expected: ";

    // Summary section constants
    private const string SUMMARY_HEADER = "THIRD PARTY LICENSES SUMMARY";

    // Allowed license types
    static readonly HashSet<string> AllowedLicenseTypes = [
        "MIT", "BSD", "BSD-2-Clause", "BSD-3-Clause", "Apache-2.0", "MPL"
    ];

    // Cache JsonSerializerOptions
    private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    static int Main(string[] args)
    {
        // Get base folder from args (default to ".")
        var baseFolder = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]) ? args[0] : ".";
        if (!Directory.Exists(baseFolder))
        {
            Console.Error.WriteLine($"Base folder does not exist: {baseFolder}");
            return 1;
        }

        // Get output folder from args (default to base folder)
        var outputFolder = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : baseFolder;

        // Create output folder if it doesn't exist
        if (!Directory.Exists(outputFolder))
        {
            try
            {
                Directory.CreateDirectory(outputFolder);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to create output folder: {outputFolder}. Error: {ex.Message}");
                return 1;
            }
        }

        // Compose all relevant paths using the base folder and output folder
        string autoJsonPath = Path.Combine(baseFolder, AUTO_LICENSE_JSON);
        string manualJsonPath = Path.Combine(baseFolder, MANUAL_LICENSE_JSON);
        string autoFolderPath = Path.Combine(baseFolder, AUTO_LICENSE_FOLDER);
        string manualFolderPath = Path.Combine(baseFolder, MANUAL_LICENSE_FOLDER);
        string outputFilePath = Path.Combine(outputFolder, OUTPUT_FILE);

        // Read JSON license lists
        var autoList = ReadLicenseList(autoJsonPath, autoFolderPath);
        var manualList = ReadLicenseList(manualJsonPath, manualFolderPath);

        // Merge: manual overrides auto (by PackageName+Version)
        var merged = manualList
            .Concat(autoList.Where(a => !manualList.Any(m => m.PackageId == a.PackageId && m.PackageVersion == a.PackageVersion)))
            .ToList();

        // 1. License type check
        var invalidLicenses = merged.Where(x => !AllowedLicenseTypes.Contains(x.License ?? "")).ToList();

        // 2. License file existence check
        var missingFiles = new List<(LicenseInfo, string)>();
        foreach (var pkg in merged)
        {
            var dirs = new[] { manualFolderPath, autoFolderPath };
            string? foundFile = null;
            string pattern = $"{pkg.PackageId}__{pkg.PackageVersion}.*";

            foreach (var dir in dirs)
            {
                if (Directory.Exists(dir))
                {
                    var files = Directory.GetFiles(dir, pattern);
                    if (files.Length > 0)
                    {
                        foundFile = files.First();
                        break;
                    }
                }
            }

            if (foundFile == null)
            {
                // Show both paths in error message (relative to base folder)
                var expectManual = Path.Combine(MANUAL_LICENSE_FOLDER, pattern);
                var expectAuto = Path.Combine(AUTO_LICENSE_FOLDER, pattern);
                missingFiles.Add((pkg, $"{expectManual} or {expectAuto}"));
            }
            else
            {
                pkg.LicenseFile = foundFile;
            }
        }

        // Error reporting (aggregate)
        if (invalidLicenses.Count != 0 || missingFiles.Count != 0)
        {
            var msg = new System.Text.StringBuilder();
            if (invalidLicenses.Count != 0)
            {
                msg.AppendLine(INVALID_LICENSE_MESSAGE);
                foreach (var lic in invalidLicenses)
                    msg.AppendLine($"  - {lic.PackageId} {lic.PackageVersion} ({lic.License})");
            }
            if (missingFiles.Count != 0)
            {
                msg.AppendLine(MISSING_FILES_MESSAGE);
                foreach (var (pkg, path) in missingFiles)
                    msg.AppendLine($"  - {pkg.PackageId} {pkg.PackageVersion} ({EXPECTED_PREFIX}{path})");
            }
            // Output error message to console and exit with error status
            Console.Error.WriteLine(msg.ToString());
            return 1; // Exit with error status
        }

        // 3. Generate merged license file with summary
        using var writer = new StreamWriter(outputFilePath, false, System.Text.Encoding.UTF8);

        // Write summary section
        writer.WriteLine(SUMMARY_HEADER);
        writer.WriteLine(SEPARATOR_LINE);
        writer.WriteLine($"Total packages: {merged.Count}");
        writer.WriteLine();

        // Group by license type and show counts
        var licenseGroups = merged.GroupBy(x => x.License).OrderBy(g => g.Key);
        foreach (var group in licenseGroups)
        {
            writer.WriteLine($"{group.Key}: {group.Count()} package{(group.Count() == 1 ? "" : "s")}");
        }
        writer.WriteLine();

        // List all packages
        foreach (var pkg in merged.OrderBy(x => x.PackageId).ThenBy(x => x.PackageVersion))
        {
            writer.WriteLine($"  {pkg.PackageId} {VERSION_PREFIX}{pkg.PackageVersion} ({pkg.License})");
        }

        writer.WriteLine($"\n{SEPARATOR_LINE}\n");

        // Write detailed license sections
        foreach (var pkg in merged.OrderBy(x => x.PackageId).ThenBy(x => x.PackageVersion))
        {
            writer.WriteLine(pkg.PackageId);
            writer.WriteLine($"{VERSION_PREFIX}{pkg.PackageVersion} ({pkg.License})");
            writer.WriteLine(new string('-', SEPARATOR_LENGTH));
            writer.WriteLine(File.ReadAllText(pkg.LicenseFile));
            writer.WriteLine($"\n{SEPARATOR_LINE}\n");
        }

        Console.WriteLine($"Generated {OUTPUT_FILE} in {outputFolder}");
        return 0; // Exit with success status
    }

    static List<LicenseInfo> ReadLicenseList(string jsonFile, string folder)
    {
        if (!File.Exists(jsonFile))
            return [];
        var json = File.ReadAllText(jsonFile);
        var list = JsonSerializer.Deserialize<List<LicenseInfo>>(json, CachedJsonSerializerOptions) ?? [];
        foreach (var x in list)
            x.IsManual = Path.GetFileName(folder).StartsWith("Manual");
        return list;
    }

    record LicenseInfo
    {
        public string PackageId { get; set; } = "";
        public string PackageVersion { get; set; } = "";
        public string? License { get; set; }
        [JsonIgnore] public bool IsManual { get; set; }
        [JsonIgnore] public string LicenseFile { get; set; } = "";
    }
}