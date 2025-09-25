namespace NasaAstronominenKuva;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Tervetuloa Nasan päivän astronomiseen kuvaan!");

        while (true)
        {
            Console.WriteLine("1. Lataa tämän tai eilisen päivän astronomisen kuvan?\n2. Ladata valitun päivän astronomisen kuvan?\n.3. Ladata satunnaisen päivän astronomisen kuvan?\n4. Poistua?");
            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    break;

                case "2":
                    break;

                case "3":
                    break;

                case "4":
                    break;

            }
        }
    }
}
