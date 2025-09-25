namespace NasaAstronominenKuva;

using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;
public static class Connection
{

    public static async Task Connect()
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            await PictureOfDay(client);
        }
    }

    public static async Task PictureOfDay(HttpClient client)
    {
        var response = await client.GetAsync("https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("url", out var urlElement) &&
                root.TryGetProperty("date", out var dateElement))
            {
                var imageUrl = urlElement.GetString();
                var dateStr = dateElement.GetString(); // muotoa "yyyy-mm-dd"
                if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(dateStr))
                {
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);

                    // Muodosta polku työpöydän Nasa-kansioon
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    DateTime date = DateTime.Parse(dateStr);
                    string folderPath = Path.Combine(desktop, "Nasa", date.ToString("yyyy"), date.ToString("MM"));
                    Directory.CreateDirectory(folderPath);

                    // Tiedostonimi muodossa yyyy-mm-dd.jpg
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
            return;
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }
   
   public static async Task PictureOfYesterDay(HttpClient client)
    {
        string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        string apiUrl = $"https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY&date={yesterday}";



        var response = await client.GetAsync(apiUrl);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("url", out var urlElement) &&
                root.TryGetProperty("date", out var dateElement))
            {
                var imageUrl = urlElement.GetString();
                var dateStr = dateElement.GetString(); // muotoa "yyyy-mm-dd"
                if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(dateStr))
                {
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);

                    // Muodosta polku työpöydän Nasa-kansioon
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    DateTime date = DateTime.Parse(dateStr);
                    string folderPath = Path.Combine(desktop, "Nasa", date.ToString("yyyy"), date.ToString("MM"));
                    Directory.CreateDirectory(folderPath);

                    // Tiedostonimi muodossa yyyy-mm-dd.jpg
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
            return;
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }

   

    
}