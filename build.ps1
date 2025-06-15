. ./build-helpers.ps1

$artifacts = "./artifacts"
$version = "0.0.0"
$publishProject = "./src/TodoApi/TodoApi.csproj"
$outputFolder = $($artifacts + "/TodoApi")

Clean-Directory $artifacts

$PSVersionTable

exec { & dotnet --info }

exec { & dotnet clean -c Release -v:m }

exec { & dotnet restore }

exec { & dotnet format --verify-no-changes --no-restore }

exec { & dotnet build -c Release --no-restore }

exec { & dotnet test -c Release --no-build }

exec { & dotnet publish $publishProject -c Release --no-build --output $outputFolder -p:Version=$version }
