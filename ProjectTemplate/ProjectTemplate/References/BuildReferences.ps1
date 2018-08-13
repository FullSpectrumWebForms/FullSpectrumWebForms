
git clone "https://github.com/FullSpectrumWebForms/FullSpectrumWebForms.git"

Remove-Item -Force FullSpectrumWebForms.dll
Remove-Item -Force FSW.Semantic.dll
Remove-Item -Force FullSpectrumWebForms_ASPC.dll

dotnet build .\FullSpectrumWebForms\src\FullSpectrumWebForms_Semantic\FullSpectrumWebForms_Semantic.csproj
dotnet build .\FullSpectrumWebForms\src\FullSpectrumWebForms\FullSpectrumWebForms.csproj
dotnet build .\FullSpectrumWebForms\src\FullSpectrumWebForms_Semantic\FullSpectrumWebForms_Semantic.csproj
dotnet build .\FullSpectrumWebForms\src\FullSpectrumWebForms_ASPC\FullSpectrumWebForms_ASPC.csproj

Copy-Item FullSpectrumWebForms\src\FullSpectrumWebForms\bin\Debug\netstandard2.0\FullSpectrumWebForms.dll FullSpectrumWebForms.dll
Copy-Item FullSpectrumWebForms\src\FullSpectrumWebForms_Semantic\bin\Debug\netstandard2.0\FSW.Semantic.dll FSW.Semantic.dll
Copy-Item FullSpectrumWebForms\src\FullSpectrumWebForms_ASPC\bin\Debug\netstandard2.0\FullSpectrumWebForms_ASPC.dll FullSpectrumWebForms_ASPC.dll

Remove-Item -Recurse -Force "FullSpectrumWebForms"