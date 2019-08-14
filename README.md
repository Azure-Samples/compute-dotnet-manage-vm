---
page_type: sample
languages:
- java
products:
- azure
services: Compute
platforms: dotnet
author: yaohaizh
---

# This sample demonstrates how to manage your Azure virtual machines using a .NET client #

          Azure Compute sample for managing virtual machines -
           - Create a virtual machine with managed OS Disk
           - Start a virtual machine
           - Stop a virtual machine
           - Restart a virtual machine
           - Update a virtual machine
             - Tag a virtual machine (there are many possible variations here)
             - Attach data disks
             - Detach data disks
           - List virtual machines
           - Delete a virtual machine.


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/compute-dotnet-manage-vm.git

    cd compute-dotnet-manage-vm

    dotnet build

    bin\Debug\net452\ManageVirtualMachine.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.