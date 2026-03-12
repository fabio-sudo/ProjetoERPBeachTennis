$dir = "c:\Users\Admin\Desktop\Projeto Beach Tenis\ArenaFrontend\pages"
$files = Get-ChildItem -Path $dir -Filter "*.html" | Where-Object { $_.Name -ne "login.html" }

foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw -Encoding UTF8
    
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u00A3", "ã")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u00A1", "á")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u00B3", "ó")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u00B5", "õ")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u00A9", "é")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u00AA", "ê")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u00A7", "ç")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u00A2", "â")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u00BA", "ú")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u00B4", "ô")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u20AC", "À")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u00AD", "í")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3\u2021", "Ç")
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00E2\u20AC\u201C", "–") # â€“
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\u00C3", "í") # leftovers for isolated Ã
    
    Set-Content -Path $f.FullName -Value $content -Encoding UTF8
    Write-Host "Fixed $($f.Name)"
}
