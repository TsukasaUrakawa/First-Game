param(
    [string]$BuildDirectory = 'Builds/Unityroom/ShelfCare',
    [int]$Port = 8765,
    [string]$EditorPath
)
$ErrorActionPreference = 'Stop'
if (-not $EditorPath) {
    $projectRoot = Split-Path $PSScriptRoot -Parent
    $versionLine = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') | Select-Object -First 1
    $version = ($versionLine -split ': ', 2)[1].Trim()
    $EditorPath = Join-Path ${env:ProgramFiles} "Unity/Hub/Editor/$version/Editor/Unity.exe"
}
$editorDirectory = Split-Path $EditorPath -Parent
$pythonPath = Join-Path $editorDirectory 'Data/PlaybackEngines/WebGLSupport/BuildTools/Emscripten/python/python.exe'
if (-not (Test-Path -LiteralPath $pythonPath -PathType Leaf)) {
    throw 'Unity WebGL bundled Python not found. Pass -EditorPath with the installed Unity.exe path, or run ServeUnityroomBuild.py with Python 3.'
}
& $pythonPath (Join-Path $PSScriptRoot 'ServeUnityroomBuild.py') --directory $BuildDirectory --port $Port
exit $LASTEXITCODE
