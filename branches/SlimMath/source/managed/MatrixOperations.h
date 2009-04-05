/*
* Copyright (c) 2007-2009 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
#pragma once

#include "IOperation.h"
#include "Primitives.h"

namespace SlimMath
{
	public ref class MatrixOps sealed
	{
	private:
		MatrixOps() { }

	public:
		static Operation<Matrix>^ Multiply(Handle<Matrix>^ value1, Handle<Matrix>^ value2);
		static Operation<Matrix>^ Identity();
		static CompoundOperation^ Inverse(Handle<Matrix>^ matrix);

		/*ref class Multiply : IOperation
		{
		private:
			Handle^ handle;
			float* data;
		public:
			Multiply( Matrix value1, Matrix value2 );
			Multiply( Handle^ value1, Matrix value2, int resultIndex );
			Multiply( Matrix value1, Handle^ value2, int resultIndex );
			Multiply( Handle^ value1, Handle^ value2, int firstHandleResultIndex, int secondHandleResultIndex );
			!Multiply() { delete [] data; data = 0; }
			~Multiply() { delete [] data; data = 0; }
			virtual Handle^ GetHandle() { return handle; }
			
		};
		
		ref class Identity : IOperation
		{
		private:
			Handle^ handle;
			float* data;
		public:
			Identity();
			!Identity() { delete [] data; data = 0; }
			~Identity() { delete [] data; data = 0; }
			virtual Handle^ GetHandle() { return handle; }
		};

		ref class Inverse : IOperation
		{
		private:
			Handle^ handle;
			float* data;
		public:
			Inverse(Matrix matrix);
			Inverse(Handle^ handle, int resultIndex);
			~Inverse() { delete [] data; data = 0; }
			!Inverse() { delete [] data; data = 0; }
			virtual Handle^ GetHandle() { return handle; }
		};*/
	};
}