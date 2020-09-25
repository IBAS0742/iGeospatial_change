using System;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Resources;
using System.Reflection;

namespace iGeospatial.Exceptions
{
	/// <summary>
	/// Installer class used to create two event sources for the 
	/// Exception Management Application Block to function correctly.
	/// </summary>
	[RunInstaller(true)]
	public class ExceptionManagerInstaller : Installer
	{
		private EventLogInstaller m_objManagerInstaller;
		private EventLogInstaller m_objManagementInstaller;
		
		private static ResourceManager resourceManager 
            = new ResourceManager(typeof(ExceptionManager).Namespace + ".ExceptionManagerText", 
            Assembly.GetAssembly(typeof(ExceptionManager)));
		
		/// <summary>
		/// Constructor with no params.
		/// </summary>
		public ExceptionManagerInstaller()
		{
			// Initialize variables.
			InitializeComponent();
		}

		/// <summary>
		/// Initialization function to set internal variables.
		/// </summary>
		private void InitializeComponent()
		{
			m_objManagerInstaller    = new EventLogInstaller();
			m_objManagementInstaller = new EventLogInstaller();
			
            // 
			// m_objManagerInstaller
			// 
			m_objManagerInstaller.Log    = "iGeospatial";
			m_objManagerInstaller.Source = resourceManager.GetString("RES_EXCEPTIONMANAGER_INTERNAL_EXCEPTIONS");
			
            // 
			// m_objManagementInstaller
			// 
			m_objManagementInstaller.Log    = "iGeospatial";
			m_objManagementInstaller.Source = resourceManager.GetString("RES_EXCEPTIONMANAGER_PUBLISHED_EXCEPTIONS");

			this.Installers.AddRange(new Installer[] {
						this.m_objManagerInstaller,
						this.m_objManagementInstaller});
		}
	}
}

