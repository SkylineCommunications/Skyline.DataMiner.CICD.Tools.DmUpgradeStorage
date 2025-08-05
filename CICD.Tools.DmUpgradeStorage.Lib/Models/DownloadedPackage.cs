namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models
{
    using System.IO;

    /// <summary>
    /// Represents a downloaded package from storage.
    /// </summary>
    /// <param name="Name">File name of the downloaded package.</param>
    /// <param name="Content">Content of the downloaded package.</param>
    public record DownloadedPackage(string Name, Stream Content);
}