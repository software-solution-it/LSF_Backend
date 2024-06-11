using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly APIDbContext _dbContext;
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public FileController(APIDbContext dbContext, IAmazonS3 s3Client, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _s3Client = s3Client;
            _bucketName = configuration["AWS:BucketName"];
        }

        [HttpGet("GetAll")]
        public IEnumerable<ProductDomain> GetAll()
        {
            return _dbContext.Product_Domain.ToList();
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile([FromForm] string fileName, [FromForm] string folderName, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var key = string.IsNullOrWhiteSpace(folderName) ? fileName : $"{folderName}/{fileName}";

            using (var stream = file.OpenReadStream())
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = key,
                    BucketName = _bucketName,
                    CannedACL = S3CannedACL.Private
                };

                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            return Ok(new { fileName, Message = "File uploaded successfully", Path = key });
        }

        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFile([FromQuery] string folder, [FromQuery] string imageName)
        {
            var key = string.IsNullOrWhiteSpace(folder) ? imageName : $"{folder}/{imageName}";
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            using var response = await _s3Client.GetObjectAsync(request);
            await using var responseStream = response.ResponseStream;
            var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);

            return File(memoryStream.ToArray(), response.Headers["Content-Type"], imageName);
        }

        [HttpGet("GeneratePresignedUrl")]
        public IActionResult GeneratePresignedUrl([FromQuery] string folder, [FromQuery] string imageName)
        {
            var key = string.IsNullOrWhiteSpace(folder) ? imageName : $"{folder}/{imageName}";
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(3),
                Verb = HttpVerb.GET
            };

            var url = _s3Client.GetPreSignedURL(request);
            return Ok(new { url });
        }
    }
}
