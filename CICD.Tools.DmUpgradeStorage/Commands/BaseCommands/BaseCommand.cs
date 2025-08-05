namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.BaseCommands
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib;

    internal abstract class BaseCommand : Command
    {
        protected BaseCommand(string name, string? description = null) : base(name, description)
        {
            AddOption(new Option<string?>(
                aliases: ["--connection-string", "-cs"],
                description: $"The connection string of the blob storage to connect to. Not needed when using the account name and key. Will take precedence over account name and key. Can also be provided via an environment variable: '{EnvironmentVariables.BlobStorageConnectionString}'"));
            AddOption(new Option<string?>(
                aliases: ["--account-name", "-an"],
                description: $"The account name of the blob storage to connect to. Not needed when using the connection string. Can also be provided via an environment variable: '{EnvironmentVariables.BlobStorageAccountName}'"));
            AddOption(new Option<string?>(
                aliases: ["--account-key", "-ak"],
                description: $"The account key of the blob storage to connect to. Not needed when using the connection string. Can also be provided via an environment variable: '{EnvironmentVariables.BlobStorageAccountKey}'"));
            AddOption(new Option<string?>(
                aliases: ["--container-name", "-cn"],
                description: $"The container of the blob storage. Can also be provided via an environment variable: '{EnvironmentVariables.BlobStorageContainerName}'"));
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Automatic binding with System.CommandLine.NamingConventionBinder")]
    internal abstract class BaseCommandHandler : ICommandHandler
    {
        public string? AccountKey { get; set; }

        public string? AccountName { get; set; }

        public string? ConnectionString { get; set; }

        public string? ContainerName { get; set; }

        public int Invoke(InvocationContext context)
        {
            return (int)ExitCodes.NotImplemented;
        }

        public abstract Task<int> InvokeAsync(InvocationContext context);
    }
}