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

#define SYSCALLS_TRACE
//#undef SYSCALLS_TRACE

using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Processes;

namespace Kernel.Core.Processes
{
    public static unsafe class SystemCalls
    {
        private static Thread DeferredSystemCallsHandlerThread;
        private static bool DeferredSystemCallsHandlerThread_Awake = false;
        private static bool DeferredSystemCallsHandlerThread_Terminate = false;
        private static Process DeferredSystemCalls_CurrentProcess;
        private static Thread DeferredSystemCalls_CurrentThread;
        private static bool DeferredSystemCalls_WakeCurrentThread = true;

        private static int Int48HandlerId = 0;
        public static void Init()
        {
            if (Int48HandlerId == 0)
            {
                // We want to ignore process state so that we handle the interrupt in the context of
                //  the calling process.
                Int48HandlerId = Hardware.Interrupts.Interrupts.AddISRHandler(48, Int48, null, true, true, "Sys Call");
            }
        }

        private static void Int48(FOS_System.Object state)
        {
            //FOS_System.GC.Enable("System calls : Int48 (1)");
            //FOS_System.Heap.PreventAllocation = false;
            //BasicConsole.WriteLine("WARNING: Enabled GC/Heap inside critical interrupt! (SysCalls: Int48 : 1)");

            //BasicConsole.Write("Sys call ");
            //BasicConsole.Write(ProcessManager.CurrentThread.SysCallNumber);
            //BasicConsole.Write(" : ");
            //BasicConsole.WriteLine(ProcessManager.CurrentProcess.Name);
            //BasicConsole.WriteLine(((FOS_System.String)" > Param1: ") + ProcessManager.CurrentThread.Param1);
            //BasicConsole.WriteLine(((FOS_System.String)" > Param2: ") + ProcessManager.CurrentThread.Param2);
            //BasicConsole.WriteLine(((FOS_System.String)" > Param3: ") + ProcessManager.CurrentThread.Param3);

            //FOS_System.GC.Disable("System calls : Int48 (1)");
            //FOS_System.Heap.PreventAllocation = true;

#if SYSCALLS_TRACE
            BasicConsole.WriteLine();
            BasicConsole.WriteLine("----- Syscall -----");
            BasicConsole.WriteLine(ProcessManager.CurrentProcess.Name);
#endif 

            Process currProcess = ProcessManager.CurrentProcess;
            Thread currThread = ProcessManager.CurrentThread;
            bool switched = false;

#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Getting param values...");
#endif

            uint syscallNumber = currThread.SysCallNumber;
            uint param1 = currThread.Param1;
            uint param2 = currThread.Param2;
            uint param3 = currThread.Param3;

            SystemCallResults result = SystemCallResults.Unhandled;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;

            Process handlerProcess = null;

#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Enumerating processes...");
#endif

            for (int i = 0; i < ProcessManager.Processes.Count; i++)
            {
                handlerProcess = (Process)ProcessManager.Processes[i];
                if (handlerProcess.SyscallsToHandle.IsSet((int)syscallNumber))
                {
                    ProcessManager.SwitchProcess(handlerProcess.Id, ProcessManager.THREAD_DONT_CARE);
                    switched = true;
                    
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Calling handler...");

                    //if (process == null)
                    //{
                    //    BasicConsole.WriteLine(" > process is null?!");
                    //}
                    //else if (process.SyscallHandler == null)
                    //{
                    //    BasicConsole.WriteLine(" > process.SysCallHandler is null?!");
                    //}
#endif

                    result = (SystemCallResults)handlerProcess.SyscallHandler(syscallNumber,
                        param1, param2, param3,
                        ref Return2, ref Return3, ref Return4, 
                        currProcess.Id, currThread.Id);

                    if (result != SystemCallResults.Unhandled)
                    {
#if SYSCALLS_TRACE
                        BasicConsole.WriteLine("Result achieved.");
#endif

                        break;
                    }
                }
            }

            if (switched)
            {
#if SYSCALLS_TRACE
                BasicConsole.WriteLine("Switching back...");
#endif

                ProcessManager.SwitchProcess(currProcess.Id, (int)currThread.Id);
            }
            
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Setting result values...");
#endif

            if (result == SystemCallResults.Deferred)
            {
#if SYSCALLS_TRACE
                BasicConsole.WriteLine("Deferring thread (by indefinite sleep)...");
#endif
                currThread._EnterSleep(Thread.IndefiniteSleep);
            }
            else
            {
                currThread.Return1 = (uint)result;
                currThread.Return2 = Return2;
                currThread.Return3 = Return3;
                currThread.Return4 = Return4;
            }

            if (currThread.TimeToSleep != 0)
            {
#if SYSCALLS_TRACE
                BasicConsole.WriteLine("Updating thread state...");
#endif
                Scheduler.UpdateCurrentState();
            }
            
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Syscall handled.");
            BasicConsole.WriteLine("---------------");
#endif

            //FOS_System.GC.Enable("System calls : Int48 (2)");
            //FOS_System.Heap.PreventAllocation = false;
            //BasicConsole.WriteLine("WARNING: Enabled GC/Heap inside critical interrupt! (SysCalls: Int48 : 2)");

            //BasicConsole.WriteLine(((FOS_System.String)" > Return 1: ") + ProcessManager.CurrentThread.Return1);
            //BasicConsole.WriteLine(((FOS_System.String)" > Return 2: ") + ProcessManager.CurrentThread.Return2);
            //BasicConsole.WriteLine(((FOS_System.String)" > Return 3: ") + ProcessManager.CurrentThread.Return3);
            //BasicConsole.WriteLine(((FOS_System.String)" > Return 4: ") + ProcessManager.CurrentThread.Return4);

            //FOS_System.GC.Disable("System calls : Int48 (2)");
            //FOS_System.Heap.PreventAllocation = true;
        }

        public static SystemCallResults HandleSystemCallForKernel(uint syscallNumber, 
            uint param1, uint param2, uint param3, 
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcesId, uint callerThreadId)
        {
            SystemCallResults result = SystemCallResults.Unhandled;
            
            switch ((SystemCallNumbers)syscallNumber)
            {
                case SystemCallNumbers.SleepThread:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Sleep");
#endif
                    SysCall_Sleep((int)param1, callerProcesId, callerThreadId);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.RegisterSyscallHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register Syscall Handler");
#endif
                    SysCall_RegisterSyscallHandler((int)param1, param2, callerProcesId);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.DeregisterSyscallHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister Syscall Handler");
#endif
                    SysCall_DeregisterSyscallHandler((int)param1, callerProcesId);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.StartThread:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Create Thread");
#endif
                    result = SystemCallResults.Deferred;
                    break;
#if SYSCALLS_TRACE
                default:
                    BasicConsole.WriteLine("System call unrecognised/unhandled by Kernel Task.");
                    break;
#endif
            }

            return result;
        }

        private static void SysCall_Sleep(int ms, uint callerProcessId, uint callerThreadId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Sleeping thread...");
#endif
            ProcessManager.GetThreadById(callerThreadId, ProcessManager.GetProcessById(callerProcessId))._EnterSleep(ms);
        }
        private static void SysCall_RegisterSyscallHandler(int syscallNum, uint handlerAddr, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Registering syscall handler...");
#endif
            Process theProcess = ProcessManager.GetProcessById(callerProcessId);

            if (handlerAddr != 0xFFFFFFFF)
            {
                theProcess.SyscallHandler = (Hardware.Processes.SyscallHanderDelegate)Utilities.ObjectUtilities.GetObject((void*)handlerAddr);
            }

            theProcess.SyscallsToHandle.Set(syscallNum);
        }
        private static void SysCall_DeregisterSyscallHandler(int syscallNum, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Deregistering syscall handler...");
#endif
            ProcessManager.GetProcessById(callerProcessId).SyscallsToHandle.Clear(syscallNum);
        }


        private static unsafe void SysCall_RequestPages(int numPages, uint callerProcessId, uint callerThreadId)
        {
            if (numPages > 0)
            {
                BasicConsole.WriteLine("Finding free pages...");
                uint blockPtr = (uint)Hardware.VirtMemManager.MapFreePages(Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly, numPages);
                BasicConsole.WriteLine("Reserving spcace for pages in data layout...");
                DeferredSystemCalls_CurrentProcess.TheMemoryLayout.DataPages.Capacity += numPages;
                BasicConsole.WriteLine("Adding pages to data layout...");
                for (uint i = 0; i < numPages; i++)
                {
                    uint vAddr = blockPtr + (i * 4096);
                    DeferredSystemCalls_CurrentProcess.TheMemoryLayout.AddDataPage(
                        Hardware.VirtMemManager.GetPhysicalAddress(vAddr),
                        vAddr);
                }
                BasicConsole.WriteLine("Done.");
                DeferredSystemCalls_CurrentThread.Return1 = blockPtr;
            }
            else
            {
                DeferredSystemCalls_CurrentThread.Return1 = 0;
            }

            DeferredSystemCalls_CurrentThread.Return2 = 0;
            DeferredSystemCalls_CurrentThread.Return3 = 0;
            DeferredSystemCalls_CurrentThread.Return4 = 0;
        }

        private static void SysCall_PlayNote(Hardware.Timers.PIT.MusicalNote note, Hardware.Timers.PIT.MusicalNoteValue duration, uint bpm)
        {
            Hardware.Tasks.PlayNotesTask.RequestNote(note, duration, bpm);
        }
        private static void SysCall_Semaphore(SemaphoreRequests request, int id, uint limitOrProcessId)
        {
            SemaphoreResponses response = SemaphoreResponses.INVALID;
            switch (request)
            {
                case SemaphoreRequests.INVALID:
                    BasicConsole.WriteLine("Error! INVALID semaphore request made.");
                    response = SemaphoreResponses.INVALID;
                    break;
                case SemaphoreRequests.Allocate:
                    BasicConsole.WriteLine("Allocate Semaphore Request");
                    id = ProcessManager.Semaphore_Allocate((int)limitOrProcessId, DeferredSystemCalls_CurrentProcess);
                    BasicConsole.Write("Allocated id: ");
                    BasicConsole.WriteLine(id);
                    response = id != -1 ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                case SemaphoreRequests.Deallocate:
                    BasicConsole.WriteLine("Deallocate Semaphore Request");
                    response = ProcessManager.Semaphore_Deallocate(id, DeferredSystemCalls_CurrentProcess) ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                case SemaphoreRequests.Wait:
                    BasicConsole.WriteLine("Wait on a Semaphore Request");
                    int result = ProcessManager.Semaphore_Wait(id, DeferredSystemCalls_CurrentProcess, DeferredSystemCalls_CurrentThread);
                    DeferredSystemCalls_WakeCurrentThread = result != 0;
                    response = result == 1 ? SemaphoreResponses.Success : (result == 0 ? SemaphoreResponses.Fail : SemaphoreResponses.Error);
                    break;
                case SemaphoreRequests.Signal:
                    BasicConsole.WriteLine("Signal a Semaphore Request");
                    response = ProcessManager.Semaphore_Signal(id, DeferredSystemCalls_CurrentProcess) ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                case SemaphoreRequests.AddOwner:
                    BasicConsole.WriteLine("Add Owner to a Semaphore Request");
                    response = ProcessManager.Semaphore_AddOwner(id, limitOrProcessId, DeferredSystemCalls_CurrentProcess) ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                default:
                    BasicConsole.WriteLine("Error! Unrecognised semaphore request made.");
                    response = SemaphoreResponses.INVALID;
                    break;
            }

            //BasicConsole.WriteLine("----- Completed -----");
            
            DeferredSystemCalls_CurrentThread.Return1 = (uint)response;
            DeferredSystemCalls_CurrentThread.Return2 = (uint)id;
            DeferredSystemCalls_CurrentThread.Return3 = 0;
            DeferredSystemCalls_CurrentThread.Return4 = 0;
        }
    }

    public enum SystemCallNumbers : uint
    {
        INVALID = 0,
        RegisterISRHandler,
        DeregisterISRHandler,
        RegisterIRQHandler,
        DeregisterIRQHandler,
        RegisterSyscallHandler,
        DeregisterSyscallHandler,
        RequestPage,
        UnmapPage,
        SharePage,
        StartProcess,
        EndProcess,
        SetProcessAttributes,
        GetProcessList,
        WaitOnProcess,
        StartThread,
        EndThread,
        SetThreadAttributes,
        GetThreadList,
        WaitOnThread,
        SleepThread,
        CreateSemaphore,
        ReleaseSemaphore,
        WaitSemaphore,
        SignalSemaphore,
        GetTime,
        SetTime,
        GetUpTime,
        RegisterPipeEndpoint,
        GetPipeEndpoints,
        CreatePipe,
        WaitOnPipeCreate,
        ReadPipe,
        WritePipe
    }
    public enum SystemCallResults : int
    {
        Unhandled = -1,
        OK = 0,
        Deferred = 1
    }



    public enum SemaphoreRequests
    {
        INVALID = 0,
        Allocate = 1,
        Deallocate = 2,
        Wait = 3,
        Signal = 4,
        AddOwner = 5
    }
    public enum SemaphoreResponses
    {
        Error = -1,
        INVALID = 0,
        Success = 1,
        Fail = 2
    }

    public enum ThreadRequests
    {
        INVALID = 0,
        Create = 1
    }
    public enum ThreadResponses
    {
        Error = -1,
        INVALID = 0,
        Success = 1
    }

    #region Play Note

    public enum MusicalNote : int
    {
        Silent = 0,
        C0 = 16,
        Cs0 = 17,
        Db0 = 17,
        D0 = 18,
        Ds0 = 19,
        Eb0 = 19,
        E0 = 20,
        F0 = 21,
        Fs0 = 23,
        Gb0 = 23,
        G0 = 24,
        Gs0 = 25,
        Ab0 = 25,
        A0 = 27,
        As0 = 29,
        Bb0 = 29,
        B0 = 30,
        C1 = 32,
        Cs1 = 34,
        Db1 = 34,
        D1 = 36,
        Ds1 = 38,
        Eb1 = 38,
        E1 = 41,
        F1 = 43,
        Fs1 = 46,
        Gb1 = 46,
        G1 = 49,
        Gs1 = 51,
        Ab1 = 51,
        A1 = 55,
        As1 = 58,
        Bb1 = 58,
        B1 = 61,
        C2 = 65,
        Cs2 = 69,
        Db2 = 69,
        D2 = 73,
        Ds2 = 77,
        Eb2 = 77,
        E2 = 82,
        F2 = 87,
        Fs2 = 92,
        Gb2 = 92,
        G2 = 98,
        Gs2 = 103,
        Ab2 = 103,
        A2 = 110,
        As2 = 116,
        Bb2 = 116,
        B2 = 123,
        C3 = 130,
        Cs3 = 138,
        Db3 = 138,
        D3 = 146,
        Ds3 = 155,
        Eb3 = 155,
        E3 = 164,
        F3 = 174,
        Fs3 = 185,
        Gb3 = 185,
        G3 = 196,
        Gs3 = 207,
        Ab3 = 207,
        A3 = 220,
        As3 = 233,
        Bb3 = 233,
        B3 = 246,
        C4 = 261,
        Cs4 = 277,
        Db4 = 277,
        D4 = 293,
        Ds4 = 311,
        Eb4 = 311,
        E4 = 329,
        F4 = 349,
        Fs4 = 369,
        Gb4 = 369,
        G4 = 392,
        Gs4 = 415,
        Ab4 = 415,
        A4 = 440,
        As4 = 466,
        Bb4 = 466,
        B4 = 493,
        C5 = 523,
        Cs5 = 554,
        Db5 = 554,
        D5 = 587,
        Ds5 = 622,
        Eb5 = 622,
        E5 = 659,
        F5 = 698,
        Fs5 = 739,
        Gb5 = 739,
        G5 = 783,
        Gs5 = 830,
        Ab5 = 830,
        A5 = 880,
        As5 = 932,
        Bb5 = 932,
        B5 = 987,
        C6 = 1046,
        Cs6 = 1108,
        Db6 = 1108,
        D6 = 1174,
        Ds6 = 1244,
        Eb6 = 1244,
        E6 = 1318,
        F6 = 1396,
        Fs6 = 1479,
        Gb6 = 1479,
        G6 = 1567,
        Gs6 = 1661,
        Ab6 = 1661,
        A6 = 1760,
        As6 = 1864,
        Bb6 = 1864,
        B6 = 1975,
        C7 = 2093,
        Cs7 = 2217,
        Db7 = 2217,
        D7 = 2349,
        Ds7 = 2489,
        Eb7 = 2489,
        E7 = 2637,
        F7 = 2793,
        Fs7 = 2959,
        Gb7 = 2959,
        G7 = 3135,
        Gs7 = 3322,
        Ab7 = 3322,
        A7 = 3520,
        As7 = 3729,
        Bb7 = 3729,
        B7 = 3951,
        C8 = 4186,
        Cs8 = 4434,
        Db8 = 4434,
        D8 = 4698,
        Ds8 = 4978,
        Eb8 = 4978,
        E8 = 5274,
        F8 = 5587,
        Fs8 = 5919,
        Gb8 = 5919,
        G8 = 6271,
        Gs8 = 6644,
        Ab8 = 6644,
        A8 = 7040,
        As8 = 7458,
        Bb8 = 7458,
        B8 = 7902
    }
    public enum MusicalNoteValue : uint
    {
        Semiquaver = 1,     //  1/16
        Quaver = 2,         //  1/8
        Crotchet = 4,       //  1/4
        Minim = 8,          //  1/2
        Semibreve = 16,     //  1
        Breve = 32,         //  2
        Longa = 64,         //  4
        Maxima = 128        //  8
    }

    #endregion
}
