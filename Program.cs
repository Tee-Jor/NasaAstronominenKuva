namespace NasaAstronominenKuva;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Tervetuloa Nasan päivän astronomiseen kuvaan!");

        using (var client = new HttpClient())

        while (true)
        {
            Console.WriteLine("1. Lataa tämän päivän kuvan?\n2. Lataa eilisen päivän kuva?\n3. Lataa valitun päivän kuva?\n4. Lataa satunnainen kuva?\n5. Poistu?");
            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    await Connection.PictureOfDay(client);
                    break;

                case "2":
                    await Connection.PictureOfYesterDay(client);
                    break;

                case "3":
                    await Connection.ChoosePicture(client);
                    break;

                case "4":
                    await Connection.RandomPicture(client);
                    break;

                case "5":
                    return;

            }
        }
    }
}
