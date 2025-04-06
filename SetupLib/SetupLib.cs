using System.Collections;
using System.ComponentModel;
using FBlockEXE;

namespace SetupLib
{
    [RunInstaller(true)]
    public partial class SetupLib : System.Configuration.Install.Installer
    {
        public SetupLib()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            ContextMenuManager.Register();
            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            ContextMenuManager.Unregister();
            base.Uninstall(savedState);
        }
    }
}
