using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ResizeFunctionApp
{
    public static class ResizeHttpTrigger
    {
        [FunctionName("ResizeHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            // Récupération des paramètres w et h de la requête
            if (!int.TryParse(req.Query["w"], out int width) || !int.TryParse(req.Query["h"], out int height))
            {
                return new BadRequestObjectResult("Merci de fournir des valeurs valides pour w et h");
            }

            byte[] imageBytes;
            using (var msInput = new MemoryStream())
            {
                await req.Body.CopyToAsync(msInput);
                imageBytes = msInput.ToArray();
            }

            byte[] targetImageBytes;
            using (var image = Image.Load(imageBytes))
            {
                image.Mutate(x => x.Resize(width, height));

                using (var msOutput = new MemoryStream())
                {
                    image.SaveAsJpeg(msOutput);
                    targetImageBytes = msOutput.ToArray();
                }
            }

            return new FileContentResult(targetImageBytes, "image/jpeg");
        }
    }
}
