﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kernel.Debug.Data;

namespace Kernel.Compiler
{
    /// <summary>
    /// Manages reading the ObjDump file generated from a C# debug database (.pdb) file.
    /// </summary>
    public class PDBDumpManager
    {
        /// <summary>
        /// The assembly manager used by the PDB Dump manager.
        /// </summary>
        AssemblyManager TheAssemblyManager;

        /// <summary>
        /// All the loaded symbols from the PDB file.
        /// </summary>
        public Dictionary<string, PDB_SymbolInfo> Symbols = new Dictionary<string, PDB_SymbolInfo>();

        /// <summary>
        /// Initialises a new PDB Dump Manager loading the specified DLL
        /// as the root assembly of the assembly manager.
        /// </summary>
        /// <param name="rootDLLFileName">The path to the DLL to use as the root assembly.</param>
        public PDBDumpManager(string rootDLLFileName)
        {
            TheAssemblyManager = new AssemblyManager();
            var rootAssembly = TheAssemblyManager.LoadAssembly(rootDLLFileName);
            TheAssemblyManager.LoadReferences(rootAssembly);
            Init();
        }
        /// <summary>
        /// Initialises a new PDB Dump Manager using the specified assembly
        /// manager.
        /// </summary>
        /// <param name="anAssemblyManager">The assembly manager to use.</param>
        public PDBDumpManager(AssemblyManager anAssemblyManager)
        {
            TheAssemblyManager = anAssemblyManager;
            Init();
        }
        /// <summary>
        /// Initialise the PDB Dump manager (called by constructors for you).
        /// Loads all the pdb dumps for the root assembly and referenced assemblies.
        /// </summary>
        public void Init()
        {
            foreach (var anAssembly in TheAssemblyManager.Assemblies.Values)
            {
                string assemblyFileName = Path.Combine(Path.GetDirectoryName(anAssembly.Location), Path.GetFileNameWithoutExtension(anAssembly.Location));
                string aPDBDumpPathname = assemblyFileName + ".pdb_dump";

                PDBDumpReader theReader = new PDBDumpReader(aPDBDumpPathname);
                Symbols = Symbols.Concat(theReader.Symbols).ToDictionary(x => x.Key, y => y.Value);
            }
        }
    }
}