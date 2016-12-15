using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System
{
    public partial class Invoker
    {
        public const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        // Invoker Private Environments //                        
        private static List<Assembly> Assemblies = new List<Assembly>();
        private static Dictionary<string, Type> Types = new Dictionary<string, Type>();
        private static Dictionary<string, MethodInfo> Hooks = new Dictionary<string, MethodInfo>();
        private static Dictionary<MethodInfo, DynamicMethod> DynamicMethods = new Dictionary<MethodInfo, DynamicMethod>();

        // Invoker Public Environments //                                
        public static bool Initialized { get; private set; }        

        #region [public] Initialize()
        public static bool Initialize(bool ignoreSystem = true)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            if (Initialized == false || assemblies.Length > Assemblies.Count)
            {
                foreach (Assembly A in assemblies)
                {
                    if (!Assemblies.Contains(A))
                    {                        
                        Assemblies.Add(A);
                        
                        foreach (Type T in A.GetTypes())
                        {
                            string typeName = T.FullName.Replace("+", ".");                            

                            // Skip: System types when it's need //
                            if (ignoreSystem && typeName.StartsWith("Mono.")) continue;
                            if (ignoreSystem && typeName.StartsWith("System.")) continue;
                            if (ignoreSystem && typeName.StartsWith("Microsoft.")) continue;                      

                            // Skip: Invoker(self), Generic(example<T>), Attr(CompilerGenerated) //
                            if (T == typeof(Invoker) || T.FullName[0] == '<') continue;                            

                            // Skip types with "Compiler Generated" attrubute //
                            if (T.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Length > 0) continue;

                            if (T.IsGenericType)
                            {
                                int argsIndex = typeName.IndexOf('`');
                                if (argsIndex == -1) continue;

                                typeName = typeName.Substring(0, argsIndex);
                                typeName += "<"+GetParameters(T.GetGenericArguments())+">";
                            }

                            // Add type to list of all types 
                            if (!Types.ContainsKey(typeName)) Types.Add(typeName, T);                            

                            // Get Methods of Type //
                            foreach (MethodInfo info in T.GetMethods(AllBindingFlags))
                            {                                
                                // Skip methods of properties (get, set) //
                                if (info.Name.StartsWith("get_") || info.Name.StartsWith("set_")) continue;
                                // Skip when contains generic parameters of methods of operator //
                                if (info.ContainsGenericParameters || info.Name.StartsWith("op_")) continue;
                                // Skip methods with "Compiler Generated" attrubute //
                                if (info.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Length > 0) continue;

                                // Apply SetBase attribute for target class//
                                object[] attributeHooks = info.GetCustomAttributes(typeof(Hook), true);
                                if (attributeHooks != null && attributeHooks.Length > 0)
                                {
                                    Hook attributeHook = (attributeHooks[0] as Hook);                                    
                                    Hooks[attributeHook.Name] = info;
                                }

                                // Register method with inject & params attribute //
                                object[] attributeInject = info.GetCustomAttributes(typeof(Patch), true);
                                object[] attributeParams = info.GetCustomAttributes(typeof(Params), true);
                                if (attributeInject != null && attributeInject.Length > 0 && attributeParams != null && attributeParams.Length > 0)
                                {
                                    Patch invokerPatch = (attributeInject[0] as Patch);
                                    Params invokerParams = (attributeParams[0] as Params);
                                    Patches.Create(invokerPatch, info, invokerParams);
                                }                                
                            }                            
                        }
                    }
                }
            }            

            return (Assemblies.Count > 0 && Types.Count > 0);
        }
        #endregion        

        // Invoker.HasParameters() //
        #region [private] HasParameters(MethodInfo method, Type[] arguments)
        private static bool HasParameters(MethodInfo method, Type[] arguments)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != arguments.Length) return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                Type argumentType = arguments[i];
                Type parameterType = parameters[i].ParameterType;

                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }

                if (parameterType == argumentType) continue;

                while (argumentType != null && parameterType != argumentType)
                {
                    argumentType = argumentType.BaseType;
                }

                if (parameterType == argumentType) continue;

                return false;
            }

            return true;
        }
        #endregion

        #region [private] HasParameters(MethodInfo method, object[] arguments)
        private static bool HasParameters(MethodInfo method, object[] arguments)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != arguments.Length) return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                Type argumentType = arguments[i].GetType();
                Type parameterType = parameters[i].ParameterType;

                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }

                if (parameterType == argumentType) continue;

                while (argumentType != null && parameterType != argumentType)
                {
                    argumentType = argumentType.BaseType;
                }

                if (parameterType == argumentType) continue;

                return false;
            }
            return true;
        }
        #endregion

        // Invoker.GetParameters() //
        #region [private] GetParameters(ParameterInfo[] parameters)
        private static string GetParameters(ParameterInfo[] parameters)
        {
            StringBuilder result = new StringBuilder();
            if (parameters != null)
            {
                foreach (ParameterInfo parameter in parameters)
                {
                    String parameterName = parameter.ParameterType.ToString().TrimEnd('&');
                    parameterName = parameterName.Replace('/', '.').Replace('+', '.');

                    if (parameterName.Contains("`") && parameterName.StartsWith("System."))
                    {
                        string interfaceName = parameterName.Substring(0, parameterName.IndexOf("`"));
                        interfaceName = interfaceName.Remove(0, interfaceName.LastIndexOf(".") + 1);

                        int paramStart = parameterName.IndexOf("[") + 1;
                        int paramEnded = parameterName.IndexOf("]") - paramStart;

                        if (paramStart > -1 && paramEnded > -1)
                        {
                            parameterName = parameterName.Substring(paramStart, paramEnded);
                        }

                        parameterName = interfaceName + "<" + parameterName + ">";
                    }
                    if (result.Length > 0) result.Append(", ");
                    result.Append(parameterName);
                }
            }
            return result.ToString().Replace(",", ", ").Replace("  ", " ");
        }
        #endregion

        #region [private] GetParameters(params object[] parameters)
        private static string GetParameters(params object[] parameters)
        {
            StringBuilder result = new StringBuilder();
            if (parameters != null)
            {
                string parameterName = "System.Object";
                foreach (object parameter in parameters)
                {
                    if (parameter != null)
                    {
                        if (parameter is System.Type)
                        {
                            parameterName = parameter.ToString();
                        }
                        else if (parameter is Mono.Cecil.ParameterDefinition)
                        {
                            parameterName = (parameter as Mono.Cecil.ParameterDefinition).ParameterType.ToString();
                        }
                        else if (parameter is Mono.Cecil.GenericParameter)
                        {
                            parameterName = (parameter as Mono.Cecil.GenericParameter).ToString();
                        }
                        else
                        {
                            parameterName = parameter.GetType().ToString();
                        }
                    }

                    parameterName = parameterName.TrimEnd('&');
                    parameterName = parameterName.Replace('/', '.').Replace('+', '.');

                    if (parameterName.Contains("`") && parameterName.StartsWith("System."))
                    {
                        string interfaceName = parameterName.Substring(0, parameterName.IndexOf('`'));
                        interfaceName = interfaceName.Remove(0, interfaceName.LastIndexOf('.') + 1);

                        int paramStart = parameterName.IndexOf('[') + 1;
                        int paramEnded = parameterName.IndexOf(']') - paramStart;

                        if (paramStart < 1 || paramEnded < paramStart)
                        {
                            paramStart = parameterName.IndexOf('<') + 1;
                            paramEnded = parameterName.IndexOf('>') - paramStart;
                        }

                        if (paramStart > -1 && paramEnded > 0)
                        {
                            parameterName = parameterName.Substring(paramStart, paramEnded);
                        }

                        parameterName = interfaceName + "<" + parameterName + ">";
                    }

                    if (result.Length > 0) result.Append(", ");
                    result.Append(parameterName);
                }
            }
            return result.ToString().Replace(",", ", ").Replace("  ", " ");
        }
        #endregion        

        // Invoker.GetMethodFullName() //
        #region [private] GetMethodFullName(string method, object[] arguments)
        private static string GetMethodFullName(MethodInfo methodInfo)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(methodInfo.DeclaringType.FullName);
            builder.Append(".");
            builder.Append(methodInfo.Name);
            builder.Append("(");
            builder.Append(GetParameters(methodInfo.GetParameters()));
            builder.Append(")");
            return builder.ToString().Replace('+', '.');
        }
        #endregion

        // Invoker.FindType() //
        #region [public] FindType(string fullname)
        public static Type FindType(string fullname)
        {
            foreach (Assembly A in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type T in A.GetTypes())
                    {
                        if (T.FullName == fullname) return T;
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

        // Invoker.FindField() //
        #region [public] FindField(string fullname)
        public static FieldInfo FindField(string fullname)
        {
            if (!string.IsNullOrEmpty(fullname) && fullname.IndexOf('.') > 0)
            {
                string[] names = fullname.Split('.');
                string type = string.Join(".", names, 0, names.Length - 1);
                string name = names[names.Length - 1];

                Type invokeType = null;
                if (!Types.TryGetValue(type, out invokeType))
                {
                    throw new Exception("Type '" + type + "' not exists in assemblie(s)");
                }

                foreach (FieldInfo fieldInfo in invokeType.GetFields(AllBindingFlags))
                {
                    if (fieldInfo.Name == name) return fieldInfo;
                }
            }
            return null;
        }
        #endregion

        // Invoker.FindField() //
        #region [public] FindProperty(string fullname)
        public static PropertyInfo FindProperty(string fullname)
        {
            if (!string.IsNullOrEmpty(fullname) && fullname.IndexOf('.') > 0)
            {
                string[] names = fullname.Split('.');
                string type = string.Join(".", names, 0, names.Length - 1);
                string name = names[names.Length - 1];

                Type invokeType = null;
                if (!Types.TryGetValue(type, out invokeType))
                {
                    throw new Exception("Type '" + type + "' not exists in assemblie(s)");
                }

                foreach (PropertyInfo propertyInfo in invokeType.GetProperties(AllBindingFlags))
                {
                    if (propertyInfo.Name == name) return propertyInfo;
                }
            }
            return null;
        }
        #endregion

        // Invoker.FindMethod() //
        #region [public] FindMethod(string fullname, params Type[] parameters)
        public static MethodInfo FindMethod(string fullname, params Type[] parameters)
        {
            if (!string.IsNullOrEmpty(fullname) && fullname.IndexOf('.') > 0)
            {
                fullname = fullname.SplitArg(0, '(');

                string[] names = fullname.Split('.');
                string type = string.Join(".", names, 0, names.Length-1);
                string name = names[names.Length - 1];

                Type invokeType = null;
                if (!Types.TryGetValue(type, out invokeType))
                {
                    throw new Exception("Type '" + type + "' not exists in assemblie(s)");
                }

                foreach (MethodInfo methodInfo in invokeType.GetMethods(AllBindingFlags))
                {
                    if (methodInfo.Name == name)
                    {
                        if (parameters.Length == 0 || methodInfo.HasParameters(parameters))
                        {
                            return methodInfo;
                        }
                    }
                }
            }
            return null;
        }
        #endregion

        // Invoker.CallHook() //
        #region [public] CallHook(string hookname)
        public static object CallHook(string hookname)
        {            
            MethodInfo method;
            if (Hooks.TryGetValue(hookname, out method))
            {
                return method.Invoke(null, new object[0]);
            }
            return null;
        }
        #endregion

        #region [public] CallHook(string hookname, object[] parameters)
        public static object CallHook(string hookname, object[] parameters)
        {
            MethodInfo method;            
            if (Hooks.TryGetValue(hookname, out method))
            {
                return method.Invoke(null, parameters);
            }
            return null;
        }
        #endregion

        // Invoker.GetValue() //
        #region [public] GetValue(string fullname, object[] index = null)
        public static object GetValue(string fullname, object[] index = null)
        {
            if (!string.IsNullOrEmpty(fullname) && fullname.IndexOf('.') > 0)
            {
                try
                {
                    string[] names = fullname.Split('.');
                    string type = string.Join(".", names, 0, names.Length - 1);
                    string name = names[names.Length - 1];

                    Type targetType = null;
                    if (!Types.TryGetValue(type, out targetType))
                    {
                        throw new Exception("Type '" + type + "' not exists in assemblie(s)");
                    }

                    Type invokeType = targetType;
                    while (invokeType != null)
                    {
                        PropertyInfo property = invokeType.GetProperty(name, AllBindingFlags);
                        if (property != null) return property.GetValue(null, index);

                        FieldInfo field = invokeType.GetField(name, AllBindingFlags);
                        if (field != null) return field.GetValue(null);

                        invokeType = invokeType.BaseType;
                    }

                    throw new Exception("Field or property '" + name + "' not exists in '" + targetType.FullName + "'.");
                }
                catch (Exception E)
                {
                    ConsoleWindow.WriteLine(ConsoleColor.Red, "[InvokerException] "+E.Message);
                }
            }
            return null;
        }
        #endregion

        #region [public] GetValue(string name, object target, object[] index = null)
        public static object GetValue(string name, object target, object[] index = null)
        {
            if (!string.IsNullOrEmpty(name) && target != null)
            {
                try
                {
                    Type targetType = target.GetType();
                    Type invokeType = targetType;
                    while (invokeType != null)
                    {
                        PropertyInfo property = invokeType.GetProperty(name, AllBindingFlags);
                        if (property != null) return property.GetValue(target, index);
                        FieldInfo field = invokeType.GetField(name, AllBindingFlags);
                        if (field != null) return field.GetValue(target);
                        invokeType = invokeType.BaseType;
                    }

                    throw new Exception("Field or property '" + name + "' not exists in '"+ targetType.FullName + "'.");
                }
                catch (Exception E)
                {
                    ConsoleWindow.WriteLine(ConsoleColor.Red, "[InvokerException] " + E.Message);
                }
            }
            return null;
        }
        #endregion

        // Invoker.GetValue<T>() //
        #region [public] GetValue<T>(string fullname, object[] index = null)
        public static T GetValue<T>(string fullname, object[] index = null)
        {
            if (!string.IsNullOrEmpty(fullname) && fullname.IndexOf('.') > 0)
            {
                try
                {
                    string[] names = fullname.Split('.');
                    string type = string.Join(".", names, 0, names.Length - 1);
                    string name = names[names.Length - 1];

                    Type targetType = null;
                    if (!Types.TryGetValue(type, out targetType))
                    {
                        throw new Exception("Type '" + type + "' not exists in assemblie(s)");
                    }

                    Type invokeType = targetType;
                    while (invokeType != null)
                    {
                        PropertyInfo property = invokeType.GetProperty(name, AllBindingFlags);
                        if (property != null && property.PropertyType == typeof(T))
                        {
                            return (T)property.GetValue(null, index);
                        }

                        FieldInfo field = invokeType.GetField(name, AllBindingFlags);
                        if (field != null && field.FieldType == typeof(T))
                        {
                            return (T)field.GetValue(null);
                        }

                        invokeType = invokeType.BaseType;
                    }

                    throw new Exception("Field or property '" + name + "' not exists in '" + targetType.FullName + "'.");
                }
                catch (Exception E)
                {
                    ConsoleWindow.WriteLine(ConsoleColor.Red, "[InvokerException] " + E.Message);
                }
            }
            return default(T);
        }
        #endregion

        #region [public] GetValue<T>(string name, object target, object[] index = null)
        public static T GetValue<T>(string name, object target, object[] index = null)
        {
            if (!string.IsNullOrEmpty(name) && target != null)
            {
                try
                {
                    Type targetType = target.GetType();
                    Type invokeType = targetType;
                    while (invokeType != null)
                    {
                        PropertyInfo property = invokeType.GetProperty(name, AllBindingFlags);
                        if (property != null && property.PropertyType == typeof(T))
                        {
                            return (T)property.GetValue(target, index);
                        }

                        FieldInfo field = invokeType.GetField(name, AllBindingFlags);
                        if (field != null && field.FieldType == typeof(T))
                        {
                            return (T)field.GetValue(target);
                        }
                        invokeType = invokeType.BaseType;
                    }

                    throw new Exception("Field or property '" + name + "' not exists in '" + targetType.FullName + "'.");
                }
                catch (Exception E)
                {
                    ConsoleWindow.WriteLine(ConsoleColor.Red, "[InvokerException] " + E.Message);
                }
            }
            return default(T);
        }
        #endregion

        // Invoker.SetValue<T>() //
        #region [public] SetValue<T>(string fullname, T value, object[] index = null)
        public static T SetValue<T>(string fullname, T value, object[] index = null)
        {
            if (!string.IsNullOrEmpty(fullname) && fullname.IndexOf('.') > 0)
            {
                try
                {
                    string[] names = fullname.Split('.');
                    string type = string.Join(".", names, 0, names.Length - 1);
                    string name = names[names.Length - 1];

                    Type invokeType = null;
                    if (!Types.TryGetValue(type, out invokeType))
                    {
                        throw new Exception("Type '" + type + "' not exists in assemblie(s)");
                    }

                    Type targetType = invokeType;
                    while (invokeType != null)
                    {
                        PropertyInfo property = invokeType.GetProperty(name, AllBindingFlags);
                        if (property != null && property.PropertyType == typeof(T))
                        {
                            property.SetValue(null, value, index);
                            return (T)property.GetValue(null, index);
                        }

                        FieldInfo field = invokeType.GetField(name, AllBindingFlags);
                        if (field != null && field.FieldType == typeof(T))
                        {
                            field.SetValue(null, value);
                            return (T)field.GetValue(null);
                        }

                        invokeType = invokeType.BaseType;
                    }

                    throw new Exception("Field or property '" + name + "' not exists in '" + targetType.FullName + "'.");
                }
                catch (Exception E)
                {
                    if (string.IsNullOrEmpty(E.StackTrace))
                    {
                        ConsoleWindow.WriteLine(ConsoleColor.Red, "[InvokerException] " + E.Message);
                    }
                }
            }
            return default(T);
        }
        #endregion

        #region [public] SetValue<T>(string name, T value, object target = null, object[] index = null)
        public static T SetValue<T>(object target, string name, T value, object[] index = null)
        {
            if (!string.IsNullOrEmpty(name) && target != null)
            {
                try
                {
                    Type targetType = target.GetType();
                    Type invokeType = targetType;
                    while (invokeType != null)
                    {
                        PropertyInfo property = invokeType.GetProperty(name, AllBindingFlags);
                        if (property != null && property.PropertyType == typeof(T))
                        {
                            property.SetValue(target, value, index);
                            return (T)property.GetValue(target, index);
                        }

                        FieldInfo field = invokeType.GetField(name, AllBindingFlags);
                        if (field != null && field.FieldType == typeof(T))
                        {
                            field.SetValue(target, value);
                            return (T)field.GetValue(target);
                        }

                        invokeType = invokeType.BaseType;
                    }

                    throw new Exception("Field or property '" + name + "' not exists in '" + targetType.FullName + "'.");
                }
                catch (Exception E)
                {
                    if (string.IsNullOrEmpty(E.StackTrace))
                    {
                        ConsoleWindow.WriteLine(ConsoleColor.Red, "[InvokerException] " + E.Message);
                    }
                }
            }
            return default(T);
        }
        #endregion

        // Invoker.Call() //
        #region [public] Call(string method)
        public static object Call(string method)
        {
            if (!string.IsNullOrEmpty(method) && method.IndexOf('.') > 0)
            {
                try
                {
                    int argsIndex = method.IndexOf('(');
                    if (argsIndex > 0) method = method.Remove(argsIndex);

                    string[] names = method.Split('.');
                    string type = string.Join(".", names, 0, names.Length - 1);
                    string name = names[names.Length - 1];

                    Type invokeType = null;
                    if (!Types.TryGetValue(type, out invokeType))
                    {
                        throw new Exception("Type '" + type + "' not exists in assemblie(s)");
                    }

                    MethodInfo invokeMethod = invokeType.GetMethod(name, AllBindingFlags, null, CallingConventions.Any, new Type[0], null);

                    if (invokeMethod == null)
                    {
                        foreach (var m in invokeType.GetMethods(AllBindingFlags))
                        {
                            if (m.Name == name && m.GetParameters().Length == 0)
                            {
                                invokeMethod = m;
                                break;
                            }
                        }
                    }

                    if (invokeMethod == null)
                    {
                        throw new Exception("Method '" + name + "' not exists in '" + type + "'.");
                    }

                    return invokeMethod.Invoke(null, new object[0]);
                }
                catch (Exception E)
                {
                    if (string.IsNullOrEmpty(E.StackTrace))
                    {
                        ConsoleWindow.WriteLine(ConsoleColor.Red, "[InvokerException] " + E.Message);
                    }
                }
            }
            return null;
        }
        #endregion        

        #region [public] Call(string method, object[] arguments)
        public static object Call(string method, object[] arguments)
        {
            if (!string.IsNullOrEmpty(method) && method.IndexOf('.') > 0)
            {
                try
                {
                    int argsIndex = method.IndexOf('(');
                    if (argsIndex > 0)
                    {
                        method = method.Remove(argsIndex);
                    }

                    string[] names = method.Split('.');
                    string type = string.Join(".", names, 0, names.Length - 1);
                    string name = names[names.Length - 1];

                    Type invokeType = null;
                    if (!Types.TryGetValue(type, out invokeType))
                    {
                        throw new Exception("Type '" + type + "' not found in assemblie(s)");
                    }

                    Type[] parameters = new Type[arguments.Length];
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (arguments[i] != null)
                        {
                            parameters[i] = arguments[i].GetType();
                        }
                        else
                        {
                            parameters[i] = typeof(object);
                        }
                    }

                    foreach (MethodInfo invokeMethod in invokeType.GetMethods(AllBindingFlags))
                    {
                        if (invokeMethod.Name == name && HasParameters(invokeMethod, parameters))
                        {
                            return invokeMethod.Invoke(null, arguments);
                        }
                    }

                    throw new Exception("Method '" + name + "' not found in '" + type + "'.");                    
                }
                catch (Exception E)
                {
                    ConsoleWindow.WriteLine(ConsoleColor.Red, "[InvokerException] " + E.Message);
                }
            }
            return null;
        }
        #endregion

        #region [public] Call(string method, object target)
        public static object Call(string name, object target)
        {
            if (!string.IsNullOrEmpty(name) && target != null)
            {
                try
                {
                    Type invokeType = target.GetType();
                    MethodInfo invokeMethod = invokeType.GetMethod(name, AllBindingFlags, null, new Type[0], null);
                    if (invokeMethod == null) throw new Exception("Method '" + name + "' not exists in '" + invokeType.FullName + "'.");
                    return invokeMethod.Invoke(target, new object[0]);
                }
                catch (Exception E)
                {
                    ConsoleWindow.WriteLine(ConsoleColor.Red, "[InvokerException] " + E.Message);
                }
            }
            return null;
        }
        #endregion

        #region [public] Call(string method, object target, object[] arguments)
        public static object Call(string name, object target, object[] arguments)
        {
            if (!string.IsNullOrEmpty(name) && target != null)
            {                
                try
                {
                    Type invokeType = target.GetType();
                    Type[] parameters = new Type[arguments.Length];
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (arguments[i] != null)
                        {
                            parameters[i] = arguments[i].GetType();
                        }
                        else
                        {
                            parameters[i] = typeof(object);
                        }
                    }

                    MethodInfo invokeMethod = invokeType.GetMethod(name, AllBindingFlags, null, CallingConventions.Any, parameters, null);

                    if (invokeMethod == null)
                    {
                        foreach (var method in invokeType.GetMethods(AllBindingFlags))
                        {
                            if (method.Name == name && method.GetParameters().Length == parameters.Length)
                            {
                                invokeMethod = method;
                                break;
                            }
                        }
                    }

                    if (invokeMethod == null)
                    {
                        throw new Exception("Method '" + name + "' not exists in '" + invokeType.FullName + "'.");
                    }

                    return invokeMethod.Invoke(target, arguments);
                }
                catch (Exception E)
                {                   
                    ConsoleWindow.WriteLine(ConsoleColor.Red, "[InvokerException] " + E.Message);
                }
            }
            return null;
        }
        #endregion

        #region [public] CallBase(string method, object target, object[] arguments)
        public static object CallBase(string name, object target, object[] arguments)
        {
            if (!string.IsNullOrEmpty(name) && target != null)
            {
                try
                {
                    Type targetType = target.GetType().BaseType;
                    Type[] parameters = new Type[arguments.Length];
                    for (int n = 0; n < arguments.Length; n++)
                    {
                        parameters[n] = arguments[n].GetType();
                    }
                    
                    MethodInfo method = targetType.GetMethod(name, AllBindingFlags, null, CallingConventions.Any, parameters, null);

                    if (method == null)
                    {
                        foreach (var m in targetType.GetMethods(AllBindingFlags))
                        {
                            if (m.Name == name && m.GetParameters().Length == parameters.Length)
                            {
                                method = m;
                                break;
                            }
                        }
                    }

                    DynamicMethod dynamicMethod = null;
                    if (!DynamicMethods.TryGetValue(method, out dynamicMethod))
                    {
                        dynamicMethod = new DynamicMethod(name, typeof(object), new[] { targetType, typeof(object) }, targetType);                        

                        ParameterInfo[] parameterInfo = method.GetParameters();
                        int i; Type[] paramTypes = new Type[parameterInfo.Length];

                        for (i = 0; i < paramTypes.Length; i++)
                        {
                            if (parameterInfo[i].ParameterType.IsByRef)
                            {
                                paramTypes[i] = parameterInfo[i].ParameterType.GetElementType();
                            }
                            else
                            {
                                paramTypes[i] = parameterInfo[i].ParameterType;
                            }
                        }

                        ILGenerator IL = dynamicMethod.GetILGenerator();
                        IL.Emit(OpCodes.Ldarg_0);

                        for (i = 0; i < paramTypes.Length; i++)
                        {
                            IL.Emit(OpCodes.Ldarg_1);
                            IL.Emit(OpCodes.Ldc_I4_S, i);
                            IL.Emit(OpCodes.Ldelem_Ref);

                            if (paramTypes[i].IsValueType)
                            {
                                IL.Emit(OpCodes.Unbox_Any, paramTypes[i]);
                            }
                            else if (paramTypes[i] != typeof(object))
                            {
                                IL.Emit(OpCodes.Castclass, paramTypes[i]);
                            }
                        }

                        IL.Emit(OpCodes.Call, method);

                        if (method.ReturnType == typeof(void))
                        {
                            IL.Emit(OpCodes.Ldnull);
                        }
                        else if (method.ReturnType.IsValueType)
                        {
                            IL.Emit(OpCodes.Box, method.ReturnType);
                        }

                        IL.Emit(OpCodes.Ret);

                        DynamicMethods[method] = dynamicMethod;
                    }

                    return dynamicMethod.Invoke(null, new object[] { target, arguments });
                }
                catch (Exception E)
                {
                    ConsoleWindow.WriteLine(ConsoleColor.Red, "[InvokerException] " + E.Message);
                }
            }
            return null;
        }
        #endregion        
    }
}
