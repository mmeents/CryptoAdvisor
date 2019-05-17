//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Data Access Application Block
//===============================================================================
// Copyright © Microsoft Corporation. All rights reserved.
// Adapted from ACA.NET with permission from Avanade Inc.
// ACA.NET copyright © Avanade Inc. All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using C0DEC0RE.Properties;

namespace C0DEC0RE
{
	/// <summary>
	/// Represents an abstract database that commands can be run against. 
	/// </summary>
	/// <remarks>
	/// The <see cref="Database"/> class leverages the provider factory model from ADO.NET. A database instance holds 
	/// a reference to a concrete <see cref="DbProviderFactory"/> object to which it forwards the creation of ADO.NET objects.
	/// </remarks>
	[ConfigurationNameMapper(typeof(DatabaseMapper))]
	[CustomFactory(typeof(DatabaseCustomFactory))]
	public abstract class Database : IInstrumentationEventProvider
	{
		private static readonly string VALID_USER_ID_TOKENS = Resources.UserName;
		private static readonly string VALID_PASSWORD_TOKENS = Resources.Password;

		private static ParameterCache parameterCache = new ParameterCache();

		private ConnectionString connectionString;
		private DbProviderFactory dbProviderFactory;

		/// <summary>
		/// The <see cref="DataInstrumentationProvider"/> instance that defines the logical events used to instrument this <see cref="Database"/> instance.
		/// </summary>
		protected DataInstrumentationProvider instrumentationProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="Database"/> class with a connection string and a <see cref="DbProviderFactory"/>.
		/// </summary>
		/// <param name="connectionString">The connection string for the database.</param>
		/// <param name="dbProviderFactory">A <see cref="DbProviderFactory"/> object.</param>
		protected Database(string connectionString, DbProviderFactory dbProviderFactory)
		{
			if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, "connectionString");
			if (dbProviderFactory == null) throw new ArgumentNullException("dbProviderFactory");

			this.connectionString = new ConnectionString(connectionString, VALID_USER_ID_TOKENS, VALID_PASSWORD_TOKENS);
			this.dbProviderFactory = dbProviderFactory;
			this.instrumentationProvider = new DataInstrumentationProvider();
		}

		/// <summary>
		/// <para>Gets the string used to open a database.</para>
		/// </summary>
		/// <value>
		/// <para>The string used to open a database.</para>
		/// </value>
		/// <seealso cref="DbConnection.ConnectionString"/>
		protected internal string ConnectionString
		{
			get
			{
				return this.connectionString.ToString();
			}
		}

		/// <summary>
		/// <para>Gets the DbProviderFactory used by the database instance.</para>
		/// </summary>
		/// <seealso cref="DbProviderFactory"/>
		public DbProviderFactory DbProviderFactory
		{
			get { return this.dbProviderFactory; }
		}

		/// <summary>
		/// <para>Gets the connection string without the username and password.</para>
		/// </summary>
		/// <value>
		/// <para>The connection string without the username and password.</para>
		/// </value>
		/// <seealso cref="ConnectionString"/>
		protected string ConnectionStringNoCredentials
		{
			get
			{
				return connectionString.ToStringNoCredentials();
			}
		}

		/// <summary>
		/// Gets the connection string without credentials.
		/// </summary>
		/// <value>
		/// The connection string without credentials.
		/// </value>
		public string ConnectionStringWithoutCredentials
		{
			get
			{
				return ConnectionStringNoCredentials;
			}
		}

		/// <summary>
		/// <para>Creates a connection for this database.</para>
		/// </summary>
		/// <returns>
		/// <para>The <see cref="DbConnection"/> for this database.</para>
		/// </returns>
		/// <seealso cref="DbConnection"/>        
		public DbConnection CreateConnection()
		{
			DbConnection newConnection = dbProviderFactory.CreateConnection();
			newConnection.ConnectionString = ConnectionString;

			return newConnection;
		}

		/// <summary>
		/// Returns the object to which the instrumentation events have been delegated.
		/// </summary>
		/// <returns>Object to which the instrumentation events have been delegated.</returns>
		public object GetInstrumentationEventProvider()
		{
			return instrumentationProvider;
		}

		/// <summary>
		/// <para>Creates a <see cref="DbCommand"/> for a stored procedure.</para>
		/// </summary>
		/// <param name="storedProcedureName"><para>The name of the stored procedure.</para></param>
		/// <returns><para>The <see cref="DbCommand"/> for the stored procedure.</para></returns>       
		public virtual DbCommand GetStoredProcCommand(string storedProcedureName)
		{
			if (string.IsNullOrEmpty(storedProcedureName)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, "storedProcedureName");

			return CreateCommandByCommandType(CommandType.StoredProcedure, storedProcedureName);
		}

		/// <summary>
		/// <para>Creates a <see cref="DbCommand"/> for a stored procedure.</para>
		/// </summary>
		/// <param name="storedProcedureName"><para>The name of the stored procedure.</para></param>
		/// <param name="parameterValues"><para>The list of parameters for the procedure.</para></param>
		/// <returns><para>The <see cref="DbCommand"/> for the stored procedure.</para></returns>
		/// <remarks>
		/// <para>The parameters for the stored procedure will be discovered and the values are assigned in positional order.</para>
		/// </remarks>        
		public virtual DbCommand GetStoredProcCommand(string storedProcedureName, params object[] parameterValues)
		{
			if (string.IsNullOrEmpty(storedProcedureName)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, "storedProcedureName");

			DbCommand command = CreateCommandByCommandType(CommandType.StoredProcedure, storedProcedureName);

			parameterCache.SetParameters(command, this);

			if (SameNumberOfParametersAndValues(command, parameterValues) == false)
			{
				throw new InvalidOperationException(Resources.ExceptionMessageParameterMatchFailure);
			}


			AssignParameterValues(command, parameterValues);
			return command;
		}

		/// <summary>
		/// Wraps around a derived class's implementation of the GetStoredProcCommandWrapper method and adds functionality for
		/// using this method with UpdateDataSet.  The GetStoredProcCommandWrapper method (above) that takes a params array 
		/// expects the array to be filled with VALUES for the parameters. This method differs from the GetStoredProcCommandWrapper 
		/// method in that it allows a user to pass in a string array. It will also dynamically discover the parameters for the 
		/// stored procedure and set the parameter's SourceColumns to the strings that are passed in. It does this by mapping 
		/// the parameters to the strings IN ORDER. Thus, order is very important.
		/// </summary>
		/// <param name="storedProcedureName"><para>The name of the stored procedure.</para></param>
		/// <param name="sourceColumns"><para>The list of DataFields for the procedure.</para></param>
		/// <returns><para>The <see cref="DbCommand"/> for the stored procedure.</para></returns>
		public DbCommand GetStoredProcCommandWithSourceColumns(string storedProcedureName, params string[] sourceColumns)
		{
			if (string.IsNullOrEmpty(storedProcedureName)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, "storedProcedureName");
			if (sourceColumns == null) throw new ArgumentNullException("sourceColumns");

			DbCommand dbCommand = GetStoredProcCommand(storedProcedureName);

			//we do not actually set the connection until the Fill or Update, so we need to temporarily do it here so we can set the sourcecolumns
			using (DbConnection connection = CreateConnection())
			{
				dbCommand.Connection = connection;
				DiscoverParameters(dbCommand);
			}

			int iSourceIndex = 0;
			foreach (IDataParameter dbParam in dbCommand.Parameters)
			{
				if ((dbParam.Direction == ParameterDirection.Input) | (dbParam.Direction == ParameterDirection.InputOutput))
				{
					dbParam.SourceColumn = sourceColumns[iSourceIndex];
					iSourceIndex++;
				}
			}

			return dbCommand;
		}

		/// <summary>
		/// <para>Creates a <see cref="DbCommand"/> for a SQL query.</para>
		/// </summary>
		/// <param name="query"><para>The text of the query.</para></param>        
		/// <returns><para>The <see cref="DbCommand"/> for the SQL query.</para></returns>        
		public DbCommand GetSqlStringCommand(string query)
		{
			if (string.IsNullOrEmpty(query)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, "query");

			return CreateCommandByCommandType(CommandType.Text, query);
		}


		/// <summary>
		/// Gets a DbDataAdapter with Standard update behavior.
		/// </summary>
		/// <returns>A <see cref="DbDataAdapter"/>.</returns>
		/// <seealso cref="DbDataAdapter"/>
		/// <devdoc>
		/// Created this new, public method instead of modifying the protected, abstract one so that there will be no
		/// breaking changes for any currently derived Database class.
		/// </devdoc>
		public DbDataAdapter GetDataAdapter()
		{
			return GetDataAdapter(UpdateBehavior.Standard);
		}

		/// <summary>
		/// Gets the DbDataAdapter with the given update behavior and connection from the proper derived class.
		/// </summary>
		/// <param name="updateBehavior">
		/// <para>One of the <see cref="UpdateBehavior"/> values.</para>
		/// </param>        
		/// <returns>A <see cref="DbDataAdapter"/>.</returns>
		/// <seealso cref="DbDataAdapter"/>
		protected DbDataAdapter GetDataAdapter(UpdateBehavior updateBehavior)
		{
			DbDataAdapter adapter = dbProviderFactory.CreateDataAdapter();

			if (updateBehavior == UpdateBehavior.Continue)
			{
				this.SetUpRowUpdatedEvent(adapter);
			}
			return adapter;
		}

		/// <summary>
		/// Sets the RowUpdated event for the data adapter.
		/// </summary>
		/// <param name="adapter">The <see cref="DbDataAdapter"/> to set the event.</param>
		protected virtual void SetUpRowUpdatedEvent(DbDataAdapter adapter)
		{
		}

		/// <summary>
		/// <para>Executes the <paramref name="command"/> and adds a new <see cref="DataTable"></see> to the existing <see cref="DataSet"></see>.</para>
		/// </summary>
		/// <param name="command">
		/// <para>The <see cref="DbCommand"/> to execute.</para>
		/// </param>
		/// <param name="dataSet">
		/// <para>The <see cref="DataSet"/> to load.</para>
		/// </param>
		/// <param name="tableName">
		/// <para>The name for the new <see cref="DataTable"/> to add to the <see cref="DataSet"/>.</para>
		/// </param>        
		/// <exception cref="System.ArgumentNullException">Any input parameter was <see langword="null"/> (<b>Nothing</b> in Visual Basic)</exception>
		/// <exception cref="System.ArgumentException">tableName was an empty string</exception>
		public virtual void LoadDataSet(DbCommand command, DataSet dataSet, string tableName)
		{
			LoadDataSet(command, dataSet, new string[] { tableName });
		}

		/// <summary>
		/// <para>Executes the <paramref name="command"/> within the given <paramref name="transaction" /> and adds a new <see cref="DataTable"></see> to the existing <see cref="DataSet"></see>.</para>
		/// </summary>
		/// <param name="command">
		/// <para>The <see cref="DbCommand"/> to execute.</para>
		/// </param>
		/// <param name="dataSet">
		/// <para>The <see cref="DataSet"/> to load.</para>
		/// </param>
		/// <param name="tableName">
		/// <para>The name for the new <see cref="DataTable"/> to add to the <see cref="DataSet"/>.</para>
		/// </param>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>        
		/// <exception cref="System.ArgumentNullException">Any input parameter was <see langword="null"/> (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="System.ArgumentException">tableName was an empty string.</exception>
		public virtual void LoadDataSet(DbCommand command, DataSet dataSet, string tableName, DbTransaction transaction)
		{
			LoadDataSet(command, dataSet, new string[] { tableName }, transaction);
		}

		/// <summary>
		/// <para>Loads a <see cref="DataSet"/> from a <see cref="DbCommand"/>.</para>
		/// </summary>
		/// <param name="command">
		/// <para>The command to execute to fill the <see cref="DataSet"/>.</para>
		/// </param>
		/// <param name="dataSet">
		/// <para>The <see cref="DataSet"/> to fill.</para>
		/// </param>
		/// <param name="tableNames">
		/// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
		/// </param>
		public virtual void LoadDataSet(DbCommand command, DataSet dataSet, string[] tableNames)
		{
			using (DbConnection connection = CreateConnection())
			{
				PrepareCommand(command, connection);
				DoLoadDataSet(command, dataSet, tableNames);
			}
		}

		/// <summary>
		/// <para>Loads a <see cref="DataSet"/> from a <see cref="DbCommand"/> in  a transaction.</para>
		/// </summary>
		/// <param name="command">
		/// <para>The command to execute to fill the <see cref="DataSet"/>.</para>
		/// </param>
		/// <param name="dataSet">
		/// <para>The <see cref="DataSet"/> to fill.</para>
		/// </param>
		/// <param name="tableNames">
		/// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
		/// </param>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command in.</para>
		/// </param>
		public virtual void LoadDataSet(DbCommand command, DataSet dataSet, string[] tableNames, DbTransaction transaction)
		{
			PrepareCommand(command, transaction);
			DoLoadDataSet(command, dataSet, tableNames);
		}

		/// <summary>
		/// <para>Loads a <see cref="DataSet"/> with the results returned from a stored procedure.</para>
		/// </summary>
		/// <param name="storedProcedureName">
		/// <para>The stored procedure name to execute.</para>
		/// </param>
		/// <param name="dataSet">
		/// <para>The <see cref="DataSet"/> to fill.</para>
		/// </param>
		/// <param name="tableNames">
		/// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
		/// </param>
		/// <param name="parameterValues">
		/// <para>An array of paramters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
		/// </param>
		public virtual void LoadDataSet(string storedProcedureName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
		{
			using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
			{
				LoadDataSet(command, dataSet, tableNames);
			}
		}

		/// <summary>
		/// <para>Loads a <see cref="DataSet"/> with the results returned from a stored procedure executed in a transaction.</para>
		/// </summary>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the stored procedure in.</para>
		/// </param>
		/// <param name="storedProcedureName">
		/// <para>The stored procedure name to execute.</para>
		/// </param>
		/// <param name="dataSet">
		/// <para>The <see cref="DataSet"/> to fill.</para>
		/// </param>
		/// <param name="tableNames">
		/// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
		/// </param>
		/// <param name="parameterValues">
		/// <para>An array of paramters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
		/// </param>
		public virtual void LoadDataSet(DbTransaction transaction, string storedProcedureName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
		{
			using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
			{
				LoadDataSet(command, dataSet, tableNames, transaction);
			}
		}

		/// <summary>
		/// <para>Loads a <see cref="DataSet"/> from command text.</para>
		/// </summary>
		/// <param name="commandType">
		/// <para>One of the <see cref="CommandType"/> values.</para>
		/// </param>
		/// <param name="commandText">
		/// <para>The command text to execute.</para>
		/// </param>
		/// <param name="dataSet">
		/// <para>The <see cref="DataSet"/> to fill.</para>
		/// </param>
		/// <param name="tableNames">
		/// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
		/// </param>
		public virtual void LoadDataSet(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
		{
			using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
			{
				LoadDataSet(command, dataSet, tableNames);
			}
		}

		/// <summary>
		/// <para>Loads a <see cref="DataSet"/> from command text in a transaction.</para>
		/// </summary>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command in.</para>
		/// </param>
		/// <param name="commandType">
		/// <para>One of the <see cref="CommandType"/> values.</para>
		/// </param>
		/// <param name="commandText">
		/// <para>The command text to execute.</para>
		/// </param>
		/// <param name="dataSet">
		/// <para>The <see cref="DataSet"/> to fill.</para>
		/// </param>
		/// <param name="tableNames">
		/// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
		/// </param>
		public void LoadDataSet(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
		{
			using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
			{
				LoadDataSet(command, dataSet, tableNames, transaction);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="command"/> and returns the results in a new <see cref="DataSet"/>.</para>
		/// </summary>
		/// <param name="command"><para>The <see cref="DbCommand"/> to execute.</para></param>
		/// <returns>A <see cref="DataSet"/> with the results of the <paramref name="command"/>.</returns>        
		public virtual DataSet ExecuteDataSet(DbCommand command)
		{
			DataSet dataSet = new DataSet();
			dataSet.Locale = CultureInfo.InvariantCulture;
			LoadDataSet(command, dataSet, "Table");
			return dataSet;
		}

		/// <summary>
		/// <para>Executes the <paramref name="command"/> as part of the <paramref name="transaction" /> and returns the results in a new <see cref="DataSet"/>.</para>
		/// </summary>
		/// <param name="command"><para>The <see cref="DbCommand"/> to execute.</para></param>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <returns>A <see cref="DataSet"/> with the results of the <paramref name="command"/>.</returns>        
		public virtual DataSet ExecuteDataSet(DbCommand command, DbTransaction transaction)
		{
			DataSet dataSet = new DataSet();
			dataSet.Locale = CultureInfo.InvariantCulture;
			LoadDataSet(command, dataSet, "Table", transaction);
			return dataSet;
		}

		/// <summary>
		/// <para>Executes the <paramref name="storedProcedureName"/> with <paramref name="parameterValues" /> and returns the results in a new <see cref="DataSet"/>.</para>
		/// </summary>
		/// <param name="storedProcedureName">
		/// <para>The stored procedure to execute.</para>
		/// </param>
		/// <param name="parameterValues">
		/// <para>An array of paramters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
		/// </param>
		/// <returns>
		/// <para>A <see cref="DataSet"/> with the results of the <paramref name="storedProcedureName"/>.</para>
		/// </returns>
		public virtual DataSet ExecuteDataSet(string storedProcedureName, params object[] parameterValues)
		{
			using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
			{
				return ExecuteDataSet(command);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="storedProcedureName"/> with <paramref name="parameterValues" /> as part of the <paramref name="transaction" /> and returns the results in a new <see cref="DataSet"/> within a transaction.</para>
		/// </summary>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <param name="storedProcedureName">
		/// <para>The stored procedure to execute.</para>
		/// </param>
		/// <param name="parameterValues">
		/// <para>An array of paramters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
		/// </param>
		/// <returns>
		/// <para>A <see cref="DataSet"/> with the results of the <paramref name="storedProcedureName"/>.</para>
		/// </returns>
		public virtual DataSet ExecuteDataSet(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
		{
			using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
			{
				return ExecuteDataSet(command, transaction);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> and returns the results in a new <see cref="DataSet"/>.</para>
		/// </summary>
		/// <param name="commandType">
		/// <para>One of the <see cref="CommandType"/> values.</para>
		/// </param>
		/// <param name="commandText">
		/// <para>The command text to execute.</para>
		/// </param>
		/// <returns>
		/// <para>A <see cref="DataSet"/> with the results of the <paramref name="commandText"/>.</para>
		/// </returns>
		public virtual DataSet ExecuteDataSet(CommandType commandType, string commandText)
		{
			using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
			{
				return ExecuteDataSet(command);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="commandText"/> as part of the given <paramref name="transaction" /> and returns the results in a new <see cref="DataSet"/>.</para>
		/// </summary>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <param name="commandType">
		/// <para>One of the <see cref="CommandType"/> values.</para>
		/// </param>
		/// <param name="commandText">
		/// <para>The command text to execute.</para>
		/// </param>
		/// <returns>
		/// <para>A <see cref="DataSet"/> with the results of the <paramref name="commandText"/>.</para>
		/// </returns>
		public virtual DataSet ExecuteDataSet(DbTransaction transaction, CommandType commandType, string commandText)
		{
			using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
			{
				return ExecuteDataSet(command, transaction);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="command"/> and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.</para>
		/// </summary>
		/// <param name="command">
		/// <para>The command that contains the query to execute.</para>
		/// </param>
		/// <returns>
		/// <para>The first column of the first row in the result set.</para>
		/// </returns>
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual object ExecuteScalar(DbCommand command)
		{
			if (command == null) throw new ArgumentNullException("command");

			using (DbConnection connection = OpenConnection())
			{
				PrepareCommand(command, connection);
				return DoExecuteScalar(command);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="command"/> within a <paramref name="transaction" />, and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.</para>
		/// </summary>
		/// <param name="command">
		/// <para>The command that contains the query to execute.</para>
		/// </param>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <returns>
		/// <para>The first column of the first row in the result set.</para>
		/// </returns>
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual object ExecuteScalar(DbCommand command, DbTransaction transaction)
		{
			PrepareCommand(command, transaction);
			return DoExecuteScalar(command);
		}

		/// <summary>
		/// <para>Executes the <paramref name="storedProcedureName"/> with the given <paramref name="parameterValues" /> and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.</para>
		/// </summary>
		/// <param name="storedProcedureName">
		/// <para>The stored procedure to execute.</para>
		/// </param>
		/// <param name="parameterValues">
		/// <para>An array of paramters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
		/// </param>
		/// <returns>
		/// <para>The first column of the first row in the result set.</para>
		/// </returns>
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual object ExecuteScalar(string storedProcedureName, params object[] parameterValues)
		{
			using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
			{
				return ExecuteScalar(command);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="storedProcedureName"/> with the given <paramref name="parameterValues" /> within a 
		/// <paramref name="transaction" /> and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.</para>
		/// </summary>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <param name="storedProcedureName">
		/// <para>The stored procedure to execute.</para>
		/// </param>
		/// <param name="parameterValues">
		/// <para>An array of paramters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
		/// </param>
		/// <returns>
		/// <para>The first column of the first row in the result set.</para>
		/// </returns>
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual object ExecuteScalar(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
		{
			using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
			{
				return ExecuteScalar(command, transaction);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" />  and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.</para>
		/// </summary>
		/// <param name="commandType">
		/// <para>One of the <see cref="CommandType"/> values.</para>
		/// </param>
		/// <param name="commandText">
		/// <para>The command text to execute.</para>
		/// </param>
		/// <returns>
		/// <para>The first column of the first row in the result set.</para>
		/// </returns>
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual object ExecuteScalar(CommandType commandType, string commandText)
		{
			using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
			{
				return ExecuteScalar(command);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> 
		/// within the given <paramref name="transaction" /> and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.</para>
		/// </summary>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <param name="commandType">
		/// <para>One of the <see cref="CommandType"/> values.</para>
		/// </param>
		/// <param name="commandText">
		/// <para>The command text to execute.</para>
		/// </param>
		/// <returns>
		/// <para>The first column of the first row in the result set.</para>
		/// </returns>
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText)
		{
			using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
			{
				return ExecuteScalar(command, transaction);
			}
		}


		/// <summary>
		/// <para>Executes the <paramref name="command"/> and returns the number of rows affected.</para>
		/// </summary>
		/// <param name="command">
		/// <para>The command that contains the query to execute.</para>
		/// </param>       
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual int ExecuteNonQuery(DbCommand command)
		{
			using (DbConnection connection = OpenConnection())
			{
				PrepareCommand(command, connection);
				return DoExecuteNonQuery(command);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="command"/> within the given <paramref name="transaction" />, and returns the number of rows affected.</para>
		/// </summary>
		/// <param name="command">
		/// <para>The command that contains the query to execute.</para>
		/// </param>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual int ExecuteNonQuery(DbCommand command, DbTransaction transaction)
		{
			PrepareCommand(command, transaction);
			return DoExecuteNonQuery(command);
		}

		/// <summary>
		/// <para>Executes the <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> and returns the number of rows affected.</para>
		/// </summary>
		/// <param name="storedProcedureName">
		/// <para>The name of the stored procedure to execute.</para>
		/// </param>
		/// <param name="parameterValues">
		/// <para>An array of paramters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
		/// </param>
		/// <returns>
		/// <para>The number of rows affected</para>
		/// </returns>
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual int ExecuteNonQuery(string storedProcedureName, params object[] parameterValues)
		{
			using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
			{
				return ExecuteNonQuery(command);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> within a transaction and returns the number of rows affected.</para>
		/// </summary>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <param name="storedProcedureName">
		/// <para>The name of the stored procedure to execute.</para>
		/// </param>
		/// <param name="parameterValues">
		/// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
		/// </param>
		/// <returns>
		/// <para>The number of rows affected.</para>
		/// </returns>
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual int ExecuteNonQuery(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
		{
			using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
			{
				return ExecuteNonQuery(command, transaction);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> and returns the number of rows affected.</para>
		/// </summary>
		/// <param name="commandType">
		/// <para>One of the <see cref="CommandType"/> values.</para>
		/// </param>
		/// <param name="commandText">
		/// <para>The command text to execute.</para>
		/// </param>
		/// <returns>
		/// <para>The number of rows affected.</para>
		/// </returns>
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual int ExecuteNonQuery(CommandType commandType, string commandText)
		{
			using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
			{
				return ExecuteNonQuery(command);
			}
		}


		/// <summary>
		/// <para>Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> as part of the given <paramref name="transaction" /> and returns the number of rows affected.</para>
		/// </summary>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <param name="commandType">
		/// <para>One of the <see cref="CommandType"/> values.</para>
		/// </param>
		/// <param name="commandText">
		/// <para>The command text to execute.</para>
		/// </param>
		/// <returns>
		/// <para>The number of rows affected</para>
		/// </returns>
		/// <seealso cref="IDbCommand.ExecuteScalar"/>
		public virtual int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText)
		{
			using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
			{
				return ExecuteNonQuery(command, transaction);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="command"/> and returns an <see cref="IDataReader"></see> through which the result can be read.
		/// It is the responsibility of the caller to close the connection and reader when finished.</para>
		/// </summary>
		/// <param name="command">
		/// <para>The command that contains the query to execute.</para>
		/// </param>
		/// <returns>
		/// <para>An <see cref="IDataReader"/> object.</para>
		/// </returns>        
		public virtual IDataReader ExecuteReader(DbCommand command)
		{
			DbConnection connection = OpenConnection();
			PrepareCommand(command, connection);

			try
			{
				return DoExecuteReader(command, CommandBehavior.CloseConnection);
			}
			catch
			{
				connection.Close();
				throw;
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="command"/> within a transaction and returns an <see cref="IDataReader"></see> through which the result can be read.
		/// It is the responsibility of the caller to close the connection and reader when finished.</para>
		/// </summary>
		/// <param name="command">
		/// <para>The command that contains the query to execute.</para>
		/// </param>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <returns>
		/// <para>An <see cref="IDataReader"/> object.</para>
		/// </returns>        
		public virtual IDataReader ExecuteReader(DbCommand command, DbTransaction transaction)
		{
			PrepareCommand(command, transaction);
			return DoExecuteReader(command, CommandBehavior.Default);
		}

		/// <summary>
		/// <para>Executes the <paramref name="storedProcedureName"/> with the given <paramref name="parameterValues" /> and returns an <see cref="IDataReader"></see> through which the result can be read.
		/// It is the responsibility of the caller to close the connection and reader when finished.</para>
		/// </summary>        
		/// <param name="storedProcedureName">
		/// <para>The command that contains the query to execute.</para>
		/// </param>
		/// <param name="parameterValues">
		/// <para>An array of paramters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
		/// </param>
		/// <returns>
		/// <para>An <see cref="IDataReader"/> object.</para>
		/// </returns>        
		public IDataReader ExecuteReader(string storedProcedureName, params object[] parameterValues)
		{
			using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
			{
				return ExecuteReader(command);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="storedProcedureName"/> with the given <paramref name="parameterValues" /> within the given <paramref name="transaction" /> and returns an <see cref="IDataReader"></see> through which the result can be read.
		/// It is the responsibility of the caller to close the connection and reader when finished.</para>
		/// </summary>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <param name="storedProcedureName">
		/// <para>The command that contains the query to execute.</para>
		/// </param>
		/// <param name="parameterValues">
		/// <para>An array of paramters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
		/// </param>
		/// <returns>
		/// <para>An <see cref="IDataReader"/> object.</para>
		/// </returns>        
		public IDataReader ExecuteReader(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
		{
			using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
			{
				return ExecuteReader(command, transaction);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> and returns an <see cref="IDataReader"></see> through which the result can be read.
		/// It is the responsibility of the caller to close the connection and reader when finished.</para>
		/// </summary>
		/// <param name="commandType">
		/// <para>One of the <see cref="CommandType"/> values.</para>
		/// </param>
		/// <param name="commandText">
		/// <para>The command text to execute.</para>
		/// </param>
		/// <returns>
		/// <para>An <see cref="IDataReader"/> object.</para>
		/// </returns>        
		public IDataReader ExecuteReader(CommandType commandType, string commandText)
		{
			using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
			{
				return ExecuteReader(command);
			}
		}

		/// <summary>
		/// <para>Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> within the given 
		/// <paramref name="transaction" /> and returns an <see cref="IDataReader"></see> through which the result can be read.
		/// It is the responsibility of the caller to close the connection and reader when finished.</para>
		/// </summary>
		/// <param name="transaction">
		/// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
		/// </param>
		/// <param name="commandType">
		/// <para>One of the <see cref="CommandType"/> values.</para>
		/// </param>
		/// <param name="commandText">
		/// <para>The command text to execute.</para>
		/// </param>
		/// <returns>
		/// <para>An <see cref="IDataReader"/> object.</para>
		/// </returns>        
		public IDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText)
		{
			using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
			{
				return ExecuteReader(command, transaction);
			}
		}

		/// <summary>
		/// <para>Calls the respective INSERT, UPDATE, or DELETE statements for each inserted, updated, or deleted row in the <see cref="DataSet"/>.</para>
		/// </summary>        
		/// <param name="dataSet"><para>The <see cref="DataSet"/> used to update the data source.</para></param>
		/// <param name="tableName"><para>The name of the source table to use for table mapping.</para></param>
		/// <param name="insertCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <seealso cref="DataRowState.Added"/></para></param>
		/// <param name="updateCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <seealso cref="DataRowState.Modified"/></para></param>        
		/// <param name="deleteCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <seealso cref="DataRowState.Deleted"/></para></param>        
		/// <param name="updateBehavior"><para>One of the <see cref="UpdateBehavior"/> values.</para></param>
		/// <returns>number of records affected</returns>        
		public int UpdateDataSet(DataSet dataSet, string tableName,
													DbCommand insertCommand, DbCommand updateCommand,
													DbCommand deleteCommand, UpdateBehavior updateBehavior)
		{
			using (DbConnection connection = OpenConnection())
			{
				if (updateBehavior == UpdateBehavior.Transactional)
				{
					DbTransaction trans = BeginTransaction(connection);
					try
					{
						int rowsAffected = UpdateDataSet(dataSet, tableName, insertCommand, updateCommand, deleteCommand, trans);
						CommitTransaction(trans);
						return rowsAffected;
					}
					catch
					{
						RollbackTransaction(trans);
						throw;
					}
				}
				else
				{
					if (insertCommand != null)
					{
						PrepareCommand(insertCommand, connection);
					}
					if (updateCommand != null)
					{
						PrepareCommand(updateCommand, connection);
					}
					if (deleteCommand != null)
					{
						PrepareCommand(deleteCommand, connection);
					}

					return DoUpdateDataSet(updateBehavior, dataSet, tableName,
												  insertCommand, updateCommand, deleteCommand);
				}
			}
		}

		/// <summary>
		/// <para>Calls the respective INSERT, UPDATE, or DELETE statements for each inserted, updated, or deleted row in the <see cref="DataSet"/> within a transaction.</para>
		/// </summary>        
		/// <param name="dataSet"><para>The <see cref="DataSet"/> used to update the data source.</para></param>
		/// <param name="tableName"><para>The name of the source table to use for table mapping.</para></param>
		/// <param name="insertCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <seealso cref="DataRowState.Added"/>.</para></param>
		/// <param name="updateCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <seealso cref="DataRowState.Modified"/>.</para></param>        
		/// <param name="deleteCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <seealso cref="DataRowState.Deleted"/>.</para></param>        
		/// <param name="transaction"><para>The <see cref="IDbTransaction"/> to use.</para></param>
		/// <returns>Number of records affected.</returns>        
		public int UpdateDataSet(DataSet dataSet, string tableName,
													DbCommand insertCommand, DbCommand updateCommand,
													DbCommand deleteCommand, DbTransaction transaction)
		{
			if (insertCommand != null)
			{
				PrepareCommand(insertCommand, transaction);
			}
			if (updateCommand != null)
			{
				PrepareCommand(updateCommand, transaction);
			}
			if (deleteCommand != null)
			{
				PrepareCommand(deleteCommand, transaction);
			}

			return DoUpdateDataSet(UpdateBehavior.Transactional,
										  dataSet, tableName, insertCommand, updateCommand, deleteCommand);
		}

		/// <summary>
		/// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
		/// </summary>
		/// <param name="command">The command to add the parameter.</param>
		/// <param name="name"><para>The name of the parameter.</para></param>
		/// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
		/// <param name="size"><para>The maximum size of the data within the column.</para></param>
		/// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>
		/// <param name="nullable"><para>Avalue indicating whether the parameter accepts <see langword="null"/> (<b>Nothing</b> in Visual Basic) values.</para></param>
		/// <param name="precision"><para>The maximum number of digits used to represent the <paramref name="value"/>.</para></param>
		/// <param name="scale"><para>The number of decimal places to which <paramref name="value"/> is resolved.</para></param>
		/// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
		/// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
		/// <param name="value"><para>The value of the parameter.</para></param>       
		public virtual void AddParameter(DbCommand command, string name, DbType dbType, int size, ParameterDirection direction, bool nullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
		{
			DbParameter parameter = CreateParameter(name, dbType, size, direction, nullable, precision, scale, sourceColumn, sourceVersion, value);
			command.Parameters.Add(parameter);
		}

		/// <summary>
		/// <para>Adds a new instance of a <see cref="DbParameter"/> object to the command.</para>
		/// </summary>
		/// <param name="command">The command to add the parameter.</param>
		/// <param name="name"><para>The name of the parameter.</para></param>
		/// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>        
		/// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>                
		/// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
		/// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
		/// <param name="value"><para>The value of the parameter.</para></param>    
		public void AddParameter(DbCommand command, string name, DbType dbType, ParameterDirection direction, string sourceColumn, DataRowVersion sourceVersion, object value)
		{
			AddParameter(command, name, dbType, 0, direction, false, 0, 0, sourceColumn, sourceVersion, value);
		}

		/// <summary>
		/// Adds a new Out <see cref="DbParameter"/> object to the given <paramref name="command"/>.
		/// </summary>
		/// <param name="command">The command to add the out parameter.</param>
		/// <param name="name"><para>The name of the parameter.</para></param>
		/// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>        
		/// <param name="size"><para>The maximum size of the data within the column.</para></param>        
		public void AddOutParameter(DbCommand command, string name, DbType dbType, int size)
		{
			AddParameter(command, name, dbType, size, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, DBNull.Value);
		}

		/// <summary>
		/// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
		/// </summary>
		/// <param name="command">The command to add the in parameter.</param>
		/// <param name="name"><para>The name of the parameter.</para></param>
		/// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>                
		/// <remarks>
		/// <para>This version of the method is used when you can have the same parameter object multiple times with different values.</para>
		/// </remarks>        
		public void AddInParameter(DbCommand command, string name, DbType dbType)
		{
			AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, null);
		}

		/// <summary>
		/// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
		/// </summary>
		/// <param name="command">The commmand to add the parameter.</param>
		/// <param name="name"><para>The name of the parameter.</para></param>
		/// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>                
		/// <param name="value"><para>The value of the parameter.</para></param>      
		public void AddInParameter(DbCommand command, string name, DbType dbType, object value)
		{
			AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, value);
		}

		/// <summary>
		/// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
		/// </summary>
		/// <param name="command">The command to add the parameter.</param>
		/// <param name="name"><para>The name of the parameter.</para></param>
		/// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>                
		/// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the value.</para></param>
		/// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
		public void AddInParameter(DbCommand command, string name, DbType dbType, string sourceColumn, DataRowVersion sourceVersion)
		{
			AddParameter(command, name, dbType, 0, ParameterDirection.Input, true, 0, 0, sourceColumn, sourceVersion, null);
		}

		/// <summary>
		/// Clears the parameter cache. Since there is only one parameter cache that is shared by all instances
		/// of this class, this clears all parameters cached for all databases.
		/// </summary>
		public static void ClearParameterCache()
		{
			Database.parameterCache.Clear();
		}

		/// <summary>
		/// Sets a parameter value.
		/// </summary>
		/// <param name="command">The command with the parameter.</param>
		/// <param name="parameterName">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		public virtual void SetParameterValue(DbCommand command, string parameterName, object value)
		{
			command.Parameters[BuildParameterName(parameterName)].Value = (value == null) ? DBNull.Value : value;
		}

		/// <summary>
		/// Gets a parameter value.
		/// </summary>
		/// <param name="command">The command that contains the parameter.</param>
		/// <param name="name">The name of the parameter.</param>
		/// <returns>The value of the parameter.</returns>
		public virtual object GetParameterValue(DbCommand command, string name)
		{
			return command.Parameters[BuildParameterName(name)].Value;
		}

		/// <summary>
		/// <para>Assigns a <paramref name="connection"/> to the <paramref name="command"/> and discovers parameters if needed.</para>
		/// </summary>
		/// <param name="command"><para>The command that contains the query to prepare.</para></param>
		/// <param name="connection">The connection to assign to the command.</param>
		protected static void PrepareCommand(DbCommand command, DbConnection connection)
		{
			if (command == null) throw new ArgumentNullException("command");
			if (connection == null) throw new ArgumentNullException("connection");

			command.Connection = connection;
		}

		/// <summary>
		/// <para>Assigns a <paramref name="transaction"/> to the <paramref name="command"/> and discovers parameters if needed.</para>
		/// </summary>
		/// <param name="command"><para>The command that contains the query to prepare.</para></param>
		/// <param name="transaction">The transaction to assign to the command.</param>
		protected static void PrepareCommand(DbCommand command, DbTransaction transaction)
		{
			if (command == null) throw new ArgumentNullException("command");
			if (transaction == null) throw new ArgumentNullException("transaction");

			PrepareCommand(command, transaction.Connection);
			command.Transaction = transaction;
		}

		/// <summary>
		/// <para>Opens a connection.</para>
		/// </summary>
		/// <returns>The opened connection.</returns>
		protected DbConnection OpenConnection()
		{
			DbConnection connection = null;
			try
			{
				try
				{
					connection = CreateConnection();
					connection.Open();
				}
				catch (Exception e)
				{
					instrumentationProvider.FireConnectionFailedEvent(ConnectionStringNoCredentials, e);
					throw;
				}

				instrumentationProvider.FireConnectionOpenedEvent();
			}
			catch
			{
				if (connection != null)
					connection.Close();

				throw;
			}

			return connection;
		}

		/// <summary>
		/// Determines if the number of parameters in the command matches the array of parameter values.
		/// </summary>
		/// <param name="command">The <see cref="DbCommand"/> containing the parameters.</param>
		/// <param name="values">The array of parameter values.</param>
		/// <returns><see langword="true"/> if the number of parameters and values match; otherwise, <see langword="false"/>.</returns>
		protected virtual bool SameNumberOfParametersAndValues(DbCommand command, object[] values)
		{
			int numberOfParametersToStoredProcedure = command.Parameters.Count;
			int numberOfValuesProvidedForStoredProcedure = values.Length;
			return numberOfParametersToStoredProcedure == numberOfValuesProvidedForStoredProcedure;
		}

		/// <summary>
		/// Builds a value parameter name for the current database.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <returns>A correctly formated parameter name.</returns>
		public virtual string BuildParameterName(string name)
		{
			return name;
		}

		/// <summary>
		/// Discovers the parameters for a <see cref="DbCommand"/>.
		/// </summary>
		/// <param name="command">The <see cref="DbCommand"/> to discover the parameters.</param>
		public void DiscoverParameters(DbCommand command)
		{
			using (DbConnection discoveryConnection = OpenConnection())
			{
				using (DbCommand discoveryCommand = CreateCommandByCommandType(command.CommandType, command.CommandText))
				{
					discoveryCommand.Connection = discoveryConnection;
					DeriveParameters(discoveryCommand);

					foreach (IDataParameter parameter in discoveryCommand.Parameters)
					{
						IDataParameter cloneParameter = (IDataParameter)((ICloneable)parameter).Clone();
						command.Parameters.Add(cloneParameter);
					}
				}
			}
		}

		/// <summary>
		/// Retrieves parameter information from the stored procedure specified in the <see cref="DbCommand"/> and populates the Parameters collection of the specified <see cref="DbCommand"/> object. 
		/// </summary>
		/// <param name="discoveryCommand">The <see cref="DbCommand"/> to do the discovery.</param>
		protected abstract void DeriveParameters(DbCommand discoveryCommand);

		private DbCommand CreateCommandByCommandType(CommandType commandType, string commandText)
		{
			DbCommand command = dbProviderFactory.CreateCommand();
			command.CommandType = commandType;
			command.CommandText = commandText;

			return command;
		}

		private int DoUpdateDataSet(UpdateBehavior behavior,
								  DataSet dataSet, string tableName, DbCommand insertCommand,
								  DbCommand updateCommand, DbCommand deleteCommand)
		{
			if (string.IsNullOrEmpty(tableName)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, "tableName");
			if (dataSet == null) throw new ArgumentNullException("dataSet");

			if (insertCommand == null && updateCommand == null && deleteCommand == null)
			{
				throw new ArgumentException(Resources.ExceptionMessageUpdateDataSetArgumentFailure);
			}

			using (DbDataAdapter adapter = GetDataAdapter(behavior))
			{
				IDbDataAdapter explicitAdapter = (IDbDataAdapter)adapter;
				if (insertCommand != null)
				{
					explicitAdapter.InsertCommand = insertCommand;
				}
				if (updateCommand != null)
				{
					explicitAdapter.UpdateCommand = updateCommand;
				}
				if (deleteCommand != null)
				{
					explicitAdapter.DeleteCommand = deleteCommand;
				}

				try
				{
					DateTime startTime = DateTime.Now;
					int rows = adapter.Update(dataSet.Tables[tableName]);
					instrumentationProvider.FireCommandExecutedEvent(startTime);
					return rows;
				}
				catch (Exception e)
				{
					instrumentationProvider.FireCommandFailedEvent("DbDataAdapter.Update() " + tableName, ConnectionStringNoCredentials, e);
					throw;
				}
			}
		}

		private void DoLoadDataSet(DbCommand command, DataSet dataSet, string[] tableNames)
		{
			if (tableNames == null) throw new ArgumentNullException("tableNames");
			if (tableNames.Length == 0)
			{
				throw new ArgumentException(Resources.ExceptionTableNameArrayEmpty, "tableNames");
			}
			for (int i = 0; i < tableNames.Length; i++)
			{
				if (string.IsNullOrEmpty(tableNames[i])) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, string.Concat("tableNames[", i, "]"));
			}

			using (DbDataAdapter adapter = GetDataAdapter(UpdateBehavior.Standard))
			{
				((IDbDataAdapter)adapter).SelectCommand = command;

				try
				{
					DateTime startTime = DateTime.Now;
					string systemCreatedTableNameRoot = "Table";
					for (int i = 0; i < tableNames.Length; i++)
					{
						string systemCreatedTableName = (i == 0)
							 ? systemCreatedTableNameRoot
							 : systemCreatedTableNameRoot + i;

						adapter.TableMappings.Add(systemCreatedTableName, tableNames[i]);
					}

					adapter.Fill(dataSet);
					instrumentationProvider.FireCommandExecutedEvent(startTime);
				}
				catch (Exception e)
				{
					instrumentationProvider.FireCommandFailedEvent(command.CommandText, ConnectionStringNoCredentials, e);
					throw;
				}
			}
		}

		private object DoExecuteScalar(DbCommand command)
		{
			try
			{
				DateTime startTime = DateTime.Now;
				object returnValue = command.ExecuteScalar();
				instrumentationProvider.FireCommandExecutedEvent(startTime);
				return returnValue;
			}
			catch (Exception e)
			{
				instrumentationProvider.FireCommandFailedEvent(command.CommandText, ConnectionStringNoCredentials, e);
				throw;
			}
		}

		private int DoExecuteNonQuery(DbCommand command)
		{
			try
			{
				DateTime startTime = DateTime.Now;
				int rowsAffected = command.ExecuteNonQuery();
				instrumentationProvider.FireCommandExecutedEvent(startTime);
				return rowsAffected;
			}
			catch (Exception e)
			{
				instrumentationProvider.FireCommandFailedEvent(command.CommandText, ConnectionStringNoCredentials, e);
				throw;
			}
		}

		private IDataReader DoExecuteReader(DbCommand command, CommandBehavior cmdBehavior)
		{
			try
			{
				DateTime startTime = DateTime.Now;
				IDataReader reader = command.ExecuteReader(cmdBehavior);
				instrumentationProvider.FireCommandExecutedEvent(startTime);
				return reader;
			}
			catch (Exception e)
			{
				instrumentationProvider.FireCommandFailedEvent(command.CommandText, ConnectionStringNoCredentials, e);
				throw;
			}
		}

		private DbTransaction BeginTransaction(DbConnection connection)
		{
			DbTransaction tran = connection.BeginTransaction();
			return tran;
		}

		private void RollbackTransaction(DbTransaction tran)
		{
			tran.Rollback();
		}

		private void CommitTransaction(DbTransaction tran)
		{
			tran.Commit();
		}

		private void AssignParameterValues(DbCommand command, object[] values)
		{
			int parameterIndexShift = UserParametersStartIndex();	// DONE magic number, depends on the database
			for (int i = 0; i < values.Length; i++)
			{
				IDataParameter parameter = command.Parameters[i + parameterIndexShift];

				// There used to be code here that checked to see if the parameter was input or input/output
				// before assigning the value to it. We took it out because of an operational bug with
				// deriving parameters for a stored procedure. It turns out that output parameters are set
				// to input/output after discovery, so any direction checking was unneeded. Should it ever
				// be needed, it should go here, and check that a parameter is input or input/output before
				// assigning a value to it.
				SetParameterValue(command, parameter.ParameterName, values[i]);
			}
		}

		/// <summary>
		/// <para>Adds a new instance of a <see cref="DbParameter"/> object.</para>
		/// </summary>
		/// <param name="name"><para>The name of the parameter.</para></param>
		/// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
		/// <param name="size"><para>The maximum size of the data within the column.</para></param>
		/// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>
		/// <param name="nullable"><para>Avalue indicating whether the parameter accepts <see langword="null"/> (<b>Nothing</b> in Visual Basic) values.</para></param>
		/// <param name="precision"><para>The maximum number of digits used to represent the <paramref name="value"/>.</para></param>
		/// <param name="scale"><para>The number of decimal places to which <paramref name="value"/> is resolved.</para></param>
		/// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
		/// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
		/// <param name="value"><para>The value of the parameter.</para></param>  
		/// <returns>A newly created <see cref="DbParameter"/> fully initialized with given parameters.</returns>
		protected DbParameter CreateParameter(string name, DbType dbType, int size, ParameterDirection direction, bool nullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
		{
			DbParameter param = CreateParameter(name);
			ConfigureParameter(param, name, dbType, size, direction, nullable, precision, scale, sourceColumn, sourceVersion, value);
			return param;
		}

		/// <summary>
		/// <para>Adds a new instance of a <see cref="DbParameter"/> object.</para>
		/// </summary>
		/// <param name="name"><para>The name of the parameter.</para></param>
		/// <returns><para>An unconfigured parameter.</para></returns>
		protected DbParameter CreateParameter(string name)
		{
			DbParameter param = dbProviderFactory.CreateParameter();
			param.ParameterName = BuildParameterName(name);

			return param;
		}

		/// <summary>
		/// Returns the starting index for parameters in a command.
		/// </summary>
		/// <returns>The starting index for parameters in a command.</returns>
		protected virtual int UserParametersStartIndex()
		{
			return 0;
		}

		/// <summary>
		/// Configures a given <see cref="DbParameter"/>.
		/// </summary>
		/// <param name="param">The <see cref="DbParameter"/> to configure.</param>
		/// <param name="name"><para>The name of the parameter.</para></param>
		/// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
		/// <param name="size"><para>The maximum size of the data within the column.</para></param>
		/// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>
		/// <param name="nullable"><para>Avalue indicating whether the parameter accepts <see langword="null"/> (<b>Nothing</b> in Visual Basic) values.</para></param>
		/// <param name="precision"><para>The maximum number of digits used to represent the <paramref name="value"/>.</para></param>
		/// <param name="scale"><para>The number of decimal places to which <paramref name="value"/> is resolved.</para></param>
		/// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
		/// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
		/// <param name="value"><para>The value of the parameter.</para></param>  
		protected virtual void ConfigureParameter(DbParameter param, string name, DbType dbType, int size, ParameterDirection direction, bool nullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
		{
			param.DbType = dbType;
			param.Size = size;
			param.Value = (value == null) ? DBNull.Value : value;
			param.Direction = direction;
			param.IsNullable = nullable;
			param.SourceColumn = sourceColumn;
			param.SourceVersion = sourceVersion;
		}

	}
}
