//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Data Access Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.ComponentModel;


namespace C0DEC0RE
{
	/// <summary>
	/// Represents the mapping from an ADO.NET provider to an Enterprise Library <see cref="Database"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The Enterprise Library Data Access Application Block leverages the ADO.NET 2.0 provider factories. To determine what type of <see cref="Database"/> matches a given provider factory type, the optional 
	/// <see cref="DbProviderMapping"/> configuration objects can be defined in the block's configuration section.
	/// </para>
	/// <para>
	/// If a mapping is not present for a given provider type, sensible defaults will be used:
	/// <list type="bullet">
	/// <item>For provider name "System.Data.SqlClient", or for a provider of type <see cref="System.Data.SqlClient.SqlClientFactory"/>, the 
	/// <see cref="C0DEC0RE.Sql.SqlDatabase"/> will be used.</item>
	/// <item>For provider name "System.Data.OracleClient", or for a provider of type <see cref="System.Data.OracleClient.OracleClientFactory"/>, the 
	/// <see cref="C0DEC0RE.Oracle.OracleDatabase"/> will be used.</item>
	/// <item>In any other case, the <see cref="GenericDatabase"/> will be used.</item>
	/// </list>
	/// </para>
	/// </remarks>
	/// <seealso cref="DatabaseConfigurationView.GetProviderMapping(string, string)"/>
	/// <seealso cref="System.Data.Common.DbProviderFactory"/>
	public class DbProviderMapping : NamedConfigurationElement
	{
		/// <summary>
		/// Default name for the Sql managed provider.
		/// </summary>
		public const string DefaultSqlProviderName = "System.Data.SqlClient";
		/// <summary>
		/// Default name for the Oracle managed provider.
		/// </summary>
		public const string DefaultOracleProviderName = "System.Data.OracleClient";

		internal const string DefaultGenericProviderName = "generic";
		private const string databaseTypeProperty = "databaseType";

		/// <summary>
		/// Initializes a new instance of the <see cref="DbProviderMapping"/> class.
		/// </summary>
		public DbProviderMapping()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DbProviderMapping"/> class with name and <see cref="Database"/> type.
		/// </summary>
		public DbProviderMapping(string dbProviderName, Type databaseType)
			: base(dbProviderName)
		{
			this.DatabaseType = databaseType;
		}

		/// <summary>
		/// Gets or sets the type of database to use for the mapped ADO.NET provider.
		/// </summary>
		[ConfigurationProperty(databaseTypeProperty)]
		[TypeConverter(typeof(AssemblyQualifiedTypeNameConverter))]
		[SubclassTypeValidator(typeof(Database))]
		public Type DatabaseType
		{
			get { return (Type)base[databaseTypeProperty]; }
			set { base[databaseTypeProperty] = value; }
		}

		/// <summary>
		///  Gets the logical name of the ADO.NET provider.
		/// </summary>
		public string DbProviderName
		{
			get { return Name; }
		}
	}
}
