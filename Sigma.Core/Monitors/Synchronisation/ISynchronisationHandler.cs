﻿/* 
MIT License

Copyright (c) 2016-2017 Florian Cäsar, Michael Plainer

For full license see LICENSE in the root directory of this project. 
*/

using System;
using Sigma.Core.Utils;

namespace Sigma.Core.Monitors.Synchronisation
{
	/// <summary>
	/// A handler that is responsible for syncing values between monitors and the training process.
	/// </summary>
	public interface ISynchronisationHandler
	{
		/// <summary>
		/// The environment this handler is associated with.
		/// </summary>
		SigmaEnvironment Sigma { get; }

		/// <summary>
		/// Indicate that a value has changed and synchronise it with the given <see cref="SigmaEnvironment"/>.
		/// 
		/// This method returns immediately - callbacks should be used if required.
		/// </summary>
		/// <typeparam name="T">The type of the value that has changed.</typeparam>
		/// <param name="registry">The registry in which the entry will be set.</param>
		/// <param name="key">The fully resolved identifier for the parameter that has changed.</param>
		/// <param name="val">The new value of the parameter.</param>
		/// <param name="onSuccess">The method that will be called when the parameter has been successfully updated.</param>
		/// <param name="onError">The method that will be called when the parameter could not be updated.</param>
		void SynchroniseSet<T>(IRegistry registry, string key, T val, Action<T> onSuccess = null, Action<Exception> onError = null);

		/// <summary>
		/// Get a value from the <see cref="SigmaEnvironment"/>.
		/// </summary>
		/// <param name="registry">The registry in which the entry will be set.</param>
		/// <typeparam name="T">The type of the value that will be gathered.</typeparam>
		/// <param name="key">The fully resolved identifier for the parameter that will be received.</param>
		T SynchroniseGet<T>(IRegistry registry, string key);

		/// <summary>
		///	Update a value with a given action if it has changed (not <see cref="object.Equals(object)"/>).
		/// </summary>
		/// <typeparam name="T">The type of the value that will be gathered.</typeparam>
		/// <param name="registry">The registry in which the entry will be set.</param>
		/// <param name="key">The fully resolved identifier for the parameter that will be received.</param>
		/// <param name="currentVal">The current value of the object.</param>
		/// <param name="update">The method that will be called if the parameter has to be updated.</param>
		void SynchroniseUpdate<T>(IRegistry registry, string key, T currentVal,Action<T> update);
	}
}