using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.FileStorage.AliYunOss
{
    public static class OssHelper
    {

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static bool CheckObjectExisted(OssClient client,string bucketName,string objectName)
        {
            var exist = client.DoesObjectExist(bucketName, objectName);
            return exist;
        }


        /// <summary>
        /// 上传一个文件
        /// </summary>
        /// <param name="ossNewFileName"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public static bool UploadOneFileFromFilePath(OssClient client, string bucketName, string ossNewFileName, string filePath)
        {
            PutObjectResult result = client.PutObject(bucketName, ossNewFileName, filePath);
            return result.HttpStatusCode == HttpStatusCode.OK;
        }
        /// <summary>
        /// 上传一个文件通过stream
        /// </summary>
        /// <param name="ossNewFileName"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public static bool UploadOneFileFromFileStream(OssClient client, string bucketName, string ossNewFileName, Stream stream)
        {
            PutObjectResult result = client.PutObject(bucketName, ossNewFileName, stream);
            return result.HttpStatusCode == HttpStatusCode.OK;
        }
        /// <summary>
        /// 删除一个文件
        /// </summary>
        /// <param name="ossFileName"></param>
        public static void DeleteOneFile(OssClient client, string bucketName, string ossFileName)
        {
            //删除文件
            client.DeleteObject(bucketName, ossFileName);
        }
        /// <summary>
        /// 下载单个文件
        /// </summary>
        /// <param name="ossFileName"></param>
        /// <returns></returns>
        public static Stream DownloadOneFile(OssClient client, string bucketName, string ossFileName)
        {
            //下载一个文件
            OssObject osObject = client.GetObject(bucketName, ossFileName);
            Stream streamContent = osObject.Content;
            return streamContent;
        }
        /// <summary>
        /// 创建一个bucket
        /// </summary>
        /// <param name="newBucketName"></param>
        /// <returns></returns>
        public static bool CreateBucket(OssClient client, string newBucketName)
        {
            //创建一个bucket
            Bucket bucket = client.CreateBucket(newBucketName);
            return bucket != null;
        }


        /// <summary>
        /// 列举所有文件夹、文件
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="Prefix">不写表示列出文件夹，写文件夹前缀表示列举文件夹下的所有文件</param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static List<IFileStoreEntry> ListObject(OssClient client, string bucketName, string prefix = "", string delimiter = "")
        {
            try
            {
                List<IFileStoreEntry> ossFileStoreEntries = new List<IFileStoreEntry>();
                List<OssFile> ossFiles = new List<OssFile>();
                List<OssDirectory> ossDirectories = new List<OssDirectory>(); 
                ObjectListing result = null;
                string nextMarker = string.Empty;
                do
                {
                    // 每页列举的文件个数通过maxKeys指定，超过指定数将进行分页显示。
                    var listObjectsRequest = new ListObjectsRequest(bucketName)
                    {
                        Marker = nextMarker,
                        MaxKeys = 100,
                        Delimiter = delimiter,
                        Prefix = prefix
                    };
                    result = client.ListObjects(listObjectsRequest);
                    foreach (OssObjectSummary summary in result.ObjectSummaries)
                    {
                        if (summary.Key.LastIndexOf("/") == (summary.Key.Length - 1) )
                        {
                            ossDirectories.Add(new OssDirectory(summary.Key,summary.LastModified) {});
                        }
                        else
                        {
                            ossFiles.Add(new OssFile(summary.Key, summary.Size,summary.LastModified) { });
                        }
                    }
                    nextMarker = result.NextMarker;
                } while (result.IsTruncated);
                ossFileStoreEntries.AddRange(ossDirectories);
                ossFileStoreEntries.AddRange(ossFiles);
                return ossFileStoreEntries;
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        /// <summary>
        /// 删除多个文件
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="keys"></param>
        public static void DeleteMultiObjects(OssClient client, string bucketName, string objectName, List<string> keys)
        {
            //var keys = new List<string>();
            var listResult = client.ListObjects(bucketName);
            foreach (var summary in listResult.ObjectSummaries)
            {
                keys.Add(summary.Key);
            }
            // quietMode为true表示简单模式，为false表示详细模式。默认为详细模式。
            var quietMode = false;
            // DeleteObjectsRequest的第三个参数指定返回模式。
            var request = new DeleteObjectsRequest(bucketName, keys, quietMode);
            // 删除多个文件。
            var result = client.DeleteObjects(request);
            //if ((!quietMode) && (result.Keys != null))
            //{
            //    foreach (var obj in result.Keys)
            //    {
            //        Console.WriteLine("Delete successfully : {0} ", obj.Key);
            //    }
            //}
        }

        public static void CopyObjectName(OssClient client, string bucketName, string objectName, string targetObjectName)
        {
            var req = new CopyObjectRequest(bucketName, objectName, bucketName, targetObjectName);
            // 拷贝文件。
            client.CopyObject(req);
        }


        /// <summary>
        /// 删除bucket
        /// </summary>
        /// <param name="bucketName"></param>
        public static void DeleteBucket(OssClient client, string bucketName)
        {
            //删除一个bucket
            client.DeleteBucket(bucketName);
        }
        /// <summary>
        /// 获取bucketList列表
        /// </summary>
        /// <returns></returns>
        public static List<OssObjectSummary> GetBucketList(OssClient client, string bucketName)
        {
            //获取bucket中的文件列表
            List<OssObjectSummary> objects = new List<OssObjectSummary>();
            ObjectListing listResult = client.ListObjects(bucketName);
            foreach (OssObjectSummary summary in listResult.ObjectSummaries)
            {
                objects.Add(summary);
            }
            return objects;
        }

        /// <summary>
        /// 获取单个文件的ObjectMetadata信息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static ObjectMetadata GetMetaData(OssClient client, string bucketName, string objectName)
        {
            ObjectMetadata meta = client.GetObjectMetadata(bucketName, objectName);
            return meta; 
        }


        /// <summary>
        /// 修改Object对象名称
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="targetObjectName"></param>
        /// <returns></returns>
        public static bool UpdateObjectName(OssClient client, string bucketName, string objectName, string targetObjectName)
        {
            var result = false;
            try
            {
                var req = new CopyObjectRequest(bucketName, objectName, bucketName, targetObjectName);
                // 拷贝文件。
                client.CopyObject(req);
                client.DeleteObject(bucketName, objectName);
                result = true;
            }
            catch (Exception ex)
            {

            }
            return result;

        }

    }
}
