Add-Type -AssemblyName System.Drawing
$img = [System.Drawing.Bitmap]::FromFile("C:\Users\Beray\.gemini\antigravity\brain\tempmediaStorage\media__1772240157245.png")
$newImg = New-Object System.Drawing.Bitmap($img.Width, $img.Height)

for ($y = 0; $y -lt $img.Height; $y++) {
    for ($x = 0; $x -lt $img.Width; $x++) {
        $color = $img.GetPixel($x, $y)
        if ($color.A -gt 0) {
            # Decrease brightness and push towards cooler tones for dungeon effect
            $r = [Math]::Max(0, [int]($color.R * 0.6))    # Less red
            $g = [Math]::Max(0, [int]($color.G * 0.55))   # Less green
            $b = [Math]::Max(0, [int]($color.B * 0.65))   # Slightly more blue relative to other colors
            
            $newColor = [System.Drawing.Color]::FromArgb($color.A, $r, $g, $b)
            $newImg.SetPixel($x, $y, $newColor)
        } else {
            $newImg.SetPixel($x, $y, $color)
        }
    }
}

$destPath = "C:\Users\Beray\Documents\GitHub\Pateyto\Assets\Topdown\TilePalletes\Dungeon_Doors.png"
$newImg.Save($destPath, [System.Drawing.Imaging.ImageFormat]::Png)
$img.Dispose()
$newImg.Dispose()
Write-Host "Processed image saved to $destPath"
