# ER Diagram Export Instructions

## Graphviz Online

1. Open https://dreampuf.github.io/GraphvizOnline/ or http://magjac.com/graphviz-visual-editor/

2. Copy the contents of `erd.dot` and paste into the editor

3. Click "Generate" or "Render" to view the diagram

4. Export options:
   - **PNG**: Right-click the rendered diagram → Save Image As
   - **SVG**: Use browser tools or copy the SVG code
   - **PDF**: Use browser print → Save as PDF

## Diagram Features

- **Color-coded entities**: 
  - User (Blue #90caf9)
  - Event (Green #a5d6a7)
  - Ticket (Yellow #ffe082)

- **Complete attributes** with:
  - Data types
  - Primary Keys (PK)
  - Foreign Keys (FK)
  - Unique constraints (UQ)
  - Required fields
  - Max length constraints
  - Default values
  - Nullable fields

- **Relationships**:
  - User 1..* Ticket (Cascade delete)
  - Event 1..* Ticket (Cascade delete)

- **Visual legend** for constraint types

## For Report Submission

1. Export as PNG (high resolution, 300 DPI if possible)
2. Insert into Word document
3. Add figure caption: "Figure X: Entity-Relationship Diagram"
4. Reference in text: "As shown in Figure X, the database consists of three main entities..."

