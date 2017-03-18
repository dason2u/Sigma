﻿/* 
MIT License

Copyright (c) 2016-2017 Florian Cäsar, Michael Plainer

For full license see LICENSE in the root directory of this project. 
*/

using System.Windows.Controls;
using Sigma.Core.Monitors.Synchronisation;
using Sigma.Core.Monitors.WPF.ViewModel.Parameterisation;
using Sigma.Core.Utils;

namespace Sigma.Core.Monitors.WPF.View.Parameterisation
{
	/// <summary>
	/// This class is a wrapper or <see cref="IParameterVisualiser"/>. It automatically implements
	/// code that is global for all <see cref="UserControl"/>s.
	/// 
	/// When extending from this class you have to call InitializeComponent manually.
	/// </summary>
	public abstract class UserControlParameterVisualiser : UserControl, IParameterVisualiser
	{
		/// <summary>
		/// Determines whether the parameter is editable or not. 
		/// </summary>
		public abstract bool IsReadOnly { get; set; }

		/// <summary>
		/// The fully resolved key to access the synchandler.
		/// </summary>
		public abstract string Key { get; set; }

		/// <summary>
		/// The registry for which the visualiser displays values. (e.g. operators registry)
		/// </summary>
		public abstract IRegistry Registry { get; set; }

		/// <summary>
		/// The SynchronisationHandler that is used to sync the parameter with the training process.
		/// </summary>
		public abstract ISynchronisationHandler SynchronisationHandler { get; set; }

		/// <summary>
		/// Force the visualiser to update its value (i.e. display the value that is stored).
		/// </summary>
		public abstract void Read();

		/// <summary>
		/// Force the visualiser to store its value (i.e. write the value that is displayed to the registry).
		/// </summary>
		public abstract void Write();

		/// <summary>
		/// This boolean determines whether there are unsaved changes or not.
		/// <c>True</c> if there are other changes, <c>false</c> otherwise.
		/// </summary>
		public abstract bool Pending { get; set; }

		/// <summary>
		/// This boolean determines whether a synchronisation erroered or not.
		/// <c>True</c> if there are errors, <c>false</c> otherwise.
		/// </summary>
		public abstract bool Errored { get; set; }
	}
}
