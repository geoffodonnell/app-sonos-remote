[CmdletBinding()]
param (
    [Parameter(Position=0, mandatory=$false)]
    [string] $SourcePath = "$PSScriptRoot/src",
    [Parameter(Position=1, mandatory=$false)]
    [string] $OutputPath = "$PSScriptRoot/build",
	[Parameter(Position=2, mandatory=$false)]
    [string] $Configuration = "Release",
	[Parameter(Position=3, mandatory=$false)]
	$OperatingSystem = "linux",
	[Parameter(Position=4, mandatory=$false)]
	$Architecture = "arm64",
	[Parameter(Position=5, mandatory=$false)]
    [string] $User = "",
	[Parameter(Position=6, mandatory=$false)]
	$HostName = "",
	[Parameter(Position=7, mandatory=$false)]
	$DestinationPath = "~/Deployment"
)

Function Invoke-Ssh {
    [CmdletBinding()]
    param (
        [string] $User,
        [string] $HostName,
        [string] $Command
    )

    $exe = "ssh";
    $arg0 = [System.String]::IsNullOrWhiteSpace($User) ? $HostName : "$User@$HostName";
    $arg1 = $Command

    Write-Verbose -Message "Executing ssh command `"$Command`""

    $result = & $exe $arg0 $arg1
    $result = [System.String]::Join("`r`n", $result ?? @());

    Write-Verbose -Message "Executed ssh command, result:`n$result "

    if ($LASTEXITCODE -ne 0) {
        Write-Error -Message "ssh exited with code '$LASTEXITCODE': $result"
    }

    return $result
}

Function Invoke-ScpUpload {
    [CmdletBinding()]
    param (
        [string] $User,
        [string] $HostName,
        [string] $SourcePath,
        [string] $DestinationPath
    )

    $exe = "scp";
    $arg0 = $SourcePath;
    $arg1 = [System.String]::IsNullOrWhiteSpace($User) ? "$($HostName):$DestinationPath" : "$($User)@$($HostName):$DestinationPath";

    Write-Verbose -Message "Executing scp command, copy $arg0 tp $arg1"

    $result = & $exe $arg0 $arg1
    $result = [System.String]::Join("`r`n", $result ?? @());

    Write-Verbose -Message "Executed scp command, result:`n$result "

    if ($LASTEXITCODE -ne 0) {
        Write-Error -Message "scp exited with code '$LASTEXITCODE': $result"
    }

    return $result
}

$ArchivePath = ""

## Build the project
try {
    $ArchivePath = & "$PSScriptRoot/build.ps1" `
        -SourcePath $SourcePath `
        -OutputPath $OutputPath `
        -Configuration $Configuration `
        -OperatingSystem $OperatingSystem `
        -Architecture $Architecture `
        -WritePath $true

    if ($LASTEXITCODE -ne 0) {
        Write-Error -Message "Build script exited with code '$LASTEXITCODE'"
        exit $LASTEXITCODE
    }
} catch {
    Write-Error -Message "Build script failed: $_"
    exit 1
}

Write-Verbose "Archive created at: $ArchivePath"

$archiveFileName = Get-Item -Path $ArchivePath | Select-Object -ExpandProperty Name

## Upload the build artifact    
Invoke-Ssh -User $User -HostName $HostName -Command "rm -rf $($DestinationPath)/{*,.*}" | Out-Null
Invoke-ScpUpload -User $User -HostName $HostName -SourcePath $ArchivePath -DestinationPath $DestinationPath | Out-Null
Invoke-Ssh -User $User -HostName $HostName -Command "unzip -o -d $($DestinationPath) $($DestinationPath)/$($archiveFileName)" | Out-Null
Invoke-Ssh -User $User -HostName $HostName -Command "sudo chmod 764 $($DestinationPath)/SonosRemote/SonosRemote" | Out-Null

Write-Verbose "Build and upload completed successfully"