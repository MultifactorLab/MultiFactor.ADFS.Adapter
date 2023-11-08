# MultiFactor ADFS Adapter uninstall script
# Run as Administrator on every ADFS server

if ($PSISE) {
    $path = Split-Path $psISE.CurrentFile.FullPath -Parent
} else {
    $path = Split-Path $MyInvocation.MyCommand.Path -Parent
}
Set-Location $path

# 1. Unregister ADFS authentication provider on master

$stsrole = Get-ADFSSyncProperties | Select-Object -ExpandProperty Role
if ($stsrole -eq "PrimaryComputer") {
	Unregister-AdfsAuthenticationProvider -Name "MultiFactor"
}

# 2. Remove adapter assembly from global cache

[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
$publish = New-Object System.EnterpriseServices.Internal.Publish

$publish.GacRemove($path + "\MultiFactor.ADFS.Adapter.dll")