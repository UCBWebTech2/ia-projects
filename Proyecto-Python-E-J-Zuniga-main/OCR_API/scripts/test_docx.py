from docx import Document

doc = Document()
doc.add_heading('Document Title', 0)
doc.save('test.docx')
print("Documento creado con éxito.")
