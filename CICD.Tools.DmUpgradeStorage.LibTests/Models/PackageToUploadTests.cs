namespace CICD.Tools.DmUpgradeStorage.LibTests.Models
{
    using FluentAssertions;

    using Skyline.DataMiner.CICD.FileSystem.FileInfoWrapper;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models;

    [TestClass]
    public class PackageToUploadTests
    {
        [TestMethod]
        [DynamicData(nameof(AdditionalData))]
        public void PackageToUpload_FromFile(string fileName, PackageToUpload? expectedPackage)
        {
            // Act
            PackageToUpload? result = PackageToUpload.FromFile(new FileInfo(fileName));

            // Assert
            result.Should().BeEquivalentTo(expectedPackage, config: options => options.Excluding(package => package!.PackageFile));
        }

        public static IEnumerable<object?[]> AdditionalData =>
        [
            ["invalid file.dmupgrade", null],
            [
                "DataMiner 10.5.5.0-15596 Full Upgrade (rc) GER-99162 PS-19 - random text.dmupgrade", new PackageToUpload
                {
                    PackageFile = new FileInfo("DataMiner 10.5.5.0-15596 Full Upgrade (rc) GER-99162 PS-19 - random text.dmupgrade"),
                    BuildNumber = 15596,
                    Version = "10.5.5.0",
                    Cu = null,
                    Type = PackageType.Rc,
                    GerritId = 99162,
                    PatchSet = 19,
                    UpgradeType = UpgradeType.Full
                }
            ],
            [
                "DataMiner 10.5.5.0-15596 Full Upgrade (rc) GER-99162 PS-19.dmupgrade", new PackageToUpload
                {
                    PackageFile = new FileInfo("DataMiner 10.5.5.0-15596 Full Upgrade (rc) GER-99162 PS-19.dmupgrade"),
                    BuildNumber = 15596,
                    Version = "10.5.5.0",
                    Cu = null,
                    Type = PackageType.Rc,
                    GerritId = 99162,
                    PatchSet = 19,
                    UpgradeType = UpgradeType.Full
                }
            ],
            [
                "DataMiner 10.5.6.0-15384 Full Upgrade.dmupgrade", new PackageToUpload
                {
                    PackageFile = new FileInfo("DataMiner 10.5.6.0-15384 Full Upgrade.dmupgrade"),
                    BuildNumber = 15384,
                    Version = "10.5.6.0",
                    Cu = null,
                    Type = PackageType.Standard,
                    GerritId = null,
                    PatchSet = null,
                    UpgradeType = UpgradeType.Full
                }
            ],
            [
                "DataMiner 10.5.5.0(CU0)-15664 Full Upgrade (internal).dmupgrade", new PackageToUpload
                {
                    PackageFile = new FileInfo("DataMiner 10.5.5.0(CU0)-15664 Full Upgrade (internal).dmupgrade"),
                    BuildNumber = 15664,
                    Version = "10.5.5.0",
                    Cu = 0,
                    Type = PackageType.Internal,
                    GerritId = null,
                    PatchSet = null,
                    UpgradeType = UpgradeType.Full
                }
            ],
            [
                "DataMiner 1.2.3.4(CU186)-99999999 Full Upgrade (blabla).dmupgrade", new PackageToUpload
                {
                    PackageFile = new FileInfo("DataMiner 1.2.3.4(CU186)-99999999 Full Upgrade (blabla).dmupgrade"),
                    BuildNumber = 99999999,
                    Version = "1.2.3.4",
                    Cu = 186,
                    Type = PackageType.Standard,
                    GerritId = null,
                    PatchSet = null,
                    UpgradeType = UpgradeType.Full
                }
            ],
            [
                "DataMiner 10.4.0.0-15634 Web Upgrade (internal).dmupgrade", new PackageToUpload
                {
                    PackageFile = new FileInfo("DataMiner 10.4.0.0-15634 Web Upgrade (internal).dmupgrade"),
                    BuildNumber = 15634,
                    Version = "10.4.0.0",
                    Cu = null,
                    Type = PackageType.Internal,
                    GerritId = null,
                    PatchSet = null,
                    UpgradeType = UpgradeType.Web
                }
            ],

        ];
    }
}