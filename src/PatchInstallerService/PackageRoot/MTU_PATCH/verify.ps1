$interfaces = @(Get-NetIPInterface -AddressFamily IPv4 | ? {$_.NlMtu -lt 1500}).Count
exit $interfaces