using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.Repositories.Contract;

namespace Roomify.GP.API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = false)]
    [Route("api/[controller]")]
    [ApiController]
    public class RoomImageController : ControllerBase
    {
        private readonly IRoomImageService _roomImageService;

        public RoomImageController(IRoomImageService roomImageService)
        {
            _roomImageService = roomImageService;
        }


        // Sending The User Inputs To AI Model 
        [HttpPost("generate-design")]
        public async Task<IActionResult> GenerateDesign([FromForm] IFormFile image, [FromForm] string descriptionText, [FromForm] RoomStyle roomStyle, [FromForm] RoomType roomType)
        {
            try
            {
                // إرسال الصورة والنص إلى FastAPI مباشرة دون حفظها على السيرفر
                var aiResult = await SendToAIAsync(image, descriptionText, roomStyle, roomType);

                return Ok(new { Status = "Success", GeneratedImageUrl = aiResult.GeneratedImageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }
        private async Task<AIResult> SendToAIAsync(IFormFile image, string descriptionText, RoomStyle roomStyle, RoomType roomType)
        {
            var client = new HttpClient();
            var form = new MultipartFormDataContent();

            // تحويل الصورة إلى ByteArrayContent
            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                var imageContent = new ByteArrayContent(memoryStream.ToArray());
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                form.Add(imageContent, "image", "room_image.jpg");

                // إضافة الوصف والنص
                form.Add(new StringContent(descriptionText), "description");
                form.Add(new StringContent(roomStyle.ToString()), "roomStyle");
                form.Add(new StringContent(roomType.ToString()), "roomType");

                // إرسال البيانات إلى FastAPI
                var response = await client.PostAsync("http://localhost:8080/generate-design", form);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AIResult>(content);
            }
        }



        // Adding The Generated Images Result To History
        [HttpPost("generated-history")]
        public async Task<IActionResult> GeneratedHistory([FromForm] IFormFile image, [FromForm] string descriptionText, [FromForm] RoomStyle roomStyle, [FromForm] RoomType roomType, [FromForm] Guid userId)
        {
            try
            {
                // Sending Result Image To FastAPI
                var aiResult = await SendToAIAsync(image, descriptionText, roomStyle, roomType);

                // Saving The Result In AIResultHistory
                await _roomImageService.SaveAIResultHistoryAsync(aiResult.GeneratedImageUrl, userId);

                return Ok(new { Status = "Success", GeneratedImageUrl = aiResult.GeneratedImageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        // The User SavedDesigns Generated From AI
        [HttpPost("save-design")]
        public async Task<IActionResult> SaveDesign([FromForm] string imageUrl, [FromForm] Guid userId)
        {
            try
            {
                // Saving Image In SavedDesign
                var savedDesign = await _roomImageService.SaveUserDesignAsync(imageUrl, userId);
                return Ok(new { Status = "Success", SavedDesign = savedDesign });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }
    }
}
