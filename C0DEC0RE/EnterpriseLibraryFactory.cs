//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Core
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using C0DEC0RE.Properties;



namespace C0DEC0RE
{
	/// <summary>
	/// Static facade for the generic building mechanism based on ObjectBuilder.
	/// </summary>
	/// <remarks>
	/// The facade uses a shared stateless <see cref="IBuilder{TStageEnum}"/> instance configured with the strategies 
	/// that perform the creation of objects in the Enterprise Library.
	/// <para>
	/// The strategies used by the <see cref="EnterpriseLibraryFactory"/> are:
	/// <list type="bullet">
	/// <item><see cref="ConfigurationNameMappingStrategy"/> to deal with default instances.</item>
	/// <item><see cref="SingletonStrategy"/> to deal with singletons.</item>
	/// <item><see cref="ConfiguredObjectStrategy"/> to perform the actual creation of the objects based on the available configuration.</item>
	/// <item><see cref="InstrumentationStrategy"/> to attach instrumentation to the created objects.</item>
	/// </list>
	/// </para>
	/// <para>
	/// The creation request can provide an <see cref="IConfigurationSource"/> to be used by the strategies that need access to 
	/// configuration. If such a configuration source is not provided, a default configuration source will be requested 
	/// to the <see cref="ConfigurationSourceFactory"/>.
	/// In any case, the configuration source is made available to the strategies through a transient <see cref="IConfigurationObjectPolicy"/>.
	/// </para>
	/// <para>
	/// The facade keeps a shared <see cref="ConfigurationReflectionCache"/> that is made available to the strategies through a transient 
	/// <see cref="IReflectionCachePolicy"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="ConfiguredObjectStrategy"/>
	public static class EnterpriseLibraryFactory
	{
		private static IBuilder<BuilderStage> builder;
		private static ConfigurationReflectionCache reflectionCache = new ConfigurationReflectionCache();

		static EnterpriseLibraryFactory()
		{
			builder = new BuilderBase<BuilderStage>();
			builder.Strategies.AddNew<ConfigurationNameMappingStrategy>(BuilderStage.PreCreation);
			builder.Strategies.AddNew<SingletonStrategy>(BuilderStage.PreCreation);
			builder.Strategies.AddNew<ConfiguredObjectStrategy>(BuilderStage.PreCreation);
			builder.Strategies.AddNew<InstrumentationStrategy>(BuilderStage.PostInitialization);
		}

		/// <overloads>
		/// Returns an instance of type <typeparamref name="T"/>.
		/// </overloads>
		/// <summary>
		/// Returns a new default instance of type <typeparamref name="T"/> based on configuration information 
		/// from the default configuration source.
		/// </summary>
		/// <typeparam name="T">The type to build.</typeparam>
		/// <returns>A new instance of <typeparamref name="T"/> or any of it subtypes.</returns>
		public static T BuildUp<T>()
		{
			return BuildUp<T>(ConfigurationSource);
		}

		/// <summary>
		/// Returns a default instance of type <typeparamref name="T"/> based on configuration information 
		/// from the default configuration source.
		/// </summary>
		/// <typeparam name="T">The type to build.</typeparam>
		/// <param name="locator">The locator to be used for this build operation.</param>
		/// <returns>A new instance of <typeparamref name="T"/> or any of it subtypes, or an existing instance
		/// if type <typeparamref name="T"/> is a singleton that is already present in the <paramref name="locator"/>.
		/// </returns>
		public static T BuildUp<T>(IReadWriteLocator locator)
		{
			return BuildUp<T>(locator, ConfigurationSource);
		}

		/// <summary>
		/// Returns a new default instance of type <typeparamref name="T"/> based on configuration information 
		/// from <paramref name="configurationSource"/>.
		/// </summary>
		/// <typeparam name="T">The type to build.</typeparam>
		/// <param name="configurationSource">The source for configuration information.</param>
		/// <returns>A new instance of <typeparamref name="T"/> or any of it subtypes.</returns>
		public static T BuildUp<T>(IConfigurationSource configurationSource)
		{
			return BuildUp<T>((IReadWriteLocator)null, configurationSource);
		}

		/// <summary>
		/// Returns a default instance of type <typeparamref name="T"/> based on configuration information 
		/// from <paramref name="configurationSource"/>.
		/// </summary>
		/// <typeparam name="T">The type to build.</typeparam>
		/// <param name="locator">The locator to be used for this build operation.</param>
		/// <param name="configurationSource">The source for configuration information.</param>
		/// <returns>A new instance of <typeparamref name="T"/> or any of it subtypes, or an existing instance
		/// if type <typeparamref name="T"/> is a singleton that is already present in the <paramref name="locator"/>.
		/// </returns>
		public static T BuildUp<T>(IReadWriteLocator locator, IConfigurationSource configurationSource)
		{
			if (configurationSource == null)
				throw new ArgumentNullException("configurationSource");

			return GetObjectBuilder().BuildUp<T>(locator, null, null, GetPolicies(configurationSource));
		}

		/// <summary>
		/// Returns a new default instance of type <typeparamref name="T"/> based on configuration information 
		/// from the default configuration source for <paramref name="id"/>.
		/// </summary>
		/// <typeparam name="T">The type to build.</typeparam>
		/// <param name="id">The id of the object to build.</param>
		/// <returns>A new instance of <typeparamref name="T"/> or any of it subtypes.</returns>
		public static T BuildUp<T>(string id)
		{
			return BuildUp<T>(id, ConfigurationSource);
		}

		/// <summary>
		/// Returns a new default instance of type <typeparamref name="T"/> based on configuration information 
		/// from <paramref name="configurationSource"/> for <paramref name="id"/>.
		/// </summary>
		/// <typeparam name="T">The type to build.</typeparam>
		/// <param name="id">The id of the object to build.</param>
		/// <param name="configurationSource">The source for configuration information.</param>
		/// <returns>A new instance of <typeparamref name="T"/> or any of it subtypes.</returns>
		public static T BuildUp<T>(string id, IConfigurationSource configurationSource)
		{
			return BuildUp<T>(null, id, configurationSource);
		}

		/// <summary>
		/// Returns a default instance of type <typeparamref name="T"/> based on configuration information 
		/// from the default configuration source for <paramref name="id"/>.
		/// </summary>
		/// <typeparam name="T">The type to build.</typeparam>
		/// <param name="id">The id of the object to build.</param>
		/// <param name="locator">The locator to be used for this build operation.</param>
		/// <returns>A new instance of <typeparamref name="T"/> or any of it subtypes, or an existing instance
		/// if type <typeparamref name="T"/> is a singleton that is already present in the <paramref name="locator"/>.
		/// </returns>
		public static T BuildUp<T>(IReadWriteLocator locator, string id)
		{
			return BuildUp<T>(locator, id, ConfigurationSource);
		}

		/// <summary>
		/// Returns a default instance of type <typeparamref name="T"/> based on configuration information 
		/// from <paramref name="configurationSource"/> for <paramref name="id"/>.
		/// </summary>
		/// <typeparam name="T">The type to build.</typeparam>
		/// <param name="id">The id of the object to build.</param>
		/// <param name="locator">The locator to be used for this build operation.</param>
		/// <param name="configurationSource">The source for configuration information.</param>
		/// <returns>A new instance of <typeparamref name="T"/> or any of it subtypes, or an existing instance
		/// if type <typeparamref name="T"/> is a singleton that is already present in the <paramref name="locator"/>.
		/// </returns>
		public static T BuildUp<T>(IReadWriteLocator locator, string id, IConfigurationSource configurationSource)
		{
			if (string.IsNullOrEmpty(id))
				throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, "id");
			if (configurationSource == null)
				throw new ArgumentNullException("configurationSource");

			return GetObjectBuilder().BuildUp<T>(locator, id, null, GetPolicies(configurationSource));
		}

		private static PolicyList GetPolicies(IConfigurationSource configurationSource)
		{
			PolicyList policyList = new PolicyList();
			policyList.Set<IConfigurationObjectPolicy>(new ConfigurationObjectPolicy(configurationSource), typeof(IConfigurationSource), null);
			policyList.Set<IReflectionCachePolicy>(new ReflectionCachePolicy(reflectionCache), typeof(IReflectionCachePolicy), null);

			return policyList;
		}

		private static IBuilder<BuilderStage> GetObjectBuilder()
		{
			return builder;
		}
        
	    private static IConfigurationSource ConfigurationSource
        {
            get 
            {
                return ConfigurationSourceFactory.Create();
            }
        }
	}
}
