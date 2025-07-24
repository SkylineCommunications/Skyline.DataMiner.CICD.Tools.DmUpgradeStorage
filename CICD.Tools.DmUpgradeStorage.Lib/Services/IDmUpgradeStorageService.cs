namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Services
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models;

    /// <summary>
    /// Interface for interacting with the DmUpgrade storage service.
    /// </summary>
    public interface IDmUpgradeStorageService
    {
        /// <summary>
        /// Authenticates the service with either a connection string or account credentials.
        /// </summary>
        /// <param name="connectionString">The blob storage connection string.</param>
        /// <param name="accountName">The storage account name.</param>
        /// <param name="accountKey">The storage account key.</param>
        void Authenticate(string? connectionString = null, string? accountName = null, string? accountKey = null);

        /// <summary>
        /// Deletes a blob.
        /// </summary>
        /// <param name="packageName">The package name.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if the blob was successfully deleted; false otherwise. Null if the package did not exist.</returns>
        Task<bool?> DeleteAsync(string packageName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a package by its name.
        /// </summary>
        /// <param name="packageName">The package name.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The downloaded package, or null if not found.</returns>
        Task<DownloadedPackage?> DownloadByNameAsync(string packageName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads the latest package matching the specified tag filter.
        /// </summary>
        /// <param name="builder">The tag filter builder.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The latest downloaded package, or null if none found.</returns>
        Task<DownloadedPackage?> DownloadLatestByTagsAsync(PackageTagFilter builder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads all packages matching the specified tag filter.
        /// </summary>
        /// <param name="filter">The tag filter to apply.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>An asynchronous stream of downloaded packages.</returns>
        IAsyncEnumerable<ConfiguredTaskAwaitable<DownloadedPackage>> DownloadPackagesByTagsAsync(PackageTagFilter filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the container name for subsequent operations.
        /// </summary>
        /// <param name="name">The container name.</param>
        void SetContainer(string? name = null);

        /// <summary>
        /// Uploads a package to blob storage.
        /// </summary>
        /// <param name="package">The package to upload.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A <see cref="UploadResult"/> with the info about the upload.</returns>
        Task<UploadResult> UploadAsync(PackageToUpload package, CancellationToken cancellationToken = default);
    }
}