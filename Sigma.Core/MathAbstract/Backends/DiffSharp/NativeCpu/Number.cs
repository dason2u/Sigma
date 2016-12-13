﻿using System;
using Sigma.Core.Handlers;

namespace Sigma.Core.MathAbstract.Backends.DiffSharp.NativeCpu
{
	/// <summary>
	/// A default implementation of the <see cref="INumber"/> interface.
	/// Represents single mathematical value (i.e. number), used for interaction between ndarrays and handlers (is more expressive and faster). 
	/// </summary>
	/// <typeparam name="T">The data type of this single value.</typeparam>
	public class Number<T> : INumber
	{
		private T _value;

		[NonSerialized]
		private IComputationHandler _associatedHandler;

		public IComputationHandler AssociatedHandler
		{
			get { return _associatedHandler; }
			set { _associatedHandler = value; }
		}

		/// <summary>
		/// Create a single value (i.e. number) with a certain initial value.
		/// </summary>
		/// <param name="value">The initial value to wrap.</param>
		public Number(T value)
		{
			_value = value;
		}

		public object Value
		{
			get { return _value; }
			set { _value = (T) value; }
		}

		internal Number<T> SetAssociatedHandler(IComputationHandler handler)
		{
			AssociatedHandler = handler;

			return this;
		}

		public object DeepCopy()
		{
			return new Number<T>(_value).SetAssociatedHandler(AssociatedHandler);
		}
	}
}