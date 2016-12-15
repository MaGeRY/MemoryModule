using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace System
{
    public enum PatchType
    {
        Injection,
        Overwrite
    }

    public enum ReturnType
    {
        None,
        NotNull,
        MethodType,
        JumpIfNotNull
    }

    public partial class Invoker
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class Patch : Attribute
        {
            public string Assembly;
            public string MethodName;
            public string[] RequiredHash;

            public Patch(string targetAssembly, string targetMethod, params string[] requiredHash)
            {
                this.Assembly = targetAssembly;
                this.MethodName = targetMethod;
                this.RequiredHash = requiredHash;
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class Params : Attribute
        {
            public PatchType PatchType;
            public int PatchIndex;            
            public int ContinueIndex;
            public string CallMethodArgs;
            public string[] PatchedHash;

            /// <summary>
            /// Parameters to invoking this method
            /// </summary>
            /// <param name="patchType">Type of patch method: injection - insert invoke method between instructions; override - replace all instructions to call method.</param>
            /// <param name="patchIndex">Index in instructions for insert invoke method, works only when type is injection.</param>
            /// <param name="patchedHash">Hashes to require when this method is already patched.</param>
            public Params(PatchType patchType, int patchIndex, params string[] patchedHash)
            {
                this.PatchType = patchType;
                this.PatchIndex = patchIndex;
                this.CallMethodArgs = string.Empty;
                this.ContinueIndex = 0;
                this.PatchedHash = patchedHash;
            }

            /// <summary>
            /// Parameters to invoking this method
            /// </summary>
            /// <param name="patchType">Type of patch method: injection - insert invoke method between instructions; override - replace all instructions to call method.</param>
            /// <param name="patchIndex">Index in instructions for insert invoke method, works only when type is injection.</param>
            /// <param name="methodArgs">Arguments to transfer for invoking method (this, arg0, arg1,..argN, var0, var1, ..varN).</param>
            /// <param name="patchedHash">Hashes to require when this method is already patched.</param>
            public Params(PatchType patchType, int patchIndex, string methodArgs, params string[] patchedHash)
            {
                this.PatchType = patchType;
                this.PatchIndex = patchIndex;
                this.CallMethodArgs = methodArgs;
                this.ContinueIndex = 0;
                this.PatchedHash = patchedHash;
            }

            /// <summary>
            /// Parameters to invoking this method
            /// </summary>
            /// <param name="patchType">Type of patch method: injection - insert invoke method between instructions; override - replace all instructions to call method.</param>
            /// <param name="patchIndex">Index in instructions for insert invoke method, works only when type is injection.</param>
            /// <param name="continueIndex">Continue index in instructions when invoking method return nullable value.</param>
            /// <param name="patchedHash">Hashes to require when this method is already patched.</param>
            public Params(PatchType patchType, int patchIndex, int continueIndex, params string[] patchedHash)
            {
                this.PatchType = patchType;
                this.PatchIndex = patchIndex;
                this.ContinueIndex = continueIndex;
                this.CallMethodArgs = string.Empty;
                this.PatchedHash = patchedHash;
            }

            /// <summary>
            /// Parameters to invoking this method
            /// </summary>
            /// <param name="patchType">Type of patch method: injection - insert invoke method between instructions; override - replace all instructions to call method.</param>
            /// <param name="patchIndex">Index in instructions for insert invoke method, works only when type is injection.</param>
            /// <param name="continueIndex">Continue index in instructions when invoking method return nullable value.</param>
            /// <param name="methodArgs">Arguments to transfer for invoking method (this, arg0, arg1,..argN, var0, var1, ..varN).</param>
            /// <param name="patchedHash">Hashes to require when this method is already patched.</param>
            public Params(PatchType patchType, int patchIndex, int continueIndex, string methodArgs, params string[] patchedHash)
            {
                this.PatchType = patchType;
                this.PatchIndex = patchIndex;
                this.ContinueIndex = continueIndex;
                this.CallMethodArgs = methodArgs;
                this.PatchedHash = patchedHash;
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class Hook : Attribute
        {
            public string Name;

            public Hook(string name)
            {
                this.Name = name;
            }
        }
    }    
}
