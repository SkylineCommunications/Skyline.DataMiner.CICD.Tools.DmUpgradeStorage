namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Authentication;
    using System.Threading;
    using System.Threading.Tasks;

    using Azure;
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models;

    /// <summary>
    /// Service to communicate with the storage for dmupgrade packages.
    /// </summary>
    public class DmUpgradeStorageService : IDmUpgradeStorageService
    {
        private readonly IConfiguration? configuration;
        private readonly ILogger<DmUpgradeStorageService> logger;
        private BlobServiceClient? client;
        private string? containerName;
        private bool setupComplete;

        /// <summary>
        /// Initializes a new instance of the <see cref="DmUpgradeStorageService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DmUpgradeStorageService(ILogger<DmUpgradeStorageService> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Constructor for DI.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public DmUpgradeStorageService(IConfiguration configuration, ILogger<DmUpgradeStorageService> logger) : this(logger)
        {
            this.configuration = configuration;
        }

        /// <inheritdoc />
        public void Authenticate(string? connectionString = null, string? accountName = null, string? accountKey = null)
        {
            DebugLog.Start(logger);

            try
            {
                connectionString ??= configuration?[UserSecrets.BlobStorageConnectionString] ??
                                     Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageConnectionString);
                if (connectionString == null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageConnectionString, EnvironmentVariableTarget.User) ?? 
                                       Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageConnectionString, EnvironmentVariableTarget.Machine);
                }

                if (!String.IsNullOrWhiteSpace(connectionString))
                {
                    logger.LogDebug("Creating the BlobServiceClient with the connection string.");
                    client = new BlobServiceClient(connectionString);
                    setupComplete = true;
                    return;
                }

                accountName ??= configuration?[UserSecrets.BlobStorageAccountName] ??
                                Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageAccountName);
                if (accountName == null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    accountName = Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageAccountName, EnvironmentVariableTarget.User) ??
                                  Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageAccountName, EnvironmentVariableTarget.Machine);
                }

                accountKey ??= configuration?[UserSecrets.BlobStorageAccountKey] ??
                               Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageAccountKey);
                if (accountKey == null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    accountKey = Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageAccountKey, EnvironmentVariableTarget.User) ??
                                 Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageAccountKey, EnvironmentVariableTarget.Machine);
                }

                if (!String.IsNullOrWhiteSpace(accountName) && !String.IsNullOrWhiteSpace(accountKey))
                {
                    logger.LogDebug("Creating the BlobServiceClient with the account name & key.");
                    string blobUri = "https://" + accountName + ".blob.core.windows.net";
                    client = new BlobServiceClient(new Uri(blobUri), new StorageSharedKeyCredential(accountName, accountKey));
                    setupComplete = true;
                    return;
                }

                throw new InvalidCredentialException("Missing credentials for connecting to the blob storage.");
            }
            finally
            {
                DebugLog.End(logger);
            }
        }

        /// <inheritdoc />
        public async Task<bool?> DeleteAsync(string packageName, CancellationToken cancellationToken = default)
        {
            DebugLog.Start(logger);

            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(packageName);

                EnsureSetup();

                BlobContainerClient container = await GetContainerAsync(cancellationToken).ConfigureAwait(false);
                BlobClient blob = container.GetBlobClient(packageName);
                if (!await blob.ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    // Blob does not exist, so we can return true.
                    return null;
                }

                Response response = await container.DeleteBlobAsync(packageName, DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken)
                                                   .ConfigureAwait(false);
                if (response.IsError)
                {
                    logger.LogError("Delete Error: {reason}", response.ReasonPhrase);
                }

                return !response.IsError;
            }
            finally
            {
                DebugLog.End(logger);
            }
        }

        /// <inheritdoc />
        public async Task<DownloadedPackage?> DownloadByNameAsync(string packageName, CancellationToken cancellationToken = default)
        {
            DebugLog.Start(logger);

            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(packageName);

                EnsureSetup();

                BlobContainerClient container = await GetContainerAsync(cancellationToken).ConfigureAwait(false);

                BlobClient blob = container.GetBlobClient(packageName);
                if (!await blob.ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    logger.LogError("Package with name {name} does not exist.", packageName);
                    return null;
                }

                return await DownloadBlobAsync(blob, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                DebugLog.End(logger);
            }
        }

        /// <inheritdoc />
        public async Task<DownloadedPackage?> DownloadLatestByTagsAsync(PackageTagFilter builder, CancellationToken cancellationToken = default)
        {
            DebugLog.Start(logger);

            try
            {
                if (builder.IsEmpty())
                {
                    logger.LogError("No filter was specified. Please specify at least one filter.");
                    return null;
                }

                EnsureSetup();

                BlobContainerClient container = await GetContainerAsync(cancellationToken).ConfigureAwait(false);

                var foundBlobs = container.FindBlobsByTagsAsync(builder.Build(), cancellationToken).ConfigureAwait(false);

                return await DownloadLatestBlobAsync(container, foundBlobs, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                DebugLog.End(logger);
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ConfiguredTaskAwaitable<DownloadedPackage>> DownloadPackagesByTagsAsync(PackageTagFilter filter, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            DebugLog.Start(logger);

            try
            {
                if (filter.IsEmpty())
                {
                    logger.LogError("No filter was specified. Please specify at least one filter.");
                    yield break;
                }

                EnsureSetup();

                BlobContainerClient container = await GetContainerAsync(cancellationToken).ConfigureAwait(false);

                var foundBlobs = container.FindBlobsByTagsAsync(filter.Build(), cancellationToken).ConfigureAwait(false);

                await foreach (TaggedBlobItem blobItem in foundBlobs)
                {
                    BlobClient blobClient = container.GetBlobClient(blobItem.BlobName);
                    yield return DownloadBlobAsync(blobClient, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                DebugLog.End(logger);
            }
        }

        /// <inheritdoc />
        public void SetContainer(string? name = null)
        {
            DebugLog.Start(logger);

            try
            {
                containerName = name ?? configuration?[UserSecrets.BlobStorageContainerName] ??
                    Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageContainerName);
                if (containerName == null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    containerName = Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageContainerName, EnvironmentVariableTarget.User) ??
                                    Environment.GetEnvironmentVariable(EnvironmentVariables.BlobStorageContainerName, EnvironmentVariableTarget.Machine);
                }

                if (String.IsNullOrWhiteSpace(containerName))
                {
                    throw new ArgumentException("Missing container name for the blob storage.");
                }
            }
            finally
            {
                DebugLog.End(logger);
            }
        }

        /// <inheritdoc />
        public async Task<bool> UploadAsync(PackageToUpload package, CancellationToken cancellationToken = default)
        {
            DebugLog.Start(logger);

            try
            {
                EnsureSetup();

                var packageFile = package.PackageFile;

                if (!packageFile.Exists)
                {
                    logger.LogError("File '{fileName}' could not be found.", packageFile.FullName);
                    return false;
                }

                if (packageFile.Extension != ".dmupgrade")
                {
                    logger.LogError("Invalid file type. The file must be a .dmupgrade file.");
                    return false;
                }

                BlobContainerClient container = await GetContainerAsync(cancellationToken).ConfigureAwait(false);

                string packageName = packageFile.Name;
                BlobClient blob = container.GetBlobClient(packageName);

                await using FileStream fileStream = packageFile.OpenRead();

                logger.LogInformation("Starting upload of package '{name}'.", packageName);

                Stopwatch sw = Stopwatch.StartNew();
                DateTime lastLogged = DateTime.MinValue;
                long size = fileStream.Length;

                var response = await blob.UploadAsync(fileStream, new BlobUploadOptions
                {
                    Tags = new Dictionary<string, string>
                    {
                        [Constants.VersionTagName] = package.Version,
                        [Constants.BuildNumberTagName] = package.BuildNumber.ToString(),
                        [Constants.CuTagName] = package.Cu?.ToString() ?? String.Empty,
                        [Constants.GerritIdTagName] = package.GerritId?.ToString() ?? String.Empty,
                        [Constants.PatchSetTagName] = package.PatchSet?.ToString() ?? String.Empty,
                        [Constants.TypeTagName] = package.Type.ToString(),
                        [Constants.UpgradeTypeTagName] = package.UpgradeType?.ToString() ?? String.Empty,
                    },
                    ProgressHandler = new Progress<long>(l =>
                    {
                        var now = DateTime.UtcNow;
                        if ((now - lastLogged).TotalSeconds >= 3)
                        {
                            lastLogged = now;
                            var percentage = (double)l / size * 100;
                            logger.LogInformation("Upload Progress: {progress:F2}%", percentage);
                        }
                    })
                }, cancellationToken).ConfigureAwait(false);

                sw.Stop();
                logger.LogInformation("Finished uploading package '{name}'.", packageName);
                logger.LogDebug("Upload took {time}", sw.Elapsed);

                return response?.Value != null;
            }
            finally
            {
                DebugLog.End(logger);
            }
        }

        private async Task<DownloadedPackage> DownloadBlobAsync(BlobClient blob, CancellationToken cancellationToken = default)
        {
            DebugLog.Start(logger);

            try
            {
                Response<BlobProperties> properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                long size = properties.Value.ContentLength;
                DateTime lastLogged = DateTime.MinValue;
                Stopwatch sw = Stopwatch.StartNew();

                logger.LogDebug("Starting download of {blobName}...", blob.Name);

                Response<BlobDownloadStreamingResult> content = await blob.DownloadStreamingAsync(new BlobDownloadOptions
                {
                    ProgressHandler = new Progress<long>(l =>
                    {
                        var now = DateTime.UtcNow;
                        if ((now - lastLogged).TotalSeconds >= 3)
                        {
                            lastLogged = now;
                            double percentage = (double)l / size * 100;
                            logger.LogInformation("Download Progress: {progress:F2}%", percentage);
                        }
                    })
                }, cancellationToken).ConfigureAwait(false);

                sw.Stop();
                logger.LogInformation("Finished download of {blobName}.", blob.Name);
                logger.LogDebug("Download took {time}", sw.Elapsed);

                return new DownloadedPackage(blob.Name, content.Value.Content);
            }
            finally
            {
                DebugLog.End(logger);
            }
        }

        private async Task<DownloadedPackage?> DownloadLatestBlobAsync(BlobContainerClient container, ConfiguredCancelableAsyncEnumerable<TaggedBlobItem> blobItems, CancellationToken cancellationToken = default)
        {
            DebugLog.Start(logger);
            try
            {
                string? latestItem = null;
                DateTimeOffset offset = DateTimeOffset.MinValue;
                await foreach (TaggedBlobItem taggedBlobItem in blobItems)
                {
                    BlobClient blob = container.GetBlobClient(taggedBlobItem.BlobName);

                    Response<BlobProperties> properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                    if (offset < properties.Value.CreatedOn)
                    {
                        latestItem = taggedBlobItem.BlobName;
                    }
                }

                if (latestItem == null)
                {
                    logger.LogDebug("No latest item found from the provided list of items.");
                    return null;
                }

                BlobClient blobClient = container.GetBlobClient(latestItem);
                return await DownloadBlobAsync(blobClient, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                DebugLog.End(logger);
            }
        }

        private void EnsureSetup()
        {
            if (setupComplete && containerName != null)
            {
                return;
            }

            // Try on environment variables only
            Authenticate();
            SetContainer();

            if (setupComplete && containerName != null)
            {
                return;
            }

            // Shouldn't happen normally as the Authenticate/SetContainer methods will throw an exception if something is missing.
            throw new InvalidOperationException($"{nameof(DmUpgradeStorageService)} is not set up. Call {nameof(Authenticate)} and {nameof(SetContainer)} first.");
        }

        private async Task<BlobContainerClient> GetContainerAsync(CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = client!.GetBlobContainerClient(containerName);

            if (!await containerClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidOperationException($"Blob container {containerName} does not exist.");
            }

            return containerClient;
        }
    }
}