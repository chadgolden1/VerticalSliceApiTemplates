# Taken from psake https://github.com/psake/psake

<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

function Clean-Directory
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][string]$path
    )
    if (Test-Path $path) { Remove-Item $path -Force -Recurse }
}

<#
.SYNOPSIS
  Publishes artifact in the .NuPkg format, including metadata.
#>
function Create-NuGet-Artifact
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=1)][string]$packageId,
        [Parameter(Mandatory=1)][string]$basePath,
        [Parameter(Mandatory=1)][string]$outFolder,
        [Parameter(Mandatory=1)][string]$packageVersion
    )

    if (Check-Octo-CLI)
    {
        & dotnet octo pack --basePath $basePath --outFolder $outFolder --version $packageVersion --id $packageId --format nupkg
        return
    }

    Write-Warning "Octopus CLI is not installed. No package will be created."
}

<#
.SYNOPSIS
  Publishes artifact as a plain zip file.
#>
function Create-Zip-Artifact
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=1)][string]$basePath,
        [Parameter(Mandatory=1)][string]$outFolder,
        [Parameter(Mandatory=1)][string]$packageVersion
    )

    $zipContents = $($basePath + "/*")
    $zipOutput = $($basePath + ".$packageVersion.zip")

    if ($PSVersionTable.PSVersion.Major -lt 7)
    {
        Write-Warning "You are using PowerShell version $($PSVersionTable.PSVersion.Major). It is recommended to use PowerShell 7 or greater to ensure cross-platform package compatibility."
    }

    Compress-Archive -Path $zipContents -DestinationPath $zipOutput
}

function Check-Octo-CLI
{
    $output = & dotnet tool list octopus.dotnet.cli -g

    if ($lastexitcode -eq 0)
    {
        return $true;
    }
    else
    {
        # Octo CLI not installed, clear last exit code
        $global:LASTEXITCODE = 0
        return $false;
    }
}
