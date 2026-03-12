$files = Get-ChildItem -Path "c:\Users\Admin\Desktop\Projeto Beach Tenis\ArenaFrontend\pages" -Filter *.html

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content
    
    # 1. Fix corrupted variables (the most critical)
    # These are specific patterns created by mojibake of 'userÃ¡', 'currentCatÃ©', etc.
    $content = $content.Replace("userÃ¡", "user")
    $content = $content.Replace("userǭ", "user")
    $content = $content.Replace("userǭ", "user")
    $content = $content.Replace("CatǸ", "Cat")
    $content = $content.Replace("catǸ", "cat")
    $content = $content.Replace("currentCatÃ©", "currentCat")
    $content = $content.Replace("matchesCatÃ©", "matchesCat")
    $content = $content.Replace("matchesCatǜ", "matchesCat")
    
    # 2. Fix grammatical particles (ao, no) over-replaced
    # Looking for the mojibake versions of "ação " and "não " that replaced "ao " and "no "
    $content = $content.Replace("aÃ§Ã£o ", "ao ")
    $content = $content.Replace("aǜo ", "ao ")
    $content = $content.Replace("aǜo ", "ao ")
    $content = $content.Replace("nÃ£o ", "no ")
    $content = $content.Replace("nǜo ", "no ")
    $content = $content.Replace("nǜo ", "no ")
    
    # 3. Fix words with accents that the previous script messed up
    # Common ones identified in Select-String:
    $content = $content.Replace("alunÃ£o", "aluno")
    $content = $content.Replace("alunǜo", "aluno")
    $content = $content.Replace("AlunÃ£o", "Aluno")
    $content = $content.Replace("Alunǜo", "Aluno")
    
    $content = $content.Replace("RelatÃ³rios", "Relatórios")
    $content = $content.Replace("HistÃ³rico", "Histórico")
    $content = $content.Replace("DiÃ¡rio", "Diário")
    $content = $content.Replace("CrǸdito", "Crédito")
    $content = $content.Replace("DǸbito", "Débito")
    $content = $content.Replace("Cartǜo", "Cartão")
    $content = $content.Replace("Gestǜo", "Gestão")
    $content = $content.Replace("perodo", "período") # Catch the binary mess
    $content = $content.Replace("periodo", "período") 
    $content = $content.Replace("Aes", "Ações")
    $content = $content.Replace("Lanamento", "Lançamento")
    $content = $content.Replace("Lanar", "Lançar")
    
    if ($content -cne $original) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "Repaired $($file.Name)"
    }
}
