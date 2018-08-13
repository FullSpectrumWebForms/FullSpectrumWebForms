CMD /C "dotnet build ..\..\..\tests\TestApplication\TestApplication.csproj"
Copy-Item ..\..\..\tests\TestApplication\bin\Debug\FullSpectrumWebForms.dll FullSpectrumWebForms.dll
Copy-Item ..\..\..\tests\TestApplication\bin\Debug\FSW.Semantic.dll FSW.Semantic.dll
Copy-Item ..\..\..\tests\TestApplication\bin\Debug\FullSpectrumWebForms_ASPC.dll FullSpectrumWebForms_ASPC.dll