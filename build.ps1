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
	$WritePath = $false
)

# Clean output path
 Get-ChildItem -Path $OutputPath | ForEach-Object {
    Write-Verbose -Message "Removing existing file or directory: $($_.FullName)"
	 Remove-Item -Path $_.FullName -Force -Recurse
} 

Write-Verbose "Starting build..."

# Build Application
dotnet publish "$SourcePath/SonosRemote/SonosRemote.csproj" `
    --configuration "$Configuration" `
    --output "$OutputPath/SonosRemote" `
    --arch "$Architecture" `
    --os "$OperatingSystem" `
    --self-contained false *>&1 | Write-Verbose

if ($LASTEXITCODE -ne 0) {
    Write-Verbose "Build FAILED."
    Write-Error -Message "dotnet publish exited with code '$LASTEXITCODE'"
    exit $LASTEXITCODE
}

Write-Verbose "Build succeeded."

## Add files from the source directory to copy
#$files = @()
#
#$files += Join-Path -Path "$SourcePath" -ChildPath "Install.ps1"
#$files += Join-Path -Path "$SourcePath" -ChildPath "Uninstall.ps1"
#$files += Join-Path -Path "$SourcePath" -ChildPath "Update.ps1"
#
#$files | ForEach-Object {
#    Copy-Item $_ -Destination $OutputPath -Force | Out-Null
#}

## Create the archive
$timestamp = (Get-ChildItem (Join-Path -Path "$OutputPath/SonosRemote/" -ChildPath "SonosRemote.dll")).LastWriteTime
$timestampAsDateTime = [System.DateTime]::Parse($timestamp)
$formattedTimestamp = $timestampAsDateTime.ToString('yyyy-MM-dd_hh-mm-ss')
$formattedRandomId = [System.Guid]::NewGuid().ToString().ToUpper().Split("-")[3]

$filename = "SonosRemote-$($formattedTimestamp)_$($formattedRandomId).zip"
$archiveFullPath = Join-Path -Path $OutputPath -ChildPath $filename

Write-Verbose -Message "Full list of items to be archived:"

Get-ChildItem -Path $OutputPath -Recurse | ForEach-Object {
    Write-Verbose -Message "`t$($_.FullName)"
    $distFiles += $_.FullName
}

$distFiles = @()

Get-ChildItem -Path $OutputPath | ForEach-Object {
    Write-Verbose -Message "Adding item to archive: $($_.FullName)"
	$distFiles += $_.FullName
}

Write-Verbose -Message "Creating archive: $archiveFullPath"

$compress = @{
  Path = $distFiles
  CompressionLevel = "Fastest"
  DestinationPath = $archiveFullPath
  Force = $true
}

Compress-Archive @compress

Write-Verbose -Message "Archive created at: $archiveFullPath"
Write-Verbose "Build completed successfully"

if ($WritePath) {
   return $archiveFullPath
}