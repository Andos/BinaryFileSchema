using Peg.Base;

namespace BFSchema
{
	public sealed class BfsCompiler
	{
		static bool gotError;
		static IBfsErrorHandler handler;

		public static BinaryFileSchema CheckBfs(BinaryFileSchema schema, IBfsErrorHandler errorHandler)
		{
			handler = errorHandler;

			IPhase[] phases = new IPhase[] {
                new Environments(),
                new TypeLinking(),
                new Hierarchy(),
                new TypeChecking(),
                new DefiniteAssignment()
            };

			gotError = false;

			foreach (IPhase phase in phases)
			{
				phase.Check(schema);

				if (gotError && errorHandler != null)
				{
					errorHandler.HandleMessage("Schema has errors. Compilation stopped.");
					return null;
				}
			}

			return schema;
		}

        public static BinaryFileSchema ParseBfs(string source, IBfsErrorHandler errorHandler)
        {
            return ParseBfs(new BinaryFileSchema(), source, errorHandler);
        }

		public static BinaryFileSchema ParseBfs(BinaryFileSchema schema, string source, IBfsErrorHandler errorHandler)
		{
			gotError = false;
			handler = errorHandler;
			BinaryFileSchemaParser.BinaryFileSchemaParser parser = new BinaryFileSchemaParser.BinaryFileSchemaParser();
			parser.Construct(source, new StreamErrorHandler(errorHandler) );
			bool matches = false;
			try
			{
				matches = parser.bfschema();
			}
			catch (PegException ex)
			{
				errorHandler.HandleMessage(ex.Message);
			}

			if (!matches)
			{
				ReportMessage("Schema didn't parse.");
				return null;
			}

			AstConvert converter = new AstConvert(schema,source);
			schema = converter.GetBFSTree(parser.GetRoot());
			schema = CheckBfs(schema,errorHandler);

			return schema;
		}

		public static void ReportMessage(string message)
		{
			if (handler != null)
				handler.HandleMessage(message);
		}

		public static void ReportError( BfsSourceRange range, string message )
		{
			gotError = true;
			if (handler != null)
				handler.HandleError(new SourceError(range, "Error: " + message));
		}
		public static void ReportError(string message)
		{
			gotError = true;
			if (handler != null)
				handler.HandleError(new SourceError(new BfsSourceRange(), "Error: " + message));
		}

		public static void ReportWarning( BfsSourceRange range, string message )
		{
			if (handler != null)
				handler.HandleWarning(new SourceError(range, "Warning: " + message));
		}
		public static void ReportWarning(string message)
		{
			if (handler != null)
				handler.HandleWarning(new SourceError(new BfsSourceRange(), "Warning: " + message));
		}

		private BfsCompiler() { }
	}

	public interface IPhase
	{
		void Check(BinaryFileSchema schema);
	}

	public interface CodeGenerator
	{
		string GenerateCode(BinaryFileSchema schema);
	}


}
