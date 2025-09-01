using System;
using Krypton.Toolkit;
using BLL;

namespace UI
{
    public partial class AltaUsuario : KryptonForm
    {
        private readonly ModoForm _modo;
        private readonly int? _idUsuario;
        private readonly IUsuarioService _svcUsuarios;
        private readonly IRolService _svcRoles;

        public AltaUsuario()   // el diseñador usa éste
        {
            InitializeComponent();
        }

        // ⬇⬇⬇  ESTE es el constructor que vamos a usar desde MainUsuarios
        public AltaUsuario(ModoForm modo, int? idUsuario, IUsuarioService svcUsuarios, IRolService svcRoles)
            : this()
        {
            _modo = modo;
            _idUsuario = idUsuario;
            _svcUsuarios = svcUsuarios;
            _svcRoles = svcRoles;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Ejemplos de uso:
            if (_idUsuario.HasValue)
            {
                var u = _svcUsuarios.ObtenerPorId(_idUsuario.Value);
                // TODO: mapear u -> controles del alta/edición
            }

            if (_modo == ModoForm.Ver)
            {
                // TODO: deshabilitar campos/botón Guardar
            }
        }
    }
}
