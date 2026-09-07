Add-Type -AssemblyName System.Drawing
$dir='C:\Users\Onur\Desktop\Exit7\Assets\Loop\Signs'
for($i=0;$i -le 8;$i++){
$b=New-Object Drawing.Bitmap(2048,256)
$g=[Drawing.Graphics]::FromImage($b)
$g.Clear([Drawing.Color]::FromArgb(247,220,89))
$g.TextRenderingHint=[Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$f=New-Object Drawing.Font('Arial',150,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel)
$g.DrawString(('↑  Exit '+$i),$f,[Drawing.Brushes]::Black,660,30)
$b.Save((Join-Path $dir ('Exit'+$i+'.png')))
$f.Dispose();$g.Dispose();$b.Dispose()
}
$b=New-Object Drawing.Bitmap(1536,1536)
$g=[Drawing.Graphics]::FromImage($b)
$g.Clear([Drawing.Color]::FromArgb(239,237,222))
$g.TextRenderingHint=[Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$f=New-Object Drawing.Font('Arial',94,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel)
$g.DrawString('ÇIKIŞ REHBERİ',$f,[Drawing.Brushes]::Black,110,120)
$f.Dispose()
$f=New-Object Drawing.Font('Arial',55,[Drawing.FontStyle]::Regular,[Drawing.GraphicsUnit]::Pixel)
$lines=@('Çevreni dikkatle incele.','','Bir anormallik fark edersen','geldiğin yoldan geri dön.','','Her şey normalse','ilerlemeye devam et.','','Sekiz doğru karar seni çıkışa ulaştırır.','Yanlış karar, sayacı sıfırlar.','','İlk geçiş normaldir. Koridoru öğren.')
$y=330
foreach($line in $lines){$g.DrawString($line,$f,[Drawing.Brushes]::Black,110,$y);$y+=83}
$b.Save((Join-Path $dir 'Rules.png'))
$f.Dispose();$g.Dispose();$b.Dispose()
