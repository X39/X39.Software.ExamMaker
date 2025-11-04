# Generating the API
To generate the API, you have to install the dotnet tool `kiota`, using:
```bash
dotnet tool update --global Microsoft.OpenApi.Kiota  
```

Afterwards, you may use the following command, at project root, to generate the API:
```bash
kiota generate -d http://localhost:8080/openapi/v1.json -l CSharp -n X39.Software.ExamMaker -o Api
```
