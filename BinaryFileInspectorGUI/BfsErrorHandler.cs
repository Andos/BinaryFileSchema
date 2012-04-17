using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BFSchema;

namespace BinaryFileInspectorGUI
{
    class BfsErrorHandler : IBfsErrorHandler
    {
        ListView list;

        public BfsErrorHandler(ListView listview)
        {
            list = listview;
        }

        public void HandleError(SourceError error)
        {
            list.Items.Add(new ListViewItem(error.Message));
        }

        public void HandleWarning(SourceError error)
        {
            list.Items.Add(new ListViewItem(error.Message));
        }

        public void HandleMessage(SourceError error)
        {
            list.Items.Add(new ListViewItem(error.Message));
        }

        public void HandleMessage(string message)
        {
            list.Items.Add(new ListViewItem(message));
        }

        public void HandleError(string message)
        {
            list.Items.Add(new ListViewItem(message));
        }
    }
}
