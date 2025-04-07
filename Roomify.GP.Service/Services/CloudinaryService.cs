using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Roomify.GP.Core.Service.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Service.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public CloudinaryService(Cloudinary cloudinary, ILogger<CloudinaryService> logger)
        {
            _cloudinary = cloudinary ?? throw new ArgumentNullException(nameof(cloudinary));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetPublicIdFromUrl(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return null;
                var uri = new Uri(imageUrl);
                var pathParts = uri.AbsolutePath.Split('/');
                var filename = pathParts.Last().Split('.')[0]; // Get filename without extension
                return filename;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting public ID from URL: {ImageUrl}", imageUrl);
                return null;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                // Validate file
                ValidateFile(file);

                // Upload process
                _logger.LogInformation("Starting upload for file: {FileName}, Size: {FileSize}", file.FileName, file.Length);

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Crop("fill").Gravity("auto"),
                    UseFilename = true,
                    UniqueFilename = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                // Check for upload errors
                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {ErrorMessage}", uploadResult.Error.Message);
                    throw new ApplicationException($"Image upload failed: {uploadResult.Error.Message}");
                }

                // Validate result
                if (uploadResult.SecureUrl == null)
                {
                    _logger.LogError("Upload completed but secure URL is null");
                    throw new ApplicationException("Image upload completed but secure URL is missing");
                }

                _logger.LogInformation("Upload successful: {PublicId}, URL: {Url}",
                    uploadResult.PublicId, uploadResult.SecureUrl.AbsoluteUri);

                return uploadResult.SecureUrl.AbsoluteUri;
            }
            catch (Exception ex) when (!(ex is ApplicationException))
            {
                _logger.LogError(ex, "Unexpected error during image upload");
                throw new ApplicationException("An unexpected error occurred during image upload", ex);
            }
        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null)
            {
                throw new ArgumentException("No file was provided");
            }

            if (file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }

            if (file.Length > _maxFileSize)
            {
                throw new ArgumentException($"File size exceeds the limit of {_maxFileSize / 1024 / 1024}MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    $"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogWarning("Attempted to delete null or empty image URL");
                    return;
                }

                var publicId = GetPublicIdFromUrl(imageUrl);
                if (string.IsNullOrEmpty(publicId))
                {
                    _logger.LogWarning("Could not extract public ID from URL: {ImageUrl}", imageUrl);
                    return;
                }

                _logger.LogInformation("Deleting image with public ID: {PublicId}", publicId);

                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Error != null)
                {
                    _logger.LogError("Error deleting image: {ErrorMessage}", result.Error.Message);
                    throw new ApplicationException($"Failed to delete image: {result.Error.Message}");
                }

                _logger.LogInformation("Image deleted successfully: {PublicId}", publicId);
            }
            catch (Exception ex) when (!(ex is ApplicationException))
            {
                _logger.LogError(ex, "Unexpected error during image deletion: {ImageUrl}", imageUrl);
                throw new ApplicationException("An unexpected error occurred during image deletion", ex);
            }
        }
    }
}