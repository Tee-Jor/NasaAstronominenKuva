namespace NasaAstronominenKuva;

using System.Net.Http.Headers; // Tarvitaan, jotta voidaan asettaa HTTP-pyyntöön otsikot (esim. Accept)
using System.Text.Json;        // Tarvitaan JSON-datan käsittelyyn (NASA:n API palauttaa JSON-muotoista tietoa)
using System.IO;               // Tarvitaan tiedostojen ja kansioiden käsittelyyn (kuvan tallennus levylle)

// Staattinen luokka, johon on kerätty kaikki yhteyden ja kuvien hakemisen metodit
public static class Connection
{
    /// <summary>
    /// Luo HttpClient-olion, asettaa pyynnön otsikot ja hakee NASA:n "Picture of the Day".
    /// Tämä on tavallaan ohjelman aloituspiste yhteyden luomiselle.
    /// </summary>
    public static async Task Connect()
    {
        // using varmistaa, että client suljetaan käytön jälkeen oikein
        using (HttpClient client = new HttpClient())
        {
            // Tyhjennetään oletusotsikot
            client.DefaultRequestHeaders.Accept.Clear();
            // Lisätään otsikko, jolla kerrotaan että halutaan JSON-dataa
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Kutsutaan metodia, joka hakee tämän päivän kuvan
            await PictureOfDay(client);
        }
    }

    /// <summary>
    /// Hakee NASA:n Astronomy Picture of the Day -kuvan ja tallentaa sen työpöydälle.
    /// </summary>
    public static async Task PictureOfDay(HttpClient client)
    {
        // Tehdään GET-pyyntö NASA:n APOD-rajapintaan
        var response = await client.GetAsync("https://api.nasa.gov/planetary/apod?api_key=OcdSExVCql7aUDejkztCjrpk8YIb2FxctVXlrnii");

        // Tarkistetaan, onnistuiko pyyntö (esim. saatiin status 200 OK)
        if (response.IsSuccessStatusCode)
        {
            // Luetaan vastaus JSON-merkkijonona
            var json = await response.Content.ReadAsStringAsync();

            // Muutetaan JSON-teksti JsonDocument-olioksi
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Yritetään hakea JSON:sta "url" (kuvan osoite) ja "date" (päivämäärä)
            if (root.TryGetProperty("url", out var urlElement) &&
                root.TryGetProperty("date", out var dateElement))
            {
                // Muutetaan elementit merkkijonoiksi
                var imageUrl = urlElement.GetString();
                var dateStr = dateElement.GetString(); // Päivämäärä muodossa "yyyy-MM-dd"

                // Varmistetaan, että molemmat arvot löytyivät
                if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(dateStr))
                {
                    // Ladataan kuvatiedosto tavutaulukkona
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);

                    // Selvitetään työpöydän polku
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                    // Muutetaan päivämäärä DateTime-muotoon
                    DateTime date = DateTime.Parse(dateStr);

                    // Tehdään kansiopolku: esim. Desktop/Nasa/2025/09
                    string folderPath = Path.Combine(desktop, "Nasa", date.ToString("yyyy"), date.ToString("MM"));
                    Directory.CreateDirectory(folderPath); // Luo kansion, jos sitä ei vielä ole

                    // Selvitetään tiedostopääte (jos puuttuu, käytetään .jpg)
                    var extension = Path.GetExtension(imageUrl);
                    if (string.IsNullOrEmpty(extension)) extension = ".jpg";

                    // Tiedostonimeksi päivämäärä (esim. "2025-09-25.jpg")
                    string fileName = $"{date:yyyy-MM-dd}{extension}";

                    // Yhdistetään polku ja tiedostonimi
                    string fullPath = Path.Combine(folderPath, fileName);

                    // Tallennetaan kuva tiedostoon
                    await File.WriteAllBytesAsync(fullPath, imageBytes);

                    // Ilmoitetaan käyttäjälle, minne kuva tallennettiin
                    Console.WriteLine($"Kuva tallennettu tiedostoon: {fullPath}");
                }
                else
                {
                    Console.WriteLine("Kuvan URL tai päivämäärä ei löytynyt.");
                }
            }
            else
            {
                Console.WriteLine("JSON:sta ei löytynyt 'url' tai 'date' kenttää.");
            }
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Hakee eilisen Astronomy Picture of the Day -kuvan.
    /// </summary>
    public static async Task PictureOfYesterDay(HttpClient client)
    {
        // Muodostetaan eilisen päivämäärä oikeassa muodossa
        string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

        // Tehdään API-pyyntö eilisen päivämäärällä
        string apiUrl = $"https://api.nasa.gov/planetary/apod?api_key=OcdSExVCql7aUDejkztCjrpk8YIb2FxctVXlrnii&date={yesterday}";

        var response = await client.GetAsync(apiUrl);

        // Alla sama logiikka kuin PictureOfDay-metodissa:
        // JSON:n luku, kuvan osoitteen ja päivämäärän hakeminen,
        // tiedoston tallennus työpöydälle oikeaan kansioon.
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("url", out var urlElement) &&
                root.TryGetProperty("date", out var dateElement))
            {
                var imageUrl = urlElement.GetString();
                var dateStr = dateElement.GetString(); // "yyyy-MM-dd"
                if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(dateStr))
                {
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);

                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    DateTime date = DateTime.Parse(dateStr);
                    string folderPath = Path.Combine(desktop, "Nasa", date.ToString("yyyy"), date.ToString("MM"));
                    Directory.CreateDirectory(folderPath);

                    var extension = Path.GetExtension(imageUrl);
                    if (string.IsNullOrEmpty(extension)) extension = ".jpg";
                    string fileName = $"{date:yyyy-MM-dd}{extension}";
                    string fullPath = Path.Combine(folderPath, fileName);

                    await File.WriteAllBytesAsync(fullPath, imageBytes);
                    Console.WriteLine($"Kuva tallennettu tiedostoon: {fullPath}");
                }
                else
                {
                    Console.WriteLine("Kuvan URL tai päivämäärä ei löytynyt.");
                }
            }
            else
            {
                Console.WriteLine("JSON:sta ei löytynyt 'url' tai 'date' kenttää.");
            }
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Antaa käyttäjän itse syöttää haluamansa päivämäärän ja hakee kyseisen päivän kuvan.
    /// </summary>
    public static async Task ChoosePicture(HttpClient client)
    {
        Console.WriteLine("Anna päivämäärä muodossa yyyy-mm-dd (esim. 2021-06-12):");
        string? day = Console.ReadLine();

        // Jos käyttäjä ei syöttänyt mitään, lopetetaan
        if (string.IsNullOrWhiteSpace(day))
        {
            Console.WriteLine("Päivämäärä ei kelpaa.");
            return;
        }

        // Yritetään muuttaa syöte DateTime-muotoon
        if (!DateTime.TryParse(day, out DateTime ChoosenDate))
        {
            Console.WriteLine("Virheellinen päivämäärämuoto.");
            return;
        }

        // Rakennetaan API-pyyntö valitulle päivämäärälle
        string apiUrl = $"https://api.nasa.gov/planetary/apod?api_key=OcdSExVCql7aUDejkztCjrpk8YIb2FxctVXlrnii&date={ChoosenDate:yyyy-MM-dd}";

        var response = await client.GetAsync(apiUrl);

        // Sama logiikka kuin muissa metodeissa: JSON:n käsittely ja kuvan tallennus
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("url", out var urlElement) &&
                root.TryGetProperty("date", out var dateElement))
            {
                var imageUrl = urlElement.GetString();
                var dateStr = dateElement.GetString(); 
                if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(dateStr))
                {
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);

                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    DateTime date = DateTime.Parse(dateStr);
                    string folderPath = Path.Combine(desktop, "Nasa", date.ToString("yyyy"), date.ToString("MM"));
                    Directory.CreateDirectory(folderPath);

                    var extension = Path.GetExtension(imageUrl);
                    if (string.IsNullOrEmpty(extension)) extension = ".jpg";
                    string fileName = $"{date:yyyy-MM-dd}{extension}";
                    string fullPath = Path.Combine(folderPath, fileName);

                    await File.WriteAllBytesAsync(fullPath, imageBytes);
                    Console.WriteLine($"Kuva tallennettu tiedostoon: {fullPath}");
                }
                else
                {
                    Console.WriteLine("Kuvan URL tai päivämäärä ei löytynyt.");
                }
            }
            else
            {
                Console.WriteLine("JSON:sta ei löytynyt 'url' tai 'date' kenttää.");
            }
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Hakee satunnaisen päivän kuvan NASA:n APOD-arkistosta (alkaa 16.6.1995).
    /// </summary>
    public static async Task RandomPicture(HttpClient client)
    {
        Random rnd = new Random();

        // APOD-kuvat alkavat päivästä 16.6.1995
        DateTime start = new DateTime(1995, 6, 16);
        DateTime end = DateTime.Today;

        // Lasketaan kuinka monta päivää on välissä
        int range = (end - start).Days;

        // Arvotaan satunnainen päivämäärä
        DateTime randomDate = start.AddDays(rnd.Next(range + 1));
        string randomDay = randomDate.ToString("yyyy-MM-dd");

        // Tehdään API-pyyntö satunnaiselle päivälle
        string apiUrl = $"https://api.nasa.gov/planetary/apod?api_key=OcdSExVCql7aUDejkztCjrpk8YIb2FxctVXlrnii&date={randomDay:yyyy-MM-dd}";

        var response = await client.GetAsync(apiUrl);

        // Sama tallennuslogiikka kuin muissa metodeissa
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("url", out var urlElement) &&
                root.TryGetProperty("date", out var dateElement))
            {
                var imageUrl = urlElement.GetString();
                var dateStr = dateElement.GetString();
                if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(dateStr))
                {
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);

                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    DateTime date = DateTime.Parse(dateStr);
                    string folderPath = Path.Combine(desktop, "Nasa", date.ToString("yyyy"), date.ToString("MM"));
                    Directory.CreateDirectory(folderPath);

                    var extension = Path.GetExtension(imageUrl);
                    if (string.IsNullOrEmpty(extension)) extension = ".jpg";
                    string fileName = $"{date:yyyy-MM-dd}{extension}";
                    string fullPath = Path.Combine(folderPath, fileName);

                    await File.WriteAllBytesAsync(fullPath, imageBytes);
                    Console.WriteLine($"Kuva tallennettu tiedostoon: {fullPath}");
                }
                else
                {
                    Console.WriteLine("Kuvan URL tai päivämäärä ei löytynyt.");
                }
            }
            else
            {
                Console.WriteLine("JSON:sta ei löytynyt 'url' tai 'date' kenttää.");
            }
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }
}
