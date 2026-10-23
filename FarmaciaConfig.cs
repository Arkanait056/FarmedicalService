namespace FarmedicalService
{
    /// Datos fiscales y comerciales que aparecen en el recibo.
    /// Reemplaza estos valores con los reales de la farmacia antes de usar el sistema en producción.
   
    public static class FarmaciaConfig
    {
        public const string Nombre = "Farmedical";
        public const string Rnc = "0-00-00000-0";
        public const string Direccion = "Dirección de la farmacia";
        public const string Telefono = "(809) 000-0000";

        ///Serie del comprobante: B02 = factura de consumo.
        public const string SerieNcf = "B02";

       
        /// Primer secuencial autorizado por la DGII para la serie. El NCF se arma como serie + secuencial de 8 dígitos.
        /// Ajusta este valor (y valida el rango autorizado) antes de emitir comprobantes reales.
     
        public const int NcfSecuencialInicial = 1;
    }
}
