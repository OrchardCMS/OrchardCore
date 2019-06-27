using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Aliyun.OSS;
using OrchardCore.Modules;

namespace OrchardCore.FileStorage.AliYunOss
{
    public class OssFileStore : IFileStore
    {
        private const string _directoryMarkerFileName = "OrchardCore.Media.txt";

        private readonly OssStorageOptions _options;
        private readonly IClock _clock;
        private readonly OssClient _ossClient;

        public OssFileStore(OssStorageOptions options, IClock clock)
        {
            _options = options;
            _clock = clock;
            _ossClient = new OssClient(_options.Endpoint, _options.AccessKeyId, _options.AccessKeySecret);
        }

        public Uri BaseUri
        {
            get
            {
                var uriBuilder = new UriBuilder(_options.BucketName);
                uriBuilder.Path = this.Combine(uriBuilder.Path, _options.BasePath);
                return uriBuilder.Uri;
            }
        }

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {

            var objectMetaData =  OssHelper.GetMetaData(_ossClient, _options.BucketName, path);

            return new OssFile(path, objectMetaData.ContentLength, objectMetaData.LastModified);
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {

            var objectMetaData = OssHelper.GetMetaData(_ossClient, _options.BucketName, path);
            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = "", bool includeSubDirectories = false)
        {
            var results = OssHelper.ListObject(_ossClient , _options.BucketName,path,".");
            //var results = new List<IFileStoreEntry>();
           
            return
                results
                    .OrderByDescending(x => x.IsDirectory)
                    .ToArray();
        }

        public async Task<bool> TryCreateDirectoryAsync(string path)
        {
            var re = OssHelper.CheckObjectExisted(_ossClient, _options.BucketName, path);
            if (re)
            {
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");
            }

            await CreateDirectoryAsync(path);

            return true;
        }



        public async Task<bool> TryDeleteFileAsync(string path)
        {

            OssHelper.DeleteOneFile(_ossClient, _options.BucketName, path);
            return true;
        }
        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<bool> TryDeleteDirectoryAsync(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new FileStoreException("Cannot delete the root directory.");
            //仅仅删除文件夹下的所有文件，不递归删除文件夹
            var ossFiles = OssHelper.ListObject(_ossClient, _options.BucketName, path);
            foreach (var item in ossFiles)
            {
                OssHelper.DeleteOneFile(_ossClient, _options.BucketName, item.Path);
            }
            return true;
        }
        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public async Task MoveFileAsync(string oldPath, string newPath)
        {

            var re = OssHelper.UpdateObjectName(_ossClient,_options.BucketName ,oldPath, newPath);
            await TryDeleteFileAsync(oldPath);
        }

  
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<Stream> GetFileStreamAsync(string path)
        {
            if (!OssHelper.CheckObjectExisted(_ossClient, _options.BucketName, path))
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            var stream = OssHelper.DownloadOneFile(_ossClient, _options.BucketName, path);
            return stream;
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="inputStream"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public async Task CreateFileFromStream(string path, Stream inputStream, bool overwrite = false)
        {
            if (!OssHelper.CheckObjectExisted(_ossClient, _options.BucketName, path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
            OssHelper.UploadOneFileFromFileStream(_ossClient,_options.BucketName,path,inputStream);
        }


        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task CreateDirectoryAsync(string path)
        {
            var filePath = path + _directoryMarkerFileName;
            OssHelper.UploadOneFileFromFilePath(_ossClient, _options.BucketName, _directoryMarkerFileName, filePath);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath)
        {
            OssHelper.CopyObjectName(_ossClient, _options.BucketName, srcPath, dstPath);
        }
    }
}
