using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Service.Contract;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Roomify.GP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
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

            // Configure HttpClient for Replicate API
            var replicateApiToken = _configuration["AppSettings:ReplicateApiToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", replicateApiToken);
        }

        // Generate designs using Replicate API
        [HttpPost("generate-design")]
        public async Task<IActionResult> GenerateDesign(
            [FromForm] IFormFile image,
            [FromForm] string descriptionText,
            [FromForm] RoomStyle roomStyle,
            [FromForm] RoomType roomType,
            [FromForm] Guid userId,
            [FromForm] bool saveToHistory = true)
        {
            try
            {
                // Validate inputs
                if (image == null || image.Length == 0)
                    return BadRequest(new { Status = "Error", Message = "An image file is required" });

                if (string.IsNullOrEmpty(descriptionText))
                    return BadRequest(new { Status = "Error", Message = "Description text is required" });

                // Upload to Cloudinary first to get a URL
                var imageUrl = await _cloudinaryService.UploadImageAsync(image);

                // Generate prompt combining description, style and room type
                var fullPrompt = $"{descriptionText} in {roomStyle.ToString()} style {roomType.ToString()}";

                // Get designs from Replicate
                var designUrls = await GenerateDesignsUsingReplicate(imageUrl, fullPrompt, 3);

                // Save to history if requested
                var historyResults = new List<AIResultHistory>();
                if (saveToHistory && userId != Guid.Empty)
                {
                    foreach (var designUrl in designUrls)
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


        // Generate more designs using the same prompt
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
                // Generate prompt combining description, style and room type
                var fullPrompt = $"{descriptionText} in {roomStyle.ToString()} style {roomType.ToString()}";

                // Send to Replicate API (not HomeDesigns)
                var designs = await GenerateDesignsUsingReplicate(originalImageUrl, fullPrompt, 3);

                // Save to history if requested
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


        // Save a previously generated design to user's saved designs
        [HttpPost("save-design")]
        public async Task<IActionResult> SaveDesign([FromForm] string imageUrl, [FromForm] Guid userId)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(imageUrl))
                    return BadRequest(new { Status = "Error", Message = "Image URL is required" });

                if (userId == Guid.Empty)
                    return BadRequest(new { Status = "Error", Message = "Valid User ID is required" });
                
                // Validate URL format
                if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
                    return BadRequest(new { Status = "Error", Message = "Invalid image URL format" });

                // Saving Image In SavedDesign
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

        // Get user's history of generated designs
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

        // Get user's saved designs
        [HttpGet("saved-designs/{userId}")]
        public async Task<IActionResult> GetSavedDesigns(Guid userId)
        {
            try
            {
                var savedDesigns = await _roomImageService.GetUserSavedDesignsAsync(userId);
                return Ok(new { Status = "Success", SavedDesigns = savedDesigns });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved designs");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while retrieving saved designs" });
            }
        }
        
        // Download an image to user's device
        [HttpGet("download")]
        public async Task<IActionResult> DownloadImage([FromQuery] string imageUrl, [FromQuery] string fileName = "room-design.jpg")
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return BadRequest(new { Status = "Error", Message = "Image URL is required" });

                // Download the image from the URL
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                // Return the file to the client for download
                return File(imageBytes, "image/jpeg", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading image");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while downloading the image" });
            }
        }



        // Helper method to generate designs using Replicate
        private async Task<List<string>> GenerateDesignsUsingReplicate(string imageUrl, string prompt, int numDesigns)
        {
            var designUrls = new List<string>();

            // Recommended model for interior design
            string modelVersion = "adirik/interior-design:76604baddc85b1b4616e1c6475eca080da339c8875bd4996705440484a6eac38";

            for (int i = 0; i < numDesigns; i++)
            {
                // Create prediction request
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

                // Start prediction
                var response = await _httpClient.PostAsync("https://api.replicate.com/v1/predictions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Replicate API error: {errorContent}");
                    throw new Exception($"Replicate API error: {response.StatusCode} - {errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var prediction = JsonConvert.DeserializeObject<ReplicatePrediction>(responseString);

                // Poll for results
                string status = prediction.status;
                string resultUrl = "";

                while (status == "starting" || status == "processing")
                {
                    await Task.Delay(1000); // Wait 1 second between polls

                    var statusResponse = await _httpClient.GetAsync($"https://api.replicate.com/v1/predictions/{prediction.id}");
                    var statusResponseString = await statusResponse.Content.ReadAsStringAsync();

                    // Log the raw response to see its format
                    _logger.LogInformation($"Replicate API response: {statusResponseString}");

                    var statusPrediction = JsonConvert.DeserializeObject<ReplicatePrediction>(statusResponseString);

                    status = statusPrediction.status;

                    if (status == "succeeded")
                    {
                        // Handle both cases: output could be a list or a single string
                        if (statusPrediction.output is string outputString)
                        {
                            resultUrl = outputString;
                        }
                        else if (statusPrediction.output != null)
                        {
                            // Try to get first item if it's a list
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

        // Updated Replicate API response class
        private class ReplicatePrediction
        {
            public string id { get; set; }
            public string status { get; set; }
            public object output { get; set; }
            public string error { get; set; }
        }

    }
}