﻿using System;

namespace Kernel
{
    /// <summary>
    /// Contains plugged methos that are pre-requisites for the kernel to boot.
    /// For example, the Multiboot Signature.
    /// </summary>
    [Compiler.PluggedClass]
    public static class PreReqs
    {
        //These methods are listed in the order they are included
        //Most of them are not callable methods - so don't touch/try!
        //Most of them, the hard-coded labels don't conform to compiler-standard labels
        //If they are public, you can actually call them 
        //      - Reset method will never return
        //      - Methods which are public must have a compiler-standard label in the 
        //          hard-coded asm so they can be called by the compiler generated code

        /// <summary>
        /// Inserts the multiboot signature at the start of the file.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\PreReqs\MultibootSignature")]
        [Compiler.SequencePriority(Priority = long.MinValue)]
        private static void MultibootSignature()
        {
        }

        /// <summary>
        /// Inserts the pre-entrypoint kernel start method plug.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\PreReqs\Kernel_Start")]
        [Compiler.SequencePriority(Priority = long.MinValue + 1)]
        private static void Kernel_Start()
        {
        }

        /// <summary>
        /// Inserts the initialise stack code. 
        /// Kernel stack space is currently hard-coded into the 
        /// Multiboot Signature asm.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\PreReqs\InitStack")]
        [Compiler.SequencePriority(Priority = long.MinValue + 2)]
        private static void InitStack()
        {
        }

        /// <summary>
        /// Initialises the Global Descriptor Table.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Descriptor Tables\GDT")]
        [Compiler.SequencePriority(Priority = long.MinValue + 3)]
        private static void InitGDT()
        {
        }

        /// <summary>
        /// Initialises the Interrupt Descriptor Table.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Descriptor Tables\IDT")]
        [Compiler.SequencePriority(Priority = long.MinValue + 4)]
        private static void InitIDT()
        {
        }

        /// <summary>
        /// Initialises CPU SSE commands (i.e. allows them to be used).
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\PreReqs\SSEInit")]
        [Compiler.SequencePriority(Priority = long.MinValue + 5)]
        private static void SSEInit()
        {
        }

        /// <summary>
        /// Inserts the stub that calls the main kernel entrypoint.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\PreReqs\MainEntrypoint")]
        [Compiler.SequencePriority(Priority = long.MinValue + 6)]
        private static void MainEntrypoint()
        {
        }

        /// <summary>
        /// Resets the OS / CPU / etc. i.e. terminates the OS
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\PreReqs\Reset")]
        [Compiler.SequencePriority(Priority = long.MinValue + 7)]
        public static void Reset()
        {
        }

        /// <summary>
        /// Inserts the method that handles what happens when the Multiboot
        /// Signature is invalid or undetected.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\PreReqs\HandleNoMultiboot")]
        [Compiler.SequencePriority(Priority = long.MinValue + 8)]
        private static void HandleNoMultiboot()
        {
        }

        /// <summary>
        /// Writes a piece of text to the first line of the screen
        /// </summary>
        /// <param name="aText">The text to write. First dword should be the length of the string. (Inserted by compiler for string literals)</param>
        /// <param name="aColour">The foreground/background (DOS) colour to write in - 0xXY where X is background colour and Y is foreground colour.</param>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\PreReqs\WriteDebugVideo")]
        [Compiler.SequencePriority(Priority = long.MinValue + 9)]
        public static void WriteDebugVideo(string aText, UInt32 aColour)
        {
        }
    }
}