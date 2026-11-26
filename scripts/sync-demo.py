"""پس از تغییر UI، فایل‌های مشترک را به docs کپی کنید؛ data.js دمو مستقل می‌ماند."""
from pathlib import Path
import shutil
root = Path(__file__).resolve().parents[1]
source = root / 'src/BootCamp.Manager.WebApi/wwwroot'
for name in ['index.html', 'css', 'fonts']:
    src, dst = source / name, root / 'docs' / name
    if src.is_dir(): shutil.copytree(src, dst, dirs_exist_ok=True)
    else: shutil.copy2(src, dst)
shutil.copy2(source / 'js/app.js', root / 'docs/js/app.js')
print('Demo UI synchronized; standalone data adapter preserved.')
