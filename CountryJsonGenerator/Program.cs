// ===== CONSOLE APP: CountryJsonGenerator =====
// Erstellen Sie eine neue Console App: dotnet new console -n CountryJsonGenerator

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CountryJsonGenerator;

class Program
{
    private static readonly HttpClient httpClient = new();

    static async Task Main(string[] args)
    {
        Console.WriteLine("🌍 Country JSON Generator");
        Console.WriteLine("========================");

        try
        {
            Console.WriteLine("📡 Fetching data from REST Countries API...");

            // REST Countries API aufrufen mit spezifischen Feldern (inkl. Phone Codes)
            var url = "https://restcountries.com/v3.1/all?fields=cca2,flag,name,translations,region,subregion,continents,idd";
            var response = await httpClient.GetStringAsync(url);

            Console.WriteLine("✅ Data received successfully!");
            Console.WriteLine("🔄 Processing countries...");

            // API Response zu unseren Models konvertieren
            var apiCountries = JsonSerializer.Deserialize<ApiCountry[]>(response);

            // Zu unserem Format konvertieren
            var countries = apiCountries
                .Where(c => !string.IsNullOrEmpty(c.cca2))
                .OrderBy(c => c.name?.common)
                .Select(ConvertToOurFormat)
                .ToList();

            Console.WriteLine($"✨ Processed {countries.Count} countries");

            // JSON generieren mit schöner Formatierung
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(countries, options);

            // Datei speichern
            var fileName = "countries.json";
            await File.WriteAllTextAsync(fileName, json);

            Console.WriteLine($"💾 Saved to {Path.GetFullPath(fileName)}");
            Console.WriteLine($"📊 Total countries: {countries.Count}");
            Console.WriteLine("\n🎉 Done! Copy this file to your Blazor project's Data folder.");

            // Statistiken anzeigen
            ShowStatistics(countries);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static CountryData ConvertToOurFormat(ApiCountry api)
    {
        return new CountryData
        {
            Code = api.cca2 ?? "",
            Flag = api.flag ?? GetFlagFromCode(api.cca2),
            PhoneCode = GetPhoneCode(api.idd), // Neu!
            Names = new CountryNames
            {
                German = GetTranslation(api.translations?.deu?.common) ?? api.name?.common ?? "",
                English = api.name?.common ?? "",
                French = GetTranslation(api.translations?.fra?.common) ?? api.name?.common ?? "",
                Italian = GetTranslation(api.translations?.ita?.common) ?? api.name?.common ?? "",
                Spanish = GetTranslation(api.translations?.spa?.common) ?? api.name?.common ?? ""
            },
            Regions = new CountryRegions
            {
                Continent = new RegionNames
                {
                    German = TranslateRegion(api.continents?.FirstOrDefault() ?? "", "de"),
                    English = api.continents?.FirstOrDefault() ?? "",
                    French = TranslateRegion(api.continents?.FirstOrDefault() ?? "", "fr"),
                    Italian = TranslateRegion(api.continents?.FirstOrDefault() ?? "", "it"),
                    Spanish = TranslateRegion(api.continents?.FirstOrDefault() ?? "", "es")
                },
                Region = new RegionNames
                {
                    German = TranslateRegion(api.region ?? "", "de"),
                    English = api.region ?? "",
                    French = TranslateRegion(api.region ?? "", "fr"),
                    Italian = TranslateRegion(api.region ?? "", "it"),
                    Spanish = TranslateRegion(api.region ?? "", "es")
                },
                SubRegion = new RegionNames
                {
                    German = TranslateRegion(api.subregion ?? "", "de"),
                    English = api.subregion ?? "",
                    French = TranslateRegion(api.subregion ?? "", "fr"),
                    Italian = TranslateRegion(api.subregion ?? "", "it"),
                    Spanish = TranslateRegion(api.subregion ?? "", "es")
                }
            }
        };
    }

    static string GetPhoneCode(ApiIdd? idd)
    {
        if (idd?.root == null) return "";

        var root = idd.root;
        var suffix = idd.suffixes?.FirstOrDefault() ?? "";

        return root + suffix;
    }

    static string GetTranslation(string? translation)
    {
        return string.IsNullOrWhiteSpace(translation) ? null : translation;
    }

    static string GetFlagFromCode(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode) || countryCode.Length != 2)
            return "🏳️";

        // Country Code zu Flag Emoji
        return string.Concat(countryCode.ToUpper().Select(c =>
            char.ConvertFromUtf32(0x1F1E6 + c - 'A')));
    }

    static string TranslateRegion(string englishRegion, string targetLang)
    {
        var translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["Europe"] = new()
            {
                ["de"] = "Europa",
                ["fr"] = "Europe",
                ["it"] = "Europa",
                ["es"] = "Europa"
            },
            ["Asia"] = new()
            {
                ["de"] = "Asien",
                ["fr"] = "Asie",
                ["it"] = "Asia",
                ["es"] = "Asia"
            },
            ["Africa"] = new()
            {
                ["de"] = "Afrika",
                ["fr"] = "Afrique",
                ["it"] = "Africa",
                ["es"] = "África"
            },
            ["North America"] = new()
            {
                ["de"] = "Nordamerika",
                ["fr"] = "Amérique du Nord",
                ["it"] = "Nord America",
                ["es"] = "América del Norte"
            },
            ["South America"] = new()
            {
                ["de"] = "Südamerika",
                ["fr"] = "Amérique du Sud",
                ["it"] = "Sud America",
                ["es"] = "América del Sur"
            },
            ["Oceania"] = new()
            {
                ["de"] = "Ozeanien",
                ["fr"] = "Océanie",
                ["it"] = "Oceania",
                ["es"] = "Oceanía"
            },
            ["Antarctica"] = new()
            {
                ["de"] = "Antarktis",
                ["fr"] = "Antarctique",
                ["it"] = "Antartide",
                ["es"] = "Antártida"
            },
            ["Western Europe"] = new()
            {
                ["de"] = "Westeuropa",
                ["fr"] = "Europe occidentale",
                ["it"] = "Europa occidentale",
                ["es"] = "Europa occidental"
            },
            ["Northern Europe"] = new()
            {
                ["de"] = "Nordeuropa",
                ["fr"] = "Europe du Nord",
                ["it"] = "Europa settentrionale",
                ["es"] = "Europa del Norte"
            },
            ["Southern Europe"] = new()
            {
                ["de"] = "Südeuropa",
                ["fr"] = "Europe du Sud",
                ["it"] = "Europa meridionale",
                ["es"] = "Europa del Sur"
            },
            ["Eastern Europe"] = new()
            {
                ["de"] = "Osteuropa",
                ["fr"] = "Europe de l'Est",
                ["it"] = "Europa orientale",
                ["es"] = "Europa Oriental"
            },
            ["Central Europe"] = new()
            {
                ["de"] = "Mitteleuropa",
                ["fr"] = "Europe centrale",
                ["it"] = "Europa centrale",
                ["es"] = "Europa central"
            },
            ["Southeast Europe"] = new()
            {
                ["de"] = "Südosteuropa",
                ["fr"] = "Europe du Sud-Est",
                ["it"] = "Europa sud-orientale",
                ["es"] = "Europa Sudoriental"
            },
            ["Eastern Asia"] = new()
            {
                ["de"] = "Ostasien",
                ["fr"] = "Asie de l'Est",
                ["it"] = "Asia orientale",
                ["es"] = "Asia Oriental"
            },
            ["South-Eastern Asia"] = new()
            {
                ["de"] = "Südostasien",
                ["fr"] = "Asie du Sud-Est",
                ["it"] = "Asia sud-orientale",
                ["es"] = "Sudeste Asiático"
            },
            ["Southern Asia"] = new()
            {
                ["de"] = "Südasien",
                ["fr"] = "Asie du Sud",
                ["it"] = "Asia meridionale",
                ["es"] = "Asia del Sur"
            },
            ["Western Asia"] = new()
            {
                ["de"] = "Westasien",
                ["fr"] = "Asie occidentale",
                ["it"] = "Asia occidentale",
                ["es"] = "Asia Occidental"
            },
            ["Central Asia"] = new()
            {
                ["de"] = "Zentralasien",
                ["fr"] = "Asie centrale",
                ["it"] = "Asia centrale",
                ["es"] = "Asia Central"
            },
            ["Caribbean"] = new()
            {
                ["de"] = "Karibik",
                ["fr"] = "Caraïbes",
                ["it"] = "Caraibi",
                ["es"] = "Caribe"
            },
            ["Central America"] = new()
            {
                ["de"] = "Mittelamerika",
                ["fr"] = "Amérique centrale",
                ["it"] = "America centrale",
                ["es"] = "América Central"
            },
            ["Northern America"] = new()
            {
                ["de"] = "Nordamerika",
                ["fr"] = "Amérique du Nord",
                ["it"] = "America settentrionale",
                ["es"] = "América del Norte"
            }
        };

        if (translations.TryGetValue(englishRegion, out var regionTranslations) &&
            regionTranslations.TryGetValue(targetLang, out var translation))
        {
            return translation;
        }

        return englishRegion; // Fallback to English
    }

    static void ShowStatistics(List<CountryData> countries)
    {
        Console.WriteLine("\n📈 Statistics:");

        var continents = countries
            .GroupBy(c => c.Regions.Continent.English)
            .OrderByDescending(g => g.Count())
            .Take(10);

        foreach (var continent in continents)
        {
            Console.WriteLine($"   {continent.Key}: {continent.Count()} countries");
        }
    }
}

// ===== API MODELS (REST Countries Response) =====
public class ApiCountry
{
    public string? cca2 { get; set; }
    public string? flag { get; set; }
    public ApiName? name { get; set; }
    public ApiTranslations? translations { get; set; }
    public string? region { get; set; }
    public string? subregion { get; set; }
    public string[]? continents { get; set; }
    public ApiIdd? idd { get; set; } // Phone codes
}

public class ApiName
{
    public string? common { get; set; }
    public string? official { get; set; }
}

public class ApiTranslations
{
    public ApiName? deu { get; set; }
    public ApiName? fra { get; set; }
    public ApiName? ita { get; set; }
    public ApiName? spa { get; set; }
}

public class ApiIdd
{
    public string? root { get; set; }    // z.B. "+4"
    public string[]? suffixes { get; set; } // z.B. ["9"] für Deutschland
}

// ===== OUTPUT MODELS (Unser Format) =====
public class CountryData
{
    public string Code { get; set; } = "";
    public string Flag { get; set; } = "";
    public string PhoneCode { get; set; } = ""; // Neu!
    public CountryNames Names { get; set; } = new();
    public CountryRegions Regions { get; set; } = new();
}

public class CountryNames
{
    public string German { get; set; } = "";
    public string English { get; set; } = "";
    public string French { get; set; } = "";
    public string Italian { get; set; } = "";
    public string Spanish { get; set; } = "";
}

public class CountryRegions
{
    public RegionNames Continent { get; set; } = new();
    public RegionNames Region { get; set; } = new();
    public RegionNames SubRegion { get; set; } = new();
}

public class RegionNames
{
    public string German { get; set; } = "";
    public string English { get; set; } = "";
    public string French { get; set; } = "";
    public string Italian { get; set; } = "";
    public string Spanish { get; set; } = "";
}

// ===== ALTERNATIVE: ALS EINZELNE DATEI ZUM AUSFÜHREN =====
/*
Speichern Sie alles in eine Datei namens "GenerateCountries.cs" und führen Sie aus:

dotnet script GenerateCountries.cs

Oder erstellen Sie ein kleines Projekt:
dotnet new console -n CountryGenerator
cd CountryGenerator
// Code einfügen in Program.cs
dotnet run
*/