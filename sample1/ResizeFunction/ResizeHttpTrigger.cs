using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ResizeFunctionApp
{
    public static class ResizeHttpTrigger
    {
        [FunctionName("ResizeHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                // Lire les dimensions
                if (!int.TryParse(req.Query["w"], out int width) || !int.TryParse(req.Query["h"], out int height))
                {
                    return new BadRequestObjectResult("Paramètres w et h invalides");
                }

                // Lire le body de la requête
                using var reader = new StreamReader(req.Body);
                var bodyString = await reader.ReadToEndAsync();

                // Parser le JSON contenant le champ "$content"
                var json = JsonNode.Parse(bodyString);
                var base64String = json?["$content"]?.ToString();

                if (string.IsNullOrEmpty(base64String))
                {
                    return new BadRequestObjectResult("Aucune image trouvée dans le corps de la requête");
                }

                // Décoder le contenu base64 en bytes
                byte[] imageBytes = Convert.FromBase64String(base64String);

                // Traitement de l’image
                byte[] resizedImageBytes;
                using (var inputStream = new MemoryStream(imageBytes))
                using (var image = Image.Load(inputStream))
                {
                    image.Mutate(x => x.Resize(width, height));
                    using var outputStream = new MemoryStream();
                    image.SaveAsJpeg(outputStream);
                    resizedImageBytes = outputStream.ToArray();
                }

                // Réponse finale
                return new FileContentResult(resizedImageBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Erreur lors du traitement");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
