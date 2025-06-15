. ./build-helpers.ps1

$artifacts = "./artifacts"
$version = "0.0.0"

Clean-Directory $artifacts

$PSVersionTable

exec { & dotnet --info }

exec { & dotnet clean -c Release -v:m }

exec { & dotnet restore }

exec { & dotnet format --verify-no-changes --no-restore }

exec { & dotnet build -c Release --no-restore }

exec { & dotnet test -c Release --no-build }

exec {
    $outputDirectory = $($artifacts + "/TodoApi")

    & dotnet publish ./src/TodoApi/TodoApi.csproj `
        -c Release `
        --no-build `
        --output $($artifacts + "/TodoApi") `
        -p:Version=$version
}
