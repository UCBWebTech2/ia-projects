FROM python:3.11-slim

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1 \
    TESSERACT_CMD=/usr/bin/tesseract

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    default-jre-headless \
    tesseract-ocr \
    tesseract-ocr-spa \
    libglib2.0-0 \
    libsm6 \
    libxext6 \
    libxrender1 \
    libgomp1 \
    libgl1 \
    poppler-utils \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt /tmp/requirements.txt
RUN pip install --upgrade pip && pip install -r /tmp/requirements.txt

COPY OCR_API /app/OCR_API

RUN mkdir -p /app/OCR_API/files /app/OCR_API/models /app/secrets

EXPOSE 8000

CMD ["uvicorn", "OCR_API.app.main:app", "--host", "0.0.0.0", "--port", "8000"]
