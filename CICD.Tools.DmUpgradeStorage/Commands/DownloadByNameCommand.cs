// ReSharper disable ClassNeverInstantiated.Global
namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Skyline.DataMiner.CICD.FileSystem.DirectoryInfoWrapper;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.BaseCommands;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Services;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.SystemCommandLine;

    internal class DownloadByNameCommand : BaseCommand
    {
        public DownloadByNameCommand() :
            base(name: "by-name", description: "Download an dmupgrade package by name.")
        {
            AddOption(new Option<IDirectoryInfoIO>(
                aliases: ["--output-directory", "-od"],
                description: "The directory where the package(s) will be stored.",
                parseArgument: OptionHelper.ParseDirectoryInfo!)
            {
                IsRequired = true
            }.LegalFilePathsOnly());

            AddOption(new Option<string>(
                aliases: ["--name", "-n"],
                description: "Retrieve package via name.")
            {
                IsRequired = true
            });
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Automatic binding with System.CommandLine.NamingConventionBinder")]
    internal class DownloadByNameCommandHandler(ILogger<DownloadByNameCommandHandler> logger, IDmUpgradeStorageService storageService) : BaseCommandHandler
    {
        public required string Name { get; set; }

        public required IDirectoryInfoIO OutputDirectory { get; set; }

        public override int Invoke(InvocationContext context)
        {
            return (int)ExitCodes.NotImplemented;
        }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            DebugLog.Start(logger);
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                storageService.Authenticate(ConnectionString, AccountName, AccountKey);
                storageService.SetContainer(ContainerName);

                // Create directory first to make sure that it can be created
                OutputDirectory.Create();

                var package = await storageService.DownloadByNameAsync(Name, context.GetCancellationToken());
                if (package == null)
                {
                    logger.LogError("No package found with the provided name: {name}", Name);
                    return (int)ExitCodes.Fail;
                }

                await using Stream stream = package.Content;
                await using FileStream fileStream = new FileStream(Path.Combine(OutputDirectory.FullName, package.Name), FileMode.Create);
                await stream.CopyToAsync(fileStream, context.GetCancellationToken());

                logger.LogInformation("Downloaded package {packageName} to {outputDirectory}.", package.Name, OutputDirectory.FullName);

                return (int)ExitCodes.Ok;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to download the package.");
                return (int)ExitCodes.UnexpectedException;
            }
            finally
            {
                sw.Stop();
                DebugLog.End(logger, sw);
            }
        }
    }
}