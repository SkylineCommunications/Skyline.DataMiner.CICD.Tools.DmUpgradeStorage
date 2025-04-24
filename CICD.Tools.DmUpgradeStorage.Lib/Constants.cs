namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib
{
    internal static class Constants
    {
        /// <summary>
        /// The name of the tag used to store the build number of the package.
        /// </summary>
        public const string BuildNumberTagName = "BuildNumber";

        /// <summary>
        /// The name of the tag used to store the CU number of the package.
        /// </summary>
        public const string CuTagName = "CU";

        /// <summary>
        /// The name of the tag used to store the Gerrit ID of the package.
        /// </summary>
        public const string GerritIdTagName = "GerritId";

        /// <summary>
        /// The name of the tag used to store the patch set number of the package.
        /// </summary>
        public const string PatchSetTagName = "PatchSet";

        /// <summary>
        /// The name of the tag used to store the type of the package.
        /// </summary>
        public const string TypeTagName = "Type";

        /// <summary>
        /// The name of the tag used to store the upgrade type of the package.
        /// </summary>
        public const string UpgradeTypeTagName = "UpgradeType";

        /// <summary>
        /// The name of the tag used to store the version of the package.
        /// </summary>
        public const string VersionTagName = "Version";
    }
}