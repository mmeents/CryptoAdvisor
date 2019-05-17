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
	/// Configuration object for Instrumentation. This section defines the instrumentation behavior 
	/// for the entire application
	/// </summary>
	public class InstrumentationConfigurationSection : SerializableConfigurationSection
    {
		private const string performanceCountersEnabled = "performanceCountersEnabled";
        private const string eventLoggingEnabled = "eventLoggingEnabled";
        private const string wmiEnabled = "wmiEnabled";

		/// <summary>
		/// Section name
		/// </summary>
        public const string SectionName = "instrumentationConfiguration";

		internal bool InstrumentationIsEntirelyDisabled
		{
			get { return (PerformanceCountersEnabled || EventLoggingEnabled || WmiEnabled) == false; }
		}

		/// <summary>
		/// Initializes enabled state of the three forms of instrumentation
		/// </summary>
		/// <param name="performanceCountersEnabled">True if performance counter instrumentation is to be enabled</param>
		/// <param name="eventLoggingEnabled">True if event logging instrumentation is to be enabled</param>
		/// <param name="wmiEnabled">True if wmi instrumentation is to be enabled</param>
        public InstrumentationConfigurationSection(bool performanceCountersEnabled, bool eventLoggingEnabled, bool wmiEnabled)
        {
            this.PerformanceCountersEnabled = performanceCountersEnabled;
            this.EventLoggingEnabled = eventLoggingEnabled;
            this.WmiEnabled = wmiEnabled;
        }

		/// <summary>
		/// Initializes object to default settings of all instrumentation types disabled
		/// </summary>
        public InstrumentationConfigurationSection()
        {
        }
        
		/// <summary>
		/// Gets and sets the value of PerformanceCountersEnabled
		/// </summary>
        [ConfigurationProperty(performanceCountersEnabled, IsRequired = false, DefaultValue = false)]
        public bool PerformanceCountersEnabled
        {
            get { return (bool)this[performanceCountersEnabled]; }
            set { this[performanceCountersEnabled] = value; }
        }
		
		/// <summary>
		/// Gets and sets the value of EventLoggingEnabled
		/// </summary>
        [ConfigurationProperty(eventLoggingEnabled, IsRequired = false, DefaultValue = false)]
        public bool EventLoggingEnabled
        {
            get { return (bool)this[eventLoggingEnabled]; }
            set { this[eventLoggingEnabled] = value; }
        }

		/// <summary>
		/// Gets and sets value of WmiEnabled
		/// </summary>
        [ConfigurationProperty(wmiEnabled, IsRequired = false, DefaultValue = false)]
        public bool WmiEnabled
        {
            get { return (bool)this[wmiEnabled]; }
            set { this[wmiEnabled] = value; }
        }
    }
}
