# CAPIET2 Thesis - LaTeX Structure

## Project Structure

```
Assets/Thesis Proper/
├── main.tex                    # Main document file - compile this
├── references.bib              # Bibliography database
├── DLSU-CCS-Logo.png          # University logo for title page
│
└── chapters/
    ├── frontmatter/
    │   ├── titlepage.tex      # Title page
    │   ├── abstract.tex       # English abstract
    │   └── abstrak.tex        # Filipino abstract
    │
    ├── chapter1/              # Introduction
    │   ├── chapter1.tex
    │   ├── figures/          # Put chapter 1 figures here
    │   └── tables/           # Put chapter 1 tables here
    │
    ├── chapter2/              # Review of Related Literature
    │   ├── chapter2.tex
    │   ├── figures/
    │   └── tables/
    │       ├── table_educational_challenges.tex
    │       ├── table_science_education.tex
    │       ├── table_serious_games.tex
    │       ├── table_chemistry_gbl.tex
    │       ├── table_stoichiometry.tex
    │       └── table_bubble_shooter.tex
    │
    ├── chapter3/              # Game Design
    │   ├── chapter3.tex
    │   ├── figures/
    │   └── tables/
    │       └── game_loop.tex
    │
    ├── chapter4/              # Design and Implementation
    │   ├── chapter4.tex
    │   ├── figures/
    │   └── tables/
    │
    └── chapter5/              # Methodology
        ├── chapter5.tex
        ├── figures/
        └── tables/
```

## How to Compile

### Option 1: Using VS Code with LaTeX Workshop
1. Install the LaTeX Workshop extension
2. Open `main.tex`
3. Click the green "Build LaTeX project" button, or press `Ctrl+Alt+B`

### Option 2: Command Line (if you have TeX Live or MiKTeX installed)
```bash
cd "d:\vanil\Documents\GithHub Repos\CAPIET-StatesThatMatter\Assets\Thesis Proper"
pdflatex main.tex
biber main
pdflatex main.tex
pdflatex main.tex
```

## Document Settings
- **Font**: Times New Roman, 12pt
- **Line Spacing**: 1.5 (onehalfspacing)
- **Margins**: 1 inch all sides
- **Document Class**: report
- **Citation Style**: APA (using biblatex)

## TODO Items

### Tables (Chapter 2)
The following tables have placeholder structures and need content filled in:
- `chapters/chapter2/tables/table_educational_challenges.tex` (lines 76-102 from original)
- `chapters/chapter2/tables/table_science_education.tex` (lines 123-152)
- `chapters/chapter2/tables/table_serious_games.tex` (lines 163-189)
- `chapters/chapter2/tables/table_chemistry_gbl.tex` (lines 194-283)
- `chapters/chapter2/tables/table_stoichiometry.tex` (lines 298-377)
- `chapters/chapter2/tables/table_bubble_shooter.tex` (lines 382-430)

### Table (Chapter 3)
- `chapters/chapter3/tables/game_loop.tex` (lines 465-488 from original)

### Missing Content
- Chapter 4: Design Changes section needs content
- Chapter 4: Implementation Challenges section needs content

### Bibliography
Some references need complete details (marked with TODO notes):
- Saputri2025
- Bridges2015
- Blyznyuk2024
- Treagust2018
- Vallespin2024
- Vega2024

## Adding Figures

To add a figure to any chapter:

1. Place your image file (PNG, JPG, PDF) in the appropriate `figures/` folder
2. In the chapter `.tex` file, add:

```latex
\begin{figure}[htbp]
    \centering
    \includegraphics[width=0.8\textwidth]{your_image_name.png}
    \caption{Your caption here}
    \label{fig:your_label}
\end{figure}
```

3. Reference it in text with: `Figure \ref{fig:your_label}`

## Adding Tables

Tables can be added directly in chapter files or in separate files in the `tables/` folder.

For complex tables, create a new `.tex` file in the appropriate `tables/` folder and use `\input` in the chapter file.

## Customization

### Changing Title
Edit `chapters/frontmatter/titlepage.tex`

### Changing Margins
In `main.tex`, modify: `\usepackage[margin=1in]{geometry}`

### Changing Font Size
In `main.tex`, modify the document class: `\documentclass[12pt,a4paper]{report}`

## Notes
- The document uses biblatex with biber backend for citations
- All citations use `\parencite{key}` for parenthetical citations
- Chapter titles are automatically uppercase and centered
- Roman numerals for front matter, Arabic for main content
