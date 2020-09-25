//===============================================================================
// Microsoft Exception Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/emab-rm.asp
//
// ExceptionSectionHandler.cs
// This file contains the configuration section handler for the exception 
// Management settings.
//
// For more information see the Implementation of the 
// ExceptionManagementSectionHandler Class section of the Exception Management 
// Application Block Implementation Overview. 
//===============================================================================
// Copyright (C) 2000-2001 Microsoft Corporation
// All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Resources;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;
using System.Globalization;

namespace iGeospatial.Exceptions
{
	/// <summary>
	/// The Configuration Section Handler for the "exceptionManagement" section of the config file. 
	/// </summary>
	public class ExceptionSectionHandler : IConfigurationSectionHandler
	{
		#region Declare variables

		// Member variables.
		private readonly static char EXCEPTION_TYPE_DELIMITER = Convert.ToChar(";");
		private const string EXCEPTIONMANAGEMENT_MODE = "mode";
		private const string PUBLISHER_NODENAME = "publisher";
		private const string PUBLISHER_MODE = "mode";
		private const string PUBLISHER_ASSEMBLY = "assembly";
		private const string PUBLISHER_TYPE = "type";
		private const string PUBLISHER_EXCEPTIONFORMAT = "exceptionFormat";
		private const string PUBLISHER_INCLUDETYPES = "include";
		private const string PUBLISHER_EXCLUDETYPES = "exclude";
		private ResourceManager resourceManager;
		
        #endregion

        #region Constructors and Destructor

		/// <summary>
		/// The constructor for the ExceptionSectionHandler to initialize the resource file.
		/// </summary>
		public ExceptionSectionHandler()
		{
			// Load Resource File for localized text.
			resourceManager = new ResourceManager(this.GetType().Namespace + ".ExceptionManagerText",this.GetType().Assembly);
		}

		#endregion

        #region Public Methods

		/// <summary>
		/// Builds the ExceptionSettings and PublisherSettings structures based on the configuration file.
		/// </summary>
		/// <param name="parent">Composed from the configuration settings in a corresponding parent configuration section.</param>
		/// <param name="configContext">Provides access to the virtual path for which the configuration section handler computes configuration values. Normally this parameter is reserved and is null.</param>
		/// <param name="section">The XML node that contains the configuration information to be handled. section provides direct access to the XML contents of the configuration section.</param>
		/// <returns>The ExceptionSettings struct built from the configuration settings.</returns>
		public object Create(object parent,object configContext,XmlNode section)
		{
			try
			{
				ExceptionSettings settings = new ExceptionSettings();

				// Exit if there are no configuration settings.
				if (section == null) return settings;

				XmlNode currentAttribute;
				XmlAttributeCollection nodeAttributes = section.Attributes;

				// Get the mode attribute.
				currentAttribute = nodeAttributes.RemoveNamedItem(EXCEPTIONMANAGEMENT_MODE);
				if (currentAttribute != null && currentAttribute.Value.ToUpper(CultureInfo.InvariantCulture) == "OFF") 
				{
					settings.Mode = ExceptionMode.Off;
				}

				#region Loop through the publisher components and load them into the ExceptionSettings
				// Loop through the publisher components and load them into the ExceptionSettings.
				PublisherSettings publisherSettings;
				foreach(XmlNode node in section.ChildNodes)
				{
					if (node.Name == PUBLISHER_NODENAME)
					{
						// Initialize a new PublisherSettings.
						publisherSettings = new PublisherSettings();

						// Get a collection of all the attributes.
						nodeAttributes = node.Attributes;

						#region Remove the known attributes and load the struct values
						// Remove the mode attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_MODE);
						if (currentAttribute != null && currentAttribute.Value.ToUpper(CultureInfo.InvariantCulture) == "OFF") publisherSettings.Mode = PublisherMode.Off;
				
						// Remove the assembly attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_ASSEMBLY);
						if (currentAttribute != null) publisherSettings.AssemblyName = currentAttribute.Value;
				
						// Remove the type attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_TYPE);
						if (currentAttribute != null) publisherSettings.TypeName = currentAttribute.Value;

						// Remove the exceptionFormat attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_EXCEPTIONFORMAT);
						if (currentAttribute != null && currentAttribute.Value.ToUpper(CultureInfo.InvariantCulture) == "XML") publisherSettings.ExceptionFormat = PublisherFormat.Xml;

						// Remove the include attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_INCLUDETYPES);
						if (currentAttribute != null)
						{
							publisherSettings.IncludeTypes = LoadTypeFilter(currentAttribute.Value.Split(EXCEPTION_TYPE_DELIMITER));
						}

						// Remove the exclude attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_EXCLUDETYPES);
						if (currentAttribute != null)
						{
							publisherSettings.ExcludeTypes = LoadTypeFilter(currentAttribute.Value.Split(EXCEPTION_TYPE_DELIMITER));
						}

						#endregion

						#region Loop through any other attributes and load them into OtherAttributes
						// Loop through any other attributes and load them into OtherAttributes.
						for (int i = 0; i < nodeAttributes.Count; i++)
						{
							publisherSettings.AddOtherAttributes(nodeAttributes.Item(i).Name,nodeAttributes.Item(i).Value);
						}
						#endregion

						// Add the PublisherSettings to the publishers collection.
						settings.Publishers.Add(publisherSettings);
					}
				}

				// Remove extra allocated space of the ArrayList of Publishers. 
				settings.Publishers.TrimToSize();

				#endregion

				// Return the ExceptionSettings loaded with the values from the config file.
				return settings;
			}
			catch(Exception exc)
			{
				throw new ConfigurationException(resourceManager.GetString("RES_EXCEPTION_LOADING_CONFIGURATION"), exc, section);
			}
		}
        
        #endregion

        #region Private Methods

		/// <summary>
		/// Creates TypeFilter with type information from the string array of type names.
		/// </summary>
		/// <param name="rawFilter">String array containing names of types to be included in the filter.</param>
		/// <returns>TypeFilter object containing type information.</returns>
		private TypeFilter LoadTypeFilter(string[] rawFilter)
		{
			// Initialize filter
			TypeFilter typeFilter = new TypeFilter();

			// Verify information was passed in
			if (rawFilter != null) 
			{
				TypeInfo exceptionTypeInfo;
		
				// Loop through the string array
				for (int i=0;i<rawFilter.GetLength(0);i++)
				{
					// If the wildcard character "*" exists set the TypeFilter to accept all types.
					if (rawFilter[i] == "*") 
					{
						typeFilter.AcceptAllTypes = true;
					}

					else
					{
						try
						{
							if (rawFilter[i].Length > 0)
							{
								// Create the TypeInfo class
								exceptionTypeInfo = new TypeInfo();

								// If the string starts with a "+"
								if (rawFilter[i].Trim().StartsWith("+")) 
								{
									// Set the TypeInfo class to include subclasses
									exceptionTypeInfo.IncludeSubClasses = true;
									// Get the Type class from the filter privided.
									exceptionTypeInfo.ClassType = Type.GetType(rawFilter[i].Trim().TrimStart(Convert.ToChar("+")),true);
									
								}
								else
								{
									// Set the TypeInfo class not to include subclasses
									exceptionTypeInfo.IncludeSubClasses = false;
									// Get the Type class from the filter privided.
									exceptionTypeInfo.ClassType = Type.GetType(rawFilter[i].Trim(),true);	
																
								}

								// Add the TypeInfo class to the TypeFilter
								typeFilter.Types.Add(exceptionTypeInfo);
							}
						}
						catch(TypeLoadException e)
						{
							// If the Type could not be created throw a configuration exception.
							ExceptionManager.PublishInternalException(new ConfigurationException(resourceManager.GetString("RES_EXCEPTION_LOADING_CONFIGURATION"), e), null);
						}													
					}
				}
			}

			return typeFilter;
		}
        
        #endregion
	}
}
