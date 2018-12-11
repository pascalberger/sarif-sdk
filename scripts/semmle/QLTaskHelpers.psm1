Set-StrictMode -Version 2.0

function Get-UniqueOutputPaths
{
    [CmdletBinding()]
    param
    (
        # The name of the project. Used as the base name for the output paths.
        [Parameter(Mandatory = $true)]
        [string] $ProjectName,
        # The parent directory in which the project directory should be created.
        [Parameter(Mandatory = $true)]
        [string] $BaseProjectDirectory,
        # The parent directory in which the results file should be created.
        [string] $BaseResultsDirectory
    )

    $Index = 1
    while ($true)
    {
        $ProjectDirectory = Join-Path $BaseProjectDirectory "${ProjectName}-${Index}"
        [string] $ResultsFile = $null
        if ($BaseResultsDirectory -ne '')
        {
            $ResultsFile = Join-Path $BaseResultsDirectory "${ProjectName}-${Index}.sarif"
        }
        # Make sure that _both_ names are available.
        if (!(Test-Path $ProjectDirectory) -and (($ResultsFile -eq $null) -or !(Test-Path $ResultsFile)))
        {
            return @{
                ProjectDirectory = $ProjectDirectory;
                ResultsFile = $ResultsFile;
            }
        }
        $Index += 1
    }
}

function Get-OutputPaths
{
    [CmdletBinding()]
    param
    (
        # The name of the project. Used as the base name for the output paths.
        [Parameter(Mandatory = $true)]
        [string] $ProjectName,
        # The parent directory in which the project directory should be created.
        [Parameter(Mandatory = $true)]
        [string] $BaseProjectDirectory,
        # The parent directory in which the results file should be created.
        [string] $BaseResultsDirectory,
        # Ensure that the output paths are unique by appending an integer suffix.
        [switch] $MakePathsUnique,
        # Remove any existing files or directories that conflict with the output paths.
        [switch] $Force
    )
    
    if ($MakePathsUnique)
    {
        return Get-UniqueOutputPaths -ProjectName $ProjectName -BaseProjectDirectory $BaseProjectDirectory -BaseResultsDirectory $BaseResultsDirectory -ErrorAction Stop
    }
    else
    {
        $ProjectDirectory = Join-Path $BaseProjectDirectory $ProjectName
        if (Test-Path $ProjectDirectory)
        {
            if ($Force)
            {
                Remove-Item -Force -Recurse $ProjectDirectory -ErrorAction Stop
            }
            else
            {
                throw "Project directory '${ProjectDirectory}' already exists."
            }
        }

        [string] $ResultsFile = $null
        if ($BaseResultsDirectory -ne '')
        {
            $ResultsFile = Join-Path $BaseResultsDirectory "${ProjectName}.sarif"
            if (Test-Path $ResultsFile)
            {
                if ($Force)
                {
                    Remove-Item -Force -Recurse $ResultsFile -ErrorAction Stop
                }
                else
                {
                    throw "Results file '${ResultsFile}' already exists."
                }
            }
        }
        return @{
            ProjectDirectory = $ProjectDirectory;
            ResultsFile = $ResultsFile;
        }
    }
}

<#
.SYNOPSIS
Finds the existing antivirus exclusion path that covers the specified directory, or $null if none is found.
#>
function Get-AntiVirusExclusion
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $MpPreference = Get-MpPreference -ErrorAction Stop
    $TestPath = $Path
    while ($TestPath -ne '')
    {
        foreach ($ExclusionPath in $MpPreference.ExclusionPath)
        {
            if ($ExclusionPath -eq $TestPath)
            {
                Write-Host "Path '${Path}' is covered by exclusion for path '${ExclusionPath}."
                return $ExclusionPath
            }
        }

        $TestPath = Split-Path $TestPath -Parent
    }

    return $null
}

<#
.SYNOPSIS
Ensures that real-time antivirus scanning is disabled for the project directory, optionally adding an exclusion if necessary.
#>
function Test-AntiVirusExclusion
{
    [CmdletBinding()]
    param
    (
        # The path to check for real-time scanning.
        [Parameter(Mandatory = $true)]
        [string] $Path,
        # Allow build even if real-time antivirus scanning is enabled for the project directory (not recommended).
        [switch] $AllowAntivirusScanning,
        # Add an antivirus exclusion for this directory, if necessary
        [string] $AddAntivirusExclusion
    )

    $MpPreference = Get-MpPreference -ErrorAction Stop
    if ($MpPreference.DisableRealtimeMonitoring) {
        Write-Host 'Realtime Monitoring is already disabled.'
        return
    }

    if ($AddAntivirusExclusion -ne '')
    {
        $ExclusionPath = Get-AntiVirusExclusion -Path $AddAntivirusExclusion -ErrorAction Stop
        if ($ExclusionPath -eq $null)
        {
            Write-Host "Adding antivirus exclusion for path '${AddAntivirusExclusion}'..."
            Add-MpPreference -ExclusionPath $AddAntivirusExclusion -ErrorAction Stop
        }
    }

    $ExclusionPath = Get-AntiVirusExclusion -Path $Path -ErrorAction Stop
    if ($ExclusionPath -eq $null)
    {
        $Message = "Realtime antivirus scanning is enabled for path '${Path}'. This is likely to cause spurious build errors."
        if ($AllowAntivirusScanning)
        {
            Write-Warning $Message
        }
        else 
        {
            throw $Message
        }
    }
}

<#
.SYNOPSIS
Builds a QL snapshot of a project, and optionally runs a suite of QL queries against it.
.DESCRIPTION
Creates a QL project file, then runs the specified build commands to build the
snapshot, then runs the query suite.
#>
function Invoke-QLEndToEndAnalysis
{
    [CmdletBinding()]
    param
    (
        # Name of the project to create
        [Parameter(Mandatory = $true)]
        [string] $ProjectName,
        # Language for which analysis is to be run.
        [Parameter(Mandatory = $true)]
        [ValidateSet('cpp', 'csharp', 'javascript', 'java', 'python')]
        [string] $Language,
        # The default timeout for the QL project, in seconds.
        [int] $Timeout,
        # The default amount of RAM to use for analysis, in MB.
        [int] $RAM,
        # The build commands to execute when building the snapshot. Each entry is an object with two
        # fields:
        # [string] $Command (Required) - The command to run.
        # [string] $WorkingDirectory (Optional) - The working directory for the command.
        [Parameter(Mandatory = $true)]
        [object[]] $BuildCommands,
        # The root of the source code for the project.
        [Parameter(Mandatory = $true)]
        [string] $SourceCodeDirectory,
        # The path to the suite file containing the list of queries to run. If not specified, no
        # analysis will be performed.
        [string] $Suite,
        # The directory in which to store the SARIF results files.
        [string] $ResultsDirectory,
        # The directory in which to store the QL projects.
        [Parameter(Mandatory = $true)]
        [string] $ProjectDirectory,
        # The name of the snapshot to create. Defaults to 's'.
        [string] $SnapshotName = 's',
        # The directory in which the Semmle QL tools are installed. If not specified,
        # the tools are found at the path specified by the ODASA_HOME environment
        # variable, if present. Otherwise, the tools are found by looking for
        # `odasa.exe` on the path.
        [string] $QLInstallationPath,
        # Ensure that the project directory and results file paths are unique by appending a number to
        # the path names.
        [switch] $MakeOutputPathsUnique,
        # Force overwrite of any existing output paths.
        [switch] $Force,
        # Allow build even if real-time antivirus scanning is enabled for the project directory (not recommended).
        [switch] $AllowAntivirusScanning,
        # Add an antivirus exclusion for this directory, if necessary
        [string] $AddAntivirusExclusion
    )

    Set-StrictMode -Version 2.0
    Import-Module (Join-Path $PSScriptRoot 'PowerQL.psm1') -ErrorAction Stop

    $ProjectNameRoot = "${ProjectName}-${Language}"
    $OutputPaths = Get-OutputPaths -ProjectName $ProjectNameRoot -BaseProjectDirectory $ProjectDirectory -BaseResultsDirectory $ResultsDirectory -MakePathsUnique:$MakeOutputPathsUnique -Force:$Force -ErrorAction Stop

    Test-AntiVirusExclusion -Path $OutputPaths.ProjectDirectory -AllowAntivirusScanning:$AllowAntivirusScanning -AddAntivirusExclusion $AddAntivirusExclusion -ErrorAction Stop

    $QLInstallationPath = Get-QLInstallationPath -QLInstallationPath $QLInstallationPath
    Write-Host "Analyzing language '${Language}'..."
    $BuildCommandObjects = @()
    if ($Language -eq 'csharp')
    {
        $BuildCommandObjects += @(
            @{
                Command = "${QLInstallationPath}/tools/java/bin/java -jar ${QLInstallationPath}/tools/extractor-asp.jar ."
            },
            @{
                Command = 'odasa index --xml --extensions config'
            }
        )
    }
    foreach ($BuildCommand in $BuildCommands)
    {
        $BuildCommandObject = @{
            index = 'true';
            Command = $BuildCommand.Command;
        }
        if ($BuildCommand['WorkingDirectory'] -ne $null)
        {
            $BuildCommandObject.dir = $BuildCommand['WorkingDirectory']
        }
        $BuildCommandObjects += $BuildCommandObject
    }
    if (($Language -eq 'csharp') -or ($Language -eq 'cpp'))
    {
        $BuildCommandObjects += @{
            Command = 'odasa duplicateCode --ram 2048 --minimum-tokens 100'
        }
    }

    Write-Host "Creating QL project at '$($OutputPaths.ProjectDirectory)'..."
    [void](New-QLProject -ProjectPath $OutputPaths.ProjectDirectory -Name $ProjectNameRoot -Language $Language `
        -Timeout $Timeout -RAM $RAM -BuildCommands $BuildCommandObjects -SourceLocation $SourceCodeDirectory `
        -Force -QLInstallationPath $QLInstallationPath -ErrorAction Stop)
    Write-Host "Created QL project at '$($OutputPaths.ProjectDirectory)'."

    Write-Host "Create QL snapshot '${SnapshotName}'..."
    $Snapshot = New-QLSnapshot -Project $OutputPaths.ProjectDirectory -Name $SnapshotName -QLInstallationPath $QLInstallationPath -Force -ErrorAction Stop
    Write-Host "Created QL snapshot '${SnapshotName}'."

    Write-Host "Building QL snapshot at '${Snapshot}'..."
    Invoke-QLBuild -Project $OutputPaths.ProjectDirectory -Snapshot $Snapshot -QLInstallationPath $QLInstallationPath -Force -ErrorAction Stop
    Write-Host "Built QL snapshot at '${Snapshot}'."

    if ($Suite -ne '')
    {
        $SuitePath = [System.IO.Path]::Combine($QLInstallationPath, $Suite)
        Write-Host "Running QL analysis on snapshot '${Snapshot}' with suite '${SuitePath}'..."
        Invoke-QLAnalysis -Project $OutputPaths.ProjectDirectory -Snapshot $Snapshot -Output $OutputPaths.ResultsFile -Suite $SuitePath -Force -QLInstallationPath $QLInstallationPath -ErrorAction Stop 
        Write-Host "Ran QL analysis on snapshot '${Snapshot}' with suite '${SuitePath}'..."
    }

    Write-Host "Analyzed language '${Language}'."
}

Import-Module (Join-Path $PSScriptRoot 'PowerQL.psm1') -ErrorAction Stop
