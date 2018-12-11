<#
.SYNOPSIS
Builds a QL snapshot of a project, and runs a suite of QL queries against it.
.DESCRIPTION
Creates a QL project file, then runs the specified build commands to build the
snapshot, then runs the query suite.
#>
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
    # The path to the suite file containing the list of queries to run.
    [Parameter(Mandatory = $true)]
    [string] $Suite,
    # The directory in which to store the SARIF results files.
    [Parameter(Mandatory = $true)]
    [string] $ResultsDirectory,
    # The directory in which to store the QL projects.
    [Parameter(Mandatory = $true)]
    [string] $ProjectDirectory,
    # Ensure that the project directory and results file paths are unique by appending a number to
    # the path names.
    [switch] $MakeOutputPathsUnique,
    # Force overwrite of any existing output paths.
    [switch] $Force,
    # The name of the snapshot to create. Defaults to 's'.
    [string] $SnapshotName = 's',
    # The directory in which the Semmle QL tools are installed. If not specified,
    # the tools are found at the path specified by the ODASA_HOME environment
    # variable, if present. Otherwise, the tools are found by looking for
    # `odasa.exe` on the path.
    [string] $QLInstallationPath,
    # Allow build even if real-time antivirus scanning is enabled for the project directory (not recommended).
    [switch] $AllowAntivirusScanning,
    # Add an antivirus exclusion for this directory, if necessary
    [string] $AddAntivirusExclusion
)

Set-StrictMode -Version 2.0
Import-Module (Join-Path $PSScriptRoot 'QLTaskHelpers.psm1') -ErrorAction Stop

$Params = $PSCmdlet.MyInvocation.BoundParameters
Invoke-QLEndToEndAnalysis @Params
