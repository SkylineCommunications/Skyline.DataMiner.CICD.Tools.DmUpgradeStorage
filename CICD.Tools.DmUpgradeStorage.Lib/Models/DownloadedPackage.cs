namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models
{
    using System.IO;

    /// <summary>
    /// Represents a downloaded package from storage.
    /// </summary>
    public record DownloadedPackage(string Name, Stream Content);
}