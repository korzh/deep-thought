dotnet build -c Release

@echo Initializing SFODN document base...
dotnet bin\Release\netcoreapp2.0\deepth.dll init sfodn data\sfodn-articles.xml 5000

@echo Training SFODN document base
dotnet bin\Release\netcoreapp2.0\deepth.dll train sfodn data\sfodn-questions-train.xml 5000

@echo Testing SFODN document base
dotnet bin\Release\netcoreapp2.0\deepth.dll test sfodn data\sfodn-questions-test.xml 2000

