﻿/* 
MIT License

Copyright (c) 2016-2017 Florian Cäsar, Michael Plainer

For full license see LICENSE in the root directory of this project. 
*/

using Sigma.Core.Data.Datasets;
using Sigma.Core.Handlers;
using Sigma.Core.MathAbstract;
using System.Collections.Generic;
using Sigma.Core.Utils;

namespace Sigma.Core.Data.Iterators
{
	/// <summary>
	/// A data iterator which iterates over a dataset in blocks. 
	/// Note: The blocks yielded are in the BatchTimeFeatures format, where Batch and Time are one-dimensional.
	/// </summary>
	public interface IDataIterator
	{
		/// <summary>
		/// The dataset underlying this data iterator.
		/// </summary>
		IDataset UnderlyingDataset { get; }

		/// <summary>
		/// A registry containing relevant parameters of and information about this data iterator.
		/// </summary>
		IRegistry Registry { get; }

		/// <summary>
		/// Yield a block from this data iterator for and with a certain handler within a certain environment.
		/// If no more blocks can be yielded, return null. 
		/// </summary>
		/// <param name="handler">The computation handler to fetch a block for (used for block creation and computations).</param>
		/// <param name="environment">The sigma environment within the block should be yielded.</param>
		/// <returns>An iterator over blocks from the underlying dataset (until the dataset is fully traversed).</returns>
		IEnumerable<IDictionary<string, INDArray>> Yield(IComputationHandler handler, SigmaEnvironment environment);

		/// <summary>
		/// Create a shallow copy of this data iterator (copy relevant properties, keep dataset).
		/// Typically used to provide workers with independent sets of data iterators for the same underlying data.
		/// </summary>
		/// <returns>A shallow copy of this data iterator.</returns>
		IDataIterator ShallowCopy();
	}
}
