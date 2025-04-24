// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global
namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models;

    /// <summary>
    /// Class to build a filter for packages based on their tags.
    /// </summary>
    public class PackageTagFilter
    {
        private readonly BlobTagFilterBuilder builder = new BlobTagFilterBuilder();
        private bool containsExpression;

        /// <summary>
        /// Adds a filter for the package based on the build number.
        /// </summary>
        /// <param name="buildNumber">The build number to filter on.</param>
        /// <returns>A reference to this instance after the with operation has completed.</returns>
        public PackageTagFilter WithBuildNumber(uint buildNumber)
        {
            AddAndIfNeeded();
            builder.Equal(Constants.BuildNumberTagName, buildNumber.ToString());
            return this;
        }

        /// <summary>
        /// Adds a filter for the package based on the CU number.
        /// </summary>
        /// <param name="cuNumber">The CU number to filter on.</param>
        /// <returns>A reference to this instance after the with operation has completed.</returns>
        public PackageTagFilter WithCu(uint cuNumber)
        {
            AddAndIfNeeded();
            builder.Equal(Constants.CuTagName, cuNumber.ToString());
            return this;
        }

        /// <summary>
        /// Adds a filter for the package based on the Gerrit ID.
        /// </summary>
        /// <param name="gerritId">The gerrit ID to filter on.</param>
        /// <returns>A reference to this instance after the with operation has completed.</returns>
        public PackageTagFilter WithGerritId(uint gerritId)
        {
            AddAndIfNeeded();
            builder.Equal(Constants.GerritIdTagName, gerritId.ToString());
            return this;
        }

        /// <summary>
        /// Adds a filter for the package based on the patch set.
        /// </summary>
        /// <param name="patchSet">The patch set to filter on.</param>
        /// <returns>A reference to this instance after the with operation has completed.</returns>
        public PackageTagFilter WithPatchSet(uint patchSet)
        {
            AddAndIfNeeded();
            builder.Equal(Constants.PatchSetTagName, patchSet.ToString());
            return this;
        }

        /// <summary>
        /// Adds a filter for the package based on the type.
        /// </summary>
        /// <param name="type">The type of the package to filter on.</param>
        /// <returns>A reference to this instance after the with operation has completed.</returns>
        public PackageTagFilter WithType(PackageType type)
        {
            AddAndIfNeeded();
            builder.Equal(Constants.TypeTagName, type.ToString());
            return this;
        }

        /// <summary>
        /// Adds a filter for the package based on the upgrade type.
        /// </summary>
        /// <param name="upgradeType">The upgrade type to filter on.</param>
        /// <returns>A reference to this instance after the with operation has completed.</returns>
        public PackageTagFilter WithUpgradeType(UpgradeType upgradeType)
        {
            AddAndIfNeeded();
            builder.Equal(Constants.UpgradeTypeTagName, upgradeType.ToString());
            return this;
        }

        /// <summary>
        /// Adds a filter for the package based on the version.
        /// </summary>
        /// <param name="version">The version to filter on.</param>
        /// <returns>A reference to this instance after the with operation has completed.</returns>
        public PackageTagFilter WithVersion(string version)
        {
            AddAndIfNeeded();
            builder.Equal(Constants.VersionTagName, version);
            return this;
        }

        internal string Build() => builder.Build();

        internal bool IsEmpty() => !containsExpression;

        private void AddAndIfNeeded()
        {
            if (containsExpression)
            {
                builder.And();
            }
            else
            {
                containsExpression = true;
            }
        }
    }

    internal class BlobTagFilterBuilder
    {
        private readonly List<string> conditions = [];

        public BlobTagFilterBuilder And() => Append("AND");

        public string Build() => String.Join(" ", conditions);

        public BlobTagFilterBuilder Equal(string tag, string value) => Append($"\"{EscapeIdentifier(tag)}\" = '{EscapeValue(value)}'");

        public BlobTagFilterBuilder GreaterThan(string tag, string value) => Append($"\"{EscapeIdentifier(tag)}\" > '{EscapeValue(value)}'");

        public BlobTagFilterBuilder GreaterThanOrEqual(string tag, string value) => Append($"\"{EscapeIdentifier(tag)}\" >= '{EscapeValue(value)}'");

        public BlobTagFilterBuilder LessThan(string tag, string value) => Append($"\"{EscapeIdentifier(tag)}\" < '{EscapeValue(value)}'");

        public BlobTagFilterBuilder LessThanOrEqual(string tag, string value) => Append($"\"{EscapeIdentifier(tag)}\" <= '{EscapeValue(value)}'");

        private static string EscapeIdentifier(string identifier)
        {
            return identifier.Replace("'", "''");
        }

        private static string EscapeValue(string value)
        {
            return value.Replace("'", "''");
        }

        private BlobTagFilterBuilder Append(string condition)
        {
            conditions.Add(condition);
            return this;
        }
    }
}