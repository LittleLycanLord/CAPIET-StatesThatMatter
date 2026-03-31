#!/usr/bin/env python3
"""
Process LaTeX and BibTeX files to create a consolidated plain text document.
More robust version that handles large files efficiently.
"""

import re
from pathlib import Path

# Configure paths
BASE_PATH = Path(r"d:\vanil\Documents\GithHub Repos\CAPIET-StatesThatMatter\Assets\Thesis Proper")

def clean_latex(text):
    """Remove LaTeX commands while preserving text content."""
    # Replace \cite{key1,key2,...} with [key1,key2,...]
    text = re.sub(r'\\cite\{([^}]+)\}', r'[\1]', text)
    
    # Remove formatting commands but keep content
    text = re.sub(r'\\textbf\{([^}]+)\}', r'\1', text)
    text = re.sub(r'\\textit\{([^}]+)\}', r'\1', text)
    text = re.sub(r'\\underline\{([^}]+)\}', r'\1', text)
    text = re.sub(r'\\emph\{([^}]+)\}', r'\1', text)
    text = re.sub(r'\\text[a-z]+\{([^}]+)\}', r'\1', text)
    
    # Remove chapter/section/subsection commands but keep titles
    text = re.sub(r'\\chapter\{([^}]+)\}', r'CHAPTER_TITLE:\1', text)
    text = re.sub(r'\\section\{([^}]+)\}', r'SECTION_TITLE:\1', text)
    text = re.sub(r'\\subsection\{([^}]+)\}', r'SUBSECTION_TITLE:\1', text)
    text = re.sub(r'\\subsubsection\{([^}]+)\}', r'SUBSUBSECTION_TITLE:\1', text)
    
    # Handle figures - extract captions
    figure_pattern = r'\\caption\{([^}]+)\}'
    text = re.sub(figure_pattern, r'[FIGURE: \1]', text)
    
    # Remove figure/table environments
    text = re.sub(r'\\begin\{(figure|table|subfigure|enumerate|itemize)\}.*?\\end\{\1\}', '', text, flags=re.DOTALL)
    
    # Remove remaining LaTeX commands
    text = re.sub(r'\\[a-zA-Z]+\{([^}]*)\}', r'\1', text)
    text = re.sub(r'\\[a-zA-Z]+\[[^\]]*\]\{([^}]*)\}', r'\1', text)
    text = re.sub(r'\\[a-zA-Z]+', '', text)
    
    # Remove special symbols
    text = text.replace('$', '').replace('~', ' ').replace('^', '')
    
    # Clean up braces and brackets
    text = re.sub(r'[{}]', '', text)
    
    # Clean up extra whitespace
    text = re.sub(r'\n\s*\n\s*\n', '\n\n', text)
    
    return text

def parse_bibtex_entry(entry_text):
    """Parse a single BibTeX entry."""
    author_match = re.search(r'author\s*=\s*\{([^}]+)\}', entry_text)
    year_match = re.search(r'year\s*=\s*\{([^}]+)\}', entry_text)
    title_match = re.search(r'title\s*=\s*\{([^}]+)\}', entry_text)
    journal_match = re.search(r'journal\s*=\s*\{([^}]+)\}', entry_text)
    booktitle_match = re.search(r'booktitle\s*=\s*\{([^}]+)\}', entry_text)
    
    author = author_match.group(1) if author_match else "Unknown"
    year = year_match.group(1) if year_match else "n.d."
    title = title_match.group(1) if title_match else "Unknown"
    
    # Parse first author's last name
    if ' and ' in author:
        first_author = author.split(' and ')[0].strip()
    else:
        first_author = author.strip()
    
    # Extract last name from "First Last" or "Last, First" format
    if ',' in first_author:
        last_name = first_author.split(',')[0].strip()
    else:
        parts = first_author.split()
        last_name = parts[-1] if parts else first_author
    
    return {
        'author': author,
        'year': year,
        'title': title,
        'cite_str': f"{last_name} ({year})",
        'journal': journal_match.group(1) if journal_match else '',
        'booktitle': booktitle_match.group(1) if booktitle_match else ''
    }

def extract_citations_from_bib():
    """Extract all citations from BibTeX file."""
    bib_file = BASE_PATH / "references.bib"
    citations = {}
    
    if not bib_file.exists():
        print(f"BibTeX file not found: {bib_file}")
        return citations
    
    with open(bib_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Find all @type{key, ... } entries
    pattern = r'@(\w+)\{([^,]+),\s*(.*?)\n\}'
    matches = re.finditer(pattern, content, re.DOTALL)
    
    for match in matches:
        key = match.group(2).strip()
        entry = match.group(3)
        citations[key] = parse_bibtex_entry(entry)
    
    return citations

def process_chapter(chapter_num):
    """Process a single chapter file."""
    chapter_file = BASE_PATH / "chapters" / f"chapter{chapter_num}" / f"chapter{chapter_num}.tex"
    
    if not chapter_file.exists():
        return None, None, None
    
    with open(chapter_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Remove comments
    lines = [line if not line.strip().startswith('%') else '' for line in content.split('\n')]
    content = '\n'.join(lines)
    
    # Clean LaTeX
    cleaned = clean_latex(content)
    
    # Extract chapter title
    chapter_match = re.search(r'CHAPTER_TITLE:([^\n]+)', cleaned)
    chapter_title = chapter_match.group(1).strip() if chapter_match else f"Chapter {chapter_num}"
    cleaned = re.sub(r'CHAPTER_TITLE:[^\n]+\n', '', cleaned)
    
    return chapter_num, chapter_title, cleaned

def process_sections(text, chapter_num):
    """Format sections properly."""
    section_num = 0
    subsection_num = 0
    result = []
    
    for line in text.split('\n'):
        if 'SECTION_TITLE:' in line:
            section_num += 1
            subsection_num = 0
            title = line.replace('SECTION_TITLE:', '').strip()
            result.append(f"\n--- SECTION {chapter_num}.{section_num}: {title} ---\n")
        elif 'SUBSECTION_TITLE:' in line:
            subsection_num += 1
            title = line.replace('SUBSECTION_TITLE:', '').strip()
            result.append(f"\n+++ SUBSECTION {chapter_num}.{section_num}.{subsection_num}: {title} +++\n")
        elif 'SUBSUBSECTION_TITLE:' in line:
            title = line.replace('SUBSUBSECTION_TITLE:', '').strip()
            result.append(f"\n... {title}\n")
        elif line.strip():
            result.append(line)
    
    return '\n'.join(result)

def main():
    print("Processing LaTeX thesis files...")
    
    # Extract all BibTeX citations
    print("Reading bibliography file...")
    all_citations = extract_citations_from_bib()
    print(f"Found {len(all_citations)} bibliography entries")
    
    # Process all chapters
    chapters_data = []
    for i in range(1, 7):
        print(f"Processing chapter {i}...")
        chapter_num, title, content = process_chapter(i)
        if content:
            formatted = process_sections(content, chapter_num)
            chapters_data.append((chapter_num, title, formatted))
    
    # Collect all citations found in text
    found_citations = set()
    for _, _, content in chapters_data:
        matches = re.findall(r'\[([^\]]+)\]', content)
        for match in matches:
            # Split by comma if multiple citations
            for cit in match.split(','):
                cit = cit.strip()
                if cit in all_citations:
                    found_citations.add(cit)
    
    # Build output document
    output = []
    output.append("=" * 80)
    output.append("THESIS: WIZ'S JOURNEY: MASTERING THE STATES OF MATTER")
    output.append("=" * 80)
    output.append("")
    
    # Add chapters
    for chapter_num, title, content in chapters_data:
        output.append(f"\n{'=' * 80}")
        output.append(f"=== CHAPTER {chapter_num}: {title} ===")
        output.append(f"{'=' * 80}")
        output.append(content)
    
    # Add References section
    output.append(f"\n{'=' * 80}")
    output.append("REFERENCES")
    output.append(f"{'=' * 80}\n")
    
    # Sort references alphabetically
    sorted_citations = sorted(found_citations, key=lambda x: all_citations[x]['cite_str'] if x in all_citations else x)
    
    for cite_key in sorted_citations:
        if cite_key in all_citations:
            info = all_citations[cite_key]
            cite_str = info['cite_str']
            title = info['title']
            author = info['author']
            year = info['year']
            
            # Format reference entry
            output.append(f"{cite_str}. \"{title}.\"")
            if info['journal']:
                output.append(f"  Journal: {info['journal']}")
            if info['booktitle']:
                output.append(f"  In: {info['booktitle']}")
            output.append("")
    
    # Write output file
    output_file = BASE_PATH / "THESIS_CONTENT_CONSOLIDATED.txt"
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write('\n'.join(output))
    
    print(f"\nConsolidated thesis written to: {output_file}")
    print(f"Chapters processed: {len(chapters_data)}")
    print(f"Citations formatted: {len(sorted_citations)}")

if __name__ == "__main__":
    main()
