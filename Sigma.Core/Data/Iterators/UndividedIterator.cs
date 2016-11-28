﻿/* 
MIT License

Copyright (c) 2016 Florian Cäsar, Michael Plainer

For full license see LICENSE in the root directory of this project. 
*/

using Sigma.Core.Data.Datasets;
using Sigma.Core.Handlers;
using Sigma.Core.MathAbstract;
using System.Collections.Generic;

namespace Sigma.Core.Data.Iterators
{
	/// <summary>
	/// An undivided data iterator which yields blocks from datasets as a whole.
	/// </summary>
	public class UndividedIterator : BaseIterator
	{
		public UndividedIterator(IDataset dataset) : base(dataset)
		{
		}

		public override IEnumerable<IDictionary<string, INDArray>> Yield(IComputationHandler handler, SigmaEnvironment environment)
		{
			CheckNotNull(handler, environment);

			int currentIndex = 0;

			while (true)
			{
				RequireBlocks(handler, currentIndex);

				if (_fetchedBlocks[currentIndex] == null)
				{
					break;
				}

				PrepareBlocksAsync(handler, currentIndex + 1);

				var currentBlock = _fetchedBlocks[currentIndex];

				yield return currentBlock;

				UnderlyingDataset.FreeBlock(currentIndex, handler);
				currentIndex++;
			}
		}
	}
}