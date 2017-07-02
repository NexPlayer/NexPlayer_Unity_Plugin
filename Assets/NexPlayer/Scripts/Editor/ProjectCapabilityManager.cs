using System;
using System.IO;

namespace NexPlayer.UnityEditor.iOS.Xcode
{
    // This class is here to help you add capabilities to your Xcode project.
    // Because capabilities modify the PBXProject, the entitlements file and/or the Info.plist and not consistently,
    // it can be tedious.
    // Therefore this class open the PBXProject that is always modify by capabilities and open Entitlement and info.plist only when needed.
    // For optimisation reasons, we write the file only in the close method.
    // If you don't call it the file will not be written.
    public class ProjectCapabilityManager
    {
        private readonly string m_BuildPath;
        private readonly string m_TargetGuid;
        private readonly string m_PBXProjectPath;
        private readonly string m_EntitlementFilePath;
        private PlistDocument m_Entitlements;
        private PlistDocument m_InfoPlist;
        protected internal PBXProject project;

        // Create the manager with the required parameter to open files and set the properties in the write place.
        public ProjectCapabilityManager(string pbxProjectPath, string entitlementFilePath, string targetName)
        {
            m_BuildPath = Directory.GetParent(Path.GetDirectoryName(pbxProjectPath)).FullName;

            m_EntitlementFilePath = entitlementFilePath;
            m_PBXProjectPath = pbxProjectPath;
            project = new PBXProject();
            project.ReadFromString(File.ReadAllText(m_PBXProjectPath));
            m_TargetGuid = project.TargetGuidByName(targetName);
        }

        // Write the actual file to the disk.
        // If you don't call this method nothing will change.
        public void WriteToFile()
        {
            File.WriteAllText(m_PBXProjectPath, project.WriteToString());
            if (m_Entitlements != null)
                m_Entitlements.WriteToFile(PBXPath.Combine(m_BuildPath, m_EntitlementFilePath));
            if (m_InfoPlist != null)
                m_InfoPlist.WriteToFile(PBXPath.Combine(m_BuildPath, "Info.plist"));
        }

        // Add the iCloud capability with the desired options.
        public void AddiCloud(bool keyValueStorage, bool iCloudDocument, string[] customContainers)
        {
            var ent = GetOrCreateEntitlementDoc();
            var val = (ent.root[ICloudEntitlements.ContainerIdValue] = new PlistElementArray()) as PlistElementArray;
            if (iCloudDocument)
            {
                val.values.Add(new PlistElementString(ICloudEntitlements.ContainerIdValue));
                var ser = (ent.root[ICloudEntitlements.ServicesKey] = new PlistElementArray()) as PlistElementArray;
                ser.values.Add(new PlistElementString(ICloudEntitlements.ServicesKitValue));
                ser.values.Add(new PlistElementString(ICloudEntitlements.ServicesDocValue));
                var ubiquity = (ent.root[ICloudEntitlements.UbiquityContainerIdKey] = new PlistElementArray()) as PlistElementArray;
                ubiquity.values.Add(new PlistElementString(ICloudEntitlements.UbiquityContainerIdValue));
                for (var i = 0; i < customContainers.Length; i++)
                {
                    ser.values.Add(new PlistElementString(customContainers[i]));
                }
            }

            if (keyValueStorage)
            {
                ent.root[ICloudEntitlements.KeyValueStoreKey] = new PlistElementString(ICloudEntitlements.KeyValueStoreValue);
            }

            project.AddCapability(m_TargetGuid, PBXCapabilityType.iCloud, m_EntitlementFilePath, iCloudDocument);
        }

        // Add Push (or remote) Notifications capability to your project
        public void AddPushNotifications(bool development)
        {
            GetOrCreateEntitlementDoc().root[PushNotificationEntitlements.Key] = new PlistElementString(development ? PushNotificationEntitlements.DevelopmentValue : PushNotificationEntitlements.ProductionValue);
            project.AddCapability(m_TargetGuid, PBXCapabilityType.PushNotifications, m_EntitlementFilePath);
        }

        // Add GameCenter capability to the project.
        public void AddGameCenter()
        {
            var arr = (GetOrCreateInfoDoc().root[GameCenterInfo.Key] ?? (GetOrCreateInfoDoc().root[GameCenterInfo.Key] = new PlistElementArray())) as PlistElementArray;
            arr.values.Add(new PlistElementString(GameCenterInfo.Value));
            project.AddCapability(m_TargetGuid, PBXCapabilityType.GameCenter);
        }

        // Add Wallet capability to the project.
        public void AddWallet(string[] passSubset)
        {
            var arr = (GetOrCreateEntitlementDoc().root[WalletEntitlements.Key] = new PlistElementArray()) as PlistElementArray;
            if ((passSubset == null || passSubset.Length == 0) && arr != null)
            {
                arr.values.Add(new PlistElementString(WalletEntitlements.BaseValue + WalletEntitlements.BaseValue));
            }
            else
            {
                for (var i = 0; i < passSubset.Length; i++)
                {
                    if (arr != null)
                        arr.values.Add(new PlistElementString(WalletEntitlements.BaseValue + passSubset[i]));
                }
            }

            project.AddCapability(m_TargetGuid, PBXCapabilityType.Wallet, m_EntitlementFilePath);
        }

        // Add Siri capability to the project.
        public void AddSiri()
        {
            GetOrCreateEntitlementDoc().root[SiriEntitlements.Key] = new PlistElementBoolean(true);

            project.AddCapability(m_TargetGuid, PBXCapabilityType.Siri, m_EntitlementFilePath);
        }

        // Add Apple Pay capability to the project.
        public void AddApplePay(string[] merchants)
        {
            var arr = (GetOrCreateEntitlementDoc().root[ApplePayEntitlements.Key] = new PlistElementArray()) as PlistElementArray;
            for (var i = 0; i < merchants.Length; i++)
            {
                arr.values.Add(new PlistElementString(merchants[i]));
            }

            project.AddCapability(m_TargetGuid, PBXCapabilityType.ApplePay, m_EntitlementFilePath);
        }

        // Add In App Purchase capability to the project.
        public void AddInAppPurchase()
        {
            project.AddCapability(m_TargetGuid, PBXCapabilityType.InAppPurchase);
        }

        // Add Maps capability to the project.
        public void AddMaps(MapsOptions options)
        {
            var bundleArr = (GetOrCreateInfoDoc().root[MapsInfo.BundleKey] ?? (GetOrCreateInfoDoc().root[MapsInfo.BundleKey] = new PlistElementArray())) as PlistElementArray;
            bundleArr.values.Add(new PlistElementDict());
            PlistElementDict bundleDic = GetOrCreateUniqueDictElementInArray(bundleArr);
            bundleDic[MapsInfo.BundleNameKey] = new PlistElementString(MapsInfo.BundleNameValue);
            var bundleTypeArr = (bundleDic[MapsInfo.BundleTypeKey] ?? (bundleDic[MapsInfo.BundleTypeKey]  = new PlistElementArray())) as PlistElementArray;
            GetOrCreateStringElementInArray(bundleTypeArr, MapsInfo.BundleTypeValue);

            var optionArr = (GetOrCreateInfoDoc().root[MapsInfo.ModeKey] ??
                            (GetOrCreateInfoDoc().root[MapsInfo.ModeKey] = new PlistElementArray())) as PlistElementArray;
            if ((options & MapsOptions.Airplane) == MapsOptions.Airplane)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModePlaneValue);
            }
            if ((options & MapsOptions.Bike) == MapsOptions.Bike)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModeBikeValue);
            }
            if ((options & MapsOptions.Bus) == MapsOptions.Bus)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModeBusValue);
            }
            if ((options & MapsOptions.Car) == MapsOptions.Car)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModeCarValue);
            }
            if ((options & MapsOptions.Ferry) == MapsOptions.Ferry)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModeFerryValue);
            }
            if ((options & MapsOptions.Other) == MapsOptions.Other)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModeOtherValue);
            }
            if ((options & MapsOptions.Pedestrian) == MapsOptions.Pedestrian)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModePedestrianValue);
            }
            if ((options & MapsOptions.RideSharing) == MapsOptions.RideSharing)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModeRideShareValue);
            }
            if ((options & MapsOptions.StreetCar) == MapsOptions.StreetCar)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModeStreetCarValue);
            }
            if ((options & MapsOptions.Subway) == MapsOptions.Subway)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModeSubwayValue);
            }
            if ((options & MapsOptions.Taxi) == MapsOptions.Taxi)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModeTaxiValue);
            }
            if ((options & MapsOptions.Train) == MapsOptions.Train)
            {
                GetOrCreateStringElementInArray(optionArr, MapsInfo.ModeTrainValue);
            }

            project.AddCapability(m_TargetGuid, PBXCapabilityType.Maps);
        }

        // Add Personal VPN capability to the project.
        public void AddPersonalVPN()
        {
            var arr = (GetOrCreateEntitlementDoc().root[VPNEntitlements.Key] = new PlistElementArray()) as PlistElementArray;
            arr.values.Add(new PlistElementString(VPNEntitlements.Value));

            project.AddCapability(m_TargetGuid, PBXCapabilityType.PersonalVPN, m_EntitlementFilePath);
        }

        // Add Background capability to the project with the options wanted.
        public void AddBackgroundModes(BackgroundModesOptions options)
        {
            var optionArr = (GetOrCreateInfoDoc().root[BackgroundInfo.Key] ??
                            (GetOrCreateInfoDoc().root[BackgroundInfo.Key] = new PlistElementArray())) as PlistElementArray;

            if ((options & BackgroundModesOptions.ActsAsABluetoothLEAccessory) == BackgroundModesOptions.ActsAsABluetoothLEAccessory)
            {
                GetOrCreateStringElementInArray(optionArr, BackgroundInfo.ModeActsBluetoothValue);
            }
            if ((options & BackgroundModesOptions.AudioAirplayPiP) == BackgroundModesOptions.AudioAirplayPiP)
            {
                GetOrCreateStringElementInArray(optionArr, BackgroundInfo.ModeAudioValue);
            }
            if ((options & BackgroundModesOptions.BackgroundFetch) == BackgroundModesOptions.BackgroundFetch)
            {
                GetOrCreateStringElementInArray(optionArr, BackgroundInfo.ModeFetchValue);
            }
            if ((options & BackgroundModesOptions.ExternalAccessoryCommunication) == BackgroundModesOptions.ExternalAccessoryCommunication)
            {
                GetOrCreateStringElementInArray(optionArr, BackgroundInfo.ModeExtAccessoryValue);
            }
            if ((options & BackgroundModesOptions.LocationUpdates) == BackgroundModesOptions.LocationUpdates)
            {
                GetOrCreateStringElementInArray(optionArr, BackgroundInfo.ModeLocationValue);
            }
            if ((options & BackgroundModesOptions.NewsstandDownloads) == BackgroundModesOptions.NewsstandDownloads)
            {
                GetOrCreateStringElementInArray(optionArr, BackgroundInfo.ModeNewsstandValue);
            }
            if ((options & BackgroundModesOptions.RemoteNotifications) == BackgroundModesOptions.RemoteNotifications)
            {
                GetOrCreateStringElementInArray(optionArr, BackgroundInfo.ModePushValue);
            }
            if ((options & BackgroundModesOptions.VoiceOverIP) == BackgroundModesOptions.VoiceOverIP)
            {
                GetOrCreateStringElementInArray(optionArr, BackgroundInfo.ModeVOIPValue);
            }
            project.AddCapability(m_TargetGuid, PBXCapabilityType.BackgroundModes);
        }

        // Add Keychain Sharing capability to the project with a list of groups.
        public void AddKeychainSharing(string[] accessGroups)
        {
            var arr = (GetOrCreateEntitlementDoc().root[KeyChainEntitlements.Key] = new PlistElementArray()) as PlistElementArray;
            if (accessGroups != null)
            {
                for (var i = 0; i < accessGroups.Length; i++)
                {
                    arr.values.Add(new PlistElementString(accessGroups[i]));
                }
            }
            else
            {
                arr.values.Add(new PlistElementString(KeyChainEntitlements.DefaultValue));
            }

            project.AddCapability(m_TargetGuid, PBXCapabilityType.KeychainSharing, m_EntitlementFilePath);
        }

        // Add Inter App Audio capability to the project.
        public void AddInterAppAudio()
        {
            GetOrCreateEntitlementDoc().root[AudioEntitlements.Key] = new PlistElementBoolean(true);
            project.AddCapability(m_TargetGuid, PBXCapabilityType.InterAppAudio, m_EntitlementFilePath);
        }

        // Add Associated Domains capability to the project.
        public void AddAssociatedDomains(string[] domains)
        {
            var arr = (GetOrCreateEntitlementDoc().root[AssociatedDomainsEntitlements.Key] = new PlistElementArray()) as PlistElementArray;
            for (var i = 0; i < domains.Length; i++)
            {
                arr.values.Add(new PlistElementString(domains[i]));
            }

            project.AddCapability(m_TargetGuid, PBXCapabilityType.AssociatedDomains, m_EntitlementFilePath);
        }

        // Add App Groups capability to the project.
        public void AddAppGroups(string[] groups)
        {
            var arr = (GetOrCreateEntitlementDoc().root[AppGroupsEntitlements.Key] = new PlistElementArray()) as PlistElementArray;
            for (var i = 0; i < groups.Length; i++)
            {
                arr.values.Add(new PlistElementString(groups[i]));
            }

            project.AddCapability(m_TargetGuid, PBXCapabilityType.AppGroups, m_EntitlementFilePath);
        }

        // Add HomeKit capability to the project.
        public void AddHomeKit()
        {
            GetOrCreateEntitlementDoc().root[HomeKitEntitlements.Key] = new PlistElementBoolean(true);
            project.AddCapability(m_TargetGuid, PBXCapabilityType.HomeKit, m_EntitlementFilePath);
        }

        // Add Data Protection capability to the project.
        public void AddDataProtection()
        {
            GetOrCreateEntitlementDoc().root[DataProtectionEntitlements.Key] = new PlistElementString(DataProtectionEntitlements.Value);
            project.AddCapability(m_TargetGuid, PBXCapabilityType.DataProtection, m_EntitlementFilePath);
        }

        // Add HealthKit capability to the project.
        public void AddHealthKit()
        {
            var capabilityArr = (GetOrCreateInfoDoc().root[HealthInfo.Key] ??
                                (GetOrCreateInfoDoc().root[HealthInfo.Key] = new PlistElementArray())) as PlistElementArray;
            GetOrCreateStringElementInArray(capabilityArr, HealthInfo.Value);
            GetOrCreateEntitlementDoc().root[HealthKitEntitlements.Key] = new PlistElementBoolean(true);
            project.AddCapability(m_TargetGuid, PBXCapabilityType.HealthKit, m_EntitlementFilePath);
        }

        // Add Wireless Accessory Configuration capability to the project.
        public void AddWirelessAccessoryConfiguration()
        {
            GetOrCreateEntitlementDoc().root[WirelessAccessoryConfigurationEntitlements.Key] = new PlistElementBoolean(true);
            project.AddCapability(m_TargetGuid, PBXCapabilityType.WirelessAccessoryConfiguration, m_EntitlementFilePath);
        }

        private PlistDocument GetOrCreateEntitlementDoc()
        {
            if (m_Entitlements == null)
            {
                m_Entitlements = new PlistDocument();
                string[] entitlementsFiles = Directory.GetFiles(m_BuildPath, m_EntitlementFilePath);
                if (entitlementsFiles.Length > 0)
                {
                    m_Entitlements.ReadFromFile(entitlementsFiles[0]);
                }
                else
                {
                    m_Entitlements.Create();
                }
            }

            return m_Entitlements;
        }

        private PlistDocument GetOrCreateInfoDoc()
        {
            if (m_InfoPlist == null)
            {
                m_InfoPlist = new PlistDocument();
                string[] infoFiles = Directory.GetFiles(m_BuildPath + "/", "Info.plist");
                if (infoFiles.Length > 0)
                {
                    m_InfoPlist.ReadFromFile(infoFiles[0]);
                }
                else
                {
                    m_InfoPlist.Create();
                }
            }

            return m_InfoPlist;
        }

        private PlistElementString GetOrCreateStringElementInArray(PlistElementArray root, string value)
        {
            PlistElementString r = null;
            var c = root.values.Count;
            var exist = false;
            for (var i = 0; i < c; i++)
            {
                if (root.values[i] is PlistElementString && (root.values[i] as PlistElementString).value == value)
                {
                    r = root.values[i] as PlistElementString;
                    exist = true;
                }
            }
            if (!exist)
            {
                r = new PlistElementString(value);
                root.values.Add(r);
            }
            return r;
        }

        private PlistElementDict GetOrCreateUniqueDictElementInArray(PlistElementArray root)
        {
            PlistElementDict r;
            if (root.values.Count == 0)
            {
                r = root.values[0] as PlistElementDict;
            }
            else
            {
                r = new PlistElementDict();
                root.values.Add(r);
            }
            return r;
        }
    }

    // The list of options available for Background Mode.
    [Flags]
    [Serializable]
    public enum BackgroundModesOptions
    {
        None                           = 0,
        AudioAirplayPiP                = 1<<0,
        LocationUpdates                = 1<<1,
        VoiceOverIP                    = 1<<2,
        NewsstandDownloads             = 1<<3,
        ExternalAccessoryCommunication = 1<<4,
        UsesBluetoothLEAccessory       = 1<<5,
        ActsAsABluetoothLEAccessory    = 1<<6,
        BackgroundFetch                = 1<<7,
        RemoteNotifications            = 1<<8
    }

    // The list of options available for Maps.
    [Serializable]
    [Flags]
    public enum MapsOptions
    {
        None          = 0,
        Airplane      = 1<<0,
        Bike          = 1<<1,
        Bus           = 1<<2,
        Car           = 1<<3,
        Ferry         = 1<<4,
        Pedestrian    = 1<<5,
        RideSharing   = 1<<6,
        StreetCar     = 1<<7,
        Subway        = 1<<8,
        Taxi          = 1<<9,
        Train         = 1<<10,
        Other         = 1<<11
    }

    /* Follows the large quantity of string used as key and value all over the place in the info.plist or entitlements file. */
    internal class GameCenterInfo
    {
        internal static readonly string Key = "UIRequiredDeviceCapabilities";
        internal static readonly string Value = "gamekit";
    }

    internal class MapsInfo
    {
        internal static readonly string BundleKey = "CFBundleDocumentTypes";
        internal static readonly string BundleNameKey = "CFBundleTypeName";
        internal static readonly string BundleNameValue = "MKDirectionsRequest";
        internal static readonly string BundleTypeKey = "LSItemContentTypes";
        internal static readonly string BundleTypeValue = "com.apple.maps.directionsrequest";
        internal static readonly string ModeKey = "MKDirectionsApplicationSupportedModes";
        internal static readonly string ModePlaneValue = "MKDirectionsModePlane";
        internal static readonly string ModeBikeValue = "MKDirectionsModeBike";
        internal static readonly string ModeBusValue = "MKDirectionsModeBus";
        internal static readonly string ModeCarValue = "MKDirectionsModeCar";
        internal static readonly string ModeFerryValue = "MKDirectionsModeFerry";
        internal static readonly string ModeOtherValue = "MKDirectionsModeOther";
        internal static readonly string ModePedestrianValue = "MKDirectionsModePedestrian";
        internal static readonly string ModeRideShareValue = "MKDirectionsModeRideShare";
        internal static readonly string ModeStreetCarValue = "MKDirectionsModeStreetCar";
        internal static readonly string ModeSubwayValue = "MKDirectionsModeSubway";
        internal static readonly string ModeTaxiValue = "MKDirectionsModeTaxi";
        internal static readonly string ModeTrainValue = "MKDirectionsModeTrain";
    }

    internal class BackgroundInfo
    {
        internal static readonly string Key = "UIBackgroundModes";
        internal static readonly string ModeAudioValue = "audio";
        internal static readonly string ModeBluetoothValue = "bluetooth-central";
        internal static readonly string ModeActsBluetoothValue = "bluetooth-peripheral";
        internal static readonly string ModeExtAccessoryValue = "external-accessory";
        internal static readonly string ModeFetchValue = "fetch";
        internal static readonly string ModeLocationValue = "location";
        internal static readonly string ModeNewsstandValue = "newsstand-content";
        internal static readonly string ModePushValue = "remote-notification";
        internal static readonly string ModeVOIPValue = "voip";
    }

    internal class HealthInfo
    {
        internal static readonly string Key = "UIRequiredDeviceCapabilities";
        internal static readonly string Value = "healthkit";
    }

    internal class ICloudEntitlements
    {
        internal static readonly string ContainerIdKey = "com.apple.developer.icloud-container-identifiers";
        internal static readonly string UbiquityContainerIdKey = "com.apple.developer.ubiquity-container-identifiers";
        internal static readonly string ContainerIdValue = "iCloud.$(CFBundleIdentifier)";
        internal static readonly string UbiquityContainerIdValue = "iCloud.$(CFBundleIdentifier)";
        internal static readonly string ServicesKey = "com.apple.developer.icloud-services";
        internal static readonly string ServicesDocValue = "CloudDocuments";
        internal static readonly string ServicesKitValue = "CloudKit";
        internal static readonly string KeyValueStoreKey = "com.apple.developer.ubiquity-kvstore-identifier";
        internal static readonly string KeyValueStoreValue = "$(TeamIdentifierPrefix)$(CFBundleIdentifier)";
    }

    internal class PushNotificationEntitlements
    {
        internal static readonly string Key = "aps-environment";
        internal static readonly string DevelopmentValue = "development";
        internal static readonly string ProductionValue = "production";
    }

    internal class WalletEntitlements
    {
        internal static readonly string Key = "com.apple.developer.pass-type-identifiers";
        internal static readonly string BaseValue = "$(TeamIdentifierPrefix)";
        internal static readonly string DefaultValue = "*";
    }

    internal class SiriEntitlements
    {
        internal static readonly string Key = "com.apple.developer.siri";
    }

    internal class ApplePayEntitlements
    {
        internal static readonly string Key = "com.apple.developer.in-app-payments";
    }

    internal class VPNEntitlements
    {
        internal static readonly string Key = "com.apple.developer.networking.vpn.api";
        internal static readonly string Value = "allow-vpn";
    }

    internal class KeyChainEntitlements
    {
        internal static readonly string Key = "keychain-access-groups";
        internal static readonly string DefaultValue = "$(AppIdentifierPrefix)$(CFBundleIdentifier)";
    }

    internal class AudioEntitlements
    {
        internal static readonly string Key = "inter-app-audio";
    }

    internal class AssociatedDomainsEntitlements
    {
        // value is an array of string of domains
        internal static readonly string Key = "com.apple.developer.associated-domains";
    }

    internal class AppGroupsEntitlements
    {
        // value is an array of string of groups
        internal static readonly string Key = "com.apple.security.application-groups";
    }

    internal class HomeKitEntitlements
    {
        // value is bool true.
        internal static readonly string Key = "com.apple.developer.homekit";
    }

    internal class DataProtectionEntitlements
    {
        internal static readonly string Key = "com.apple.developer.default-data-protection";
        internal static readonly string Value = "NSFileProtectionComplete";
    }

    internal class HealthKitEntitlements
    {
        // value is bool true.
        internal static readonly string Key = "com.apple.developer.healthkit";
    }

    internal class WirelessAccessoryConfigurationEntitlements
    {
        // value is bool true.
        internal static readonly string Key = "com.apple.external-accessory.wireless-configuration";
    }
}
