using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;

// Azure Management dependencies
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Compute.Models;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Azure.Management.Storage;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Management.Network.Models;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var secret = Environment.GetEnvironmentVariable("AZURE_SECRET");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            if(new List<string>{ tenantId, clientId, secret, subscriptionId }.Any(i => String.IsNullOrEmpty(i))) {
                Console.WriteLine("Please provide ENV vars for AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_SECRET and AZURE_SUBSCRIPTION_ID.");
            }
            else
            {
                RunSample(tenantId, clientId, secret, subscriptionId).Wait();                
            }
        }

        public static async Task RunSample(string tenantId, string clientId, string secret, string subscriptionId)
        {
            // Build the service credentials and Azure Resource Manager clients
            var serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, secret);
            var resourceClient = new ResourceManagementClient(serviceCreds);
            resourceClient.SubscriptionId = subscriptionId;
            var computeClient = new ComputeManagementClient(serviceCreds);
            computeClient.SubscriptionId = subscriptionId;
            var storageClient = new StorageManagementClient(serviceCreds);
            storageClient.SubscriptionId = subscriptionId;
            var networkClient = new NetworkManagementClient(serviceCreds);
            networkClient.SubscriptionId = subscriptionId;

            var resourceGroupName = "sample-dotnet-vm-group";
            var westus = "westus";

            // Create the resource group
            Write("Creating resource group: {0}", westus);
            resourceClient.ResourceGroups.CreateOrUpdate(resourceGroupName, new ResourceGroup { Location = westus});

            // Create the storage account
            Random r = new Random();
            int postfix = r.Next(0, 1000000);
            var storageAccountName = String.Format("dotnetstor{1}", resourceGroupName, postfix);
            Write("Creating a premium storage account with encryption off named {0} in resource group {1}", storageAccountName, resourceGroupName);
            var storCreateParams = new StorageAccountCreateParameters {
                Location = westus,
                Sku = new Microsoft.Azure.Management.Storage.Models.Sku(SkuName.PremiumLRS, SkuTier.Premium),
                Kind = Microsoft.Azure.Management.Storage.Models.Kind.Storage,
                Encryption = new Encryption(new EncryptionServices(new EncryptionService(false))),
            };
            storageClient.StorageAccounts.Create(resourceGroupName, storageAccountName, storCreateParams);

            // Create the virtual network
            Write("Creating a virtual network for the VM"); 
            var vnetCreateParams = new VirtualNetwork {
                Location = westus,
                AddressSpace = new AddressSpace{ AddressPrefixes = new []{ "10.0.0.0/16" } },
                DhcpOptions = new DhcpOptions{ DnsServers = new []{ "8.8.8.8" } },
                Subnets = new List<Subnet>{ new Subnet{ Name = "dotnetsubnet", AddressPrefix = "10.0.0.0/24" } }
            };
            var vnet = networkClient.VirtualNetworks.CreateOrUpdate(resourceGroupName, "sample-dotnet-vnet", vnetCreateParams);

            // Create the public IP
            Write("Creating a public IP address for the VM");            
            var publicIpCreateParams = new PublicIPAddress {
                Location = westus,
                PublicIPAllocationMethod = IPAllocationMethod.Dynamic,
                DnsSettings = new PublicIPAddressDnsSettings{ DomainNameLabel = "sample-dotnet-domain-name-label" }
            };
            var pubIp = networkClient.PublicIPAddresses.CreateOrUpdate(resourceGroupName, "sample-dotnet-pubip", publicIpCreateParams);

            // Create the network interface
            Write("Creating a network interface for the VM");            
            var vnetNicCreateParams = new NetworkInterface {
                Location = westus,
                IpConfigurations = new List<NetworkInterfaceIPConfiguration>{ 
                    new NetworkInterfaceIPConfiguration { 
                        Name = "sample-dotnet-nic",
                        PrivateIPAllocationMethod = IPAllocationMethod.Dynamic,
                        Subnet = vnet.Subnets.First(),
                        PublicIPAddress = pubIp
                    } 
                }
            };
            var nic = networkClient.NetworkInterfaces.CreateOrUpdate(resourceGroupName, "sample-dotnet-nic", vnetNicCreateParams);

            Write("Creating a Ubuntu 14.04.3 Standard DS1 V2 virtual machine w/ a public IP");
            // Create the virtual machine
            var vmCreateParams = new VirtualMachine{
                Location = westus,
                OsProfile = new OSProfile {
                    ComputerName = "sample-vm",
                    AdminUsername = "notAdmin",
                    AdminPassword = "Pa$$w0rd92"
                },
                HardwareProfile = new HardwareProfile{ VmSize = VirtualMachineSizeTypes.StandardDS1V2 },
                StorageProfile = new StorageProfile{
                    ImageReference = new ImageReference {
                        Publisher = "Canonical",
                        Offer = "UbuntuServer",
                        Sku = "14.04.3-LTS",
                        Version = "latest"
                    },
                    OsDisk = new OSDisk {
                        Name = "sample-os-disk",
                        Caching = CachingTypes.None,
                        CreateOption = DiskCreateOptionTypes.FromImage,
                        Vhd = new VirtualHardDisk{
                            Uri = String.Format("https://{0}.blob.core.windows.net/dotnetcontainer/dotnetlinux.vhd", storageAccountName)
                        }
                    }
                },
                NetworkProfile = new NetworkProfile {
                    NetworkInterfaces = new List<NetworkInterfaceReference>{ 
                        new NetworkInterfaceReference {
                            Id = nic.Id,
                            Primary = true
                        }
                    }
                }
            };
            var sshPubLocation = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".ssh", "id_rsa.pub");
            if(File.Exists(sshPubLocation)){
                Write("Found SSH public key in {0}. Disabling password and enabling SSH Authentication.", sshPubLocation);
                var pubKey = File.ReadAllText(sshPubLocation);
                Write("Using public key: {0}", pubKey);
                vmCreateParams.OsProfile.LinuxConfiguration = new LinuxConfiguration {
                    DisablePasswordAuthentication = true,
                    Ssh = new SshConfiguration{ 
                        PublicKeys = new List<SshPublicKey>{ 
                            new SshPublicKey{ 
                                KeyData = pubKey, 
                                Path = "/home/notAdmin/.ssh/authorized_keys" 
                            } 
                        } 
                    }
                };
            }
            var vm = computeClient.VirtualMachines.CreateOrUpdate(resourceGroupName, "sample-dotnet-vm", vmCreateParams);
            Write("Your Linux Virtual Machine is built.");

            Write("Now that we've built a VM, lets turn off the VM.");
            computeClient.VirtualMachines.PowerOff(resourceGroupName, vm.Name);
            Write("Your VM is now off. Lets restart the VM.");

            computeClient.VirtualMachines.Start(resourceGroupName, vm.Name);
            Write("Your VM has been restarted.");
 
            Write("Connect to your new virtual machine via: `ssh {0}@{1}`", vm.OsProfile.AdminUsername, pubIp.DnsSettings.Fqdn);
        }

        private static void Write(string format, params object[] items) 
        {
            Console.WriteLine(String.Format(format, items));
        }
    }
}
