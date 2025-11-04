namespace BLL.Contracts
{
    public class DVDiscrepancia
    {
        public string Tabla { get; set; }
        public string PK { get; set; }
        public string DVHActual { get; set; }
        public string DVHCalculado { get; set; }
    }

    public interface IDVServices
    {
        string RecalcularDVV(string tabla);
        string ObtenerDVV(string tabla);
        bool VerificarTabla(string tabla, out string dvvCalculado, out string dvvGuardado);

        bool GuardarDVV(string tabla, string dvv);
        System.Collections.Generic.List<DVDiscrepancia> VerificarDVH(string tabla);
        int RecalcularDVH(string tabla);
        bool RecalcularYGuardarDVV(string tabla, out string dvv);
    }
}
