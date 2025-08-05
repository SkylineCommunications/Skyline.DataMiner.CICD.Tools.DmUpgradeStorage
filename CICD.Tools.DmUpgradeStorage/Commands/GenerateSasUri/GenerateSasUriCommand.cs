namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.GenerateSasUri
{
    using System.CommandLine;

    internal class GenerateSasUriCommand : Command
    {
        public GenerateSasUriCommand() :
            base(name: "generate-sas-uri", description: "Generate a SAS URI for downloading dmupgrade packages.")
        {
            AddCommand(new GenerateSasUriByNameCommand());
            AddCommand(new GenerateSasUriLatestByTagCommand());
            AddCommand(new GenerateSasUriByTagCommand());
        }
    }
}