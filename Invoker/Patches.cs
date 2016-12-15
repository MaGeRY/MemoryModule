using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Cryptography;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace System
{
    public partial class Invoker
    {
        public class Patches
        {
            private static List<PatchInfo> PatchList = new List<PatchInfo>();
            public static PatchInfo[] List => PatchList.ToArray();

            #region [public] FindType(string name)
            public static System.Type FindType(string name)
            {
                foreach (Assembly A in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (Type T in A.GetTypes())
                        {
                            if (T.Name == name) return T;
                        }
                    }
                    catch (Exception E)
                    {
                        Log.Console(ConsoleColor.Red, "Module: " + A + "\n" + E);
                    }
                }
                return null;
            }
            #endregion

            #region [public] FindMethod(string methodName, params Type[] parameterTypes)
            public static MethodInfo FindMethod(string methodName, params System.Type[] parameterTypes)
            {
                if (methodName.IndexOf(".") > 0)
                {
                    string invokeType = methodName.Substring(0, methodName.LastIndexOf("."));
                    string invokeName = methodName.Replace(invokeType + ".", "");
                    System.Type invokerType = FindType(invokeType);

                    if (invokerType != null)
                    {
                        foreach (MethodInfo info in invokerType.GetMethods(Invoker.AllBindingFlags))
                        {
                            if (info.Name == invokeName)
                            {
                                ParameterInfo[] parameters = info.GetParameters();
                                if (info.GetParameters().Length == parameterTypes.Length)
                                {
                                    bool isMethod = true;

                                    for (int i = 0; i < parameters.Length; i++)
                                    {
                                        if (parameters[i].ParameterType != parameterTypes[i])
                                        {
                                            isMethod = false;
                                            break;
                                        }
                                    }

                                    if (isMethod)
                                    {
                                        return info;
                                    }
                                }
                            }
                        }
                    }
                }
                return null;
            }
            #endregion

            #region [public] FindMethodDefinition(MethodInfo methodInfo)
            public static MethodDefinition FindMethodDefinition(MethodInfo methodInfo)
            {
                if (methodInfo != null)
                {
                    string filename = methodInfo.DeclaringType.Assembly.Location;
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    string methodType = methodInfo.DeclaringType.FullName;
                    string methodName = methodInfo.Name;

                    if (System.IO.File.Exists(filename))
                    {
                        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(filename);
                        TypeDefinition typeDefinition = assembly.MainModule.GetType(methodType);

                        foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
                        {
                            if (methodDefinition.Name == methodName)
                            {
                                ParameterDefinition[] arguments = methodDefinition.Parameters.ToArray();
                                if (arguments.Length == parameters.Length)
                                {
                                    bool isMethodDefinition = true;
                                    for (int i = 0; i < arguments.Length; i++)
                                    {
                                        if (Type.GetType(arguments[i].ParameterType.FullName, true) != parameters[i].ParameterType)
                                        {
                                            isMethodDefinition = false;
                                            break;
                                        }
                                    }

                                    if (isMethodDefinition)
                                    {
                                        return methodDefinition;
                                    }
                                }
                            }
                        }
                    }
                }
                return null;
            }
            #endregion

            #region [public] FindTypeDefinition(AssemblyDefinition assembly, string fullName)
            public static TypeDefinition FindTypeDefinition(AssemblyDefinition assembly, string fullName)
            {
                foreach (TypeDefinition type in assembly.MainModule.Types)
                {                    
                    if (type.FullName == fullName)
                    {
                        return type;
                    }

                    if (type.HasNestedTypes)
                    {
                        foreach (TypeDefinition nestedType in type.NestedTypes)
                        {
                            if (nestedType.FullName.Replace('/', '.') == fullName)
                            {
                                return nestedType;
                            }
                        }
                    }

                    if (type.HasGenericParameters)
                    {
                        string typeName = type.FullName;
                        int argsIndex = typeName.IndexOf('`');
                        if (argsIndex > -1)
                        {
                            typeName = typeName.Substring(0, argsIndex);
                            typeName += "<" + GetParameters(type.GenericParameters.ToArray()) + ">";                            
                            
                            if (typeName == fullName)
                            {
                                return type;
                            }
                        }
                    }
                }
                return null;
            }
            #endregion

            #region [public] FindMethodDefinition(AssemblyDefinition assembly, string fullName)
            public static MethodDefinition FindMethodDefinition(AssemblyDefinition assembly, string fullName)
            {
                if (assembly != null && !string.IsNullOrEmpty(fullName) && fullName.IndexOf('.') > 0)
                {                    
                    string args = string.Empty;
                    int argsIndex = fullName.IndexOf('(');
                    if (argsIndex > 0)
                    {
                        args = fullName.Substring(argsIndex).Trim('(', ')');
                        fullName = fullName.Substring(0, argsIndex);
                    }

                    string[] names = fullName.Split('.');
                    string type = string.Join(".", names, 0, names.Length - 1);
                    string name = names[names.Length - 1];

                    if (name == "ctor" || name == "cctor") name = "." + name;

                    TypeDefinition typeDefinition = FindTypeDefinition(assembly, type);
                    if (typeDefinition != null && typeDefinition.HasMethods)
                    {
                        foreach (MethodDefinition M in typeDefinition.Methods)
                        {
                            if (M.Name == name && GetParameters(M.Parameters.ToArray()) == args)
                            {
                                return M;
                            }    
                        }
                    }

                }
                return null;
            }
            #endregion

            #region [public] FindAssemblyDefinition(string name)
            public static AssemblyDefinition FindAssemblyDefinition(string name)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.EscapedCodeBase != null && assembly.EscapedCodeBase.Length > 0)
                    {
                        string filename = assembly.Location;
                        if (System.IO.Path.GetFileName(filename).Equals(name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (System.IO.File.Exists(filename))
                            {
                                return AssemblyDefinition.ReadAssembly(filename);
                            }
                        }
                    }
                }
                return null;
            }
            #endregion           
            
            public static void Create(Patch patch, MethodInfo method, Params parameters)
            {
                string arguments = parameters.CallMethodArgs.RemoveWhitespaces();

                PatchInfo patches = new PatchInfo();
                patches.TargetAssembly = patch.Assembly;
                patches.TargetMethod = patch.MethodName;
                patches.RequiredHash = patch.RequiredHash;
                patches.InvokeMethod = method;
                patches.PatchType = parameters.PatchType;
                patches.PatchIndex = parameters.PatchIndex;
                patches.ContinueIndex = parameters.ContinueIndex;
                patches.InvokeAgruments = new string[0];

                if (arguments.Length > 0)
                {
                    patches.InvokeAgruments = arguments.ToLower().Split(',');
                }                

                patches.PatchedHash = parameters.PatchedHash;
                PatchList.Add(patches);
            }
        }                

        public class PatchInfo
        {            
            public string TargetAssembly;
            public string TargetMethod;
            public string[] RequiredHash;
            public PatchType PatchType;
            public int PatchIndex;
            public int ContinueIndex;
            public string[] PatchedHash;
            public MethodInfo InvokeMethod;
            public string[] InvokeAgruments;            

            public static string AssemblyDirectory { get; private set; }
            public static Mono.Cecil.AssemblyDefinition PatchAssembly { get; private set; }
            public static List<Mono.Cecil.AssemblyDefinition> AssembliesPatched { get; private set; }
            public Mono.Cecil.MethodDefinition InvokerMethod { get; private set; }            
            public Mono.Cecil.MethodDefinition PatchMethod { get; private set; }

            public Int32 Index { get; private set; }
            public string InvokeCallName { get; private set; }
            public List<Mono.Cecil.Cil.VariableDefinition> Variables { get; private set; }
            public List<Mono.Cecil.Cil.Instruction> Instructions { get; private set; }
            public List<Mono.Cecil.Cil.ExceptionHandler> ExceptionHandlers { get; private set; }

            private class PatchBackup
            {
                public VariableDefinition[] Variables;
                public Instruction[] Instructions;
                public ExceptionHandler[] ExceptionHandlers;
            }

            private PatchBackup Backup;

            private struct ExceptionHandlersJump
            {
                public int HandlerStart;
                public int HandlerEnd;
                public int TryStart;
                public int TryEnd;
            }

            private List<int> InstructionsJumps;
            private List<ExceptionHandlersJump> ExceptionHandlersJumps;                        
            private Mono.Cecil.Cil.Instruction ContinueInstruction;

            public bool IsApplied { get; private set; }
            public bool IsPatched { get; private set; }

            public string InvokeMethodName
            {
                get
                {
                    return InvokeMethod.DeclaringType.FullName + "." + InvokeMethod.Name;
                }
            }

            public PatchInfo()
            {                
                InstructionsJumps = new List<int>();
                AssembliesPatched = new List<AssemblyDefinition>();
            }

            #region [this] BuildHash()
            private string BuildHash(Instruction[] instructions, VariableDefinition[] variables, ExceptionHandler[] exceptionHandlers)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < variables.Length; i++)
                {
                    builder.Append("VAR_").Append(i.ToString("x4") + ": ");
                    builder.AppendLine(variables[i].VariableType.FullName);
                }

                for (int i = 0; i < instructions.Length; i++)
                {
                    builder.AppendLine(instructions[i].ToString());
                }

                for (int i = 0; i < exceptionHandlers.Length; i++)
                {
                    builder.Append("HANDLER_" + i.ToString("x4") + ": ");
                    builder.Append(exceptionHandlers[i].HandlerType.ToString());
                    if (exceptionHandlers[i].CatchType != null)
                    {
                        builder.Append("/" + exceptionHandlers[i].CatchType.ToString());
                    }
                    builder.Append(" (IL_");
                    builder.Append(exceptionHandlers[i].TryStart.Offset.ToString("x4") + ", IL_");
                    builder.Append(exceptionHandlers[i].TryEnd.Offset.ToString("x4") + ", IL_");
                    builder.Append(exceptionHandlers[i].HandlerStart.Offset.ToString("x4") + ", IL_");
                    builder.AppendLine(exceptionHandlers[i].HandlerEnd.Offset.ToString("x4") + ")");
                }
                return builder.ToString();
            }
            #endregion

            #region [this] GetHash()
            public string GetHash(bool flush_buffer = false)
            {
                string buffer = null;
                if (Instructions != null && Instructions.Count > 0)
                {
                    buffer = BuildHash(Instructions.ToArray(), Variables.ToArray(), ExceptionHandlers.ToArray());                    
                }
                else if (PatchMethod != null && PatchMethod.HasBody && PatchMethod.Body.Instructions.Count > 0)
                {
                    buffer = BuildHash(PatchMethod.Body.Instructions.ToArray(), PatchMethod.Body.Variables.ToArray(), PatchMethod.Body.ExceptionHandlers.ToArray());
                }
                if (buffer != null && buffer != string.Empty)
                {
                    #if (DEBUG)
                    if (flush_buffer)
                    {
                        byte[] flush = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(buffer));
                        string name = BitConverter.ToString(flush).Replace("-", "").ToLower();
                        File.WriteAllText("D:/" + name + ".txt", buffer);
                    }
                    #endif

                    byte[] bytes = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(buffer));
                    return BitConverter.ToString(bytes).Replace("-", "").ToLower();
                }                
                return new string('0', 32);
            }
            #endregion

            #region [this] SetInvoker(string invokerName = null, params Type[] invokerParameterTypes)
            public void SetInvoker(string invokerName, params Type[] invokerParameterTypes)
            {
                if (string.IsNullOrEmpty(invokerName))
                {
                    invokerName = "Invoker.CallHook";
                }                

                MethodInfo invokerMethod = Patches.FindMethod(invokerName, invokerParameterTypes);
                if (invokerMethod == null)
                {
                    throw new Exception("Method '" + invokerName + "(" + GetParameters(invokerParameterTypes) + ")' of invoker not exists.");
                }

                InvokerMethod = Patches.FindMethodDefinition(invokerMethod);
                if (InvokerMethod == null)
                {
                    throw new Exception("Method definition '" + invokerName + "(" + GetParameters(invokerMethod.GetParameters()) + ")' of invoker not exists.");
                }
            }
            #endregion

            #region [this] HasPatch()
            private bool HasPatch(string invokeName)
            {
                int injectIndex = -1;
                Instruction instruction = null;
                string opCodeString = string.Empty;
                int checkIndex = PatchMethod.Body.Instructions.Count;                

                //uEngine.Log.Console($"Checking patch [{invokeName}/{InvokeMethodName}({InvokeAgruments.ToString(", ")})]");

                // Ищем вызов хука в теле метода //
                while (--checkIndex > -1)
                {
                    instruction = PatchMethod.Body.Instructions[checkIndex];

                    if (instruction.OpCode.Code == Code.Ldstr && instruction.Operand.ToString().ToLower() == invokeName)
                    {
                        if (InvokeAgruments.Length == 0)
                        {
                            injectIndex = checkIndex + 1;
                            instruction = PatchMethod.Body.Instructions[injectIndex];
                        }
                        else
                        {
                            injectIndex = checkIndex + 2;
                            instruction = PatchMethod.Body.Instructions[injectIndex];
                        }

                        #region [ Если найден вызов CallHook в методе для этого патча ]
                        if (instruction.OpCode.Code == Code.Call && instruction.Operand.ToString().ToLower() == InvokerMethod.ToString().ToLower())
                        {
                            opCodeString = instruction.OpCode.Code.ToString().ToLower();
                            //uEngine.Log.Console($"Patch found on line #{injectIndex}: " + opCodeString);                            

                            // Возвращаем TRUE если нет аргументов у патча при вызове //
                            if (InvokeAgruments.Length == 0) return true;

                            int currentIndex = injectIndex;
                            int ldc_i4 = 0, argIndex = 0, objectArrayLength = -1;

                            // Проверяем патч на имя инжекта и массива аргументов //
                            instruction = PatchMethod.Body.Instructions[--currentIndex];
                            opCodeString = instruction.OpCode.ToString().ToLower();
                            //uEngine.Log.Console("Instruction[" + currentIndex + "]: " + opCodeString + " -> " + instruction.Operand?.ToString().ToLower());

                            if (!opCodeString.StartsWith("ldloc.")) return false;

                            instruction = PatchMethod.Body.Instructions[--currentIndex];
                            opCodeString = instruction.OpCode.ToString().ToLower();
                            //uEngine.Log.Console("Instruction[" + currentIndex + "]: " + opCodeString + " -> " + instruction.Operand?.ToString().ToLower());

                            if (instruction.OpCode.Code != Code.Ldstr || instruction.Operand.ToString().ToLower() != invokeName) return false;

                            instruction = PatchMethod.Body.Instructions[--currentIndex];
                            opCodeString = instruction.OpCode.ToString().ToLower();
                            //uEngine.Log.Console("Instruction[" + currentIndex + "]: " + opCodeString + " -> " + instruction.Operand?.ToString().ToLower());

                            if (!opCodeString.StartsWith("stloc.")) return false;
                                                       
                            // Проверяем патч если есть аргументы //
                            for (int i = InvokeAgruments.Length-1; i >= 0; i--)
                            {                                
                                //uEngine.Log.Console("Check patch argument["+i+"]: " + InvokeAgruments[i]);

                                if (InvokeAgruments[i].Length != 4) return false;

                                // Проверка OpCode => stelem.ref
                                instruction = PatchMethod.Body.Instructions[--currentIndex];
                                opCodeString = instruction.OpCode.ToString().ToLower();
                                //uEngine.Log.Console("Instruction[" + currentIndex + "]: " + opCodeString + " -> " + instruction.Operand?.ToString().ToLower());

                                if (!opCodeString.StartsWith("stelem.ref")) return false;

                                // Проверка OpCode => box or ldarg/ldloc
                                instruction = PatchMethod.Body.Instructions[--currentIndex];
                                opCodeString = instruction.OpCode.ToString().ToLower();
                                //uEngine.Log.Console("Instruction[" + currentIndex + "]: " + opCodeString + " -> " + instruction.Operand?.ToString().ToLower());

                                if (InvokeAgruments[i].StartsWith("this"))
                                {
                                    argIndex = 0;

                                    if (!opCodeString.StartsWith("ldarg.") || opCodeString.Substring(6).ToInt32() != argIndex)
                                    {
                                        return false;
                                    }
                                }
                                else if (InvokeAgruments[i].StartsWith("arg"))
                                {
                                    if (!int.TryParse(InvokeAgruments[i].Substring(3), out argIndex)) return false;
                                    
                                    // Увеличиваем индекс аргумента если метод не статичный
                                    if (!PatchMethod.IsStatic) argIndex++;

                                    ParameterDefinition parameter = PatchMethod.Parameters[argIndex];
                                    if (parameter.ParameterType.IsByReference)
                                    {
                                        if (opCodeString != "box") return false;

                                        instruction = PatchMethod.Body.Instructions[--currentIndex];
                                        
                                        // Проверка OpCode => Ldobj
                                        if (instruction.OpCode != OpCodes.Ldobj) return false;

                                        // Выбераем предыдущую инструкцию OpCode => ldarg/ldloc
                                        instruction = PatchMethod.Body.Instructions[--currentIndex];
                                        opCodeString = instruction.OpCode.ToString().ToLower();
                                    }
                                    else if (parameter.HasConstant || parameter.ParameterType.IsValueType)
                                    {
                                        if (opCodeString != "box") return false;

                                        // Выбераем предыдущую инструкцию OpCode => ldarg/ldloc
                                        instruction = PatchMethod.Body.Instructions[--currentIndex];
                                        opCodeString = instruction.OpCode.ToString().ToLower();
                                    }

                                    //uEngine.Log.Console("Instruction[" + currentIndex + "]: " + opCodeString + " -> " + instruction.Operand?.ToString().ToLower());

                                    if (!opCodeString.StartsWith("ldarg.") || opCodeString.Substring(6).ToInt32() != argIndex)
                                    {
                                        return false;
                                    }
                                }
                                else if (InvokeAgruments[i].StartsWith("var"))
                                {
                                    if (!int.TryParse(InvokeAgruments[i].Substring(3), out argIndex)) return false;

                                    if (opCodeString != "box") return false;

                                    // Выбераем предыдущую инструкцию OpCode => ldarg/ldloc
                                    instruction = PatchMethod.Body.Instructions[--currentIndex];
                                    opCodeString = instruction.OpCode.ToString().ToLower();

                                    //uEngine.Log.Console("Instruction[" + currentIndex + "]: " + opCodeString + " -> " + instruction.Operand?.ToString().ToLower());

                                    if (!opCodeString.StartsWith("ldloc.") || opCodeString.Substring(6).ToInt32() != argIndex)
                                    {
                                        return false;
                                    }
                                }

                                // Проверка OpCode => ldc.i4.X (Где X номер в массиве переменных object[] в вызове хука)
                                instruction = PatchMethod.Body.Instructions[--currentIndex];
                                opCodeString = instruction.OpCode.ToString().ToLower();

                                //uEngine.Log.Console("Instruction[" + currentIndex + "]: " + opCodeString + " -> " + instruction.Operand?.ToString().ToLower());

                                if (opCodeString.StartsWith("ldc.i4."))
                                {
                                    if (!opCodeString.StartsWith("ldc.i4.") || !int.TryParse(opCodeString.Substring(7), out ldc_i4) || ldc_i4 != i)
                                    {
                                        return false;
                                    }                                    
                                }

                                // Проверка OpCode => dup
                                instruction = PatchMethod.Body.Instructions[--currentIndex];
                                opCodeString = instruction.OpCode.ToString().ToLower();

                                //uEngine.Log.Console("Instruction[" + currentIndex + "]: " + opCodeString + " -> " + instruction.Operand?.ToString().ToLower());

                                if (opCodeString != "dup") return false;
                            }

                            // Проверка OpCode => Newarr
                            instruction = PatchMethod.Body.Instructions[--currentIndex];
                            //uEngine.Log.Console("Instruction[" + currentIndex + "]: " + instruction.OpCode.ToString().ToLower() + " -> " + instruction.Operand?.ToString().ToLower());
                            if (instruction.OpCode.Code != Code.Newarr || instruction.Operand?.ToString().ToLower() != "system.object") return false;                            

                            instruction = PatchMethod.Body.Instructions[--currentIndex];
                            opCodeString = instruction.OpCode.ToString().ToLower();

                            //uEngine.Log.Console("Instruction[" + currentIndex + "]: " + opCodeString + " -> " + instruction.Operand?.ToString().ToLower());
                            
                            // Проверка OpCode => ldc.i4.X (где X - кол-во переменных в object[] массиве для вызова хука)
                            if (!opCodeString.StartsWith("ldc.i4.") || !int.TryParse(opCodeString.Substring(7), out objectArrayLength)) return false;

                            // Проверяем кол-во переменных для object[] массива при вызове хука
                            if (objectArrayLength != InvokeAgruments.Length) return false;

                            //uEngine.Log.Console(ConsoleColor.Green, $"Patch [{invokeName}/{InvokeMethodName}] is verified.");

                            return true;
                        }
                        #endregion
                    }
                }
                return false;
            }
            #endregion            

            #region [this] GetMethodBody()
            public void GetMethodBody()
            {
                Variables = new List<Mono.Cecil.Cil.VariableDefinition>();
                Instructions = new List<Mono.Cecil.Cil.Instruction>();                
                ExceptionHandlers = new List<Mono.Cecil.Cil.ExceptionHandler>();

                InstructionsJumps = new List<int>();
                ExceptionHandlersJumps = new List<ExceptionHandlersJump>();

                if (PatchMethod != null && PatchMethod.HasBody)
                {
                    if (PatchIndex > PatchMethod.Body.Instructions.Count)
                    {
                        throw new Exception("Out of Range: PatchIndex > Body Instructions Count for patch "+ PatchMethod.DeclaringType.FullName+"."+PatchMethod.Name);
                    }

                    // Get Variables //
                    if (PatchMethod.Body.HasVariables)
                    {
                        for (int i = 0; i < PatchMethod.Body.Variables.Count; i++)
                        {
                            Variables.Add(PatchMethod.Body.Variables[i]);
                        }
                    }

                    // Get Instructions //
                    for (int i = 0; i < PatchMethod.Body.Instructions.Count; i++)
                    {
                        // Save jumps in instructions //
                        if (PatchMethod.Body.Instructions[i].Operand == PatchMethod.Body.Instructions[PatchIndex])
                        {
                            InstructionsJumps.Add(i);
                        }
                        
                        Instructions.Add(PatchMethod.Body.Instructions[i]);
                    }

                    // Get Exception Handlers //
                    if (PatchMethod.Body.HasExceptionHandlers)
                    {
                        ExceptionHandlers.AddRange(PatchMethod.Body.ExceptionHandlers);

                        // Save exception handlers in instructions //
                        Instruction patchInstruction = Instructions[PatchIndex];
                        for (int i = 0; i < ExceptionHandlers.Count; i++)
                        {
                            ExceptionHandler exceptionHandler = ExceptionHandlers[i];
                            ExceptionHandlersJump exceptionHandlersJump = new ExceptionHandlersJump()
                            {
                                HandlerStart = (exceptionHandler.HandlerStart == patchInstruction) ? PatchIndex : -1,
                                HandlerEnd = (exceptionHandler.HandlerEnd == patchInstruction) ? PatchIndex : -1,
                                TryStart = (exceptionHandler.TryStart == patchInstruction) ? PatchIndex : -1,
                                TryEnd = (exceptionHandler.TryEnd == patchInstruction) ? PatchIndex : -1
                            };
                            ExceptionHandlersJumps.Add(exceptionHandlersJump);
                        }
                    }
                }
            }
            #endregion

            #region [this] UpdateIndexes()
            private void UpdateIndexes()
            {
                if (Instructions != null && Instructions.Count > 0)
                {
                    int offset = 0;
                    for (int i = 0; i < Instructions.Count; i++)
                    {
                        Instruction ins = Instructions[i];

                        if (i == 0)
                            ins.Previous = null;
                        else
                            ins.Previous = Instructions[i - 1];

                        if (i == Instructions.Count - 1)
                            ins.Next = null;
                        else
                            ins.Next = Instructions[i + 1];

                        ins.Offset = offset;
                        offset += ins.GetSize();
                    }

                    // Restore jumps in instructions //
                    for (int i = 0; i < InstructionsJumps.Count; i++)
                    {
                        int jumpIndex = InstructionsJumps[i];
                        Instructions[jumpIndex].Operand = Instructions[PatchIndex];
                    }

                    // Restore exception handlers for instructions //
                    for (int i = 0; i < ExceptionHandlers.Count; i++)
                    {
                        ExceptionHandler exceptionHandler = ExceptionHandlers[i];
                        ExceptionHandlersJump exceptionHandlersJump = ExceptionHandlersJumps[i];
                        if (exceptionHandlersJump.HandlerStart > -1) exceptionHandler.HandlerStart = Instructions[exceptionHandlersJump.HandlerStart];
                        if (exceptionHandlersJump.HandlerEnd > -1) exceptionHandler.HandlerEnd = Instructions[exceptionHandlersJump.HandlerEnd];
                        if (exceptionHandlersJump.TryStart > -1) exceptionHandler.TryStart = Instructions[exceptionHandlersJump.TryStart];
                        if (exceptionHandlersJump.TryEnd > -1) exceptionHandler.TryEnd = Instructions[exceptionHandlersJump.TryEnd];
                    }
                }
            }
            #endregion

            #region [this] UpdateMethodBody()
            public void UpdateMethodBody()
            {
                if (PatchMethod != null && PatchMethod.HasBody)
                {
                    if (Instructions != null && Instructions.Count > 0)
                    {
                        PatchMethod.Body.Variables.Clear();
                        PatchMethod.Body.Instructions.Clear();
                        PatchMethod.Body.ExceptionHandlers.Clear();

                        // Set Variables //
                        if (Variables != null && Variables.Count > 0)
                        {
                            for (int i = 0; i < Variables.Count; i++)
                            {
                                PatchMethod.Body.Variables.Add(Variables[i]);
                            }
                        }

                        // Set Instructions //
                        for (int i = 0; i < Instructions.Count; i++)
                        {
                            PatchMethod.Body.Instructions.Add(Instructions[i]);
                        }

                        // Set Exception Handlers //
                        if (ExceptionHandlers != null && ExceptionHandlers.Count > 0)
                        {
                            for (int i = 0; i < ExceptionHandlers.Count; i++)
                            {
                                PatchMethod.Body.ExceptionHandlers.Add(ExceptionHandlers[i]);
                            }
                        }

                        if (ExceptionHandlers != null)
                        {
                            ExceptionHandlers.Clear();
                            ExceptionHandlers = null;
                        }

                        Instructions.Clear();
                        Instructions = null;

                        if (Variables != null)
                        {
                            Variables.Clear();
                            Variables = null;
                        }
                    }                    
                }
            }
            #endregion

            #region [this] AddVariable(TypeReference type, string name = "")
            private VariableDefinition AddVariable(TypeReference type, string name = "")
            {
                if (Variables != null)
                {
                    if (string.IsNullOrEmpty(name)) name = "V_" + Variables.Count;
                    VariableDefinition variable = new VariableDefinition(name, type);
                    Variables.Add(variable);
                    return variable;
                }
                return null;
            }
            #endregion

            #region [this] Insert(OpCode opcode)
            private PatchInfo Insert(OpCode opcode)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, byte value)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, value));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, CallSite site)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, site));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, double value)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, value));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, FieldReference field)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, field));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, string value)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, value));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, sbyte value)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, value));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, int value)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, value));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, long value)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, value));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, float value)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, value));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, Instruction target)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, target));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, Instruction[] targets)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, targets));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, MethodReference method)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, method));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, ParameterDefinition parameter)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, parameter));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, TypeReference type)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, type));
                return this;
            }
            private PatchInfo Insert(OpCode opcode, VariableDefinition variable)
            {
                Instructions.Insert(Index++, Instruction.Create(opcode, variable));
                return this;
            }
            #endregion            

            #region [this] Insert_Ldc_I4_S(int index)
            private void Insert_Ldc_I4_S(int index)
            {
                switch (index)
                {
                    case 0: Insert(OpCodes.Ldc_I4_0); break;
                    case 1: Insert(OpCodes.Ldc_I4_1); break;
                    case 2: Insert(OpCodes.Ldc_I4_2); break;
                    case 3: Insert(OpCodes.Ldc_I4_3); break;
                    case 4: Insert(OpCodes.Ldc_I4_4); break;
                    case 5: Insert(OpCodes.Ldc_I4_5); break;
                    case 6: Insert(OpCodes.Ldc_I4_6); break;
                    case 7: Insert(OpCodes.Ldc_I4_7); break;
                    case 8: Insert(OpCodes.Ldc_I4_8); break;
                    default: Insert(OpCodes.Ldc_I4_S, (sbyte)index); break;
                }
            }
            #endregion

            #region [this] Insert_Ldarg_S(int index)
            public Instruction Insert_Ldarg_S(ParameterDefinition parameter)
            {
                Instruction instruction = Instruction.Create(OpCodes.Ldarga_S, parameter);
                Instructions.Insert(Index++, instruction);
                return instruction;
            }
            #endregion

            #region [this] Insert_Starg_S(int index)
            public Instruction Insert_Starg_S(ParameterDefinition parameter)
            {
                Instruction instruction = Instruction.Create(OpCodes.Starg_S, parameter);
                Instructions.Insert(Index++, instruction);
                return instruction;
            }
            #endregion

            #region [this] Insert_Stloc_S(VariableDefinition variable)
            private Instruction Insert_Stloc_S(VariableDefinition variable)
            {
                Instruction instruction = Instruction.Create(OpCodes.Stloc_S, variable);
                Instructions.Insert(Index++, instruction);
                return instruction;
            }
            #endregion

            #region [this] Insert_Stloc(VariableDefinition variable)
            private Instruction Insert_Stloc(VariableDefinition variable)
            {
                int n = Variables.IndexOf(variable);
                Instruction instruction;
                switch (n)
                {
                    case 0: instruction = Instruction.Create(OpCodes.Stloc_0); break;
                    case 1: instruction = Instruction.Create(OpCodes.Stloc_1); break;
                    case 2: instruction = Instruction.Create(OpCodes.Stloc_2); break;
                    case 3: instruction = Instruction.Create(OpCodes.Stloc_3); break;
                    default: instruction = Instruction.Create(OpCodes.Stloc_S, variable); break;
                }
                Instructions.Insert(Index++, instruction);
                return instruction;
            }
            #endregion

            #region [this] Insert_Ldloc(VariableDefinition variable)
            private Instruction Insert_Ldloc(VariableDefinition variable)
            {
                int n = Variables.IndexOf(variable);
                Instruction instruction;
                switch (n)
                {
                    case 0: instruction = Instruction.Create(OpCodes.Ldloc_0); break;
                    case 1: instruction = Instruction.Create(OpCodes.Ldloc_1); break;
                    case 2: instruction = Instruction.Create(OpCodes.Ldloc_2); break;
                    case 3: instruction = Instruction.Create(OpCodes.Ldloc_3); break;
                    default: instruction = Instruction.Create(OpCodes.Ldloc_S, variable); break;
                }
                Instructions.Insert(Index++, instruction);
                return instruction;
            }
            #endregion            

            #region [this] Insert_ReturnRefArguments()
            private void Insert_ReturnRefArguments(VariableDefinition var_arguments, bool increaseIndex = true)
            {
                ParameterInfo[] parameters = InvokeMethod.GetParameters();
                if (parameters.Length > 0)
                {                    
                    int instructionIndex = Index;
                    for (int i = 0; i < parameters.Length; i++)
                    {                        
                        if (parameters[i].ParameterType.IsByRef && InvokeAgruments[i] != "this")
                        {
                            int paramIndex = 0;
                            if (InvokeAgruments[i].StartsWith("arg"))
                            {                               
                                if (int.TryParse(InvokeAgruments[i].Replace("arg", ""), out paramIndex))
                                {
                                    if (PatchMethod.Parameters[paramIndex].ParameterType.IsByReference)
                                    {
                                        Insert_Ldarg_S(PatchMethod.Parameters[paramIndex]);
                                        Insert_Ldloc(var_arguments);
                                        Insert_Ldc_I4_S(i);
                                        Insert(OpCodes.Ldelem_Ref);
                                        Insert(OpCodes.Unbox_Any, PatchMethod.Parameters[paramIndex].ParameterType);
                                        Insert(OpCodes.Stobj, PatchMethod.Parameters[paramIndex].ParameterType);
                                    }
                                    else
                                    {
                                        Insert_Ldloc(var_arguments);
                                        Insert_Ldc_I4_S(i);
                                        Insert(OpCodes.Ldelem_Ref);
                                        if (PatchMethod.Parameters[paramIndex].ParameterType.IsValueType)
                                        {                                            
                                            Insert(OpCodes.Unbox_Any, PatchMethod.Parameters[paramIndex].ParameterType);                                            
                                        }
                                        else
                                        {
                                            Insert(OpCodes.Castclass, PatchMethod.Parameters[paramIndex].ParameterType);
                                        }
                                        Insert_Starg_S(PatchMethod.Parameters[paramIndex]);
                                    }
                                }
                            }
                            else if (InvokeAgruments[i].StartsWith("var"))
                            {
                                if (int.TryParse(InvokeAgruments[i].Replace("var", ""), out paramIndex))
                                {
                                    Insert_Ldloc(var_arguments);                                    
                                    Insert_Ldc_I4_S(i);
                                    Insert(OpCodes.Ldelem_Ref);

                                    if (PatchMethod.Body.Variables[paramIndex].VariableType.IsValueType)
                                    {
                                        Insert(OpCodes.Unbox_Any, Variables[paramIndex].VariableType);
                                    }
                                    else
                                    {
                                        Insert(OpCodes.Castclass, Variables[paramIndex].VariableType);
                                    }
                                    Insert_Stloc_S(Variables[paramIndex]);
                                }
                            }
                        }
                    }

                    if (!increaseIndex)
                    {
                        Index = instructionIndex;
                    }
                }
            }
            #endregion            

            #region [this] Apply()
            public PatchInfo Apply()
            {
                string filename = Assembly.GetExecutingAssembly().Location;
                AssemblyDirectory = System.IO.Path.GetDirectoryName(filename);

                // Save previous assembly when target assembly is changed //
                PatchAssembly = AssembliesPatched.FirstOrDefault(E => Path.GetFileName(E.MainModule.FullyQualifiedName) == TargetAssembly);

                if (PatchAssembly == null)
                {
                    PatchAssembly = Patches.FindAssemblyDefinition(TargetAssembly);
                }

                if (PatchAssembly == null)
                {
                    throw new Exception("Assembly file '" + TargetAssembly + "' not found.");
                }                

                // Get method definition of patch target method //
                PatchMethod = Patches.FindMethodDefinition(PatchAssembly, TargetMethod);
                if (PatchMethod == null)
                {
                    throw new Exception("Method definition '" + TargetMethod + "' not exists in '"+ TargetAssembly + "' to patch.");
                }                

                // Check method on body //
                if (!PatchMethod.HasBody)
                {
                    throw new Exception("Method definition '" + TargetMethod + "' not have body with instructions.");
                }

                //*DEBUG*/ ConsoleWindow.WriteLine("InvokeAgruments: " + InvokeAgruments.Length);

                // Set invoker method for call //
                if (InvokeAgruments.Length == 0)
                {
                    SetInvoker(null, typeof(string));
                }
                else
                {
                    SetInvoker(null, typeof(string), typeof(object[]));
                }

                MethodInfo booleanMethod = Patches.FindMethod("ConvertExtensions.ToBoolean", typeof(object));

                if (booleanMethod == null)
                {
                    throw new Exception("Conversion method 'ConvertExtensions.ToBoolean' of invoker not exists.");
                }

                // Set invoke call name by invoker //
                if (InvokerMethod.Name == "CallHook")
                {
                    InvokeCallName = InvokeMethodName + "(" + GetParameters(InvokeMethod.GetParameters()) + ")";
                    byte[] bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(InvokeCallName));
                    InvokeCallName = BitConverter.ToString(bytes).Replace("-", "").ToLower();
                    if (!Invoker.Hooks.ContainsKey(InvokeCallName))
                    {
                        Invoker.Hooks.Add(InvokeCallName, InvokeMethod);
                    }
                    else
                    {
                        throw new Exception("Duplicate hook name for method \"" + InvokeMethodName + "\" already exists in dictionary.");
                    }
                }
                else
                {
                    InvokeCallName = InvokeMethodName;
                }

                //*DEBUG*/ ConsoleWindow.WriteLine();
                //*DEBUG*/ ConsoleWindow.WriteLine(ConsoleColor.DarkGray, "Directory: " + AssemblyDirectory);                
                //*DEBUG*/ ConsoleWindow.WriteLine(ConsoleColor.DarkYellow, "Patch Assembly: " + PatchAssembly.MainModule.Name);
                //*DEBUG*/ ConsoleWindow.WriteLine(ConsoleColor.Yellow, "Patch Method: " + PatchMethod.FullName.Replace('/', '.').Replace("::", ".").Replace(",", ", "));               
                //*DEBUG*/ ConsoleWindow.WriteLine(ConsoleColor.DarkGreen, "Invoker Method: " + InvokerMethod.ToString().Replace(",", ", "));
                //*DEBUG*/ ConsoleWindow.WriteLine(ConsoleColor.Green, "Call Method: " + InvokeMethod);                

                string currentHash = GetHash();

                //*DEBUG*/ ConsoleWindow.WriteLine();
                //*DEBUG*/ ConsoleWindow.WriteLine(ConsoleColor.Yellow, "Current Hash: " + currentHash);

                // Current hash contains in patched hashes //
                if (PatchedHash.Length > 0)
                {
                    if (PatchedHash.Contains(currentHash))
                    {
                        IsApplied = false;
                        IsPatched = true;
                        return this;
                    }
                }
                
                // Is already patched in instructuions //
                if (HasPatch(InvokeCallName))
                {
                    IsApplied = false;
                    IsPatched = true;
                    return this;
                }                

                // Current hash is invalid, and not contains in required hashes //
                if (RequiredHash.Length > 0 && !RequiredHash.Contains(currentHash))
                {
                    IsPatched = false;
                    IsApplied = false;
                    throw new Exception("Invalid hash \"" + currentHash + "\" of method \"" + InvokeMethodName + "\".");
                }

                // Set last index for PatchIndex when index above of instructions count //
                PatchIndex = Math.Min(PatchIndex, PatchMethod.Body.Instructions.Count - 1);

                // Set last index for ContinueIndex when index above of instructions count //
                ContinueIndex = Math.Min(ContinueIndex, PatchMethod.Body.Instructions.Count-1);

                string InvokeMethod_ReturnType_FullName = InvokeMethod.ReturnType.ToString().Replace('+', '/');

                if (!InvokeMethod_ReturnType_FullName.EndsWith("[]"))
                {
                    InvokeMethod_ReturnType_FullName = InvokeMethod_ReturnType_FullName.Replace('[', '<').Replace(']', '>');
                }

                // Verify return type of calling method for target method //
                if (PatchMethod.ReturnType.FullName != "System.Void" && InvokeMethod_ReturnType_FullName != "System.Void")
                {                    
                    if (this.PatchType == System.PatchType.Overwrite)
                    {
                        if (InvokeMethod_ReturnType_FullName != "System.Object" && PatchMethod.ReturnType.FullName != InvokeMethod_ReturnType_FullName)
                        {
                            throw new Exception("Invoking method \"" + InvokeMethodName + "\" must have \"" + PatchMethod.ReturnType.FullName + "\" type for return, now is \"" + InvokeMethod_ReturnType_FullName + "\".");
                        }
                    }
                    else
                    {
                        if (InvokeMethod_ReturnType_FullName != "System.Object" && PatchMethod.ReturnType.FullName != InvokeMethod_ReturnType_FullName)
                        {
                            throw new Exception("Invoking method \"" + InvokeMethodName + "\" must have \"" + PatchMethod.ReturnType.FullName + "\" type for return, now is \"" + InvokeMethod_ReturnType_FullName + "\".");
                        }
                    }
                }

                // Make Instructions Backup //
                Backup = new PatchInfo.PatchBackup()
                {
                    Variables = PatchMethod.Body.Variables.ToArray(),
                    Instructions = PatchMethod.Body.Instructions.ToArray(),
                    ExceptionHandlers = PatchMethod.Body.ExceptionHandlers.ToArray()
                };

                // Get Method Body //
                GetMethodBody();

                //*DEBUG*/ ConsoleWindow.WriteLine(ConsoleColor.Red, "Injecting Invoke: " + invokeFullname);

                // Prepare to insert invoking method //
                if (PatchType == PatchType.Injection)
                {
                    Index = PatchIndex;
                    // Set continue instruction for jump after invoke (when method returning null value) //                
                    if (ContinueIndex > PatchIndex && Instructions[ContinueIndex].OpCode.Code != Code.Ret)
                    {
                        ContinueInstruction = Instructions[ContinueIndex];
                    }
                    else
                    {
                        ContinueInstruction = null;
                    }
                }
                else
                {                    
                    Variables.Clear();
                    Instructions.Clear();
                    ExceptionHandlers.Clear();
                    InstructionsJumps.Clear();
                    ExceptionHandlersJumps.Clear();                    

                    ContinueInstruction = null;
                    Index = 0;
                }
                             
                // Add "object[]" variable for invoker arguments //
                VariableDefinition var_arguments = this.AddVariable(new ArrayType(PatchMethod.Module.TypeSystem.Object));                

                // Insert new array "object[]" to instructions //
                if (InvokeAgruments.Length > 0)
                {
                    // Insert size of "object[]" array //
                    Insert_Ldc_I4_S(InvokeAgruments.Length);

                    // Insert new array "object[]" to instructions //
                    Insert(OpCodes.Newarr, PatchMethod.Module.TypeSystem.Object);

                    // Insert arguments to instructions of new array "object[]" // 
                    for (int i = 0; i < InvokeAgruments.Length; i++)
                    {
                        // Copy current topmost value on the evaluation stack //
                        Insert(OpCodes.Dup);

                        // Set number of variable in arguments array "object[]" //
                        Insert_Ldc_I4_S(i);

                        if (InvokeAgruments[i].Length < 4)
                        {
                            throw new Exception("Invalid argument \"" + InvokeAgruments[i] + "\" in patch for method \"" + InvokeMethodName + "\".");
                        }
                        else if (InvokeAgruments[i] == "self" || InvokeAgruments[i] == "this")
                        {
                            Insert(OpCodes.Ldarg_0);
                        }
                        else if (InvokeAgruments[i].StartsWith("arg"))
                        {
                            int argIndex = 0;
                            if (int.TryParse(InvokeAgruments[i].Replace("arg", ""), out argIndex))
                            {
                                ParameterDefinition parameter = PatchMethod.Parameters[argIndex];
                                if (!PatchMethod.IsStatic) argIndex++;

                                switch (argIndex)
                                {
                                    case 0: Insert(OpCodes.Ldarg_0); break;
                                    case 1: Insert(OpCodes.Ldarg_1); break;
                                    case 2: Insert(OpCodes.Ldarg_2); break;
                                    case 3: Insert(OpCodes.Ldarg_3); break;
                                    default: Insert(OpCodes.Ldarg_S, parameter); break;
                                }

                                if (!PatchMethod.IsStatic) argIndex--;

                                if (parameter.ParameterType.IsByReference)
                                {
                                    // Copies the value type object pointed to by an address to the top of the evaluation stack.
                                    Insert(OpCodes.Ldobj, PatchMethod.Parameters[argIndex].ParameterType);
                                    // Converts a value type to an object reference (type O).
                                    Insert(OpCodes.Box, PatchMethod.Parameters[argIndex].ParameterType);
                                }
                                else if (parameter.HasConstant || parameter.ParameterType.IsValueType)
                                {
                                    // Boxing argument in array "object[]" to parameters type //
                                    Insert(OpCodes.Box, PatchMethod.Parameters[argIndex].ParameterType);
                                }
                            }
                            else
                            {
                                throw new Exception("Invalid argument \"" + InvokeAgruments[i] + "\" in patch for method \"" + InvokeMethodName + "\".");
                            }
                        }
                        else if (InvokeAgruments[i].StartsWith("var"))
                        {
                            int varIndex = 0;
                            if (int.TryParse(InvokeAgruments[i].Replace("var", ""), out varIndex))
                            {
                                switch (varIndex)
                                {
                                    case 0: this.Insert(OpCodes.Ldloc_0); break;
                                    case 1: this.Insert(OpCodes.Ldloc_1); break;
                                    case 2: this.Insert(OpCodes.Ldloc_2); break;
                                    case 3: this.Insert(OpCodes.Ldloc_3); break;
                                    default: this.Insert(OpCodes.Ldloc_S, Variables[varIndex]); break;
                                }

                                // Boxing argument in array "object[]" to variable type //
                                Insert(OpCodes.Box, Variables[varIndex].VariableType);
                            }
                        }
                        else
                        {
                            throw new Exception("Invalid argument \"" + InvokeAgruments[i] + "\" in patch for method \"" + InvokeMethodName + "\".");
                        }
                        
                        // Set argument reference for array //
                        Insert(OpCodes.Stelem_Ref);
                    }

                    // Set arguments array "object[]" to variable //
                    Insert_Stloc(var_arguments);

                    // Insert full name of invoking method //
                    Insert(OpCodes.Ldstr, InvokeCallName);

                    // Get arguments array "object[]" from variable //
                    Insert_Ldloc(var_arguments);
                }
                else
                {
                    // Insert full name of invoking method //
                    Insert(OpCodes.Ldstr, InvokeCallName);
                }

                Insert(OpCodes.Call, PatchMethod.Module.Import(InvokerMethod));

                //ConsoleWindow.WriteLine("PatchMethod.ReturnType.FullName: " + PatchMethod.ReturnType.FullName);
                //ConsoleWindow.WriteLine("InvokeMethod.ReturnType.FullName: " + InvokeMethod_ReturnType_FullName);

                // When patch method not have return type //
                if (PatchMethod.ReturnType.FullName == "System.Void")
                {
                    // Return from patch method when invoke method have not null return type //
                    if (InvokeMethod_ReturnType_FullName != "System.Void")
                    {
                        // Return refs after invoke //
                        Insert_ReturnRefArguments(var_arguments);                        

                        if (ContinueInstruction != null)
                        {
                            Insert(OpCodes.Brtrue_S, ContinueInstruction);
                        }
                        else
                        {
                            Insert(OpCodes.Brfalse_S, Instructions[Index]);
                            if (this.PatchType != System.PatchType.Overwrite)
                            {
                                Insert(OpCodes.Ret);
                            }
                        }                        
                    }
                    else // No return value //
                    {
                        Insert(OpCodes.Pop);

                        // Return refs after invoke //
                        Insert_ReturnRefArguments(var_arguments);
                    }
                }
                // Return from patch method when invoke method have not null return type //
                else if (InvokeMethod_ReturnType_FullName != "System.Void")
                {
                    if (this.PatchType == System.PatchType.Overwrite)
                    {
                        Insert(OpCodes.Unbox_Any, PatchMethod.ReturnType);
                    }
                    else
                    {
                        if (ContinueInstruction != null)
                        {
                            VariableDefinition var_result = AddVariable(PatchMethod.Module.TypeSystem.Object);

                            // Get return value is invoked method //
                            Insert_Stloc(var_result);

                            // Return refs after invoke //
                            Insert_ReturnRefArguments(var_arguments);

                            // Invoker return value is method return type? //
                            Insert_Ldloc(var_result);
                            Insert(OpCodes.Brtrue_S, ContinueInstruction);
                        }
                        else
                        {
                            VariableDefinition var_result = AddVariable(PatchMethod.Module.TypeSystem.Object);
                            VariableDefinition var_equals = AddVariable(PatchMethod.Module.TypeSystem.Boolean);

                            // Get return value is invoked method //
                            Insert_Stloc(var_result);

                            // Return refs after invoke //
                            Insert_ReturnRefArguments(var_arguments);

                            // Invoker return value is method return type? //
                            Insert_Ldloc(var_result);
                            Insert(OpCodes.Isinst, PatchMethod.ReturnType);
                            Insert(OpCodes.Ldnull);
                            Insert(OpCodes.Cgt_Un);
                            Insert_Stloc(var_equals);

                            // Skip return when result is null //
                            Insert_Ldloc(var_equals);
                            Insert(OpCodes.Brfalse_S, Instructions[Index]);

                            VariableDefinition var_return = AddVariable(PatchMethod.ReturnType);

                            // Convert to return value, when is not null //                                                    
                            Insert_Ldloc(var_result);
                            Insert(OpCodes.Unbox_Any, var_return.VariableType);
                            Insert_Stloc(var_return);

                            // Return value //
                            Insert_Ldloc(var_return);
                            Insert(OpCodes.Ret);
                        }
                    }
                }
                else
                {
                    Insert(OpCodes.Pop);

                    // Return refs after invoke //
                    Insert_ReturnRefArguments(var_arguments);
                }

                // Insert return from current method when patch type is overwrite //
                if (this.PatchType == System.PatchType.Overwrite)
                {
                    Insert(OpCodes.Ret);
                }

                // Update indexes of instructions //
                UpdateIndexes();

                string patchedHash = GetHash();
                //*DEBUG*/ ConsoleWindow.WriteLine(ConsoleColor.Red, "Patched Hash: " + patchedHash);

                // Current hash contains in patched hashes and is already patched //
                if (PatchedHash.Length > 0 && !PatchedHash.Contains(patchedHash))
                {
                    IsApplied = false;
                    IsPatched = false;
                    return this;
                }                

                UpdateMethodBody();

                // Add patched assembly to list of patched assemblies //
                if (!AssembliesPatched.Contains(PatchAssembly))
                {
                    AssembliesPatched.Add(PatchAssembly);
                }

                IsPatched = false;
                IsApplied = true;
                return this;
            }
            #endregion

            #region [this] Rollback()
            public PatchInfo Rollback()
            {
                if (Backup != null)
                {                                        
                    PatchMethod.Body.Variables.Clear();
                    PatchMethod.Body.Instructions.Clear();
                    PatchMethod.Body.ExceptionHandlers.Clear();

                    foreach (var variable in Backup.Variables)
                    {
                        PatchMethod.Body.Variables.Add(variable);
                    }
                    foreach (var instruction in Backup.Instructions)
                    {
                        PatchMethod.Body.Instructions.Add(instruction);
                    }
                    foreach (var exceptionHandler in Backup.ExceptionHandlers)
                    {
                        PatchMethod.Body.ExceptionHandlers.Add(exceptionHandler);
                    }

                    Backup = null;
                }
                return this;
            }
            #endregion

            #region [this] Save(string filename = null)
            public void Save(string filename = null)
            {
                if (PatchAssembly != null)
                {
                    if (string.IsNullOrEmpty(filename))
                    {
                        filename = PatchAssembly.MainModule.FullyQualifiedName;
                    }
                    PatchAssembly.Write(filename);
                }
            }
            #endregion

            #region [public] Save()
            public static void Save()
            {
                foreach (Mono.Cecil.AssemblyDefinition assemblyDefinition in AssembliesPatched)
                {
                    assemblyDefinition.Write(assemblyDefinition.MainModule.FullyQualifiedName);
                }
            }
            #endregion

            #region [public] Save(Mono.Cecil.AssemblyDefinition assemblyDefinition, string filename = null)
            public static void Save(Mono.Cecil.AssemblyDefinition assemblyDefinition, string filename = null)
            {
                if (assemblyDefinition != null)
                {
                    if (string.IsNullOrEmpty(filename))
                    {
                        filename = assemblyDefinition.MainModule.FullyQualifiedName;
                    }
                    assemblyDefinition.Write(filename);
                }
            }
            #endregion            
        }
    }
}
