using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BE
{
    public static class EstadosUsuario
    {
        public const int Habilitado = 1;
        public const int Bloqueado = 2;
        public const int Baja = 3;
    }

    public class Usuario
    {
        public Usuario()
        {
            Permisos = new List<Permiso>();
            DebeCambiarContraseña = true;
            Tries = 0;
        }

        public int Id { get; set; }

        [DisplayName("Nombre")]
        public string Name { get; set; }

        [DisplayName("Apellido")]
        public string LastName { get; set; }

        [DisplayName("Usuario")]
        public string UserName { get; set; }

        [DisplayName("Email")]
        public string Email { get; set; }

        [Browsable(false)]
        public string PasswordHash { get; set; }

        [Browsable(false)]
        public string PasswordSalt { get; set; }

        [Browsable(false)]
        public int PasswordIterations { get; set; }

        [Browsable(false)]
        [DisplayName("Intentos Fallidos")]
        public int Tries { get; set; }

        [Browsable(false)]
        [DisplayName("Debe Cambiar Contraseña")]
        public bool DebeCambiarContraseña { get; set; }

        [Browsable(false)]
        public int EstadoUsuarioId { get; set; }

        [DisplayName("Habilitado")]
        public bool IsEnabled => EstadoUsuarioId == EstadosUsuario.Habilitado;

        [DisplayName("Estado")]
        public string EstadoDisplay
        {
            get
            {
                switch (EstadoUsuarioId)
                {
                    case EstadosUsuario.Habilitado: return "Habilitado";
                    case EstadosUsuario.Bloqueado: return "Bloqueado";
                    case EstadosUsuario.Baja: return "Baja";
                    default: return "Desconocido";
                }
            }
        }

        [Browsable(false)]
        public int? IdiomaId { get; set; }

        [DisplayName("Idioma")]
        public string IdiomaNombre { get; set; }

        [DisplayName("Documento")]
        public string Documento { get; set; }

        /// <summary>
        /// Si manejás un perfil principal por FK (opcional, según tu DAL/UI).
        /// </summary>
        [Browsable(false)]
        public int RolId { get; set; }

        [DisplayName("Nombre Completo")]
        public string NombreCompleto => $"{(LastName ?? "").Trim()}, {(Name ?? "").Trim()}";

        [Browsable(false)]
        public DateTime? UltimoLoginUtc { get; set; }

        [Browsable(false)]
        public DateTime? BloqueadoHastaUtc { get; set; }

        [Browsable(false)]
        public string DVH { get; set; }

        /// <summary>
        /// Permisos efectivos del usuario (directos o por familia).
        /// </summary>
        [Browsable(false)]
        public List<Permiso> Permisos { get; set; }

        [Obsolete("Usar EstadoUsuarioId/EstadoDisplay/IsEnabled.")]
        [Browsable(false)]
        public Estado Enabled
        {
            get { return new Estado { Id = EstadoUsuarioId, Name = EstadoDisplay }; }
            set { if (value != null) EstadoUsuarioId = value.Id; }
        }

        [Obsolete("Usar IdiomaId/IdiomaNombre.")]
        [Browsable(false)]
        public Idioma Language { get; set; }
    }

}
