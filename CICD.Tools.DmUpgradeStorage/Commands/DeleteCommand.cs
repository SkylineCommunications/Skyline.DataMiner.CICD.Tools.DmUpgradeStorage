// ReSharper disable ClassNeverInstantiated.Global
namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.BaseCommands;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Services;

    internal class DeleteCommand : BaseCommand
    {
        public DeleteCommand() :
            base(name: "delete", description: "Delete a package from the storage.")
        {
            AddOption(new Option<string>(
                aliases: ["--package-name", "-pn"],
                description: "The package name that needs to be deleted.")
            {
                IsRequired = true
            });
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Automatic binding with System.CommandLine.NamingConventionBinder")]
    internal class DeleteCommandHandler(ILogger<DeleteCommandHandler> logger, IDmUpgradeStorageService storageService) : BaseCommandHandler
    {
        public required string PackageName { get; set; }

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

                bool result = await storageService.DeleteAsync(PackageName, context.GetCancellationToken());
                if (!result)
                {
                    logger.LogError("Failed to delete the package.");
                    return (int)ExitCodes.Fail;
                }

                return (int)ExitCodes.Ok;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete the upgrade package.");
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