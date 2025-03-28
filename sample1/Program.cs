using Newtonsoft.Json;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace sample1;

class Personne
{
    public string Nom { get; set; }
    public int Age { get; set; }

    // Constructeur de la classe
    public Personne(string nom, int age)
    {
        Nom = nom;
        Age = age;
    }

    // Methode optionnelle Hello(bool isLowercase)
    public string Hello(bool isLowercase)
    {
        string message = $"hello {Nom}, you are {Age}";
        return isLowercase ? message : message.ToUpper();
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Creation d'une instance de Personne
        Personne personne = new Personne("Damien", 23);

        // Sérialiser l'objet Personne en JSON (avec indentation)
        string json = JsonConvert.SerializeObject(personne, Formatting.Indented);

        // Affichage a l'ecran en utilisant la methode Hello
        Console.WriteLine(personne.Hello(true));

        // Afficher le résultat JSON à l'écran
        Console.WriteLine(json);

        var inputDir = "input_images";
        var outputDir = "output_images";

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        string[] images = Directory.GetFiles(inputDir, "*.jpg");

        Stopwatch stopwatch = Stopwatch.StartNew();

        Parallel.ForEach(images, imagePath =>
        {
            using (Image image = Image.Load(imagePath))
            {
                image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));

                string outputPath = Path.Combine(outputDir, Path.GetFileName(imagePath));
                image.Save(outputPath);

                Console.WriteLine($"Image traitée : {outputPath}");
            }
        });

        stopwatch.Stop();

        Console.WriteLine($"Temps total de traitement parallèle : {stopwatch.ElapsedMilliseconds} ms");
    }
}
