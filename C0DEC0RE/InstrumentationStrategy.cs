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


namespace C0DEC0RE
{
	/// <summary>
	/// This type supports the Enterprise Library infrastructure and is not intended to be used directly from your code.
	/// Adapter used to inject instrumentation attachment process into ObjectBuilder creation process
	/// for objects.
	/// </summary>
	/// <seealso cref="InstrumentationAttachmentStrategy"/>
	public class InstrumentationStrategy : EnterpriseLibraryBuilderStrategy
	{
		/// <summary>
		/// Implementation of <see cref="IBuilderStrategy.BuildUp"/>.
		/// </summary>
		/// <remarks>
		/// This implementation will attach instrumentation to the created objects. 
		/// </remarks>
		/// <param name="context">The build context.</param>
		/// <param name="t">The type of the object being built.</param>
		/// <param name="existing">The existing instance of the object.</param>
		/// <param name="id">The ID of the object being built.</param>
		/// <returns>The built object.</returns>
		public override object BuildUp(IBuilderContext context, Type t, object existing, string id)
		{
			if (existing != null)
			{
				IConfigurationSource configurationSource = GetConfigurationSource(context);
				ConfigurationReflectionCache reflectionCache = GetReflectionCache(context);
				InstrumentationAttachmentStrategy instrumentation = new InstrumentationAttachmentStrategy();

				if (ConfigurationNameProvider.IsMadeUpName(id))
				{
					instrumentation.AttachInstrumentation(existing, configurationSource, reflectionCache);
				}
				else
				{
					instrumentation.AttachInstrumentation(id, existing, configurationSource, reflectionCache);
				}
			}

			return base.BuildUp(context, t, existing, id);
		}
	}
}
