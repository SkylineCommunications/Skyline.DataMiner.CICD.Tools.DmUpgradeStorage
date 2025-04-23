namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib
{
    /// <summary>
    /// Static class for environment variables.
    /// </summary>
    public static class EnvironmentVariables
    {
        /// <summary>
        /// The name of the environment variable used to store the connection string for the blob storage.
        /// </summary>
        public const string BlobStorageConnectionString = "BLOBSTORAGE__CONNECTIONSTRING";

        /// <summary>
        /// The name of the environment variable used to store the account name for the blob storage.
        /// </summary>
        public const string BlobStorageAccountName = "BLOBSTORAGE__ACCOUNTNAME";

        /// <summary>
        /// The name of the environment variable used to store the account key for the blob storage.
        /// </summary>
        public const string BlobStorageAccountKey = "BLOBSTORAGE__ACCOUNTKEY";

        /// <summary>
        /// The name of the environment variable used to store the container name for the blob storage.
        /// </summary>
        public const string BlobStorageContainerName = "BLOBSTORAGE__CONTAINERNAME";
    }

    internal static class UserSecrets
    {
        public const string BlobStorageConnectionString = "BLOBSTORAGE:CONNECTIONSTRING";
        public const string BlobStorageAccountName = "BLOBSTORAGE:ACCOUNTNAME";
        public const string BlobStorageAccountKey = "BLOBSTORAGE:ACCOUNTKEY";
        public const string BlobStorageContainerName = "BLOBSTORAGE:CONTAINERNAME";
    }
}