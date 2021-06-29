[![Build status](https://ci.appveyor.com/api/projects/status/2q0mv1isiddp18ay?svg=true)](https://ci.appveyor.com/project/MultifactorLab/multifactor-adfs-adapter) [![License](https://img.shields.io/badge/license-view-orange)](LICENSE.md)

## MultiFactor.ADFS.Adapter

_Also available in other languages: [Русский](README.ru.md)_

MultiFactor.ADFS.Adapter allows to protect access to corporate Active Directory Federation Services (ADFS) applications with <a href="https://multifactor.pro/" target="_blank">MultiFactor</a> 2FA hybrid solution.

The component is developed and supported by MultiFactor, distributed for free with the source code.

* <a href="https://github.com/MultifactorLab/MultiFactor.ADFS.Adapter" target="_blank">Source code</a>
* <a href="https://github.com/MultifactorLab/MultiFactor.ADFS.Adapter/releases" target="_blank">Build</a>

See documentation at https://multifactor.pro/docs/adfs-2fa/ for additional guidance on integrating 2FA into your ADFS applications.

## Table of Contents

- [Operation Principle](#operation-principle)
- [Available authentication methods](#available-authentication-methods)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
  - [MultiFactor Configuration](#multifactor-configuration)
  - [ADFS Configuration](#adfs-configuration)
- [Additional information](#additional-information)
- [License](#license)

## Operation Principle

1. User opens a corporate application;
2. ADFS asks for the first authentication factor: login and password,  then checks credentials in ActiveDirectory and, if they are correct, proceeds to the second stage of authentication;
3. In the second stage, the Multifactor prompt opens, inviting the user to confirm access;
4. The user confirms access with the second factor and proceeds to the application.

## Available authentication methods

* ``MultiFactor Mobile Application``
* ``Telegram``
* ``SMS``
* ``Biometrics``
* ``Hardware OTP tokens``
* ``Software OTP tokens (e.g. Google Authenticator)``

## Prerequisites

1. The component must have access to the ```api.multifactor.ru``` on TCP port 443 (TLS) directly or via HTTP proxy;
2. The server must be set to the correct time.

## Configuration

### MultiFactor Configuration

1. Open <a href="https://admin.multifactor.ru/" target="_blank">Multifactor management console</a>, then create a new **Web-site** with default settings under **Resources** section. Keep ```API Key``` and ```API Secret``` parameters displayed upon resource creation: these are needed to complete the setup.

### ADFS Configuration

1. Download and unzip the <a href="https://github.com/MultifactorLab/MultiFactor.ADFS.Adapter/releases" target="_blank">archive</a> to the server with ADFS;
2. In ```MultiFactor.ADFS.Adapter.dll.config``` configuration file fill in ```API Key``` and ```API Secret``` from the MultiFactor personal account;
3. Run the PowerShell script ```install.ps1``` with administrator privileges;
4. Navigate to ADFS management console and under **Authentication methods** -> **Multi-factor Authentication Methods** enable the **MultiFactor** method;
5. Under **Relying Party Trusts**, edit the Access Policy for the applications where you want to enable 2FA.

## Additional information

* To work in a cluster configuration, the component must be installed on all servers in the cluster;
* Component log can be viewed on the ADFS server Windows Log in Application Log section (source: MultiFactor) and ADFS section.

## License

Please note, the [license](LICENSE.md) does not entitle you to modify the source code of the Component or create derivative products based on it. The source code is provided as-is for evaluation purposes.