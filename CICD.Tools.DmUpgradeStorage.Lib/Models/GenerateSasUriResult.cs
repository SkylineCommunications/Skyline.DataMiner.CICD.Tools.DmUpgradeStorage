namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the result of a generate SAS URI operation.
    /// </summary>
    public class GenerateSasUriResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateSasUriResult"/> class.
        /// </summary>
        public GenerateSasUriResult()
        {
            SasUris = [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateSasUriResult"/> class with the specified collection of
        /// URIs.
        /// </summary>
        /// <param name="uris">A collection of URIs representing the SAS (Shared Access Signature) resources. Cannot be null. Each URI
        /// should be valid and accessible.</param>
        public GenerateSasUriResult(IEnumerable<Uri> uris)
        {
            SasUris = uris.ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateSasUriResult"/> class with the specified SAS URI.
        /// </summary>
        /// <param name="uri">The SAS URI to include in the result. Cannot be null.</param>
        public GenerateSasUriResult(Uri uri)
        {
            SasUris = [uri];
        }

        /// <summary>
        /// Gets or sets the generated SAS URIs.
        /// </summary>
        public List<Uri> SasUris { get; }
    }
}