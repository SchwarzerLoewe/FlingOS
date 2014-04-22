﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_0"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_1"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_2"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_3"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_4"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_5"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_6"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_7"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_8"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_M1"/> and 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_S"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_0"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_1"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_2"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_3"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_4"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_5"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_6"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_7"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_8"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_M1"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_S"/>
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4_0)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4_1)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4_2)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4_3)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4_4)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4_5)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4_6)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4_7)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4_8)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4_M1)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I4_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_I8)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_R4)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldc_R8)]
    public abstract class Ldc : ILOp
    {
    }
}