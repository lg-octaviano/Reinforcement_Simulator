using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Simulador_Final
{

    public class PainelArgs : EventArgs
    {
        public StackPanel painel;
        public Control c;
        public int id;

        public PainelArgs(StackPanel p, Control c, int id)
        {
            this.painel = p;
            this.c = c;
            this.id = id;
        }

    }
}
