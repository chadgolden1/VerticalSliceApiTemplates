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
        --output $outputDirectory `
        -p:Version=$version

    # Create artifact in nupkg format for tools such as Octopus Deploy or other package repositories requiring
    # package metadata.
    #
    # For simpler artifact needs, such as a basic zip file without metadata, try:
    #   Create-Zip-Artifact -basePath:$outputDirectory -outFolder:$artifacts -packageVersion:$version
    Create-NuGet-Artifact -basePath:$outputDirectory -outFolder:$artifacts -packageVersion:$version -packageId:"TodoApi"
}
