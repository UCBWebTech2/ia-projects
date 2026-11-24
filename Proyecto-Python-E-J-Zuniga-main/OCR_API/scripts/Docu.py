from argparse import ArgumentParser
import cv2
import numpy as np
import pytesseract

# Especifica la ruta a Tesseract
pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'

# Definimos los argumentos de entrada del programa.
argument_parser = ArgumentParser()
argument_parser.add_argument('-i', '--image', type=str, required=True, help='Ruta a la imagen de entrada.')
arguments = vars(argument_parser.parse_args())

# Cargamos la imagen y la convertimos a escala de grises.
image = cv2.imread(arguments['image'])
text = pytesseract.image_to_string(image)
print("Resultado Original: "+text)
gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
text = pytesseract.image_to_string(gray)
print("Resultado gray: "+text)
# Aplicamos thresholding automático con el algoritmo de Otsu.
thresholded = cv2.threshold(gray, 0, 255, cv2.THRESH_BINARY_INV | cv2.THRESH_OTSU)[1]
cv2.imshow('Otsu', thresholded)
cv2.waitKey(0)
text = pytesseract.image_to_string(thresholded)
print("resultado thresholded: "+ text)

# Calculamos y normalizamos la transformada de distancia.
dist = cv2.distanceTransform(thresholded, cv2.DIST_L2, 5)
dist = cv2.normalize(dist, dist, 0, 1.0, cv2.NORM_MINMAX)
dist = (dist * 255).astype('uint8')
cv2.imshow('Dist', dist)
cv2.waitKey(0)
text = pytesseract.image_to_string(dist)
print("Resultado Dist: "+text)
# Aplicamos thresholding al resultado de la operación anterior.
dist_otsu = cv2.threshold(dist, 0, 255, cv2.THRESH_BINARY | cv2.THRESH_OTSU)[1]
cv2.imshow('Dist Otsu', dist_otsu)
cv2.waitKey(0)

# Extraemos todo el texto de la imagen dist Otsu.
text = pytesseract.image_to_string(dist_otsu)
print(text)

# Mostramos la imagen resultante de todo el procesamiento.
cv2.imshow('Final', dist_otsu)
cv2.waitKey(0)

# Destruimos las ventanas creadas durante la ejecución del programa. 
cv2.destroyAllWindows()
