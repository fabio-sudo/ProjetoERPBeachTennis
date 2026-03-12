$replacements = @(
    @("Histrico", "Histórico"),
    @("Dirio", "Diário"),
    @("Relatrios", "Relatórios"),
    @("Carto", "Cartão"),
    @("Crdito", "Crédito"),
    @("Dbito", "Débito"),
    @("Aes", "Ações"),
    @("Incio", "Início"),
    @("Preo", "Preço"),
    @("ao ", "ação "),
    @("aões", "ações"),
    @("no ", "não "),
    @("at ", "até "),
    @("Unitrio", "Unitário"),
    @("excludo", "excluído"),
    @("Cobrana", "Cobrança"),
    @("Transferncia", "Transferência"),
    @("Lanamento", "Lançamento"),
    @("lanamento", "lançamento"),
    @("ser ", "será "),
    @("subtrado", "subtraído"),
    @("perodo", "período"),
    @("ltimos", "Últimos"),
    @("crtico", "crítico"),
    @("Annimo", "Anônimo"),
    @("Restitudo", "Restituído"),
    @("Nmero", "Número")
)

$files = Get-ChildItem -Path "c:\Users\Admin\Desktop\Projeto Beach Tenis\ArenaFrontend\pages" -Filter *.html

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content
    
    foreach ($pair in $replacements) {
        $content = $content.Replace($pair[0], $pair[1])
    }
    
    if ($content -cne $original) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "Fixed encoding in $($file.Name)"
    }
}
