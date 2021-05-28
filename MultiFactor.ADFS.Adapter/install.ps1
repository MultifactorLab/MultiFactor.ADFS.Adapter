$path = Split-Path $MyInvocation.MyCommand.Path -Parent

Set-Location $path

[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
$publish = New-Object System.EnterpriseServices.Internal.Publish

$publish.GacInstall($path + '\MultiFactor.ADFS.Adapter.dll')

$fn = ([System.Reflection.Assembly]::LoadFile($path + '\MultiFactor.ADFS.Adapter.dll')).FullName

$typeName = "MultiFactor.ADFS.Adapter.AuthenticationAdapter, " + $fn.ToString() + ", processorArchitecture=MSIL"
Register-AdfsAuthenticationProvider -TypeName $typeName -Name "MultiFactor" -ConfigurationFilePath ($path + '\MultiFactor.ADFS.Adapter.dll.config')

net stop adfssrv
net start adfssrv

New-EventLog -LogName Application -Source MultiFactor