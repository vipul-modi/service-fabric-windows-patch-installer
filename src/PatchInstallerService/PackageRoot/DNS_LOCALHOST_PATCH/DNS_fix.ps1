Write-Output ("Getting IPv4 interfaces list ...")
$interfaces = @(Get-NetIPInterface -AddressFamily IPv4)

for ($i = 0; $i -lt $interfaces.Length; $i++) {
    $ipAddress = (Get-NetIPAddress -InterfaceIndex $interfaces[$i].InterfaceIndex -AddressFamily Ipv4 | Select-Object -Property IPAddress).IPAddress
    $dnsServers = @(Get-DnsClientServerAddress -AddressFamily Ipv4 -InterfaceIndex $interfaces[$i].InterfaceIndex | Select-Object -Property ServerAddresses).ServerAddresses
    
    $localHost = '127.0.0.1'
    # Length should be atleast 2, (Node IP Address + Azure DNS)
    if ( ($dnsServers.Count -gt 1) -and ($dnsServers[0] -eq $ipAddress) -and ($dnsServers[1] -ne $localHost)) {
        # Opening bracket
        $modifiedDnsServersList = "(" + '"' + $dnsServers[0] + '"' + "," + '"' + $localHost + '"' 
        for ($j = 1; $j -lt $dnsServers.Count; $j++) {        
                $modifiedDnsServersList = $modifiedDnsServersList + "," + '"' + $dnsServers[$j] + '"'
        }

        # Closing bracket
        $modifiedDnsServersList = $modifiedDnsServersList + ")"

        Try {
            $command = 'Set-DnsClientServerAddress -InterfaceIndex ' + $interfaces[$i].InterfaceIndex + ' -ServerAddresses ' + $modifiedDnsServersList
            Write-Output ('Executing command ' + $command)
            Invoke-Expression -Command $command
        }
        Catch {
            $ErrorMessage = $_.Exception.Message
            Write-Output ('Exception happened for interface ' + $interfaces[$i].InterfaceAlias + ' with error message ' + $ErrorMessage)
        }
    }   
}