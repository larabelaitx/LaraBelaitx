using System;

namespace UI.Seguridad
{
    public static class Perms
    {
        // ===== USUARIO =====
        public const string Usuario_Alta = "Usuario_Alta";
        public const string Usuario_Listar = "Usuario_Listar";
        public const string Usuario_Editar = "Usuario_Editar";
        public const string Usuario_Eliminar = "Usuario_Eliminar";
        public const string Usuario_Bloquear = "Usuario_Bloquear";
        public const string Usuario_Desbloquear = "Usuario_Desbloquear";
        public const string Usuario_RestablecerPassword = "Usuario_RestablecerPassword";
        public const string Usuario_CambiarIdioma = "Usuario_CambiarIdioma";

        // ===== FAMILIA =====
        public const string Familia_Alta = "FAMILIA_ALTA";
        public const string Familia_Listar = "FAMILIA_LISTAR";
        public const string Familia_Editar = "FAMILIA_EDITAR";
        public const string Familia_Eliminar = "FAMILIA_ELIMINAR";
        public const string Familia_AsignarPatentes = "FAMILIA_ASIGNARPATENTES";

        // ===== PATENTE =====
        public const string Patente_Listar = "Patente_Listar";
        public const string Patente_AsignarAUsuario = "Patente_AsignarAUsuario"; // <— usar ésta para el módulo
        public const string Patente_Eliminar = "Patente_Eliminar";

        // ===== CLIENTE =====
        public const string Cliente_Alta = "Cliente_Alta";
        public const string Cliente_Listar = "Cliente_Listar";
        public const string Cliente_Editar = "Cliente_Editar";
        public const string Cliente_Eliminar = "Cliente_Eliminar";
        public const string Cliente_Bloquear = "Cliente_Bloquear";

        // ===== CUENTA =====
        public const string Cuenta_Alta = "Cuenta_Alta";
        public const string Cuenta_Listar = "Cuenta_Listar";
        public const string Cuenta_Editar = "Cuenta_Editar";
        public const string Cuenta_Eliminar = "Cuenta_Eliminar";
        public const string Cuenta_Suspender = "Cuenta_Suspender";
        public const string Cuenta_Reactivar = "Cuenta_Reactivar";

        // ===== TARJETA =====
        public const string Tarjeta_Alta = "Tarjeta_Alta";
        public const string Tarjeta_Listar = "Tarjeta_Listar";
        public const string Tarjeta_Editar = "Tarjeta_Editar";
        public const string Tarjeta_Eliminar = "Tarjeta_Eliminar";
        public const string Tarjeta_Imprimir = "Tarjeta_Imprimir";
        public const string Tarjeta_Bloquear = "Tarjeta_Bloquear";
        public const string Tarjeta_Reactivar = "Tarjeta_Reactivar";

        // ===== OTRAS =====
        public const string Bitacora_Listar = "Bitacora_Listar";
        public const string DV_Verificar = "DV_Verificar";
        public const string Backup_Crear = "Backup_Crear";
        public const string Backup_Restaurar = "Backup_Restaurar";
    }
}
