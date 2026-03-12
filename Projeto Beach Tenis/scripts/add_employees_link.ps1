$dir = "c:\Users\Admin\Desktop\Projeto Beach Tenis\ArenaFrontend\pages"
$files = Get-ChildItem -Path $dir -Filter "*.html" | Where-Object { $_.Name -ne "login.html" }

$t1 = "<a href=`"admin.html`" class=`"nav-item`"><span class=`"nav-icon`"><i class=`"ph ph-gear`"></i></span> Controle de Acesso</a>"
$t2 = "<a href=`"admin.html`" class=`"nav-item active`"><span class=`"nav-icon`"><i class=`"ph ph-gear`"></i></span> Controle de Acesso</a>"
$app = "`r`n                    <a href=`"admin.html#employees`" class=`"nav-item`"><span class=`"nav-icon`"><i class=`"ph ph-identification-card`"></i></span> Funcionários</a>"

foreach ($f in $files) {
    $c = [System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8)
    if (-not $c.Contains("admin.html#employees")) {
        $c = $c.Replace($t1, $t1 + $app)
        $c = $c.Replace($t2, $t2 + $app)
        [System.IO.File]::WriteAllText($f.FullName, $c, [System.Text.Encoding]::UTF8)
        Write-Host "Fixed $($f.Name)"
    }
}
