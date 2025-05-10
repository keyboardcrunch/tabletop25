
$InstallerSource = "C:\Users\frank\projects\tabletop25\Installer\ProgramFiles\"
$InstallDest = "C:\Users\frank\AppData\Local\Programs\BeaverNotesPro\"

$UpdateInstalled = Read-Host "Update current installation? (y/N)"

$CompilePaths = @(
    'C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverElevateService\bin\Release\',
    'C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverLib\bin\Release\',
    'C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverSync\bin\Release\',
    'C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverUpdate\bin\Release\'
)

Write-Host "Moving files..." -ForegroundColor Yellow
ForEach ($proj in $CompilePaths) {
    Get-ChildItem -Path $proj -Filter "*.exe" | % { Copy-Item $_.FullName -Destination $InstallerSource -Force -Verbose }
    Get-ChildItem -Path $proj -Filter "*.dll" | % { Copy-Item $_.FullName -Destination $InstallerSource -Force -Verbose }
    if ($UpdateInstalled -eq "y") {
        Get-ChildItem -Path $proj -Filter "*.exe" | % { Copy-Item $_.FullName -Destination $InstallDest -Force -Verbose }
        Get-ChildItem -Path $proj -Filter "*.dll" | % { Copy-Item $_.FullName -Destination $InstallDest -Force -Verbose }
    }
}