---
services: Compute
platforms: .Net
author: selvasingh
---

#Getting Started with Compute - Manage Virtual Machine - in .Net #

      Azure Compute sample for managing virtual machines -
       - Create a virtual machine
       - Start a virtual machine
       - Stop a virtual machine
       - Restart a virtual machine
       - Update a virtual machine
         - Expand the OS drive
         - Tag a virtual machine (there are many possible variations here)
         - Attach data disks
         - Detach data disks
       - List virtual machines
       - Delete a virtual machine.


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-sdk-for-net/blob/Fluent/AUTH.md).

    git clone https://github.com/Azure-Samples/compute-dotnet-manage-vm.git

    cd compute-dotnet-manage-vm

    dotnet restore

    dotnet run

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.