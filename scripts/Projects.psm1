<#
.SYNOPSIS
    Provides a list of SARIF SDK projects and frameworks.
.DESCRIPTION
    The Projects module exports variables whose properties specify the
    various kinds of projects in the SARIF SDK, and the frameworks for which
    they are built.
#>

$Frameworks = @{}

# .NET Framework versions for which we build.
$Frameworks.NetFx = @("net461")

# Frameworks for which we build libraries.
$Frameworks.Library = @("netstandard2.0") + $Frameworks.NetFx

# Frameworks for which we build applications.
$Frameworks.Application = @("netcoreapp2.0", "netcoreapp2.1") + $Frameworks.NetFx

$Frameworks.All = ($Frameworks.Library + $Frameworks.Application | Select -Unique)

$Projects = @{}

# Projects built with the VS 2017 project system.
$Projects.Libraries = @(
    "Sarif",
    "Sarif.Converters",
    "Sarif.Driver"
)

$Projects.Applications = @(
    "Sarif.Multitool"
    )

$Projects.Products = $Projects.Libraries + $Projects.Applications

$Projects.Tests = @(
    "Test.UnitTests.Sarif",
    "Test.UnitTests.Sarif.Converters",
    "Test.UnitTests.Sarif.Driver",
    "Test.FunctionalTests.Sarif",
    "Test.UnitTests.Sarif.Multitool"
    )

$Projects.All = $Projects.Products + $Projects.Tests

Export-ModuleMember -Variable Frameworks, Projects