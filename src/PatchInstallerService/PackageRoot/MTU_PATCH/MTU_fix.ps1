Write-Output ("Getting IPv4 interfaces list ...")
$interfaces = @(Get-NetIPInterface -AddressFamily IPv4)

for ($i = 0; $i -lt $interfaces.Length; $i++) {
    if ( $interfaces[$i].NlMtu -lt 1500 ) {
        Write-Output ('Found IPv4 interface named ' + $interfaces[$i].InterfaceAlias + ' with MTU size less than 1500.')

        Try {
            $command = 'Set-NetIPInterface -InterfaceIndex ' + $interfaces[$i].InterfaceIndex + ' -NlMtuBytes 1500'
            Write-Output ('Executing command ' + $command)
            Invoke-Expression -Command $command
        }
        Catch {
            $ErrorMessage = $_.Exception.Message
            Write-Output ('Exception happened for interface ' + $interfaces[$i].InterfaceAlias + ' with error message ' + $ErrorMessage)
        }
    }
}