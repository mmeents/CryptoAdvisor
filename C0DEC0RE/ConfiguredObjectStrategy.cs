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
using System.Reflection;
using C0DEC0RE.Properties;

namespace C0DEC0RE
{
	/// <summary>
	/// Implementation of <see cref="IBuilderStrategy"/> which creates objects.
	/// </summary>
	/// <remarks>
	/// <para>The strategy looks for the <see cref="CustomFactoryAttribute">CustomFactory</see> attribute to 
	/// retrieve the <see cref="ICustomFactory"/> implementation needed to build the requested types based on 
	/// configuration.</para>
	/// <para>The provided context must have a <see cref="ConfigurationObjectPolicy"/> holding a <see cref="IConfigurationSource"/>
	/// where to request the configuration information.</para>
	/// </remarks>
	/// <seealso cref="ICustomFactory"/>
	/// <seealso cref="CustomFactoryAttribute"/>
	/// <seealso cref="ConfigurationObjectPolicy"/>
	public class ConfiguredObjectStrategy : EnterpriseLibraryBuilderStrategy
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConfiguredObjectStrategy"/> class.
		/// </summary>
		public ConfiguredObjectStrategy()
		{ }

		/// <summary>
		/// Override of <see cref="IBuilderStrategy.BuildUp"/>. Creates the requested object using the custom factory associated to type <paramref name="t"/>.
		/// </summary>
		/// <param name="context">The <see cref="IBuilderContext"/> that represents the current building process.</param>
		/// <param name="t">The type to build.</param>
		/// <param name="existing">The existing object. Should be <see langword="null"/>.</param>
		/// <param name="id">The ID of the object to be created.</param>
		/// <returns>The created object.</returns>
		/// <exception cref="InvalidOperationException"> when the requested type does not have the 
		/// required <see cref="CustomFactoryAttribute">CustomFactory</see> attribute.</exception>
		/// <exception cref="System.Configuration.ConfigurationErrorsException"> when the configuration for the requested ID is not present or is 
		/// invalid in the configuration source.</exception>
		public override object BuildUp(IBuilderContext context, Type t, object existing, string id)
		{
			string newId = id;

			IConfigurationSource configurationSource = GetConfigurationSource(context);
			ConfigurationReflectionCache reflectionCache = GetReflectionCache(context);

			ICustomFactory factory = GetCustomFactory(t, reflectionCache);
			if (factory != null)
			{
				existing = factory.CreateObject(context, newId, configurationSource, reflectionCache);
			}
			else
			{
				throw new InvalidOperationException(
					string.Format(
						Resources.Culture,
						Resources.ExceptionCustomFactoryAttributeNotFound,
						t.FullName,
						id));
			}

			return base.BuildUp(context, t, existing, newId);
		}

		private ICustomFactory GetCustomFactory(Type t, ConfigurationReflectionCache reflectionCache)
		{
			ICustomFactory customFactory = reflectionCache.GetCustomFactory(t);

			return customFactory;
		}
	}
}
