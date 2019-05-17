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
using System.Configuration;

namespace C0DEC0RE
{
	/// <summary>
	/// Implementation of <see cref="IBuilderStrategy"/> which maps null instance names into a different name.
	/// </summary>
	/// <remarks>
	/// The strategy is used to deal with default names.
	/// </remarks>
	/// <seealso cref="ConfigurationNameMapperAttribute"/>
	/// <seealso cref="IConfigurationNameMapper"/>
	public class ConfigurationNameMappingStrategy : EnterpriseLibraryBuilderStrategy
	{
		/// <summary>
		/// Override of <see cref="IBuilderStrategy.BuildUp"/>. Updates the instance name using a name mapper associated to type <paramref name="t"/>
		/// so later strategies in the build chain will use the updated instance name.
		/// </summary>
		/// <remarks>
		/// Will only update the instance name if it is <see langword="null"/>.
		/// </remarks>
		/// <param name="context">The <see cref="IBuilderContext"/> that represents the current building process.</param>
		/// <param name="t">The type to build.</param>
		/// <param name="existing">The existing object. Should be <see langword="null"/>.</param>
		/// <param name="id">The ID of the object to be created.</param>
		/// <returns>The created object.</returns>
		/// <exception cref="System.Configuration.ConfigurationErrorsException"> when the configuration required to do the mapping is not present or is 
		/// invalid in the configuration source.</exception>
		public override object BuildUp(IBuilderContext context, Type t, object existing, string id)
		{
			if (id == null)
			{
				ConfigurationReflectionCache reflectionCache = GetReflectionCache(context);

				IConfigurationNameMapper mapper = reflectionCache.GetConfigurationNameMapper(t);
				if (mapper != null)
				{
					id = mapper.MapName(id, GetConfigurationSource(context));
				}
			}

			return base.BuildUp(context, t, existing, id);
		}
	}
}
