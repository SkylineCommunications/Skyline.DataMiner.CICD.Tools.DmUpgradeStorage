// ReSharper disable ClassNeverInstantiated.Global
namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Skyline.DataMiner.CICD.FileSystem.FileInfoWrapper;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.BaseCommands;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Services;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.SystemCommandLine;

    internal class UploadCommand : BaseCommand
    {
        public UploadCommand() :
            base(name: "upload", description: "Upload a dmupgrade package to the storage.")
        {
            AddOption(new Option<IFileInfoIO>(
                aliases: ["--dmupgrade-file", "-df"],
                description: "The file that will be created that holds the information.",
                parseArgument: OptionHelper.ParseFileInfo!)
            {
                IsRequired = true
            }.LegalFilePathsOnly()!.ExistingOnly());

            AddOption(new Option<string?>(
                aliases: ["--version", "-dv"],
                description: "Represents the DataMiner version. Expected format: X.X.X.X"));

            AddOption(new Option<uint?>(
                aliases: ["--build-number", "-bn"],
                description: "Represents the build number of the upgrade package."));

            AddOption(new Option<uint?>(
                aliases: ["--cu", "-cu"],
                description: "Represents the CU number."));

            AddOption(new Option<uint?>(
                aliases: ["--gerrit-id", "-gi"],
                description: "Represents the gerrit ID."));

            AddOption(new Option<uint?>(
                aliases: ["--patch-set", "-ps"],
                description: "Represents the patch set number of the gerrit item."));

            AddOption(new Option<PackageType?>(
                aliases: ["--package-type"],
                description: "What type of package is this?"));

            AddOption(new Option<UpgradeType?>(
                aliases: ["--upgrade-type", "-ut"],
                description: "What type of upgrade is this?"));

            AddOption(new Option<IFileInfoIO>(
                aliases: ["--output-file", "-of"],
                description: "Contains the unique identifier of the uploaded package. Will be created or overwritten.",
                parseArgument: OptionHelper.ParseFileInfo!)
            {
                IsRequired = true
            }.LegalFilePathsOnly());
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Automatic binding with System.CommandLine.NamingConventionBinder")]
    internal class UploadCommandHandler(ILogger<UploadCommandHandler> logger, IDmUpgradeStorageService storageService) : BaseCommandHandler
    {
        public uint? BuildNumber { get; set; }

        public uint? Cu { get; set; }

        public required IFileInfoIO DmUpgradeFile { get; set; }

        public uint? GerritId { get; set; }

        public PackageType? PackageType { get; set; }

        public uint? PatchSet { get; set; }

        public UpgradeType? UpgradeType { get; set; }

        public string? Version { get; set; }

        public required IFileInfoIO OutputFile { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            DebugLog.Start(logger);
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                if (DmUpgradeFile.Extension != ".dmupgrade")
                {
                    logger.LogError("Invalid file type. The file must be a .dmupgrade file.");
                    return (int)ExitCodes.Fail;
                }

                storageService.Authenticate(ConnectionString, AccountName, AccountKey);
                storageService.SetContainer(ContainerName);

                PackageToUpload packageToUpload = CreateUploadPackage();

                UploadResult result = await storageService.UploadAsync(packageToUpload, context.GetCancellationToken());

                if (!result.Success)
                {
                    logger.LogError("Failed to upload the upgrade package.");
                    return (int)ExitCodes.Fail;
                }

                var outputFile = new UploadOutputFile
                {
                    UniqueIdentifier = result.Identifier
                };

                // Make sure the directory is created
                OutputFile.Directory.Create();
                await using StreamWriter streamWriter = OutputFile.CreateText();
                await streamWriter.WriteAsync(JsonSerializer.Serialize(outputFile));

                return (int)ExitCodes.Ok;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to upload the upgrade package.");
                return (int)ExitCodes.UnexpectedException;
            }
            finally
            {
                sw.Stop();
                DebugLog.End(logger, sw);
            }
        }

        private PackageToUpload CreateUploadPackage()
        {
            PackageToUpload? package = PackageToUpload.FromFile(DmUpgradeFile);

            if (package == null)
            {
                logger.LogDebug("Failed to create {className} from the provided file: {filePath}", nameof(PackageToUpload), DmUpgradeFile.FullName);

                if (Version == null)
                {
                    throw new ArgumentException("Version could not be retrieved from the file or is not provided to the command.");
                }

                if (BuildNumber == null)
                {
                    throw new ArgumentException("BuildNumber could not be retrieved from the file or is not provided to the command.");
                }

                package = new PackageToUpload
                {
                    PackageFile = DmUpgradeFile,
                    Version = Version,
                    BuildNumber = BuildNumber.Value,
                };
            }

            // Overwrite the properties if supplied via the command.
            if (!String.IsNullOrWhiteSpace(Version))
            {
                logger.LogDebug("Overwriting {propName} from file ({fileValue}) with specified value ({value}).", nameof(package.Version), package.Version, Version);
                package.Version = Version;
            }

            if (BuildNumber != null)
            {
                logger.LogDebug("Overwriting {propName} from file ({fileValue}) with specified value ({value}).", nameof(package.BuildNumber), package.BuildNumber, BuildNumber);
                package.BuildNumber = BuildNumber.Value;
            }

            if (Cu != null)
            {
                logger.LogDebug("Overwriting {propName} from file ({fileValue}) with specified value ({value}).", nameof(package.Cu), package.Cu, Cu);
                package.Cu = Cu;
            }

            if (GerritId != null)
            {
                logger.LogDebug("Overwriting {propName} from file ({fileValue}) with specified value ({value}).", nameof(package.GerritId), package.GerritId, GerritId);
                package.GerritId = GerritId;
            }

            if (PatchSet != null)
            {
                logger.LogDebug("Overwriting {propName} from file ({fileValue}) with specified value ({value}).", nameof(package.PatchSet), package.PatchSet, PatchSet);
                package.PatchSet = PatchSet;
            }

            if (PackageType != null)
            {
                logger.LogDebug("Overwriting {propName} from file ({fileValue}) with specified value ({value}).", nameof(package.Type), package.Type, PackageType);
                package.Type = PackageType.Value;
            }

            if (UpgradeType != null)
            {
                logger.LogDebug("Overwriting {propName} from file ({fileValue}) with specified value ({value}).", nameof(package.UpgradeType), package.UpgradeType, UpgradeType);
                package.UpgradeType = UpgradeType;
            }

            return package;
        }
    }

    internal class UploadOutputFile
    {
        public required Guid UniqueIdentifier { get; set; }
    }
}