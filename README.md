<img src="LogoFSW.png" width="180" height="180">

# FullSpectrumWebForms
Full Spectrum Web Forms is a library that simplify development of Web and desktop applications with Asp.Net Core. It enables you to manage all your client side interactions from the server. It ships with a full set of controls you can use to interact with the UI without writing any "client side" JavaScript code

For the full documentation, head to the [the wiki](https://github.com/FullSpectrumWebForms/FullSpectrumWebForms/wiki) !

# Installing / Getting started

## For Windows:
### Using Visual Studio

1. Install Visual Studio [Here](https://visualstudio.microsoft.com/downloads/)
2. Make sure to install the TypeScript SDK 2.5, you can find it in the "Individual Components" section of the installer
3. If you want to use the .net core instead of .net framework, download and install the lastest .net Core SDK [Here]
4. To get the source code from git, take a look [Here](https://docs.microsoft.com/en-us/vsts/repos/git/clone?view=vsts&tabs=visual-studio#clone-from-another-git-provider) and use this url: https://github.com/FullSpectrumWebForms/FullSpectrumWebForms.git
5. Open FullSpectrumWebForms.sln with Visual Studio
6. Hit f5 to launch the test project!

### Using VSCode

1. Install VSCode [Here](https://code.visualstudio.com/download)
2. If you want to use the .net core instead of .net framework, download and install the lastest .net Core SDK [Here](https://www.microsoft.com/net/download)
3. We need npm. It is contained in the NodeJs installation: [Here](https://nodejs.org/en/download/current/)
4. Install Typescript
```shell
npm install -g typescript@2.5
```
5. Get the source code from git.
```shell
git clone https://github.com/FullSpectrumWebForms/FullSpectrumWebForms.git
```
6. Open the project folder in VSCode
7. You will be prompted to install OmniSharp plugin.
8. Hit F5 To launch the test project!

For Linux:

1. Install the last .net Core for your distribution [Here](https://www.microsoft.com/net/download/linux-package-manager/rhel/sdk-current)
2. Install npm and git
```shell
sudo apt-get install nodejs npm git
```
3. Install Typescript
```shell
sudo npm install -g typescript@2.5
```
3. Install VsCode from [here](https://code.visualstudio.com/)
4. Get the source code in your desired folder
```shell
git clone https://github.com/FullSpectrumWebForms/FullSpectrumWebForms.git
```
5. After opening the source in VsCode you will be prompt to install OmniSharp plugin, from here you must be able to hit F5 to      start the server
