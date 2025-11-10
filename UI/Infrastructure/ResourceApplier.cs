using System.ComponentModel;
using System.Windows.Forms;

namespace UI.Infrastructure
{
    public static class ResourceApplier
    {
        public static void ApplyTo(Control root)
        {
            var mgr = new ComponentResourceManager(root.GetType());
            ApplyRecursive(root, mgr);
            mgr.ApplyResources(root, "$this");
        }

        private static void ApplyRecursive(Control ctl, ComponentResourceManager mgr)
        {
            foreach (Control child in ctl.Controls)
            {
                mgr.ApplyResources(child, child.Name);
                ApplyRecursive(child, mgr);
            }

            if (ctl is MenuStrip ms)
                foreach (ToolStripItem it in ms.Items)
                    ApplyToolItem(it, mgr);
        }

        private static void ApplyToolItem(ToolStripItem it, ComponentResourceManager mgr)
        {
            mgr.ApplyResources(it, it.Name);
            if (it is ToolStripDropDownItem dd)
                foreach (ToolStripItem sub in dd.DropDownItems)
                    ApplyToolItem(sub, mgr);
        }
    }
}
