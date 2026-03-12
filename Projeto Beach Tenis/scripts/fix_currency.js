const fs = require('fs');
const path = require('path');

const dir = 'c:/Users/Admin/Desktop/Projeto Beach Tenis/ArenaFrontend/pages';
const files = fs.readdirSync(dir).filter(f => f.endsWith('.html'));

const oldFmtRegex = /const\s+fmt\s*=\s*v\s*=>\s*'R\$\s*'\s*\+\s*parseFloat\(v\s*\|\|\s*0\)\.toLocaleString\('pt-BR',\s*\{\s*minimumFractionDigits:\s*2\s*\}\);/g;
const newFmt = "const fmt = v => { let n = typeof v === 'string' ? parseFloat(v.replace(',','.')) : Number(v||0); return isNaN(n) ? 'R$ 0,00' : n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }); };";

let count = 0;
files.forEach(f => {
    const fullPath = path.join(dir, f);
    let content = fs.readFileSync(fullPath, 'utf8');
    if (oldFmtRegex.test(content)) {
        content = content.replace(oldFmtRegex, newFmt);
        fs.writeFileSync(fullPath, content);
        console.log('Fixed', f);
        count++;
    }
});

console.log(`Fixed ${count} files.`);
