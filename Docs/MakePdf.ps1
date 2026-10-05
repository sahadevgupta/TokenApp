# MakePdf.ps1 — convert PrinterIntegration.md to PDF using pandoc (or wkhtmltopdf as fallback)

$md = "PrinterIntegration.md"
$out = "PrinterIntegration.pdf"
$docsDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $docsDir

if (Get-Command pandoc -ErrorAction SilentlyContinue) {
	Write-Output "Using pandoc to generate PDF..."
	pandoc $md -o $out --pdf-engine=xelatex
	if (Test-Path $out) { Write-Output "PDF created: $out" } else { Write-Error "Pandoc conversion failed." }
	return
}

if (Get-Command wkhtmltopdf -ErrorAction SilentlyContinue) {
	Write-Output "Using wkhtmltopdf to generate PDF via HTML conversion..."
	$html = "PrinterIntegration.html"
	pandoc $md -o $html || { Write-Error "Markdown->HTML failed"; return }
	wkhtmltopdf $html $out
	if (Test-Path $out) { Write-Output "PDF created: $out" } else { Write-Error "wkhtmltopdf conversion failed." }
	return
}

Write-Error "No conversion tool found. Install pandoc or wkhtmltopdf to generate PDF. You can also copy the Markdown file to any machine with conversion tools and run: pandoc PrinterIntegration.md -o PrinterIntegration.pdf --pdf-engine=xelatex"
