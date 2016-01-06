﻿#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldobj : IL.ILOps.Ldobj
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            Type theType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
            Types.TypeInfo theTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theType);
            int size = theTypeInfo.SizeOnStackInBytes;

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = size,
                isGCManaged = false,
                isValue = theTypeInfo.IsValueType
            });
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when loading a static float field.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Load the metadata token used to get the type info
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            //Get the type info for the object to load
            Type theType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
            Types.TypeInfo theTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theType);

            //Get the object size information
            int memSize = theTypeInfo.IsValueType ? theTypeInfo.SizeOnHeapInBytes : theTypeInfo.SizeOnStackInBytes;

            //Load the object onto the stack
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "ECX" });
            for (int i = memSize - 4; i >= 0; i -= 4)
            {
                switch (i)
                {
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    default:
                        conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[ECX+" + i.ToString() + "]", Dest = "EAX" });
                        conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "EAX" });
                        break;
                }
            }
            int paddingSize = 4 - (memSize % 4);
            switch (paddingSize)
            {
                case 1:
                    conversionState.Append(new ASMOps.Sub() { Dest = "ESP", Src = "1" });
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Dest = "[ESP]", Src = "0" });
                    break;
                case 2:
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "0" });
                    break;
                case 3:
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "0" });
                    conversionState.Append(new ASMOps.Add() { Dest = "ESP", Src = "1" });
                    break;
            }

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = memSize,
                isGCManaged = false,
                isValue = theTypeInfo.IsValueType
            });
        }
    }
}
