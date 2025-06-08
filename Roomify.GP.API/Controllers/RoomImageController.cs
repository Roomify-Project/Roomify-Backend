using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Service.Contract;
using System.Net.Http.Headers;
using Roomify.GP.Core.DTOs.GenerateDesign;

namespace Roomify.GP.API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoomImageController : ControllerBase
    {
        private readonly IRoomImageService _roomImageService;
        private readonly ILogger<RoomImageController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly HttpClient _httpClient;

        public RoomImageController(IRoomImageService roomImageService,
            ILogger<RoomImageController> logger,
            IConfiguration configuration,
            ICloudinaryService cloudinaryService)
        {
            _roomImageService = roomImageService;
            _logger = logger;
            _configuration = configuration;
            _cloudinaryService = cloudinaryService;
            _httpClient = new HttpClient();

            var replicateApiToken = _configuration["AppSettings:ReplicateApiToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", replicateApiToken);
        }

        [HttpPost("generate-design")]
        public async Task<IActionResult> GenerateDesign([FromForm] GenerateDesignRequest request)
        {
            try
            {
                if (request.Image == null || request.Image.Length == 0)
                    return BadRequest(new { Status = "Error", Message = "An image file is required" });

                if (string.IsNullOrEmpty(request.DescriptionText))
                    return BadRequest(new { Status = "Error", Message = "Description text is required" });

                var imageUrl = await _cloudinaryService.UploadImageAsync(request.Image);
                var fullPrompt = $"{request.DescriptionText} in {request.RoomStyle} style {request.RoomType}";

                var designUrls = await GenerateDesignsUsingReplicate(imageUrl, fullPrompt, 3);

                var historyResults = new List<AIResultHistory>();
                if (request.SaveToHistory && request.UserId != Guid.Empty)
                {
                    foreach (var designUrl in designUrls)
                    {
                        var historyResult = await _roomImageService.SaveGeneratedDesignWithPromptAsync(
                            designUrl,
                            request.UserId,
                            request.DescriptionText,
                            request.RoomStyle,
                            request.RoomType);

                        historyResults.Add(historyResult);
                    }
                }

                return Ok(new
                {
                    OriginalRoomImage = imageUrl,
                    Status = "Success",
                    GeneratedImageUrls = designUrls,
                    HistoryResults = historyResults.Select(h => new { h.Id, h.GeneratedImageUrl })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating designs");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while processing your request", Details = ex.Message });
            }
        }

        [HttpPost("generate-more")]
        public async Task<IActionResult> GenerateMoreDesigns(
            [FromForm] string originalImageUrl,
            [FromForm] string descriptionText,
            [FromForm] RoomStyle roomStyle,
            [FromForm] RoomType roomType,
            [FromForm] Guid userId,
            [FromForm] bool saveToHistory = true)
        {
            try
            {
                var fullPrompt = $"{descriptionText} in {roomStyle.ToString()} style {roomType.ToString()}";
                var designs = await GenerateDesignsUsingReplicate(originalImageUrl, fullPrompt, 3);

                var historyResults = new List<AIResultHistory>();
                if (saveToHistory && userId != Guid.Empty)
                {
                    foreach (var designUrl in designs)
                    {
                        var historyResult = await _roomImageService.SaveGeneratedDesignWithPromptAsync(
                            designUrl,
                            userId,
                            descriptionText,
                            roomStyle,
                            roomType);

                        historyResults.Add(historyResult);
                    }
                }

                return Ok(new
                {
                    Status = "Success",
                    GeneratedImageUrls = designs,
                    HistoryResults = historyResults.Select(h => new { h.Id, h.GeneratedImageUrl })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating more designs");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while processing your request", Details = ex.Message });
            }
        }

        [HttpPost("save-design")]
        public async Task<IActionResult> SaveDesign([FromForm] string imageUrl, [FromForm] Guid userId)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return BadRequest(new { Status = "Error", Message = "Image URL is required" });

                if (userId == Guid.Empty)
                    return BadRequest(new { Status = "Error", Message = "Valid User ID is required" });

                if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
                    return BadRequest(new { Status = "Error", Message = "Invalid image URL format" });

                var savedDesign = await _roomImageService.SaveUserDesignAsync(imageUrl, userId);
                return Ok(new { Status = "Success", Message = "Design saved successfully", SavedDesign = savedDesign });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument provided");
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation failed");
                return StatusCode(422, new { Status = "Error", Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving design");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while saving your design" });
            }
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetHistory(Guid userId)
        {
            try
            {
                var history = await _roomImageService.GetUserHistoryAsync(userId);
                return Ok(new { Status = "Success", History = history });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving history");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while retrieving history" });
            }
        }

        [HttpGet("saved-designs/{userId}")]
        public async Task<IActionResult> GetSavedDesigns(Guid userId)
        {
            try
            {
                var savedDesigns = await _roomImageService.GetUserSavedDesignsWithUserInfoAsync(userId);
                return Ok(new { Status = "Success", SavedDesigns = savedDesigns.Select(sd => new
                {
                    Id = sd.Id,
                    GeneratedImageUrl = sd.GeneratedImageUrl,
                    SavedAt = sd.SavedAt,
                    UserId = sd.ApplicationUserId,
                    UserFullName = sd.ApplicationUser?.FullName,
                    UserProfilePicture = sd.ApplicationUser?.ProfilePicture
                })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved designs");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while retrieving saved designs" });
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadImage([FromQuery] string imageUrl, [FromQuery] string fileName = "room-design.jpg")
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return BadRequest(new { Status = "Error", Message = "Image URL is required" });

                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                return File(imageBytes, "image/jpeg", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading image");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while downloading the image" });
            }
        }

        private async Task<List<string>> GenerateDesignsUsingReplicate(string imageUrl, string prompt, int numDesigns)
        {
            var designUrls = new List<string>();
            string modelVersion = "adirik/interior-design:76604baddc85b1b4616e1c6475eca080da339c8875bd4996705440484a6eac38";

            for (int i = 0; i < numDesigns; i++)
            {
                var predictionRequest = new
                {
                    version = modelVersion,
                    input = new
                    {
                        image = imageUrl,
                        prompt = prompt,
                        a_prompt = "best quality, interior design, detailed, realistic",
                        n_prompt = "longbody, lowres, bad anatomy, bad hands, missing fingers, duplicate objects, blurry"
                    }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(predictionRequest),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("https://api.replicate.com/v1/predictions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Replicate API error: {errorContent}");
                    throw new Exception($"Replicate API error: {response.StatusCode} - {errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var prediction = JsonConvert.DeserializeObject<ReplicatePrediction>(responseString);

                string status = prediction.status;
                string resultUrl = "";

                while (status == "starting" || status == "processing")
                {
                    await Task.Delay(1000);

                    var statusResponse = await _httpClient.GetAsync($"https://api.replicate.com/v1/predictions/{prediction.id}");
                    var statusResponseString = await statusResponse.Content.ReadAsStringAsync();

                    _logger.LogInformation($"Replicate API response: {statusResponseString}");

                    var statusPrediction = JsonConvert.DeserializeObject<ReplicatePrediction>(statusResponseString);
                    status = statusPrediction.status;

                    if (status == "succeeded")
                    {
                        if (statusPrediction.output is string outputString)
                        {
                            resultUrl = outputString;
                        }
                        else if (statusPrediction.output != null)
                        {
                            var outputList = statusPrediction.output as Newtonsoft.Json.Linq.JArray;
                            if (outputList != null && outputList.Count > 0)
                            {
                                resultUrl = outputList[0].ToString();
                            }
                        }
                        break;
                    }
                    else if (status == "failed")
                    {
                        throw new Exception($"Replicate prediction failed: {statusPrediction.error}");
                    }
                }

                if (!string.IsNullOrEmpty(resultUrl))
                {
                    designUrls.Add(resultUrl);
                }
            }

            return designUrls;
        }

        private class ReplicatePrediction
        {
            public string id { get; set; }
            public string status { get; set; }
            public object output { get; set; }
            public string error { get; set; }
        }
    }
}
