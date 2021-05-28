$path = Split-Path $MyInvocation.MyCommand.Path -Parent
Set-Location $path

Unregister-AdfsAuthenticationProvider -Name "MultiFactor"

[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
$publish = New-Object System.EnterpriseServices.Internal.Publish

$publish.GacRemove($path + "\MultiFactor.ADFS.Adapter.dll")