
$InstallerSource = "C:\Users\frank\projects\tabletop25\Installer\ProgramFiles\"
$InstallDest = "C:\Users\frank\AppData\Local\Programs\BeaverNotesPro\"

$UpdateInstalled = Read-Host "Update current installation? (y/N)"

$CompilePaths = @(
    'C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverElevateService\bin\Release\',
    'C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverLib\bin\Release\',
    'C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverSync\bin\Release\',
    'C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverUpdate\bin\Release\'
)

Write-Host "Signing files..." -ForegroundColor Yellow
Sign -sourceFile "C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverSync\bin\Release\BeaverSync.exe"
Sign -sourceFile "C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverSync\bin\Release\BeaverLib.dll"
Sign -sourceFile "C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverUpdate\bin\Release\BeaverUpdate.exe"
Sign -sourceFile "C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverUpdate\bin\Release\BeaverLib.dll"
Sign -sourceFile "C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverElevateService\bin\Release\BeaverElevateService.exe"
Sign -sourceFile "C:\Users\frank\projects\tabletop25\BeaverNotesPro\BeaverLib\bin\Release\BeaverLib.dll"


Write-Host "Moving files..." -ForegroundColor Yellow
ForEach ($proj in $CompilePaths) {
    Get-ChildItem -Path $proj -Filter "*.exe" | % { Copy-Item $_.FullName -Destination $InstallerSource -Force }
    Get-ChildItem -Path $proj -Filter "*.dll" | % { Copy-Item $_.FullName -Destination $InstallerSource -Force }
    if ($UpdateInstalled -eq "y") {
        Get-ChildItem -Path $proj -Filter "*.exe" | % { Copy-Item $_.FullName -Destination $InstallDest -Force }
        Get-ChildItem -Path $proj -Filter "*.dll" | % { Copy-Item $_.FullName -Destination $InstallDest -Force }
    }
}

# Start signing things
function Sign {
    Param(
        $sourceFile
    )
    $password = "beaver"
    $pfx = "C:\Users\frank\projects\tabletop25\beaver.pfx"
    & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe" sign `
        /f "C:\Users\frank\projects\tabletop25\beaver.pfx" `
        /sha1 "3BF63C9410E4C86EB4C5F247A5C0E2301A3EBD38" `
        /fd SHA256 `
        /p $password `
        $sourceFile
}