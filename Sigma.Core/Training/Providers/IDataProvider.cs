﻿/* 
MIT License

Copyright (c) 2016-2017 Florian Cäsar, Michael Plainer

For full license see LICENSE in the root directory of this project. 
*/

using System;
using System.Collections.Generic;
using Sigma.Core.Layers;
using Sigma.Core.MathAbstract;
using Sigma.Core.Utils;

namespace Sigma.Core.Training.Providers
{
	/// <summary>
	/// A data provider which provides external inputs with data and provides external listeners with external output data.
	/// </summary>
	public interface IDataProvider
	{
		// TODO rename links to bindings, it's more fitting (also in DefaultDataProvider)

		/// <summary>
		/// The external input links attached to this provider.
		/// </summary>
		IReadOnlyDictionary<string, Action<IRegistry, ILayer, IDictionary<string, INDArray>>> ExternalInputLinks { get; }

		/// <summary>
		/// The external output links attached to this provider.
		/// </summary>
		IReadOnlyDictionary<string, Action<IRegistry, ILayer, IDictionary<string, INDArray>>> ExternalOutputLinks { get; }

		/// <summary>
		/// Set an external input link for a certain external input alias to a link action.
		/// </summary>
		/// <param name="externalInputAlias">The external input alias to attach the link to.</param>
		/// <param name="linkAction">The link action to execute that supplies the registry with input data for that layer and input alias.</param>
		void SetExternalInputLink(string externalInputAlias, Action<IRegistry, ILayer, IDictionary<string, INDArray>> linkAction);

		/// <summary>
		/// Remove an external input link with a certain external input alias.
		/// </summary>
		/// <param name="externalInputAlias">The external input alias to detach.</param>
		void RemoveExternalInputLink(string externalInputAlias);

		/// <summary>
		/// Set an external output link for a certain external output alias to a link action.
		/// </summary>
		/// <param name="externalOutputAlias">The external output alias to attach the link to.</param>
		/// <param name="linkAction">The link action to execute that supplies the registry with output data for that layer and output alias.</param>
		void SetExternalOutputLink(string externalOutputAlias, Action<IRegistry, ILayer, IDictionary<string, INDArray>> linkAction);

		/// <summary>
		/// Remove an external output link with a certain external output alias.
		/// </summary>
		/// <param name="externalOutputAlias">The external output alias to detach.</param>
		void RemoveExternalOutputLink(string externalOutputAlias);

		/// <summary>
		/// Provide the external input for a certain layer's external input registry. 
		/// </summary>
		/// <param name="externalInputAlias">The alias of the external input. Typically indicates the type of input data to set.</param>
		/// <param name="inputRegistry">The input registry in which to set the external inputs.</param>
		/// <param name="layer">The layer the external input is attached to.</param>
		/// <param name="currentTrainingBlock">The current training block (as provided by the training data iterator).</param>
		void ProvideExternalInput(string externalInputAlias, IRegistry inputRegistry, ILayer layer, IDictionary<string, INDArray> currentTrainingBlock);

		/// <summary>
		/// Provide the external output from a certain layer's external output registry.
		/// </summary>
		/// <param name="externalOutputAlias">The alias of the external output. Typically indicates the type of output data that was set.</param>
		/// <param name="outputRegistry">The output registry from which to get the external outputs.</param>
		/// <param name="layer">The layer the external output is attached to.</param>
		/// <param name="currentTrainingBlock">The current training block (as provided by the training data iterator).</param>
		void ProvideExternalOutput(string externalOutputAlias, IRegistry outputRegistry, ILayer layer, IDictionary<string, INDArray> currentTrainingBlock);
	}
}
