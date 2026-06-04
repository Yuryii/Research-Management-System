$ErrorActionPreference = 'Stop'
$SourceAbs = (Resolve-Path -LiteralPath 'C:\Users\dungl\.cursor\worktrees\sqlite-dev-e3f2a1b0').Path
$MainAbs = (Resolve-Path -LiteralPath 'C:\Users\dungl\Downloads\Carrer\Projects\CA-RMS').Path

$SourceGitDir = (git -C $SourceAbs rev-parse --git-common-dir).Trim()
$MainGitDir = (git -C $MainAbs rev-parse --git-common-dir).Trim()
if (-not [System.IO.Path]::IsPathRooted($SourceGitDir)) {
    $SourceGitDir = Join-Path $SourceAbs $SourceGitDir
}
if (-not [System.IO.Path]::IsPathRooted($MainGitDir)) {
    $MainGitDir = Join-Path $MainAbs $MainGitDir
}
$SourceCommon = (Resolve-Path $SourceGitDir).Path
$MainCommon = (Resolve-Path $MainGitDir).Path

if ($SourceCommon -ne $MainCommon) {
    Write-Error "source and main do not share the same repository. Source: $SourceCommon vs Main: $MainCommon"
    exit 1
}

$untrackedRaw = git -C $SourceAbs ls-files --others --exclude-standard
$untrackedFiles = @()
if ($untrackedRaw) {
    $untrackedFiles = $untrackedRaw -split "`n" | Where-Object { $_.Trim() -ne '' }
}

$trackedDiff = git -C $SourceAbs diff --name-only HEAD
$stagedDiff = git -C $SourceAbs diff --cached --name-only
$hasTracked = (($trackedDiff) -or ($stagedDiff))

if (-not $hasTracked -and $untrackedFiles.Count -eq 0) {
    Write-Output "Nothing to apply."
    exit 0
}

if ($hasTracked) {
    git -C $SourceAbs add -u -- .
    git -C $SourceAbs commit -m "tmp: worktree apply snapshot" --no-verify --allow-empty
    $tempCommit = (git -C $SourceAbs rev-parse HEAD).Trim()
    try {
        git -C $MainAbs cherry-pick --no-commit $tempCommit
    } catch {}
    git -C $MainAbs reset HEAD -- . 2>$null
    git -C $SourceAbs reset --mixed HEAD~1 2>$null
}

foreach ($f in $untrackedFiles) {
    $dst = Join-Path $MainAbs $f
    $dstDir = Split-Path $dst -Parent
    if (-not (Test-Path $dstDir)) {
        New-Item -ItemType Directory -Path $dstDir -Force | Out-Null
    }
    Copy-Item -Path (Join-Path $SourceAbs $f) -Destination $dst -Force
}

git -C $MainAbs status --short
Write-Output "MAIN_WORKTREE=$MainAbs"
Write-Output "SOURCE_WORKTREE=$SourceAbs"
