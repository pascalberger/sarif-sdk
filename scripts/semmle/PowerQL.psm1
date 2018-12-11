Set-StrictMode -Version 2.0

<#
.SYNOPSIS
Invokes the specified command.
.OUTPUT
All output from the command itself is redirected to the Information stream.
#>
function Invoke-LoggedCommand
{
    [CmdletBinding()]
    param
    (
        # The command to invoke
        [Parameter(Mandatory = $true)]
        [string] $Command,
        # The arguments to pass to the command
        [string[]] $CommandArgs
    )
    Write-Information ($Command + ' ' + $CommandArgs)
    &$Command @CommandArgs *>&1 | ForEach-Object { Write-Information $_ }
    $Error = $LASTEXITCODE
    if ($Error -ne 0)
    {
        throw "Command '${Command}' failed with exit code '${Error}'."
    }
}

function Invoke-Odasa
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Verb,
        [string[]] $CommandArgs,
        [string] $QLInstallationPath
    )

    $Command = Get-QLToolPath $QLInstallationPath
    $OdasaCommandArgs = @($Verb)
    $OdasaCommandArgs += $CommandArgs
    Invoke-LoggedCommand -Command $Command -CommandArgs $OdasaCommandArgs
}

<#
.SYNOPSIS
    Runs a set of queries on a QL snapshot and produces a results file.
.DESCRIPTION
    Runs 'odasa analyzeSnapshot'
#>
function Invoke-QLAnalysis
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Snapshot,
        [ValidateSet('csv', 'sarifv1', 'sarifv2')]
        [string] $Format = 'sarifv2',
        [Parameter(Mandatory = $true)]
        [string] $Output,
        [string] $Suite,
        [string] $Project,
        [switch] $Force,
        [string] $QLInstallationPath,
        [switch] $UngroupResults,
        [string[]] $Queries,
        [ValidateRange(0, 6)]
        [int] $LogLevel = 3
    )

    $OutputDirectory = Split-Path $Output -Parent
    if (!(Test-Path $OutputDirectory))
    {
        [void](New-Item $OutputDirectory -ItemType Directory -ErrorAction Stop)
    }

    $OdasaArgs = @(
        '--format', $Format,
        '--output-file', $Output,
        '--suite', $Suite,
        '--verbosity', $LogLevel
    )
    if ($Queries -ne $null) 
    {
        $OdasaArgs += '--queries'
        $OdasaArgs += $Queries
    }
    if ($Project -ne '')
    {
        $OdasaArgs += @('--project', $Project)
    }
    if ($UngroupResults)
    {
        $OdasaArgs += '--ungroup-results'
    }
    if ($Force)
    {
        $OdasaArgs += '--rerun'
    }
    $OdasaArgs += $Snapshot

    Invoke-Odasa -QLInstallationPath $QLInstallationPath -Verb 'analyzeSnapshot' -CommandArgs $OdasaArgs
}

function Invoke-QLBuild
{
    [CmdletBinding()]
    param
    (
        [string] $Project,
        [Parameter(Mandatory = $true)]
        [string] $Snapshot,
        [switch] $Force,
        [ValidateRange(0, 6)]
        [int] $LogLevel = 3,
        [string] $QLInstallationPath,
        [switch] $SkipBuild,
        [switch] $BuildOnly
    )

    $OdasaArgs = @(
        '--verbosity', $LogLevel
    )
    if ($Force)
    {
        $OdasaArgs += '--overwrite'
    }
    if ($SkipBuild)
    {
        $OdasaArgs += '--skip-build'
    }
    if ($BuildOnly)
    {
        $OdasaArgs += '--build-only'
    }
    if ($Project -ne '')
    {
        $OdasaArgs += @('--project', $Project)
    }
    $OdasaArgs += $Snapshot

    Invoke-Odasa -QLInstallationPath $QLInstallationPath -Verb 'buildSnapshot' -CommandArgs $OdasaArgs
}

function New-QLSnapshot
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Project,
        [Parameter(Mandatory = $true)]
        [string] $Name,
        [switch] $Force,
        [ValidateRange(0, 6)]
        [int] $LogLevel = 3,
        [object] $Variables,
        [string] $QLInstallationPath
    )

    $OdasaArgs = @(
        '--project', $Project,
        '--name', $Name,
        '--verbosity', $LogLevel,
        '--latest'
    )
    if ($Force)
    {
        $OdasaArgs += '--overwrite'
    }
    if ($Variables -ne $null)
    {
        foreach ($Pair in $Variables)
        {
            $OdasaArgs += @('--variable', $Pair.Key, $Pair.Value)
        }
    }

    Invoke-Odasa -QLInstallationPath $QLInstallationPath -Verb 'addSnapshot' -CommandArgs $OdasaArgs

    $SnapshotPath = Join-Path $Project $Name
    return $SnapshotPath
}

function AddXmlElement
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [object] $Parent,
        [Parameter(Mandatory = $true)]
        [string] $Name,
        [string] $Value,
        [object] $Attributes
    )

    $Element = $Parent.OwnerDocument.CreateElement($Name)
    [void]$Parent.AppendChild($Element)
    if ($Value -ne $null)
    {
        $Element.InnerText = $Value
    }
    if ($Attributes -ne $null)
    {
        foreach ($Attribute in $Attributes.GetEnumerator())
        {
            $Element.SetAttribute($Attribute.Key, $Attribute.Value)
        }
    }

    return $Element
}

function AddCommandElement
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [object] $Parent,
        [Parameter(Mandatory = $true)]
        [string] $Name,
        [Parameter(Mandatory = $true)]
        [object] $Command
    )

    $Element = AddXmlElement -Parent $Parent -Name $Name
    if ($Command -is [string])
    {
        $Element.InnerText = $Command
    }
    else
    {
        foreach ($Pair in $Command.GetEnumerator())
        {
            if ($Pair.Key -eq 'command')
            {
                $Element.InnerText = $Pair.Value
            }
            else
            {
                $Element.SetAttribute($Pair.Key, $Pair.Value)
            }
        }
    }
}

function New-QLProject
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ProjectPath,
        [string] $Name,
        [Parameter(Mandatory = $true)]
        [ValidateSet('cpp', 'csharp', 'java', 'javascript', 'python')]
        [string] $Language,
        [int] $Timeout,
        [int] $RAM,
        [Parameter(ParameterSetName = 'AutoUpdateFile')]
        [string] $AutoUpdate,
        [Parameter(ParameterSetName = 'AutoUpdateSwitches')]
        [object[]] $BuildCommands,
        [Parameter(ParameterSetName = 'AutoUpdateSwitches')]
        [object[]] $CheckoutCommands,
        [Parameter(ParameterSetName = 'AutoUpdateSwitches')]
        [string] $SourceLocation,
        [string] $QLInstallationPath,
        [switch] $Force
    )

    $OdasaArgs = @('--language', $Language)

    if ($Name -ne '') 
    {
        $OdasaArgs += @('--name', $Name)
    }
    if ($TimeOut -ne '')
    {
        $OdasaArgs += @('--timeout', $Timeout)
    }
    if ($RAM -ne '') 
    {
        $OdasaArgs += @('--ram', $RAM)
    }
    switch ($PSCmdlet.ParameterSetName)
    {
        'AutoUpdateFile' 
            {
                if ($AutoUpdate -ne '')
                {
                    $OdasaArgs += @('--autoupdate', $AutoUpdate)
                }
            }

        'AutoUpdateSwitches' 
            {
                $AutoUpdateXml = [xml]'<autoupdate/>'
                if ($CheckoutCommands -ne $null)
                {
                    foreach ($Command in $CheckoutCommands)
                    {
                        AddCommandElement -Parent $AutoUpdateXml.DocumentElement -Name 'checkout' -Command $Command
                    }
                }
                if ($BuildCommands -ne $null)
                {
                    foreach ($Command in $BuildCommands)
                    {
                        AddCommandElement -Parent $AutoUpdateXml.DocumentElement -Name 'build' -Command $Command
                    }
                }
                if ($SourceLocation -ne '')
                {
                    [void](AddXmlElement -Parent $AutoUpdateXml.DocumentElement -Name 'source-location' -Value $SourceLocation)
                }

                $AutoUpdateFile = New-TemporaryFile -ErrorAction Stop
                $AutoUpdateXml.Save($AutoUpdateFile)
                $OdasaArgs += @('--autoupdate', $AutoUpdateFile)
            }
        }
    if ($Force)
    {
        $OdasaArgs += '--force'
    }
    $OdasaArgs += $ProjectPath

    Invoke-Odasa -QLInstallationPath $QLInstallationPath -Verb 'createProject' -CommandArgs $OdasaArgs

    Get-Item -Path (Join-Path $ProjectPath 'project') -ErrorAction Stop
}

function Get-QLToolPath
{
    [CmdletBinding()]
    param
    (
        [string] $QLInstallationPath
    )

    $QLInstallationPath = Get-QLInstallationPath -QLInstallationPath $QLInstallationPath

    # First, look for the version of odasa.exe with telemetry support
    $TelemetryOdasaPath = Join-Path $QLInstallationPath 'telemetry\odasa.exe'
    if (Test-Path $TelemetryOdasaPath)
    {
        return Resolve-Path $TelemetryOdasaPath -ErrorAction Stop
    }

    # Next, look for the non-telemetry version
    $ToolsOdasaPath = Join-Path $QLInstallationPath 'tools\odasa.exe'
    if (Test-Path $ToolsOdasaPath)
    {
        return Resolve-Path $ToolsOdasaPath -ErrorAction Stop
    }
    else
    {
        throw "Could not find '${ToolsOdasaPath}'."
    }
}

function Get-QLInstallationPath
{
    [CmdletBinding()]
    param
    (
        [string] $QLInstallationPath
    )

    if ($QLInstallationPath -ne '')
    {
        # We were told exactly where to look.
        return Resolve-Path $QLInstallationPath -ErrorAction Stop
    }

    if ($env:ODASA_HOME -ne $null)
    {
        # Path to odasa was specified in environment from setup.bat
        return Resolve-Path $env:ODASA_HOME -ErrorAction Stop
    }

    # Look for odasa.exe on the path
    foreach ($Path in ${env:PATH}.Split(';'))
    {
        $PotentialOdasaExePath = Join-Path $Path.Trim() 'odasa.exe'
        if (Test-Path $PotentialOdasaExePath -PathType Leaf)
        {
            return Resolve-Path (Join-Path $PotentialOdasaExePath '..\..') -ErrorAction Stop
        }
    }

    throw "Could not find 'odasa.exe' on path."
}

$SCRIPT:Languages = @('csharp', 'java', 'cpp', 'javascript', 'python')

Export-ModuleMember -Function @(
    'Get-QLInstallationPath',
    'Get-QLToolPath',
    'New-QLSnapshot',
    'New-QLProject',
    'Invoke-QLBuild',
    'Invoke-QLAnalysis'
)
