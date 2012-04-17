using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using BFSchema;
using System.Globalization;

namespace BFSchema
{
	public interface IBfsErrorHandler
	{
		void HandleMessage(string message);
		void HandleError(string message);
		void HandleError(SourceError errorMessage);
		void HandleWarning(SourceError warningMessage);
	}

	public class SourceError
	{
		public string Message { get; set; }
		public BfsSourceRange SourceRange { get; set; }

		public SourceError(BfsSourceRange range, string message )
		{
			Message = message;
			SourceRange = range;
		}

		public override string ToString()
		{
			return Message;
		}
	}

	public class StreamErrorHandler : TextWriter
	{
		IBfsErrorHandler errorhandler;

		public StreamErrorHandler(IBfsErrorHandler errorhandler) : base(CultureInfo.InvariantCulture)
		{
			this.errorhandler = errorhandler;
		}

		public override Encoding Encoding
		{
			get { return Encoding.Default; }
		}

		public override void Write(string value)
		{
			errorhandler.HandleError(value);
		}

		public override void WriteLine(string value)
		{
			errorhandler.HandleError(value);
		}
	}

    public class ConsoleErrorHandler : IBfsErrorHandler
    {
        public bool GotError { get; set; }
        public void HandleMessage(string message)
        {
            Console.WriteLine("Message: " + message);
        }

        public void HandleError(string message)
        {
            Console.WriteLine("Error: " + message);
            GotError = true;
        }

        public void HandleError(SourceError errorMessage)
        {
            Console.WriteLine("<" + errorMessage.SourceRange.Begin + "> Error: " + errorMessage.Message);
            GotError = true;
        }

        public void HandleWarning(SourceError warningMessage)
        {
            Console.WriteLine("<" + warningMessage.SourceRange.Begin + "> Message: " + warningMessage.Message);
        }
    }

}
