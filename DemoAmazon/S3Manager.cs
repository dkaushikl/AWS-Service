using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;

namespace DemoAmazon
{
    public class S3Manager
    {
        private static IAmazonS3 _client;
        private const string BucketName = "test";
        private const string AwsAccessKeyId = "Access Key ID";
        private const string AwsSecretAccessKey = "Access Secret Key ID";
        private static readonly RegionEndpoint EndPoint = RegionEndpoint.USWest2;

        private static void Main()
        {
            byte[] data = null;
            var r53Client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint);
            ListObjectsRequest request = new ListObjectsRequest();
            request.BucketName = BucketName;
            ListObjectsResponse response = r53Client.ListObjects(request);
            if (response.IsTruncated)
            {

                // Set the marker property
                request.Marker = response.NextMarker;
            }
            else
            {
                request = null;
            }

            foreach (S3Object obj in response.S3Objects)
            {
                S3Bucket bk = new S3Bucket();
                S3FileInfo s3f = new S3FileInfo(r53Client, BucketName, obj.Key);
                TryGetFileData(obj.Key, ref data);
                string result = System.Text.Encoding.UTF8.GetString(data);
                Console.WriteLine("data - " + result);
                //File.Create("D:\\Mail.text").Write(data,0,0);
                Log_text_File(result);
                var ddf = s3f.OpenText();

                var teest = (ddf.BaseStream);
                var ss = s3f.OpenRead();


                using (StreamReader reader = new StreamReader(ddf.BaseStream))
                {
                    Console.WriteLine(reader.ReadLine());
                }


                
                Console.WriteLine("Object - " + obj.Key);
                Console.WriteLine(" Size - " + obj.Size);
                Console.WriteLine(" LastModified - " + obj.LastModified);
                Console.WriteLine(" Storage class - " + obj.StorageClass);
            }
            Console.ReadKey();
        }

        public static void Log_text_File(string content)
         {
            string App_Full_Path = @"D:\\create";
            if (!Directory.Exists(App_Full_Path))
            {
                Directory.CreateDirectory(App_Full_Path);
            }
            //set up a filestream
            FileStream fs = new FileStream(App_Full_Path + @"\Mail.txt", FileMode.OpenOrCreate, FileAccess.Write);

            //set up a streamwriter for adding text
            StreamWriter sw = new StreamWriter(fs);

            //find the end of the underlying filestream
            sw.BaseStream.Seek(0, SeekOrigin.End);

            //add the text
            sw.WriteLine(content);
            //add the text to the underlying filestream

            sw.Flush();
            //close the writer
            sw.Close();
        }

        public static bool FileExists(string fileKey)
        {
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = BucketName,
                    Key = fileKey
                };

                var s3FileInfo = new S3FileInfo(_client, BucketName, fileKey);

            }
            return false;
        }

        public static bool TryGetFileData(string fileKey, ref byte[] data)
        {
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                var request = new GetObjectRequest
                {
                    BucketName = BucketName,
                    Key = fileKey
                };

                var response = _client.GetObject(request);
                if (response.ResponseStream == null) return false;
                var buffer = new byte[16 * 1024];
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = response.ResponseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    data = ms.ToArray();
                }
                return true;
            }
        }

        public static void WritePublicObject(string filePath, string fileKey)
        {
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                try
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = BucketName,
                        Key = fileKey,
                        FilePath = filePath,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    var response = _client.PutObject(putRequest);
                }
                catch (AmazonS3Exception amazonS3Exception)
                {
                    if (amazonS3Exception.ErrorCode != null &&
                      (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                      ||
                      amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                    {
                        Console.WriteLine("Check the provided AWS Credentials.");
                        Console.WriteLine(
                            "For service sign up go to http://aws.amazon.com/s3");
                    }
                    else
                    {
                        Console.WriteLine(
                            "Error occurred. Message:'{0}' when writing an object"
                            , amazonS3Exception.Message);
                    }
                }
            }
        }

        public static void WritePublicObject(Stream fileStream, string fileKey)
        {
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                try
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = BucketName,
                        Key = fileKey,
                        InputStream = fileStream,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    var response = _client.PutObject(putRequest);
                }
                catch (AmazonS3Exception amazonS3Exception)
                {
                    if (amazonS3Exception.ErrorCode != null &&
                      (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                      ||
                      amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                    {
                        Console.WriteLine("Check the provided AWS Credentials.");
                        Console.WriteLine(
                            "For service sign up go to http://aws.amazon.com/s3");
                    }
                    else
                    {
                        Console.WriteLine(
                            "Error occurred. Message:'{0}' when writing an object"
                            , amazonS3Exception.Message);
                    }
                }
            }
        }

        public static void DeleteObject(string fileKey)
        {
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                try
                {
                    var deleteObjectRequest = new DeleteObjectRequest()
                    {
                        BucketName = BucketName,
                        Key = fileKey
                    };
                    var deleteObjectResponse = _client.DeleteObject(deleteObjectRequest);
                }
                catch (AmazonS3Exception amazonS3Exception)
                {
                    if (amazonS3Exception.ErrorCode != null &&
                      (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                      ||
                      amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                    {
                        Console.WriteLine("Check the provided AWS Credentials.");
                        Console.WriteLine(
                            "For service sign up go to http://aws.amazon.com/s3");
                    }
                    else
                    {
                        Console.WriteLine(
                            "Error occurred. Message:'{0}' when writing an object"
                            , amazonS3Exception.Message);
                    }
                }
            }
        }

        public static string GetUrl(string fileKey)
        {
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                var urlRequest = new GetPreSignedUrlRequest
                {
                    BucketName = BucketName,
                    Key = fileKey,
                    Expires = DateTime.Now.AddMinutes(5)
                };
                return _client.GetPreSignedURL(urlRequest);
            }
        }

        public static List<S3Object> ListFilesInFolder(string filePath)
        {
            var files = new List<S3Object>();
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                try
                {
                    var listRequest = new ListObjectsRequest
                    {
                        BucketName = BucketName,
                        Prefix = filePath
                    };
                    ListObjectsResponse listResponse;
                    do
                    {
                        // Get a list of objects
                        listResponse = _client.ListObjects(listRequest);
                        files.AddRange(listResponse.S3Objects);

                        // Set the marker property
                        listRequest.Marker = listResponse.NextMarker;
                    } while (listResponse.IsTruncated);
                }
                catch (AmazonS3Exception amazonS3Exception)
                {
                    if (amazonS3Exception.ErrorCode != null &&
                      (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                      ||
                      amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                    {
                        Console.WriteLine("Check the provided AWS Credentials.");
                        Console.WriteLine(
                            "For service sign up go to http://aws.amazon.com/s3");
                    }
                    else
                    {
                        Console.WriteLine(
                            "Error occurred. Message:'{0}' when writing an object"
                            , amazonS3Exception.Message);
                    }
                }
            }
            return files;
        }


        /// <summary>
        /// check particular bucket is exist in AWS S3 
        /// </summary>
        /// <returns>True if exist or false</returns>
        public static bool IsbucketExixt()
        {
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                var response = _client.ListBuckets();

                return response.Buckets.Any(bucket => bucket.BucketName == BucketName);
            }
        }


        /// <summary>
        ///Check particular folder inside partiular folder and  Create New folder inside prtcular Bucket with specify name
        /// </summary>
        /// <param name="mainFolder"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static bool CreateNewSubFolder(string mainFolder, string folderName)
        {
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                var findFolderRequest = new ListObjectsRequest
                {
                    BucketName = BucketName,
                    Delimiter = "/",
                    Prefix = folderName
                };
                var findFolderResponse = _client.ListObjects(findFolderRequest);
                var commonPrefixes = findFolderResponse.CommonPrefixes;
                var folderExists = commonPrefixes.Any();
                if (folderExists) return true;
                const string delimiter = "/";
                var folderKey = string.Concat(folderName, delimiter);
                var folderRequest = new PutObjectRequest
                {
                    BucketName = BucketName,
                    Key = folderKey,
                    InputStream = new MemoryStream(new byte[0])
                };
                var folderResponse = _client.PutObject(folderRequest);
                return true;
            }
        }

        /// <summary>
        /// Upload image to particular folder with particular bucket
        /// </summary>
        /// <param name="mainFolder"></param>
        /// <param name="subfolderName"></param>
        /// <param name="newfileName"></param>
        /// <returns></returns>
        public static bool UploadImageToParticularFolder(string mainFolder, string subfolderName, string newfileName)
        {
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                const string delimiter = "/";

                var putObjectRequest = new PutObjectRequest
                {
                    Key = newfileName,
                    CannedACL = S3CannedACL.PublicRead,
                    BucketName = string.Concat(BucketName, delimiter, mainFolder, delimiter, subfolderName)
                };

                var putObjectResponse = _client.PutObject(putObjectRequest);
                return putObjectResponse.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        /// <summary>
        /// Delete image from particular folder with specific image name
        /// </summary>
        /// <param name="mainFolder"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static bool DeleteFile(string mainFolder, string keyName)
        {
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                const string delimiter = "/";
                var deleteFolderRequest = new DeleteObjectRequest
                {
                    BucketName = BucketName,
                    Key = string.Concat(mainFolder, delimiter, keyName)
                };
                var deleteObjectResponse = _client.DeleteObject(deleteFolderRequest);
                return deleteObjectResponse.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            }
        }

        /// <summary>
        /// get All Images from particular folder
        /// </summary>
        /// <param name="mainFolder"></param>
        /// <param name="folderName"></param>
        public static void GetListOfObject(string mainFolder, string folderName)
        {
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                const string delimiter = "/";
                var listObjectsRequest = new ListObjectsRequest
                {
                    BucketName = BucketName,
                    Prefix = string.Concat(mainFolder, delimiter, folderName)
                };
                var listObjectsResponse = _client.ListObjects(listObjectsRequest);

                foreach (var entry in listObjectsResponse.S3Objects)
                {
                    if (entry.Size > 0)
                    {
                        Console.WriteLine("Found object with key {0}, size {1}, last modification date {2}", entry.Key, entry.Size, entry.LastModified);
                    }
                }
            }
        }

        public static bool WritingAnObject(string fileName, string filePath, string mainFolder, string subfolderName)
        {
            const string delimiter = "/";
            using (_client = new AmazonS3Client(AwsAccessKeyId, AwsSecretAccessKey, EndPoint))
            {
                try
                {
                    var request = new PutObjectRequest
                    {
                        FilePath = filePath,
                        BucketName = string.Concat(BucketName, delimiter, mainFolder, delimiter, subfolderName),
                        Key = fileName,
                        CannedACL = S3CannedACL.PublicRead
                    };
                    var response = _client.PutObject(request);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}