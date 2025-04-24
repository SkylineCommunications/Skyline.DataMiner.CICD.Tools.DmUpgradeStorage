namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    using Skyline.DataMiner.CICD.FileSystem.FileInfoWrapper;

    /// <summary>
    /// Contains all the info needed for the upload of a package.
    /// </summary>
    public partial class PackageToUpload
    {
        /// <summary>
        /// Represents the build number of the upgrade package.
        /// </summary>
        public required uint BuildNumber { get; set; }

        /// <summary>
        /// Represents the CU number.
        /// </summary>
        public uint? Cu { get; set; }

        /// <summary>
        /// Represents the gerrit ID.
        /// </summary>
        public uint? GerritId { get; set; }

        /// <summary>
        /// Represents the file to upload.
        /// </summary>
        public required IFileInfoIO PackageFile { get; init; }

        /// <summary>
        /// Represents the patch set number of the gerrit item.
        /// </summary>
        public uint? PatchSet { get; set; }

        /// <summary>
        /// Represents what type of package this is.
        /// </summary>
        public PackageType Type { get; set; }

        /// <summary>
        /// Represent the type of upgrade.
        /// </summary>
        public UpgradeType? UpgradeType { get; set; }

        /// <summary>
        /// Represents the DataMiner version. Expected format: X.X.X.X
        /// </summary>
        public required string Version { get; set; }

        /// <summary>
        /// Create a <see cref="PackageToUpload"/> object from a dmupgrade file based on the file name.
        /// </summary>
        /// <param name="fileInfo">The dmupgrade file</param>
        /// <returns>A <see cref="PackageToUpload"/> if successfully found the info based on file name, <see langword="null"/> if not.</returns>
        /// <exception cref="InvalidDataException">Invalid file type. The file must be a .dmupgrade file.</exception>
        public static PackageToUpload? FromFile(IFileInfoIO fileInfo)
        {
            ArgumentNullException.ThrowIfNull(fileInfo);

            if (fileInfo.Extension != ".dmupgrade")
            {
                throw new InvalidDataException("Invalid file type. The file must be a .dmupgrade file.");
            }

            Match match = DmUpgradeFileNameRegex().Match(fileInfo.Name);

            if (match.Success)
            {
                return new PackageToUpload
                {
                    PackageFile = fileInfo,
                    Version = match.Groups["Version"].Value,
                    BuildNumber = UInt32.Parse(match.Groups["BuildNumber"].Value),
                    Cu = UInt32.TryParse(match.Groups["CU"].Value, out uint cuNumber) ? cuNumber : null,
                    GerritId = UInt32.TryParse(match.Groups["GER"].Value, out uint gerritId) ? gerritId : null,
                    PatchSet = UInt32.TryParse(match.Groups["PS"].Value, out uint patchSet) ? patchSet : null,
                    Type = Enum.TryParse(match.Groups["Type"].Value, true, out PackageType type) ? type : PackageType.Standard,
                    UpgradeType = Enum.TryParse(match.Groups["UpgradeType"].Value, true, out UpgradeType upgradeType) ? upgradeType : null
                };
            }

            return null;
        }

        [GeneratedRegex(@"DataMiner\s(?<Version>\d+\.\d+\.\d+\.\d+)(?:\(CU(?<CU>\d+)\))?-(?<BuildNumber>\d+)\s(?<UpgradeType>Full|Web)\sUpgrade(?:\s\((?<Type>rc|internal)\))?(?:\sGER-(?<GER>\d+))?(?:\sPS-(?<PS>\d+))?")]
        private static partial Regex DmUpgradeFileNameRegex();
    }
}