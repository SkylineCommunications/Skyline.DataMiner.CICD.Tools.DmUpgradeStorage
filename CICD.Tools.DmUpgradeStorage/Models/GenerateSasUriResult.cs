namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the result of a generate SAS URI operation.
    /// </summary>
    public class GenerateSasUriResult
    {
        /// <summary>
        /// Gets or sets the generated SAS URIs.
        /// </summary>
        public List<Uri> SasUris { get; set; } = new List<Uri>();
    }
}