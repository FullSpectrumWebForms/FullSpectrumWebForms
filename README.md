<img src="LogoFSW.png" width="180" height="180">

![.NET Core](https://github.com/FullSpectrumWebForms/FullSpectrumWebForms/workflows/.NET%20Core/badge.svg)

# FullSpectrumWebForms
Full Spectrum Web Forms is a library that simplify development of Web and desktop applications with Asp.Net Core. It enables you to manage all your client side interactions from the server. It ships with a full set of controls you can use to interact with the UI without writing any "client side" JavaScript code

To learn more about FSW and how it differs from more common web servers or web framework, head to the [Getting Started](https://github.com/FullSpectrumWebForms/FullSpectrumWebForms/wiki/Getting-Started) section of the wiki.  
For the full documentation, head to the [the wiki](https://github.com/FullSpectrumWebForms/FullSpectrumWebForms/wiki) !

# Installing / Getting started

## Using the template project

To easily start a new project with FSW, use the pre-configured [Template project](https://github.com/FullSpectrumWebForms/FullSpectrumWebForms/wiki/Starting-a-project-from-template) !


## Building the source on Windows:
### Using Visual Studio

1. Install Visual Studio from [here](https://visualstudio.microsoft.com/downloads/). Install the ".NET Core cross-plateform developement" tool in the Installer
2. Make sure to install the TypeScript SDK 2.8, you can find it in the "Individual Components" section of the installer
3. To get the source code from git, take a look [Here](https://docs.microsoft.com/en-us/vsts/repos/git/clone?view=vsts&tabs=visual-studio#clone-from-another-git-provider) and use this url: https://github.com/FullSpectrumWebForms/FullSpectrumWebForms.git
4. Open FullSpectrumWebForms.sln with Visual Studio
5. Rebuild the solution

### Using VSCode

1. Install VSCode from [Here](https://code.visualstudio.com/download)
2. If you want to use the .net core instead of .net framework, download and install the lastest .net Core SDK from [Here](https://www.microsoft.com/net/download)
3. We need npm. It is contained in the NodeJs installation: [Here](https://nodejs.org/en/download/current/)
4. Install Typescript
```shell
npm install -g typescript@2.8
```
5. Download git for windows from [Here](https://gitforwindows.org/)
6. Get the source code from git.
```shell
git clone https://github.com/FullSpectrumWebForms/FullSpectrumWebForms.git
```
7. Open the project folder in VSCode
8. You will be prompted to install OmniSharp plugin.
9. Hit F5 To launch the test project!

## Building the source on Linux:

1. Install the last .net Core for your distribution [Here](https://www.microsoft.com/net/download/linux-package-manager/rhel/sdk-current)
2. Install npm and git
```shell
sudo apt-get install nodejs npm git
sudo snap install powershell
```
3. Install Typescript
```shell
sudo npm install -g typescript@2.8
```
3. Install VsCode from [here](https://code.visualstudio.com/)
4. Get the source code in your desired folder
```shell
git clone https://github.com/FullSpectrumWebForms/FullSpectrumWebForms.git
```
5. You will be prompted to install OmniSharp plugin.
6. Hit F5 To launch the test project!
