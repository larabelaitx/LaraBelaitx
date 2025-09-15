namespace BLL.Contracts
{
    public interface IDVServices
    {
        string RecalcularDVV(string tabla);
        string ObtenerDVV(string tabla);
        bool VerificarTabla(string tabla, out string dvvCalculado, out string dvvGuardado);
    }
}
