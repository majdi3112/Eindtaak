using System;
using ClientSimulator_DL.Repository;
using ClientSimulatorUpload;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var landRepo = new LandRepository();
        bool doorgaan = true;

        while (doorgaan)
        {
            Console.Clear();
            Console.WriteLine("=====================================");
            Console.WriteLine("      ClientSimulator Upload Tool     ");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            Console.WriteLine("Kies een land om te importeren:");
            Console.WriteLine("1. België");
            Console.WriteLine("2. Denemarken");
            Console.WriteLine("3. Finland");
            Console.WriteLine("4. Polen");
            Console.WriteLine("5. Tsjechië");
            Console.WriteLine("6. Spanje");
            Console.WriteLine("7. Zwitserland");
            Console.WriteLine("8. Zweden");
            Console.WriteLine("0. Afsluiten");
            Console.WriteLine();
            Console.Write("Uw keuze: ");

            string choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            int landId;

            switch (choice)
            {
                case "1":
                    landId = landRepo.InsertOfOphalen("België");
                    new BelgiumImporter(landId).Import();
                    break;

                case "2":
                    landId = landRepo.InsertOfOphalen("Denemarken");
                    new DenmarkImporter(landId).Import();
                    break;

                case "3":
                    landId = landRepo.InsertOfOphalen("Finland");
                    new FinlandImporter(landId).Import();
                    break;

                case "4":
                    landId = landRepo.InsertOfOphalen("Polen");
                    new PolandImporter(landId).Import();
                    break;

                case "5":
                    landId = landRepo.InsertOfOphalen("Tsjechië");
                    new TsjechiëImporter(landId).Import();
                    break;

                case "6":
                    landId = landRepo.InsertOfOphalen("Spanje");
                    new SpainImporter(landId).Import();
                    break;

                case "7":
                    landId = landRepo.InsertOfOphalen("Zwitserland");
                    new SwitzerlandImporter(landId).Import();
                    break;

                case "8":
                    landId = landRepo.InsertOfOphalen("Zweden");
                    new SwedenImporter(landId).Import();
                    break;

                case "0":
                    doorgaan = false;
                    continue;

                default:
                    Console.WriteLine("❌ Ongeldige keuze.");
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("=====================================");
            Console.WriteLine("Import afgerond.");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            Console.Write("Nog een land importeren? (j/n): ");

            string antwoord = Console.ReadLine()?.Trim().ToLower();
            if (antwoord != "j" && antwoord != "ja")
                doorgaan = false;
        }

        Console.WriteLine();
        Console.WriteLine("Programma afgesloten.");
    }
}
