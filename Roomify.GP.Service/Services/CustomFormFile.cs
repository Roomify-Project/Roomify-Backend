using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Roomify.GP.Service.Services
{
    public class CustomFormFile : IFormFile
    {
        private readonly Stream _stream;
        private readonly string _name;
        private readonly string _fileName;

        public CustomFormFile(Stream stream, string fileName, string contentType)
        {
            _stream = stream;
            _fileName = fileName;
            _name = "file";
            ContentType = contentType;
            Length = stream.Length;
            Headers = new HeaderDictionary();
        }

        public string ContentType { get; set; }

        public string ContentDisposition => $"form-data; name=\"{_name}\"; filename=\"{_fileName}\"";

        public IHeaderDictionary Headers { get; set; }

        public long Length { get; }

        public string Name => _name;

        public string FileName => _fileName;

        public Stream OpenReadStream()
        {
            _stream.Position = 0;
            return _stream;
        }

        public void CopyTo(Stream target)
        {
            _stream.Position = 0;
            _stream.CopyTo(target);
        }

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            _stream.Position = 0;
            await _stream.CopyToAsync(target, cancellationToken);
        }
    }
}