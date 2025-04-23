// ReSharper disable ClassNeverInstantiated.Global
namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands
{
    using System;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.BaseCommands;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Services;

    /*
     * TODO: Remove this command (or at least comment it out)
     */

    internal class InfoCommand : BaseCommand
    {
        public InfoCommand() :
            base(name: "info", description: "Used for debugging.")
        {
            // Purely used for debugging
            IsHidden = true;
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Automatic binding with System.CommandLine.NamingConventionBinder")]
    internal class InfoCommandHandler(ILogger<InfoCommandHandler> logger, IDmUpgradeStorageService storageService) : BaseCommandHandler
    {
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

                PackageTagFilter builder = new PackageTagFilter();

                builder.WithType(PackageType.Rc);

                await foreach (var task in storageService.DownloadPackagesByTagsAsync(builder, context.GetCancellationToken()))
                {
                    var package = await task;

                    logger.LogInformation("Package: {name}", package.Name);
                }

                return (int)ExitCodes.Ok;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to receive all info.");
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