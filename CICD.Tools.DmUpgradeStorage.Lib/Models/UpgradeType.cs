namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models
{
    /// <summary>
    /// Represents the type of upgrade.
    /// </summary>
    public enum UpgradeType
    {
        /// <summary>
        /// Full DataMiner upgrade.
        /// </summary>
        Full,

        /// <summary>
        /// Partial DataMiner upgrade which contains a subset of features or fixes.
        /// </summary>
        Partial,

        /// <summary>
        /// Web-only DataMiner upgrade.
        /// </summary>
        Web
    }
}