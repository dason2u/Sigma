﻿/* 
MIT License

Copyright (c) 2016 Florian Cäsar, Michael Plainer

For full license see LICENSE in the root directory of this project. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sigma.Core.Handlers;
using Sigma.Core.Math;
using System.ComponentModel;
using log4net;
using Sigma.Core.Utils;

namespace Sigma.Core.Data.Extractors
{
	/// <summary>
	/// A byte record extractor, which extracts named ranges to ndarrays bytewise.
	/// </summary>
	public class ByteRecordExtractor : BaseExtractor
	{
		private ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public override string[] SectionNames { get; protected set; }
		private Dictionary<string, long[][]> indexMappings;

		/// <summary>
		/// Create a byte record extractor with a certain named index mapping.
		/// </summary>
		/// <param name="indexMappings">The named index mapping.</param>
		public ByteRecordExtractor(Dictionary<string, long[][]> indexMappings)
		{
			if (indexMappings == null)
			{
				throw new ArgumentNullException("Index mappings cannot be null.");
			}

			foreach (string name in indexMappings.Keys)
			{
				if (indexMappings[name].Length % 2 != 0)
				{
					throw new ArgumentException($"All index mapping arrays have to be a multiple of 2 (start and end indices of each range), but index mapping for name {name} had {indexMappings[name].Length}.");
				}
			}

			this.indexMappings = indexMappings;
			this.SectionNames = indexMappings.Keys.ToArray();
		}

		public override Dictionary<string, INDArray> ExtractDirect(int numberOfRecords, IComputationHandler handler)
		{
			if (Reader == null)
			{
				throw new InvalidOperationException("Cannot extract from record extractor before attaching a reader (reader was null).");
			}

			if (handler == null)
			{
				throw new ArgumentNullException("Computation handler cannot be null.");
			}

			if (numberOfRecords <= 0)
			{
				throw new ArgumentException($"Number of records to read must be > 0 but was {numberOfRecords}.");
			}

			return ExtractFrom(Reader.Read(numberOfRecords), numberOfRecords, handler);
		}

		public override Dictionary<string, INDArray> ExtractFrom(object readData, int numberOfRecords, IComputationHandler handler)
		{
			// read data being null means no more data could be read so we will just pass that along
			if (readData == null)
			{
				return null;
			}

			byte[][] rawRecords = (byte[][]) readData;

			int numberOfRecordsRead = rawRecords.Length;

			logger.Info($"Extracting {numberOfRecordsRead} records from reader {Reader} (requested: {numberOfRecords})...");

			Dictionary<string, INDArray> namedArrays = new Dictionary<string, INDArray>();

			foreach (string name in indexMappings.Keys)
			{
				long[][] mappings = indexMappings[name];
				long[][] perMappingShape = new long[mappings.Length / 2][];
				long[] perMappingLength = new long[mappings.Length / 2];
				long[] featureShape = new long[mappings[0].Length];

				for (int i = 0; i < mappings.Length; i += 2)
				{
					int halfIndex = i / 2;
					perMappingShape[halfIndex] = new long[mappings[0].Length];

					for (int y = 0; y < featureShape.Length; y++)
					{
						perMappingShape[halfIndex][y] = mappings[i + 1][y] - mappings[i][y];
						featureShape[y] += perMappingShape[halfIndex][y];
					}

					perMappingLength[i / 2] = ArrayUtils.Product(perMappingShape[halfIndex]);
				}

				long[] shape = new long[featureShape.Length + 2];

				shape[0] = numberOfRecordsRead;
				shape[1] = 1;

				Array.Copy(featureShape, 0, shape, 2, featureShape.Length);

				Console.WriteLine(ArrayUtils.ToString(featureShape));

				INDArray array = handler.Create(shape);
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(double));

				long[] globalBufferIndices = new long[shape.Length];

				for (int r = 0; r < numberOfRecordsRead; r++)
				{
					byte[] record = rawRecords[r];

					globalBufferIndices[0] = r; //BatchTimeFeatures indexing
					globalBufferIndices[1] = 0;

					for (int i = 0; i < mappings.Length; i += 2)
					{
						long[] beginShape = mappings[i];
						long[] localShape = perMappingShape[i / 2];
						long[] localStrides = NDArray<byte>.GetStrides(localShape);
						long[] localBufferIndices = new long[mappings[i].Length];
						long length = perMappingLength[i / 2];
						long beginFlatIndex = ArrayUtils.Product(beginShape);

						for (int y = 0; y < length; y++)
						{
							localBufferIndices = NDArray<byte>.GetIndices(y, localShape, localStrides, localBufferIndices);
							localBufferIndices = ArrayUtils.Add(beginShape, localBufferIndices, localBufferIndices);

							Array.Copy(localBufferIndices, 0, globalBufferIndices, 2, localBufferIndices.Length);

							array.SetValue<byte>(record[beginFlatIndex + y], globalBufferIndices);
						}
					}
				}

				namedArrays.Add(name, array);
			}

			return namedArrays;
		}

		public override void Dispose()
		{
		}
	}
}