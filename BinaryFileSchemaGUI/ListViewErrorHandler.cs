using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using BFSchema;

namespace BinaryFileSchemaGUI
{
	public class ListViewErrorHandler : IBfsErrorHandler
	{
		ListView box;
		public ListViewErrorHandler(ListView listview)
		{
			box = listview;
		}

		public void HandleMessage(string message)
		{
			box.Items.Add(message, 0);
		}

		public void HandleWarning(SourceError warning)
		{
			ListViewItem item = box.Items.Add(warning.Message, 1);
			item.Tag = warning;
		}

		public void HandleError(SourceError error)
		{
			ListViewItem item = box.Items.Add(error.Message, 2);
			item.Tag = error;
		}

		public void HandleError(string error)
		{
			ListViewItem item = box.Items.Add(error, 2);
		}
	}
}
