$dir = "c:\Users\Admin\Desktop\Projeto Beach Tenis\ArenaFrontend\pages"
$files = Get-ChildItem -Path $dir -Filter "*.html" | Where-Object { $_.Name -ne "login.html" }

$adminSection = @"
                <div class="nav-section">
                    <div class="nav-section-label">Admin</div>
                    <a href="admin.html" class="nav-item"><span class="nav-icon"><i class="ph ph-gear"></i></span> Controle de Acesso</a>
                    <a href="admin.html#employees" class="nav-item"><span class="nav-icon"><i class="ph ph-identification-card"></i></span> Funcionários</a>
                </div>
            </nav>
"@

foreach ($f in $files) {
    if ($f.Name -ne "admin.html") {
        $c = [System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8)
        if (-not $c.Contains("nav-section-label`">Admin</div>")) {
            $c = $c -replace "</nav>", $adminSection
            [System.IO.File]::WriteAllText($f.FullName, $c, [System.Text.Encoding]::UTF8)
            Write-Host "Injected Admin Section into $($f.Name)"
        }
    }
}
