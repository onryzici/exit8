Add-Type -AssemblyName System.Drawing
$assetDir = 'C:\Users\Onur\Desktop\Exit7\Assets\Textures'
function New-Sign($width,$height) {
    $script:bitmap = New-Object System.Drawing.Bitmap($width,$height)
    $script:canvas = [System.Drawing.Graphics]::FromImage($script:bitmap)
    $script:canvas.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $script:canvas.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
}
function Draw-Label($text,$family,$size,$x,$y,$brush) {
    $f = New-Object System.Drawing.Font($family,$size,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Pixel)
    $script:canvas.DrawString($text,$f,$brush,[float]$x,[float]$y)
    $f.Dispose()
}
function Save-Sign($name) {$script:bitmap.Save((Join-Path $assetDir $name),[System.Drawing.Imaging.ImageFormat]::Png);$script:canvas.Dispose();$script:bitmap.Dispose()}
$black = [System.Drawing.Brushes]::Black
$red = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(165,24,19))
$jpExit = [string][char]0x51FA + [char]0x53E3
New-Sign 2048 256
$canvas.Clear([System.Drawing.Color]::FromArgb(247,220,89))
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::Black,14)
$canvas.DrawLine($pen,785,210,785,45)
$canvas.DrawLine($pen,785,45,735,105)
$canvas.DrawLine($pen,785,45,835,105)
Draw-Label $jpExit 'MS Gothic' 92 872 23 $black
Draw-Label 'Exit' 'Arial' 66 884 124 $black
Draw-Label '7' 'Arial' 218 1123 -3 $black
Save-Sign 'ExitSign.png'
New-Sign 768 960
$canvas.Clear([System.Drawing.Color]::FromArgb(235,235,224))
$pen.Width=16
foreach($x in @(60,420)) {
    $points = [System.Drawing.PointF[]]@([System.Drawing.PointF]::new($x,205),[System.Drawing.PointF]::new($x+65,110),[System.Drawing.PointF]::new($x+205,110),[System.Drawing.PointF]::new($x+270,205),[System.Drawing.PointF]::new($x+205,295),[System.Drawing.PointF]::new($x+65,295),[System.Drawing.PointF]::new($x,205))
    $canvas.DrawClosedCurve($pen,$points,0.2,[System.Drawing.Drawing2D.FillMode]::Alternate)
    $canvas.FillEllipse($black,$x+67,126,137,159)
    $canvas.FillEllipse([System.Drawing.Brushes]::Ivory,$x+114,181,41,41)
}
$cameraText = [string][char]0x9632+[char]0x72AF+[char]0x30AB+[char]0x30E1+[char]0x30E9
$activeText = [string][char]0x4F5C+[char]0x52D5+[char]0x4E2D+[char]0xFF01
Draw-Label $cameraText 'MS Gothic' 115 95 430 $black
Draw-Label $activeText 'MS Gothic' 136 113 595 $red
Draw-Label 'Security camera in operation' 'Arial' 42 70 845 $black
Save-Sign 'SecurityNotice.png'
$pen.Dispose();$red.Dispose()
