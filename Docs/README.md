Printer Integration docs

Files:
- PrinterIntegration.md — full step-by-step documentation for Android Wi‑Fi & static URL printing
- MakePdf.ps1 — PowerShell helper to convert Markdown to PDF using pandoc or wkhtmltopdf

To generate PDF locally (Windows PowerShell):
1. Install pandoc (https://pandoc.org/installing.html) and a TeX engine such as TinyTeX or TeX Live if you need PDF generation via xelatex.
2. Open PowerShell in the Docs folder and run:
   .\MakePdf.ps1

Alternative quick conversion with pandoc and HTML engine:
pandoc PrinterIntegration.md -o PrinterIntegration.pdf --pdf-engine=weasyprint

If you want me to attempt generating the PDF directly inside the repository, tell me and I will try to run conversion using available tools on the environment.
