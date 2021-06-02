# MultiFactor ADFS Adapter install script
# Run as Administrator on every ADFS server

$path = Split-Path $MyInvocation.MyCommand.Path -Parent

Set-Location $path

# 1. Add adapter assembly to global cache

[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
$publish = New-Object System.EnterpriseServices.Internal.Publish

$publish.GacInstall($path + '\MultiFactor.ADFS.Adapter.dll')

# 2. Register ADFS authentication provider on master

$fn = ([System.Reflection.Assembly]::LoadFile($path + '\MultiFactor.ADFS.Adapter.dll')).FullName

$typeName = "MultiFactor.ADFS.Adapter.AuthenticationAdapter, " + $fn.ToString() + ", processorArchitecture=MSIL"

$stsrole = Get-ADFSSyncProperties | Select-Object -ExpandProperty Role
if ($stsrole -eq "PrimaryComputer") {
	Register-AdfsAuthenticationProvider -TypeName $typeName -Name "MultiFactor" -ConfigurationFilePath ($path + '\MultiFactor.ADFS.Adapter.dll.config')
}

# 3. Restart ADFS service

net stop adfssrv
net start adfssrv

# 4. Create Application EventLog source MultiFactor if not exists

if (!([System.Diagnostics.EventLog]::SourceExists("MultiFactor"))){
	New-Eventlog -LogName "Application" -Source "MultiFactor"
}