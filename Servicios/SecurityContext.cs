using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public static class SecurityContext
    {
        public static BE.Usuario CurrentUser { get; set; }
        public static int CurrentUserId => CurrentUser?.Id ?? 0;

        public static void Clear()
        {
            CurrentUser = null;
        }
    }
}