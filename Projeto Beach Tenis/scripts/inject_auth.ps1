$dir = "c:\Users\Admin\Desktop\Projeto Beach Tenis\ArenaFrontend\pages"
$files = Get-ChildItem -Path $dir -Filter "*.html" | Where-Object { $_.Name -ne "login.html" }

foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw
    if (-not $content.Contains("auth.js")) {
        $content = $content.Replace("</body>", "    <script src=`"../js/auth.js`"></script>`r`n</body>")
    }
    
    $footerStr = "<div class=`"sidebar-footer`">Arena Beach v2.0</div>"
    if ($content.Contains($footerStr)) {
        $newFooter = "<div class=`"sidebar-footer`">`r`n                <button onclick=`"auth.logout()`" class=`"btn btn-ghost`" style=`"width:100%;color:var(--danger);display:flex;align-items:center;justify-content:center;gap:8px`"><i class=`"ph ph-sign-out`"></i> Sair</button>`r`n            </div>"
        $content = $content.Replace($footerStr, $newFooter)
    }

    Set-Content -Path $f.FullName -Value $content -Encoding UTF8
    Write-Host "Updated $($f.Name)"
}
