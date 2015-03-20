﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace LoyaltyService.Common
{
	/// <summary>
	/// wraps a value, representing that it can either have a value, or can have no value.
	/// </summary>
	/// <typeparam name="T">The type of the value that Maybe will wrap.</typeparam>
	public struct Maybe<T> : IEnumerable<T>, IEquatable<Maybe<T>>
	{
		/// <summary>
		/// The empty instance.
		/// </summary>
		public static readonly Maybe<T> Empty = new Maybe<T>();

		readonly bool _hasValue;
		readonly T _value;

		/// <summary>
		/// Initializes a new <see cref="Optional{T}"/> instance.
		/// </summary>
		/// <param name="value">The value to initialize with.</param>
		public Maybe(T value)
		{
			_hasValue = true;
			_value = value;
		}

		/// <summary>
		/// Gets an indication if this instance has a value.
		/// </summary>
		public bool HasValue
		{
			get { return _hasValue; }
		}

		/// <summary>
		/// Gets the value associated with this instance.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when this instance has no value.</exception>
		public T Value
		{
			get
			{
				if (!HasValue)
					throw new InvalidOperationException("There is no value associated with this instance.");
				return _value;
			}
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			if (HasValue)
			{
				yield return _value;
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, null)) return false;
			if (GetType() != obj.GetType()) return false;
			return Equals((Maybe<T>) obj);
		}

		/// <summary>
		/// Determines whether the specified <see cref="Optional{T}" /> is equal to this instance.
		/// </summary>
		/// <param name="other">The <see cref="Optional{T}" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="Optional{T}" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals(Maybe<T> other)
		{
			return _hasValue.Equals(other._hasValue) &&
				EqualityComparer<T>.Default.Equals(_value, other._value);
		}

		/// <summary>
		/// Determines whether <see cref="Optional{T}">instance 1</see> is equal to <see cref="Optional{T}">instance 2</see>.
		/// </summary>
		/// <param name="instance1">The first instance.</param>
		/// <param name="instance2">The second instance.</param>
		/// <returns><c>true</c> if <see cref="Optional{T}">instance 1</see> is equal to <see cref="Optional{T}">instance 2</see>; otherwise, <c>false</c>.</returns>
		public static bool operator ==(Maybe<T> instance1, Maybe<T> instance2)
		{
			return instance1.Equals(instance2);
		}

		/// <summary>
		/// Determines whether <see cref="Optional{T}">instance 1</see> is not equal to <see cref="Optional{T}">instance 2</see>.
		/// </summary>
		/// <param name="instance1">The first instance.</param>
		/// <param name="instance2">The second instance.</param>
		/// <returns><c>true</c> if <see cref="Optional{T}">instance 1</see> is not equal to <see cref="Optional{T}">instance 2</see>; otherwise, <c>false</c>.</returns>
		public static bool operator !=(Maybe<T> instance1, Maybe<T> instance2)
		{
			return !instance1.Equals(instance2);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance. 
		/// </returns>
		public override int GetHashCode()
		{
			return _hasValue.GetHashCode() ^ EqualityComparer<T>.Default.GetHashCode(_value) ^ typeof (T).GetHashCode();
		}
	}
}

