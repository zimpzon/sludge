using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sludge
{
    public class BlobStorage : IBlobStorage
    {
        private const string ContainerName = "sludge";
        private readonly BlobContainerClient _container;
        private readonly Random _random = new Random();

        public BlobStorage(IConfiguration config)
        {
            string connStr = config["blob_connectionstring"];
            _container = new BlobContainerClient(connStr, ContainerName);
        }

        public async Task<bool> DeleteBlobAsync(string path)
        {
            var blob = _container.GetBlobClient(path);
            return await blob.DeleteIfExistsAsync();
        }

        public async Task<byte[]> GetBlobAsync(string path, bool throwIfNotFound = true)
        {
            return await InternalGetBlobAsync(path, throwIfNotFound);
        }

        public async Task<string> GetTextBlobAsync(string path, bool throwIfNotFound = true)
        {
            var bytes = await InternalGetBlobAsync(path, throwIfNotFound);
            return bytes == null ? null : Encoding.UTF8.GetString(bytes);
        }

        public async Task StoreBlobAsync(string path, byte[] bytes, bool overwriteExisting = true, string leaseId = null)
        {
            await InternalStoreBlobAsync(path, bytes, overwriteExisting);
        }

        public async Task StoreTextBlobAsync(string path, string text, bool overwriteExisting = true, string leaseId = null)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            await InternalStoreBlobAsync(path, bytes, overwriteExisting, leaseId);
        }

        public async Task StoreStreamBlobAsync(string path, Stream stream, bool overwriteExisting = true, string leaseId = null)
        {
            await InternalStoreStreamBlobAsync(path, stream, overwriteExisting, leaseId);
        }

        public async Task<bool> BlobExistsAsync(string path)
        {
            var blob = _container.GetBlobClient(path);
            return await blob.ExistsAsync();
        }

        public async Task<IEnumerable<string>> GetBlobPaths(string prefix)
        {
            var blobs = new List<BlobItem>();
            await foreach (var blob in _container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix))
            {
                blobs.Add(blob);
            }
            return blobs.Select(b => b.Name);
        }

        public async Task EnsureBlobExists(string path)
        {
            bool exists = await BlobExistsAsync(path);
            if (!exists)
            {
                try
                {
                    await StoreTextBlobAsync(path, string.Empty);
                }
                catch (Exception)
                {
                    // If this blob is used for leases we will get an exception if someone else managed to
                    // create and lease the blob before we completed the write. If it exists now consider
                    // it a success.
                    exists = await BlobExistsAsync(path);
                    if (!exists)
                        throw;
                }
            }
        }

        public async Task<string> TryAcquireBlobLease(string path, TimeSpan duration, TimeSpan? timeout = null)
        {
            var blob = _container.GetBlobClient(path);
            var leaseClient = blob.GetBlobLeaseClient();

            var time = DateTime.UtcNow;
            var endWaitTime = time.Add(timeout ?? TimeSpan.Zero);
            string acquireFailMessage = string.Empty;

            while (time <= endWaitTime)
            {
                try
                {
                    var lease = await leaseClient.AcquireAsync(duration);
                    return lease.Value.LeaseId;
                }
                catch (RequestFailedException e)
                {
                    acquireFailMessage = e.Message;
                }

                await Task.Delay(millisecondsDelay: _random.Next(100, 500));
                time = DateTime.UtcNow;
            }

            return null;
        }

        public async Task ReleaseBlobLease(string path, string leaseId)
        {
            var blob = _container.GetBlobClient(path);
            var leaseClient = blob.GetBlobLeaseClient(leaseId);
            try
            {
                await leaseClient.ReleaseAsync();
            }
            catch (RequestFailedException)
            {
                // This is expected if ReleaseBlobLease is called after duration has expired.
            }
        }

        public async Task<DateTimeOffset> GetLastUpdatedUtc(string path, bool throwIfNotFound = true)
        {
            try
            {
                var blob = _container.GetBlobClient(path);
                BlobProperties properties = await blob.GetPropertiesAsync();
                return properties.LastModified;
            }
            catch (RequestFailedException e)
            {
                if (throwIfNotFound)
                    throw new ArgumentException($"No blob found at path {path}", e);

                return DateTimeOffset.MinValue;
            }
        }

        private async Task<byte[]> InternalGetBlobAsync(string path, bool throwIfNotFound)
        {
            try
            {
                var blob = _container.GetBlobClient(path);
                var result = new MemoryStream();
                await blob.DownloadToAsync(result);
                return result.ToArray();
            }
            catch (RequestFailedException e)
            {
                if (throwIfNotFound)
                    throw new ArgumentException($"No blob found at path {path}", e);

                return null;
            }
        }

        private async Task InternalStoreBlobAsync(string path, byte[] bytes, bool overwriteExisting = true, string leaseId = null)
        {
            var stream = new MemoryStream(bytes);
            await InternalStoreStreamBlobAsync(path, stream, overwriteExisting, leaseId);
        }

        private async Task InternalStoreStreamBlobAsync(string path, Stream stream, bool overwriteExisting = true, string leaseId = null)
        {
            var blob = _container.GetBlobClient(path);
            if (!overwriteExisting && await blob.ExistsAsync())
                throw new ArgumentException($"Path already exists: {path}");

            var uploadOptions = new BlobUploadOptions { Conditions = new BlobRequestConditions() { LeaseId = leaseId } };
            await blob.UploadAsync(stream, uploadOptions);
        }
    }
}
