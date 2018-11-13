$overlayinterfaceindices = @()
$overlaynetworks = @(Get-HNSNetwork | ? {$_.Type -match "Overlay"})

for ($i = 0; $i -lt $overlaynetworks.Length; $i++){
    # Get a list of ids of interfaces that belong to an overlay network. 
    # This is done by first getting the adapters for an overlay network and then finding all the interfaces used by those adapters.
    if (!$overlaynetworks[$i].NetworkAdapterName) {
        continue
    } 

    $adapter = Get-NetAdapter -Name $overlaynetworks[$i].NetworkAdapterName    
    # Virtual adapter has same MAC address as its corresponding physical adapter
    $vadapter = Get-NetAdapter | ? {$_.MacAddress -match $adapter.MacAddress -and $_.virtual -eq $true}
    $overlayinterfaceindices += $vadapter.ifIndex
}  

Write-Output ("Getting IPv4 interfaces list ...")
$interfaces = @(Get-NetIPInterface -AddressFamily IPv4)

$numbadinterfaces = 0
for ($i = 0; $i -lt $interfaces.Length; $i++) {   
    # Default MTU size for a non-overlay network interface
    $desiredMTU = 1500        
    # If an interface index is in this list, we know that interface is used by an overlay network
    if ($overlayinterfaceindices.contains($interfaces[$i].ifIndex)) {
        # Default MTU size for an overlay network interface
        $desiredMTU = 1450
    }

    if ($interfaces[$i].NlMtu -lt $desiredMTU) {
        $numbadinterfaces++
    }
}

exit $numbadinterfaces
