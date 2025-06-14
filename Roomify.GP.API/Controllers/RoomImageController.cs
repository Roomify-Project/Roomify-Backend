using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Service.Contract;
using System.Net.Http.Headers;
using Roomify.GP.Core.DTOs.GenerateDesign;
using Roomify.GP.Service.Services;

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

            // Set timeout to 5 minutes for AI model calls (300 seconds)
            _httpClient.Timeout = TimeSpan.FromMinutes(5);  //changed to 10min
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
                var fullPrompt = $"<interiorx>{request.DescriptionText} in {request.RoomType} with {request.RoomStyle} style, professional interior design, realistic lighting, detailed furniture and decor, photorealistic, 8K resolution, well-composed shot, interior design magazine quality";

                // Use Azure model instead of Replicate
                var designUrls = await GenerateDesignsUsingAzure(request.Image, fullPrompt, 1);

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
                var fullPrompt = $"<interiorx>{descriptionText} in  {roomType.ToString()} with {roomStyle.ToString()} style";

                // Download the original image to pass to Azure model
                var imageBytes = await _httpClient.GetByteArrayAsync(originalImageUrl);
                var imageFile = new CustomFormFile(new MemoryStream(imageBytes), "image.jpg", "image/jpeg");

                var designs = await GenerateDesignsUsingAzure(imageFile, fullPrompt, 1);

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


        // NEW METHOD: Replace Replicate with Azure model
        private async Task<List<string>> GenerateDesignsUsingAzure(IFormFile imageFile, string prompt, int numDesigns)
        {
            var designUrls = new List<string>();

            for (int i = 0; i < numDesigns; i++)
            {
                try
                {
                    // Create a separate HttpClient with longer timeout for Azure model calls
                    using var azureHttpClient = new HttpClient();
                    azureHttpClient.Timeout = TimeSpan.FromMinutes(15); // 15 minutes timeout

                    using var multipartContent = new MultipartFormDataContent();

                    // Add image file
                    var imageContent = new StreamContent(imageFile.OpenReadStream());
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                    multipartContent.Add(imageContent, "image", imageFile.FileName);

                    // Add prompt
                    multipartContent.Add(new StringContent(prompt), "prompt");

                    // Add fixed parameters (steps=15, size=384)
                    multipartContent.Add(new StringContent("15"), "steps");
                    multipartContent.Add(new StringContent("384"), "size");

                    // Log the start of the request
                    _logger.LogInformation($"Starting Azure model request for design {i + 1}");

                    // Call your Azure model
                    var response = await _httpClient.PostAsync("https://roomify.azurewebsites.net/generate", multipartContent);
                    _logger.LogInformation($"Azure model response received for design {i + 1}");
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Azure model API error: {errorContent}");
                        throw new Exception($"Azure model API error: {response.StatusCode} - {errorContent}");
                    }

                    // Handle the response based on content type
                    var contentType = response.Content.Headers.ContentType?.MediaType;

                    if (contentType == "application/json")
                    {
                        // If Azure returns JSON with image URL
                        var responseString = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<AzureModelResponse>(responseString);

                        if (!string.IsNullOrEmpty(result?.ImageUrl))
                        {
                            designUrls.Add(result.ImageUrl);
                        }
                        else if (!string.IsNullOrEmpty(result?.Output))
                        {
                            designUrls.Add(result.Output);
                        }
                    }
                    else
                    {
                        // If Azure returns image directly, upload it to Cloudinary
                        var imageBytes = await response.Content.ReadAsByteArrayAsync();
                        var tempFile = new CustomFormFile(new MemoryStream(imageBytes), $"generated-design-{i}.jpg", "image/jpeg");
                        var cloudinaryUrl = await _cloudinaryService.UploadImageAsync(tempFile);

                        if (!string.IsNullOrEmpty(cloudinaryUrl))
                        {
                            designUrls.Add(cloudinaryUrl);
                        }
                    }
                }
                catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, $"Timeout occurred while generating design {i + 1}. Consider increasing the timeout or optimizing the AI model.");
                    // Continue with other designs even if one times out
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error generating design {i + 1}");
                    // Continue with other designs even if one fails
                }
            }

            return designUrls;
        }


        // Helper class for Azure model response
        private class AzureModelResponse
        {
            [JsonProperty("image_url")]
            public string ImageUrl { get; set; }

            [JsonProperty("output")]
            public string Output { get; set; }

            [JsonProperty("success")]
            public bool Success { get; set; }
        }

        // Keep the old ReplicatePrediction class for compatibility (can be removed later)
        private class ReplicatePrediction
        {
            public string id { get; set; }
            public string status { get; set; }
            public object output { get; set; }
            public string error { get; set; }
        }
    }
}
