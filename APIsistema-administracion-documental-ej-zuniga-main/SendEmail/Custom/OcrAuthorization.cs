namespace SendEmail.Custom
{
    public static class OcrPermissions
    {
        public const string Process = "process";
        public const string Admin = "admin";
    }

    public static class OcrOperations
    {
        public const string SubirArchivo = "SubirArchivo";
        public const string SubirArchivoPaddle = "SubirArchivoPaddle";
        public const string SubirArchivoClasificado = "SubirArchivoClasificado";
        public const string ExtraerEntidades = "ExtraerEntidades";
        public const string PrepararDataset = "PrepararDataset";
        public const string ProbarPaddleOCRImagen = "ProbarPaddleOCRImagen";
        public const string ProbarOCRAutoImagen = "ProbarOCRAutoImagen";
        public const string CompararOCR = "CompararOCR";
        public const string CompararOCRImagen = "CompararOCRImagen";
        public const string RecargarModelo = "RecargarModelo";
        public const string ModelMetrics = "ModelMetrics";
    }
}
