dotnet build -c Release

@echo Initializing SOFDN document base...
dotnet bin\Release\netcoreapp2.0\deepth.dll init sofdn data\sofdn-articles.xml 5000

@echo Training SOFDN document base
dotnet bin\Release\netcoreapp2.0\deepth.dll train sofdn data\sofdn-questions-train.xml 5000

@echo Testing SOFDN document base
dotnet bin\Release\netcoreapp2.0\deepth.dll test sofdn data\sofdn-questions-test.xml 2000

