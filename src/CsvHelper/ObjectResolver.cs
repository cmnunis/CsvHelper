﻿// Copyright 2009-2017 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System;

namespace CsvHelper
{
	/// <summary>
	/// Creates objects from a given type.
	/// </summary>
	public class ObjectResolver : IObjectResolver
	{
		private static readonly object locker = new object();
		private static IObjectResolver current = new ObjectResolver( type => true, ReflectionHelper.CreateInstanceWithoutContractResolver );

		/// <summary>
		/// A value indicating if the resolver's <see cref="CanResolve"/>
		/// returns false that an object will still be created using
		/// CsvHelper's object creation. True to fallback, otherwise false.
		/// Default value is true.
		/// </summary>
		public bool WillFallback { get; private set; }

		/// <summary>
		/// A value indicating if the resolver is able to resolve
		/// the given type. True if the type can be resolved,
		/// otherwise false.
		/// </summary>
		public Func<Type, bool> CanResolve { get; private set; }

		/// <summary>
		/// The function that creates an object from a given type.
		/// </summary>
		public Func<Type, object[], object> ResolveFunction { get; private set; }
		
		/// <summary>
		/// Creates an instance of the object resolver using default values.
		/// </summary>
		public ObjectResolver() : this( type => true, ReflectionHelper.CreateInstanceWithoutContractResolver ) { }

		/// <summary>
		/// Creates an instance of the object resolver using the given can create function
		/// and creat function.
		/// </summary>
		/// <param name="canResolve"></param>
		/// <param name="resolveFunction"></param>
		/// <param name="willFallback"></param>
		public ObjectResolver( Func<Type, bool> canResolve, Func<Type, object[], object> resolveFunction, bool willFallback = true )
		{
			CanResolve = canResolve ?? throw new ArgumentNullException( nameof( canResolve ) );
			ResolveFunction = resolveFunction ?? throw new ArgumentNullException( nameof( resolveFunction ) );
			WillFallback = willFallback;
		}

		/// <summary>
		/// Creates an object from the given type using the <see cref="ResolveFunction"/>
		/// function. If <see cref="CanResolve"/> is false, the object will be
		/// created using CsvHelper's default object creation. If <see cref="WillFallback"/>
		/// is false, an exception is thrown.
		/// </summary>
		/// <param name="type">The type to create an instance from. The created object
		/// may not be the same type as the given type.</param>
		/// <param name="constructorArgs">Constructor arguments used to create the type.</param>
		public object Resolve( Type type, object[] constructorArgs = null )
		{
			if( CanResolve( type ) )
			{
				return ResolveFunction( type, constructorArgs );
			}

			if( WillFallback )
			{
				return ReflectionHelper.CreateInstance( type, constructorArgs );
			}

			var message = $"Type '{type.FullName}' can't be resolved and fallback is turned off.";
			throw new CsvHelperException( message );
		}

		/// <summary>
		/// Gets or sets the current resolver.
		/// </summary>
		public static IObjectResolver Current
		{
			get
			{
				lock( locker )
				{
					return current;
				}
			}
			set
			{
				if( value == null )
				{
					throw new InvalidOperationException( "IObjectResolver cannot be null." );
				}

				lock( locker )
				{
					current = value;
				}
			}
		}
	}
}
