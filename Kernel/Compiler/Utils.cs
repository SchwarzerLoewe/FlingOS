﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Kernel.Compiler
{
    /// <summary>
    /// Static utility methods used throughout the compiler.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// The string of characters which are illegal to use in ASM labels.
        /// </summary>
        public const string IllegalIdentifierChars = "&,+$<>{}-`\'/\\ ()[]*!=.";
        /// <summary>
        /// Replaces illegal characters from an ASM label (identifier) with an underscore ('_')
        /// </summary>
        /// <param name="x">The label to filter.</param>
        /// <returns>The filtered label.</returns>
        public static string FilterIdentifierForInvalidChars(string x)
        {
            string xTempResult = x;
            foreach (char c in IllegalIdentifierChars)
            {
                xTempResult = xTempResult.Replace(c, '_');
            }
            return String.Intern(xTempResult);
        }

        /// <summary>
        /// Gets the signature of the specified method. A method's signature will (probably) always be unique.
        /// </summary>
        /// <param name="aMethod">The method to get the signature of.</param>
        /// <returns>The method's siganture string.</returns>
        public static string GetMethodSignature(MethodBase aMethod)
        {
            string aMethodSignature = "";
            if (aMethod.IsConstructor || aMethod is ConstructorInfo)
            {
                aMethodSignature = typeof(void).FullName + "_RETEND_" + aMethod.DeclaringType.FullName + "_DECLEND_" + aMethod.Name + "_NAMEEND_(";
            }
            else
            {
                aMethodSignature = ((MethodInfo)aMethod).ReturnType.FullName + "_RETEND_" + aMethod.DeclaringType.FullName + "_DECLEND_" + aMethod.Name + "_NAMEEND_(";
            }
            ParameterInfo[] Params = aMethod.GetParameters();
            bool firstParam = true;
            foreach (ParameterInfo aParam in Params)
            {
                if (!firstParam)
                {
                    aMethodSignature += ",";
                }
                firstParam = false;

                aMethodSignature += aParam.ParameterType.FullName;
            }
            aMethodSignature += ")";
            return aMethodSignature;
        }
        /// <summary>
        /// Reverse the method signature i.e. returns a string array of:
        /// Return type full name,
        /// Declaring type full name,
        /// Method name
        /// </summary>
        /// <param name="methodSig">The method signature to reverse.</param>
        /// <returns>See summary.</returns>
        public static string[] ReverseMethodSignature(string methodSig)
        {
            string[] result = new string[3];

            int RETENDIndex = methodSig.IndexOf("_RETEND_");
            int DECLENDIndex = methodSig.IndexOf("_DECLEND_");
            int NAMEENDIndex = methodSig.IndexOf("_NAMEEND_");

            result[0] = methodSig.Substring(0, RETENDIndex);
            result[1] = methodSig.Substring(RETENDIndex + 8, DECLENDIndex - RETENDIndex - 8);
            result[2] = methodSig.Substring(DECLENDIndex + 9, NAMEENDIndex - DECLENDIndex - 9);

            return result;
        }
        /// <summary>
        /// Gets the signature of the specified field. A field's signature will (probably) always be unique.
        /// </summary>
        /// <param name="aField">The field to get the signature of.</param>
        /// <returns>The field's siganture string.</returns>
        public static string GetFieldSignature(FieldInfo aField)
        {
            return aField.FieldType.FullName + " " + aField.DeclaringType.FullName + "." + aField.Name;
        }

        /// <summary>
        /// Gets the number of bytes used by a given type when it is represented on the stack.
        /// </summary>
        /// <param name="theType">The type to get the size of.</param>
        /// <returns>The number of bytes used to represent the specified type on the stack.</returns>
        public static int GetNumBytesForType(Type theType)
        {
            //Assume its a 32-bit pointer/reference unless it is:
            // - A value type
            //TODO - this should be moved to the target architecture's library
            int result = 4;
            
            if (theType.IsValueType)
            {
                if (theType.AssemblyQualifiedName == typeof(void).AssemblyQualifiedName)
                {
                    result = 0;
                }                
                else if (theType.AssemblyQualifiedName == typeof(byte).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(sbyte).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(UInt16).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(Int16).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(UInt32).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(Int32).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(UInt64).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(Int64).AssemblyQualifiedName)
                {
                    result = 8;
                }
                else if (theType.AssemblyQualifiedName == typeof(string).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(char).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(float).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(double).AssemblyQualifiedName)
                {
                    result = 8;
                }
                else if (theType.AssemblyQualifiedName == typeof(bool).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(decimal).AssemblyQualifiedName)
                {
                    result = 16;
                }
                else if (theType.AssemblyQualifiedName == typeof(IntPtr).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else
                {
                    List<FieldInfo> AllFields = theType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();

                    //This is a value type from a struct
                    result = 0;
                    foreach (FieldInfo anInfo in AllFields)
                    {
                        result += GetNumBytesForType(anInfo.FieldType);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the mnemonic for a given number of bytes (e.g. 1 = byte, 2 = word, 4 = dword, 8 = qword)
        /// </summary>
        /// <param name="numBytes">The number of bytes. Shouldbe a power of 2.</param>
        /// <returns>The mnemonic or null if the number of bytes was not recognised.</returns>
        public static string GetOpSizeString(int numBytes)
        {
            string size = null;
            switch (numBytes)
            {
                case 1:
                    size = "byte";
                    break;
                case 2:
                    size = "word";
                    break;
                case 4:
                    size = "dword";
                    break;
                case 8:
                    size = "qword";
                    break;
            }
            return size;
        }

        /// <summary>
        /// Determines whether the specified type is a floating point number (inc. single and double precision).
        /// </summary>
        /// <param name="aType">The type to check.</param>
        /// <returns>True if the type is a flaoting point type. Otherwise false.</returns>
        public static bool IsFloat(Type aType)
        {
            bool isFloat = false;

            if (aType.AssemblyQualifiedName == typeof(float).AssemblyQualifiedName ||
                aType.AssemblyQualifiedName == typeof(double).AssemblyQualifiedName)
            {
                isFloat = true;
            }

            return isFloat;
        }

        /// <summary>
        /// Reads a signed integer 16 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static Int16 ReadInt16(byte[] bytes, int offset)
        {
            return BitConverter.ToInt16(bytes, offset);
        }
        /// <summary>
        /// Reads a signed integer 32 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static Int32 ReadInt32(byte[] bytes, int offset)
        {
            return BitConverter.ToInt32(bytes, offset);
        }
        /// <summary>
        /// Reads a signed integer 64 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static Int64 ReadInt64(byte[] bytes, int offset)
        {
            return BitConverter.ToInt64(bytes, offset);
        }
        /// <summary>
        /// Reads a single-precision (32-bit) floating point number from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static float ReadFloat32(byte[] bytes, int offset)
        {
            return (float)(BitConverter.ToSingle(bytes, 0));
        }
        /// <summary>
        /// Reads a double-precision (64-bit) floating point number from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static double ReadFloat64(byte[] bytes, int offset)
        {
            return (double)(BitConverter.ToDouble(bytes, 0));
        }

        /// <summary>
        /// Determines whether the specified type is managed by the 
        /// garbage collector or not.
        /// </summary>
        /// <param name="theType">The type to check for GC management.</param>
        /// <returns>Whether the specified type is managed by the 
        /// garbage collector or not.</returns>
        public static bool IsGCManaged(Type theType)
        {
            bool isGCManaged = true;

            if(theType.IsValueType || theType.IsPointer)
            {
                isGCManaged = false;
            }

            return isGCManaged;
        }
    }
}