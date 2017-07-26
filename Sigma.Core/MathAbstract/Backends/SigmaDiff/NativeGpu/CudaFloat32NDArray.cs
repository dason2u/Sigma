﻿/* 
MIT License

Copyright (c) 2016-2017 Florian Cäsar, Michael Plainer

For full license see LICENSE in the root directory of this project. 
*/

using System;
using System.Linq;
using DiffSharp;
using DiffSharp.Interop.Float32;
using Sigma.Core.Data;
using Sigma.Core.Handlers.Backends.SigmaDiff;
using Sigma.Core.Handlers.Backends.SigmaDiff.NativeGpu;
using Sigma.Core.MathAbstract.Backends.SigmaDiff.NativeCpu;
using Sigma.Core.Utils;

namespace Sigma.Core.MathAbstract.Backends.SigmaDiff.NativeGpu
{
	/// <summary>
	/// An ndarray with a float32 CUDA-based in-GPU-memory backend Sigma.DiffSharp handle for tracing and AD operations.
	/// </summary>
	public class CudaFloat32NDArray : ADNDArray<float>
	{
		public DNDArray _adArrayHandle;
		private CudaSigmaDiffDataBuffer<float> _underlyingCudaBuffer;

		public CudaFloat32NDArray(long backendTag, params long[] shape) : this(new DNDArray(new SigmaDiffDataBuffer<float>(ArrayUtils.Product(shape), backendTag), NDArrayUtils.CheckShape(shape)))
		{
		}

		public CudaFloat32NDArray(long backendTag, float[] data, params long[] shape) : this(new DNDArray(new SigmaDiffDataBuffer<float>(data, backendTag), NDArrayUtils.CheckShape(shape)))
		{
		}

		public CudaFloat32NDArray(DNDArray adArrayHandle) : base(CheckCudaBuffer(adArrayHandle.Buffer.DataBuffer), adArrayHandle.Buffer.Shape)
		{
			if (adArrayHandle == null) throw new ArgumentNullException(nameof(adArrayHandle));

			_adArrayHandle = adArrayHandle;
			_adArrayHandle.Buffer.Shape = Shape;
			_underlyingCudaBuffer = (CudaSigmaDiffDataBuffer<float>) Data;
		}

		private static IDataBuffer<float> CheckCudaBuffer(Util.ISigmaDiffDataBuffer<float> dataBuffer)
		{
			if (!(dataBuffer is CudaSigmaDiffDataBuffer<float>))
			{
				throw new InvalidOperationException($"Data buffer passed to {nameof(CudaFloat32NDArray)} must be of type {nameof(CudaSigmaDiffDataBuffer<float>)} but was of type {dataBuffer.GetType()}.");
			}

			return (IDataBuffer<float>) dataBuffer;
		}

		public CudaFloat32NDArray(long backendTag, IDataBuffer<float> buffer, long[] shape) : this(new DNDArray(new SigmaDiffDataBuffer<float>(buffer, 0, buffer.Length, backendTag), NDArrayUtils.CheckShape(shape)))
		{
		}

		protected override void Reinitialise(long[] shape, long[] strides)
		{
			_adArrayHandle = _adArrayHandle.ShallowCopy();

			base.Reinitialise(shape, strides);

			_adArrayHandle.Buffer.Shape = shape;
		}

		/// <summary>
		/// Get a slice of this ndarray of a certain region as a new ndarray with the same underlying data.
		/// </summary>
		/// <param name="beginIndices">The begin indices (inclusively, where the slice should begin).</param>
		/// <param name="endIndices">The end indices (exclusively, where the slice should end).</param>
		/// <returns></returns>
		public override INDArray Slice(long[] beginIndices, long[] endIndices)
		{
			long[] slicedShape = GetSlicedShape(beginIndices, endIndices);

			//we want the end indices to be inclusive for easier handling
			endIndices = endIndices.Select(i => i - 1).ToArray();

			long absoluteBeginOffset = NDArrayUtils.GetFlatIndex(Shape, Strides, beginIndices);
			long absoluteEndOffset = NDArrayUtils.GetFlatIndex(Shape, Strides, endIndices);
			long length = absoluteEndOffset - absoluteBeginOffset + 1;

			return new CudaFloat32NDArray(new DNDArray(new CudaSigmaDiffDataBuffer<float>(Data, absoluteBeginOffset, length, ((SigmaDiffDataBuffer<float>)Data).BackendTag, _underlyingCudaBuffer.CudaContext), slicedShape));
		}

		public override INDArray Reshape(params long[] newShape)
		{
			if (Length != ArrayUtils.Product(newShape))
			{
				throw new ArgumentException("Reshaping cannot change total ndarray length, only array shape.");
			}

			return new CudaFloat32NDArray(DNDArray.Reshape(_adArrayHandle, newShape));
		}

		public override object DeepCopy()
		{
			return new CudaFloat32NDArray(_adArrayHandle.DeepCopy());
		}
	}
}