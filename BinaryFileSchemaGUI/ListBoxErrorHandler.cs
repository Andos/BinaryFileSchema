using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using BFSchema;

namespace BinaryFileSchemaGUI
{
	public class ListBoxErrorHandler : IBfsErrorHandler
	{
		ListBox box;
		public ListBoxErrorHandler(ListBox listbox)
		{
			box = listbox;
		}

		public void HandleMessage(string message)
		{
			box.Items.Add(message);
		}

		public void HandleWarning(SourceError warning)
		{
			box.Items.Add(warning);
		}

		public void HandleError(SourceError error)
		{
			box.Items.Add(error);
		}

		public void HandleError(string error)
		{
			box.Items.Add(error);
		}

	}
}
