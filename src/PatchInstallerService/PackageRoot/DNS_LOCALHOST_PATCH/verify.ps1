Write-Output ("Getting IPv4 interfaces list ...")
$interfaces = @(Get-NetIPInterface -AddressFamily IPv4)

$numbadinterfaces = 0
for ($i = 0; $i -lt $interfaces.Length; $i++) {
    $ipAddress = (Get-NetIPAddress -InterfaceIndex $interfaces[$i].InterfaceIndex -AddressFamily Ipv4 | Select-Object -Property IPAddress).IPAddress
    $dnsServers = @(Get-DnsClientServerAddress -AddressFamily Ipv4 -InterfaceIndex $interfaces[$i].InterfaceIndex | Select-Object -Property ServerAddresses).ServerAddresses
    
    $localHost = '127.0.0.1'
    # Length should be atleast 2, (Node IP Address + Azure DNS)
    if ( ($dnsServers.Count -gt 1) -and ($dnsServers[0] -eq $ipAddress) -and ($dnsServers[1] -ne $localHost)) {
       $numbadinterfaces++
    }   
}

exit $numbadinterfaces