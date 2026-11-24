-- Backfill DocumentoEntidades desde Indices (ejecutar tras 004_feature_expansion.sql)
SET XACT_ABORT ON;
INSERT INTO dbo.DocumentoEntidades (idDoc,tipoEntidad,valor,valorNormalizado,fuente,confianza,fechaCreacion)
SELECT i.idDoc,
       UPPER(LTRIM(RTRIM(i.tipoTermino))),
       LTRIM(RTRIM(i.termino)),
       UPPER(LTRIM(RTRIM(i.termino))),
       'NER',
       NULL,
       GETUTCDATE()
FROM dbo.Indices i
WHERE i.idDoc IS NOT NULL
  AND i.termino IS NOT NULL
  AND LTRIM(RTRIM(i.termino)) <> ''
  AND i.tipoTermino IS NOT NULL
  AND LTRIM(RTRIM(i.tipoTermino)) <> ''
  AND NOT EXISTS (
      SELECT 1 FROM dbo.DocumentoEntidades e
      WHERE e.idDoc = i.idDoc
        AND e.tipoEntidad = UPPER(LTRIM(RTRIM(i.tipoTermino)))
        AND e.valorNormalizado = UPPER(LTRIM(RTRIM(i.termino)))
  );
GO