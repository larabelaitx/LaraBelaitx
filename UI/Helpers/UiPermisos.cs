using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BLL.Contracts;

namespace UI.Helpers
{
    public static class UiPermisos
    {
        // Muestra/oculta controles según patente(s). Si requiere todas, usa requireAll=true
        public static void BindVisibility(IRolService roles, int idUsuario, Control ctrl, params string[] codigos)
        {
            bool ok = roles.TieneAlguna(idUsuario, codigos);
            ctrl.Visible = ok;
        }

        // Habilita/deshabilita (lo deja visible pero desactivado)
        public static void BindEnabled(IRolService roles, int idUsuario, Control ctrl, params string[] codigos)
        {
            bool ok = roles.TieneAlguna(idUsuario, codigos);
            ctrl.Enabled = ok;
        }

        // Variante “todas”
        public static void BindEnabledAll(IRolService roles, int idUsuario, Control ctrl, params string[] codigos)
        {
            bool ok = roles.TieneTodas(idUsuario, codigos);
            ctrl.Enabled = ok;
        }
    }
}