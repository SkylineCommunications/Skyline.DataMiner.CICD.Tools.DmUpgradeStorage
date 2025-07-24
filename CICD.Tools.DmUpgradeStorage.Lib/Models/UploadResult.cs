namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models
{
    using System;

    /// <summary>
    /// Represents the result of an upload operation, including its success status and a unique identifier for the
    /// uploaded package.
    /// </summary>
    /// <param name="Success">Indicates if the upload was successful.</param>
    /// <param name="Identifier">Unique identifier of the uploaded package.</param>
    public record UploadResult(bool Success, Guid Identifier);
}