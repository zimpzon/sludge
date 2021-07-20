using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sludge
{
    /// <summary>
    /// Access to BLOB files. Users of this library must install nuget package Azure.Storage.Blobs.
    /// </summary>
    public interface IBlobStorage
    {
        /// <summary>
        /// Store a blob.
        /// </summary>
        Task StoreBlobAsync(string path, byte[] bytes, bool overwriteExisting = true, string leaseId = null);

        /// <summary>
        /// Store text as a blob. Internally it will be stored as UTF-8.
        /// </summary>
        Task StoreTextBlobAsync(string path, string text, bool overwriteExisting = true, string leaseId = null);

        /// <summary>
        /// Store a stream.
        /// </summary>
        Task StoreStreamBlobAsync(string path, Stream stream, bool overwriteExisting = true, string leaseId = null);

        /// <summary>
        /// Get an already stored blob. Throws ArgumentException if blob does not exist and throwIfNotFound is true, else returns null.
        /// </summary>
        Task<byte[]> GetBlobAsync(string path, bool throwIfNotFound = true);

        /// <summary>
        /// Get a text file stored with StoreTextBlob. Throws ArgumentException if blob does not exist and throwIfNotFound is true, else returns null.
        /// </summary>
        Task<string> GetTextBlobAsync(string path, bool throwIfNotFound = true);

        /// <summary>
        /// Delete an existing blob. If the blob does not exists the operation does nothing.
        /// </summary>
        Task<bool> DeleteBlobAsync(string path);

        /// <summary>
        /// Check if a blob exists at the given path.
        /// </summary>
        /// <returns>True if exists, else false</returns>
        Task<bool> BlobExistsAsync(string path);

        /// <summary>
        /// Returns path to all blobs starting with the given prefix.
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>List of blobs found</returns>
        Task<IEnumerable<string>> GetBlobPaths(string prefix);

        /// <summary>
        /// This will write an empty blob if none is found at path. Can be used in conjunction with TryAcquireBlobLease as a systemwide lock.
        /// </summary>
        /// <param name="path">The blob path</param>
        Task EnsureBlobExists(string path);

        /// <summary>
        /// Try to acquire a lease on the blob at the given path. The blob must exist.
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="duration">Must be between 15 and 60 seconds or ArgumentOutOfRangeException will be thrown</param>
        /// <param name="timeout">If this parameter is not null the call will block until the lease is acquired or timeout is reached</param>
        /// <returns>The leaseId if successful, else null</returns>
        Task<string> TryAcquireBlobLease(string path, TimeSpan duration, TimeSpan? timeout = null);

        /// <summary>
        /// Release a leaseId acquired by TryAcquireBlobLease. If the lease does not exist/has expired the operation does nothing.
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="leaseId">The leaseId</param>
        /// <returns></returns>
        Task ReleaseBlobLease(string path, string leaseId);

        /// <summary>
        /// Get the timestamp (in UTC) of when a stored blob was last updated. Throws ArgumentException if blob does not exist and throwIfNotFound is true, else returns null.
        /// </summary>
        /// <param name="path">The location of the blob in the container.</param>
        /// <param name="throwIfNotFound">Whether or not an exception whould be thrown if the blob was not found.</param>
        /// <returns>The timestamp (in UTC) of when the blob was last updated.</returns>
        Task<DateTimeOffset> GetLastUpdatedUtc(string path, bool throwIfNotFound = true);
    }
}
