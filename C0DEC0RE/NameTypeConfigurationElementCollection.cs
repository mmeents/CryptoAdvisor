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
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using C0DEC0RE.Properties;



namespace C0DEC0RE
{
	/// <summary>
	/// Represesnts a collection of <see cref="NameTypeConfigurationElement"/> objects.
	/// </summary>
	/// <typeparam name="T">The type of <see cref="NameTypeConfigurationElement"/> object this collection contains.</typeparam>
	public class NameTypeConfigurationElementCollection<T> : PolymorphicConfigurationElementCollection<T>
		where T : NameTypeConfigurationElement, new()
	{
		private const string typeAttribute = "type";
		
		/// <summary>
		/// Get the configuration object for each <see cref="NameTypeConfigurationElement"/> object in the collection.
		/// </summary>
		/// <param name="reader">The <see cref="XmlReader"/> that is deserializing the element.</param>
		protected override Type RetrieveConfigurationElementType(XmlReader reader)
		{
			Type configurationElementType = null;
			if (reader.AttributeCount > 0)
			{
				// expect the first attribute to be the name
				for (bool go = reader.MoveToFirstAttribute(); go; go = reader.MoveToNextAttribute())
				{
					if (typeAttribute.Equals(reader.Name))
					{
						Type providerType = Type.GetType(reader.Value, false);
						if (providerType == null)
						{
							throw new ConfigurationErrorsException(string.Format(Resources.Culture, Resources.ExceptionTypeCouldNotBeCreated, reader.Value));
						}

						Attribute attribute = Attribute.GetCustomAttribute(providerType, typeof(ConfigurationElementTypeAttribute));
						if (attribute == null)
						{
							throw new ConfigurationErrorsException(string.Format(Resources.Culture, Resources.ExceptionNoConfigurationElementAttribute, providerType.Name));
						}
						
						configurationElementType = ((ConfigurationElementTypeAttribute)attribute).ConfigurationType;

						break;
					}
				}

				if (configurationElementType == null)
				{
					throw new ConfigurationErrorsException(string.Format(Resources.Culture, Resources.ExceptionNoTypeAttribute, reader.Name));
				}

				// cover the traces ;)
				reader.MoveToElement();
			}
			return configurationElementType;
		}
	}
}
