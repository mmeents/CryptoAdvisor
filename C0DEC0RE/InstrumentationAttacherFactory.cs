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
	internal class InstrumentationAttacherFactory
	{
		public IInstrumentationAttacher CreateBinder(object createdObject, object[] constructorArgs, ConfigurationReflectionCache reflectionCache)
		{
			InstrumentationListenerAttribute listenerAttribute = GetInstrumentationListenerAttribute(createdObject, reflectionCache);

			if (listenerAttribute == null) return new NoBindingInstrumentationAttacher();

			Type listenerType = listenerAttribute.ListenerType;
			Type listenerBinderType = listenerAttribute.ListenerBinderType;

			if(listenerBinderType == null) return new ReflectionInstrumentationAttacher(createdObject, listenerType, constructorArgs);
			return new ExplicitInstrumentationAttacher(createdObject, listenerType, constructorArgs, listenerBinderType);
		}

		private InstrumentationListenerAttribute GetInstrumentationListenerAttribute(object createdObject, ConfigurationReflectionCache reflectionCache)
		{
			Type createdObjectType = createdObject.GetType();
			InstrumentationListenerAttribute listenerAttribute 
				= reflectionCache.GetCustomAttribute<InstrumentationListenerAttribute>(createdObjectType, true);
			return listenerAttribute;
		}
	}
}
