import os
import glob

directory = r"c:\Users\Admin\Desktop\Projeto Beach Tenis\ArenaFrontend\pages"
html_files = glob.glob(os.path.join(directory, "*.html"))

target_str = '<a href="admin.html#employees" class="nav-item"><span class="nav-icon"><i class="ph ph-identification-card"></i></span> Funcionários</a>'
target_str_active = '<a href="admin.html#employees" class="nav-item active"><span class="nav-icon"><i class="ph ph-identification-card"></i></span> Funcionários</a>'

for file_path in html_files:
    with open(file_path, "r", encoding="utf-8") as f:
        content = f.read()
    
    # Remove lines containing the target string
    lines = content.split('\n')
    new_lines = []
    for line in lines:
        if 'href="admin.html#employees"' not in line:
            new_lines.append(line)
            
    new_content = '\n'.join(new_lines)
    if new_content != content:
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(new_content)
        print(f"Updated {file_path}")
