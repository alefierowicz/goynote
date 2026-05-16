using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace goynote.Models.System
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string bucketName;
        public S3Service(IConfiguration configuration)
        {
            // BE AWARE
            // this requires dotnet user-secrets to be set for the proper values.
            bucketName = configuration["AWS:BucketName"]!;
            _s3Client = new AmazonS3Client(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"],
                RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
                );
        }

        public async Task<string?> UploadAsync(IFormFile file)
        {
            using Stream stream = file.OpenReadStream();
            string key = $"{Guid.NewGuid()}";
            
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = file.ContentType
                };

                await _s3Client.PutObjectAsync(request);
                return key;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"S3 Exception: {ex.Message}");
                return null;
            }
            catch (Exception ex) {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<Stream?> DownloadAsync(string key)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                var response = await _s3Client.GetObjectAsync(request);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"S3 Exception: {ex.Message}");
                return null;
            }
            catch (Exception ex) {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        } 

    }
}
